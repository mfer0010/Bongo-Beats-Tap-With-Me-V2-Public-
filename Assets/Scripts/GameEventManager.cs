using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class GameEventManager : MonoBehaviour
{
    // Objects to Link
    public AudioSource musicAudioSource;
    public AudioSource crowdAudioSource;
    // The Generated CSV File containing the beat times and beat counts
    public string [] csvSongBeatFilePath;
    // The Location to save the CSV File containing the generated notes (old)
    public string [] csvGeneratedNotesFilePath;
    // The Location to save the CSV File containing the generated notes of different difficulty levels
    public string [] csvGeneratedNotesFilePath1;
    public string [] csvGeneratedNotesFilePath2;
    public string [] csvGeneratedNotesFilePath3;
    public string [] csvGeneratedNotesFilePath4;
    public string [] csvGeneratedNotesFilePath5;
    // List of all the songs in the same order as the beat file list and the generated notes list
    public AudioClip[] songs;
    public GameObject leftDrum;
    public GameObject rightDrum;
    // Empty game object to be the parent of all notes (for organisation)
    public GameObject noteParent;
    // Reference to the note prefab
    public GameObject notePrefab;
    //List of notes that are spawned for this level used in fixed difficulty mode
    private Queue<GameObject> spawnedNotes;
    // List of notes for each difficulty only used in adaptive difficulty mode
    private Queue<GameObject>[] ddaSpawnedNotes;
    // Sound to play at the end of the level from the crowd
    public AudioClip crowdCheeringAudioClip;
    // UI to Enable at the end of the level
    [SerializeField] private GameObject endOfLevelUI;

    // Variables
    [SerializeField] private float timeToBeginSong = 5.0f;
    private int nextNotePointer;
    // Used in fixed difficulty mode
    private List<float> noteTimes;
    private List<Variables.NoteType> noteTypes;
    // Used in adaptive difficulty mode
    private List<float> [] ddaNoteTimes;
    private List<Variables.NoteType> [] ddaNoteTypes;
    private int[] ddaNextNotePointer;
    private int difficultyLevel = 2;
    private int notesExecutedThisCycle;
    private bool bShouldIncreaseLevel;
    private bool bShouldDecreaseLevel;

    // References to Objects needed by the game manager
    private Transform leftDrumTarget, rightDrumTarget;
    private Transform leftNoteSpawnPoint, rightNoteSpawnPoint;
    private Transform leftNoteDespawnPoint, rightNoteDespawnPoint;

    [HideInInspector] public bool bIsGamePaused;


    private void Awake()
    {
        // Simple Checks to ensure all required game objects are linked in level
        if (!musicAudioSource)
        {
            Debug.LogError("Please attach music audio source to GameEventManager script in " + gameObject.name);
        }
        else
        {
            if (musicAudioSource.playOnAwake)
                musicAudioSource.playOnAwake = false;
        }

        if (!crowdAudioSource)
        {
            Debug.LogError("Please attach crowd audio source to GameEventManager script in " + gameObject.name);
        }

        if (!leftDrum || !rightDrum)
        {
            Debug.LogError("One (or both) of the drums are not linked to GameEventManager script in " + gameObject.name);
        }
        else
        {
            leftDrumTarget = leftDrum.transform.Find("Target");
            rightDrumTarget = rightDrum.transform.Find("Target");
            //Debug.Log("Left Drum Target: " + leftDrumTarget.position);
            //Debug.Log("Right Drum Target: " + rightDrumTarget.position);

            leftNoteSpawnPoint = leftDrum.transform.Find("SpawnPoint");
            rightNoteSpawnPoint = rightDrum.transform.Find("SpawnPoint");
            //Debug.Log("Left Spawn Point: " + leftNoteSpawnPoint.position);
            //Debug.Log("Right Spawn Point: " + rightNoteSpawnPoint.position);

            leftNoteDespawnPoint = leftDrum.transform.Find("DespawnPoint");
            rightNoteDespawnPoint = rightDrum.transform.Find("DespawnPoint");
        }

        if (!noteParent)
        {
            Debug.LogError("Please assign a game object to be the parent of all notes spawned by " + gameObject.name);
        }

        if (!crowdCheeringAudioClip)
        {
            Debug.LogWarning("No clip Associated with crowd Cheering in " + gameObject.name);
        }

        if (!endOfLevelUI)
        {
            Debug.LogError("No End of Level UI associated with " + gameObject.name);
        }

        if (csvGeneratedNotesFilePath.Length == 0)
        {
            Debug.LogError("Please Ensure to populate the beats file paths accordingly in " + gameObject.name);
        }

        if (csvGeneratedNotesFilePath1.Length == 0 || csvGeneratedNotesFilePath2.Length == 0 ||
            csvGeneratedNotesFilePath3.Length == 0 || csvGeneratedNotesFilePath4.Length == 0 ||
            csvGeneratedNotesFilePath5.Length == 0)
        {
            Debug.LogError("Please Ensure to populate the generated notes file paths accordingly in " + gameObject.name);
        }
    }

    private void Start()
    {
        bIsGamePaused = false;

        if (!Variables.bAdaptiveDifficulty)
        {
            // Load the Notes:
            if (!LoadLevelNotes())
            {
                Debug.LogError("Failed to Load Level Notes");
                Application.Quit();
            }
        }
        else
        {
            // Load the Notes for DDA:
            if (!DDALoadLevelNotes())
            {
                Debug.LogError("Failed to Load DDA Level Notes");
                Application.Quit();
            }
            difficultyLevel = 2;
            bShouldDecreaseLevel = false;
            bShouldIncreaseLevel = false;
            notesExecutedThisCycle = 0;
            Variables.DDA_NotesMissedThisCycle = 0;
            Variables.DDATotalNotesThisCycle = 0;
        }

        // Reset Score Values
        ScoreCounter.ResetValues();

        // Start PLaying the song after delay
        StartCoroutine(StartSongIn(timeToBeginSong));
    }

    private void Update()
    {
        // Do nothing if game is paused
        if (bIsGamePaused) return;

        // Logic in the case adaptive difficulty is not enabled
        if (!Variables.bAdaptiveDifficulty)
        {
            // Do nothing if last note is reached
            if (nextNotePointer >= noteTypes.Count) return;
            // Checks the elapsed time of song with the time the note needs to spawn
            // and spawns notes accordingly
            if (GetElapsedSongTimeInSeconds() >= noteTimes[nextNotePointer])
            {
                spawnedNotes.Dequeue().SetActive(true);
                if (noteTypes[nextNotePointer] == Variables.NoteType.Both)
                    spawnedNotes.Dequeue().SetActive(true);

                nextNotePointer++;
            }
        }
        else
        {
            // Do nothing if last note is reached
            if (ddaNextNotePointer[difficultyLevel] >= ddaNoteTypes[difficultyLevel].Count) return;
            // Checks the elapsed time of song with the time the note needs to spawn
            // and spawns notes accordingly
            if (GetElapsedSongTimeInSeconds() >= ddaNoteTimes[difficultyLevel][ddaNextNotePointer[difficultyLevel]])
            {
                ddaSpawnedNotes[difficultyLevel].Dequeue().SetActive(true);
                if (ddaNoteTypes[difficultyLevel][ddaNextNotePointer[difficultyLevel]] == Variables.NoteType.Both)
                    ddaSpawnedNotes[difficultyLevel].Dequeue().SetActive(true);

                ddaNextNotePointer[difficultyLevel]++;
                notesExecutedThisCycle++;

                // Check if difficulty check should be made
                if (notesExecutedThisCycle == Variables.DDAStartCheck)
                {
                    float performanceRatio = ((float) Variables.DDA_NotesMissedThisCycle / Variables.DDATotalNotesThisCycle);
                    Debug.Log("Checking Performance! Ratio: " + performanceRatio);

                    if (performanceRatio < Variables.ddaIncrementLevelThreshold)
                    {
                        // Increase Level
                        if (difficultyLevel != 4)
                        {
                            bShouldIncreaseLevel = true;
                            prepareQueueOfDesiredLevel(difficultyLevel + 1);
                        }
                    }
                    else if (performanceRatio > Variables.ddaDecrementLevelThreshold)
                    {
                        // Decrease Level
                        if (difficultyLevel != 0)
                        {
                            bShouldDecreaseLevel = true;
                            prepareQueueOfDesiredLevel(difficultyLevel - 1);
                        }
                    }

                }
                if (notesExecutedThisCycle == Variables.DDAWindowLength)
                {
                    // At the end of the window we will reset the variables
                    Debug.Log("New Window");
                    notesExecutedThisCycle = 0;
                    Variables.DDATotalNotesThisCycle = 0;
                    Variables.DDA_NotesMissedThisCycle = 0;
                    
                    // And change the level accordingly
                    if (bShouldIncreaseLevel)
                    {                       
                        bShouldIncreaseLevel = false;
                        difficultyLevel++;
                        Debug.Log("Increased Level to " + difficultyLevel);
                    }
                    else if (bShouldDecreaseLevel)
                    {
                        bShouldDecreaseLevel = false;
                        difficultyLevel--;
                        Debug.Log("Decreased Level to " + difficultyLevel);
                    }
                }
            }
        }
    }

    private void prepareQueueOfDesiredLevel(int level)
    {
        // See how many notes from current ponter we need to look ahead to get the time
        int lookahead = Variables.DDAWindowLength - Variables.DDAStartCheck;
        float startTime = ddaNoteTimes[difficultyLevel][ddaNextNotePointer[difficultyLevel] + lookahead];

        // If you're reading this, sorry for the pointer confusion :')
        // Increment the pointer of the next desired level until the times match
        while (ddaNoteTimes[level][ddaNextNotePointer[level]] < startTime)
        {
            // Dequeue the note and mark it for destroy
            Destroy(ddaSpawnedNotes[level].Dequeue());
            if (ddaNoteTypes[level][ddaNextNotePointer[level]] == Variables.NoteType.Both)
                Destroy(ddaSpawnedNotes[level].Dequeue());
            // And Increment the pointer
            ddaNextNotePointer[level]++;
        }
    }

    // Starts playing song after delay
    IEnumerator StartSongIn(float delay)
    {
        yield return new WaitForSeconds(delay);

        musicAudioSource.clip = songs[Variables.songID];
        musicAudioSource.Play();
        // Sets the timer to end the level
        StartCoroutine(EndLevel());
    }

    // Returns the exact time elapsed in the song using samples for better accuracy
    public float GetElapsedSongTimeInSeconds()
    {
        if (!musicAudioSource.isPlaying)
            return 0;

        return musicAudioSource.timeSamples / (float) musicAudioSource.clip.frequency;
    }


    // Loads the notes from the csvGeneratedNotedFilePath (Use GenerateLevel Script to Generate
    // these notes). Returns True on Success and False otherwise.
    private bool LoadLevelNotes()
    {
        StreamReader strReader;
        try
        {
            strReader = new StreamReader(csvGeneratedNotesFilePath[Variables.songID]);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }

        noteTimes = new List<float>();
        noteTypes = new List<Variables.NoteType>();

        bool eof = false;
        while (!eof)
        {
            // loop throug the file and save the note times and note types in their respective lists
            string line = strReader.ReadLine();
            if (line == null)
            {
                eof = true;
                break;
            }

            var values = line.Split(',');
            noteTimes.Add(float.Parse(values[0]));
            noteTypes.Add((Variables.NoteType) System.Enum.Parse(typeof(Variables.NoteType), values[1]));
        }

        // Debug Log for testing
        //for (int i = 0; i < 10; i++)
        //    {
        //        Debug.Log("Time: " + noteTimes[i] + "; Type: " + noteTypes[i].ToString("G"));
        //    }

        // Offset the times of all the notes by Variables.noteTimeToReachTarget
        for (int i = 0; i < noteTimes.Count; i++)
        {
            noteTimes[i] -= Variables.noteTimeToReachTarget;
        }

        // Instantiate all the notes in the level 
        spawnedNotes = new Queue<GameObject>();
        for (int i = 0; i < noteTypes.Count; i++)
        {
            GameObject spawnedNote;
            if (noteTypes[i] == Variables.NoteType.Left || noteTypes[i] == Variables.NoteType.Both)
            {
                spawnedNote = Instantiate(notePrefab, leftNoteSpawnPoint.position, 
                                          Quaternion.identity, noteParent.transform);
                spawnedNote.GetComponent<NotesController>().InitializeVariables(leftNoteSpawnPoint.position,
                                                                                leftDrumTarget.position,
                                                                                leftNoteDespawnPoint.position,
                                                                                true);
                spawnedNote.GetComponent<AudioSource>().panStereo = -1;

                spawnedNotes.Enqueue(spawnedNote);
            }
            if (noteTypes[i] == Variables.NoteType.Right || noteTypes[i] == Variables.NoteType.Both)
            {
                spawnedNote = Instantiate(notePrefab, rightNoteSpawnPoint.position, 
                                          Quaternion.identity, noteParent.transform);
                spawnedNote.GetComponent<NotesController>().InitializeVariables(rightNoteSpawnPoint.position,
                                                                                rightDrumTarget.position,
                                                                                rightNoteDespawnPoint.position,
                                                                                false);
                spawnedNote.GetComponent<AudioSource>().panStereo = 1;

                spawnedNotes.Enqueue(spawnedNote);
            }
        }

        // Set the pointer to the top of the list
        nextNotePointer = 0;

        return true;
    }

    // Loads the notes of different difficulties the csvGeneratedNotedFilePath1, ... , csvGeneratedNotesFilePath5
    //  (Use GenerateLevel Script to Generate these notes). Returns True on Success and False otherwise.
    private bool DDALoadLevelNotes()
    {
        // Initialise the arrays (note the queues and lists still need to be initialised separately)
        ddaSpawnedNotes = new Queue<GameObject>[5];
        ddaNoteTimes = new List<float>[5];
        ddaNoteTypes = new List<Variables.NoteType>[5];

        // Load notes for each difficulty level
        DDALoadLevelNoteForDifficulty(0, csvGeneratedNotesFilePath1[Variables.songID]);
        Debug.Log("Length 0: " + ddaNoteTypes[0].Count);
        DDALoadLevelNoteForDifficulty(1, csvGeneratedNotesFilePath2[Variables.songID]);
        Debug.Log("Length 1: " + ddaNoteTypes[1].Count);
        DDALoadLevelNoteForDifficulty(2, csvGeneratedNotesFilePath3[Variables.songID]);
        Debug.Log("Length 2: " + ddaNoteTypes[2].Count);
        DDALoadLevelNoteForDifficulty(3, csvGeneratedNotesFilePath4[Variables.songID]);
        Debug.Log("Length 3: " + ddaNoteTypes[3].Count);
        DDALoadLevelNoteForDifficulty(4, csvGeneratedNotesFilePath5[Variables.songID]);
        Debug.Log("Length 4: " + ddaNoteTypes[4].Count);

        ddaNextNotePointer = new int[5];
        for (int i = 0; i < 5; i++)
            ddaNextNotePointer[i] = 0;

        return true;
    }

    // Helper for DDA Load Level Notes
    // ptr = difficulty level - 1, file path = csvGeneratedNotesFilePath{difficultyLevel}
    private bool DDALoadLevelNoteForDifficulty(int ptr, string filePath)
    {
        StreamReader strReader;
        try
        {
            strReader = new StreamReader(filePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }

        ddaNoteTimes[ptr] = new List<float>();
        ddaNoteTypes[ptr] = new List<Variables.NoteType>();

        bool eof = false;
        while (!eof)
        {
            // loop throug the file and save the note times and note types in their respective lists
            string line = strReader.ReadLine();
            if (line == null)
            {
                eof = true;
                break;
            }

            var values = line.Split(',');
            ddaNoteTimes[ptr].Add(float.Parse(values[0]));
            ddaNoteTypes[ptr].Add((Variables.NoteType)System.Enum.Parse(typeof(Variables.NoteType), values[1]));
        }

        // Offset the times of all the notes by Variables.noteTimeToReachTarget
        for (int i = 0; i < ddaNoteTimes[ptr].Count; i++)
        {
            ddaNoteTimes[ptr][i] -= Variables.noteTimeToReachTarget;
        }

        // Instantiate all the notes in the level 
        ddaSpawnedNotes[ptr] = new Queue<GameObject>();
        for (int i = 0; i < ddaNoteTypes[ptr].Count; i++)
        {
            GameObject spawnedNote;
            if (ddaNoteTypes[ptr][i] == Variables.NoteType.Left || ddaNoteTypes[ptr][i] == Variables.NoteType.Both)
            {
                spawnedNote = Instantiate(notePrefab, leftNoteSpawnPoint.position,
                                          Quaternion.identity, noteParent.transform);
                spawnedNote.GetComponent<NotesController>().InitializeVariables(leftNoteSpawnPoint.position,
                                                                                leftDrumTarget.position,
                                                                                leftNoteDespawnPoint.position,
                                                                                true);
                spawnedNote.GetComponent<AudioSource>().panStereo = -1;

                ddaSpawnedNotes[ptr].Enqueue(spawnedNote);
            }
            if (ddaNoteTypes[ptr][i] == Variables.NoteType.Right || ddaNoteTypes[ptr][i] == Variables.NoteType.Both)
            {
                spawnedNote = Instantiate(notePrefab, rightNoteSpawnPoint.position,
                                          Quaternion.identity, noteParent.transform);
                spawnedNote.GetComponent<NotesController>().InitializeVariables(rightNoteSpawnPoint.position,
                                                                                rightDrumTarget.position,
                                                                                rightNoteDespawnPoint.position,
                                                                                false);
                spawnedNote.GetComponent<AudioSource>().panStereo = 1;

                ddaSpawnedNotes[ptr].Enqueue(spawnedNote);
            }
        }

        return true;
    }

    // End the level at the end of the song
    private IEnumerator EndLevel()
    {
        yield return new WaitForSeconds(musicAudioSource.clip.length);

        Debug.Log("Ending Level");
        crowdAudioSource.clip = crowdCheeringAudioClip;
        crowdAudioSource.loop = false;
        crowdAudioSource.Play();
        PauseLevel();
        endOfLevelUI.SetActive(true);
        ScoreCounter.LogScores(musicAudioSource.clip.name);
    }

    public void PauseLevel()
    {
        Time.timeScale = 0;
        bIsGamePaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        bIsGamePaused = false;
    }
}

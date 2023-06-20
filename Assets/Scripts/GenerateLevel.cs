using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class GenerateLevel : MonoBehaviour
{
    // Notes will not be generated for the first songStartBuffer seconds
    [Range(0, 10)]
    [SerializeField] private float songStartBuffer = 5;
    // Sets the minimum possible time between notes 
    [Range(0.3f, 10)]
    [SerializeField] private float minTimeBetweenNotes = 0.5f;
    // The probability that a double note is generated instead of a single note, note that the
    // probabilities of left or right notes would be (1-probDoubleNote)/2
    [Range(0, 1)]
    [SerializeField] private double probDoubleNote = 0.33;
    // The probability that a note is generated on a beat that's identified as viable
    // In the first 30 seconds
    [Range(0, 1)]
    [SerializeField] private double probNoteGenerated1 = 0.20;
    // Between 30 seconds and 1 minute
    [Range(0, 1)]
    [SerializeField] private double probNoteGenerated2 = 0.30;
    // Between 1 minute and 1:30
    [Range(0, 1)]
    [SerializeField] private double probNoteGenerated3 = 0.40;
    // After 1:30
    [Range(0, 1)]
    [SerializeField] private double probNoteGenerated4 = 0.50;
    // Random seed used when generating the notes
    [SerializeField] private int randomSeed = 42;
    // If the RMS Energy during this beat is < this threshold, no notes will be generated
    [Range(0, 0.1f)]
    [SerializeField] private float RMSEnergyThreshold = 0.01f;
    // Probability of generating a triplet
    [Range(0, 0.4f)]
    [SerializeField] private float probOfTriplet = 0.01f;
    // The Id of the song you're generating notes for
    [SerializeField] private int songID;

    // The time of the last note generated
    private float _lastNoteGenerated;

    // GameEventManager Script must be in the same Game Object
    private GameEventManager gameEventManagerRef;
    // Song Used to Generate the Level
    private AudioClip levelSong;
    // Holds the Beat Times as read form the CSV file
    private List<float> beats;
    // Holds the Beat Count as read from the CSV file
    private List<float> beatCounts;
    // Holds the generated note times
    private List<float> generatedNoteTime;
    // Holds the generated note types
    private List<Variables.NoteType> generatedNoteType;

    // Generates notes for the song based on the variables set in the inspector and saves them in the generic
    // csvGeneratedNotesFilePath
    public void GenerateNotes()
    {
        Debug.Log("Generating Level");
        gameEventManagerRef = GetComponent<GameEventManager>();
        GenerateNotesIn(gameEventManagerRef.csvGeneratedNotesFilePath[songID]);
        Debug.Log("Level Generated, notes saved at " + gameEventManagerRef.csvGeneratedNotesFilePath[songID]);
    }

    // Generates notes for the song in 5 different difficulty levels based on the table of values and
    // saves them in csvGeneratedNotesFilePath1 , ... , csvGeneratedNotesFilePath5
    public void GenerateNotesWithDifferentDifficulties()
    {
        Debug.Log("Generating Notes for all songs with all difficulty levels");
        gameEventManagerRef = GetComponent<GameEventManager>();
        // for all the songs i in the list
        for (int i = 0; i < gameEventManagerRef.csvGeneratedNotesFilePath1.Length; i++)
        {
            songID = i;
            // change the settings for each difficulty
            // Level 1
            minTimeBetweenNotes = 2;
            probNoteGenerated1 = 0.05f;
            probNoteGenerated2 = 0.08f;
            probNoteGenerated3 = 0.1f;
            probNoteGenerated4 = 0.15f;
            probOfTriplet = 0;
            probDoubleNote = 0;
            GenerateNotesIn(gameEventManagerRef.csvGeneratedNotesFilePath1[i]);
            // Level 2
            minTimeBetweenNotes = 1.5f;
            probNoteGenerated1 = 0.05f;
            probNoteGenerated2 = 0.08f;
            probNoteGenerated3 = 0.125f;
            probNoteGenerated4 = 0.18f;
            probOfTriplet = 0;
            probDoubleNote = 0.05f;
            GenerateNotesIn(gameEventManagerRef.csvGeneratedNotesFilePath2[i]);
            // Level 3
            minTimeBetweenNotes = 1.25f;
            probNoteGenerated1 = 0.05f;
            probNoteGenerated2 = 0.1f;
            probNoteGenerated3 = 0.15f;
            probNoteGenerated4 = 0.2f;
            probOfTriplet = 0;
            probDoubleNote = 0.15f;
            GenerateNotesIn(gameEventManagerRef.csvGeneratedNotesFilePath3[i]);
            // Level 4
            minTimeBetweenNotes = 1;
            probNoteGenerated1 = 0.1f;
            probNoteGenerated2 = 0.15f;
            probNoteGenerated3 = 0.2f;
            probNoteGenerated4 = 0.3f;
            probOfTriplet = 0.01f;
            probDoubleNote = 0.33f;
            GenerateNotesIn(gameEventManagerRef.csvGeneratedNotesFilePath4[i]);
            // Level 5
            minTimeBetweenNotes = 0.8f;
            probNoteGenerated1 = 0.15f;
            probNoteGenerated2 = 0.2f;
            probNoteGenerated3 = 0.3f;
            probNoteGenerated4 = 0.35f;
            probOfTriplet = 0.05f;
            probDoubleNote = 0.33f;
            GenerateNotesIn(gameEventManagerRef.csvGeneratedNotesFilePath5[i]);
            Debug.Log("... Generated for " + gameEventManagerRef.songs[i].name);
        }
        Debug.Log("Done");
    }

    private void GenerateNotesIn(string generatedFilePath)
    {
        // Create RNG using the seed
        System.Random _rng = new System.Random(randomSeed);

        // Initialize all variables as needed
        gameEventManagerRef = GetComponent<GameEventManager>();
        levelSong = gameEventManagerRef.songs[songID];
        beats = new List<float>();
        beatCounts = new List<float>();
        generatedNoteTime = new List<float>();
        generatedNoteType = new List<Variables.NoteType>();

        // Check to see if user defined a file path for a location to save the notes, if not 
        // create one.
        if (string.IsNullOrEmpty(generatedFilePath))
        {
            generatedFilePath = Application.persistentDataPath + "GeneratedNotes.csv";
        }

        // Read the CSV file of Beat times and save to the required lists
        if (!ParseCSVFile())
        {
            // If Reading of CSV file failed for any reason, display and error message and stop
            Debug.LogError("Could Not Parse CSV File, please ensure CSVFilePath is correct");
            return;
        }

        // Extract features needed for Generation Algorithm
        float[] rms = new float[levelSong.samples / AudioFeatures.HOP_LENGTH];
        rms = AudioFeatures.Calculate_RMS(levelSong);
        int frame1, frame2;

        // Main Loop to Generate the Notes in the Level
        bool triplet = false;
        Variables.NoteType tripletFirstNote = Variables.NoteType.Left;
        _lastNoteGenerated = 0;
        for (int i = 0; i < beats.Count; i++)
        {
            // If triplet:
            if (triplet)
            {
                Debug.Log("Generated Triplet");
                // Add note between the beats
                generatedNoteTime.Add((beats[i - 1] + beats[i]) / 2);
                generatedNoteType.Add((Variables.NoteType)((int)(tripletFirstNote + 1) % 2));
                // Add the second note on beat
                generatedNoteTime.Add(beats[i]);
                generatedNoteType.Add(tripletFirstNote);

                _lastNoteGenerated = beats[i];

                triplet = false;
                continue;
            }

            // If the time on current beat < songStartBuffer, skip beat
            if (beats[i] < songStartBuffer) continue;
            // If time on current beat < Variables.noteTimeToReachTarget, skip beat 
            // (in case songStartBuffer < Variables.noteTimeToReachTarget)
            if (beats[i] < Variables.noteTimeToReachTarget) continue;
            // If time on current beat < min time between notes, skip beat
            if ((beats[i] - _lastNoteGenerated) < minTimeBetweenNotes) continue;

            // If RMS Energy at time of beat < RMSThreshold, skip beat
            AudioFeatures.GetNearestFrameIndex(AudioFeatures.TimeToSample(beats[i], levelSong.frequency),
                                               levelSong.samples, out frame1, out frame2);
            if (frame2 == -1)
            {
                if (rms[frame1] <= RMSEnergyThreshold) continue;
            }
            else if ((rms[frame1] + rms[frame2]) / 2 <= RMSEnergyThreshold)
                continue;

            // probNoteGenerated chance to generate note
            if (beats[i] < 30)
            {
                if (_rng.NextDouble() > probNoteGenerated1) continue;
            }
            else if (beats[i] < 60)
            {
                if (_rng.NextDouble() > probNoteGenerated2) continue;
            }
            else if (beats[i] < 90)
            {
                if (_rng.NextDouble() > probNoteGenerated3) continue;
            }
            else
            {
                if (_rng.NextDouble() > probNoteGenerated4) continue;
            }

            //Debug.Log("Generating Note");

            // Generate the note
            // First check if triplet should be generated
            if (i != 0 && _rng.NextDouble() < probOfTriplet)
            {
                triplet = true;
                // triplets should only be left or right notes
                tripletFirstNote = (Variables.NoteType)_rng.Next(2);
                generatedNoteType.Add(tripletFirstNote);
            }
            else
            {
                // Not a triplet so
                // First check if double note should be generated
                if (_rng.NextDouble() < probDoubleNote)
                    generatedNoteType.Add(Variables.NoteType.Both);
                else // Generate left or right notes
                    generatedNoteType.Add((Variables.NoteType)_rng.Next(2));
            }
            generatedNoteTime.Add(beats[i]);
            _lastNoteGenerated = beats[i];


        }

        // Save Generated Notes to CSV
        SaveToCSV(generatedFilePath);
    }

    private bool ParseCSVFile()
    {
        // Read File
        StreamReader strReader;
        try
        {
            strReader = new StreamReader(gameEventManagerRef.csvSongBeatFilePath[songID]);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }
            
        bool eof = false;
        while (!eof)
        {
            // loop throug the file and save the beat times and beat counts in their respective lists
            string line = strReader.ReadLine();
            if (line == null)
            {
                eof = true;
                break;
            }

            var values = line.Split(',');
            beats.Add(float.Parse(values[0]));
            beatCounts.Add(float.Parse(values[1]));
        }
        return true;
    }

    private void SaveToCSV(string generatedFilePath)
    {
        StreamWriter sw = new StreamWriter(generatedFilePath);

        using (sw)
        {
            for (int i = 0; i < generatedNoteTime.Count; i++)
            {
                sw.WriteLine(generatedNoteTime[i] + "," + generatedNoteType[i].ToString("G"));
            }
        }

        sw.Close();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class NotesController : MonoBehaviour
{
    private Vector3 startingLocation, drumTargetLocation, despawnLocation;
    //private float timeToTarget;
    private bool bIsHit;
    private AudioSource audioSource;

    public AudioClip noteHit, noteMissed, noteHitEarly, noteHitLate;
    [Range(0.1f, 0.5f)]
    [SerializeField] private float volumeChangeMultiplier = 0.2f;
    [Range(0.1f, 0.5f)]
    [SerializeField] private float pitchChangeMultiplier = 0.2f;

    // The material to change to when the note is hit
    [SerializeField] private Material noteHitMaterial;
    // The material to change to when the note is missed
    [SerializeField] private Material noteMissedMaterial;
    // The child object with the material to change
    [SerializeField] private MeshRenderer noteRenderer;

    // SteamVR action in action set that points to the touchpad to vibrate
    [SerializeField] private SteamVR_Action_Vibration hapticAction;

    // The curve controlling the intensity of the haptic feedback on the Vive Controller
    private AnimationCurve hapticCurve = AnimationCurve.Linear(0, 0, Variables.vibrationDistanceRatio, Variables.maxVibrationIntensity);
    private float inVibrationDistance = 1 - Variables.vibrationDistanceRatio;
    private bool bShouldVibrate;
    private bool bIsLeftNote;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (!audioSource)
        {
            Debug.LogWarning("Please attach audio source to " + gameObject.name);
        }

        if (!(noteHit && noteMissed && noteHitEarly && noteHitLate))
        {
            Debug.LogWarning("Please assign all audio clips to variables in " + gameObject.name);
        }

        if (!(noteHitMaterial && noteMissedMaterial))
        {
            Debug.LogWarning("Please assign materials to noteHitMaterial and noteMissedMaterial in " + gameObject.name);
        }

        if (!(noteRenderer))
        {
            Debug.LogWarning("Shader object not assigned in script in " + gameObject.name);
        }
    }

    private void OnEnable()
    {
        if (drumTargetLocation == null)
            drumTargetLocation = new Vector3(0, 0, 0);

        StartCoroutine(MoveToPosition(transform, drumTargetLocation, Variables.noteTimeToReachTarget));
    }

    //private void OnDisable()
    //{
    //    if (startingLocation == null)
    //        startingLocation = new Vector3(0, 0, 0);
    //    if (despawnLocation == null)
    //        despawnLocation = new Vector3(0, 0, 0);

    //    transform.position = startingLocation;
    //}

    // Controls the movement of the notes
    private IEnumerator MoveToPosition(Transform transform, Vector3 position, 
        float timeToMove, bool movingToFirstTarget = true)
    {
        var currentPos = transform.position;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / timeToMove;
            transform.position = Vector3.Lerp(currentPos, position, t);

            if (t > inVibrationDistance && bShouldVibrate)
            {
                float distance = (t - inVibrationDistance) / Variables.vibrationDistanceRatio;
                float pulse = hapticCurve.Evaluate(distance);
                float seconds = pulse / 1000000f;
                // Pulse according to curve!
                if (bIsLeftNote)
                {
                    hapticAction.Execute(0, seconds, 1f / seconds, 1, SteamVR_Input_Sources.LeftHand);
                }
                else
                {
                    hapticAction.Execute(0, seconds, 1f / seconds, 1, SteamVR_Input_Sources.RightHand);
                }

            }

            yield return null;
        }

        // After Move
        if (movingToFirstTarget)
        {
            bShouldVibrate = false;
            // After Reaching target, keep moving to despawn point
            StartCoroutine(MoveToPosition(transform, despawnLocation, timeToMove, false));
        }
        else
        {
            // Despawn Point is reached
            Destroy(gameObject);
        }
    }

    // Initialize all variables as needed, good to use in case we switch to an object pooling model to improve performance
    public void InitializeVariables(Vector3 _startingLocation, Vector3 _targetLocation, Vector3 _despawnLocation, bool _bIsLeftNote)
    {
        startingLocation = _startingLocation;
        drumTargetLocation = _targetLocation;
        despawnLocation = _despawnLocation;

        bIsHit = false;
        bShouldVibrate = true;
        bIsLeftNote = _bIsLeftNote;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "drum_stick" && !bIsHit)
        {
            bIsHit = true;

            PlayAudioWithModulation(noteHit);
            other.GetComponent<DrumStickController>().VibrateController();
            ScoreCounter.IncrementLevelScore(3);
            ScoreCounter.IncrementNoteHit();

            // DDA Stuff
            if (Variables.bAdaptiveDifficulty)
            {
                Variables.DDATotalNotesThisCycle++;
            }

            noteRenderer.material = noteHitMaterial;
        }
    }

    // To be called from Note Other Trigger if the note is hit early, late or missed
    public void TriggerNoteHit()
    {
        bIsHit = true;

        // DDA Stuff
        if (Variables.bAdaptiveDifficulty)
        {
            Variables.DDATotalNotesThisCycle++;
            Variables.DDA_NotesMissedThisCycle++;
        }

        noteRenderer.material = noteMissedMaterial;
    }

    // Checks if the note is already hit
    public bool CheckNoteHit()
    {
        return bIsHit;
    }

    public void PlayAudioWithModulation(AudioClip clip)
    {
        audioSource.volume = Random.Range(0.7f - volumeChangeMultiplier, 0.7f);
        audioSource.pitch = Random.Range(1 - pitchChangeMultiplier, 1 + pitchChangeMultiplier);

        audioSource.PlayOneShot(clip);
    }
}

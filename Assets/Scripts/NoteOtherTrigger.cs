using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteOtherTrigger : MonoBehaviour
{
    private NotesController parentScript;
    [SerializeField] Variables.NoteTriggerType type;

    private void Awake()
    {
        parentScript = transform.parent.GetComponent<NotesController>();

        if (!parentScript)
            Debug.LogError(gameObject.name + " failed to find parent NotesController Script");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag=="drum_stick" && !parentScript.CheckNoteHit())
        {
            if (type == Variables.NoteTriggerType.Early)
                parentScript.PlayAudioWithModulation(parentScript.noteHitEarly);
            else
                parentScript.PlayAudioWithModulation(parentScript.noteHitLate);

            // Update Score Accordingly
            ScoreCounter.IncrementLevelScore(1);
            ScoreCounter.IncrementNotesHitOffBeat();

            // Set the variables to ensure this note can not be hit again
            parentScript.TriggerNoteHit();
        }
    }

    // When the last collider is missed, then play the noteMissed feedback
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "drum_target" && type == Variables.NoteTriggerType.Late)
        {
            if (!parentScript.CheckNoteHit())
            {
                parentScript.PlayAudioWithModulation(parentScript.noteMissed);

                // Update Score Accordingly
                ScoreCounter.IncrementNotesMissed();

                // Set the material to note missed material
                parentScript.TriggerNoteHit();
            }
        }
    }
}

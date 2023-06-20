using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndLevelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText, notesHitText, notesMissedText;
    [SerializeField] private AudioClip goodJob, totalScore, notesHit, notesMissed;
    // Amount of time between one text being read and another
    [SerializeField] private float voiceBuffer;
    private UIScore uiScoreRef;

    private void Awake()
    {
        if (!(scoreText && notesHitText && notesMissedText))
        {
            Debug.LogError("Please Assign all Text UI Elements accordingly in " + gameObject.name);
        }

        if (!(goodJob && totalScore && notesHit && notesMissed))
        {
            Debug.LogWarning("Please ensure all audio clips are assigned in " + gameObject.name);
        }

        uiScoreRef = GetComponentInParent<UIScore>();
        if (!uiScoreRef)
        {
            Debug.LogError(name + " could not find UIScore Script in Parent");
        }
    }

    private void OnEnable()
    {
        scoreText.text = ScoreCounter.GetScore().ToString("0000");
        notesHitText.text = ScoreCounter.GetTotalNotesHit().ToString("0000");
        notesMissedText.text = ScoreCounter.GetNotesMissed().ToString("0000");

        StartCoroutine(StartDictation());
    }

    private IEnumerator StartDictation()
    {
        AudioSource audioSource = uiScoreRef.GetAudioSource();

        yield return new WaitForSecondsRealtime(1);

        // Good job
        audioSource.clip = goodJob;
        audioSource.Play();
        yield return new WaitForSecondsRealtime(audioSource.clip.length + voiceBuffer);

        // Total Score
        audioSource.clip = totalScore;
        audioSource.Play();
        yield return new WaitForSecondsRealtime(audioSource.clip.length + voiceBuffer);
        char[] digits = new char[4];
        digits = scoreText.text.ToCharArray();
        yield return StartCoroutine(uiScoreRef.Dictate_V2(digits));
        yield return new WaitForSecondsRealtime(voiceBuffer);

        // Notes Hit
        audioSource.clip = notesHit;
        audioSource.Play();
        yield return new WaitForSecondsRealtime(audioSource.clip.length + voiceBuffer);
        digits = notesHitText.text.ToCharArray();
        yield return StartCoroutine(uiScoreRef.Dictate_V2(digits));
        yield return new WaitForSecondsRealtime(voiceBuffer);

        // Notes Missed
        audioSource.clip = notesMissed;
        audioSource.Play();
        yield return new WaitForSecondsRealtime(audioSource.clip.length + voiceBuffer);
        digits = notesMissedText.text.ToCharArray();
        yield return StartCoroutine(uiScoreRef.Dictate_V2(digits));
        yield return new WaitForSecondsRealtime(voiceBuffer);
    }

}

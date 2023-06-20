using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumCollision : MonoBehaviour
{
    [Range(0.1f, 0.5f)]
    [SerializeField] private float volumeChangeMultiplier = 0.2f;
    [Range(0.1f, 0.5f)]
    [SerializeField] private float pitchChangeMultiplier = 0.2f;


    private AudioSource drumAudioSource;

    private void Awake()
    {
        drumAudioSource = GetComponent<AudioSource>();
        if (!drumAudioSource)
        {
            Debug.LogError("No Audio Source Found for " + gameObject.name);
        }
        else
        {
            if (!drumAudioSource.clip)
            {
                Debug.LogWarning("Drum Audio Source had no clip in " + gameObject.name);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        //Debug.Log("Triggered by " + other.gameObject.name);
        drumAudioSource.volume = Random.Range(1 - volumeChangeMultiplier, 1);
        drumAudioSource.pitch = Random.Range(1 - pitchChangeMultiplier, 1 + pitchChangeMultiplier);

        drumAudioSource.PlayOneShot(drumAudioSource.clip);
    }
}

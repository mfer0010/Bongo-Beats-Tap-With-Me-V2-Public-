using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ButtonScript : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public string songName, artist;
    public float clipStartTime;
    public AudioClip song;

    [SerializeField] private TMP_Text textObject;
    private AudioSource audioSource;
    private bool isSelected;

    private void Awake()
    {
        if (!song)
        {
            Debug.LogWarning(gameObject.name + " has no associated song");
        }

        if (!textObject)
        {
            Debug.LogError("Please assign the button's text object properly in " + gameObject.name);
        }
    }

    private void Start()
    {
        textObject.text = "\"" + songName + "\"" + "(by " + artist + ")";
 
        audioSource = FindObjectOfType<AudioSource>();
        if (!audioSource)
            Debug.LogError(gameObject.name + " couldn't find Audio Source in Scene");
    }

    private void Update()
    {         
        if (isSelected && audioSource.isPlaying && audioSource.time >= clipStartTime + 20)
        {
            audioSource.Stop();
            StartCoroutine(StartSongIn(1));
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        StartCoroutine(StartSongIn(1.2f));
    }

    public void OnDeselect(BaseEventData data)
    {
        //Debug.Log(songName + " deselected");

        audioSource.Stop();
        audioSource.clip = null;

        isSelected = false;
    }

    private IEnumerator StartSongIn(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        
        if (isSelected)
        {
            audioSource.clip = song;
            audioSource.time = clipStartTime;
            audioSource.Play();
        }
    }
}

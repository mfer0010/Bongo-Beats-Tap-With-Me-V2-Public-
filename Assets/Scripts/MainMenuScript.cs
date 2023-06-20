using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] private GameObject firstSongButton;

    private void Awake()
    {
        if (!firstSongButton)
            Debug.LogWarning("No Button is set to be active at first in " + gameObject.name);
    }

    private void Start()
    {
        SetFocusToButtons();
    }

    // Sets focus to the buttons which will start playing the songs accordingly
    public void SetFocusToButtons()
    {
        // Always clear selected game object otherwise it won't work well
        EventSystem.current.SetSelectedGameObject(null);
        // Then set the desired selected object
        EventSystem.current.SetSelectedGameObject(firstSongButton);
    }
}

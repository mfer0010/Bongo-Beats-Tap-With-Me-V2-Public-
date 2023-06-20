using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    private UIScore uIScore;

    private void Awake()
    {
        uIScore = GetComponent<UIScore>();
        if (!uIScore)
            Debug.LogError(gameObject.name + " failed to find UIScore Component, please ensure TutorialUI " +
                "script is in a game object with audio source and UIScore components");
    }

    public void IncrementScore()
    {
        ScoreCounter.IncrementLevelScore(127);
    }
}

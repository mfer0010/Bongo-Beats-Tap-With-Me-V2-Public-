using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonManager : MonoBehaviour
{
    private MainMenuScript mainMenuRef;
    public GameObject warning;

    private void Start()
    {
        mainMenuRef = FindObjectOfType<MainMenuScript>();
    }

    // Called when the song is chosen to load the level and set the desired song with DDA
    public void LoadLevel(int songID)
    {
        Debug.Log("Loading Level");

        Variables.songID = songID;
        Variables.ExperimentID = "Final Experiment";

        if (!Variables.bAdaptiveDifficulty)
            Variables.bAdaptiveDifficulty = true;

        SceneManager.LoadScene(1);
    }

    // Called for Experiment 3 with DDA
    public void LoadLevelExperiment3DDA(int songID)
    {
        Debug.Log("Loading Level");

        Variables.songID = songID;
        Variables.ExperimentID = "Experiment 3 - DDA";

        if (!Variables.bAdaptiveDifficulty)
            Variables.bAdaptiveDifficulty = true;

        SceneManager.LoadScene(1);
    }

    // Called when the song is chosen to load the level and set the desired song without DDA
    public void LoadLevelNotDDA(int songID)
    {
        Debug.Log("Loading Level");

        Variables.songID = songID;
        Variables.ExperimentID = "Experiment 3 - Normal";

        if (Variables.bAdaptiveDifficulty)
            Variables.bAdaptiveDifficulty = false;

        SceneManager.LoadScene(1);
    }

    // To be called when the text field is filled in, to change focus to the buttons and save the participant ID
    public void LogID(string id)
    {
        int.TryParse(id, out Variables.playerID);

        StartCoroutine(ChangeButtonFocus());
    }

    // To be called in Experiment 3 - DDA
    public void LogIdWarningDDA(string id)
    {
        int.TryParse(id, out Variables.playerID);

        if (Variables.playerID % 2 == 0)
            warning.SetActive(true);

        StartCoroutine(ChangeButtonFocus());
    }

    // To be called in Experiment 3 - Normal
    public void LogIdWarningNormal(string id)
    {
        int.TryParse(id, out Variables.playerID);

        if (Variables.playerID % 2 != 0)
            warning.SetActive(true);

        StartCoroutine(ChangeButtonFocus());
    }

    private IEnumerator ChangeButtonFocus()
    {
        yield return new WaitForEndOfFrame();

        mainMenuRef.SetFocusToButtons();
    }

    // For Experiment 1, Sets odd numbers to Control Group and Even Numbers to experiment group
    public void LoadLevelExperiment1()
    {
        if (Variables.playerID % 2 == 0)
        {
            Variables.ExperimentID = "Experiment 1 - Experiment";
            Variables.songID = 0;       // Experiment Group (Generated)  
        }
        else
        {
            Variables.ExperimentID = "Experiment 1 - Control";
            Variables.songID = 15;      // Control Group (Original)
        }

        if (Variables.bAdaptiveDifficulty)
            Variables.bAdaptiveDifficulty = false;

        SceneManager.LoadScene(1);
    }

    // For Experiment 2, Always load butterfly original song as old version will be the control group
    public void LoadLevelExperiment2()
    {
        if (Variables.bAdaptiveDifficulty)
            Variables.bAdaptiveDifficulty = false;

        Variables.songID = 15;
        Variables.ExperimentID = "Experiment 2";
        SceneManager.LoadScene(1);
    }
}

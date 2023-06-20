using UnityEngine;
using UnityEditor;


// NOTE: MUST BE IN THE EDITOR FOLDER

// This script allows us to customize the fieldsshown in the inspector for the Generate Level Script
[CustomEditor(typeof(GenerateLevel))]
public class GenerateLevelScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GenerateLevel generateLevelScript = (GenerateLevel) target;
        if (GUILayout.Button("Generate Notes"))
        {
            generateLevelScript.GenerateNotes();
        }

        if (GUILayout.Button("Generate DDA Notes"))
        {
            generateLevelScript.GenerateNotesWithDifferentDifficulties();
        }
    }
}

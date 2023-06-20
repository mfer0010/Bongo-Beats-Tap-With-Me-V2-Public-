using UnityEngine;

public static class Variables
{ 
    public enum NoteType { Left = 0,
                           Right = 1, 
                           Both = 2 
    };

    public enum NoteTriggerType { Early = 0,
                                  Late = 1

    };

    // Controls the speed of the movement of the notes
    public const float noteTimeToReachTarget = 2.0f;

    // Vive Vibration for audio haptics mode (for coordinator)
    // Distance ratio 0.2 => when note is (1-0.2)% to target, start vibrating
    public const float vibrationDistanceRatio = 0.50f;
    public const float maxVibrationIntensity = 1600;

    // Controller Vibration (for musician)
    public const float musicianFeedbackVibrationIntensity = 1f;
    public const float musicianFeedbackTimeToStopVibration = 0.35f;

    // Player ID used for logging scores 
    public static int playerID = 1;
    // Experiment ID used in experiment logs
    public static string ExperimentID = "Final Experiment";
    // Song ID to play
    public static int songID = 0;

    // Toggle adaptive difficulty
    public static bool bAdaptiveDifficulty = false;
    public static int DDAWindowLength = 8;
    public static int DDAStartCheck = 8;
    public static int DDATotalNotesThisCycle = 0;
    public static int DDA_NotesMissedThisCycle = 0;
    public static float ddaIncrementLevelThreshold = 0.25f;
    public static float ddaDecrementLevelThreshold = 0.55f;
}

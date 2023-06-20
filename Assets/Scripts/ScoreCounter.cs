using System.IO;

public static class ScoreCounter
{
    private static ushort levelScore;
    private static ushort notesHit;
    private static ushort notesHitOffBeat;
    private static ushort notesMissed;

    private static string data_filePath = "Assets/DataLogs/Scores.csv";

    // To be called at the begging of the level
    public static void ResetValues()
    {
        // Reset Values at the beginning of the level
        levelScore = 0;
        notesHit = 0;
        notesHitOffBeat = 0;
        notesMissed = 0;
    }

    public static void IncrementLevelScore(ushort amount)
    {
        levelScore += amount;
    }

    public static void IncrementNotesMissed()
    {
        notesMissed++;
    }

    public static void IncrementNotesHitOffBeat()
    {
        notesHitOffBeat++;
    }

    public static void IncrementNoteHit()
    {
        notesHit++;
    }

    public static ushort GetScore()
    {
        return levelScore;
    }

    public static ushort GetTotalNotesHit()
    {
        return notesHit;
    }

    public static ushort GetNotesMissed()
    {
        return (ushort) (notesMissed + notesHitOffBeat);
    }

    public static void LogScores(string levelSong)
    {
        bool bNewFile = false;

        // Check if this is the first time writing to the file, so we can add the header row
        // if needed
        if (!File.Exists(data_filePath))
            bNewFile = true;

        // Append to file if it already exists
        StreamWriter sw = new StreamWriter(data_filePath, true);

        using (sw)
        {
            // Add header row if file is new
            if (bNewFile)
                sw.WriteLine("PlayerID,Experiment,Song Name,Total Score,# Notes Hit On Beat,# Notes Hit Off Beat,# Notes Missed");

            sw.WriteLine(Variables.playerID.ToString() + "," + Variables.ExperimentID + ","  + levelSong + "," + levelScore.ToString() +
                "," + notesHit.ToString() + "," + notesHitOffBeat.ToString() + "," + notesMissed.ToString());
        }
        sw.Close();
    }
}

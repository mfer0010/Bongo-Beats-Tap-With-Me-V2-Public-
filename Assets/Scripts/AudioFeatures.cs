using UnityEngine;
using System.IO;

public static class AudioFeatures
{
    public static int FRAME_LENGTH = 1024;
    public static int HOP_LENGTH = 512;

    public static float[] Calculate_RMS(AudioClip audio_clip)
    {
        float[] channel1 = new float[audio_clip.samples];
        float[] channel2 = new float[audio_clip.samples];

        GetDataInChannels(audio_clip, out channel1, out channel2);

        // Checking Results:
        //string string1 = "";
        //string string2 = "";
        //for (int i = 0; i < 5000; i++)
        //{
        //    string1 += channel1[i].ToString() + ",";
        //    string2 += channel2[i].ToString() + ",";
        //}

        //StreamWriter sw = new StreamWriter("Assets/Audio/test2.csv");
        //using (sw)
        //{
        //    sw.WriteLine(string1);
        //    sw.WriteLine(string2);
        //}

        // calculate rms over each frame
        float[] rms = new float[audio_clip.samples / HOP_LENGTH];
        for (int i = 0; i < audio_clip.samples / HOP_LENGTH; i++)
        {
            rms[i] = Mathf.Sqrt((MeanSquareArray(channel1, i * HOP_LENGTH, i * HOP_LENGTH + FRAME_LENGTH)
                + MeanSquareArray(channel2, i * HOP_LENGTH, i * HOP_LENGTH + FRAME_LENGTH)) / 2);
        }
        return rms;
    }

    // Given a sample number, returns the 2 frames that the sample is a part of
    // -1 implies that it is only part of 1 frame (edge cases)
    public static void GetNearestFrameIndex(int sampleNr, int audioClipSamples, out int first_frame, out int second_frame)
    {
        // integer division, will return only the whole number
        int frame1 = sampleNr / HOP_LENGTH;

        int frame2;
        if (sampleNr < HOP_LENGTH)
            frame2 = -1;
        // looks a bit weird, but the first division is an intereger division, so only the whole number remains,
        // Thus this will return the centre of the last frame
        else if (sampleNr > (audioClipSamples / HOP_LENGTH) * HOP_LENGTH)
            frame2 = -1;
        else
            frame2 = frame1 - 1;
            

        first_frame = frame1;
        second_frame = frame2;
    }

    // Given a time, returns the sample number (or nearest sample number)
    public static int TimeToSample(float time, int frequency)
    {
        return (int) (time * frequency);
    }

    private static void GetDataInChannels(AudioClip audio_clip, out float[] ret_channel1, out float[] ret_channel2)
    {
        float[] channel1 = new float[audio_clip.samples];
        float[] channel2 = new float[audio_clip.samples];

        // This function only works for audio files with 2 channels
        if (audio_clip.channels != 2)
        {
            Debug.LogError("Please use an audio file with 2 channels");
            ret_channel1 = channel1;
            ret_channel2 = channel2;
            return;
        }

        // Get the amplitudes of each sample on each channel
        // this returns in the order channel1[0], channel2[0], channel1[1], channel2[1], ....
        float[] clipData = new float[audio_clip.samples * audio_clip.channels];
        audio_clip.GetData(clipData, 0); 

        // Separate the channels into separate arrays
        int ptr = 0;
        int i = 0;
        while (ptr < audio_clip.samples * audio_clip.channels)
        {
            channel1[i] = clipData[ptr++];
            channel2[i] = clipData[ptr++];
            i++;
        }

        ret_channel1 = channel1;
        ret_channel2 = channel2;
    }

    // Mean square of elements in an array from start index (included) to end index (not included)
    private static float MeanSquareArray(float [] array, int start_index, int end_index)
    {
        float sum = 0;
        int elems = 0;
        for (int i = start_index; i < end_index; i++)
        {
            if (i >= array.Length) break;

            sum += array[i]*array[i];
            elems++;
        }

        return sum / elems;
    }
}

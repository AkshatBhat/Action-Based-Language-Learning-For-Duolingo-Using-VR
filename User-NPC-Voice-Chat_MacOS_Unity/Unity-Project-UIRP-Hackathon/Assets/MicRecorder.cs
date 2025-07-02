using UnityEngine;

public class MicRecorder : MonoBehaviour
{
    public AudioClip recordedClip;
    private bool isRecording = false;
    private string micName;
    private int maxDuration = 10;

    public void StartRecording()
    {
        if (isRecording) return;
        micName = Microphone.devices[0];
        recordedClip = Microphone.Start(micName, false, maxDuration, 16000);
        isRecording = true;
        Debug.Log("🎙️ Started recording...");
    }

    public AudioClip StopRecording()
    {
        if (!isRecording) return null;
        Microphone.End(micName);
        isRecording = false;
        Debug.Log("🛑 Stopped recording.");
        return recordedClip;
    }
}

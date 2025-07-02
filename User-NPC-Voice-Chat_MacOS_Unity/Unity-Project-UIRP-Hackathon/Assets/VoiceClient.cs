using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class VoiceClient : MonoBehaviour
{
    public MicRecorder mic;
    public AudioSource speaker;

    private string backendUrl = "http://localhost:8000/process_audio";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            mic.StartRecording();
        else if (Input.GetKeyUp(KeyCode.Space))
            StartCoroutine(SendAudioToBackend());
    }

    IEnumerator SendAudioToBackend()
    {
        AudioClip clip = mic.StopRecording();
        if (clip == null) yield break;

        string filePath = Path.Combine(Application.persistentDataPath, "input.wav");
        SaveWav(filePath, clip);

        byte[] wavData = File.ReadAllBytes(filePath);

        WWWForm form = new WWWForm();
        form.AddBinaryData("audio", wavData, "input.wav", "audio/wav");
        form.AddField("language", "es-ES");

        UnityWebRequest req = UnityWebRequest.Post(backendUrl, form);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Request failed: " + req.error);
            yield break;
        }

        string json = req.downloadHandler.text;
        var response = JsonUtility.FromJson<VoiceResponse>(json);
        Debug.Log("📝 Tú (transcript): " + response.transcript);
        Debug.Log("✅ Gemini: " + response.text);


        // Download and play audio
        UnityWebRequest audioReq = UnityWebRequestMultimedia.GetAudioClip("http://localhost:8000/" + response.audio, AudioType.WAV);
        yield return audioReq.SendWebRequest();

        if (audioReq.result == UnityWebRequest.Result.Success)
        {
            AudioClip responseClip = DownloadHandlerAudioClip.GetContent(audioReq);
            speaker.clip = responseClip;

            if (responseClip != null)
            {
                Debug.Log("🔈 Audio clip loaded. Duration: " + responseClip.length + " seconds");
                speaker.Play();
            }
            else
            {
                Debug.LogError("⚠️ Failed to load AudioClip from response.");
            }
        }
        else
        {
            Debug.LogError("❌ Audio download failed: " + audioReq.error);
        }

    }

    void SaveWav(string path, AudioClip clip)
    {
        // Use helper to write WAV header and samples
        byte[] wavData = WavUtility.FromAudioClip(clip);
        File.WriteAllBytes(path, wavData);
    }

    [System.Serializable]
    public class VoiceResponse
    {
        public string text;
        public string transcript;
        public string audio;
    }
}

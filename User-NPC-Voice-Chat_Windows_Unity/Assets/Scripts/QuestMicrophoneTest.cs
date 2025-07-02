using UnityEngine;
using UnityEngine.UI;

public class QuestMicrophoneTest : MonoBehaviour
{
    [Header("Microphone Test Settings")]
    public int sampleRate = 44100;
    public int recordLength = 10;
    public float volumeThreshold = 0.01f;
    
    [Header("Visual Feedback")]
    public Slider volumeSlider; // Optional: drag a UI slider here
    public Text statusText; // Optional: drag a UI text here
    
    private AudioSource audioSource;
    private string microphoneName;
    private bool isRecording = false;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        TestMicrophoneSetup();
    }
    
    void TestMicrophoneSetup()
    {
        Debug.Log("=== QUEST MICROPHONE TEST ===");
        
        // List all available microphones
        Debug.Log($"Total microphones found: {Microphone.devices.Length}");
        
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("❌ NO MICROPHONES DETECTED!");
            UpdateStatus("❌ No microphones found");
            return;
        }
        
        // List each microphone
        for (int i = 0; i < Microphone.devices.Length; i++)
        {
            Debug.Log($"Microphone {i}: '{Microphone.devices[i]}'");
        }
        
        // Use first available microphone (usually Quest headset mic)
        microphoneName = Microphone.devices[0];
        Debug.Log($"✅ Using microphone: {microphoneName}");
        UpdateStatus($"Using: {microphoneName}");
        
        // Start recording test
        StartMicrophoneTest();
    }
    
    void StartMicrophoneTest()
    {
        try
        {
            Debug.Log("Starting microphone recording test...");
            audioSource.clip = Microphone.Start(microphoneName, true, recordLength, sampleRate);
            
            if (audioSource.clip == null)
            {
                Debug.LogError("❌ Failed to start microphone recording!");
                UpdateStatus("❌ Recording failed");
                return;
            }
            
            isRecording = true;
            Debug.Log("✅ Microphone recording started successfully!");
            UpdateStatus("✅ Recording active - SPEAK NOW!");
            
            // Wait for microphone to actually start
            while (!(Microphone.GetPosition(microphoneName) > 0)) 
            {
                // Wait for recording to begin
            }
            
            // Optional: Play back what we're recording (you'll hear yourself)
            audioSource.Play();
            Debug.Log("🎤 You should now hear your own voice if mic is working");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Microphone test failed: {e.Message}");
            UpdateStatus($"❌ Error: {e.Message}");
        }
    }
    
    void Update()
    {
        if (isRecording)
        {
            // Get current volume level
            float volume = GetMicrophoneVolume();
            
            // Update UI
            if (volumeSlider != null)
                volumeSlider.value = volume;
                
            // Check if voice is detected
            if (volume > volumeThreshold)
            {
                Debug.Log($"🗣️ VOICE DETECTED! Volume: {volume:F3}");
                UpdateStatus($"🗣️ Voice detected! Vol: {volume:F2}");
            }
        }
    }
    
    float GetMicrophoneVolume()
    {
        if (audioSource.clip == null) return 0f;
        
        float[] samples = new float[256];
        int micPosition = Microphone.GetPosition(microphoneName);
        
        if (micPosition > 256)
        {
            audioSource.clip.GetData(samples, micPosition - 256);
            
            float sum = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                sum += Mathf.Abs(samples[i]);
            }
            
            return sum / samples.Length;
        }
        
        return 0f;
    }
    
    void RestartMicrophoneTest()
    {
        Debug.Log("🔄 Restarting microphone test...");
        StopMicrophoneTest();
        StartMicrophoneTest();
    }
    
    void StopMicrophoneTest()
    {
        if (isRecording)
        {
            Microphone.End(microphoneName);
            audioSource.Stop();
            isRecording = false;
            Debug.Log("⏹️ Microphone test stopped");
            UpdateStatus("⏹️ Test stopped");
        }
    }
    
    void UpdateStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
    
    void OnDestroy()
    {
        StopMicrophoneTest();
    }
    
    // GUI for testing without UI elements
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Microphone: {microphoneName}");
        GUI.Label(new Rect(10, 30, 300, 20), $"Recording: {isRecording}");
        
        if (isRecording)
        {
            float volume = GetMicrophoneVolume();
            GUI.Label(new Rect(10, 50, 300, 20), $"Volume: {volume:F3}");
            
            // Volume bar
            GUI.Box(new Rect(10, 70, 200, 20), "");
            GUI.Box(new Rect(10, 70, volume * 2000, 20), "");
            
            // Status indicator
            if (volume > volumeThreshold)
            {
                GUI.Label(new Rect(10, 95, 300, 20), "🗣️ VOICE DETECTED!");
            }
            else
            {
                GUI.Label(new Rect(10, 95, 300, 20), "Speak into microphone...");
            }
        }
    }
}
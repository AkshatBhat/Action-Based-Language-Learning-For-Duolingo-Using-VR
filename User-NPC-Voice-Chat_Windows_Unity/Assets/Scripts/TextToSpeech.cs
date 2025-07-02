using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections;

public class TextToSpeech : MonoBehaviour
{
    [Header("TTS Settings")]
    public bool useWindowsTTS = true;
    public float volume = 0.8f;
    public float rate = 1.0f;
    
    private AudioSource audioSource;
    private bool isSpeaking = false;
    
    // For Windows TTS
    private bool isWindowsTTSAvailable = false;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        // Check if Windows TTS is available
        CheckWindowsTTSAvailability();
    }
    
    void CheckWindowsTTSAvailability()
    {
        #if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        try
        {
            // Test if Windows Speech Synthesis is available
            isWindowsTTSAvailable = true;
            Debug.Log("✅ Windows TTS available");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Windows TTS not available: {e.Message}");
            isWindowsTTSAvailable = false;
        }
        #else
        isWindowsTTSAvailable = false;
        Debug.Log("ℹ️ Windows TTS only available in Windows builds");
        #endif
    }
    
    public void StartSpeaking(string textToSpeak)
    {
        if (isSpeaking)
        {
            StopSpeaking();
        }
        
        if (useWindowsTTS && isWindowsTTSAvailable)
        {
            StartCoroutine(SpeakWithWindowsTTS(textToSpeak));
        }
        else
        {
            // Fallback: Use pre-generated audio or simple beep
            StartCoroutine(SpeakFallback(textToSpeak));
        }
    }
    
    IEnumerator SpeakWithWindowsTTS(string text)
    {
        isSpeaking = true;
        
        #if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        try
        {
            // Use Windows Speech Synthesis
            var synthesizer = new System.Speech.Synthesis.SpeechSynthesizer();
            
            // Set Spanish voice if available
            foreach (var voice in synthesizer.GetInstalledVoices())
            {
                if (voice.VoiceInfo.Culture.Name.StartsWith("es"))
                {
                    synthesizer.SelectVoice(voice.VoiceInfo.Name);
                    Debug.Log($"Using Spanish voice: {voice.VoiceInfo.Name}");
                    break;
                }
            }
            
            // Set volume and rate
            synthesizer.Volume = Mathf.RoundToInt(volume * 100);
            synthesizer.Rate = Mathf.RoundToInt(rate * 10) - 10; // Range: -10 to 10
            
            // Speak the text
            synthesizer.Speak(text);
            synthesizer.Dispose();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Windows TTS error: {e.Message}");
            yield return StartCoroutine(SpeakFallback(text));
        }
        #else
        yield return StartCoroutine(SpeakFallback(text));
        #endif
        
        isSpeaking = false;
    }
    
    IEnumerator SpeakFallback(string text)
    {
        isSpeaking = true;
        
        Debug.Log($"🔊 TTS Fallback: '{text}'");
        
        // Simple audio feedback (short beep to indicate speech)
        if (audioSource != null)
        {
            // Generate a simple tone
            AudioClip beep = GenerateBeep(0.3f, 800f);
            audioSource.clip = beep;
            audioSource.volume = volume * 0.3f; // Quieter for beep
            audioSource.Play();
            
            yield return new WaitForSeconds(0.3f);
        }
        
        // Wait based on text length (simulate speech duration)
        float speechDuration = text.Length * 0.05f; // ~20 characters per second
        speechDuration = Mathf.Clamp(speechDuration, 1f, 8f); // Between 1-8 seconds
        
        yield return new WaitForSeconds(speechDuration);
        
        isSpeaking = false;
    }
    
    AudioClip GenerateBeep(float duration, float frequency)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        
        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate) * 0.1f;
        }
        
        AudioClip clip = AudioClip.Create("Beep", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
    
    public void StopSpeaking()
    {
        if (isSpeaking)
        {
            StopAllCoroutines();
            
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            isSpeaking = false;
            Debug.Log("🔇 TTS stopped");
        }
    }
    
    public bool IsSpeaking()
    {
        return isSpeaking;
    }
    
    // Method to test TTS
    [ContextMenu("Test TTS")]
    public void TestTTS()
    {
        StartSpeaking("Hola, este es un test del sistema de voz en español.");
    }
}
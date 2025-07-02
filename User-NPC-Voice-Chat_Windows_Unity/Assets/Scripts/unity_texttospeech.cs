using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

namespace LanguageVR.Pipeline.TextToSpeech
{
    public class GoogleCloudTextToSpeechService : MonoBehaviour
    {
        private AudioSource audioSource;
        private System.Action<string> logCallback;
        private GoogleCloudAuth authService;
        
        // API Configuration
        private const string TTS_API_URL = "https://texttospeech.googleapis.com/v1/text:synthesize";
        
        // Current voice settings
        private string currentVoiceName = "es-ES-Neural2-A";
        private string currentLanguageCode = "es-ES";
        private string currentGender = "FEMALE";
        
        // Audio settings
        private double speakingRate = 1.0;
        private double pitch = 0.0;
        private double volumeGainDb = 0.0;

        [System.Serializable]
        public class TTSRequest
        {
            public Input input;
            public Voice voice;
            public AudioConfig audioConfig;
        }

        [System.Serializable]
        public class Input
        {
            public string text;
        }

        [System.Serializable]
        public class Voice
        {
            public string languageCode;
            public string name;
            public string ssmlGender;
        }

        [System.Serializable]
        public class AudioConfig
        {
            public string audioEncoding = "LINEAR16";
            public int sampleRateHertz = 24000;
            public double speakingRate = 1.0;
            public double pitch = 0.0;
            public double volumeGainDb = 0.0;
        }

        [System.Serializable]
        public class TTSResponse
        {
            public string audioContent;
        }

        void Awake()
        {
            // Add AudioSource component if not present
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Configure AudioSource for VR
            audioSource.spatialBlend = 0f; // 2D audio
            audioSource.volume = 1f;
            
            // Add authentication service
            authService = gameObject.AddComponent<GoogleCloudAuth>();
        }

        public void Initialize(System.Action<string> logger = null)
        {
            logCallback = logger;
            InitializeTextToSpeech();
        }

        private void InitializeTextToSpeech()
        {
            try
            {
                LogMessage("🎙️ Initializing Google Cloud Text-to-Speech...");

                // Initialize authentication service
                authService.Initialize(LogMessage);

                LogMessage("✅ Text-to-Speech initialized successfully");
                LogMessage($"🗣️ Default voice: {currentVoiceName} ({currentGender})");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Error initializing Text-to-Speech: {ex.Message}");
            }
        }

        public IEnumerator SpeakCoroutine(string text, bool saveToFile = false)
        {
            if (authService == null || !authService.IsReady())
            {
                LogMessage("❌ Authentication not ready - check setup instructions above");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                LogMessage("⚠️ No text provided to speak");
                yield break;
            }

            LogMessage($"🗣️ Speaking: \"{text}\"");

            // Get API key
            string apiKey = "";
            yield return StartCoroutine(authService.GetValidAPIKey((key) => apiKey = key));

            if (string.IsNullOrEmpty(apiKey))
            {
                LogMessage("❌ Could not obtain API key");
                yield break;
            }

            // Create request object
            TTSRequest request = new TTSRequest
            {
                input = new Input { text = text },
                voice = new Voice 
                { 
                    languageCode = currentLanguageCode,
                    name = currentVoiceName,
                    ssmlGender = currentGender
                },
                audioConfig = new AudioConfig
                {
                    audioEncoding = "LINEAR16",
                    sampleRateHertz = 24000,
                    speakingRate = speakingRate,
                    pitch = pitch,
                    volumeGainDb = volumeGainDb
                }
            };

            // Convert to JSON
            string jsonRequest = JsonUtility.ToJson(request);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);

            // Create web request with API key
            using (UnityWebRequest www = new UnityWebRequest(TTS_API_URL + "?key=" + apiKey, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    TTSResponse response = JsonUtility.FromJson<TTSResponse>(www.downloadHandler.text);
                    
                    if (!string.IsNullOrEmpty(response.audioContent))
                    {
                        LogMessage("✅ Speech synthesized successfully");
                        
                        // Convert base64 audio to bytes and play
                        byte[] audioData = Convert.FromBase64String(response.audioContent);
                        yield return StartCoroutine(PlayAudioCoroutine(audioData, 24000));

                        // Optionally save to file
                        if (saveToFile)
                        {
                            string savedFile = Path.Combine(Application.persistentDataPath, 
                                $"npc_response_{DateTime.Now:yyyyMMdd_HHmmss}.wav");
                            SaveWavFile(savedFile, audioData, 24000);
                            LogMessage($"💾 Audio saved to: {savedFile}");
                        }
                    }
                    else
                    {
                        LogMessage("❌ Empty audio response");
                    }
                }
                else
                {
                    LogMessage($"❌ TTS Error: {www.error}");
                    LogMessage($"Response: {www.downloadHandler.text}");
                }
            }
        }

        private IEnumerator PlayAudioCoroutine(byte[] audioData, int sampleRate)
        {
            LogMessage("🔊 Playing audio...");

            // Convert byte array to float array for AudioClip
            float[] floatData = ConvertBytesToFloats(audioData);
            
            if (floatData == null || floatData.Length == 0)
            {
                LogMessage("❌ Invalid audio data");
                yield break;
            }
            
            // Create AudioClip
            AudioClip clip = AudioClip.Create("TTS_Audio", floatData.Length, 1, sampleRate, false);
            if (clip == null)
            {
                LogMessage("❌ Failed to create audio clip");
                yield break;
            }
            
            clip.SetData(floatData, 0);

            // Play audio
            audioSource.clip = clip;
            audioSource.Play();

            // Wait for playback to finish
            while (audioSource.isPlaying)
            {
                yield return null;
            }

            LogMessage("✅ Audio playback complete");
        }

        private float[] ConvertBytesToFloats(byte[] audioBytes)
        {
            float[] floatArray = new float[audioBytes.Length / 2];
            for (int i = 0; i < floatArray.Length; i++)
            {
                short value = BitConverter.ToInt16(audioBytes, i * 2);
                floatArray[i] = value / 32768f;
            }
            return floatArray;
        }

        private void SaveWavFile(string filepath, byte[] audioData, int sampleRate)
        {
            using (FileStream fs = new FileStream(filepath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // WAV header
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + audioData.Length);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(sampleRate);
                writer.Write(sampleRate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(audioData.Length);
                writer.Write(audioData);
            }
        }

        public void SetVoice(string voiceName)
        {
            currentVoiceName = voiceName;
            currentGender = voiceName.Contains("-A") || voiceName.Contains("-C") ? "FEMALE" : "MALE";
            LogMessage($"🗣️ Voice changed to: {voiceName} ({currentGender})");
        }

        public void SetSpeechParameters(double speakingRateParam = 1.0, double pitchParam = 0.0, double volumeGainDbParam = 0.0)
        {
            speakingRate = Math.Max(0.25, Math.Min(4.0, speakingRateParam));
            pitch = Math.Max(-20.0, Math.Min(20.0, pitchParam));
            volumeGainDb = Math.Max(-96.0, Math.Min(16.0, volumeGainDbParam));

            LogMessage($"🎚️ Speech parameters updated:");
            LogMessage($"   Speed: {speakingRate}x");
            LogMessage($"   Pitch: {pitch:+0.0;-0.0;0}");
            LogMessage($"   Volume: {volumeGainDb:+0.0;-0.0;0}dB");
        }

        private void LogMessage(string message)
        {
            Debug.Log(message);
            logCallback?.Invoke(message);
        }

        public void Dispose()
        {
            LogMessage("🔇 Text-to-Speech service disposed");
        }
    }
}
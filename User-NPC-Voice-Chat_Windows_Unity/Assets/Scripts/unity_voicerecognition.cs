using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

namespace LanguageVR.Pipeline.VoiceToText
{
    public class GoogleCloudSpeechService : MonoBehaviour
    {
        private AudioClip recordingClip;
        private bool isRecording = false;
        private const int SAMPLE_RATE = 16000;
        private const int MAX_RECORDING_LENGTH = 30; // seconds
        private System.Action<string> logCallback;
        private GoogleCloudAuth authService;
        
        // API Configuration
        private const string SPEECH_API_URL = "https://speech.googleapis.com/v1/speech:recognize";
        
        // For Oculus Quest 2 integration
        private bool useOculusInput = false;

        [System.Serializable]
        public class SpeechRequest
        {
            public Config config;
            public Audio audio;
        }

        [System.Serializable]
        public class Config
        {
            public string encoding = "LINEAR16";
            public int sampleRateHertz = 16000;
            public string languageCode = "es-ES";
            public bool enableAutomaticPunctuation = true;
            public string model = "latest_long";
            public bool useEnhanced = true;
            public string[] alternativeLanguageCodes = { "es-MX", "es-US" };
        }

        [System.Serializable]
        public class Audio
        {
            public string content; // Base64 encoded audio
        }

        [System.Serializable]
        public class SpeechResponse
        {
            public Result[] results;
        }

        [System.Serializable]
        public class Result
        {
            public Alternative[] alternatives;
        }

        [System.Serializable]
        public class Alternative
        {
            public string transcript;
            public float confidence;
        }
        
        void Awake()
        {
            // Check if running on Android (Quest 2)
            #if UNITY_ANDROID && !UNITY_EDITOR
            useOculusInput = true;
            #endif
            
            // Add authentication service
            authService = gameObject.AddComponent<GoogleCloudAuth>();
        }

        public void Initialize(System.Action<string> logger = null)
        {
            logCallback = logger;
            InitializeGoogleCloudSpeech();
        }

        private void InitializeGoogleCloudSpeech()
        {
            try
            {
                LogMessage("🔧 Initializing Google Cloud Speech...");

                // Initialize authentication service
                authService.Initialize(LogMessage);

                // Initialize microphone
                if (InitializeMicrophone())
                {
                    LogMessage("✅ Google Cloud Speech Service ready");
                    LogMessage("✅ Microphone ready for button-based recording");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Error initializing Speech Service: {ex.Message}");
            }
        }

        private bool InitializeMicrophone()
        {
            try
            {
                // Check microphone permission on Android
                #if UNITY_ANDROID && !UNITY_EDITOR
                if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
                {
                    UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
                    LogMessage("📱 Requesting microphone permission...");
                    return false;
                }
                #endif

                // Check available microphones
                if (Microphone.devices.Length > 0)
                {
                    LogMessage($"🎤 Found {Microphone.devices.Length} microphone(s)");
                    LogMessage($"🎤 Using: {Microphone.devices[0]}");
                    return true;
                }
                else
                {
                    LogMessage("⚠️ No microphone detected");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"⚠️ Microphone initialization warning: {ex.Message}");
                return false;
            }
        }

        // Called when button is pressed
        public void StartRecording()
        {
            if (isRecording)
            {
                LogMessage("⚠️ Already recording!");
                return;
            }

            try
            {
                // Start recording
                recordingClip = Microphone.Start(null, false, MAX_RECORDING_LENGTH, SAMPLE_RATE);
                isRecording = true;
                LogMessage("🎤 Recording... Speak now!");

                // Start coroutine to show recording progress
                StartCoroutine(ShowRecordingProgress());
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Failed to start recording: {ex.Message}");
                isRecording = false;
            }
        }

        private IEnumerator ShowRecordingProgress()
        {
            int seconds = 0;
            while (isRecording)
            {
                yield return new WaitForSeconds(1f);
                seconds++;
                if (isRecording)
                {
                    LogMessage($"🎤 Recording... {seconds}s");
                }
            }
        }

        // Called when button is released
        public byte[] StopRecording()
        {
            if (!isRecording)
            {
                LogMessage("⚠️ Not recording!");
                return null;
            }

            try
            {
                isRecording = false;
                
                // Get recording position
                int position = Microphone.GetPosition(null);
                Microphone.End(null);
                
                LogMessage("⏹️ Recording stopped");
                
                if (position <= 0)
                {
                    LogMessage("❌ No audio recorded");
                    return null;
                }

                // Extract audio data
                float[] samples = new float[position];
                recordingClip.GetData(samples, 0);
                
                // Convert to bytes
                byte[] audioData = ConvertFloatsToBytes(samples);
                
                LogMessage($"📊 Recorded {position / (float)SAMPLE_RATE:F1} seconds of audio");
                
                return audioData;
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Failed to stop recording: {ex.Message}");
                return null;
            }
        }

        private byte[] ConvertFloatsToBytes(float[] floatArray)
        {
            byte[] byteArray = new byte[floatArray.Length * 2];
            for (int i = 0; i < floatArray.Length; i++)
            {
                short value = (short)(floatArray[i] * 32767);
                BitConverter.GetBytes(value).CopyTo(byteArray, i * 2);
            }
            return byteArray;
        }

        public IEnumerator RecognizeFromAudioDataCoroutine(byte[] audioData, System.Action<string> callback)
        {
            if (authService == null || !authService.IsReady())
            {
                callback("ERROR: Authentication not ready - check setup instructions");
                yield break;
            }

            if (audioData == null || audioData.Length == 0)
            {
                callback("ERROR: No audio data");
                yield break;
            }

            LogMessage("📡 Sending to Google Cloud Speech...");

            // Get API key
            string apiKey = "";
            yield return StartCoroutine(authService.GetValidAPIKey((key) => apiKey = key));

            if (string.IsNullOrEmpty(apiKey))
            {
                callback("ERROR: Could not obtain API key");
                yield break;
            }

            // Convert audio to base64
            string base64Audio = Convert.ToBase64String(audioData);

            // Create request
            SpeechRequest request = new SpeechRequest
            {
                config = new Config 
                { 
                    encoding = "LINEAR16",
                    sampleRateHertz = SAMPLE_RATE,
                    languageCode = "es-ES",
                    enableAutomaticPunctuation = true,
                    model = "latest_long",
                    useEnhanced = true,
                    alternativeLanguageCodes = new string[] { "es-MX", "es-US" }
                },
                audio = new Audio { content = base64Audio }
            };

            string jsonRequest = JsonUtility.ToJson(request);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);

            // Create web request with API key
            using (UnityWebRequest www = new UnityWebRequest(SPEECH_API_URL + "?key=" + apiKey, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    LogMessage("📡 Received speech recognition response");
                    
                    SpeechResponse response = JsonUtility.FromJson<SpeechResponse>(www.downloadHandler.text);
                    
                    if (response.results != null && response.results.Length > 0 && 
                        response.results[0].alternatives != null && response.results[0].alternatives.Length > 0)
                    {
                        string transcript = response.results[0].alternatives[0].transcript;
                        float confidence = response.results[0].alternatives[0].confidence;
                        
                        LogMessage($"✅ Recognized with {confidence:P0} confidence");
                        LogMessage($"✅ Recognized: \"{transcript}\"");
                        callback(transcript);
                    }
                    else
                    {
                        LogMessage("😶 No speech detected");
                        callback("");
                    }
                }
                else
                {
                    LogMessage($"❌ Speech recognition error: {www.error}");
                    LogMessage($"Response: {www.downloadHandler.text}");
                    callback($"ERROR: {www.error}");
                }
            }
        }

        public bool IsListening => isRecording;

        private void LogMessage(string message)
        {
            Debug.Log(message);
            logCallback?.Invoke(message);
        }

        public void Dispose()
        {
            isRecording = false;
            if (Microphone.IsRecording(null))
            {
                Microphone.End(null);
            }
            LogMessage("🔇 Speech service disposed");
        }
    }
}
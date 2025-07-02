using System;
using System.Collections;
using UnityEngine;
using LanguageVR.Pipline.ChatIntegration;
using LanguageVR.Pipline.Controller;
using LanguageVR.Pipeline.VoiceToText;
using LanguageVR.Pipeline.TextToSpeech;

namespace LanguageVR.Pipline
{
    public class PipelineManager : MonoBehaviour
    {
        private GeminiService geminiService;
        private GoogleCloudSpeechService speechService;
        private GoogleCloudTextToSpeechService ttsService;
        private VRController vrController;
        private System.Action<string> logCallback;

        void Awake()
        {
            // Create child GameObjects for services
            GameObject speechGO = new GameObject("SpeechService");
            speechGO.transform.SetParent(transform);
            speechService = speechGO.AddComponent<GoogleCloudSpeechService>();

            GameObject ttsGO = new GameObject("TTSService");
            ttsGO.transform.SetParent(transform);
            ttsService = ttsGO.AddComponent<GoogleCloudTextToSpeechService>();

            GameObject geminiGO = new GameObject("GeminiService");
            geminiGO.transform.SetParent(transform);
            geminiService = geminiGO.AddComponent<GeminiService>();

            GameObject vrGO = new GameObject("VRController");
            vrGO.transform.SetParent(transform);
            vrController = vrGO.AddComponent<VRController>();
        }

        void Start()
        {
            StartCoroutine(InitializeServices());
        }

        IEnumerator InitializeServices()
        {
            LogMessage("🔧 Initializing Pipeline Manager...");
            
            // Initialize all services with logger
            speechService.Initialize(LogMessage);
            yield return new WaitForSeconds(0.5f);
            
            ttsService.Initialize(LogMessage);
            yield return new WaitForSeconds(0.5f);
            
            geminiService.Initialize(LogMessage);
            yield return new WaitForSeconds(0.5f);
            
            vrController.Initialize(this, LogMessage);
            
            LogMessage("✅ Pipeline Manager initialized");
            LogMessage("✅ All services ready for VR interaction");
        }

        public void SetUnityLogger(System.Action<string> logger)
        {
            logCallback = logger;
        }

        public IEnumerator PlayInitialGreetingCoroutine()
        {
            string greeting = "¡Hola! Bienvenido a nuestra tienda. ¿En qué puedo ayudarte hoy?";
            LogMessage($"\n🤖 NPC: {greeting}");
            yield return StartCoroutine(ttsService.SpeakCoroutine(greeting));
        }

        public IEnumerator StartVRModeCoroutine()
        {
            LogMessage("\n🎮 VR Mode Active");
            LogMessage("================================");
            
            #if UNITY_ANDROID && !UNITY_EDITOR
            LogMessage("📱 Press Quest 2 controller trigger to START/STOP recording");
            #else
            LogMessage("📱 Press SPACEBAR to START recording (simulates Quest 2 button)");
            LogMessage("📱 Press SPACEBAR again to STOP recording and process");
            #endif
            
            LogMessage("📱 Press ESC to exit\n");

            // VR Controller will handle the main loop
            yield return null;
        }

        public IEnumerator ProcessVoiceInputCoroutine(byte[] audioData, System.Action<string> callback)
        {
            string result = "";
            bool hasError = false;
            string errorMessage = "";
            
            LogMessage("\n🔄 Processing your voice...");

            // Step 1: Check audio quality
            if (audioData == null || audioData.Length < 8000)
            {
                errorMessage = "Audio muy corto. Por favor, mantén presionado el botón mientras hablas.";
                yield return StartCoroutine(ttsService.SpeakCoroutine(errorMessage));
                callback($"ERROR: {errorMessage}");
                yield break;
            }

            // Step 2: Convert speech to text
            string recognizedText = "";
            yield return StartCoroutine(speechService.RecognizeFromAudioDataCoroutine(audioData, 
                (text) => recognizedText = text));

            if (string.IsNullOrWhiteSpace(recognizedText) || recognizedText.StartsWith("ERROR:"))
            {
                LogMessage("❌ No se pudo reconocer el audio");
                errorMessage = "No pude entenderte bien. ¿Puedes repetir más claro por favor?";
                yield return StartCoroutine(ttsService.SpeakCoroutine(errorMessage));
                callback($"ERROR: {errorMessage}");
                yield break;
            }

            LogMessage($"📝 Recognized: '{recognizedText}'");

            // Step 3: Check if Spanish
            if (!IsSpanish(recognizedText))
            {
                LogMessage("⚠️ Not Spanish - requesting Spanish input");
                string spanishRequest = "Por favor, habla en español. Estoy aquí para ayudarte a practicar.";
                yield return StartCoroutine(ttsService.SpeakCoroutine(spanishRequest));
                callback(spanishRequest);
                yield break;
            }

            // Step 4: Get NPC response from Gemini
            LogMessage("🤖 Getting NPC response...");
            string npcResponse = "";
            yield return StartCoroutine(geminiService.GetNPCResponseCoroutine(recognizedText, "Spanish",
                (response) => npcResponse = response));

            // Step 5: Speak the response
            LogMessage("🗣️ Speaking NPC response...");
            yield return StartCoroutine(ttsService.SpeakCoroutine(npcResponse));

            // Step 6: Log the interaction
            LogInteraction(recognizedText, npcResponse);

            result = npcResponse;
            callback(result);
        }

        private bool IsSpanish(string text)
        {
            string lowerText = text.ToLower();
            // Check for Spanish characters and common words
            return text.Contains("¿") || text.Contains("ñ") || text.Contains("á") || text.Contains("é") ||
                   text.Contains("í") || text.Contains("ó") || text.Contains("ú") ||
                   lowerText.Contains("hola") || lowerText.Contains("dónde") || lowerText.Contains("cuánto") ||
                   lowerText.Contains("está") || lowerText.Contains("necesito") || lowerText.Contains("qué") ||
                   lowerText.Contains("cómo") || lowerText.Contains("gracias") || lowerText.Contains("por favor");
        }

        private void LogInteraction(string userInput, string npcResponse)
        {
            LogMessage("\n" + new string('=', 60));
            LogMessage("💬 INTERACTION:");
            LogMessage($"👤 You: {userInput}");
            LogMessage($"🤖 NPC: {npcResponse}");
            LogMessage(new string('=', 60) + "\n");
        }

        public GoogleCloudSpeechService GetSpeechService()
        {
            return speechService;
        }

        private void LogMessage(string message)
        {
            Debug.Log(message);
            logCallback?.Invoke(message);
        }

        void OnDestroy()
        {
            LogMessage("\nShutting down Pipeline Manager...");
            if (speechService != null) speechService.Dispose();
            if (geminiService != null) geminiService.Dispose();
            if (ttsService != null) ttsService.Dispose();
            if (vrController != null) vrController.Dispose();
            LogMessage("Pipeline Manager disposed");
        }
    }
}
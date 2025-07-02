using System;
using System.Threading.Tasks;
using LanguageVR.Pipline.ChatIntegration;
using LanguageVR.Pipline.Controller;
using LanguageVR.Pipeline.VoiceToText;
using LanguageVR.Pipeline.TextToSpeech;

namespace LanguageVR.Pipline
{
    public class PipelineManager
    {
        private readonly GeminiService geminiService;
        private readonly GoogleCloudSpeechService speechService;
        private readonly GoogleCloudTextToSpeechService ttsService;
        private VRController vrController;

        public PipelineManager()
        {
            // Initialize all services
            speechService = new GoogleCloudSpeechService();
            geminiService = new GeminiService();
            ttsService = new GoogleCloudTextToSpeechService();

            Console.WriteLine("✅ Pipeline Manager initialized");
            Console.WriteLine("✅ All services ready for VR interaction");
        }

        public async Task PlayInitialGreetingAsync()
        {
            string greeting = "¡Hola! Bienvenido a nuestra tienda. ¿En qué puedo ayudarte hoy?";
            Console.WriteLine($"\n🤖 NPC: {greeting}");
            await ttsService.SpeakAsync(greeting);
        }

        public async Task StartVRModeAsync()
        {
            Console.WriteLine("\n🎮 VR Mode Active");
            Console.WriteLine("================================");
            Console.WriteLine("📱 Press SPACEBAR to START recording (simulates Quest 2 button)");
            Console.WriteLine("📱 Press SPACEBAR again to STOP recording and process");
            Console.WriteLine("📱 Press ESC to exit\n");

            try
            {
                // Create VR Controller and start
                vrController = new VRController(this);
                await vrController.StartVRModeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ VR Mode error: {ex.Message}");
            }
        }

        public async Task<string> ProcessVoiceInputAsync(byte[] audioData)
        {
            try
            {
                Console.WriteLine("\n🔄 Processing your voice...");

                // Step 1: Check audio quality
                if (audioData == null || audioData.Length < 8000) // Less than 0.5 seconds at 16kHz
                {
                    string errorMsg = "Audio muy corto. Por favor, mantén presionado el botón mientras hablas.";
                    await ttsService.SpeakAsync(errorMsg);
                    return $"ERROR: {errorMsg}";
                }

                // Step 2: Convert speech to text
                string recognizedText = await speechService.RecognizeFromAudioDataAsync(audioData);

                if (string.IsNullOrWhiteSpace(recognizedText) || recognizedText.StartsWith("ERROR:"))
                {
                    Console.WriteLine("❌ No se pudo reconocer el audio");
                    string errorMsg = "No pude entenderte bien. ¿Puedes repetir más claro por favor?";
                    await ttsService.SpeakAsync(errorMsg);
                    return $"ERROR: {errorMsg}";
                }

                Console.WriteLine($"📝 Recognized: '{recognizedText}'");

                // Step 3: Check if Spanish
                if (!IsSpanish(recognizedText))
                {
                    Console.WriteLine("⚠️ Not Spanish - requesting Spanish input");
                    string spanishRequest = "Por favor, habla en español. Estoy aquí para ayudarte a practicar.";
                    await ttsService.SpeakAsync(spanishRequest);
                    return spanishRequest;
                }

                // Step 4: Get NPC response from Gemini
                Console.WriteLine("🤖 Getting NPC response...");
                string npcResponse = await geminiService.GetNPCResponseAsync(recognizedText, "Spanish");

                // Step 5: Speak the response
                Console.WriteLine("🗣️ Speaking NPC response...");
                await ttsService.SpeakAsync(npcResponse);

                // Step 6: Log the interaction
                LogInteraction(recognizedText, npcResponse);

                return npcResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Processing error: {ex.Message}");
                string errorResponse = "Lo siento, hubo un problema técnico. Intenta de nuevo.";
                await ttsService.SpeakAsync(errorResponse);
                return errorResponse;
            }
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
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("💬 INTERACTION:");
            Console.WriteLine($"👤 You: {userInput}");
            Console.WriteLine($"🤖 NPC: {npcResponse}");
            Console.WriteLine(new string('=', 60) + "\n");
        }

        public GoogleCloudSpeechService GetSpeechService()
        {
            return speechService;
        }

        public void Dispose()
        {
            Console.WriteLine("\nShutting down Pipeline Manager...");
            speechService?.Dispose();
            geminiService?.Dispose();
            ttsService?.Dispose();
            vrController?.Dispose();
            Console.WriteLine("Pipeline Manager disposed");
        }
    }
}
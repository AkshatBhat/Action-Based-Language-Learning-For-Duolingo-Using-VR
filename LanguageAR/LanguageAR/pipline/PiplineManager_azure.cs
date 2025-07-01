//using System;
//using System.Threading.Tasks;
//using LanguageVR.Pipline.ChatIntegration;
//using LanguageVR.Pipline.Controller;
//using LanguageVR.Pipeline.VoiceToText;

//namespace LanguageVR.Pipline
//{
//    public class PipelineManager
//    {
//        private readonly GeminiService geminiService;
//        private readonly AzureSpeechService speechService;

//        public PipelineManager()
//        {
//            // Initialize all services
//            speechService = new AzureSpeechService();
//            geminiService = new GeminiService();

//            Console.WriteLine("Pipeline Manager initialized");
//            Console.WriteLine("All services coordinated and ready");
//        }

//        public async Task StartVRModeAsync()
//        {
//            Console.WriteLine("\nStarting VR Pipeline Mode...");
//            Console.WriteLine("================================");
//            Console.WriteLine("This coordinates:");
//            Console.WriteLine("   1. VR Controller (button activation)");
//            Console.WriteLine("   2. Speech Recognition (Azure)");
//            Console.WriteLine("   3. Chat Integration (Gemini)");
//            Console.WriteLine("   4. Response Generation");

//            try
//            {
//                // Create VR Controller and pass this pipeline manager to it
//                var vrController = new VRController(this);
//                await vrController.StartVRModeAsync();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Pipeline error: {ex.Message}");
//            }
//        }

//        public async Task<string> ProcessVoiceInputAsync()
//        {
//            try
//            {
//                Console.WriteLine("Pipeline processing voice input...");

//                // Step 1: Get REAL speech input from Azure Speech Service
//                Console.WriteLine("Listening for speech...");
//                string recognizedText = await speechService.RecognizeOnceAsync();

//                if (recognizedText.StartsWith("ERROR:"))
//                {
//                    Console.WriteLine($"Speech recognition failed: {recognizedText}");
//                    return recognizedText;
//                }

//                // Step 2: Detect language from the actual transcribed text
//                string detectedLanguage = DetectLanguageFromText(recognizedText);
//                Console.WriteLine($"Text: '{recognizedText}'");
//                Console.WriteLine($"Language: {detectedLanguage}");

//                // Step 3: Validate input
//                if (string.IsNullOrWhiteSpace(recognizedText))
//                {
//                    return "ERROR: No text provided";
//                }

//                // Step 4: Check if Spanish (as per requirements)
//                if (!IsSpanish(detectedLanguage))
//                {
//                    Console.WriteLine("Only processing Spanish input");
//                    return "Por favor, habla en español.";
//                }

//                // Step 5: Send REAL transcribed text to Gemini for NPC response
//                Console.WriteLine("Sending to Gemini for NPC response...");
//                string npcResponse = await geminiService.GetNPCResponseAsync(recognizedText, detectedLanguage);

//                // Step 6: Log the complete interaction
//                LogInteraction(recognizedText, npcResponse);

//                return npcResponse;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Pipeline processing error: {ex.Message}");
//                return "Lo siento, hubo un problema técnico.";
//            }
//        }

//        private string DetectLanguageFromText(string text)
//        {
//            // Simple Spanish detection based on common Spanish words/characters
//            if (text.Contains("¿") || text.Contains("ñ") ||
//                text.ToLower().Contains("hola") ||
//                text.ToLower().Contains("dónde") ||
//                text.ToLower().Contains("cuánto") ||
//                text.ToLower().Contains("está") ||
//                text.ToLower().Contains("necesito") ||
//                text.ToLower().Contains("disculpe"))
//            {
//                return "Spanish";
//            }
//            return "English";
//        }

//        private bool IsSpanish(string detectedLanguage)
//        {
//            return detectedLanguage.ToLower().Contains("spanish") ||
//                   detectedLanguage.Contains("es-") ||
//                   detectedLanguage.ToLower().Contains("español");
//        }

//        private void LogInteraction(string userInput, string npcResponse)
//        {
//            Console.WriteLine("\n" + new string('=', 60));
//            Console.WriteLine("COMPLETE INTERACTION LOG:");
//            Console.WriteLine($"User: {userInput}");
//            Console.WriteLine($"NPC:  {npcResponse}");
//            Console.WriteLine(new string('=', 60) + "\n");
//        }

//        public void ShowSystemStatus()
//        {
//            Console.WriteLine("\nSystem Status:");
//            Console.WriteLine("Pipeline Manager: Active");
//            Console.WriteLine("Gemini Service: Ready");
//            Console.WriteLine("VR Controller: Ready");
//            Console.WriteLine("Azure Speech Service: Ready");
//            Console.WriteLine("Text-to-Speech: [Future feature]");
//        }

//        public void ShowAvailableCommands()
//        {
//            Console.WriteLine("\nAvailable Commands:");
//            Console.WriteLine("SPACEBAR - Activate voice recognition");
//            Console.WriteLine("Q - Quit current mode");
//            Console.WriteLine("\nTry these Spanish phrases:");
//            Console.WriteLine("• Hola, ¿dónde están las manzanas?");
//            Console.WriteLine("• ¿Cuánto cuesta la leche?");
//            Console.WriteLine("• Disculpe, ¿dónde está la caja?");
//            Console.WriteLine("• Necesito ayuda, por favor");
//        }

//        public async Task TestPipelineAsync()
//        {
//            Console.WriteLine("\nTesting Pipeline Components...");

//            // Test speech service
//            Console.WriteLine("Testing Speech service...");
//            speechService.TestMicrophone();

//            // Test Gemini integration
//            Console.WriteLine("Testing Gemini service...");
//            string testResponse = await geminiService.GetNPCResponseAsync(
//                "Hola, ¿dónde están las manzanas?",
//                "Spanish"
//            );
//            Console.WriteLine($"Gemini test result: '{testResponse}'");

//            Console.WriteLine("\nPipeline test completed!");
//        }

//        public void Dispose()
//        {
//            Console.WriteLine("Shutting down Pipeline Manager...");
//            speechService?.Dispose();
//            geminiService?.Dispose();
//            Console.WriteLine("Pipeline Manager disposed");
//        }
//    }
//}
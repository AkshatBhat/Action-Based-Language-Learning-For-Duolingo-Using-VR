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

        public PipelineManager()
        {
            // Initialize all services
            speechService = new GoogleCloudSpeechService();
            geminiService = new GeminiService();
            ttsService = new GoogleCloudTextToSpeechService();

            Console.WriteLine("✅ Pipeline Manager initialized");
            Console.WriteLine("✅ All services coordinated and ready");
            Console.WriteLine("   • Speech Recognition (Google Cloud)");
            Console.WriteLine("   • Chat AI (Gemini)");
            Console.WriteLine("   • Text-to-Speech (Google Cloud)");
        }

        public async Task StartVRModeAsync()
        {
            Console.WriteLine("\n🎮 Starting VR Pipeline Mode...");
            Console.WriteLine("================================");
            Console.WriteLine("This coordinates:");
            Console.WriteLine("   1. VR Controller (button activation)");
            Console.WriteLine("   2. Speech Recognition (Google Cloud)");
            Console.WriteLine("   3. Chat Integration (Gemini)");
            Console.WriteLine("   4. Text-to-Speech (Google Cloud)");
            Console.WriteLine("   5. NPC Voice Response");

            try
            {
                // Create VR Controller and pass this pipeline manager to it
                var vrController = new VRController(this);
                await vrController.StartVRModeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Pipeline error: {ex.Message}");
            }
        }

        public async Task<string> ProcessVoiceInputAsync()
        {
            try
            {
                Console.WriteLine("\n🔄 Pipeline processing voice input...");

                // Step 1: Get REAL speech input from Google Cloud Speech Service
                Console.WriteLine("🎤 Using Google Cloud Speech for recognition...");
                string recognizedText = await speechService.RecognizeOnceAsync();

                if (recognizedText.StartsWith("ERROR:"))
                {
                    Console.WriteLine($"❌ Speech recognition failed: {recognizedText}");
                    return recognizedText;
                }

                // Step 2: Detect language from the actual transcribed text
                string detectedLanguage = DetectLanguageFromText(recognizedText);
                Console.WriteLine($"📝 Text: '{recognizedText}'");
                Console.WriteLine($"🌐 Language: {detectedLanguage}");

                // Step 3: Validate input
                if (string.IsNullOrWhiteSpace(recognizedText))
                {
                    return "ERROR: No text provided";
                }

                // Step 4: Check if Spanish (as per requirements)
                if (!IsSpanish(detectedLanguage))
                {
                    Console.WriteLine("⚠️ Only processing Spanish input");
                    string spanishWarning = "Por favor, habla en español.";

                    // Speak the warning
                    await ttsService.SpeakAsync(spanishWarning);

                    return spanishWarning;
                }

                // Step 5: Send REAL transcribed text to Gemini for NPC response
                Console.WriteLine("🤖 Sending to Gemini for NPC response...");
                string npcResponse = await geminiService.GetNPCResponseAsync(recognizedText, detectedLanguage);

                // Step 6: Convert NPC response to speech
                Console.WriteLine("🗣️ Converting NPC response to speech...");
                bool speechSuccess = await ttsService.SpeakAsync(npcResponse);

                if (!speechSuccess)
                {
                    Console.WriteLine("⚠️ Could not play audio, but text response is available");
                }

                // Step 7: Log the complete interaction
                LogInteraction(recognizedText, npcResponse);

                return npcResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Pipeline processing error: {ex.Message}");
                string errorResponse = "Lo siento, hubo un problema técnico.";

                // Try to speak the error message
                await ttsService.SpeakAsync(errorResponse);

                return errorResponse;
            }
        }

        private string DetectLanguageFromText(string text)
        {
            // Simple Spanish detection based on common Spanish words/characters
            if (text.Contains("¿") || text.Contains("ñ") ||
                text.ToLower().Contains("hola") ||
                text.ToLower().Contains("dónde") ||
                text.ToLower().Contains("cuánto") ||
                text.ToLower().Contains("está") ||
                text.ToLower().Contains("necesito") ||
                text.ToLower().Contains("disculpe"))
            {
                return "Spanish";
            }
            return "English";
        }

        private bool IsSpanish(string detectedLanguage)
        {
            return detectedLanguage.ToLower().Contains("spanish") ||
                   detectedLanguage.Contains("es-") ||
                   detectedLanguage.ToLower().Contains("español");
        }

        private void LogInteraction(string userInput, string npcResponse)
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("💬 COMPLETE INTERACTION LOG:");
            Console.WriteLine($"👤 User: {userInput}");
            Console.WriteLine($"🤖 NPC:  {npcResponse}");
            Console.WriteLine(new string('=', 60) + "\n");
        }

        public void ShowSystemStatus()
        {
            Console.WriteLine("\n📊 System Status:");
            Console.WriteLine(" Pipeline Manager: Active");
            Console.WriteLine(" Gemini Service: Ready");
            Console.WriteLine(" VR Controller: Ready");
            Console.WriteLine(" Google Cloud Speech Service: Ready");
            Console.WriteLine(" Google Cloud Text-to-Speech: Ready");
        }

        public void ShowAvailableCommands()
        {
            Console.WriteLine("\n⌨️ Available Commands:");
            Console.WriteLine("SPACEBAR - Activate voice recognition");
            Console.WriteLine("Q - Quit current mode");
            Console.WriteLine("\n🇪🇸 Try these Spanish phrases:");
            Console.WriteLine("• Hola, ¿dónde están las manzanas?");
            Console.WriteLine("• ¿Cuánto cuesta la leche?");
            Console.WriteLine("• Disculpe, ¿dónde está la caja?");
            Console.WriteLine("• Necesito ayuda, por favor");
        }

        public async Task TestPipelineAsync()
        {
            Console.WriteLine("\n🧪 Testing Pipeline Components...");

            // Test speech service
            Console.WriteLine("\n1️⃣ Testing Speech service...");
            speechService.TestMicrophone();

            // Test Gemini integration
            Console.WriteLine("\n2 Testing Gemini service...");
            string testResponse = await geminiService.GetNPCResponseAsync(
                "Hola, ¿dónde están las manzanas?",
                "Spanish"
            );
            Console.WriteLine($" Gemini test result: '{testResponse}'");

            // Test Text-to-Speech
            Console.WriteLine("\n3 Testing Text-to-Speech service...");
            await ttsService.TestVoicesAsync();

            Console.WriteLine("\n Pipeline test completed!");
        }

        public async Task ConfigureNPCVoiceAsync()
        {
            Console.WriteLine("\n🎭 NPC Voice Configuration");
            ttsService.ShowAvailableVoices();

            Console.WriteLine("\nSelect voice (1-6) or press Enter for default:");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": ttsService.SetVoice("es-ES-Standard-A"); break;
                case "2": ttsService.SetVoice("es-ES-Standard-B"); break;
                case "3": ttsService.SetVoice("es-ES-Wavenet-C"); break;
                case "4": ttsService.SetVoice("es-ES-Wavenet-B"); break;
                case "5": ttsService.SetVoice("es-ES-Neural2-A"); break;
                case "6": ttsService.SetVoice("es-ES-Neural2-B"); break;
            }

            Console.WriteLine("\nAdjust speech parameters? (y/n):");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                Console.Write("Speaking rate (0.25-4.0, default 1.0): ");
                if (double.TryParse(Console.ReadLine(), out double rate))
                {
                    Console.Write("Pitch (-20 to +20, default 0): ");
                    if (double.TryParse(Console.ReadLine(), out double pitch))
                    {
                        Console.Write("Volume gain (-96 to +16 dB, default 0): ");
                        if (double.TryParse(Console.ReadLine(), out double volume))
                        {
                            ttsService.SetSpeechParameters(rate, pitch, volume);
                        }
                    }
                }
            }

            // Test the configured voice
            Console.WriteLine("\nTesting configured voice...");
            await ttsService.SpeakAsync("Hola, bienvenido a nuestra tienda. ¿En qué puedo ayudarte?");
        }

        public void Dispose()
        {
            Console.WriteLine("Shutting down Pipeline Manager...");
            speechService?.Dispose();
            geminiService?.Dispose();
            ttsService?.Dispose();
            Console.WriteLine("Pipeline Manager disposed");
        }
    }
}
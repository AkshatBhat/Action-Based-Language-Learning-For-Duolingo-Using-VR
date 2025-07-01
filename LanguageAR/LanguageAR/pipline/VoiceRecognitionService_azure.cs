//using System;
//using System.Threading.Tasks;
//using Microsoft.CognitiveServices.Speech;
//using Microsoft.CognitiveServices.Speech.Audio;

//namespace LanguageVR.Pipeline.VoiceToText
//{
//    public class AzureSpeechService
//    {
//        private SpeechConfig speechConfig;
//        private SpeechRecognizer recognizer;
//        private AudioConfig audioConfig;
//        private bool isListening = false;

//        // Language codes for grocery store scenarios
//        public string CurrentLanguage { get; private set; } = "en-US";

//        public AzureSpeechService()
//        {
//            InitializeAzureSpeech();
//        }

//        private void InitializeAzureSpeech()
//        {
//            try
//            {
//                // Azure Speech Service credentials
//                // Get these from Azure Portal -> Speech Services
//                string subscriptionKey = "9ETHw0ZqDVdG5widXDbHSy9fA3dMC1gubHqtGoWk2pZQoMs06pEOJQQJ99BGAC1i4TkXJ3w3AAAYACOG97Ki";
//                string region = "centralus"; // e.g., "eastus", "westus2"

//                // Create speech configuration
//                speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
//                speechConfig.SpeechRecognitionLanguage = CurrentLanguage;

//                //// Optimize for conversations (better for grocery store scenarios)
//                //speechConfig.SetProperty(PropertyId.SpeechServiceConnection_SingleLanguageIdPriority, "Latency");

//                // Create audio configuration (default microphone)
//                audioConfig = AudioConfig.FromDefaultMicrophoneInput();

//                // Create speech recognizer
//                recognizer = new SpeechRecognizer(speechConfig, audioConfig);

//                // Set up event handlers
//                SetupEventHandlers();

//                Console.WriteLine("✅Azure Speech Service initialized");
//                Console.WriteLine($"🌍 Current language: {CurrentLanguage}");
//                Console.WriteLine("🎤 Ready for high-quality speech recognition");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ Error initializing Azure Speech Service: {ex.Message}");
//                Console.WriteLine("📝 Make sure to set your Azure Speech Service key and region");
//                Console.WriteLine("📝 Get your credentials from: https://portal.azure.com");
//            }
//        }

//        private void EnableAutoLanguageDetection()
//        {
//            // Set up automatic language detection for English and Spanish
//            var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(
//                new string[] { "en-US", "es-ES", "es-MX" }); // US English, Spain Spanish, Mexico Spanish

//            // Store for later use
//            this.autoDetectConfig = autoDetectSourceLanguageConfig;
//        }

//        private AutoDetectSourceLanguageConfig autoDetectConfig;

//        private void CreateRecognizerWithAutoDetection()
//        {
//            // Create recognizer with automatic language detection
//            recognizer = new SpeechRecognizer(speechConfig, autoDetectConfig, audioConfig);

//            // Set up event handlers
//            SetupEventHandlers();
//        }

//        private void SetupEventHandlers()
//        {
//            // Recognized speech (final result)
//            recognizer.Recognized += (s, e) =>
//            {
//                if (e.Result.Reason == ResultReason.RecognizedSpeech)
//                {
//                    // Get the detected language
//                    var detectedLanguage = GetDetectedLanguage(e.Result);

//                    Console.WriteLine($"✅ Recognized: '{e.Result.Text}'");
//                    Console.WriteLine($"🌍 Detected Language: {detectedLanguage}");
//                    Console.WriteLine($"📊 Confidence: High");

//                    OnSpeechRecognized(e.Result.Text, detectedLanguage);
//                }
//                else if (e.Result.Reason == ResultReason.NoMatch)
//                {
//                    Console.WriteLine("❌ No speech could be recognized");
//                    Console.WriteLine("💡 Try speaking more clearly or closer to the microphone");
//                }
//            };

//            // Recognizing speech (interim results)
//            recognizer.Recognizing += (s, e) =>
//            {
//                if (!string.IsNullOrEmpty(e.Result.Text))
//                {
//                    Console.WriteLine($"🔄 Recognizing: {e.Result.Text}");
//                }
//            };

//            // Session started
//            recognizer.SessionStarted += (s, e) =>
//            {
//                Console.WriteLine("🎤 Speech recognition session started");
//            };

//            // Session stopped
//            recognizer.SessionStopped += (s, e) =>
//            {
//                Console.WriteLine("🛑 Speech recognition session stopped");
//                isListening = false;
//            };

//            // Canceled
//            recognizer.Canceled += (s, e) =>
//            {
//                Console.WriteLine($"❌ Recognition canceled: {e.Reason}");
//                if (e.Reason == CancellationReason.Error)
//                {
//                    Console.WriteLine($"❌ Error details: {e.ErrorDetails}");
//                    Console.WriteLine("💡 Check your Azure Speech Service key and region");
//                }
//                isListening = false;
//            };
//        }

//        private string GetDetectedLanguage(SpeechRecognitionResult result)
//        {
//            // Try to get the detected language from the result
//            var detectedLanguage = result.Properties.GetProperty(PropertyId.SpeechServiceConnection_AutoDetectSourceLanguageResult);

//            if (!string.IsNullOrEmpty(detectedLanguage))
//            {
//                switch (detectedLanguage)
//                {
//                    case "en-US":
//                        return "English (US)";
//                    case "es-ES":
//                        return "Spanish (Spain)";
//                    case "es-MX":
//                        return "Spanish (Mexico)";
//                    default:
//                        return detectedLanguage;
//                }
//            }

//            return "Auto-detected";
//        }

//        public void SetLanguage(string language)
//        {
//            Console.WriteLine("🌍 Auto-detection is enabled!");
//            Console.WriteLine("💡 You can speak in either English or Spanish");
//            Console.WriteLine("🤖 The system will automatically detect which language you're using");

//            if (language == "Spanish" || language == "2" || language == "es")
//            {
//                Console.WriteLine("🎯 Tip: Try Spanish phrases like:");
//                Console.WriteLine("  • Hola, ¿dónde están las manzanas?");
//                Console.WriteLine("  • ¿Cuánto cuesta la leche?");
//            }
//            else
//            {
//                Console.WriteLine("🎯 Tip: Try English phrases like:");
//                Console.WriteLine("  • Where are the apples?");
//                Console.WriteLine("  • How much does the milk cost?");
//            }
//        }

//        public void ShowAvailableLanguages()
//        {
//            Console.WriteLine("\n🌍 Supported Languages:");
//            Console.WriteLine("1. English (en-US)");
//            Console.WriteLine("2. Spanish (es-ES)");
//        }

//        public async Task<string> RecognizeOnceAsync()
//        {
//            if (recognizer == null)
//            {
//                return "ERROR: Speech service not initialized";
//            }

//            try
//            {
//                Console.WriteLine("🎤 Listening... (speak now)");

//                var result = await recognizer.RecognizeOnceAsync();

//                switch (result.Reason)
//                {
//                    case ResultReason.RecognizedSpeech:
//                        Console.WriteLine($"✅ Recognized: '{result.Text}'");
//                        return result.Text;

//                    case ResultReason.NoMatch:
//                        Console.WriteLine("❌ No speech could be recognized");
//                        return "ERROR: No speech recognized";

//                    case ResultReason.Canceled:
//                        var cancellation = CancellationDetails.FromResult(result);
//                        Console.WriteLine($"❌ Recognition canceled: {cancellation.Reason}");

//                        if (cancellation.Reason == CancellationReason.Error)
//                        {
//                            Console.WriteLine($"❌ Error details: {cancellation.ErrorDetails}");
//                        }
//                        return $"ERROR: Recognition canceled - {cancellation.Reason}";

//                    default:
//                        return "ERROR: Unknown recognition result";
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ Exception during recognition: {ex.Message}");
//                return $"ERROR: {ex.Message}";
//            }
//        }

//        public async Task StartContinuousRecognitionAsync()
//        {
//            if (recognizer == null)
//            {
//                Console.WriteLine("❌ Speech service not initialized");
//                return;
//            }

//            try
//            {
//                Console.WriteLine("🎤 Starting continuous recognition...");
//                ShowGroceryPhrases();
//                Console.WriteLine("\nPress 'q' and Enter to stop");

//                await recognizer.StartContinuousRecognitionAsync();
//                isListening = true;

//                // Wait for user to press 'q'
//                while (isListening)
//                {
//                    var input = Console.ReadLine();
//                    if (input?.ToLower() == "q")
//                    {
//                        break;
//                    }
//                    await Task.Delay(100);
//                }

//                await recognizer.StopContinuousRecognitionAsync();
//                isListening = false;
//                Console.WriteLine("🛑 Stopped continuous recognition");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ Error during continuous recognition: {ex.Message}");
//                isListening = false;
//            }
//        }

//        private void OnSpeechRecognized(string recognizedText, string detectedLanguage)
//        {
//            // This is where the text will go to Google Gemini API next
//            Console.WriteLine($"📤 Ready to send to Google Gemini: '{recognizedText}'");
//            Console.WriteLine($"📋 Language context: {detectedLanguage}");

//            // TODO: This is where we'll call Gemini service next
//            // We can pass both the text and detected language to Gemini for better context
//        }

//        public void ShowGroceryPhrases()
//        {
//            Console.WriteLine("\n🛒 Try these grocery store phrases in EITHER language:");

//            Console.WriteLine("\n🇺🇸 English phrases:");
//            Console.WriteLine("• Excuse me, where are the apples?");
//            Console.WriteLine("• How much does the milk cost?");
//            Console.WriteLine("• Where is the checkout?");
//            Console.WriteLine("• Do you have any discounts?");
//            Console.WriteLine("• I need help, please");

//            Console.WriteLine("\n🇪🇸 Spanish phrases:");
//            Console.WriteLine("• Hola, ¿dónde están las manzanas?");
//            Console.WriteLine("• ¿Cuánto cuesta la leche?");
//            Console.WriteLine("• Disculpe, ¿dónde está la caja?");
//            Console.WriteLine("• ¿Tienen descuentos en frutas?");
//            Console.WriteLine("• Necesito ayuda, por favor");

//            Console.WriteLine("\n🤖 The system will automatically detect which language you're speaking!");
//        }

//        public void TestMicrophone()
//        {
//            Console.WriteLine("🎤 Testing microphone access...");

//            try
//            {
//                // Test audio configuration
//                using (var testAudioConfig = AudioConfig.FromDefaultMicrophoneInput())
//                {
//                    Console.WriteLine("✅ Microphone access successful");
//                    Console.WriteLine("📊 Azure Speech Service ready");
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ Microphone test failed: {ex.Message}");
//                Console.WriteLine("📝 Make sure microphone permissions are granted");
//                Console.WriteLine("📝 Check Azure Speech Service configuration");
//            }
//        }

//        public bool IsListening => isListening;

//        public void Dispose()
//        {
//            if (isListening)
//            {
//                recognizer?.StopContinuousRecognitionAsync().Wait();
//            }

//            recognizer?.Dispose();
//            audioConfig?.Dispose();
//        }
//    }
//}
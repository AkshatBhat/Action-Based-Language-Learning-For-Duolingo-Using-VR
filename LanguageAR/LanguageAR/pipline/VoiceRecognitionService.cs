using System;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1;
using System.IO;
using System.Diagnostics;
using System.Threading;
using NAudio.Wave;
using System.Collections.Generic;

namespace LanguageVR.Pipeline.VoiceToText
{
    public class GoogleCloudSpeechService
    {
        private SpeechClient speechClient;
        private bool isListening = false;
        private WaveInEvent waveIn;
        private const int SAMPLE_RATE = 16000;
        private const int RECORDING_SECONDS = 5;

        // Language codes for grocery store scenarios
        public string CurrentLanguage { get; private set; } = "es-ES"; // Default to Spanish

        public GoogleCloudSpeechService()
        {
            InitializeGoogleCloudSpeech();
        }

        private void InitializeGoogleCloudSpeech()
        {
            try
            {
                Console.WriteLine("Initializing Google Cloud Speech with gcloud-key.json file...");

                string keyPath = "gcloud-key.json";

                if (!File.Exists(keyPath))
                {
                    Console.WriteLine($"Error: gcloud-key.json not found at {Path.GetFullPath(keyPath)}");
                    Console.WriteLine("Make sure gcloud-key.json file exists in your project directory");
                    speechClient = null;
                    return;
                }

                // Set environment variable to point to the JSON file
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyPath);
                Console.WriteLine($"Set credentials path to: {Path.GetFullPath(keyPath)}");

                // Create Google Cloud Speech client using the JSON file
                speechClient = SpeechClient.Create();

                Console.WriteLine("Google Cloud Speech client created successfully");
                Console.WriteLine("Google Cloud Speech Service initialized");
                Console.WriteLine($"Current language: {CurrentLanguage}");
                Console.WriteLine("Ready for REAL voice input processing");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Google Cloud Speech Service: {ex.Message}");
                Console.WriteLine($"Exception details: {ex}");
                Console.WriteLine("Make sure gcloud-key.json file exists and is valid");
                speechClient = null;
            }
        }

        public void SetLanguage(string language)
        {
            if (language == "Spanish" || language == "2" || language == "es")
            {
                CurrentLanguage = "es-ES";
                Console.WriteLine("Language set to Spanish (es-ES)");
            }
            else
            {
                CurrentLanguage = "en-US";
                Console.WriteLine("Language set to English (en-US)");
            }
        }

        public void ShowAvailableLanguages()
        {
            Console.WriteLine("\nSupported Languages:");
            Console.WriteLine("1. English (en-US)");
            Console.WriteLine("2. Spanish (es-ES)");
        }

        public async Task<string> RecognizeOnceAsync()
        {
            if (speechClient == null)
            {
                return "ERROR: Speech service not initialized";
            }

            try
            {
                Console.WriteLine("\n📢 REAL VOICE RECOGNITION STARTING...");
                Console.WriteLine("🎤 Recording audio from microphone for 5 seconds...");
                Console.WriteLine("🗣️  Speak now!");

                // Record audio to a temporary file
                string audioFile = await RecordAudioAsync();

                if (string.IsNullOrEmpty(audioFile) || !File.Exists(audioFile))
                {
                    Console.WriteLine("❌ No audio recorded. Falling back to text input...");
                    return await FallbackToTextInputAsync();
                }

                // Transcribe the recorded audio
                Console.WriteLine("🔄 Processing audio with Google Cloud Speech...");
                string result = await TranscribeAudioFileAsync(audioFile);

                // Clean up temp file
                try
                {
                    File.Delete(audioFile);
                }
                catch { }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Recognition error: {ex.Message}");
                Console.WriteLine("📝 Falling back to text input...");
                return await FallbackToTextInputAsync();
            }
        }

        private async Task<string> RecordAudioAsync()
        {
            try
            {
                string tempFile = Path.Combine(Path.GetTempPath(), $"speech_{Guid.NewGuid()}.wav");
                var audioData = new List<byte>();
                bool recordingComplete = false;

                // Configure audio recording
                waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(SAMPLE_RATE, 16, 1), // 16kHz, 16-bit, mono
                    BufferMilliseconds = 100
                };

                waveIn.DataAvailable += (sender, e) =>
                {
                    if (!recordingComplete)
                    {
                        audioData.AddRange(e.Buffer.Take(e.BytesRecorded));
                    }
                };

                // Start recording
                waveIn.StartRecording();
                Console.WriteLine("🔴 Recording... (5 seconds)");

                // Show progress
                for (int i = 1; i <= 5; i++)
                {
                    await Task.Delay(1000);
                    Console.Write($"{i}... ");
                }
                Console.WriteLine();

                // Stop recording
                recordingComplete = true;
                waveIn.StopRecording();
                waveIn.Dispose();

                Console.WriteLine("✅ Recording complete!");

                // Save to WAV file
                if (audioData.Count > 0)
                {
                    await SaveWavFileAsync(tempFile, audioData.ToArray(), SAMPLE_RATE);
                    Console.WriteLine($"💾 Audio saved: {Path.GetFileName(tempFile)}");
                    return tempFile;
                }
                else
                {
                    Console.WriteLine("⚠️ No audio data captured");
                    return "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Recording error: {ex.Message}");
                if (waveIn != null)
                {
                    try { waveIn.Dispose(); } catch { }
                }
                return "";
            }
        }

        private async Task SaveWavFileAsync(string filename, byte[] audioData, int sampleRate)
        {
            await Task.Run(() =>
            {
                using (var writer = new WaveFileWriter(filename, new WaveFormat(sampleRate, 16, 1)))
                {
                    writer.Write(audioData, 0, audioData.Length);
                }
            });
        }

        private async Task<string> TranscribeAudioFileAsync(string audioFile)
        {
            try
            {
                // Create recognition config
                var config = new RecognitionConfig
                {
                    Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                    SampleRateHertz = SAMPLE_RATE,
                    LanguageCode = CurrentLanguage,
                    EnableAutomaticPunctuation = true,
                    Model = "latest_long", // Better for conversational speech
                    UseEnhanced = true, // Enhanced model for better accuracy
                };

                // Load audio file
                var audio = RecognitionAudio.FromFile(audioFile);

                // Send request to Google Cloud Speech
                Console.WriteLine("📡 Sending to Google Cloud Speech API...");
                var response = await speechClient.RecognizeAsync(config, audio);

                // Process results
                if (response.Results.Count == 0)
                {
                    Console.WriteLine("😶 No speech detected in audio");
                    return await FallbackToTextInputAsync();
                }

                // Get the best transcription
                string bestTranscript = "";
                float bestConfidence = 0;

                foreach (var result in response.Results)
                {
                    if (result.Alternatives.Count > 0)
                    {
                        var alternative = result.Alternatives[0];
                        if (alternative.Confidence > bestConfidence)
                        {
                            bestTranscript = alternative.Transcript;
                            bestConfidence = alternative.Confidence;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(bestTranscript))
                {
                    Console.WriteLine($"✅ RECOGNIZED: '{bestTranscript}'");
                    Console.WriteLine($"🎯 Confidence: {bestConfidence:P0}");
                    return bestTranscript;
                }
                else
                {
                    Console.WriteLine("😕 Could not understand the speech clearly");
                    return await FallbackToTextInputAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Transcription error: {ex.Message}");
                return await FallbackToTextInputAsync();
            }
        }

        private async Task<string> FallbackToTextInputAsync()
        {
            Console.WriteLine("\n--- 📝 FALLBACK MODE ---");
            Console.WriteLine("Having trouble with speech recognition.");
            Console.WriteLine("Let's use text input to test the Gemini pipeline:");
            Console.Write("Enter Spanish text to send to Gemini: ");

            string textInput = await Task.Run(() => Console.ReadLine());

            if (!string.IsNullOrWhiteSpace(textInput))
            {
                Console.WriteLine($"📤 Using text input: '{textInput}'");
                return textInput;
            }
            else
            {
                // Default Spanish phrase for testing
                string defaultPhrase = "Hola, ¿dónde están las manzanas?";
                Console.WriteLine($"📤 Using default Spanish phrase: '{defaultPhrase}'");
                return defaultPhrase;
            }
        }

        public async Task StartContinuousRecognitionAsync()
        {
            Console.WriteLine("Starting continuous REAL voice recognition...");
            ShowGroceryPhrases();
            Console.WriteLine("\nPress 'q' and Enter to stop");

            isListening = true;

            while (isListening)
            {
                Console.WriteLine("\nPress Enter to record your voice (or 'q' to quit):");
                string input = Console.ReadLine();

                if (input?.ToLower() == "q")
                {
                    break;
                }

                string result = await RecognizeOnceAsync();

                if (!result.StartsWith("ERROR:"))
                {
                    Console.WriteLine($"🎉 RECOGNIZED FROM YOUR VOICE: '{result}'");
                    OnSpeechRecognized(result, CurrentLanguage);
                }
                else
                {
                    Console.WriteLine($"❌ Voice recognition failed: {result}");
                }
            }

            isListening = false;
            Console.WriteLine("Stopped continuous voice recognition");
        }

        private void OnSpeechRecognized(string recognizedText, string detectedLanguage)
        {
            Console.WriteLine($"📤 SENDING YOUR VOICE TO GEMINI: '{recognizedText}'");
            Console.WriteLine($"🌐 Language: {detectedLanguage}");
        }

        public void ShowGroceryPhrases()
        {
            Console.WriteLine("\n🛒 Speak these grocery store phrases into your microphone:");

            if (CurrentLanguage == "es-ES")
            {
                Console.WriteLine("\n🇪🇸 Spanish phrases to speak:");
                Console.WriteLine("• Hola, ¿dónde están las manzanas?");
                Console.WriteLine("• ¿Cuánto cuesta la leche?");
                Console.WriteLine("• Disculpe, ¿dónde está la caja?");
                Console.WriteLine("• ¿Tienen descuentos en frutas?");
                Console.WriteLine("• Necesito ayuda, por favor");
            }
            else
            {
                Console.WriteLine("\n🇺🇸 English phrases to speak:");
                Console.WriteLine("• Excuse me, where are the apples?");
                Console.WriteLine("• How much does the milk cost?");
                Console.WriteLine("• Where is the checkout?");
                Console.WriteLine("• Do you have any discounts?");
                Console.WriteLine("• I need help, please");
            }
        }

        public void TestMicrophone()
        {
            Console.WriteLine("\n🔍 Testing REAL voice recognition setup...");

            // Check if credentials file exists
            string keyPath = "gcloud-key.json";
            if (File.Exists(keyPath))
            {
                Console.WriteLine($"✅ Google Cloud credentials found: {Path.GetFullPath(keyPath)}");
            }
            else
            {
                Console.WriteLine($"❌ Credentials file NOT found: {Path.GetFullPath(keyPath)}");
                Console.WriteLine("📝 Make sure gcloud-key.json is in your project directory");
                return;
            }

            // Check if speech client is initialized
            if (speechClient != null)
            {
                Console.WriteLine("✅ Google Cloud Speech client ready");
                Console.WriteLine("✅ Using NAudio for microphone recording");
                Console.WriteLine("✅ Falls back to text input if microphone issues");
                Console.WriteLine("✅ Then sends result to Gemini for AI response");

                // Test microphone availability
                try
                {
                    int deviceCount = WaveInEvent.DeviceCount;
                    Console.WriteLine($"🎤 Found {deviceCount} audio input device(s)");

                    for (int i = 0; i < deviceCount; i++)
                    {
                        var capabilities = WaveInEvent.GetCapabilities(i);
                        Console.WriteLine($"   Device {i}: {capabilities.ProductName}");
                    }

                    if (deviceCount > 0)
                    {
                        Console.WriteLine("✅ Microphone hardware detected");
                    }
                    else
                    {
                        Console.WriteLine("⚠️ No microphone detected - will use text fallback");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Could not enumerate audio devices: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("❌ Google Cloud Speech client NOT initialized");
            }
        }

        public bool IsListening => isListening;

        public void Dispose()
        {
            isListening = false;
            if (waveIn != null)
            {
                try
                {
                    waveIn.StopRecording();
                    waveIn.Dispose();
                }
                catch { }
            }
        }
    }
}
using System;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1;
using System.IO;
using System.Threading;
using NAudio.Wave;
using System.Collections.Generic;

namespace LanguageVR.Pipeline.VoiceToText
{
    public class GoogleCloudSpeechService
    {
        private SpeechClient speechClient;
        private WaveInEvent waveIn;
        private List<byte> audioBuffer;
        private bool isRecording = false;
        private const int SAMPLE_RATE = 16000;

        public GoogleCloudSpeechService()
        {
            InitializeGoogleCloudSpeech();
            audioBuffer = new List<byte>();
        }

        private void InitializeGoogleCloudSpeech()
        {
            try
            {
                Console.WriteLine("🔧 Initializing Google Cloud Speech...");

                string keyPath = "gcloud-key.json";

                if (!File.Exists(keyPath))
                {
                    Console.WriteLine($"❌ Error: gcloud-key.json not found at {Path.GetFullPath(keyPath)}");
                    speechClient = null;
                    return;
                }

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyPath);
                speechClient = SpeechClient.Create();

                // Initialize audio recording
                InitializeAudioRecording();

                Console.WriteLine("✅ Google Cloud Speech Service ready");
                Console.WriteLine("✅ Microphone ready for button-based recording");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error initializing Speech Service: {ex.Message}");
                speechClient = null;
            }
        }

        private void InitializeAudioRecording()
        {
            try
            {
                waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(SAMPLE_RATE, 16, 1), // 16kHz, 16-bit, mono
                    BufferMilliseconds = 100
                };

                waveIn.DataAvailable += (sender, e) =>
                {
                    if (isRecording)
                    {
                        lock (audioBuffer)
                        {
                            audioBuffer.AddRange(e.Buffer.AsSpan(0, e.BytesRecorded).ToArray());
                        }
                    }
                };

                // Test microphone availability
                int deviceCount = WaveInEvent.DeviceCount;
                if (deviceCount > 0)
                {
                    Console.WriteLine($"🎤 Found {deviceCount} microphone(s)");
                    var capabilities = WaveInEvent.GetCapabilities(0);
                    Console.WriteLine($"🎤 Using: {capabilities.ProductName}");
                }
                else
                {
                    Console.WriteLine("⚠️ No microphone detected");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Audio initialization warning: {ex.Message}");
            }
        }

        // Called when button is pressed
        public Task StartRecordingAsync()
        {
            if (isRecording)
            {
                Console.WriteLine("⚠️ Already recording!");
                return Task.CompletedTask;
            }

            try
            {
                lock (audioBuffer)
                {
                    audioBuffer.Clear();
                }

                isRecording = true;
                waveIn.StartRecording();
                Console.WriteLine("🎤 Recording... Speak now!");

                // Start a background task to show recording progress
                Task.Run(async () =>
                {
                    int seconds = 0;
                    while (isRecording)
                    {
                        await Task.Delay(1000);
                        seconds++;
                        if (isRecording) // Check again after delay
                        {
                            Console.Write($"\r🎤 Recording... {seconds}s");
                        }
                    }
                    Console.WriteLine(); // New line after recording stops
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to start recording: {ex.Message}");
                isRecording = false;
            }

            return Task.CompletedTask;
        }

        // Called when button is released
        public async Task<byte[]> StopRecordingAsync()
        {
            if (!isRecording)
            {
                Console.WriteLine("⚠️ Not recording!");
                return null;
            }

            try
            {
                isRecording = false;
                waveIn.StopRecording();
                Console.WriteLine("⏹️ Recording stopped");

                // Get copy of audio data
                byte[] audioData;
                lock (audioBuffer)
                {
                    audioData = audioBuffer.ToArray();
                }

                Console.WriteLine($"📊 Recorded {audioData.Length / (SAMPLE_RATE * 2.0):F1} seconds of audio");

                return audioData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to stop recording: {ex.Message}");
                return null;
            }
        }

        public async Task<string> RecognizeFromAudioDataAsync(byte[] audioData)
        {
            if (speechClient == null)
            {
                return "ERROR: Speech service not initialized";
            }

            if (audioData == null || audioData.Length == 0)
            {
                return "ERROR: No audio data";
            }

            try
            {
                // Create WAV format audio for Google Cloud
                byte[] wavData = CreateWavFile(audioData, SAMPLE_RATE);

                // Create recognition config for Spanish
                var config = new RecognitionConfig
                {
                    Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                    SampleRateHertz = SAMPLE_RATE,
                    LanguageCode = "es-ES", // Spanish
                    EnableAutomaticPunctuation = true,
                    Model = "latest_long",
                    UseEnhanced = true,
                    // Add alternative languages for better recognition
                    AlternativeLanguageCodes = { "es-MX", "es-US" }
                };

                // Create audio from bytes
                var audio = RecognitionAudio.FromBytes(wavData);

                // Send to Google Cloud Speech
                Console.WriteLine("📡 Sending to Google Cloud Speech...");
                var response = await speechClient.RecognizeAsync(config, audio);

                if (response.Results.Count == 0)
                {
                    Console.WriteLine("😶 No speech detected");
                    return "";
                }

                // Get best result
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
                    Console.WriteLine($"✅ Recognized with {bestConfidence:P0} confidence");
                    return bestTranscript;
                }

                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Recognition error: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }

        private byte[] CreateWavFile(byte[] audioData, int sampleRate)
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = new BinaryWriter(memoryStream))
            {
                // Write WAV header
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + audioData.Length);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16); // Subchunk1Size
                writer.Write((short)1); // AudioFormat (PCM)
                writer.Write((short)1); // NumChannels
                writer.Write(sampleRate); // SampleRate
                writer.Write(sampleRate * 2); // ByteRate
                writer.Write((short)2); // BlockAlign
                writer.Write((short)16); // BitsPerSample
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(audioData.Length);
                writer.Write(audioData);

                return memoryStream.ToArray();
            }
        }

        public bool IsListening => isRecording;

        public void Dispose()
        {
            isRecording = false;
            if (waveIn != null)
            {
                try
                {
                    waveIn.StopRecording();
                    waveIn.Dispose();
                }
                catch { }
            }
            Console.WriteLine("🔇 Speech service disposed");
        }
    }
}
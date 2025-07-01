using System;
using System.IO;
using System.Threading.Tasks;
using Google.Cloud.TextToSpeech.V1;
using NAudio.Wave;

namespace LanguageVR.Pipeline.TextToSpeech
{
    public class GoogleCloudTextToSpeechService
    {
        private TextToSpeechClient ttsClient;
        private VoiceSelectionParams currentVoice;
        private AudioConfig audioConfig;

        // Available Spanish voices for variety
        private readonly string[] spanishVoices = new[]
        {
            "es-ES-Standard-A", // Female
            "es-ES-Standard-B", // Male
            "es-ES-Wavenet-C",  // Female (WaveNet - more natural)
            "es-ES-Wavenet-B",  // Male (WaveNet - more natural)
            "es-ES-Neural2-A",  // Female (Neural2 - most natural)
            "es-ES-Neural2-B",  // Male (Neural2 - most natural)
        };

        public GoogleCloudTextToSpeechService()
        {
            InitializeTextToSpeech();
        }

        private void InitializeTextToSpeech()
        {
            try
            {
                Console.WriteLine("🎙️ Initializing Google Cloud Text-to-Speech...");

                // Check for credentials (uses same file as Speech-to-Text)
                string keyPath = "gcloud-key.json";
                if (!File.Exists(keyPath))
                {
                    Console.WriteLine($"❌ Error: gcloud-key.json not found at {Path.GetFullPath(keyPath)}");
                    ttsClient = null;
                    return;
                }

                // Create TTS client
                ttsClient = TextToSpeechClient.Create();

                // Set default voice (Spanish female, Neural2 for best quality)
                currentVoice = new VoiceSelectionParams
                {
                    LanguageCode = "es-ES",
                    Name = "es-ES-Neural2-A",
                    SsmlGender = SsmlVoiceGender.Female
                };

                // Configure audio output (MP3 for good quality/size balance)
                audioConfig = new AudioConfig
                {
                    AudioEncoding = AudioEncoding.Mp3,
                    SpeakingRate = 1.0, // Normal speed
                    Pitch = 0.0,        // Normal pitch
                    VolumeGainDb = 0.0  // Normal volume
                };

                Console.WriteLine("✅ Text-to-Speech initialized successfully");
                Console.WriteLine($"🗣️ Default voice: {currentVoice.Name} ({currentVoice.SsmlGender})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error initializing Text-to-Speech: {ex.Message}");
                ttsClient = null;
            }
        }

        public async Task<bool> SpeakAsync(string text, bool saveToFile = false)
        {
            if (ttsClient == null)
            {
                Console.WriteLine("❌ Text-to-Speech service not initialized");
                return false;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine("⚠️ No text provided to speak");
                return false;
            }

            try
            {
                Console.WriteLine($"🗣️ Speaking: \"{text}\"");

                // Create synthesis input
                var input = new SynthesisInput
                {
                    Text = text
                };

                // Perform text-to-speech request
                var response = await ttsClient.SynthesizeSpeechAsync(
                    input,
                    currentVoice,
                    audioConfig
                );

                // Save audio to temporary file
                string tempFile = Path.Combine(Path.GetTempPath(), $"npc_speech_{Guid.NewGuid()}.mp3");
                using (var output = File.Create(tempFile))
                {
                    response.AudioContent.WriteTo(output);
                }

                Console.WriteLine("✅ Speech synthesized successfully");

                // Play the audio
                await PlayAudioAsync(tempFile);

                // Optionally save to permanent location
                if (saveToFile)
                {
                    string savedFile = $"npc_response_{DateTime.Now:yyyyMMdd_HHmmss}.mp3";
                    File.Copy(tempFile, savedFile, true);
                    Console.WriteLine($"💾 Audio saved to: {savedFile}");
                }

                // Clean up temp file
                try { File.Delete(tempFile); } catch { }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Speech synthesis error: {ex.Message}");
                return false;
            }
        }

        private async Task PlayAudioAsync(string audioFile)
        {
            try
            {
                Console.WriteLine("🔊 Playing audio...");

                using (var audioFileReader = new AudioFileReader(audioFile))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFileReader);
                    outputDevice.Play();

                    // Wait for playback to finish
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        await Task.Delay(100);
                    }
                }

                Console.WriteLine("✅ Audio playback complete");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Audio playback error: {ex.Message}");
                Console.WriteLine("💡 Audio file saved but couldn't play automatically");
            }
        }

        public void SetVoice(string voiceName)
        {
            if (Array.Exists(spanishVoices, v => v == voiceName))
            {
                currentVoice.Name = voiceName;
                // Determine gender from voice name
                currentVoice.SsmlGender = voiceName.Contains("-A") || voiceName.Contains("-C")
                    ? SsmlVoiceGender.Female
                    : SsmlVoiceGender.Male;

                Console.WriteLine($"🗣️ Voice changed to: {voiceName} ({currentVoice.SsmlGender})");
            }
            else
            {
                Console.WriteLine($"⚠️ Unknown voice: {voiceName}");
                ShowAvailableVoices();
            }
        }

        public void SetSpeechParameters(double speakingRate = 1.0, double pitch = 0.0, double volumeGainDb = 0.0)
        {
            audioConfig.SpeakingRate = Math.Max(0.25, Math.Min(4.0, speakingRate));
            audioConfig.Pitch = Math.Max(-20.0, Math.Min(20.0, pitch));
            audioConfig.VolumeGainDb = Math.Max(-96.0, Math.Min(16.0, volumeGainDb));

            Console.WriteLine($"🎚️ Speech parameters updated:");
            Console.WriteLine($"   Speed: {audioConfig.SpeakingRate}x");
            Console.WriteLine($"   Pitch: {audioConfig.Pitch:+0.0;-0.0;0}");
            Console.WriteLine($"   Volume: {audioConfig.VolumeGainDb:+0.0;-0.0;0}dB");
        }

        public void ShowAvailableVoices()
        {
            Console.WriteLine("\n🗣️ Available Spanish voices:");
            Console.WriteLine("Standard voices (good quality):");
            Console.WriteLine("  • es-ES-Standard-A (Female)");
            Console.WriteLine("  • es-ES-Standard-B (Male)");
            Console.WriteLine("\nWaveNet voices (better quality):");
            Console.WriteLine("  • es-ES-Wavenet-C (Female)");
            Console.WriteLine("  • es-ES-Wavenet-B (Male)");
            Console.WriteLine("\nNeural2 voices (best quality - recommended):");
            Console.WriteLine("  • es-ES-Neural2-A (Female) ⭐");
            Console.WriteLine("  • es-ES-Neural2-B (Male) ⭐");
        }

        public async Task TestVoicesAsync()
        {
            Console.WriteLine("\n🎤 Testing different Spanish NPC voices...");
            string testPhrase = "Hola, las manzanas están en el pasillo tres, junto a las frutas.";

            foreach (var voice in new[] { "es-ES-Standard-A", "es-ES-Wavenet-C", "es-ES-Neural2-A" })
            {
                SetVoice(voice);
                Console.WriteLine($"\n🗣️ Testing {voice}...");
                await SpeakAsync(testPhrase);
                await Task.Delay(1000); // Pause between voices
            }
        }

        public void Dispose()
        {
            // Client will be garbage collected
            Console.WriteLine("🔇 Text-to-Speech service disposed");
        }
    }
}
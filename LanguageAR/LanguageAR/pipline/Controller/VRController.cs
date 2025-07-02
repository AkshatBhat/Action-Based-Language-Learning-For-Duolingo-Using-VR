using System;
using System.Threading;
using System.Threading.Tasks;

namespace LanguageVR.Pipline.Controller
{
    public class VRController
    {
        private readonly PipelineManager pipelineManager;
        private bool isRunning = true;
        private bool isButtonPressed = false;
        private bool isProcessing = false;
        private byte[] recordedAudio = null;

        // VR/Keyboard activation settings
        public ConsoleKey ActivationKey { get; set; } = ConsoleKey.Spacebar;
        public ConsoleKey ExitKey { get; set; } = ConsoleKey.Escape;

        public VRController(PipelineManager pipeline)
        {
            pipelineManager = pipeline;
        }

        public async Task StartVRModeAsync()
        {
            Console.WriteLine("🎮 Ready for interaction!");
            Console.WriteLine("Press SPACEBAR to start recording");
            Console.WriteLine("Press SPACEBAR again to stop recording and process\n");

            // Main VR interaction loop
            await VRMainLoopAsync();
        }

        private async Task VRMainLoopAsync()
        {
            while (isRunning)
            {
                if (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);

                    if (keyInfo.Key == ExitKey)
                    {
                        Console.WriteLine("\n🚪 Exiting VR mode...");
                        isRunning = false;
                        break;
                    }
                    else if (keyInfo.Key == ActivationKey && !isProcessing)
                    {
                        await HandleButtonToggleAsync();
                    }
                }

                await Task.Delay(50); // Small delay to prevent CPU spinning
            }
        }

        private async Task HandleButtonToggleAsync()
        {
            if (!isButtonPressed)
            {
                // First press - start recording
                await StartRecordingAsync();
            }
            else
            {
                // Second press - stop recording and process
                await StopRecordingAndProcessAsync();
            }
        }

        private async Task StartRecordingAsync()
        {
            try
            {
                isButtonPressed = true;
                Console.WriteLine("\n🔴 Recording started...");
                Console.WriteLine("Press SPACEBAR again to stop recording");

                // Start recording through speech service
                var speechService = pipelineManager.GetSpeechService();
                await speechService.StartRecordingAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to start recording: {ex.Message}");
                isButtonPressed = false;
            }
        }

        private async Task StopRecordingAndProcessAsync()
        {
            if (isProcessing) return;

            isProcessing = true;
            isButtonPressed = false;

            try
            {
                Console.WriteLine("⏹️ Recording stopped");

                // Stop recording and get audio data
                var speechService = pipelineManager.GetSpeechService();
                recordedAudio = await speechService.StopRecordingAsync();

                if (recordedAudio == null || recordedAudio.Length == 0)
                {
                    Console.WriteLine("❌ No audio recorded. Try again.");
                    Console.WriteLine("\nPress SPACEBAR to start recording...\n");
                    return;
                }

                Console.WriteLine("🔄 Processing...");

                // Process the recorded audio through the pipeline
                string result = await pipelineManager.ProcessVoiceInputAsync(recordedAudio);

                if (result.StartsWith("ERROR:"))
                {
                    Console.WriteLine("\n🔄 Please try recording again");
                    Console.WriteLine("💡 Tips: Speak clearly in Spanish\n");
                }
                else
                {
                    Console.WriteLine("\n✅ Interaction complete!");
                }

                Console.WriteLine("\nPress SPACEBAR to start new recording...\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Processing error: {ex.Message}");
                Console.WriteLine("\nPress SPACEBAR to try again...\n");
            }
            finally
            {
                isProcessing = false;
                recordedAudio = null;
            }
        }

        public void Dispose()
        {
            isRunning = false;
            // PipelineManager handles all service disposal
        }
    }
}
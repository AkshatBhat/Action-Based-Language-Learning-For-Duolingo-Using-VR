using System;
using System.Threading;
using System.Threading.Tasks; 

namespace LanguageVR.Pipline.Controller
{
    public class VRController
    {
        private readonly PipelineManager pipelineManager;
        private bool isProcessing = false;

        // VR/Keyboard activation settings
        public ConsoleKey ActivationKey { get; set; } = ConsoleKey.Spacebar;

        public VRController(PipelineManager pipeline)
        {
            pipelineManager = pipeline;

            Console.WriteLine("VR Controller initialized");
            Console.WriteLine($"Press {ActivationKey} to activate voice recognition");
            Console.WriteLine("Press 'Q' to quit");
        }

        public async Task StartVRModeAsync()
        {
            Console.WriteLine("\nVR Mode Started!");
            Console.WriteLine("====================");
            Console.WriteLine("This simulates Meta Quest 2 button activation");
            Console.WriteLine($"Press {ActivationKey} to start listening (like pressing VR controller button)");
            Console.WriteLine("System will automatically:");
            Console.WriteLine("   1. Listen for REAL speech from microphone");
            Console.WriteLine("   2. Detect language from REAL transcription");
            Console.WriteLine("   3. Send REAL Spanish text to Gemini");
            Console.WriteLine("   4. Return NPC response based on YOUR voice");
            Console.WriteLine("\nPress 'Q' to exit VR mode\n");

            // Start the main VR loop
            await VRMainLoopAsync();
        }

        private async Task VRMainLoopAsync()
        {
            while (true)
            {
                // Check for key press (simulates VR controller button)
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                    if (keyInfo.Key == ConsoleKey.Q)
                    {
                        Console.WriteLine("Exiting VR mode...");
                        break;
                    }
                    else if (keyInfo.Key == ActivationKey && !isProcessing)
                    {
                        await HandleVRButtonPressAsync();
                    }
                }

                // Small delay to prevent CPU spinning
                await Task.Delay(50);
            }
        }

        private async Task HandleVRButtonPressAsync()
        {
            if (isProcessing)
            {
                Console.WriteLine("Already processing, please wait...");
                return;
            }

            isProcessing = true;

            try
            {
                Console.WriteLine("VR Button Activated!");
                Console.WriteLine("Starting REAL voice recognition pipeline...");

                // ONLY use the pipeline manager - NO simulation, NO direct Gemini calls
                // This calls: Azure Speech -> Language Detection -> Gemini -> Response
                string result = await pipelineManager.ProcessVoiceInputAsync();

                // Handle the result from the complete pipeline
                if (result.StartsWith("ERROR:"))
                {
                    Console.WriteLine($"Speech recognition error: {result}");
                }
                else if (result.Contains("problema técnico"))
                {
                    Console.WriteLine($"Technical issue: {result}");
                }
                else if (result.Contains("habla en español"))
                {
                    Console.WriteLine($"Language info: {result}");
                    Console.WriteLine("Try speaking in Spanish to get an NPC response");
                }
                else
                {
                    // Success - got real NPC response from Gemini based on real voice
                    Console.WriteLine("\n" + new string('=', 50));
                    Console.WriteLine("NPC RESPONSE (based on your real voice):");
                    Console.WriteLine($"{result}");
                    Console.WriteLine(new string('=', 50) + "\n");

                    Console.WriteLine("[Next step: Convert this response to speech for NPC voice]");
                }

                Console.WriteLine($"\nReady! Press {ActivationKey} again to speak...\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in VR processing: {ex.Message}");
            }
            finally
            {
                isProcessing = false;
            }
        }

        public void ShowCommands()
        {
            Console.WriteLine("\nVR Controller Commands:");
            Console.WriteLine($"{ActivationKey} - Activate REAL voice recognition");
            Console.WriteLine("Q - Quit VR mode");
            Console.WriteLine("\nSpeak these Spanish phrases into your microphone:");
            Console.WriteLine("• Hola, ¿dónde están las manzanas?");
            Console.WriteLine("• ¿Cuánto cuesta la leche?");
            Console.WriteLine("• Disculpe, ¿dónde está la caja?");
            Console.WriteLine("• Necesito ayuda, por favor");
        }

        public void SetActivationKey(ConsoleKey key)
        {
            ActivationKey = key;
            Console.WriteLine($"Activation key set to: {key}");
        }

        public void Dispose()
        {
            // PipelineManager handles all service disposal
        }
    }
}
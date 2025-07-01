using System;
using System.Threading.Tasks;
using LanguageVR.Pipline;

namespace LanguageVR
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🌐 Language Learning VR - Grocery Store Simulator");
            Console.WriteLine("================================================");
            Console.WriteLine("Learn Spanish through realistic shopping scenarios!");
            Console.WriteLine();

            var pipelineManager = new PipelineManager();

            while (true)
            {
                Console.WriteLine("\n📋 Main Menu:");
                Console.WriteLine("1. 🎮 Start VR Mode (Voice → Gemini → NPC Speech)");
                Console.WriteLine("2. 🧪 Test Pipeline Components");
                Console.WriteLine("3. 🎭 Configure NPC Voice");
                Console.WriteLine("4. 📊 Show System Status");
                Console.WriteLine("5. ❓ Show Available Commands");
                Console.WriteLine("6. 🚪 Exit");
                Console.Write("\nSelect option (1-6): ");

                string choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            await pipelineManager.StartVRModeAsync();
                            break;

                        case "2":
                            await pipelineManager.TestPipelineAsync();
                            break;

                        case "3":
                            await pipelineManager.ConfigureNPCVoiceAsync();
                            break;

                        case "4":
                            pipelineManager.ShowSystemStatus();
                            break;

                        case "5":
                            pipelineManager.ShowAvailableCommands();
                            break;

                        case "6":
                            Console.WriteLine("\n👋 ¡Adiós! Thanks for learning Spanish!");
                            pipelineManager.Dispose();
                            return;

                        default:
                            Console.WriteLine("❌ Invalid option. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error: {ex.Message}");
                    Console.WriteLine("Please try again or check your configuration.");
                }
            }
        }
    }
}
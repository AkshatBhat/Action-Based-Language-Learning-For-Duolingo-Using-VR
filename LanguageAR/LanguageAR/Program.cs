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
            Console.WriteLine("¡Bienvenido a nuestra tienda de comestibles!");
            Console.WriteLine("Welcome to our grocery store!");
            Console.WriteLine();

            var pipelineManager = new PipelineManager();

            // Play initial Spanish greeting
            await pipelineManager.PlayInitialGreetingAsync();

            // Start VR mode directly
            await pipelineManager.StartVRModeAsync();

            // Cleanup
            pipelineManager.Dispose();
            Console.WriteLine("\n👋 ¡Adiós! Thanks for learning Spanish!");
        }
    }
}
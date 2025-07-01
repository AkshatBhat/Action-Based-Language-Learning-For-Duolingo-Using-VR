using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LanguageVR.Pipline.ChatIntegration
{
    public class GeminiService
    {
        private readonly HttpClient httpClient;
        private readonly string apiKey;

        public GeminiService()
        {
            httpClient = new HttpClient();

            // Read API key from file (like your reference code)
            try
            {
                apiKey = File.ReadAllText("gemini-key.txt").Trim();
                Console.WriteLine("✅ Gemini API key loaded from gemini-key.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error reading gemini-key.txt: {ex.Message}");
                Console.WriteLine("📝 Create gemini-key.txt file with your API key");
                apiKey = "AIzaSyD-IQEAqMreTJLFQHSOALvPqVbCILaj5lg";
            }

            // Set up the HTTP client with API key header
            httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

            Console.WriteLine("✅ Gemini Service initialized");
            Console.WriteLine("🤖 Ready to generate NPC responses for grocery store conversations");
        }

        public async Task<string> GetNPCResponseAsync(string userMessage, string detectedLanguage)
        {
            try
            {
                // Only process Spanish messages as requested
                if (!IsSpanish(detectedLanguage))
                {
                    return "I only respond to Spanish. Try saying something in Spanish!";
                }

                Console.WriteLine($"Sending to Gemini: '{userMessage}' ({detectedLanguage})");

                // Create the grocery store NPC prompt (based on your reference)
                string prompt = CreateGroceryStorePrompt(userMessage);

                // Create request payload (matching your reference structure)
                var payload = new
                {
                    contents = new[]
                    {
                        new {
                            role = "user",
                            parts = new[] {
                                new { text = prompt }
                            }
                        }
                    }
                };

                string jsonRequest = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Send request to Gemini (using your working endpoint)
                var response = await httpClient.PostAsync(
                    "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent",
                    content);

                string json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Gemini error: {response.StatusCode} -> {json}");
                    return "Lo siento, no pude responder en este momento.";
                }

                return ParseGeminiResponse(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Gemini API: {ex.Message}");
                return "Lo siento, hay un problema técnico.";
            }
        }

        private bool IsSpanish(string detectedLanguage)
        {
            return detectedLanguage.Contains("Spanish") ||
                   detectedLanguage.Contains("es-") ||
                   detectedLanguage.ToLower().Contains("spanish");
        }

        private string CreateGroceryStorePrompt(string userMessage)
        {
            // Based on your working prompt structure
            return $@"
                Eres un asistente de tienda amigable ayudando a personas que están aprendiendo español. 
                Tu trabajo es responder solo en español, de forma muy breve, clara y sencilla — como si hablaras con un principiante. 
                Usa frases cortas de máximo 1-2 líneas. 
                No expliques más de lo necesario.
                
                Contexto: Trabajas en un supermercado y ayudas a los clientes a encontrar productos, 
                responder preguntas sobre precios, y brindar información sobre servicios de la tienda.
                
                Productos comunes están en estas áreas:
                - Frutas y verduras: sección de productos frescos
                - Leche y lácteos: sección de lácteos
                - Carnes: carnicería
                - Pan: panadería
                - Caja: al final del pasillo central
                
                El cliente dijo: ""{userMessage}"". ¿Qué responderías tú?";
        }

        private string ParseGeminiResponse(string json)
        {
            try
            {
                // Parse response using your working method
                using var doc = JsonDocument.Parse(json);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                string response = text ?? "Lo siento, no pude entender la respuesta.";

                Console.WriteLine($"Gemini response: '{response}'");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing Gemini response: {ex.Message}");
                Console.WriteLine($"Raw response: {json}");
                return "Lo siento, ocurrió un error al analizar la respuesta.";
            }
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}
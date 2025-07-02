using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace LanguageVR.Pipline.ChatIntegration
{
    public class GeminiService : MonoBehaviour
    {
        private string apiKey;
        private System.Action<string> logCallback;
        private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        public void Initialize(System.Action<string> logger = null)
        {
            logCallback = logger;
            LoadApiKey();
        }

        private void LoadApiKey()
        {
            try
            {
                // Try persistent data path first
                string keyPath = Path.Combine(Application.persistentDataPath, "gemini-key.txt");
                
                // Check StreamingAssets if not in persistent path
                if (!File.Exists(keyPath))
                {
                    StartCoroutine(LoadKeyFromStreamingAssets());
                }
                else
                {
                    apiKey = File.ReadAllText(keyPath).Trim();
                    LogMessage("✅ Gemini API key loaded from persistent data");
                    LogMessage("✅ Gemini Service initialized");
                    LogMessage("🤖 Ready to generate NPC responses for grocery store conversations");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Error loading API key: {ex.Message}");
                apiKey = "AIzaSyD-IQEAqMreTJLFQHSOALvPqVbCILaj5lg"; // Fallback
            }
        }

        IEnumerator LoadKeyFromStreamingAssets()
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath, "gemini-key.txt");
            
            #if UNITY_ANDROID && !UNITY_EDITOR
            // On Android, StreamingAssets are in a compressed archive
            using (UnityWebRequest www = UnityWebRequest.Get(streamingPath))
            {
                yield return www.SendWebRequest();
                
                if (www.result == UnityWebRequest.Result.Success)
                {
                    apiKey = www.downloadHandler.text.Trim();
                    
                    // Save to persistent path for next time
                    string persistentPath = Path.Combine(Application.persistentDataPath, "gemini-key.txt");
                    
                    bool saveSuccessful = false;
                    string saveError = "";
                    
                    // Try to save without try-catch around yield
                    if (!string.IsNullOrEmpty(apiKey))
                    {
                        try
                        {
                            File.WriteAllText(persistentPath, apiKey);
                            saveSuccessful = true;
                            LogMessage("✅ Gemini API key loaded from StreamingAssets");
                        }
                        catch (Exception ex)
                        {
                            saveError = ex.Message;
                            saveSuccessful = false;
                        }
                    }
                    
                    if (!saveSuccessful && !string.IsNullOrEmpty(saveError))
                    {
                        LogMessage($"⚠️ Could not save API key: {saveError}");
                    }
                }
                else
                {
                    LogMessage($"❌ Failed to load gemini-key.txt from StreamingAssets");
                    LogMessage("📝 Please place gemini-key.txt in StreamingAssets folder");
                    apiKey = "AIzaSyD-IQEAqMreTJLFQHSOALvPqVbCILaj5lg"; // Fallback
                }
            }
            #else
            // On other platforms, we can read directly
            bool keyLoaded = false;
            string readError = "";
            
            if (File.Exists(streamingPath))
            {
                // Try to read file without try-catch around yield
                try
                {
                    apiKey = File.ReadAllText(streamingPath).Trim();
                    keyLoaded = true;
                }
                catch (Exception ex)
                {
                    readError = ex.Message;
                    keyLoaded = false;
                }
                
                if (keyLoaded)
                {
                    // Try to save to persistent path
                    try
                    {
                        string persistentPath = Path.Combine(Application.persistentDataPath, "gemini-key.txt");
                        File.WriteAllText(persistentPath, apiKey);
                        LogMessage("✅ Gemini API key loaded from StreamingAssets");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"⚠️ Could not save API key: {ex.Message}");
                    }
                }
                else
                {
                    LogMessage($"❌ Error reading API key: {readError}");
                }
            }
            
            if (!keyLoaded)
            {
                LogMessage($"❌ gemini-key.txt not found in StreamingAssets");
                apiKey = "AIzaSyD-IQEAqMreTJLFQHSOALvPqVbCILaj5lg"; // Fallback
            }
            
            yield return null;
            #endif
            
            LogMessage("✅ Gemini Service initialized");
            LogMessage("🤖 Ready to generate NPC responses for grocery store conversations");
        }

        public IEnumerator GetNPCResponseCoroutine(string userMessage, string detectedLanguage, System.Action<string> callback)
        {
            string response = "";
            
            // Only process Spanish messages as requested
            if (!IsSpanish(detectedLanguage))
            {
                callback("I only respond to Spanish. Try saying something in Spanish!");
                yield break;
            }

            LogMessage($"Sending to Gemini: '{userMessage}' ({detectedLanguage})");

            // Create the grocery store NPC prompt
            string prompt = CreateGroceryStorePrompt(userMessage);
            
            // Escape the prompt for JSON (this is crucial!)
            string escapedPrompt = prompt.Replace("\"", "\\\"")
                                        .Replace("\n", "\\n")
                                        .Replace("\r", "\\r")
                                        .Replace("\t", "\\t");

            // Create JSON manually (Unity's JsonUtility doesn't handle complex nested objects properly)
            string jsonRequest = $@"{{
    ""contents"": [
        {{
            ""role"": ""user"",
            ""parts"": [
                {{
                    ""text"": ""{escapedPrompt}""
                }}
            ]
        }}
    ]
}}";
            
            LogMessage("📡 Sending request to Gemini...");
            
            // Create UnityWebRequest
            using (UnityWebRequest request = new UnityWebRequest(GEMINI_API_URL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("x-goog-api-key", apiKey);

                // Send request
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogMessage($"❌ Gemini error: {request.error}");
                    LogMessage($"Response: {request.downloadHandler.text}");
                    response = "Lo siento, no pude responder en este momento.";
                }
                else
                {
                    LogMessage("✅ Gemini response received successfully");
                    string json = request.downloadHandler.text;
                    response = ParseGeminiResponse(json);
                }
            }

            callback(response);
        }

        private bool IsSpanish(string detectedLanguage)
        {
            return detectedLanguage.Contains("Spanish") ||
                   detectedLanguage.Contains("es-") ||
                   detectedLanguage.ToLower().Contains("spanish");
        }

        private string CreateGroceryStorePrompt(string userMessage)
        {
            return $@"Eres un asistente de tienda amigable ayudando a personas que están aprendiendo español. Tu trabajo es responder solo en español, de forma muy breve, clara y sencilla — como si hablaras con un principiante. Usa frases cortas de máximo 1-2 líneas. No expliques más de lo necesario.

Contexto: Trabajas en un supermercado y ayudas a los clientes a encontrar productos, responder preguntas sobre precios, y brindar información sobre servicios de la tienda.

Productos comunes están en estas áreas:
- Frutas y verduras: sección de productos frescos
- Leche y lácteos: sección de lácteos  
- Carnes: carnicería
- Pan: panadería
- Caja: al final del pasillo central

El cliente dijo: '{userMessage}'. ¿Qué responderías tú?";
        }

        private string ParseGeminiResponse(string json)
        {
            try
            {
                // Unity's JsonUtility doesn't handle nested JSON well, so we'll do basic parsing
                // Look for the text field in the response
                int textIndex = json.IndexOf("\"text\":");
                if (textIndex == -1)
                {
                    LogMessage("Could not find text field in response");
                    return "Lo siento, no pude entender la respuesta.";
                }

                int startQuote = json.IndexOf("\"", textIndex + 7);
                int endQuote = json.IndexOf("\"", startQuote + 1);
                
                // Handle escaped quotes
                while (endQuote > 0 && json[endQuote - 1] == '\\')
                {
                    endQuote = json.IndexOf("\"", endQuote + 1);
                }

                if (startQuote != -1 && endQuote != -1)
                {
                    string response = json.Substring(startQuote + 1, endQuote - startQuote - 1);
                    
                    // Unescape basic JSON escapes
                    response = response.Replace("\\n", "\n")
                                     .Replace("\\\"", "\"")
                                     .Replace("\\\\", "\\")
                                     .Replace("\\r", "\r")
                                     .Replace("\\t", "\t");
                    
                    LogMessage($"Gemini response: '{response}'");
                    return response;
                }

                LogMessage("Failed to parse response text");
                return "Lo siento, ocurrió un error al analizar la respuesta.";
            }
            catch (Exception ex)
            {
                LogMessage($"Error parsing Gemini response: {ex.Message}");
                LogMessage($"Raw response: {json}");
                return "Lo siento, ocurrió un error al analizar la respuesta.";
            }
        }

        private void LogMessage(string message)
        {
            Debug.Log(message);
            logCallback?.Invoke(message);
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleCloudAuth : MonoBehaviour
{
    [System.Serializable]
    public class ServiceAccountKey
    {
        public string type;
        public string project_id;
        public string private_key_id;
        public string private_key;
        public string client_email;
        public string client_id;
        public string auth_uri;
        public string token_uri;
    }

    private ServiceAccountKey serviceAccount;
    private string apiKey;
    private bool isInitialized = false;
    private System.Action<string> logCallback;

    public void Initialize(System.Action<string> logger = null)
    {
        logCallback = logger;
        StartCoroutine(SetupAuthentication());
    }

    private IEnumerator SetupAuthentication()
    {
        LogMessage("🔍 Setting up Google Cloud authentication...");
        
        // First, try to read and parse the JSON file
        yield return StartCoroutine(ReadServiceAccountJSON());
        
        if (serviceAccount != null)
        {
            LogMessage($"✅ Found service account for project: {serviceAccount.project_id}");
            LogMessage($"📧 Service account email: {serviceAccount.client_email}");
            
            // Now check if they have an API key
            yield return StartCoroutine(CheckForAPIKey());
        }
        else
        {
            LogMessage("❌ Could not read service account JSON");
        }
    }

    private IEnumerator ReadServiceAccountJSON()
    {
        string keyPath = Path.Combine(Application.persistentDataPath, "gcloud-key.json");
        
        // Check StreamingAssets if not in persistent path
        if (!File.Exists(keyPath))
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath, "gcloud-key.json");
            
            #if UNITY_ANDROID && !UNITY_EDITOR
            // On Android, use UnityWebRequest to read from StreamingAssets
            using (UnityWebRequest www = UnityWebRequest.Get(streamingPath))
            {
                yield return www.SendWebRequest();
                
                if (www.result == UnityWebRequest.Result.Success)
                {
                    File.WriteAllText(keyPath, www.downloadHandler.text);
                }
                else
                {
                    LogMessage("❌ Could not load gcloud-key.json from StreamingAssets");
                    yield break;
                }
            }
            #else
            if (File.Exists(streamingPath))
            {
                File.Copy(streamingPath, keyPath, true);
            }
            else
            {
                LogMessage("❌ gcloud-key.json not found in StreamingAssets");
                yield break;
            }
            #endif
        }

        // Parse the JSON
        try
        {
            string jsonContent = File.ReadAllText(keyPath);
            serviceAccount = JsonUtility.FromJson<ServiceAccountKey>(jsonContent);
            LogMessage("✅ Successfully parsed service account JSON");
        }
        catch (Exception ex)
        {
            LogMessage($"❌ Error parsing service account key: {ex.Message}");
        }
    }

    private IEnumerator CheckForAPIKey()
    {
        // Check for API key file
        string apiKeyPath = Path.Combine(Application.streamingAssetsPath, "google-api-key.txt");
        
        if (File.Exists(apiKeyPath))
        {
            try
            {
                apiKey = File.ReadAllText(apiKeyPath).Trim();
                if (!string.IsNullOrEmpty(apiKey) && apiKey.StartsWith("AIza"))
                {
                    LogMessage("✅ Found valid API key file!");
                    LogMessage("🎉 Authentication setup complete!");
                    isInitialized = true;
                    yield break;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Error reading API key: {ex.Message}");
            }
        }
        
        // If we get here, they need to create an API key
        ShowAPIKeyInstructions();
        yield return null;
    }

    private void ShowAPIKeyInstructions()
    {
        LogMessage("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        LogMessage("🔧 SETUP REQUIRED: Create API Key for Your Project");
        LogMessage("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        LogMessage("");
        LogMessage($"📋 Your project ID: {serviceAccount.project_id}");
        LogMessage($"📧 Your service account: {serviceAccount.client_email}");
        LogMessage("");
        LogMessage("🌐 Steps to create API key:");
        LogMessage("1. Go to: https://console.cloud.google.com/");
        LogMessage($"2. Make sure you're in project: {serviceAccount.project_id}");
        LogMessage("3. Go to: APIs & Services → Credentials");
        LogMessage("4. Click: Create Credentials → API Key");
        LogMessage("5. Copy the API key (starts with 'AIza...')");
        LogMessage("6. Create file: StreamingAssets/google-api-key.txt");
        LogMessage("7. Paste ONLY the API key in that file");
        LogMessage("");
        LogMessage("💡 Example API key: AIzaSyD1234567890abcdefghijklmnopqrstuvwxyz");
        LogMessage("");
        LogMessage("✅ After creating the file, restart Unity and try again!");
        LogMessage("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }

    public IEnumerator GetValidAPIKey(System.Action<string> callback)
    {
        if (!isInitialized)
        {
            LogMessage("❌ Authentication not initialized");
            callback(null);
            yield break;
        }

        callback(apiKey);
    }

    public bool IsReady()
    {
        return isInitialized && !string.IsNullOrEmpty(apiKey);
    }

    private void LogMessage(string message)
    {
        Debug.Log(message);
        logCallback?.Invoke(message);
    }
}
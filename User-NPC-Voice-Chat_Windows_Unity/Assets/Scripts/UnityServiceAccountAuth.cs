using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class UnityServiceAccountAuth : MonoBehaviour
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

    [System.Serializable]
    public class AccessTokenResponse
    {
        public string access_token;
        public string token_type;
        public int expires_in;
    }

    private ServiceAccountKey serviceAccount;
    private string accessToken;
    private DateTime tokenExpiryTime;
    private System.Action<string> logCallback;
    private bool isInitialized = false;

    public void Initialize(System.Action<string> logger = null)
    {
        logCallback = logger;
        StartCoroutine(LoadServiceAccountKey());
    }

    private IEnumerator LoadServiceAccountKey()
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
                    LogMessage("✅ Service account key copied from StreamingAssets");
                }
                else
                {
                    LogMessage("❌ Could not load gcloud-key.json from StreamingAssets");
                    LogMessage("💡 Alternative: Use google-api-key.txt with simple API key");
                    yield break;
                }
            }
            #else
            if (File.Exists(streamingPath))
            {
                File.Copy(streamingPath, keyPath, true);
                LogMessage("✅ Service account key copied from StreamingAssets");
            }
            else
            {
                LogMessage("❌ gcloud-key.json not found in StreamingAssets");
                LogMessage("💡 Alternative: Create google-api-key.txt with simple API key");
                yield break;
            }
            #endif
        }

        // Parse the JSON
        try
        {
            string jsonContent = File.ReadAllText(keyPath);
            serviceAccount = JsonUtility.FromJson<ServiceAccountKey>(jsonContent);
            LogMessage("✅ Service account key loaded");
            LogMessage($"📧 Client email: {serviceAccount.client_email}");
            LogMessage($"🏗️ Project ID: {serviceAccount.project_id}");
        }
        catch (Exception ex)
        {
            LogMessage($"❌ Error parsing service account key: {ex.Message}");
            yield break;
        }

        // Try to get access token using simplified approach
        yield return StartCoroutine(GetAccessTokenViaGCloud());
    }

    private IEnumerator GetAccessTokenViaGCloud()
    {
        LogMessage("🔄 Attempting simplified authentication...");
        LogMessage("⚠️ Service account JWT signing not supported in Unity");
        LogMessage("💡 Using alternative approach...");

        // For now, let's create a fallback that instructs the user
        LogMessage("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        LogMessage("🔧 AUTHENTICATION WORKAROUND NEEDED:");
        LogMessage("1. Go to Google Cloud Console");
        LogMessage("2. APIs & Services → Credentials");
        LogMessage("3. Create Credentials → API Key");
        LogMessage("4. Copy the API key");
        LogMessage("5. Create: StreamingAssets/google-api-key.txt");
        LogMessage("6. Paste the API key in that file");
        LogMessage("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        // Try to check if they have an API key file as fallback
        yield return StartCoroutine(CheckForApiKeyFallback());
    }

    private IEnumerator CheckForApiKeyFallback()
    {
        string apiKeyPath = Path.Combine(Application.streamingAssetsPath, "google-api-key.txt");
        
        if (File.Exists(apiKeyPath))
        {
            try
            {
                string apiKey = File.ReadAllText(apiKeyPath).Trim();
                if (!string.IsNullOrEmpty(apiKey) && apiKey.StartsWith("AIza"))
                {
                    LogMessage("✅ Found API key fallback file!");
                    LogMessage("🔄 Switching to API key authentication...");
                    
                    // Store the API key for use
                    accessToken = apiKey;
                    isInitialized = true;
                    tokenExpiryTime = DateTime.MaxValue; // API keys don't expire
                    
                    LogMessage("✅ API key authentication ready");
                    yield break;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Error reading API key: {ex.Message}");
            }
        }
        
        LogMessage("❌ No valid authentication found");
        LogMessage("📝 Please create google-api-key.txt with your API key");
    }

    public IEnumerator GetValidAccessToken(System.Action<string> callback)
    {
        if (!isInitialized)
        {
            LogMessage("❌ Authentication not initialized");
            callback(null);
            yield break;
        }

        callback(accessToken);
    }

    public bool IsUsingApiKey()
    {
        return isInitialized && accessToken != null && accessToken.StartsWith("AIza");
    }

    private void LogMessage(string message)
    {
        Debug.Log(message);
        logCallback?.Invoke(message);
    }
}
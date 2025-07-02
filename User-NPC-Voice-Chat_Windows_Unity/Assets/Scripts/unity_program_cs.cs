using System.Collections;
using UnityEngine;
using LanguageVR.Pipline;

namespace LanguageVR
{
    public class LanguageVRMain : MonoBehaviour
    {
        private PipelineManager pipelineManager;
        
        // Unity UI references
        [Header("UI References")]
        [SerializeField] private UnityEngine.UI.Text statusText;
        [SerializeField] private UnityEngine.UI.Text conversationText;
        
        void Start()
        {
            StartCoroutine(InitializeVR());
        }
        
        IEnumerator InitializeVR()
        {
            LogToUnity("🌐 Language Learning VR - Grocery Store Simulator");
            LogToUnity("================================================");
            LogToUnity("¡Bienvenido a nuestra tienda de comestibles!");
            LogToUnity("Welcome to our grocery store!");
            
            // Create PipelineManager as a component (since it inherits from MonoBehaviour)
            GameObject pipelineGO = new GameObject("PipelineManager");
            pipelineGO.transform.SetParent(transform);
            pipelineManager = pipelineGO.AddComponent<PipelineManager>();
            pipelineManager.SetUnityLogger(LogToUnity);
            
            // Wait for initialization
            yield return new WaitForSeconds(1f);
            
            // Play initial Spanish greeting
            yield return StartCoroutine(pipelineManager.PlayInitialGreetingCoroutine());
            
            // Start VR mode
            yield return StartCoroutine(pipelineManager.StartVRModeCoroutine());
        }
        
        void OnDestroy()
        {
            if (pipelineManager != null)
            {
                // PipelineManager will handle cleanup in its own OnDestroy method
                LogToUnity("\n👋 ¡Adiós! Thanks for learning Spanish!");
            }
        }
        
        public void LogToUnity(string message)
        {
            Debug.Log(message);
            
            // Update UI if available
            if (statusText != null)
            {
                statusText.text = message;
            }
            
            if (conversationText != null && message.Contains("💬"))
            {
                conversationText.text += message + "\n";
            }
        }
    }
}
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

namespace LanguageVR.Pipline.Controller
{
    public class VRController : MonoBehaviour
    {
        private PipelineManager pipelineManager;
        private bool isRunning = true;
        private bool isButtonPressed = false;
        private bool isProcessing = false;
        private byte[] recordedAudio = null;
        private System.Action<string> logCallback;

        // VR Controller settings
        private bool useVRInput = false;
        private InputDevice rightController;
        private InputDevice leftController;
        private bool previousTriggerState = false;
        private bool previousGripState = false;
        
        // Fallback keyboard for testing
        private bool useFallbackKeyboard = false;

        void Awake()
        {
            // Try to detect VR
            if (XRSettings.enabled && XRSettings.loadedDeviceName != "None")
            {
                useVRInput = true;
                LogMessage("🎮 VR mode detected - using controller input");
            }
            else
            {
                useFallbackKeyboard = true;
                LogMessage("🖥️ Desktop mode - enabling keyboard fallback");
            }
        }

        void Start()
        {
            if (useVRInput)
            {
                // Initialize VR controllers
                StartCoroutine(InitializeVRControllers());
            }
        }

        public void Initialize(PipelineManager pipeline, System.Action<string> logger = null)
        {
            pipelineManager = pipeline;
            logCallback = logger;
            
            StartCoroutine(VRMainLoop());
        }

        IEnumerator InitializeVRControllers()
        {
            LogMessage("🔍 Looking for VR controllers...");
            
            // Wait a bit for controllers to initialize
            yield return new WaitForSeconds(1f);
            
            var inputDevices = new List<InputDevice>();
            InputDevices.GetDevices(inputDevices);
            
            foreach (var device in inputDevices)
            {
                if (device.characteristics.HasFlag(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller))
                {
                    rightController = device;
                    LogMessage($"✅ Right controller found: {device.name}");
                }
                else if (device.characteristics.HasFlag(InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller))
                {
                    leftController = device;
                    LogMessage($"✅ Left controller found: {device.name}");
                }
            }
            
            if (!rightController.isValid && !leftController.isValid)
            {
                LogMessage("⚠️ No VR controllers found, enabling keyboard fallback");
                useFallbackKeyboard = true;
                useVRInput = false;
            }
            else
            {
                LogMessage("🎮 VR controllers ready!");
            }
        }

        void Update()
        {
            if (!isRunning || isProcessing) return;

            bool buttonPressed = false;

            if (useVRInput)
            {
                // VR controller input
                buttonPressed = CheckVRControllerButton();
            }
            else if (useFallbackKeyboard)
            {
                // Keyboard fallback (without using old Input system)
                buttonPressed = CheckKeyboardInput();
            }

            // Handle button press
            if (buttonPressed && !isProcessing)
            {
                StartCoroutine(HandleButtonToggleCoroutine());
            }
        }

        private bool CheckVRControllerButton()
        {
            bool currentTriggerPressed = false;
            bool currentGripPressed = false;
            
            // Check right controller first
            if (rightController.isValid)
            {
                rightController.TryGetFeatureValue(CommonUsages.triggerButton, out currentTriggerPressed);
                if (!currentTriggerPressed)
                {
                    rightController.TryGetFeatureValue(CommonUsages.gripButton, out currentGripPressed);
                }
            }
            
            // If right controller not pressed, check left
            if (!currentTriggerPressed && !currentGripPressed && leftController.isValid)
            {
                leftController.TryGetFeatureValue(CommonUsages.triggerButton, out currentTriggerPressed);
                if (!currentTriggerPressed)
                {
                    leftController.TryGetFeatureValue(CommonUsages.gripButton, out currentGripPressed);
                }
            }
            
            bool currentButtonPressed = currentTriggerPressed || currentGripPressed;
            
            // Check for button press (not hold)
            bool buttonJustPressed = currentButtonPressed && !previousTriggerState && !previousGripState;
            
            // Update previous states
            previousTriggerState = currentTriggerPressed;
            previousGripState = currentGripPressed;
            
            return buttonJustPressed;
        }

        private bool CheckKeyboardInput()
        {
            // Simple polling approach for keyboard (avoiding old Input system)
            // This is a basic fallback - for production you'd want to use the new Input System package
            try
            {
                // Try to use keyboard input without UnityEngine.Input
                // Note: This is a simplified fallback - ideally use new Input System
                return false; // Disabled to avoid Input System conflicts
            }
            catch
            {
                return false;
            }
        }

        IEnumerator VRMainLoop()
        {
            LogMessage("🎮 Ready for interaction!");
            
            if (useVRInput)
            {
                LogMessage("🎯 Press controller TRIGGER or GRIP button to start/stop recording");
                LogMessage("🎯 Try both left and right controllers");
            }
            else
            {
                LogMessage("⚠️ VR controllers not detected");
                LogMessage("💡 Make sure your VR headset is connected and controllers are on");
                LogMessage("💡 Try pressing controller buttons to test");
            }

            while (isRunning)
            {
                yield return null;
            }
        }

        IEnumerator HandleButtonToggleCoroutine()
        {
            if (!isButtonPressed)
            {
                // First press - start recording
                yield return StartCoroutine(StartRecordingCoroutine());
            }
            else
            {
                // Second press - stop recording and process
                yield return StartCoroutine(StopRecordingAndProcessCoroutine());
            }
        }

        IEnumerator StartRecordingCoroutine()
        {
            bool recordingStarted = false;
            string errorMessage = "";
            
            isButtonPressed = true;
            LogMessage("\n🔴 Recording started...");
            LogMessage("🎯 Press trigger/grip again to stop recording");

            // Start recording through speech service
            var speechService = pipelineManager.GetSpeechService();
            if (speechService != null)
            {
                speechService.StartRecording();
                recordingStarted = true;
            }
            else
            {
                errorMessage = "Speech service not available";
                recordingStarted = false;
            }
            
            // Add haptic feedback for VR
            if (useVRInput && recordingStarted)
            {
                TriggerHapticFeedback(0.1f, 0.5f);
            }
            
            // Handle error after yield
            if (!recordingStarted)
            {
                LogMessage($"❌ Failed to start recording: {errorMessage}");
                isButtonPressed = false;
            }
            
            yield return null;
        }

        IEnumerator StopRecordingAndProcessCoroutine()
        {
            if (isProcessing) yield break;

            isProcessing = true;
            isButtonPressed = false;
            bool processingSuccessful = false;
            string errorMessage = "";

            LogMessage("⏹️ Recording stopped");

            // Stop recording and get audio data
            var speechService = pipelineManager.GetSpeechService();
            if (speechService != null)
            {
                recordedAudio = speechService.StopRecording();
                processingSuccessful = true;
            }
            else
            {
                errorMessage = "Speech service not available";
                processingSuccessful = false;
            }

            if (!processingSuccessful)
            {
                LogMessage($"❌ Error stopping recording: {errorMessage}");
                LogMessage("\n🎯 Press controller button to try again...\n");
                isProcessing = false;
                recordedAudio = null;
                yield break;
            }

            if (recordedAudio == null || recordedAudio.Length == 0)
            {
                LogMessage("❌ No audio recorded. Try again.");
                LogMessage("\n🎯 Press controller button to start recording...\n");
                isProcessing = false;
                yield break;
            }

            LogMessage("🔄 Processing...");

            // Add haptic feedback for VR
            if (useVRInput)
            {
                TriggerHapticFeedback(0.2f, 0.3f);
            }

            // Process the recorded audio through the pipeline
            string result = "";
            yield return StartCoroutine(pipelineManager.ProcessVoiceInputCoroutine(recordedAudio, 
                (response) => result = response));

            // Handle results after processing
            if (result.StartsWith("ERROR:"))
            {
                LogMessage("\n🔄 Please try recording again");
                LogMessage("💡 Tips: Speak clearly in Spanish\n");
            }
            else
            {
                LogMessage("\n✅ Interaction complete!");
            }

            LogMessage("\n🎯 Press controller button to start new recording...\n");

            // Cleanup
            isProcessing = false;
            recordedAudio = null;
        }

        private void TriggerHapticFeedback(float amplitude, float duration)
        {
            if (!useVRInput) return;
            
            // Try right controller first
            if (rightController.isValid)
            {
                HapticCapabilities capabilities;
                if (rightController.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
                {
                    rightController.SendHapticImpulse(0, amplitude, duration);
                    return;
                }
            }
            
            // Try left controller
            if (leftController.isValid)
            {
                HapticCapabilities capabilities;
                if (leftController.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
                {
                    leftController.SendHapticImpulse(0, amplitude, duration);
                }
            }
        }

        public void SwitchController(bool useLeftHand)
        {
            LogMessage($"🎮 Primary controller set to {(useLeftHand ? "left" : "right")}");
        }

        private void LogMessage(string message)
        {
            Debug.Log(message);
            logCallback?.Invoke(message);
        }

        public void Dispose()
        {
            isRunning = false;
            StopAllCoroutines();
        }

        void OnDestroy()
        {
            Dispose();
        }
    }
}
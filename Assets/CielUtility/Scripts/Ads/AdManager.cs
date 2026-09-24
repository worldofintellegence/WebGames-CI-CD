using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CIEL.WebGames.Ads
{
    /// <summary>
    /// Cross-platform Ad Manager for Unity WebGL and Unity Editor.
    /// Handles Interstitial and Rewarded Ads with Google AdSense / H5 Ads bridge,
    /// automatic audio muting, time pausing, and callbacks.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class AdManager : MonoBehaviour
    {
        private static AdManager _instance;
        public static AdManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<AdManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("[AdManager]");
                        _instance = go.AddComponent<AdManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Ad Settings")]
        [Tooltip("Pause game time (Time.timeScale = 0) while an ad is active")]
        [SerializeField] private bool autoPauseGame = true;

        [Tooltip("Mute all audio (AudioListener.pause = true) while an ad is active")]
        [SerializeField] private bool autoMuteAudio = true;

        [Header("Editor Simulation")]
        [Tooltip("Simulated ad duration in seconds when running inside Unity Editor")]
        [SerializeField] private float simulatedEditorAdDuration = 3f;

        // State tracking
        private bool _isAdShowing = false;
        private float _previousTimeScale = 1f;
        private bool _previousAudioPause = false;

        // Active callbacks
        private Action _activeInterstitialCallback;
        private Action _activeRewardedSuccessCallback;
        private Action _activeRewardedFailedCallback;

        // Public Events
        public event Action<string> OnAdOpenedEvent;
        public event Action<string> OnAdClosedEvent;
        public event Action<string> OnInterstitialCompletedEvent;
        public event Action<string> OnRewardedSuccessEvent;
        public event Action<string> OnRewardedFailedEvent;

        public bool IsAdShowing => _isAdShowing;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void JS_InitAdBridge(string gameObjectName);

        [DllImport("__Internal")]
        private static extern void JS_ShowInterstitial(string placement);

        [DllImport("__Internal")]
        private static extern void JS_ShowRewarded(string placement);

        [DllImport("__Internal")]
        private static extern int JS_IsAdBlockerDetected();
#endif

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitBridge();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitBridge()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                JS_InitAdBridge(gameObject.name);
                Debug.Log($"[AdManager] WebGL Ad Bridge initialized for GameObject '{gameObject.name}'.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AdManager] Failed to initialize JS Ad Bridge: {ex.Message}");
            }
#else
            Debug.Log($"[AdManager] Initialized in Editor/Standalone mode (Simulation active).");
#endif
        }

        /// <summary>
        /// Request an Interstitial (full-screen break) ad.
        /// </summary>
        /// <param name="placement">Identifier for the ad location (e.g. 'level_complete', 'game_over')</param>
        /// <param name="onComplete">Callback executed after the ad finishes or fails</param>
        public void ShowInterstitial(string placement = "default", Action onComplete = null)
        {
            if (_isAdShowing)
            {
                Debug.LogWarning("[AdManager] An ad is already in progress. Ignoring request.");
                onComplete?.Invoke();
                return;
            }

            _activeInterstitialCallback = onComplete;
            HandleAdStart();

#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                JS_ShowInterstitial(placement);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AdManager] Error calling JS_ShowInterstitial: {ex.Message}");
                HandleAdEnd();
                _activeInterstitialCallback?.Invoke();
                _activeInterstitialCallback = null;
            }
#else
            StartCoroutine(SimulateEditorAd(placement, isRewarded: false));
#endif
        }

        /// <summary>
        /// Request a Rewarded Ad (offers player rewards upon completion).
        /// </summary>
        /// <param name="placement">Identifier for the reward (e.g. 'extra_life', 'double_coins')</param>
        /// <param name="onRewardSuccess">Callback executed when player finishes watching and earns reward</param>
        /// <param name="onRewardFailed">Callback executed if ad was skipped, dismissed early, or failed to load</param>
        public void ShowRewarded(string placement = "reward", Action onRewardSuccess = null, Action onRewardFailed = null)
        {
            if (_isAdShowing)
            {
                Debug.LogWarning("[AdManager] An ad is already in progress. Ignoring request.");
                onRewardFailed?.Invoke();
                return;
            }

            _activeRewardedSuccessCallback = onRewardSuccess;
            _activeRewardedFailedCallback = onRewardFailed;
            HandleAdStart();

#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                JS_ShowRewarded(placement);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AdManager] Error calling JS_ShowRewarded: {ex.Message}");
                HandleAdEnd();
                _activeRewardedFailedCallback?.Invoke();
                _activeRewardedSuccessCallback = null;
                _activeRewardedFailedCallback = null;
            }
#else
            StartCoroutine(SimulateEditorAd(placement, isRewarded: true));
#endif
        }

        /// <summary>
        /// Check if an AdBlocker is detected in the browser.
        /// </summary>
        public bool IsAdBlockerDetected()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                return JS_IsAdBlockerDetected() == 1;
            }
            catch
            {
                return false;
            }
#else
            return false;
#endif
        }

        #region Internal Audio & Time State Handling

        private void HandleAdStart()
        {
            _isAdShowing = true;

            if (autoPauseGame)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }

            if (autoMuteAudio)
            {
                _previousAudioPause = AudioListener.pause;
                AudioListener.pause = true;
            }
        }

        private void HandleAdEnd()
        {
            _isAdShowing = false;

            if (autoPauseGame)
            {
                Time.timeScale = _previousTimeScale;
            }

            if (autoMuteAudio)
            {
                AudioListener.pause = _previousAudioPause;
            }
        }

        #endregion

        #region Callbacks Received From WebGL JavaScript (unityInstance.SendMessage)

        /// <summary>
        /// Invoked by JS when an ad overlay or modal appears.
        /// </summary>
        public void OnAdOpened(string placement)
        {
            Debug.Log($"[AdManager] OnAdOpened: {placement}");
            OnAdOpenedEvent?.Invoke(placement);
        }

        /// <summary>
        /// Invoked by JS when an ad closes.
        /// </summary>
        public void OnAdClosed(string placement)
        {
            Debug.Log($"[AdManager] OnAdClosed: {placement}");
            HandleAdEnd();
            OnAdClosedEvent?.Invoke(placement);
        }

        /// <summary>
        /// Invoked by JS when an interstitial break is complete.
        /// </summary>
        public void OnInterstitialCompleted(string placement)
        {
            Debug.Log($"[AdManager] OnInterstitialCompleted: {placement}");
            HandleAdEnd();
            
            Action cb = _activeInterstitialCallback;
            _activeInterstitialCallback = null;
            cb?.Invoke();

            OnInterstitialCompletedEvent?.Invoke(placement);
        }

        /// <summary>
        /// Invoked by JS when a rewarded ad was successfully viewed.
        /// </summary>
        public void OnRewardedSuccess(string placement)
        {
            Debug.Log($"[AdManager] OnRewardedSuccess: {placement} -> Granting reward!");
            HandleAdEnd();

            Action successCb = _activeRewardedSuccessCallback;
            _activeRewardedSuccessCallback = null;
            _activeRewardedFailedCallback = null;
            successCb?.Invoke();

            OnRewardedSuccessEvent?.Invoke(placement);
        }

        /// <summary>
        /// Invoked by JS when a rewarded ad failed or was closed without completing.
        /// </summary>
        public void OnRewardedFailed(string placement)
        {
            Debug.LogWarning($"[AdManager] OnRewardedFailed: {placement}");
            HandleAdEnd();

            Action failCb = _activeRewardedFailedCallback;
            _activeRewardedSuccessCallback = null;
            _activeRewardedFailedCallback = null;
            failCb?.Invoke();

            OnRewardedFailedEvent?.Invoke(placement);
        }

        #endregion

        #region Editor Simulation

#if UNITY_EDITOR
        private IEnumerator SimulateEditorAd(string placement, bool isRewarded)
        {
            Debug.Log($"<color=cyan>[AdManager (Editor Simulation)]</color> Starting simulated {(isRewarded ? "Rewarded" : "Interstitial")} ad ('{placement}') for {simulatedEditorAdDuration:F1}s...");
            OnAdOpened(placement);

            // Use WaitForSecondsRealtime since timeScale may be 0
            yield return new WaitForSecondsRealtime(simulatedEditorAdDuration);

            OnAdClosed(placement);

            if (isRewarded)
            {
                OnRewardedSuccess(placement);
            }
            else
            {
                OnInterstitialCompleted(placement);
            }

            Debug.Log($"<color=green>[AdManager (Editor Simulation)]</color> Finished simulated ad ('{placement}').");
        }
#endif

        #endregion
    }
}

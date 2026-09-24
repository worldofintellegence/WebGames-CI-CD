using UnityEngine;

namespace CIEL.WebGames.Ads
{
    /// <summary>
    /// Interactive demonstration and testing UI for AdManager.
    /// Can be added to any GameObject in any scene. Renders an OnGUI testing dashboard
    /// and provides public methods for UI buttons.
    /// </summary>
    public class AdDemoUI : MonoBehaviour
    {
        [Header("Demo State")]
        [SerializeField] private int coins = 0;
        [SerializeField] private bool showOnGuiOverlay = true;

        private string _status = "Ready";
        private float _statusTimer = 0f;

        private void Start()
        {
            // Subscribe to AdManager events for logging / status display
            AdManager.Instance.OnAdOpenedEvent += (p) => SetStatus($"Ad Opened [{p}]");
            AdManager.Instance.OnAdClosedEvent += (p) => SetStatus($"Ad Closed [{p}]");
            AdManager.Instance.OnInterstitialCompletedEvent += (p) => SetStatus($"Interstitial Finished [{p}]");
            AdManager.Instance.OnRewardedSuccessEvent += (p) =>
            {
                coins += 50;
                SetStatus($"Rewarded Success! +50 Coins (Total: {coins})");
            };
            AdManager.Instance.OnRewardedFailedEvent += (p) => SetStatus($"Rewarded Ad Skipped/Failed [{p}]");
        }

        private void Update()
        {
            if (_statusTimer > 0f)
            {
                _statusTimer -= Time.unscaledDeltaTime;
                if (_statusTimer <= 0f)
                {
                    _status = "Ready";
                }
            }
        }

        private void SetStatus(string message)
        {
            _status = message;
            _statusTimer = 5f;
            Debug.Log($"[AdDemoUI] {message}");
        }

        /// <summary>
        /// Public method for triggering interstitial ad (can be wired to uGUI Button OnClick)
        /// </summary>
        public void TriggerInterstitial()
        {
            SetStatus("Requesting Interstitial Ad...");
            AdManager.Instance.ShowInterstitial("level_transition", () =>
            {
                SetStatus("Interstitial callback completed.");
            });
        }

        /// <summary>
        /// Public method for triggering rewarded ad (can be wired to uGUI Button OnClick)
        /// </summary>
        public void TriggerRewarded()
        {
            SetStatus("Requesting Rewarded Ad...");
            AdManager.Instance.ShowRewarded("double_reward",
                onRewardSuccess: () =>
                {
                    // Handled also in OnRewardedSuccess event
                },
                onRewardFailed: () =>
                {
                    SetStatus("User did not complete rewarded ad.");
                }
            );
        }

        private void OnGUI()
        {
            if (!showOnGuiOverlay) return;

            // Scaled high-DPI box
            float scale = Mathf.Max(1.0f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));

            GUILayout.BeginArea(new Rect(20, 20, 320, 280), GUI.skin.box);
            GUILayout.Label("<b>🎮 CIEL WebGL Ads Controller</b>", GUI.skin.label);
            GUILayout.Space(6);

            GUILayout.Label($"<b>Status:</b> {_status}");
            GUILayout.Label($"<b>Coins:</b> {coins} 🪙");
            GUILayout.Label($"<b>Time.timeScale:</b> {Time.timeScale:F1} | <b>Audio Muted:</b> {AudioListener.pause}");
            GUILayout.Label($"<b>Ad Active:</b> {(AdManager.Instance.IsAdShowing ? "<color=yellow>YES</color>" : "No")}");

            GUILayout.Space(10);

            GUI.enabled = !AdManager.Instance.IsAdShowing;

            if (GUILayout.Button("▶ Show Interstitial Ad (Level Break)", GUILayout.Height(34)))
            {
                TriggerInterstitial();
            }

            GUILayout.Space(4);

            if (GUILayout.Button("🎁 Show Rewarded Ad (+50 Coins)", GUILayout.Height(34)))
            {
                TriggerRewarded();
            }

            GUI.enabled = true;

            GUILayout.Space(6);
            if (GUILayout.Button("Check AdBlocker", GUILayout.Height(24)))
            {
                bool blocked = AdManager.Instance.IsAdBlockerDetected();
                SetStatus(blocked ? "AdBlocker Detected!" : "No AdBlocker Detected.");
            }

            GUILayout.EndArea();
        }
    }
}

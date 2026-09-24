using System.IO;
using UnityEditor;
using UnityEngine;

namespace CIEL.WebGames.Editor
{
    /// <summary>
    /// Interactive visual editor window for configuring game metadata, icons, screenshots,
    /// PlayerSettings, and ad parameters.
    /// </summary>
    public class GameConfigEditorWindow : EditorWindow
    {
        private GameConfigData _config;
        private Vector2 _scrollPos;
        private Texture2D _iconTexture;
        private int _selectedTab = 0;
        private readonly string[] _tabTitles = new string[] { "Game Identity", "Assets & Media", "Player Settings", "Ads & Monetization" };

        [MenuItem("Tools/WebGL Game Config/Open Config Manager Window", false, 0)]
        public static void OpenWindow()
        {
            GameConfigEditorWindow window = GetWindow<GameConfigEditorWindow>("Game Config Manager");
            window.minSize = new Vector2(480, 560);
            window.Show();
        }

        private void OnEnable()
        {
            LoadData();
        }

        private void LoadData()
        {
            _config = GameConfigManager.LoadConfig();
            if (!string.IsNullOrEmpty(_config.icon))
            {
                _iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(_config.icon);
            }
        }

        private void OnGUI()
        {
            if (_config == null)
            {
                LoadData();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🎮 CIEL WebGL Game Config Manager", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Configure game metadata, icons, player settings, and ads for multi-game repositories.", EditorStyles.miniLabel);
            EditorGUILayout.Space(6);

            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabTitles, GUILayout.Height(28));
            EditorGUILayout.Space(10);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            switch (_selectedTab)
            {
                case 0:
                    DrawIdentityTab();
                    break;
                case 1:
                    DrawAssetsTab();
                    break;
                case 2:
                    DrawPlayerSettingsTab();
                    break;
                case 3:
                    DrawAdsTab();
                    break;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(12);
            DrawBottomActions();
            EditorGUILayout.Space(6);
        }

        private void DrawIdentityTab()
        {
            EditorGUILayout.LabelField("Game Identity & Metadata", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("These details will be used in PlayerSettings, web browser title, and web game portals.", MessageType.Info);

            _config.gameId = EditorGUILayout.TextField("Game ID (Slug)", _config.gameId);
            _config.productName = EditorGUILayout.TextField("Product Name", _config.productName);
            _config.companyName = EditorGUILayout.TextField("Company Name", _config.companyName);
            _config.version = EditorGUILayout.TextField("Version", _config.version);
            _config.category = EditorGUILayout.TextField("Category", _config.category);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Description:");
            _config.description = EditorGUILayout.TextArea(_config.description, GUILayout.Height(60));

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Controls / Instructions:");
            _config.portalMetadata.controls = EditorGUILayout.TextArea(_config.portalMetadata.controls, GUILayout.Height(40));
        }

        private void DrawAssetsTab()
        {
            EditorGUILayout.LabelField("App Icon & Media", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Select or drag an app icon. It will be assigned to Unity WebGL PlayerSettings icon.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("App Icon (512x512)");
            Texture2D newIcon = (Texture2D)EditorGUILayout.ObjectField(_iconTexture, typeof(Texture2D), false, GUILayout.Width(72), GUILayout.Height(72));
            if (newIcon != _iconTexture)
            {
                _iconTexture = newIcon;
                if (_iconTexture != null)
                {
                    _config.icon = AssetDatabase.GetAssetPath(_iconTexture);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Icon Asset Path:", _config.icon, EditorStyles.miniLabel);

            EditorGUILayout.Space(14);
            EditorGUILayout.LabelField("Gameplay Screenshots:", EditorStyles.boldLabel);

            for (int i = 0; i < _config.screenshots.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _config.screenshots[i] = EditorGUILayout.TextField($"Screenshot #{i + 1}", _config.screenshots[i]);
                if (GUILayout.Button("✕", GUILayout.Width(26)))
                {
                    ArrayUtility.RemoveAt(ref _config.screenshots, i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Screenshot Path", GUILayout.Height(24)))
            {
                ArrayUtility.Add(ref _config.screenshots, "Assets/Screenshots/new_screenshot.png");
            }
        }

        private void DrawPlayerSettingsTab()
        {
            EditorGUILayout.LabelField("WebGL & Player Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Canvas resolution and WebGL presentation settings.", MessageType.Info);

            _config.playerSettings.defaultScreenWidth = EditorGUILayout.IntField("Canvas Width (px)", _config.playerSettings.defaultScreenWidth);
            _config.playerSettings.defaultScreenHeight = EditorGUILayout.IntField("Canvas Height (px)", _config.playerSettings.defaultScreenHeight);
            _config.playerSettings.runInBackground = EditorGUILayout.Toggle("Run In Background", _config.playerSettings.runInBackground);
            _config.playerSettings.webGLTemplate = EditorGUILayout.TextField("WebGL Template", _config.playerSettings.webGLTemplate);

            EditorGUILayout.Space(6);
            if (GUILayout.Button("Set Default WebGL Template (PROJECT:AdSupportedTemplate)"))
            {
                _config.playerSettings.webGLTemplate = "PROJECT:AdSupportedTemplate";
            }
        }

        private void DrawAdsTab()
        {
            EditorGUILayout.LabelField("Google Ads & Monetization Config", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Configure Google H5 Games Ads / Ad Placement API settings for this game.", MessageType.Info);

            _config.adsConfig.testMode = EditorGUILayout.Toggle("Test Mode Active", _config.adsConfig.testMode);
            _config.adsConfig.publisherId = EditorGUILayout.TextField("Publisher ID", _config.adsConfig.publisherId);
            _config.adsConfig.adFrequencyHint = EditorGUILayout.TextField("Frequency Hint", _config.adsConfig.adFrequencyHint);
            _config.adsConfig.interstitialDuration = EditorGUILayout.IntSlider("Interstitial Duration (s)", _config.adsConfig.interstitialDuration, 2, 10);
            _config.adsConfig.rewardedDuration = EditorGUILayout.IntSlider("Rewarded Duration (s)", _config.adsConfig.rewardedDuration, 3, 15);
        }

        private void DrawBottomActions()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
            if (GUILayout.Button("💾 Save Config to JSON", GUILayout.Height(36)))
            {
                GameConfigManager.SaveConfig(_config);
            }

            GUI.backgroundColor = new Color(0.3f, 0.7f, 1.0f);
            if (GUILayout.Button("⚡ Apply to PlayerSettings", GUILayout.Height(36)))
            {
                GameConfigManager.SaveConfig(_config);
                GameConfigManager.ApplyConfigToPlayerSettings();
            }

            GUI.backgroundColor = Color.white;
            if (GUILayout.Button("🔄 Pull Current Settings", GUILayout.Height(36)))
            {
                if (EditorUtility.DisplayDialog("Pull PlayerSettings?",
                    "This will overwrite product name, version, and screen size in the current configuration with current Unity PlayerSettings. Continue?",
                    "Yes", "Cancel"))
                {
                    GameConfigManager.PullPlayerSettingsIntoConfig();
                    LoadData();
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CIEL.WebGames.Editor
{
    [Serializable]
    public class GameConfigData
    {
        public string gameId = "ciel-minigame-template";
        public string productName = "CIEL Web Game";
        public string companyName = "CIEL Tech";
        public string version = "1.0.0";
        public string bundleVersion = "1";
        public string description = "High-performance WebGL mini-game template with Google Ads, responsive layout, and two-way browser communication.";
        public string category = "Arcade";
        public string[] tags = new string[] { "webgl", "html5", "unity", "arcade", "casual" };
        public string icon = "Assets/Icons/app-icon.png";
        public string[] screenshots = new string[] { "Assets/Screenshots/screenshot1.png", "Assets/Screenshots/screenshot2.png" };
        public PlayerSettingsConfig playerSettings = new PlayerSettingsConfig();
        public AdsConfigData adsConfig = new AdsConfigData();
        public PortalMetadata portalMetadata = new PortalMetadata();
    }

    [Serializable]
    public class PlayerSettingsConfig
    {
        public int defaultScreenWidth = 960;
        public int defaultScreenHeight = 600;
        public bool runInBackground = true;
        public string webGLTemplate = "PROJECT:AdSupportedTemplate";
        public int webGLCompressionFormat = 0;
        public string defaultWebScreenOrientation = "Landscape";
    }

    [Serializable]
    public class AdsConfigData
    {
        public bool testMode = true;
        public string publisherId = "ca-pub-0000000000000000";
        public string adFrequencyHint = "30s";
        public int interstitialDuration = 4;
        public int rewardedDuration = 5;
    }

    [Serializable]
    public class PortalMetadata
    {
        public string developer = "CIEL Tech";
        public string releaseDate = "2026-09-15";
        public string rating = "Everyone";
        public string controls = "Desktop: Keyboard / Mouse. Mobile: Touch.";
    }

    /// <summary>
    /// Core manager for loading, saving, and syncing game configuration with Unity PlayerSettings.
    /// </summary>
    public static class GameConfigManager
    {
        public const string RootConfigPath = "game-config.json";
        public const string StreamingAssetsConfigPath = "Assets/StreamingAssets/game-config.json";
        public const string TemplateConfigPath = "Assets/WebGLTemplates/AdSupportedTemplate/TemplateData/game-config.json";

        /// <summary>
        /// Loads the game configuration from root game-config.json
        /// </summary>
        public static GameConfigData LoadConfig()
        {
            if (!File.Exists(RootConfigPath))
            {
                Debug.LogWarning($"[GameConfigManager] '{RootConfigPath}' not found. Creating default configuration.");
                GameConfigData defaultConfig = new GameConfigData();
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            try
            {
                string json = File.ReadAllText(RootConfigPath);
                GameConfigData data = JsonUtility.FromJson<GameConfigData>(json);
                return data ?? new GameConfigData();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameConfigManager] Failed to parse {RootConfigPath}: {ex.Message}");
                return new GameConfigData();
            }
        }

        /// <summary>
        /// Saves the game configuration to root, StreamingAssets, and WebGL template.
        /// </summary>
        public static void SaveConfig(GameConfigData config)
        {
            try
            {
                string json = JsonUtility.ToJson(config, prettyPrint: true);

                // 1. Save to Project Root
                File.WriteAllText(RootConfigPath, json);

                // 2. Save to StreamingAssets (for in-game runtime read)
                string streamingDir = Path.GetDirectoryName(StreamingAssetsConfigPath);
                if (!string.IsNullOrEmpty(streamingDir) && !Directory.Exists(streamingDir))
                {
                    Directory.CreateDirectory(streamingDir);
                }
                File.WriteAllText(StreamingAssetsConfigPath, json);

                // 3. Save to WebGL TemplateData (for web portal read)
                string templateDir = Path.GetDirectoryName(TemplateConfigPath);
                if (!string.IsNullOrEmpty(templateDir) && !Directory.Exists(templateDir))
                {
                    Directory.CreateDirectory(templateDir);
                }
                File.WriteAllText(TemplateConfigPath, json);

                AssetDatabase.Refresh();
                Debug.Log($"<color=green>[GameConfigManager]</color> Successfully saved game configuration across project locations.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameConfigManager] Failed to save config: {ex.Message}");
            }
        }

        /// <summary>
        /// Reads game-config.json and applies all settings directly into Unity's PlayerSettings.
        /// </summary>
        [MenuItem("Tools/WebGL Game Config/Apply Config to PlayerSettings", false, 1)]
        public static void ApplyConfigToPlayerSettings()
        {
            GameConfigData config = LoadConfig();

            // Product & Company
            PlayerSettings.productName = config.productName;
            PlayerSettings.companyName = config.companyName;
            PlayerSettings.bundleVersion = config.version;

            // Screen & Presentation
            PlayerSettings.defaultWebScreenWidth = config.playerSettings.defaultScreenWidth;
            PlayerSettings.defaultWebScreenHeight = config.playerSettings.defaultScreenHeight;
            PlayerSettings.runInBackground = config.playerSettings.runInBackground;

            // WebGL Template
            if (!string.IsNullOrEmpty(config.playerSettings.webGLTemplate))
            {
                PlayerSettings.WebGL.template = config.playerSettings.webGLTemplate;
            }

            // WebGL Compression
            PlayerSettings.WebGL.compressionFormat = (WebGLCompressionFormat)config.playerSettings.webGLCompressionFormat;

            // App Icon
            if (!string.IsNullOrEmpty(config.icon))
            {
                Texture2D iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(config.icon);
                if (iconTexture != null)
                {
                    try
                    {
                        NamedBuildTarget webglTarget = NamedBuildTarget.WebGL;
                        PlayerSettings.SetIcons(webglTarget, new Texture2D[] { iconTexture }, IconKind.Application);
                        Debug.Log($"<color=green>[GameConfigManager]</color> Applied icon from: {config.icon}");
                    }
                    catch
                    {
                        // Fallback for older API
                        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.WebGL, new Texture2D[] { iconTexture });
                    }
                }
                else
                {
                    Debug.LogWarning($"[GameConfigManager] Icon texture not found at '{config.icon}'.");
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"<color=green>[GameConfigManager]</color> Successfully applied '{config.productName}' settings to Unity PlayerSettings!");
            EditorUtility.DisplayDialog("Config Applied",
                $"Successfully applied '{config.productName}' (v{config.version}) to Unity PlayerSettings!\n\nResolution: {config.playerSettings.defaultScreenWidth}x{config.playerSettings.defaultScreenHeight}\nTemplate: {PlayerSettings.WebGL.template}",
                "OK");
        }

        /// <summary>
        /// Reads current Unity PlayerSettings and updates game-config.json with them.
        /// </summary>
        [MenuItem("Tools/WebGL Game Config/Pull PlayerSettings into Config JSON", false, 2)]
        public static void PullPlayerSettingsIntoConfig()
        {
            GameConfigData config = LoadConfig();

            config.productName = PlayerSettings.productName;
            config.companyName = PlayerSettings.companyName;
            config.version = PlayerSettings.bundleVersion;

            config.playerSettings.defaultScreenWidth = PlayerSettings.defaultWebScreenWidth;
            config.playerSettings.defaultScreenHeight = PlayerSettings.defaultWebScreenHeight;
            config.playerSettings.runInBackground = PlayerSettings.runInBackground;
            config.playerSettings.webGLTemplate = PlayerSettings.WebGL.template;
            config.playerSettings.webGLCompressionFormat = (int)PlayerSettings.WebGL.compressionFormat;

            SaveConfig(config);
            Debug.Log($"<color=green>[GameConfigManager]</color> Updated game-config.json with current PlayerSettings.");
        }

        /// <summary>
        /// Command-line & Editor Menu helper to build the WebGL project to Builds/WebGL
        /// </summary>
        [MenuItem("Tools/WebGL Game Config/Build WebGL Project", false, 3)]
        public static void BuildWebGLProject()
        {
            ApplyConfigToPlayerSettingsSilent();

            string buildPath = "build/WebGL";
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-customBuildPath" && i + 1 < args.Length)
                {
                    buildPath = args[i + 1];
                    break;
                }
            }

            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }

            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[GameConfigManager] No enabled scenes found in Build Settings!");
                throw new Exception("[GameConfigManager] No enabled scenes found in Build Settings!");
            }

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            Debug.Log($"<color=cyan>[GameConfigManager]</color> Starting WebGL build to target directory '{buildPath}'...");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                // Auto-create .nojekyll file for GitHub Pages compatibility
                string noJekyllPath = Path.Combine(buildPath, ".nojekyll");
                File.WriteAllText(noJekyllPath, "");

                // Auto-create .gitignore to un-ignore Build/ folder
                string gitIgnorePath = Path.Combine(buildPath, ".gitignore");
                File.WriteAllText(gitIgnorePath, "!Build/\n!Build/*\n!Build/**\n");

                Debug.Log($"<color=green>[GameConfigManager]</color> WebGL Build succeeded! Created .nojekyll and .gitignore for GitHub Pages. Total Size: {summary.totalSize} bytes. Location: {buildPath}");
            }
            else
            {
                Debug.LogError($"[GameConfigManager] WebGL Build failed with result: {summary.result}");
                throw new Exception($"[GameConfigManager] WebGL Build failed with result: {summary.result}");
            }
        }

        public static void ApplyConfigToPlayerSettingsSilent()
        {
            GameConfigData config = LoadConfig();

            PlayerSettings.productName = config.productName;
            PlayerSettings.companyName = config.companyName;
            PlayerSettings.bundleVersion = config.version;

            PlayerSettings.defaultWebScreenWidth = config.playerSettings.defaultScreenWidth;
            PlayerSettings.defaultWebScreenHeight = config.playerSettings.defaultScreenHeight;
            PlayerSettings.runInBackground = config.playerSettings.runInBackground;

            if (!string.IsNullOrEmpty(config.playerSettings.webGLTemplate))
            {
                PlayerSettings.WebGL.template = config.playerSettings.webGLTemplate;
            }

            PlayerSettings.WebGL.compressionFormat = (WebGLCompressionFormat)config.playerSettings.webGLCompressionFormat;

            if (!string.IsNullOrEmpty(config.icon))
            {
                Texture2D iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(config.icon);
                if (iconTexture != null)
                {
                    try
                    {
                        NamedBuildTarget webglTarget = NamedBuildTarget.WebGL;
                        PlayerSettings.SetIcons(webglTarget, new Texture2D[] { iconTexture }, IconKind.Application);
                    }
                    catch
                    {
                        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.WebGL, new Texture2D[] { iconTexture });
                    }
                }
            }

            AssetDatabase.SaveAssets();
        }
    }
}

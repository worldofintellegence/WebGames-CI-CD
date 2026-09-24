using UnityEditor;
using UnityEngine;
using CIEL.WebGames.Ads;

namespace CIEL.WebGames.Editor
{
    public static class WebGLTemplateSelector
    {
        private const string TemplateName = "PROJECT:AdSupportedTemplate";

        [MenuItem("Tools/WebGL Ads/Apply AdSupported Template", false, 1)]
        public static void ApplyTemplate()
        {
            PlayerSettings.WebGL.template = TemplateName;
            AssetDatabase.SaveAssets();
            Debug.Log($"<color=green>[WebGL Ads]</color> Successfully applied WebGL Template: <b>{TemplateName}</b>");
            EditorUtility.DisplayDialog("WebGL Template Applied",
                $"WebGL Template successfully set to '{TemplateName}'.\n\nWhen building for WebGL, Unity will now use your AdSupportedTemplate!",
                "OK");
        }

        [MenuItem("Tools/WebGL Ads/Add AdManager & Demo UI to Current Scene", false, 2)]
        public static void AddAdManagerToScene()
        {
            AdManager manager = Object.FindFirstObjectByType<AdManager>();
            if (manager == null)
            {
                GameObject go = new GameObject("[AdManager]");
                go.AddComponent<AdManager>();
                go.AddComponent<AdDemoUI>();
                Undo.RegisterCreatedObjectUndo(go, "Create AdManager & Demo UI");
                Selection.activeGameObject = go;
                Debug.Log("<color=green>[WebGL Ads]</color> Created [AdManager] with AdDemoUI in current scene!");
            }
            else
            {
                if (manager.GetComponent<AdDemoUI>() == null)
                {
                    Undo.AddComponent<AdDemoUI>(manager.gameObject);
                }
                Selection.activeGameObject = manager.gameObject;
                Debug.Log("<color=yellow>[WebGL Ads]</color> [AdManager] already exists in scene. Selected it in hierarchy.");
            }
        }
    }
}

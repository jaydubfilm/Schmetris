using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.ScriptableObjects.Analytics
{
    //[CreateAssetMenu(fileName = "Game Summary Settings", menuName = "Star Salvager/Scriptable Objects/Game Summary Settings")]
    public class GameSummarySettingsScriptableObject : ScriptableObject
    {
        [FolderPath]
        public string SessionsDirectory;
    }

#if UNITY_EDITOR

    namespace StarSalvager.ScriptableObjects.Analytics.Editor
    {
        using UnityEditor;

        public static class GameSummarySettings
        {
            [MenuItem("Star Salvager/Scriptable Objects/Game Summary Settings")]
            public static void CreateMyAsset()
            {
                GameSummarySettingsScriptableObject asset =
                    ScriptableObject.CreateInstance<GameSummarySettingsScriptableObject>();

                asset.SessionsDirectory =
                    Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName, "RemoteData");

                AssetDatabase.CreateAsset(asset, "Assets/Scriptable Objects/Game Summary Settings.asset");
                AssetDatabase.SaveAssets();

                EditorUtility.FocusProjectWindow();

                Selection.activeObject = asset;
            }
        }
    }

#endif

}


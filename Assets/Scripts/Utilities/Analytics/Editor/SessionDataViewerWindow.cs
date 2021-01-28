using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector.Editor;
using StarSalvager.Factories;
using StarSalvager.ScriptableObjects.Analytics;
using StarSalvager.ScriptableObjects.Analytics.StarSalvager.ScriptableObjects.Analytics.Editor;
using StarSalvager.Utilities.Analytics.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Utilities.Analytics.Editor
{
    public class SessionDataViewerWindow : OdinMenuEditorWindow
    {
        
        private Dictionary<string, List<SessionData>> _playerSessions;

        private GameSummarySettingsScriptableObject _summarySettingsScriptableObject;
        
        [MenuItem("Window/Star Salvager/Review Sessions")]
        private static void OpenWindow()
        {
            GetWindow<SessionDataViewerWindow>().Show();
            
            
            
        }

        private void FindSettingsAsset()
        {
            string[] guids = AssetDatabase.FindAssets("Game Summary Settings t:GameSummarySettingsScriptableObject", new[] {"Assets/Scriptable Objects"});

            if (guids.Length <= 0)
            {
                GameSummarySettings.CreateMyAsset();
                
                guids = AssetDatabase.FindAssets("Game Summary Settings t:GameSummarySettingsScriptableObject", new[] {"Assets/Scriptable Objects"});
                
                if (guids.Length <= 0)
                    throw new FileLoadException("Cannot find the Game Summary Settings File");
            }

            _summarySettingsScriptableObject = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]),
                typeof(GameSummarySettingsScriptableObject)) as GameSummarySettingsScriptableObject ;

        }

        protected override OdinMenuTree BuildMenuTree()
        {
            FindSettingsAsset();
                
            var tree = new OdinMenuTree();
            tree.Selection.SupportsMultiSelect = false;
            
            var directory = new DirectoryInfo(_summarySettingsScriptableObject.SessionsDirectory);

            var files = directory.GetFiles("*.session");
            
            _playerSessions = new Dictionary<string, List<SessionData>>();

            foreach (var file in files)
            {
                var jsonData = File.ReadAllText(file.FullName);

                var sessionData = Newtonsoft.Json.JsonConvert.DeserializeObject<SessionData>(jsonData);

                //Don't want to add any sessions that are no longer supported
                if (sessionData.Version != SessionDataProcessor.VERSION)
                    continue;
                
            
                if(!_playerSessions.ContainsKey(sessionData.PlayerID))
                    _playerSessions.Add(sessionData.PlayerID, new List<SessionData>());
                
                _playerSessions[sessionData.PlayerID].Add(sessionData);
            }
            
            tree.Add("Settings", _summarySettingsScriptableObject);
            
            tree.Add("Total Summary", new SessionSummaryData("Total Summary", _playerSessions.Values));

            foreach (var playerSession in _playerSessions)
            {
                //TODO Need to get the player summary data here
                
                tree.Add($"{playerSession.Key}", new SessionSummaryData("Player Summary", playerSession.Value));

                for (var i = 0; i < playerSession.Value.Count; i++)
                {
                    var sessionData = playerSession.Value[i];
                    var sessionSummary = sessionData.GetSessionSummary();

                    var sessionDateName = sessionData.date.ToString("ddd, MMM d, yyyy");


                    tree.Add($"{playerSession.Key}/Session {i + 1}", sessionSummary);

                    for (var index = 0; index < sessionData.waves.Count; index++)
                    {
                        var wave = sessionData.waves[index];
                        /*tree.Add(
                            $"{playerSession.Key}/{sessionDateName}/Session {i + 1}/Sector {wave.sectorNumber + 1} Wave {wave.waveNumber + 1}[{index}]",
                            wave);*/
                        tree.Add(
                            $"{playerSession.Key}/Session {i + 1}/Sector {wave.sectorNumber + 1} Wave {wave.waveNumber + 1}[{index}]",
                            wave);
                    }
                }
            }

            return tree;
        }
    }
    

    
    public class BlockDataListDrawer : OdinValueDrawer<List<BlockData>>
    {
        private const float BRICK_SIZE = 32;
        private const float PREVIEW_HEIGHT = 256;
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, PREVIEW_HEIGHT);
            rect.width = rect.height = PREVIEW_HEIGHT;
            
            Vector2 center = new Vector2(rect.x + rect.width / 2f, rect.y + rect.height / 2f);
            

            //if (label != null)
            //{
            //    rect = EditorGUI.PrefixLabel(rect, label);
            //}
            
            EditorGUI.DrawRect(rect, new Color(0.17f, 0.17f, 0.17f));

            if (label != null)
            {
                var newStyle = new GUIStyle(EditorStyles.label) {normal = {textColor = Color.white}};
                var labelRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                GUI.contentColor = Color.white;
                EditorGUI.LabelField(labelRect, label, newStyle);
            }

            var blockDataList = this.ValueEntry.SmartValue;
            foreach (var blockData in blockDataList)
            {
                var sprite = GetSprite(blockData);

                var imageCenter = center + CoordinateToPosition(blockData.Coordinate) - Vector2.one * (BRICK_SIZE/2);
                var imageRect = new Rect(imageCenter.x, imageCenter.y, BRICK_SIZE, BRICK_SIZE);
                
                DrawTexturePreview(imageRect, sprite);
            }


            //this.ValueEntry.SmartValue = blockDataList;
        }
        
        private static void DrawTexturePreview(Rect position, Sprite sprite)
        {
            Vector2 fullSize = new Vector2(sprite.texture.width, sprite.texture.height);
            Vector2 size = new Vector2(sprite.textureRect.width, sprite.textureRect.height);
 
            Rect coords = sprite.textureRect;
            coords.x /= fullSize.x;
            coords.width /= fullSize.x;
            coords.y /= fullSize.y;
            coords.height /= fullSize.y;
 
            Vector2 ratio;
            ratio.x = position.width / size.x;
            ratio.y = position.height / size.y;
            float minRatio = Mathf.Min(ratio.x, ratio.y);
 
            Vector2 center = position.center;
            position.width = size.x * minRatio;
            position.height = size.y * minRatio;
            position.center = center;
 
            GUI.DrawTextureWithTexCoords(position, sprite.texture, coords);
        }
        private static Sprite GetSprite(BlockData blockData)
        {
            switch (blockData.ClassType)
            {
                case nameof(ScrapyardBit):
                case nameof(Bit):
                    return Object.FindObjectOfType<FactoryManager>().BitProfileData.GetProfile((BIT_TYPE)blockData.Type).GetSprite(blockData.Level);
                case nameof(Component):
                    return Object.FindObjectOfType<FactoryManager>().componentSprite;
                case nameof(ScrapyardPart):
                case nameof(Part):
                    return Object.FindObjectOfType<FactoryManager>().PartsProfileData.GetProfile((PART_TYPE)blockData.Type).GetSprite(blockData.Level);
                default:
                    throw new ArgumentOutOfRangeException(nameof(blockData.ClassType), blockData.ClassType, null);
            }
        }

        private static Vector2 CoordinateToPosition(Vector2Int coordinate)
        {
            return new Vector2(coordinate.x * BRICK_SIZE, coordinate.y * BRICK_SIZE);
        }
    }


}
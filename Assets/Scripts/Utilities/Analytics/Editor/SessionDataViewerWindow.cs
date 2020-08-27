using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEditor;
using UnityEngine;

namespace StarSalvager.Utilities.Analytics.Editor
{
    public class SessionDataViewerWindow : OdinMenuEditorWindow
    {

        private SessionData _sessionData;
        
        [MenuItem("Window/Star Salvager/Review Sessions")]
        private static void OpenWindow()
        {
            GetWindow<SessionDataViewerWindow>().Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            tree.Selection.SupportsMultiSelect = false;

            var path = Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName, "RemoteData",
                $"Test_Session_{0}.txt");
            var jsonData = File.ReadAllText(path);

            _sessionData = Newtonsoft.Json.JsonConvert.DeserializeObject<SessionData>(jsonData);

            
            
            tree.Add("Session 0", null);

            for (var index = _sessionData.waves.Count - 1; index >= 0 ; index--)
            {
                var wave = _sessionData.waves[index];
                tree.Add($"Session 0/Sector {wave.sectorNumber + 1} Wave {wave.waveNumber + 1}", wave);
            }

            //tree.Add("Settings", GeneralDrawerConfig.Instance);
            //tree.Add("Utilities", new TextureUtilityEditor());
            //tree.AddAllAssetsAtPath("Odin Settings", "Assets/Plugins/Sirenix", typeof(ScriptableObject), true, true);
            return tree;
        }
    }
    
    public class MyStructDrawer : OdinValueDrawer<List<BlockData>>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            rect.height = 256;
            rect.width = 256;
            

            //if (label != null)
            //{
            //    rect = EditorGUI.PrefixLabel(rect, label);
            //}

            var value = this.ValueEntry.SmartValue;
            var sprite = Object.FindObjectOfType<FactoryManager>().PartsProfileData.GetProfile(PART_TYPE.CORE).GetSprite(0);
            foreach (var data in value)
            {
                var pos = new Rect(rect.x + 256 / 2f, rect.y + 256 / 2f, 32f, 32f);
                DrawTexturePreview(pos, sprite);
            }


            this.ValueEntry.SmartValue = value;
        }
        
        private void DrawTexturePreview(Rect position, Sprite sprite)
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
    }

}
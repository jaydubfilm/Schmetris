using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using StarSalvager.Editor.CustomEditors;
using StarSalvager.Factories;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarSalvager.Editor
{
    public class SpriteAtlasEditorWindow : OdinEditorWindow
    {
        [Serializable]
        public struct PartAtlasData
        {
            public PART_TYPE type;
            public string name;

            [ValueDropdown("GetFolders"), OnValueChanged("SetSprites")]
            public string selectedVersion;

            [ReadOnly, HideIf("NoPath")] 
            public Texture2D[] sprites;



            public ValueDropdownList<string> GetFolders()
            {
                var folder = Path.Combine(_spriteAtlasSettings.partSpritePath, type.ToString());
                var subDirectories = new DirectoryInfo(folder).GetDirectories();

                var valueDropdownList = new ValueDropdownList<string>();

                foreach (var subDirectory in subDirectories)
                {
                    valueDropdownList.Add(new ValueDropdownItem<string>
                    {
                        Text = subDirectory.Name,
                        Value = subDirectory.FullName
                    });
                }
                

                return valueDropdownList;
            }

            private void SetSprites()
            {
                if (string.IsNullOrEmpty(selectedVersion))
                    return;

                var textures = new List<Texture2D>();
                var files = new DirectoryInfo(selectedVersion).GetFiles("*.png");

                foreach (var fileInfo in files)
                {
                    var path = fileInfo.FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets");
                    
                    var texture =  (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
                    
                    if(texture is null)
                        continue;
                    
                    textures.Add(texture);
                }

                sprites = textures.ToArray();
            }

            private bool NoPath()
            {
                return string.IsNullOrEmpty(selectedVersion);
            }
        }
        
        
        private static SpriteAtlasSettingsScriptableObject _spriteAtlasSettings;
        private static SpriteAtlasEditorWindow _window;
        
        [MenuItem("Window/Star Salvager/Sprite Atlas Editor")]
        public static void BulkPartCostEditor()
        {
            _window = GetWindow<SpriteAtlasEditorWindow>("Sprite Atlas Editor", true);
            _window.Show();

            var settings = AssetDatabase.LoadAssetAtPath<SpriteAtlasSettingsScriptableObject>("Assets/Editor/Sprite Atlas Settings.asset");

            _spriteAtlasSettings = settings
                ? settings
                : throw new NotImplementedException("Need to create the Settings asset if none is present");

            _window.partAtlasDatas = _window.SetupPartList();

            //_window._partCostDatas = ToPartCostDataList(FindObjectOfType<FactoryManager>().PartsRemoteData.partRemoteData);

            /*if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(Texture2D))
                spriteTools.spritesheet = (Texture2D) Selection.activeObject;*/
        }

        [SerializeField]
        private List<PartAtlasData> partAtlasDatas;

        private List<PartAtlasData> SetupPartList()
        {
            var outList = new List<PartAtlasData>();
            


            foreach (PART_TYPE part in Enum.GetValues(typeof(PART_TYPE)))
            {
                var path = Path.Combine(_spriteAtlasSettings.partSpritePath, part.ToString());
                if (!Directory.Exists(path))
                    continue;
                
                outList.Add(new PartAtlasData
                {
                    type = part,
                    name = part.ToString()
                });
            }


            return outList;
        }
    }
}

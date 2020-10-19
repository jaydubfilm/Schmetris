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

namespace StarSalvager.Editor
{
    public class SpriteAtlasEditorWindow : OdinEditorWindow
    {
        [Serializable]
        private class PartAtlasData : AtlasDataBase<PART_TYPE>
        {
            protected override string GetName()
            {
                var factory = FindObjectOfType<FactoryManager>();

                if (factory is null)
                    return type.ToString();

                var remoteData = factory.PartsRemoteData.GetRemoteData(type);

                return remoteData is null ?type.ToString(): remoteData.name;

            }
        }
        
        [Serializable]
        private class BitAtlasData : AtlasDataBase<BIT_TYPE>
        {
            protected override string GetName()
            {
                var factory = FindObjectOfType<FactoryManager>();

                if (factory is null)
                    return type.ToString();

                var remoteData = factory.BitsRemoteData.GetRemoteData(type);

                return remoteData is null ?type.ToString(): remoteData.name;

            }
        }

        private class ComponentAtlasData : AtlasDataBase<COMPONENT_TYPE>
        {
            protected override string GetName()
            {
                var factory = FindObjectOfType<FactoryManager>();

                if (factory is null)
                    return type.ToString();

                var remoteData = factory.componentRemoteData.GetRemoteData(type);

                return remoteData is null ? type.ToString() : remoteData.name;
            }
        }

        //SpriteAtlasEditor Properties
        //====================================================================================================================//
        
        internal static SpriteAtlasSettingsScriptableObject SpriteAtlasSettings
        {
            get
            {
                if (_spriteAtlasSettings)
                    return _spriteAtlasSettings;
                
                var settings = AssetDatabase.LoadAssetAtPath<SpriteAtlasSettingsScriptableObject>("Assets/Editor/Sprite Atlas Settings.asset");

                _spriteAtlasSettings = settings
                    ? settings
                    : throw new NotImplementedException("Need to create the Settings asset if none is present");

                return _spriteAtlasSettings;
            }
        }
        private static SpriteAtlasSettingsScriptableObject _spriteAtlasSettings;
        
        private static SpriteAtlasEditorWindow _window;

        //SpriteAtlasEditor Functions
        //====================================================================================================================//
        
        [MenuItem("Window/Star Salvager/Sprite Atlas Editor")]
        public static void BulkPartCostEditor()
        {
            _window = GetWindow<SpriteAtlasEditorWindow>("Sprite Atlas Editor", true);
            _window.Show();

            

            _window.SetupLists();

        }

        [InfoBox("This Tool is still a work in progress", InfoMessageType.Error), DisplayAsString]
        public string warning = "Warning";
        
        
        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, DrawScrollView = false), FoldoutGroup("Parts")]
        private List<PartAtlasData> partAtlasDatas;
        
        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, DrawScrollView = false), FoldoutGroup("Bits")]
        private List<BitAtlasData> bitAtlasDatas;

        private void SetupLists()
        {
            partAtlasDatas = SetupList<PartAtlasData, PART_TYPE>();
            bitAtlasDatas = SetupList<BitAtlasData, BIT_TYPE>();
        }
        
        private static List<T> SetupList<T, TE>() where TE: Enum where T: AtlasDataBase<TE>, new()
        {
            var outList = new List<T>();
            
            //TODO Need to load the saved data

            foreach (TE type in Enum.GetValues(typeof(TE)))
            {
                string path = SpriteAtlasSettings.GetTypePath(type);
                
                if (!Directory.Exists(path))
                    continue;
                
                outList.Add(new T
                {
                    type = type,
                    selectedVersion = SpriteAtlasSettings.GetDataPath(type)
                });
            }

            return outList;
        }
        
        
        [Button]
        private void SaveChanges()
        {
            int index;
            var factoryManager = FindObjectOfType<FactoryManager>();
            
            foreach (var partAtlasData in partAtlasDatas)
            {
                SpriteAtlasSettings.UpdatePath(partAtlasData.type, partAtlasData.selectedVersion);

                index = factoryManager.PartsProfileData.GetProfileIndex(partAtlasData.type);
                if (index < 0) 
                    continue;

                var profile = factoryManager.PartsProfileData.profiles[index];
                partAtlasData.Sprites.CopyTo(profile.Sprites, 0);

                factoryManager.PartsProfileData.profiles[index] = profile;
            }
            
            foreach (var bitAtlasData in bitAtlasDatas)
            {
                SpriteAtlasSettings.UpdatePath(bitAtlasData.type, bitAtlasData.selectedVersion);
            }
            //TODO Need to update the Part/Bit/Component Profiles
            //TODO Need to Update the Part/Bit/Component Atlases
            
            EditorUtility.SetDirty(factoryManager.PartsProfileData);
            EditorUtility.SetDirty(SpriteAtlasSettings);
            AssetDatabase.SaveAssets();
        }

        //====================================================================================================================//
        
    }
    
    [Serializable]
    public abstract class AtlasDataBase<TE> where TE: Enum
    {
        [HideInInspector]
        public TE type;

        [ShowInInspector, DisplayAsString, PropertyOrder(-10)]
        public string name => GetName();

        [ValueDropdown("GetFolders"), OnValueChanged("SetSprites")]
        public string selectedVersion;

        [HideInInspector]
        public Texture2D[] textures;
        [HideInInspector]
        public Sprite[] Sprites;
        
        [OnInspectorGUI]
        [HorizontalGroup("Sprites"), HideIf("NoPath"), TableColumnWidth(300)]
        private void ShowImage()
        {
            if(textures == null)
                SetSprites();
            
            
            GUILayout.BeginHorizontal();
            foreach (var sprite in textures)
            {
                GUILayout.Label(sprite, GUILayout.Height(50), GUILayout.Width(50));
            }
            GUILayout.EndHorizontal();
            //GUILayout.Label(AssetDatabase.LoadAssetAtPath("Assets/Plugins/Sirenix/Assets/Editor/Odin Inspector Logo.png"));
        }


        public ValueDropdownList<string> GetFolders()
        {
            var folder = SpriteAtlasEditorWindow.SpriteAtlasSettings.GetTypePath(type);
            var subDirectories = new DirectoryInfo(folder).GetDirectories();

            var valueDropdownList = new ValueDropdownList<string>();

            foreach (var subDirectory in subDirectories)
            {
                var value = Path.Combine(folder, subDirectory.Name).Replace("\\","/");
                valueDropdownList.Add(new ValueDropdownItem<string>
                {
                    Text = subDirectory.Name,
                    Value = value
                });
            }
            

            return valueDropdownList;
        }

        private void SetSprites()
        {
            if (string.IsNullOrEmpty(selectedVersion))
                return;

            var textures = new List<Texture2D>();
            var sprites = new List<Sprite>();
            var files = new DirectoryInfo(selectedVersion).GetFiles("*.png");
            
            

            foreach (var fileInfo in files)
            {
                var path = fileInfo.FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets");
                
                var texture =  (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
                
                if(texture is null)
                    continue;

                var sprite = (Sprite) AssetDatabase.LoadAssetAtPath(path, typeof(Sprite));
                sprites.Add(sprite);
                textures.Add(texture);
            }

            this.textures = textures.ToArray();
            Sprites = sprites.ToArray();
        }

        private bool NoPath()
        {
            return string.IsNullOrEmpty(selectedVersion);
        }

        protected abstract string GetName();
    }
        
        
}

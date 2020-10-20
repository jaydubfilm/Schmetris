using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using StarSalvager.Editor.CustomEditors;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

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
        [Serializable]
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
        
        [SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true, DrawScrollView = false), FoldoutGroup("Bits")]
        private List<ComponentAtlasData> componentAtlasDatas;

        private void SetupLists()
        {
            partAtlasDatas = SetupList<PartAtlasData, PART_TYPE>();
            bitAtlasDatas = SetupList<BitAtlasData, BIT_TYPE>();
            componentAtlasDatas = SetupList<ComponentAtlasData, COMPONENT_TYPE>();
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

            #region Clunky Updates that I dont like...

            //--------------------------------------------------------------------------------------------------------//
            
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
            
            //--------------------------------------------------------------------------------------------------------//
            
            foreach (var bitAtlasData in bitAtlasDatas)
            {
                SpriteAtlasSettings.UpdatePath(bitAtlasData.type, bitAtlasData.selectedVersion);

                index = factoryManager.BitProfileData.GetProfileIndex(bitAtlasData.type);
                if (index < 0) 
                    continue;

                var profile = factoryManager.BitProfileData.profiles[index];
                bitAtlasData.Sprites.CopyTo(profile.Sprites, 0);

                factoryManager.BitProfileData.profiles[index] = profile;
            }
            
            //--------------------------------------------------------------------------------------------------------//
            
            foreach (var componentAtlasData in componentAtlasDatas)
            {
                SpriteAtlasSettings.UpdatePath(componentAtlasData.type, componentAtlasData.selectedVersion);

                index = factoryManager.ComponentProfile.GetProfileIndex(componentAtlasData.type);
                if (index < 0) 
                    continue;

                var profile = factoryManager.ComponentProfile.profiles[index];
                componentAtlasData.Sprites.CopyTo(profile.Sprites, 0);

                factoryManager.ComponentProfile.profiles[index] = profile;
            }
            
            //--------------------------------------------------------------------------------------------------------//

            #endregion //Clunky Updates that I dont like...

            /*UpdateData<PartAtlasData, PART_TYPE>(partAtlasDatas);*/
            
            //Update the Part/Bit/Component Profiles
            EditorUtility.SetDirty(factoryManager.PartsProfileData);
            EditorUtility.SetDirty(factoryManager.BitProfileData);
            EditorUtility.SetDirty(factoryManager.ComponentProfile);
            
            //Update the Part/Bit Atlases
            UpdateSpriteAtlas<PartAtlasData, PART_TYPE>(SpriteAtlasSettings.partsAtlas, partAtlasDatas);
            UpdateSpriteAtlas<BitAtlasData, BIT_TYPE>(SpriteAtlasSettings.bitsAtlas, bitAtlasDatas);
            //TODO Need to Update Component Atlas
            //UpdateSpriteAtlas<ComponentAtlasData, COMPONENT_TYPE>(SpriteAtlasSettings., partAtlasDatas);
            
            
            EditorUtility.SetDirty(SpriteAtlasSettings);
            AssetDatabase.SaveAssets();
        }

        //FIXME Really want this to work, but it is not playing nice
        /*private void UpdateData<TD>(List<TD> atlasDatas) where TD: AtlasDataBase
        {
            var factoryManager = FindObjectOfType<FactoryManager>();
            

            switch (true)
            {
                case bool _ when typeof(TD) == typeof(PartAtlasData):
                    UpdateProfileData<PartProfileScriptableObject, PartAtlasData, PART_TYPE>(
                        (List<PartAtlasData>)atlasDatas,
                        factoryManager.PartsProfileData);
                    break;
                case bool _ when typeof(TE) == typeof(BIT_TYPE):
                    profileData = factoryManager.BitProfileData ;
                    break;
                case bool _ when typeof(TE) == typeof(COMPONENT_TYPE):
                    profileData = factoryManager.ComponentProfile;
                    break;
            }
        }

        private void UpdateProfileData<T, TD, TE>(List<TD> atlasDatas, T profileData)
            where T : AttachableProfileScriptableObject<IProfile, TE> 
            where TD: AtlasDataBase
            where TE : Enum 
        {
            
            
            foreach (var atlasData in atlasDatas)
            {
                SpriteAtlasSettings.UpdatePath(atlasData.type, atlasData.selectedVersion);

                var index = profileData.GetProfileIndex(atlasData.type);
                if (index < 0)
                    continue;

                var profile = profileData.profiles[index];
                atlasData.Sprites.CopyTo(profile.Sprites, 0);

                profileData.profiles[index] = profile;
            }
        }*/

        private static void UpdateSpriteAtlas<TD, TE>(SpriteAtlas spriteAtlas, IEnumerable<TD> atlasDataBase) where TD: AtlasDataBase<TE> where TE : Enum
        {
            if (atlasDataBase.IsNullOrEmpty())
                return;
            
            var spriteList = new List<Object>();
            
            var current = spriteAtlas.GetPackables();
            spriteAtlas.Remove(current);

            foreach (var dataBase in atlasDataBase)
            {
                spriteList.AddRange(dataBase.Sprites);
            }
            
            
            spriteAtlas.Add(spriteList.ToArray());
            
            EditorUtility.SetDirty(spriteAtlas);
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

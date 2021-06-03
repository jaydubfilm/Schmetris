using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Parts.Data;
using StarSalvager.PatchTrees;
using StarSalvager.ScriptableObjects.PatchTrees;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using System.IO;
using StarSalvager.Utilities;
using UnityEditor;

#endif

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class PartRemoteData : RemoteDataBase
    {
        //Properties
        //====================================================================================================================//

        [FoldoutGroup("$title"), VerticalGroup("$title/row2/right")]
        public bool isImplemented;

        [FoldoutGroup("$title"), VerticalGroup("$title/row2/right"), HorizontalGroup("$title/row2/right/row1")]
        public PART_TYPE partType;

        [FoldoutGroup("$title"), VerticalGroup("$title/row2/right")]
        public string name;



        [FoldoutGroup("$title"), VerticalGroup("$title/row2/right")]
        public bool lockRotation;

        [TextArea, FoldoutGroup("$title")] public string description;

        [FoldoutGroup("$title"), LabelText("Part Properties")] public PartProperties[] dataTest;

        [SerializeField, FoldoutGroup("$title")]
        public bool isManual;

        [FoldoutGroup("$title")] public BIT_TYPE category;

        [FoldoutGroup("$title")] public int ammoUseCost;

        [FoldoutGroup("$title")] public int PatchSockets = 2;

        public bool HasPatchTree => !string.IsNullOrEmpty(patchTreeData);
        [FoldoutGroup("$title"), SerializeField] 
        public string patchTreeData;


        //====================================================================================================================//

        //This only compares Type and not all individual properties

        #region IEquatable

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(RemoteDataBase other)
        {
            if (other is PartRemoteData partRemote)
                return other != null && partType == partRemote.partType;
            else
                return false;
        }

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PartRemoteData) obj);
        }

        public override int GetHashCode()
        {
            //unchecked
            //{
            //    var hashCode = (name != null ? name.GetHashCode() : 0);
            //    hashCode = (hashCode * 397) ^ (int) partType;
            //    hashCode = (hashCode * 397) ^ (health != null ? health.GetHashCode() : 0);
            //    hashCode = (hashCode * 397) ^ (costs != null ? costs.GetHashCode() : 0);
            //    hashCode = (hashCode * 397) ^ (data != null ? data.GetHashCode() : 0);
            //    return hashCode;
            //}
            return base.GetHashCode();
        }

        #endregion //IEquatable


        //Get PartProprty Key Values
        //====================================================================================================================//

        public T GetDataValue<T>(PartProperties.KEYS key)
        {
            var keyString = PartProperties.Names[(int) key];
            var dataValue = dataTest.FirstOrDefault(d => d.key.Equals(keyString));

            if (dataValue.Equals(null))
                throw new MissingFieldException($"{key} missing from {partType} remote data");

            var value = dataValue.GetValue();

            if (!(value is T i))
                throw new InvalidCastException($"{key} is not of type {typeof(T)}. Expected {value.GetType()}");

            return i;
        }

        //FIXME I think i want to have a "safe" version of this function that will throw the exception itself
        public bool TryGetValue<T>(PartProperties.KEYS key, out T value)
        {
            value = default;

            var keyString = PartProperties.Names[(int) key];
            var dataValue = dataTest.FirstOrDefault(d => d.key.Equals(keyString));

            if (string.IsNullOrEmpty(dataValue.key) || dataValue.Equals(null))
                return false;

            if (!(dataValue.GetValue() is T i))
                return false;

            value = i;

            return true;
        }

        public bool TryGetValue(PartProperties.KEYS key, out object value)
        {
            value = default;

            var keyString = PartProperties.Names[(int) key];
            var dataValue = dataTest.FirstOrDefault(d => d.key.Equals(keyString));

            if (string.IsNullOrEmpty(dataValue.key) || dataValue.Equals(null))
                return false;

            //if (!(dataValue.GetValue() is T i))
            //    return false;

            value = dataValue.GetValue();

            return true;
        }

        //====================================================================================================================//

#if UNITY_EDITOR

        public string title => $"{name} {(isImplemented ? string.Empty : "[NOT IMPLEMENTED]")}";

        [ShowInInspector, PreviewField(Height = 65, Alignment = ObjectFieldAlignment.Right),
         HorizontalGroup("$title/row2", 65), VerticalGroup("$title/row2/left"), HideLabel, PropertyOrder(-100),
         ReadOnly]
        public Sprite Sprite => !HasProfile(out var profile) ? null : profile.Sprite;

        [Button("To Profile"), HorizontalGroup("$title/row2/right/row1"), EnableIf(nameof(HasProfileSimple))]
        private void GoToProfileData()
        {
            var path = AssetDatabase.GetAssetPath(Object.FindObjectOfType<FactoryManager>().PartsProfileData);
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
        }

        private bool HasProfileSimple()
        {
            var partProfile = Object.FindObjectOfType<FactoryManager>().PartsProfileData.GetProfile(partType);

            return !(partProfile is null);
        }

        private bool HasProfile(out PartProfile partProfile)
        {
            partProfile = Object.FindObjectOfType<FactoryManager>().PartsProfileData.GetProfile(partType);

            return !(partProfile is null);
        }


        //Patch Trees
        //====================================================================================================================//
        
        //TODO I really should centralize the file naming schemes
        [Button, FoldoutGroup("$title"), HideIf("HasPatchTreeFile"), PropertyOrder(-1000)]
        private void CreatePatchTree()
        {
            var dialogueContainerObject = ScriptableObject.CreateInstance<PatchTreeContainer>();
            dialogueContainerObject.PartType = partType;
            
            AssetDatabase.CreateAsset(dialogueContainerObject, GetAssetPath());
            AssetDatabase.SaveAssets();

            EditPatchTree();
        }
        [Button, FoldoutGroup("$title"), ShowIf("HasPatchTreeFile"), PropertyOrder(-1000)]
        private void EditPatchTree()
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(GetAssetPath());
        }

        private bool HasPatchTreeFile() => File.Exists(GetFilePath());

        private string GetFilePath()
        {
            const string DIRECTORY = "/Scriptable Objects/Patch Trees/";
            string GetFilePath(in string filename) => $"{Application.dataPath}{DIRECTORY}{filename}.asset";

            return  GetFilePath($"{partType.ToString()}_PatchTree");
        }

        private string GetAssetPath()
        {
            const string DIRECTORY = "/Scriptable Objects/Patch Trees/";
            return $"Assets{DIRECTORY}{partType.ToString()}_PatchTree.asset";
        }

        //====================================================================================================================//
        

#endif
    }
}



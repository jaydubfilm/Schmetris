using System;
using System.Collections;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class PartProfile : IProfile
    {
        public string Name
        {
            #if UNITY_EDITOR
            get => GetName();
            #else
            get => string.Empty;
            #endif
            set => throw new NotImplementedException();
        }

        //Part Type
        //====================================================================================================================//
        
        public int Type => (int) partType;
        [SerializeField, FoldoutGroup("$Name"), VerticalGroup("$Name/row2/right"), HorizontalGroup("$Name/row2/right/row1")]
        public PART_TYPE partType;

        //Sprite
        //====================================================================================================================//
        
        public Sprite Sprite => _sprite;
        [SerializeField, FoldoutGroup("$Name"), VerticalGroup("$Name/row2/right")]
        private Sprite _sprite;

        //Animation
        //====================================================================================================================//
        
        public AnimationScriptableObject animation
        {
            get => _animation;
            set => _animation = value;
        }
        [SerializeField, FoldoutGroup("$Name"), VerticalGroup("$Name/row2/right")]
        private AnimationScriptableObject _animation;

        //====================================================================================================================//
        
        public Sprite[] Sprites
        {
            get => null;
            set => Debug.LogError("Trying to get Sprites List from Part Profile, this is a defunct variable");
        }

        //Unity Editor
        //====================================================================================================================//
        
        #region UNITY_EDITOR



        #if UNITY_EDITOR
        
        [Button("To Remote"), HorizontalGroup("$Name/row2/right/row1"), EnableIf(nameof(HasRemoteDataSimple))]
        private void GoToRemote()
        {
            var path = AssetDatabase.GetAssetPath(Object.FindObjectOfType<FactoryManager>().PartsRemoteData);
            Selection.activeObject=AssetDatabase.LoadMainAssetAtPath(path);
        }
        
        [ShowInInspector, PreviewField(Height = 65, Alignment = ObjectFieldAlignment.Right), HorizontalGroup("$Name/row2", 65), VerticalGroup("$Name/row2/left"), HideLabel, PropertyOrder(-100), ReadOnly]
        private Sprite spritePreview
        {
            get
            {
                return _animation == null ? _sprite : _animation.GetFrame(0);
            }
        }

        private string GetName()
        {
            return HasRemoteData(out var remoteData) == false ? "NO REMOTE DATA" : remoteData.title;
        }

        private bool HasRemoteDataSimple()
        {
            var partRemoteData = Object.FindObjectOfType<FactoryManager>().PartsRemoteData.GetRemoteData(partType);

            return !(partRemoteData is null);
        }
        private bool HasRemoteData(out PartRemoteData partRemoteData)
        {
            partRemoteData = Object.FindObjectOfType<FactoryManager>().PartsRemoteData.GetRemoteData(partType);

            return !(partRemoteData is null);
        }

#endif

        #endregion
    }
}
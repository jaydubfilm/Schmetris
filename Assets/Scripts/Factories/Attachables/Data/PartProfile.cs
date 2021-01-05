using System;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public struct PartProfile : IProfile
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


        public int Type => (int) partType;
        [SerializeField, FoldoutGroup("$Name"), VerticalGroup("$Name/row2/right")]
        public PART_TYPE partType;

        public AnimationScriptableObject animation
        {
            get => _animation;
            set => _animation = value;
        }
        [SerializeField, FoldoutGroup("$Name"), VerticalGroup("$Name/row2/right")]
        private AnimationScriptableObject _animation;

        public Sprite Sprite => _sprite;
        [SerializeField, FoldoutGroup("$Name"), ListDrawerSettings(ShowIndexLabels = true), Space(10f)]
        private Sprite _sprite;

        public Sprite[] Sprites
        {
            get => _sprites;
            set => _sprites = value;
        }

        [SerializeField, FoldoutGroup("$Name"), ListDrawerSettings(ShowIndexLabels = true), Space(10f)]
        private Sprite[] _sprites;
        


        #region UNITY_EDITOR

        #if UNITY_EDITOR
        
        [ShowInInspector, PreviewField(Height = 65, Alignment = ObjectFieldAlignment.Right), HorizontalGroup("$Name/row2", 65), VerticalGroup("$Name/row2/left"), HideLabel, PropertyOrder(-100), ReadOnly]
        private Sprite spritePreview
        {
            get
            {
                if (_sprites == null || _sprites.Length == 0)
                    return null;
                
                return _animation == null ? _sprites[0] : _animation.GetFrame(0);
            }
        }

        private string GetName()
        {
            var remoteData = Object.FindObjectOfType<FactoryManager>().PartsRemoteData.GetRemoteData(partType);
            return remoteData is null ? "NO REMOTE DATA" : remoteData.name;
        }
        
        #endif

        #endregion
    }
}
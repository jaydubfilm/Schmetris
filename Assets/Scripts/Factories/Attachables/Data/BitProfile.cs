﻿using Sirenix.OdinInspector;
using StarSalvager.Utilities.Animations;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct BitProfile : IProfile
    {
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        [SerializeField, FoldoutGroup("$Name"), VerticalGroup("$Name/row2/right")]
        private string _name;

        public int Type => (int) bitType;
        [SerializeField, FoldoutGroup("$Name"), VerticalGroup("$Name/row2/right")]
        public BIT_TYPE bitType;

        public AnimationScriptableObject animation
        {
            get => _animation;
            set => _animation = value;
        }
        [SerializeField, FoldoutGroup("$Name"), VerticalGroup("$Name/row2/right")]
        private AnimationScriptableObject _animation;

        [HorizontalGroup("$Name/row3"), VerticalGroup("$Name/row3/col1")]
        public Sprite refinedSprite;
        
        [VerticalGroup("$Name/row3/col1")]
        public Color color;

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

        [ShowInInspector, PreviewField(Height = 65, Alignment = ObjectFieldAlignment.Right),
         HorizontalGroup("$Name/row3", 65), HideLabel, PropertyOrder(-100), ReadOnly]
        private Sprite refinedSpritePreview => refinedSprite;

#endif

        #endregion

    }
}
using Sirenix.OdinInspector;
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
        private Sprite spritePreview => animation == null ? _sprites[0] : animation.GetFrame(0);
        
#endif

        #endregion
        /*public int Type => (int) bitType;

        [SerializeField, FoldoutGroup("$Name")]
        public BIT_TYPE bitType;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        [SerializeField, FoldoutGroup("$Name")]
        private string _name;


        public Sprite[] Sprites
        {
            get => _sprites;
            set => _sprites = value;
        }

        

        [SerializeField, FoldoutGroup("$Name")]
        private Sprite[] _sprites;
        
        public AnimationScriptableObject animation
        {
            get => _animation;
            set => _animation = value;
        }
        [SerializeField, FoldoutGroup("$Name")]
        private AnimationScriptableObject _animation;*/
    }

}
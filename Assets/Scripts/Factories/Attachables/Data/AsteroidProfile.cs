using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Utilities.Animations;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    //[System.Serializable]
    /*public struct AsteroidProfile
    {
        public Vector2Int Dimensions => dimensions;
        [SerializeField]
        private Vector2Int dimensions;

        public ASTEROID_SIZE Size => size;
        [SerializeField]
        private ASTEROID_SIZE size;

        public Sprite[] Sprites
        {
            get => _sprites;
            set => _sprites = value;
        }

        [SerializeField]
        private Sprite[] _sprites;
    }*/
    
    [System.Serializable]
    public struct AsteroidProfile
    {
        public ASTEROID_SIZE Size => size;
        [SerializeField, VerticalGroup("row2/right")]
        private ASTEROID_SIZE size;
        
        public Vector2Int Dimensions => dimensions;
        [SerializeField, VerticalGroup("row2/right")]
        private Vector2Int dimensions;
        public Sprite[] Sprites
        {
            get => _sprites;
            set => _sprites = value;
        }

        [SerializeField, ListDrawerSettings(ShowIndexLabels = true), Space(10f)]
        private Sprite[] _sprites;

        #region UNITY_EDITOR

#if UNITY_EDITOR

        [ShowInInspector, PreviewField(Height = 65, Alignment = ObjectFieldAlignment.Right), HorizontalGroup("row2", 65), VerticalGroup("row2/left"), HideLabel, PropertyOrder(-100), ReadOnly]
        private Sprite spritePreview
        {
            get
            {
                if (_sprites == null || _sprites.Length == 0)
                    return null;

                return _sprites[0];
            }
        }

#endif

        #endregion

    }
}
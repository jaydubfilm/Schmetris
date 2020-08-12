using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Utilities.Animations;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct AsteroidProfile
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
    }
}
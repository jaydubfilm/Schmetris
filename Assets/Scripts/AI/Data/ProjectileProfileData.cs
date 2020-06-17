using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct ProjectileProfileData
    {
        public int Type => (int)m_projectileType;

        [SerializeField, FoldoutGroup("$ProjectileType")]
        private PROJECTILE_TYPE m_projectileType;

        [SerializeField, FoldoutGroup("$ProjectileType")]
        private Sprite m_sprite;

        [SerializeField, FoldoutGroup("$ProjectileType")]
        private float m_projectileSpeed;

        public PROJECTILE_TYPE ProjectileType
        {
            get => m_projectileType;
        }

        public Sprite Sprite
        {
            get => m_sprite;
        }

        public float ProjectileSpeed
        {
            get => m_projectileSpeed;
        }
    }
}
using Sirenix.OdinInspector;
using UnityEngine;
using StarSalvager.AI;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public class ProjectileProfileData
    {
        [SerializeField, FoldoutGroup("$ProjectileType")]
        private string m_projectileType;

        [SerializeField, FoldoutGroup("$ProjectileType")]
        private Sprite m_sprite;

        [SerializeField, FoldoutGroup("$ProjectileType")]
        private float m_projectileSpeed;

        [SerializeField, FoldoutGroup("$ProjectileType")]
        private string m_projectileTypeID = System.Guid.NewGuid().ToString();

        public string ProjectileType
        {
            get => m_projectileType;
        }

        public string ProjectileTypeID
        {
            get => m_projectileTypeID;
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
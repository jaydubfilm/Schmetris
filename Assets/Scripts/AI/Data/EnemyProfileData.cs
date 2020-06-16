using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct EnemyProfileData
    {
        public int Type => (int)m_enemyType;

        [SerializeField]
        private ENEMY_TYPE m_enemyType;

        [SerializeField]
        private Sprite m_sprite;

        [SerializeField]
        private ENEMY_MOVETYPE m_movementType;

        [SerializeField]
        private ENEMY_ATTACKTYPE m_attackType;

        public ENEMY_TYPE EnemyType
        {
            get => m_enemyType;
        }

        public Sprite Sprite
        {
            get => m_sprite;
        }

        public ENEMY_MOVETYPE MovementType
        {
            get => m_movementType;
        }

        public ENEMY_ATTACKTYPE AttackType
        {
            get => m_attackType;
        }
    }
}
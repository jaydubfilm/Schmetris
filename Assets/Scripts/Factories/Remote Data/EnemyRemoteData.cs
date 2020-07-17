using Sirenix.OdinInspector;
using UnityEngine;
using StarSalvager.AI;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct EnemyRemoteData
    {
        public int Type => (int)m_enemyType;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private ENEMY_TYPE m_enemyType;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private int m_enemyID;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private string m_name;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private int m_health;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private float m_movementSpeed;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private float m_attackDamage;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private float m_attackSpeed;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private int m_minBitExplosionCount;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private int m_maxBitExplosionCount;

        public ENEMY_TYPE EnemyType
        {
            get => m_enemyType;
        }

        public int EnemyID
        {
            get => m_enemyID;
        }

        public string Name
        {
            get => m_name;
        }

        public int Health
        {
            get => m_health;
        }

        public float MovementSpeed
        {
            get => m_movementSpeed;
        }

        public float AttackDamage
        {
            get => m_attackDamage;
        }

        public float AttackSpeed
        {
            get => m_attackSpeed;
        }

        public int MinBitExplosionCount
        {
            get => m_minBitExplosionCount;
        }
        public int MaxBitExplosionCount
        {
            get => m_maxBitExplosionCount;
        }
    }
}
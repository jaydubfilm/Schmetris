using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct EnemyRemoteData
    {
        public int Type => (int)m_enemyType;

        [SerializeField]
        private ENEMY_TYPE m_enemyType;

        [SerializeField]
        private int m_enemyID;

        [SerializeField]
        private string m_name;

        [SerializeField]
        private int m_health;

        [SerializeField]
        private float m_movementSpeed;

        [SerializeField]
        private float m_attackDamage;

        [SerializeField]
        private float m_attackSpeed;

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
    }
}
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct EnemyRemoteData
    {
        public int Type => (int)_enemyType;

        [SerializeField]
        private ENEMY_TYPE _enemyType;

        [SerializeField]
        private int _enemyID;

        [SerializeField]
        private string _name;

        [SerializeField]
        private int _health;

        [SerializeField]
        private float _movementSpeed;

        [SerializeField]
        private float _attackDamage;

        [SerializeField]
        private float _attackSpeed;

        public ENEMY_TYPE EnemyType
        {
            get => _enemyType;
        }

        public int EnemyID
        {
            get => _enemyID;
        }

        public string Name
        {
            get => _name;
        }

        public int Health
        {
            get => _health;
        }

        public float MovementSpeed
        {
            get => _movementSpeed;
        }

        public float AttackDamage
        {
            get => _attackDamage;
        }

        public float AttackSpeed
        {
            get => _attackSpeed;
        }
    }
}
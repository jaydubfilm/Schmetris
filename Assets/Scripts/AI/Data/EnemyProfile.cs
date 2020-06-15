using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct EnemyProfile
    {
        public int Type => (int)enemyType;

        [SerializeField]
        public ENEMY_TYPE enemyType;

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

        public int EnemyID
        {
            get => _enemyID;
            set => _enemyID = value;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public int Health
        {
            get => _health;
            set => _health = value;
        }

        public float MovementSpeed
        {
            get => _movementSpeed;
            set => _movementSpeed = value;
        }

        public float AttackDamage
        {
            get => _attackDamage;
            set => _attackDamage = value;
        }

        public float AttackSpeed
        {
            get => _attackSpeed;
            set => _attackSpeed = value;
        }
    }
}
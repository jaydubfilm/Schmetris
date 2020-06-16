using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct EnemyProfileData
    {
        public int Type => (int)_enemyType;

        [SerializeField]
        private ENEMY_TYPE _enemyType;

        [SerializeField]
        private Sprite _sprite;

        [SerializeField]
        private ENEMY_MOVETYPE _movementType;

        [SerializeField]
        private ENEMY_ATTACKTYPE _attackType;

        public ENEMY_TYPE EnemyType
        {
            get => _enemyType;
        }

        public Sprite Sprite
        {
            get => _sprite;
        }

        public ENEMY_MOVETYPE MovementType
        {
            get => _movementType;
        }

        public ENEMY_ATTACKTYPE AttackType
        {
            get => _attackType;
        }
    }
}
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct EnemyProfileData
    {
        public int Type => (int)m_enemyType;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private ENEMY_TYPE m_enemyType;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private Sprite m_sprite;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private ENEMY_MOVETYPE m_movementType;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private ENEMY_ATTACKTYPE m_attackType;

        //Variables that are only shown based on the EnemyType
        [SerializeField, ShowIf("m_movementType", ENEMY_MOVETYPE.Zigzag)]
        private float m_zigZagsPerSecond;

        [SerializeField, ShowIf("m_movementType", ENEMY_MOVETYPE.Orbit)]
        private float m_orbitRadius;


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

        public float ZigZagsPerSecond
        {
            get => m_zigZagsPerSecond;
        }

        public float OrbitRadius
        {
            get => m_orbitRadius;
        }
    }
}
using Sirenix.OdinInspector;
using UnityEngine;
using StarSalvager.AI;

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
        private bool showOscillationsPerSecond => m_movementType == ENEMY_MOVETYPE.Oscillate || m_movementType == ENEMY_MOVETYPE.OscillateHorizontal;
        [SerializeField, ShowIf("showOscillationsPerSecond")]
        private float m_oscillationsPerSeconds;

        private bool showOscillationAngleRange => m_movementType == ENEMY_MOVETYPE.Oscillate || m_movementType == ENEMY_MOVETYPE.OscillateHorizontal;
        [SerializeField, ShowIf("showOscillationAngleRange")]
        private float m_oscillationAngleRange;

        [SerializeField, ShowIf("m_movementType", ENEMY_MOVETYPE.Orbit)]
        private float m_orbitRadius;

        private bool showSpreadAngle => m_attackType == ENEMY_ATTACKTYPE.AtPlayerCone || m_attackType == ENEMY_ATTACKTYPE.Spray;
        [SerializeField, ShowIf("showSpreadAngle")]
        private float m_spreadAngle;

        [SerializeField, ShowIf("m_attackType", ENEMY_ATTACKTYPE.Spray)]
        private int m_sprayCount;


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

        public float OscillationsPerSeconds
        {
            get => m_oscillationsPerSeconds;
        }

        public float OscillationAngleRange
        {
            get => m_oscillationAngleRange;
        }

        public float OrbitRadius
        {
            get => m_orbitRadius;
        }

        public float SpreadAngle
        {
            get => m_spreadAngle;
        }

        public int SprayCount
        {
            get => m_sprayCount;
        }
    }
}
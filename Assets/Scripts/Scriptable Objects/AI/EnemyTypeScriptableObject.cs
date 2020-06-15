using System;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Enemy_Type", menuName = "Star Salvager/Scriptable Objects/Enemy Type")]
    public class EnemyTypeScriptableObject : ScriptableObject
    {
        public enum EnemyMovementType
        {
            Standard,
            Zigzag,
            Orbit
        }

        public enum EnemyAttackType
        {
            Fast,
            Slow
        }

        [SerializeField]
        public ENEMY_TYPE enemyType;

        [SerializeField]
        private Sprite m_enemySprite;

        [SerializeField]
        private EnemyMovementType m_enemyMovementType;

        [SerializeField]
        private EnemyAttackType m_enemyAttackType;

        public Sprite GetSprite()
        {
            return m_enemySprite;
        }
    }
}
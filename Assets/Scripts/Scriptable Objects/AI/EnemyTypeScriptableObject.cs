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
            Zigzag
        }

        [SerializeField]
        private StringScriptableObject m_enemyName;

        [SerializeField]
        private SpriteScriptableObject m_enemySprite;

        [SerializeField]
        private IntScriptableObject m_enemyHealth;

        [SerializeField]
        private EnemyMovementType m_enemyMovementType;

        [SerializeField]
        private FloatScriptableObject m_enemyMovementSpeed;

        public String GetName()
        {
            return m_enemyName.GetValue();
        }

        public Sprite GetSprite()
        {
            return m_enemySprite.GetValue();
        }

        public int GetHealth()
        {
            return m_enemyHealth.GetValue();
        }
    }
}
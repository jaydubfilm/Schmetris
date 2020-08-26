using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public class EnemyRemoteData
    {
        [SerializeField, FoldoutGroup("$EnemyType"), DisplayAsString, LabelText("Enemy ID")]
        private string m_enemyType = Guid.NewGuid().ToString();

#if UNITY_EDITOR
        [Button("Copy"), FoldoutGroup("$EnemyType")]
        private void CopyID()
        {
            GUIUtility.systemCopyBuffer = m_enemyType;
        }

#endif
        
        [SerializeField, FoldoutGroup("$EnemyType")]
        private string m_name;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private int m_health;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private float m_movementSpeed;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private float m_attackDamage;

        [SerializeField, FoldoutGroup("$EnemyType")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_attackSpeed")]
        private float m_rateOfFire;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private Vector2Int m_dimensions;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private int m_maxDrops;

        [SerializeField, FoldoutGroup("$EnemyType")]
        private List<RDSEnemyData> m_rdsEnemyData;

        public string EnemyID => m_enemyType;

        public string Name => m_name;

        public int Health => m_health;

        public float MovementSpeed => m_movementSpeed;

        public float AttackDamage => m_attackDamage;

        public float RateOfFire => m_rateOfFire;

        public Vector2Int Dimensions => m_dimensions;

        public int MaxDrops => m_maxDrops;

        public List<RDSEnemyData> rdsEnemyData => m_rdsEnemyData;
    }
}
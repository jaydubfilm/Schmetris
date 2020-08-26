using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct EnemyRemoteData
    {
        [SerializeField, FoldoutGroup("$GetEnemyType"), ValueDropdown("GetEnemyTypes")]
        private string m_enemyType;

        [SerializeField, FoldoutGroup("$GetEnemyType")]
        private string m_name;

        [SerializeField, FoldoutGroup("$GetEnemyType")]
        private int m_health;

        [SerializeField, FoldoutGroup("$GetEnemyType")]
        private float m_movementSpeed;

        [SerializeField, FoldoutGroup("$GetEnemyType")]
        private float m_attackDamage;

        [SerializeField, FoldoutGroup("$GetEnemyType")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_attackSpeed")]
        private float m_rateOfFire;

        [SerializeField, FoldoutGroup("$GetEnemyType")]
        private Vector2Int m_dimensions;

        [SerializeField, FoldoutGroup("$GetEnemyType")]
        private int m_maxDrops;

        [SerializeField, FoldoutGroup("$GetEnemyType")]
        private List<RDSEnemyData> m_rdsEnemyData;

        public string EnemyType => m_enemyType;

        public string Name => m_name;

        public int Health => m_health;

        public float MovementSpeed => m_movementSpeed;

        public float AttackDamage => m_attackDamage;

        public float RateOfFire => m_rateOfFire;

        public Vector2Int Dimensions => m_dimensions;

        public int MaxDrops => m_maxDrops;

        public List<RDSEnemyData> rdsEnemyData => m_rdsEnemyData;

#if UNITY_EDITOR
        private string GetEnemyType()
        {
            return Object.FindObjectOfType<FactoryManager>().EnemyProfile.GetEnemyName(EnemyType);
        }
        
        private static IEnumerable GetEnemyTypes()
        {
            return Object.FindObjectOfType<FactoryManager>().EnemyProfile.GetEnemyTypes();
        }
#endif
    }
}
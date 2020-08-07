using Sirenix.OdinInspector;
using UnityEngine;
using StarSalvager.AI;
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
        private float m_attackSpeed;

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

        public float AttackSpeed => m_attackSpeed;

        public Vector2Int Dimensions => m_dimensions;

        public int MaxDrops => m_maxDrops;

        public List<RDSEnemyData> rdsEnemyData => m_rdsEnemyData;

        private string GetEnemyType()
        {
            string value = m_enemyType;
            ValueDropdownList<string> enemyTypes = new ValueDropdownList<string>();
            foreach (EnemyProfileData data in GameObject.FindObjectOfType<FactoryManager>().EnemyProfile.m_enemyProfileData)
            {
                enemyTypes.Add(data.EnemyType, data.EnemyTypeID);
            }
            return enemyTypes.Find(s => s.Value == value).Text;
        }

        private IEnumerable GetEnemyTypes()
        {
            ValueDropdownList<string> enemyTypes = new ValueDropdownList<string>();
            foreach (EnemyProfileData data in GameObject.FindObjectOfType<FactoryManager>().EnemyProfile.m_enemyProfileData)
            {
                enemyTypes.Add(data.EnemyType, data.EnemyTypeID);
            }
            return enemyTypes;
        }
    }
}
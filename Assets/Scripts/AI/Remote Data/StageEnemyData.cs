using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.AI
{
    [Serializable]
    public class StageEnemyData
    {
        [SerializeField, FoldoutGroup("$GetEnemyType"), ValueDropdown("GetEnemyTypes")]
        private string m_enemyType;
        [SerializeField, FoldoutGroup("$GetEnemyType")]
        private int m_enemyCount;

        public string EnemyType => m_enemyType;
        public int EnemyCount => m_enemyCount;

#if UNITY_EDITOR
        private string GetEnemyType()
        {
            return Object.FindObjectOfType<FactoryManager>().EnemyProfile.GetEnemyName(m_enemyType);
        }

        private IEnumerable GetEnemyTypes()
        {
            return Object.FindObjectOfType<FactoryManager>().EnemyProfile.GetEnemyTypes();
        }
#endif

    }
}
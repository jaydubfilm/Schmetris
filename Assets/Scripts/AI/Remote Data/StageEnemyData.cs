using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.AI
{
    [Serializable]
    public class StageEnemyData
    {
        [SerializeField, FoldoutGroup("$m_enemyType"), ValueDropdown("GetEnemyTypes")]
        private string m_enemyType;
        [SerializeField, FoldoutGroup("$m_enemyType")]
        private int m_enemyCount;

        public string EnemyType => m_enemyType;
        public int EnemyCount => m_enemyCount;

        private IEnumerable<string> GetEnemyTypes()
        {
            List<string> enemyTypes = new List<string>();
            foreach (EnemyProfileData data in GameObject.FindObjectOfType<FactoryManager>().EnemyProfile.m_enemyProfileData)
            {
                enemyTypes.Add(data.EnemyType);
            }
            return enemyTypes;
        }
    }
}
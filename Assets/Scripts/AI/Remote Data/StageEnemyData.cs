using System;
using System.Collections;
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
        [SerializeField, FoldoutGroup("$GetEnemyType"), ValueDropdown("GetEnemyTypes")]
        private string m_enemyType;
        [SerializeField, FoldoutGroup("$GetEnemyType")]
        private int m_enemyCount;

        public string EnemyType => m_enemyType;
        public int EnemyCount => m_enemyCount;

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
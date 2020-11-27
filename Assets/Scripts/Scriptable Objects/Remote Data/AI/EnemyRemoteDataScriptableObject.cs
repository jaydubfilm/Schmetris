﻿using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;
using System.Collections.Generic;
using StarSalvager.AI;
using System.Collections;
using Sirenix.OdinInspector;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Enemy_Remote", menuName = "Star Salvager/Scriptable Objects/Enemy Remote Data")]
    public class EnemyRemoteDataScriptableObject : ScriptableObject
    {
        public List<EnemyRemoteData> m_enemyRemoteData = new List<EnemyRemoteData>();

        public EnemyRemoteData GetEnemyRemoteData(string TypeID)
        {
            return m_enemyRemoteData
                .FirstOrDefault(p => p.EnemyID == TypeID);
        }
        public EnemyRemoteData GetEnemyRemoteDataByName(string enemyName)
        {
            return m_enemyRemoteData
                .FirstOrDefault(p => p.Name.Equals(enemyName));
        }


#if UNITY_EDITOR

        public IEnumerable<(string EnemyName, string EnemyID)> GetAllEnemyNamesIds()
        {
            var outList = m_enemyRemoteData.Select(x => (x.Name, x.EnemyID)).ToList();

            return outList;
        }

        public string GetEnemyName(string id)
        {
            return GetEnemyRemoteData(id)?.Name;
        }

        public string GetEnemyId(string name)
        {
            return GetEnemyRemoteDataByName(name)?.EnemyID;
        }

        public IEnumerable GetEnemyTypes()
        {
            ValueDropdownList<string> enemyTypes = new ValueDropdownList<string>();
            foreach (var (enemyName, enemyID) in GetAllEnemyNamesIds())
            {
                enemyTypes.Add(enemyName, enemyID);
            }
            return enemyTypes;
        }

#endif

    }

}
using System.Collections;
using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.AI;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Enemy_Profile", menuName = "Star Salvager/Scriptable Objects/Enemy Profile")]
    public class EnemyProfileScriptableObject : ScriptableObject
    {
        //Properties
        //====================================================================================================================//
        
        [SerializeField, Required] public GameObject m_enemyDecoy;

        [SerializeField]
        private List<EnemyProfileData> m_enemyProfileData = new List<EnemyProfileData>();
        
        //====================================================================================================================//
        
        public EnemyProfileData GetEnemyProfileData(string typeID)
        {
            return m_enemyProfileData
                .FirstOrDefault(p => p.EnemyID.Equals(typeID));
        }
        
        public EnemyProfileData GetEnemyProfileDataByName(string enemyName)
        {
            return m_enemyProfileData
                .FirstOrDefault(p => p.EnemyName.Equals(enemyName));
        }

        //Unity Editor
        //====================================================================================================================//

        #region Unity Editor

#if UNITY_EDITOR
        
        public IEnumerable<(string EnemyName, string EnemyID)> GetAllEnemyNamesIds()
        {
            var outList = m_enemyProfileData.Select(x => (x.EnemyName, x.EnemyID)).ToList();

            return outList;
        }

        public string GetEnemyName(string id)
        {
            return GetEnemyProfileData(id)?.EnemyName;
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

        #endregion //Unity Editor

        //====================================================================================================================//
        
    }

}
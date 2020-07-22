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
        [SerializeField, Required]
        public GameObject m_prefab;

        [SerializeField, Required]
        public GameObject m_attachablePrefab;

        public List<EnemyProfileData> m_enemyProfileData = new List<EnemyProfileData>();

        public EnemyProfileData GetEnemyProfileData(string TypeID)
        {
            return m_enemyProfileData
                .FirstOrDefault(p => p.EnemyTypeID == TypeID);
        }
    }

}
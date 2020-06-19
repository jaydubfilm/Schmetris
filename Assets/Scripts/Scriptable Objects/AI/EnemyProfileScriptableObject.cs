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

        public List<EnemyProfileData> m_enemyProfileData = new List<EnemyProfileData>();

        public EnemyProfileData GetEnemyProfileData(ENEMY_TYPE Type)
        {
            return m_enemyProfileData
                .FirstOrDefault(p => p.EnemyType == Type);
        }
    }

}
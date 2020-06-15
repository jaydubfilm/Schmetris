using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;
using System.Collections.Generic;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Enemy_Profile", menuName = "Star Salvager/Scriptable Objects/Enemy Profile")]
    public class EnemyProfileScriptableObject : ScriptableObject
    {
        public GameObject m_enemyPrefab;
        public List<EnemyProfile> m_enemyProfiles = new List<EnemyProfile>();

        public EnemyProfile GetProfile(ENEMY_TYPE Type)
        {
            return m_enemyProfiles
                .FirstOrDefault(p => p.enemyType == Type);
        }
    }

}
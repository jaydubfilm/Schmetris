using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;
using System.Collections.Generic;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Enemy_Profile", menuName = "Star Salvager/Scriptable Objects/Enemy Profile")]
    public class EnemyProfileScriptableObject : ScriptableObject
    {
        public List<EnemyProfileData> m_enemyProfileData = new List<EnemyProfileData>();

        public EnemyProfileData GetEnemyProfileData(ENEMY_TYPE Type)
        {
            return m_enemyProfileData
                .FirstOrDefault(p => p.EnemyType == Type);
        }
    }

}
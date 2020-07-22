using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;
using System.Collections.Generic;
using StarSalvager.AI;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Enemy_Remote", menuName = "Star Salvager/Scriptable Objects/Enemy Remote Data")]
    public class EnemyRemoteDataScriptableObject : ScriptableObject
    {
        public List<EnemyRemoteData> m_enemyRemoteData = new List<EnemyRemoteData>();

        public EnemyRemoteData GetRemoteData(string TypeID)
        {
            return m_enemyRemoteData
                .FirstOrDefault(p => p.EnemyType == TypeID);
        }
    }

}
using StarSalvager.AI;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Player Level Remote", menuName = "Star Salvager/Scriptable Objects/Player Level Data")]
    public class PlayerLevelRemoteDataScriptableObject : ScriptableObject
    {
        [SerializeField]
        private List<PlayerLevelRemoteData> m_playerLevelRemoteData;
        
        public PlayerLevelRemoteData GetRemoteData(int levelNumber)
        {
            return m_playerLevelRemoteData[levelNumber];
        }
    }
}


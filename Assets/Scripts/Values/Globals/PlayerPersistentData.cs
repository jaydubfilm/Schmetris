using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Values
{
    public static class PlayerPersistentData
    {
        private static List<PlayerData> m_playerData = new List<PlayerData>();

        public static PlayerData PlayerData => GetPlayerData(0);
        
        public static PlayerData GetPlayerData(int index)
        {
            if (m_playerData.Count > index)
                return m_playerData[index];

            PlayerData playerData = new PlayerData();
            m_playerData.Add(playerData);
            return m_playerData[index];
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Values
{
    public static class PlayerPersistentData
    {
        private static List<PlayerData> m_playerData = new List<PlayerData>();

        public static PlayerData GetPlayerData()
        {
            if (m_playerData.Count > 0)
                return m_playerData[0];

            PlayerData playerData = new PlayerData();
            m_playerData.Add(playerData);
            return m_playerData[0];
        }
    }
}
using Newtonsoft.Json;
using StarSalvager.Factories;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StarSalvager.Values
{
    public static class PlayerPersistentData
    {
        private static readonly string persistentDataPath = Application.dataPath + "/RemoteData/PlayerPersistentData.player";
        private static List<PlayerData> m_playerData = new List<PlayerData>();

        public static void Init()
        {
            m_playerData.Add(ImportPlayerPersistentData());
        }

        public static PlayerData PlayerData => GetPlayerData(0);
        
        public static PlayerData GetPlayerData(int index)
        {
            if (m_playerData.Count > index)
                return m_playerData[index];

            Init();
            return m_playerData[index];
        }

        public static void ResetPlayerData()
        {
            PlayerData data = new PlayerData();
            for (int i = 0; i < FactoryManager.Instance.SectorRemoteData.Count; i++)
            {
                data.AddSectorProgression(i, 0);
            }
            m_playerData[0] = data;
        }

        private static string ExportPlayerPersistentData(PlayerData editorData)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(persistentDataPath, export);

            return export;
        }

        private static PlayerData ImportPlayerPersistentData()
        {
            if (!Directory.Exists(persistentDataPath))
                System.IO.Directory.CreateDirectory(Application.dataPath + "/RemoteData/");

            if (!File.Exists(persistentDataPath))
            {
                PlayerData data = new PlayerData();
                for (int i = 0; i < FactoryManager.Instance.SectorRemoteData.Count; i++)
                {
                    data.AddSectorProgression(i, 0);
                }
                return data;
            }

            var loaded = JsonConvert.DeserializeObject<PlayerData>(File.ReadAllText(persistentDataPath));

            return loaded;
        }

        public static void CustomOnApplication()
        {
            ExportPlayerPersistentData(PlayerData);
        }
    }
}
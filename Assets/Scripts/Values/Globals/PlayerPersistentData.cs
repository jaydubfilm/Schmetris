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
        private static readonly List<string> persistentDataPaths = new List<string>
        {
            Application.dataPath + "/RemoteData/PlayerPersistentDataSaveFile0.player",
            Application.dataPath + "/RemoteData/PlayerPersistentDataSaveFile1.player",
            Application.dataPath + "/RemoteData/PlayerPersistentDataSaveFile2.player"
        };
        private static List<PlayerData> m_playerData = new List<PlayerData>();

        public static bool IsNewFile = false;

        public static int CurrentSaveFile = 0;

        public static void Init()
        {
            for (int i = m_playerData.Count; i < 3; i++)
            {
                m_playerData.Add(ImportPlayerPersistentData(i));
            }
        }

        public static PlayerData PlayerData => GetPlayerData(CurrentSaveFile);

        public static PlayerData GetPlayerData(int index)
        {
            CurrentSaveFile = index;
            if (m_playerData.Count <= CurrentSaveFile)
            {
                Init();
            }
            return m_playerData[CurrentSaveFile];
        }

        public static void ResetPlayerData()
        {
            PlayerData data = new PlayerData();
            for (int i = 0; i < FactoryManager.Instance.SectorRemoteData.Count; i++)
            {
                data.AddSectorProgression(i, 0);
            }
            m_playerData[0] = data;

            IsNewFile = true;
        }

        private static string ExportPlayerPersistentData(PlayerData editorData, int saveSlot)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(persistentDataPaths[saveSlot], export);

            return export;
        }

        private static PlayerData ImportPlayerPersistentData(int saveSlot)
        {
            if (!Directory.Exists(persistentDataPaths[saveSlot]))
                System.IO.Directory.CreateDirectory(Application.dataPath + "/RemoteData/");

            if (!File.Exists(persistentDataPaths[saveSlot]))
            {
                PlayerData data = new PlayerData();
                for (int i = 0; i < FactoryManager.Instance.SectorRemoteData.Count; i++)
                {
                    data.AddSectorProgression(i, 0);
                }
                IsNewFile = true;
                return data;
            }

            var loaded = JsonConvert.DeserializeObject<PlayerData>(File.ReadAllText(persistentDataPaths[saveSlot]));

            IsNewFile = false;

            return loaded;
        }

        public static void ClearPlayerData()
        {
            m_playerData.Clear();
            
            Init();
        }

        public static void CustomOnApplicationQuit()
        {
            for (int i = 0; i < 3; i++)
            {
                ExportPlayerPersistentData(GetPlayerData(i), i);
            }
        }
    }
}
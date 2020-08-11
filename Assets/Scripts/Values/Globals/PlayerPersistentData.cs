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

        private static readonly string persistentMetadataPath =
            Application.dataPath + "/RemoteData/PlayerPersistentMetadata.player";

        public static bool IsNewFile = false;

        private static int CurrentSaveFile = -1;

        public static void Init()
        {

        }

        public static PlayerData PlayerData = new PlayerData();

        public static PlayerMetadata PlayerMetadata = ImportPlayerPersistentMetadata();

        public static void SetCurrentSaveFile(int saveFile)
        {
            int index;
            if (PlayerMetadata.saveFileLastAccessedOrder.Count <= saveFile)
            {
                index = PlayerMetadata.ActivateNextEmptySaveFile();
            }
            else 
            {
                PlayerMetadata.MoveSaveFileToFront(saveFile);
                index = PlayerMetadata.GetSaveFileAtIndex(0);
            }

            if (CurrentSaveFile >= 0)
            {
                ExportPlayerPersistentData(PlayerData, CurrentSaveFile);
            }
            else if (index == CurrentSaveFile)
            {
                return;
            }

            CurrentSaveFile = index;
            PlayerData = ImportPlayerPersistentData(index);
        }

        public static void ResetPlayerData()
        {
            PlayerData data = new PlayerData();
            for (int i = 0; i < FactoryManager.Instance.SectorRemoteData.Count; i++)
            {
                data.AddSectorProgression(i, 0);
            }
            PlayerData = data;

            IsNewFile = true;
        }

        private static string ExportPlayerPersistentData(PlayerData editorData, int saveSlot)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(persistentDataPaths[saveSlot], export);

            return export;
        }

        private static string ExportPlayerPersistentMetadata(PlayerMetadata editorData)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(persistentMetadataPath, export);

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

        private static PlayerMetadata ImportPlayerPersistentMetadata()
        {
            if (!Directory.Exists(persistentMetadataPath))
                System.IO.Directory.CreateDirectory(Application.dataPath + "/RemoteData/");

            if (!File.Exists(persistentMetadataPath))
            {
                PlayerMetadata data = new PlayerMetadata();
                return data;
            }

            var loaded = JsonConvert.DeserializeObject<PlayerMetadata>(File.ReadAllText(persistentMetadataPath));
            return loaded;
        }

        public static void ClearPlayerData()
        {
            PlayerData = null;
        }

        public static void CustomOnApplicationQuit()
        {
            if (CurrentSaveFile >= 0)
                ExportPlayerPersistentData(PlayerData, CurrentSaveFile);

            ExportPlayerPersistentMetadata(PlayerMetadata);
        }
    }
}
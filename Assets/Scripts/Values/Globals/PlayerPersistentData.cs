using Newtonsoft.Json;
using StarSalvager.Factories;
using StarSalvager.Missions;
using StarSalvager.Utilities.Saving;
using System;
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
            Application.dataPath + "/RemoteData/PlayerPersistentDataSaveFile2.player",
            Application.dataPath + "/RemoteData/PlayerPersistentDataSaveFile3.player",
            Application.dataPath + "/RemoteData/PlayerPersistentDataSaveFile4.player",
            Application.dataPath + "/RemoteData/PlayerPersistentDataSaveFile5.player"
        };

        private static readonly string persistentMetadataPath =
            Application.dataPath + "/RemoteData/PlayerPersistentMetadata.player";

        private static string CurrentSaveFile = string.Empty;

        public static PlayerData PlayerData = new PlayerData();

        public static PlayerMetadata PlayerMetadata = ImportPlayerPersistentMetadata();

        public static void SetCurrentSaveFile(string saveFile)
        {
            PlayerData = ImportPlayerPersistentData(saveFile);
            CurrentSaveFile = saveFile;
        }

        public static string GetNextAvailableSaveSlot()
        {
            foreach (var path in persistentDataPaths)
            {
                if (!Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(Application.dataPath + "/RemoteData/");

                if (!File.Exists(path))
                    return path;
            }

            return string.Empty;
        }

        public static void ResetPlayerData()
        {
            PlayerData data = new PlayerData();
            for (int i = 0; i < FactoryManager.Instance.SectorRemoteData.Count; i++)
            {
                data.AddSectorProgression(i, 0);
            }
            PlayerData = data;
        }

        public static string ExportPlayerPersistentData(PlayerData editorData, string saveSlot)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(saveSlot, export);

            return export;
        }

        private static string ExportPlayerPersistentMetadata(PlayerMetadata editorData)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(persistentMetadataPath, export);

            return export;
        }

        public static PlayerData ImportPlayerPersistentData(string saveSlot)
        {
            if (!Directory.Exists(saveSlot))
                System.IO.Directory.CreateDirectory(Application.dataPath + "/RemoteData/");

            if (!File.Exists(saveSlot))
            {
                PlayerData data = new PlayerData();
                for (int i = 0; i < FactoryManager.Instance.SectorRemoteData.Count; i++)
                {
                    data.AddSectorProgression(i, 0);
                }
                //ExportPlayerPersistentData(data, saveSlot);
                return data;
            }

            var loaded = JsonConvert.DeserializeObject<PlayerData>(File.ReadAllText(saveSlot));

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
            if (CurrentSaveFile != string.Empty)
                ExportPlayerPersistentData(PlayerData, CurrentSaveFile);

            if (PlayerMetadata.CurrentSaveFile == null)
            {
                SaveFileData newSaveFile = new SaveFileData
                {
                    Name = DateTime.Now.ToString(),
                    Date = DateTime.Now,
                    FilePath = CurrentSaveFile,
                    MissionFilePath = CurrentSaveFile.Replace("PlayerPersistentData", "MissionsCurrentData")
                };

                PlayerMetadata.SaveFiles.Add(newSaveFile);
            }

            ExportPlayerPersistentMetadata(PlayerMetadata);
        }
    }
}
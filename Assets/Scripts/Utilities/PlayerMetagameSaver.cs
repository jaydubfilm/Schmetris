using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StarSalvager
{
    public static class PlayerMetagameSaver
    {
        private static readonly string metaGameDataPath = Application.dataPath + "/RemoteData/PlayerMetagameData.data";
        public static PlayerMetagameData PlayerMetagameData;

        public static void Init()
        {
            PlayerMetagameData = ImportPlayerMetagameData();
        }

        public static string ExportPlayerMetagameData(MissionsMasterData editorData)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(metaGameDataPath, export);

            return export;
        }

        public static PlayerMetagameData ImportPlayerMetagameData()
        {
            if (!File.Exists(metaGameDataPath))
                return new PlayerMetagameData();

            var loaded = JsonConvert.DeserializeObject<PlayerMetagameData>(File.ReadAllText(metaGameDataPath));

            return loaded;
        }
    }
}
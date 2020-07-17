using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StarSalvager
{
    public static class MissionManager
    {
        public static MissionsCurrentData MissionsCurrentData
        {
            get
            {
                if (m_missionsCurrentData == null)
                    m_missionsCurrentData = ImportRemoteData();

                return m_missionsCurrentData;
            }
        }
        private static MissionsCurrentData m_missionsCurrentData = null;

        public static void Init()
        {
            
        }

        public static void AddMission(string missionName)
        {

        }

        private static string ExportRemoteData(MissionsCurrentData editorData)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(Application.dataPath + "/RemoteData/MissionsCurrentData.txt", export);

            return export;
        }

        private static MissionsCurrentData ImportRemoteData()
        {
            if (!File.Exists(Application.dataPath + "/RemoteData/MissionsCurrentData.txt"))
                return new MissionsCurrentData();

            var loaded = JsonConvert.DeserializeObject<MissionsCurrentData>(File.ReadAllText(Application.dataPath + "/RemoteData/MissionsCurrentData.txt"));

            return loaded;
        }
    }
}
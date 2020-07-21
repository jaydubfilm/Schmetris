using Newtonsoft.Json;
using StarSalvager.AI;
using StarSalvager.Factories;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StarSalvager
{
    public static class MissionManager
    {
        private static bool fromScriptable = true;
        
        private static MissionsTotalData MissionsTotalData
        {
            get
            {
                if (m_missionsTotalData == null)
                {
                    if (fromScriptable)
                    {
                        m_missionsTotalData = new MissionsTotalData();
                        foreach (var mission in FactoryManager.Instance.MissionRemoteData.GenerateMissionData())
                        {
                            m_missionsTotalData.AddMission(mission);
                        }
                    }
                    else
                    {
                        m_missionsTotalData = ImportMissionsTotalRemoteData();
                    }
                }

                return m_missionsTotalData;
            }
        }
        private static MissionsTotalData m_missionsTotalData = null;

        public static MissionsCurrentData MissionsCurrentData
        {
            get
            {
                if (m_missionsCurrentData == null)
                {

                    if (fromScriptable)
                    {
                        m_missionsCurrentData = new MissionsCurrentData();
                    }
                    else
                    {
                        m_missionsCurrentData = ImportMissionsCurrentRemoteData();
                    }
                }

                return m_missionsCurrentData;
            }
        }
        private static MissionsCurrentData m_missionsCurrentData = null;

        public static void Init()
        {
            AddMissionCurrent("Resource Mission 1");
            AddMissionCurrent("Enemy Mission 1");
        }

        public static void AddMissionCurrent(string missionName)
        {
            MissionsCurrentData.AddMission(MissionsTotalData.m_missionsTotalList.Find(m => m.m_missionName == missionName));
        }

        public static void ProcessResourceCollectedMissionData(BIT_TYPE resourceType, int amount)
        {
            Debug.Log("Resource mission event");
            for (int i = MissionsCurrentData.m_resourceCollectedMissions.Count - 1; i >= 0; i--)
            {
                ResourceCollectedMission mission = MissionsCurrentData.m_resourceCollectedMissions[i];
                mission.ProcessMissionData(resourceType, amount);
                if (mission.MissionComplete())
                {
                    Debug.Log("Mission " + mission.m_missionName + " Complete!");
                    MissionsCurrentData.m_resourceCollectedMissions.Remove(mission);
                    mission.MissionStatus = MISSION_STATUS.COMPLETED;
                    MissionsCurrentData.m_completedMissions.Add(mission);
                }
            }
        }

        public static void ProcessEnemyKilledMissionData(ENEMY_TYPE enemyType, int amount)
        {
            Debug.Log("Enemy killed mission event");
            for (int i = MissionsCurrentData.m_enemyKilledMissions.Count - 1; i >= 0; i--)
            {
                EnemyKilledMission mission = MissionsCurrentData.m_enemyKilledMissions[i];
                mission.ProcessMissionData(enemyType, amount);
                if (mission.MissionComplete())
                {
                    Debug.Log("Mission " + mission.m_missionName + " Complete!");
                    mission.MissionStatus = MISSION_STATUS.COMPLETED;
                    MissionsCurrentData.m_completedMissions.Add(mission);
                    MissionsCurrentData.m_enemyKilledMissions.RemoveAt(i);
                }
            }
        }

        private static string ExportMissionsCurrentRemoteData(MissionsCurrentData editorData)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(Application.dataPath + "/RemoteData/MissionsCurrentData.txt", export);

            return export;
        }

        private static string ExportMissionsTotalRemoteData(MissionsTotalData editorData)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(Application.dataPath + "/RemoteData/MissionsTotalData.txt", export);

            return export;
        }

        private static MissionsCurrentData ImportMissionsCurrentRemoteData()
        {
            if (!File.Exists(Application.dataPath + "/RemoteData/MissionsCurrentData.txt"))
                return new MissionsCurrentData();

            var loaded = JsonConvert.DeserializeObject<MissionsCurrentData>(File.ReadAllText(Application.dataPath + "/RemoteData/MissionsCurrentData.txt"));

            return loaded;
        }

        private static MissionsTotalData ImportMissionsTotalRemoteData()
        {
            if (!File.Exists(Application.dataPath + "/RemoteData/MissionsCurrentData.txt"))
                return new MissionsTotalData();

            var loaded = JsonConvert.DeserializeObject<MissionsTotalData>(File.ReadAllText(Application.dataPath + "/RemoteData/MissionsCurrentData.txt"));

            return loaded;
        }

        public static void OnApplicationQuit()
        {
            ExportMissionsCurrentRemoteData(MissionsCurrentData);
            ExportMissionsTotalRemoteData(MissionsTotalData);
        }
    }
}
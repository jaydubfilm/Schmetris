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

        private static readonly string currentDataPath = Application.dataPath + "/RemoteData/MissionsCurrentData.mission";
        private static readonly string masterDataPath = Application.dataPath + "/RemoteData/MissionsMasterData.mission";


        public static MissionsMasterData MissionsMasterData
        {
            get
            {
                if (m_missionsMasterData == null)
                {
                    if (fromScriptable)
                    {
                        m_missionsMasterData = new MissionsMasterData();
                        foreach (var mission in FactoryManager.Instance.MissionRemoteData.GenerateMissionData())
                        {
                            m_missionsMasterData.AddMission(mission);
                        }
                    }
                    else
                    {
                        m_missionsMasterData = ImportMissionsMasterRemoteData();
                    }
                }

                return m_missionsMasterData;
            }
        }
        private static MissionsMasterData m_missionsMasterData = null;

        public static MissionsCurrentData MissionsCurrentData
        {
            get
            {
                if (m_missionsCurrentData == null)
                {

                    if (fromScriptable)
                    {
                        m_missionsCurrentData = new MissionsCurrentData();
                        foreach (Mission mission in MissionsMasterData.m_missionsTotalList)
                        {
                            m_missionsCurrentData.m_notStartedMissions.Add(mission);
                        }
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
            AddMissionCurrent("Enemy Mission 2");
            AddMissionCurrent("Level Mission 1");
            AddMissionCurrent("Combo Mission 1");
        }

        public static void AddMissionCurrent(string missionName)
        {
            MissionsCurrentData.AddMission(MissionsMasterData.m_missionsTotalList.Find(m => m.m_missionName == missionName));
        }

        /*public static void ProcessMissionData<T>() where T : Mission
        {

        }*/

        //Next 4 functions receive information from outside the missionmanager when an event relevant to missions has occurred.
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
                    ProcessMissionCompleteUnlocks(mission.m_missionName);
                }
            }
        }

        public static void ProcessEnemyKilledMissionData(string enemyType, int amount)
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
                    ProcessMissionCompleteUnlocks(mission.m_missionName);
                }
            }
        }

        public static void ProcessComboBlocksMissionData(BIT_TYPE comboType, int amount)
        {
            Debug.Log("Combo Blocks mission event");
            for (int i = MissionsCurrentData.m_comboBlocksMissions.Count - 1; i >= 0; i--)
            {
                ComboBlocksMission mission = MissionsCurrentData.m_comboBlocksMissions[i];
                mission.ProcessMissionData(comboType, amount);
                if (mission.MissionComplete())
                {
                    Debug.Log("Mission " + mission.m_missionName + " Complete!");
                    mission.MissionStatus = MISSION_STATUS.COMPLETED;
                    MissionsCurrentData.m_completedMissions.Add(mission);
                    MissionsCurrentData.m_comboBlocksMissions.RemoveAt(i);
                    ProcessMissionCompleteUnlocks(mission.m_missionName);
                }
            }
        }

        public static void ProcessLevelProgressMissionData(int sectorNumber, int waveNumber)
        {
            Debug.Log("Level Progress mission event");
            for (int i = MissionsCurrentData.m_levelProgressMissions.Count - 1; i >= 0; i--)
            {
                LevelProgressMission mission = MissionsCurrentData.m_levelProgressMissions[i];
                mission.ProcessMissionData(sectorNumber, waveNumber);
                if (mission.MissionComplete())
                {
                    Debug.Log("Mission " + mission.m_missionName + " Complete!");
                    mission.MissionStatus = MISSION_STATUS.COMPLETED;
                    MissionsCurrentData.m_completedMissions.Add(mission);
                    MissionsCurrentData.m_levelProgressMissions.RemoveAt(i);
                    ProcessMissionCompleteUnlocks(mission.m_missionName);
                }
            }
        }

        public static void ProcessMissionCompleteUnlocks(string missionName)
        {
            Debug.Log("Mission Complete");
            for (int i = MissionsCurrentData.m_notStartedMissions.Count - 1; i >= 0; i--)
            {
                Mission mission = MissionsCurrentData.m_notStartedMissions[i];

                if (mission.MissionUnlockType == MISSION_UNLOCK_PARAMETERS.MISSION_COMPLETE)
                {
                    MissionsCurrentData.AddMission(mission);
                }
            }
        }

        public static string ExportMissionsCurrentRemoteData(MissionsCurrentData editorData)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(currentDataPath, export);

            return export;
        }

        public static string ExportMissionsMasterRemoteData(MissionsMasterData editorData)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(masterDataPath, export);

            return export;
        }

        public static MissionsCurrentData ImportMissionsCurrentRemoteData()
        {
            if (!File.Exists(currentDataPath))
                return new MissionsCurrentData();

            var loaded = JsonConvert.DeserializeObject<MissionsCurrentData>(File.ReadAllText(currentDataPath));

            return loaded;
        }

        public static MissionsMasterData ImportMissionsMasterRemoteData()
        {
            if (!File.Exists(masterDataPath))
                return new MissionsMasterData();

            var loaded = JsonConvert.DeserializeObject<MissionsMasterData>(File.ReadAllText(masterDataPath));

            return loaded;
        }

        public static void OnApplicationQuit()
        {
            ExportMissionsCurrentRemoteData(MissionsCurrentData);
            ExportMissionsMasterRemoteData(MissionsMasterData);
        }
    }
}
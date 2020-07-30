using Newtonsoft.Json;
using StarSalvager.Factories;
using System.IO;
using UnityEngine;

namespace StarSalvager.Missions
{
    public static class MissionManager
    {
        private static bool fromScriptable = false;

        private static readonly string REMOTEDATA_PATH = Application.dataPath + "/RemoteData/";
        private static readonly string currentDataPath = REMOTEDATA_PATH + "MissionsCurrentData.mission";
        private static readonly string masterDataPath = REMOTEDATA_PATH + "MissionsMasterData.mission";

        public static string recentCompletedMissionName = "";
        public static int recentCompletedSectorName;
        public static int recentCompletedWaveName;

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
                            m_missionsMasterData.m_missionsMasterData.Add(mission.ToMissionData());
                        }
                    }
                    else
                    {
                        m_missionsMasterData = ImportMissionsMasterRemoteData();
                    }
                    m_missionsMasterData.LoadMissionData();
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
                        foreach (Mission mission in MissionsMasterData.GetMasterMissions())
                        {
                            m_missionsCurrentData.m_notStartedMissionData.Add(mission.ToMissionData());
                        }
                    }
                    else
                    {
                        m_missionsCurrentData = ImportMissionsCurrentRemoteData();
                    }
                    m_missionsCurrentData.LoadMissionData();
                }

                return m_missionsCurrentData;
            }
        }
        private static MissionsCurrentData m_missionsCurrentData = null;

        public static void Init()
        {
            CheckUnlocks();
        }

        public static void AddMissionCurrent(string missionName)
        {
            MissionsCurrentData.AddMission(MissionsMasterData.GetMasterMissions().Find(m => m.m_missionName == missionName));
        }

        /*public static void ProcessMissionData<T>() where T : Mission
        {

        }*/

        //Next 4 functions receive information from outside the missionmanager when an event relevant to missions has occurred.
        public static void ProcessResourceCollectedMissionData(BIT_TYPE resourceType, int amount)
        {
            //Debug.Log("Resource mission event");
            for (int i = MissionsCurrentData.CurrentMissions.Count - 1; i >= 0; i--)
            {
                if (MissionsCurrentData.CurrentMissions[i] is ResourceCollectedMission resourceCollectedMission)
                {
                    resourceCollectedMission.ProcessMissionData(resourceType, amount);
                    if (resourceCollectedMission.MissionComplete())
                    {
                        Debug.Log("Mission " + resourceCollectedMission.m_missionName + " Complete!");
                        resourceCollectedMission.MissionStatus = MISSION_STATUS.COMPLETED;
                        MissionsCurrentData.CompleteMission(resourceCollectedMission);
                        MissionsCurrentData.CurrentMissions.RemoveAt(i);
                        ProcessMissionComplete(resourceCollectedMission.m_missionName);
                    }
                }
            }
        }

        public static void ProcessEnemyKilledMissionData(string enemyType, int amount)
        {
            //Debug.Log("Enemy killed mission event");
            for (int i = MissionsCurrentData.CurrentMissions.Count - 1; i >= 0; i--)
            {
                if (MissionsCurrentData.CurrentMissions[i] is EnemyKilledMission enemyKilledMission)
                {
                    enemyKilledMission.ProcessMissionData(enemyType, amount);
                    if (enemyKilledMission.MissionComplete())
                    {
                        Debug.Log("Mission " + enemyKilledMission.m_missionName + " Complete!");
                        enemyKilledMission.MissionStatus = MISSION_STATUS.COMPLETED;
                        MissionsCurrentData.CompleteMission(enemyKilledMission);
                        MissionsCurrentData.CurrentMissions.RemoveAt(i);
                        ProcessMissionComplete(enemyKilledMission.m_missionName);
                    }
                }
            }
        }

        public static void ProcessComboBlocksMissionData(BIT_TYPE comboType, int amount)
        {
            //Debug.Log("Combo Blocks mission event");
            for (int i = MissionsCurrentData.CurrentMissions.Count - 1; i >= 0; i--)
            {
                if (MissionsCurrentData.CurrentMissions[i] is ComboBlocksMission comboBlocksMission)
                {
                    comboBlocksMission.ProcessMissionData(comboType, amount);
                    if (comboBlocksMission.MissionComplete())
                    {
                        Debug.Log("Mission " + comboBlocksMission.m_missionName + " Complete!");
                        comboBlocksMission.MissionStatus = MISSION_STATUS.COMPLETED;
                        MissionsCurrentData.CompleteMission(comboBlocksMission);
                        MissionsCurrentData.CurrentMissions.RemoveAt(i);
                        ProcessMissionComplete(comboBlocksMission.m_missionName);
                    }
                }
            }
        }

        public static void ProcessLevelProgressMissionData(int sectorNumber, int waveNumber)
        {
            //Debug.Log("Level Progress mission event");
            for (int i = MissionsCurrentData.CurrentMissions.Count - 1; i >= 0; i--)
            {
                if (MissionsCurrentData.CurrentMissions[i] is LevelProgressMission levelProgressMission)
                {
                    levelProgressMission.ProcessMissionData(sectorNumber, waveNumber);
                    if (levelProgressMission.MissionComplete())
                    {
                        Debug.Log("Mission " + levelProgressMission.m_missionName + " Complete!");
                        levelProgressMission.MissionStatus = MISSION_STATUS.COMPLETED;
                        MissionsCurrentData.CompleteMission(levelProgressMission);
                        MissionsCurrentData.CurrentMissions.RemoveAt(i);
                        ProcessMissionComplete(levelProgressMission.m_missionName);
                    }
                }
            }
            ProcessWaveComplete(sectorNumber, waveNumber);
        }

        public static void ProcessMissionComplete(string missionName)
        {
            Toast.AddToast(missionName + " Successful!!!!", time: 3.0f, verticalLayout: Toast.Layout.Start, horizontalLayout: Toast.Layout.End);
            recentCompletedMissionName = missionName;
            CheckUnlocks();
        }

        private static void ProcessWaveComplete(int sectorNumber, int waveNumber)
        {
            recentCompletedSectorName = sectorNumber;
            recentCompletedWaveName = waveNumber;
            CheckUnlocks();
        }

        private static void CheckUnlocks()
        {
            for (int i = MissionsCurrentData.NotStartedMissions.Count - 1; i >= 0; i--)
            {
                Mission mission = MissionsCurrentData.NotStartedMissions[i];

                if (mission.CheckUnlockParameters())
                {
                    MissionsCurrentData.AddMission(mission);
                }
            }
        }

        public static string ExportMissionsCurrentRemoteData(MissionsCurrentData editorData)
        {
            editorData.SaveMissionData();
            
            if (!Directory.Exists(REMOTEDATA_PATH))
                System.IO.Directory.CreateDirectory(REMOTEDATA_PATH);
            
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(currentDataPath, export);

            return export;
        }

        public static string ExportMissionsMasterRemoteData(MissionsMasterData editorData)
        {
            editorData.SaveMissionData();

            if (!Directory.Exists(REMOTEDATA_PATH))
                System.IO.Directory.CreateDirectory(REMOTEDATA_PATH);

            
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(masterDataPath, export);

            return export;
        }

        public static MissionsCurrentData ImportMissionsCurrentRemoteData()
        {
            if (!Directory.Exists(REMOTEDATA_PATH))
                System.IO.Directory.CreateDirectory(REMOTEDATA_PATH);

            if (!File.Exists(currentDataPath))
            {
                MissionsCurrentData currentData = new MissionsCurrentData();
                foreach (Mission mission in MissionsMasterData.GetMasterMissions())
                {
                    currentData.m_notStartedMissionData.Add(mission.ToMissionData());
                }
                return currentData;
            }

            var loaded = JsonConvert.DeserializeObject<MissionsCurrentData>(File.ReadAllText(currentDataPath));

            return loaded;
        }

        public static MissionsMasterData ImportMissionsMasterRemoteData()
        {
            if (!Directory.Exists(REMOTEDATA_PATH))
                System.IO.Directory.CreateDirectory(REMOTEDATA_PATH);

            if (!File.Exists(masterDataPath))
            {
                MissionsMasterData masterData = new MissionsMasterData();
                foreach (var mission in FactoryManager.Instance.MissionRemoteData.GenerateMissionData())
                {
                    masterData.m_missionsMasterData.Add(mission.ToMissionData());
                }
                return masterData;
            }

            var loaded = JsonConvert.DeserializeObject<MissionsMasterData>(File.ReadAllText(masterDataPath));

            return loaded;
        }

        public static void CustomOnApplicationQuit()
        {
            ExportMissionsCurrentRemoteData(MissionsCurrentData);
            ExportMissionsMasterRemoteData(MissionsMasterData);
        }
    }
}
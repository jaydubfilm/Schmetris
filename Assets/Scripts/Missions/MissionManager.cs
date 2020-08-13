using Newtonsoft.Json;
using StarSalvager.Factories;
using StarSalvager.Values;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StarSalvager.Missions
{
    public static class MissionManager
    {
        private static bool fromScriptable = true;

        private static readonly string REMOTEDATA_PATH = Application.dataPath + "/RemoteData/";
        private static readonly List<string> currentDataPaths = new List<string>
        {
            REMOTEDATA_PATH + "MissionsCurrentDataSaveFile0.mission",
            REMOTEDATA_PATH + "MissionsCurrentDataSaveFile1.mission",
            REMOTEDATA_PATH + "MissionsCurrentDataSaveFile2.mission"
        };
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

        public static MissionsCurrentData MissionsCurrentData = new MissionsCurrentData();

        private static int CurrentSaveFile = -1;

        public static void SetCurrentSaveFile()
        {
            int index = PlayerPersistentData.PlayerMetadata.GetSaveFileAtIndex(0);

            if (CurrentSaveFile >= 0)
            {
                ExportMissionsCurrentRemoteData(MissionsCurrentData, CurrentSaveFile);
            }
            else if (index == CurrentSaveFile)
            {
                return;
            }

            CurrentSaveFile = index;
            MissionsCurrentData = ImportMissionsCurrentRemoteData(index);
            MissionsCurrentData.LoadMissionData();
            CheckUnlocks();
        }

        public static void Init()
        {
        }

        public static void AddMissionCurrent(string missionName)
        {
            MissionsCurrentData.AddMission(MissionsMasterData.GetMasterMissions().Find(m => m.m_missionName == missionName));
        }

        /*public static void ProcessMissionData<T>() where T : Mission
        {

        }*/

        //Next functions receive information from outside the missionmanager when an event relevant to missions has occurred.
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

        public static void ProcessLiquidResourceConvertedMission(BIT_TYPE resourceType, float amount)
        {
            //Debug.Log("Resource mission event");
            for (int i = MissionsCurrentData.CurrentMissions.Count - 1; i >= 0; i--)
            {
                if (MissionsCurrentData.CurrentMissions[i] is LiquidResourceConvertedMission liquidResourceConvertedMission)
                {
                    liquidResourceConvertedMission.ProcessMissionData(resourceType, amount);
                    if (liquidResourceConvertedMission.MissionComplete())
                    {
                        Debug.Log("Mission " + liquidResourceConvertedMission.m_missionName + " Complete!");
                        liquidResourceConvertedMission.MissionStatus = MISSION_STATUS.COMPLETED;
                        MissionsCurrentData.CompleteMission(liquidResourceConvertedMission);
                        MissionsCurrentData.CurrentMissions.RemoveAt(i);
                        ProcessMissionComplete(liquidResourceConvertedMission.m_missionName);
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

        public static void ProcessComboBlocksMissionData(BIT_TYPE comboType, int comboLevel, int amount)
        {
            //Debug.Log("Combo Blocks mission event");
            for (int i = MissionsCurrentData.CurrentMissions.Count - 1; i >= 0; i--)
            {
                if (MissionsCurrentData.CurrentMissions[i] is ComboBlocksMission comboBlocksMission)
                {
                    comboBlocksMission.ProcessMissionData(comboType, comboLevel, amount);
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

        public static void ProcessChainWavesMissionData(int waveNumber)
        {
            //Debug.Log("Chain Waves mission event");
            for (int i = MissionsCurrentData.CurrentMissions.Count - 1; i >= 0; i--)
            {
                if (MissionsCurrentData.CurrentMissions[i] is ChainWavesMission chainWavesMission)
                {
                    chainWavesMission.ProcessMissionData(waveNumber);
                    if (chainWavesMission.MissionComplete())
                    {
                        Debug.Log("Mission " + chainWavesMission.m_missionName + " Complete!");
                        chainWavesMission.MissionStatus = MISSION_STATUS.COMPLETED;
                        MissionsCurrentData.CompleteMission(chainWavesMission);
                        MissionsCurrentData.CurrentMissions.RemoveAt(i);
                        ProcessMissionComplete(chainWavesMission.m_missionName);
                    }
                }
            }
        }

        public static void ProcessCraftPartMissionData(PART_TYPE partType, int level)
        {
            //Debug.Log("Craft part mission event");
            for (int i = MissionsCurrentData.CurrentMissions.Count - 1; i >= 0; i--)
            {
                if (MissionsCurrentData.CurrentMissions[i] is CraftPartMission craftPartMission)
                {
                    craftPartMission.ProcessMissionData(partType, level);
                    if (craftPartMission.MissionComplete())
                    {
                        Debug.Log("Mission " + craftPartMission.m_missionName + " Complete!");
                        craftPartMission.MissionStatus = MISSION_STATUS.COMPLETED;
                        MissionsCurrentData.CompleteMission(craftPartMission);
                        MissionsCurrentData.CurrentMissions.RemoveAt(i);
                        ProcessMissionComplete(craftPartMission.m_missionName);
                    }
                }
            }
        }

        public static void ProcessWhiteBumperMissionData(int bitsShifted, bool shiftedThroughCenter)
        {
            //Debug.Log("White Bumper mission event");
            for (int i = MissionsCurrentData.CurrentMissions.Count - 1; i >= 0; i--)
            {
                if (MissionsCurrentData.CurrentMissions[i] is WhiteBumperMission whiteBumperMission)
                {
                    whiteBumperMission.ProcessMissionData(shiftedThroughCenter, PART_TYPE.CORE, bitsShifted);
                    if (whiteBumperMission.MissionComplete())
                    {
                        Debug.Log("Mission " + whiteBumperMission.m_missionName + " Complete!");
                        whiteBumperMission.MissionStatus = MISSION_STATUS.COMPLETED;
                        MissionsCurrentData.CompleteMission(whiteBumperMission);
                        MissionsCurrentData.CurrentMissions.RemoveAt(i);
                        ProcessMissionComplete(whiteBumperMission.m_missionName);
                    }
                }
            }
        }

        public static void ProcessAsteroidCollisionMissionData(BIT_TYPE? bitType, int amount)
        {
            Debug.Log("Asteroid collision mission event");
            for (int i = MissionsCurrentData.CurrentMissions.Count - 1; i >= 0; i--)
            {
                if (MissionsCurrentData.CurrentMissions[i] is AsteroidCollisionMission asteroidCollisionMission)
                {
                    asteroidCollisionMission.ProcessMissionData(bitType, amount);
                    if (asteroidCollisionMission.MissionComplete())
                    {
                        Debug.Log("Mission " + asteroidCollisionMission.m_missionName + " Complete!");
                        asteroidCollisionMission.MissionStatus = MISSION_STATUS.COMPLETED;
                        MissionsCurrentData.CompleteMission(asteroidCollisionMission);
                        MissionsCurrentData.CurrentMissions.RemoveAt(i);
                        ProcessMissionComplete(asteroidCollisionMission.m_missionName);
                    }
                }
            }
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

        public static string ExportMissionsCurrentRemoteData(MissionsCurrentData editorData, int saveSlot)
        {
            editorData.SaveMissionData();
            
            if (!Directory.Exists(REMOTEDATA_PATH))
                System.IO.Directory.CreateDirectory(REMOTEDATA_PATH);
            
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(currentDataPaths[saveSlot], export);

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

        public static MissionsCurrentData ImportMissionsCurrentRemoteData(int saveSlot)
        {
            if (!Directory.Exists(REMOTEDATA_PATH))
                System.IO.Directory.CreateDirectory(REMOTEDATA_PATH);

            if (!File.Exists(currentDataPaths[saveSlot]))
            {
                MissionsCurrentData currentData = new MissionsCurrentData();
                foreach (Mission mission in MissionsMasterData.GetMasterMissions())
                {
                    currentData.m_notStartedMissionData.Add(mission.ToMissionData());
                }
                return currentData;
            }

            var loaded = JsonConvert.DeserializeObject<MissionsCurrentData>(File.ReadAllText(currentDataPaths[saveSlot]));

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

        public static void ResetMissionData()
        {
            MissionsCurrentData currentData = new MissionsCurrentData();
            foreach (Mission mission in MissionsMasterData.GetMasterMissions())
            {
                currentData.m_notStartedMissionData.Add(mission.ToMissionData());
            }
            MissionsCurrentData = currentData;

            MissionsCurrentData.LoadMissionData();
            CheckUnlocks();
        }

        public static void CustomOnApplicationQuit()
        {
            SaveMissionDatas();
        }

        public static void SaveMissionDatas()
        {
            if (CurrentSaveFile >= 0)
            {
                ExportMissionsCurrentRemoteData(MissionsCurrentData, CurrentSaveFile);
                ExportMissionsMasterRemoteData(MissionsMasterData);
            }
        }
    }
}
using StarSalvager.Factories;
using StarSalvager.Values;
using StarSalvager.Utilities.FileIO;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    public static class MissionManager
    {
        private static readonly bool fromScriptable = true;

        public static string RecentCompletedMissionName = "";
        public static int RecentCompletedSectorName;
        public static int RecentCompletedWaveName;

        private static bool HasInit;
        public static List<Mission> MissionTypes 
        { get
            {
                if (!HasInit)
                {
                    Init();
                }
                return m_missionTypes;
            } 
        }
        private static List<Mission> m_missionTypes;

        public static void Init()
        {
            m_missionTypes = new List<Mission>();

            m_missionTypes.Add(new AsteroidCollisionMission(new MissionRemoteData()));
            m_missionTypes.Add(new ResourceCollectedMission(new MissionRemoteData()));
            m_missionTypes.Add(new EnemyKilledMission(new MissionRemoteData()));
            m_missionTypes.Add(new LevelProgressMission(new MissionRemoteData()));
            m_missionTypes.Add(new ComboBlocksMission(new MissionRemoteData()));
            m_missionTypes.Add(new CraftPartMission(new MissionRemoteData()));
            m_missionTypes.Add(new WhiteBumperMission(new MissionRemoteData()));
            m_missionTypes.Add(new ChainWavesMission(new MissionRemoteData()));
            m_missionTypes.Add(new LiquidResourceConvertedMission(new MissionRemoteData()));
            m_missionTypes.Add(new SectorsCompletedMission(new MissionRemoteData()));
            m_missionTypes.Add(new FlightLengthMission(new MissionRemoteData()));
            m_missionTypes.Add(new ChainBonusShapesMission(new MissionRemoteData()));
            m_missionTypes.Add(new FacilityUpgradeMission(new MissionRemoteData()));
            m_missionTypes.Add(new PlayerLevelMission(new MissionRemoteData()));
            m_missionTypes.Add(new ComponentCollectedMission(new MissionRemoteData()));

            HasInit = true;
        }

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
                        m_missionsMasterData = Files.ImportMissionsMasterRemoteData();
                    }
                    m_missionsMasterData.LoadMissionData();
                }

                return m_missionsMasterData;
            }
        }
        private static MissionsMasterData m_missionsMasterData = null;

        public static MissionsCurrentData MissionsCurrentData => PlayerPersistentData.PlayerData.missionsCurrentData;

        public static void LoadMissionData()
        {
            if (MissionsCurrentData == null)
            {
                ResetMissionData();
            }
            
            MissionsCurrentData.LoadMissionData();
            CheckUnlocks();
        }

        public static void AddMissionCurrent(string missionName)
        {
            MissionsCurrentData.AddMission(MissionsMasterData.GetMasterMissions().Find(m => m.m_missionName == missionName));
        }

        public static void ProcessMissionData(Type missionType, MissionProgressEventData missionProgressEventData)
        {
            for (int i = MissionsCurrentData.CurrentMissions.Count - 1; i >= 0; i--)
            {
                Mission mission = MissionsCurrentData.CurrentMissions[i];

                if (mission.GetType() == missionType)
                {
                    mission.ProcessMissionData(missionProgressEventData);
                    if (mission.MissionComplete())
                    {
                        Debug.Log("Mission " + mission.m_missionName + " Complete!");
                        mission.MissionStatus = MISSION_STATUS.COMPLETED;
                        MissionsCurrentData.CompleteMission(mission);
                        ProcessMissionComplete(mission.m_missionName);
                    }
                }
            }
        }

        public static void ProcessMissionComplete(string missionName)
        {
            Toast.AddToast(missionName + " Successful!!!!", time: 3.0f, verticalLayout: Toast.Layout.Start, horizontalLayout: Toast.Layout.End);
            LevelManager.Instance.MissionsCompletedDuringThisFlight.Add(missionName);
            RecentCompletedMissionName = missionName;
            if (LevelManager.Instance.WaveEndSummaryData != null)
            {
                LevelManager.Instance.WaveEndSummaryData.missionCompletedStrings.Add(missionName);
            }

            CheckUnlocks();
        }

        private static void ProcessWaveComplete(int sectorNumber, int waveNumber)
        {
            RecentCompletedSectorName = sectorNumber;
            RecentCompletedWaveName = waveNumber;
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

                    if (LevelManager.Instance != null && LevelManager.Instance.WaveEndSummaryData != null)
                    {
                        LevelManager.Instance.WaveEndSummaryData.missionUnlockedStrings.Add(mission.m_missionName);
                    }
                }
            }
        }



        private static void ResetMissionData()
        {
            MissionsCurrentData currentData = new MissionsCurrentData();
            currentData.ResetMissionData();
            PlayerPersistentData.PlayerData.missionsCurrentData = currentData;
        }

        public static void CustomOnApplicationQuit()
        {
            SaveMissionDatas();
        }

        public static void SaveMissionDatas()
        {
            Files.ExportMissionsMasterRemoteData(MissionsMasterData);
        }
    }
}
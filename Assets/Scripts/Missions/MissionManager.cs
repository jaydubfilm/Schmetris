using StarSalvager.Factories;
using StarSalvager.Values;
using StarSalvager.Utilities.FileIO;
using UnityEngine;
using System;
using System.Collections.Generic;
using StarSalvager.Utilities.Saving;

namespace StarSalvager.Missions
{
    public static class MissionManager
    {
        private static readonly bool fromScriptable = true;

        public static string RecentCompletedMissionName = "";

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
            m_missionTypes = new List<Mission>
            {
                new AsteroidCollisionMission(new MissionRemoteData()),
                new ResourceCollectedMission(new MissionRemoteData()),
                new EnemyKilledMission(new MissionRemoteData()),
                new LevelProgressMission(new MissionRemoteData()),
                new ComboBlocksMission(new MissionRemoteData()),
                new CraftPartMission(new MissionRemoteData()),
                new WhiteBumperMission(new MissionRemoteData()),
                new ChainWavesMission(new MissionRemoteData()),
                new LiquidResourceConvertedMission(new MissionRemoteData()),
                new SectorsCompletedMission(new MissionRemoteData()),
                new FlightLengthMission(new MissionRemoteData()),
                new ChainBonusShapesMission(new MissionRemoteData()),
                new FacilityUpgradeMission(new MissionRemoteData()),
                new PlayerLevelMission(new MissionRemoteData()),
                new ComponentCollectedMission(new MissionRemoteData())
            };


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

        public static MissionsCurrentData MissionsCurrentData => PlayerDataManager.GetMissionsCurrentData();

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
            MissionsCurrentData.AddMission(MissionsMasterData.GetMasterMissions().Find(m => m.missionName == missionName));
        }

        public static void ProcessMissionData(Type missionType, MissionProgressEventData missionProgressEventData)
        {
            //Dont want to process our missions during the tutorial
            if (Globals.UsingTutorial)
                return;
            
            for (int i = MissionsCurrentData.CurrentMissions.Count - 1; i >= 0; i--)
            {
                Mission mission = MissionsCurrentData.CurrentMissions[i];

                if (mission.GetType() == missionType)
                {
                    mission.ProcessMissionData(missionProgressEventData);
                    if (mission.MissionComplete())
                    {
                        Debug.Log("Mission " + mission.missionName + " Complete!");
                        mission.MissionStatus = MISSION_STATUS.COMPLETED;
                        MissionsCurrentData.CompleteMission(mission);
                        ProcessMissionComplete(mission.missionName);
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
                LevelManager.Instance.WaveEndSummaryData.AddCompletedMission(missionName);
            }

            CheckUnlocks();
        }

        public static void ProcessWaveComplete()
        {
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
                        LevelManager.Instance.WaveEndSummaryData.AddUnlockedMission(mission.missionName);
                    }
                }
            }
        }



        private static void ResetMissionData()
        {
            MissionsCurrentData currentData = new MissionsCurrentData();
            currentData.ResetMissionData();
            PlayerDataManager.SetMissionsCurrentData(currentData);
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
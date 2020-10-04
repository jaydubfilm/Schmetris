using StarSalvager.Missions;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections.Generic;

namespace StarSalvager.Utilities.Extensions
{
    public static class MissionDataExtensions
    {
        public static List<Mission> ImportMissionDatas(this List<MissionData> missionDatas)
        {
            List<Mission> missions = new List<Mission>();

            foreach (MissionData missionData in missionDatas)
            {
                int i = MissionManager.MissionTypes.FindIndex(m => m.MissionEventType == missionData.MissionEventType);

                Mission newMission = (Mission)Activator.CreateInstance(MissionManager.MissionTypes[i].GetType(), missionData);
                missions.Add(newMission);

                /*switch (missionData.ClassType)
                {
                    case "ResourceCollectedMission":
                        missions.Add(new ResourceCollectedMission(missionData.ResourceType, missionData.IsFromEnemyLoot, missionData.MissionName, missionData.MissionDescription, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "EnemyKilledMission":
                        missions.Add(new EnemyKilledMission(missionData.EnemyType, missionData.MissionName, missionData.MissionDescription, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "LevelProgressMission":
                        missions.Add(new LevelProgressMission(missionData.SectorNumber, missionData.WaveNumber, missionData.MissionName, missionData.MissionDescription, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "ComboBlocksMission":
                        missions.Add(new ComboBlocksMission(missionData.ResourceType, missionData.ComboLevel, missionData.IsAdvancedCombo, missionData.MissionName, missionData.MissionDescription, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "CraftPartMission":
                        missions.Add(new CraftPartMission(missionData.PartType, missionData.PartLevel, missionData.MissionName, missionData.MissionDescription, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "WhiteBumperMission":
                        missions.Add(new WhiteBumperMission(missionData.ThroughPart, missionData.OrphanBit, missionData.HasCombos, missionData.PartType, missionData.MissionName, missionData.MissionDescription, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "AsteroidCollisionMission":
                        missions.Add(new AsteroidCollisionMission(missionData.ResourceType, missionData.MissionName, missionData.MissionDescription, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "ChainWavesMission":
                        missions.Add(new ChainWavesMission(missionData.WaveNumber, missionData.MissionName, missionData.MissionDescription, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "LiquidResourceConvertedMission":
                        missions.Add(new LiquidResourceConvertedMission(missionData.ResourceType, missionData.MissionName, missionData.MissionDescription, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "SectorsCompletedMission":
                        missions.Add(new SectorsCompletedMission(missionData.MissionName, missionData.MissionDescription, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "FlightLengthMission":
                        missions.Add(new FlightLengthMission(missionData.FlightLength, missionData.MissionName, missionData.MissionDescription, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "ChainBonusShapesMission":
                        missions.Add(new ChainBonusShapesMission(missionData.BonusShapeNumber, missionData.MissionName, missionData.MissionDescription, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "FacilityUpgradeMission":
                        missions.Add(new FacilityUpgradeMission(missionData.FacilityType, missionData.FacilityLevel, missionData.MissionName, missionData.MissionDescription, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "PlayerLevelMission":
                        missions.Add(new PlayerLevelMission(missionData.PlayerLevel, missionData.MissionName, missionData.MissionDescription, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "ComponentCollectedMission":
                        missions.Add(new ComponentCollectedMission(missionData.ComponentType, missionData.MissionName, missionData.MissionDescription, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                }*/
            }

            return missions;
        }
    }
}
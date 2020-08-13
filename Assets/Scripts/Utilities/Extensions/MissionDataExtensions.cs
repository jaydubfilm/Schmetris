using StarSalvager.Factories;
using StarSalvager.Missions;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class MissionDataExtensions
    {
        public static List<Mission> ImportMissionDatas(this List<MissionData> missionDatas)
        {
            List<Mission> missions = new List<Mission>();

            foreach (MissionData missionData in missionDatas)
            {
                switch (missionData.ClassType)
                {
                    case "ResourceCollectedMission":
                        missions.Add(new ResourceCollectedMission(missionData.ResourceType, missionData.MissionName, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "EnemyKilledMission":
                        missions.Add(new EnemyKilledMission(missionData.EnemyType, missionData.MissionName, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "LevelProgressMission":
                        missions.Add(new LevelProgressMission(missionData.SectorNumber, missionData.WaveNumber, missionData.MissionName, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "ComboBlocksMission":
                        missions.Add(new ComboBlocksMission(missionData.ResourceType, missionData.ComboLevel, missionData.MissionName, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "CraftPartMission":
                        missions.Add(new CraftPartMission(missionData.PartType, missionData.PartLevel, missionData.MissionName, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "WhiteBumperMission":
                        missions.Add(new WhiteBumperMission(missionData.ThroughPart, missionData.PartType, missionData.MissionName, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                    case "AsteroidCollisionMission":
                        missions.Add(new AsteroidCollisionMission(missionData.ResourceType, missionData.MissionName, missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas(), missionData.AmountNeeded));
                        break;
                }
            }

            return missions;
        }
    }
}
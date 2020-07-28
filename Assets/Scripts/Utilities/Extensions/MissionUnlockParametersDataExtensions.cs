using StarSalvager.Factories;
using StarSalvager.Missions;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class MissionUnlockParametersDataExtensions
    {
        public static List<MissionUnlockCheck> ImportMissionUnlockParametersDatas(this List<MissionUnlockCheckData> missionUnlockParameterDatas)
        {
            List<MissionUnlockCheck> missionUnlockChecks = new List<MissionUnlockCheck>();

            foreach (MissionUnlockCheckData missionUnlockParameterData in missionUnlockParameterDatas)
            {
                switch (missionUnlockParameterData.ClassType)
                {
                    case "MissionCompleteMissionUnlockCheck":
                        missionUnlockChecks.Add(new MissionCompleteMissionUnlockCheck(missionUnlockParameterData.MissionName));
                        break;
                    case "LevelCompleteMissionUnlockCheck":
                        missionUnlockChecks.Add(new LevelCompleteMissionUnlockCheck(missionUnlockParameterData.SectorNumber, missionUnlockParameterData.WaveNumber));
                        break;
                }
            }

            return missionUnlockChecks;
        }

        public static List<MissionUnlockCheckData> ImportMissionUnlockParametersDatas(this List<MissionUnlockCheck> missionUnlockChecks)
        {
            List<MissionUnlockCheckData> missionUnlockParameterDatas = new List<MissionUnlockCheckData>();

            foreach (MissionUnlockCheck missionUnlockCheck in missionUnlockChecks)
            {
                missionUnlockParameterDatas.Add(missionUnlockCheck.ToMissionUnlockParameterData());
            }

            return missionUnlockParameterDatas;
        }
    }
}
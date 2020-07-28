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
        public static List<IMissionUnlockCheck> ImportMissionUnlockParametersDatas(this List<MissionUnlockCheckData> missionUnlockParameterDatas)
        {
            List<IMissionUnlockCheck> missionUnlockChecks = new List<IMissionUnlockCheck>();

            foreach (MissionUnlockCheckData missionUnlockParameterData in missionUnlockParameterDatas)
            {
                switch (missionUnlockParameterData.ClassType)
                {
                    case nameof(MissionCompleteUnlockCheck):
                        missionUnlockChecks.Add(new MissionCompleteUnlockCheck(missionUnlockParameterData.MissionName));
                        break;
                    case nameof(LevelCompleteUnlockCheck):
                        missionUnlockChecks.Add(new LevelCompleteUnlockCheck(missionUnlockParameterData.SectorNumber, missionUnlockParameterData.WaveNumber));
                        break;
                }
            }

            return missionUnlockChecks;
        }

        public static List<MissionUnlockCheckData> ExportMissionUnlockParametersDatas(this List<IMissionUnlockCheck> missionUnlockChecks)
        {
            List<MissionUnlockCheckData> missionUnlockParameterDatas = new List<MissionUnlockCheckData>();

            foreach (IMissionUnlockCheck missionUnlockCheck in missionUnlockChecks)
            {
                missionUnlockParameterDatas.Add(missionUnlockCheck.ToMissionUnlockParameterData());
            }

            return missionUnlockParameterDatas;
        }
    }
}
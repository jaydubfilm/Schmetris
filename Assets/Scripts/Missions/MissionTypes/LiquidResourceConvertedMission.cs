using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class LiquidResourceConvertedMission : Mission
    {
        public BIT_TYPE? m_resourceType;

        public LiquidResourceConvertedMission(MissionRemoteData missionRemoteData) : base(missionRemoteData)
        {
            MissionEventType = MISSION_EVENT_TYPE.LIQUID_RESOURCE;
            m_resourceType = missionRemoteData.ResourceValue();
        }

        public LiquidResourceConvertedMission(MissionData missionData) : base(missionData)
        {
            MissionEventType = MISSION_EVENT_TYPE.LIQUID_RESOURCE;
            m_resourceType = missionData.BitType;
        }

        public override bool MissionComplete()
        {
            return currentAmount >= amountNeeded;
        }

        public override void ProcessMissionData(MissionProgressEventData missionProgressEventData)
        {
            BIT_TYPE bitType = missionProgressEventData.bitType.Value;
            float amount = missionProgressEventData.floatAmount;

            if (!m_resourceType.HasValue || bitType == m_resourceType)
            {
                currentAmount += amount;
            }
        }

        public override MissionData ToMissionData()
        {
            return new MissionData
            {
                ClassType = GetType().Name,
                MissionName = missionName,
                MissionDescription = missionDescription,
                AmountNeeded = amountNeeded,
                CurrentAmount = currentAmount,
                MissionEventType = this.MissionEventType,
                MissionStatus = this.MissionStatus,
                MissionUnlockChecks = missionUnlockChecks.ExportMissionUnlockParametersDatas(),

                BitType = m_resourceType
            };
        }
    }
}
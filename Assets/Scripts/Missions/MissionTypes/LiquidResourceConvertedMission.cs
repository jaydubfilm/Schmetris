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
            return m_currentAmount >= m_amountNeeded;
        }

        public override void ProcessMissionData(MissionProgressEventData missionProgressEventData)
        {
            BIT_TYPE bitType = missionProgressEventData.bitType.Value;
            float amount = missionProgressEventData.floatAmount;

            if (!m_resourceType.HasValue || bitType == m_resourceType)
            {
                m_currentAmount += amount;
            }
        }

        public override MissionData ToMissionData()
        {
            return new MissionData
            {
                ClassType = GetType().Name,
                MissionName = m_missionName,
                MissionDescription = m_missionDescription,
                AmountNeeded = m_amountNeeded,
                CurrentAmount = m_currentAmount,
                MissionEventType = this.MissionEventType,
                MissionStatus = this.MissionStatus,
                MissionUnlockChecks = missionUnlockChecks.ExportMissionUnlockParametersDatas(),

                BitType = m_resourceType
            };
        }
    }
}
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class LiquidResourceConvertedMission : Mission
    {
        public BIT_TYPE? m_resourceType;

        public LiquidResourceConvertedMission(BIT_TYPE? resourceType, string missionName, string missionDescription, List<IMissionUnlockCheck> missionUnlockData, float amountNeeded) : base(missionName, missionDescription, amountNeeded, missionUnlockData)
        {
            MissionEventType = MISSION_EVENT_TYPE.LIQUID_RESOURCE;
            m_resourceType = resourceType;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData(BIT_TYPE resourceType, float amount)
        {
            if (m_resourceType == null || resourceType == m_resourceType)
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

                ResourceType = m_resourceType
            };
        }
    }
}
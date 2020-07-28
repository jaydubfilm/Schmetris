using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class ResourceCollectedMission : Mission
    {
        public BIT_TYPE m_resourceType;

        public ResourceCollectedMission(BIT_TYPE resourceType, string missionName, List<MissionUnlockCheck> missionUnlockData, int amountNeeded) : base(missionName, amountNeeded, missionUnlockData)
        {
            MissionEventType = MISSION_EVENT_TYPE.RESOURCE_COLLECTED;
            m_resourceType = resourceType;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData(BIT_TYPE resourceType, int amount)
        {
            if (resourceType == m_resourceType)
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
                AmountNeeded = m_amountNeeded,
                CurrentAmount = m_currentAmount,
                MissionEventType = this.MissionEventType,
                MissionStatus = this.MissionStatus,
                MissionUnlockChecks = missionUnlockChecks.ImportMissionUnlockParametersDatas(),

                ResourceType = m_resourceType
            };
        }
    }
}
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class ComponentCollectedMission : Mission
    {
        public COMPONENT_TYPE? m_componentType;

        public ComponentCollectedMission(COMPONENT_TYPE? componentType, string missionName, string missionDescription, List<IMissionUnlockCheck> missionUnlockData, float amountNeeded) : base(missionName, missionDescription, amountNeeded, missionUnlockData)
        {
            MissionEventType = MISSION_EVENT_TYPE.COMPONENT_COLLECTED;
            m_componentType = componentType;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData(COMPONENT_TYPE componentType, int amount)
        {
            if (m_componentType == null || componentType == m_componentType)
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

                ComponentType = m_componentType,
            };
        }
    }
}
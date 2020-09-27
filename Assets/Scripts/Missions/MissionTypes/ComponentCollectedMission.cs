using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class ComponentCollectedMission : Mission
    {
        public COMPONENT_TYPE? m_componentType;

        public ComponentCollectedMission(MissionRemoteData missionRemoteData) : base(missionRemoteData)
        {
            MissionEventType = MISSION_EVENT_TYPE.COMPONENT_COLLECTED;
            m_componentType = missionRemoteData.ComponentValue();
        }

        public ComponentCollectedMission(MissionData missionData) : base(missionData)
        {
            MissionEventType = MISSION_EVENT_TYPE.COMPONENT_COLLECTED;
            m_componentType = missionData.ComponentType;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public override void ProcessMissionData(MissionProgressEventData missionProgressEventData)
        {
            COMPONENT_TYPE componentType = missionProgressEventData.componentType;
            int amount = missionProgressEventData.intAmount;

            if (!m_componentType.HasValue || componentType == m_componentType)
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
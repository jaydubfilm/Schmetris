using System.Collections.Generic;

namespace StarSalvager
{
    public class ResourceCollectedMission : Mission
    {
        private BIT_TYPE m_resourceType;
        private int m_amountNeeded;
        private int m_currentAmount;

        public ResourceCollectedMission()
        {
            MissionEventType = MISSION_EVENT_TYPE.RESOURCE_COLLECTED;
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
    }
}
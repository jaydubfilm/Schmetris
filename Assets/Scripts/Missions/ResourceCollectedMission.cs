using System.Collections.Generic;

namespace StarSalvager
{
    [System.Serializable]
    public class ResourceCollectedMission : Mission
    {
        public BIT_TYPE m_resourceType;

        public ResourceCollectedMission(BIT_TYPE resourceType, string missionName, int amountNeeded) : base(missionName, amountNeeded)
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
    }
}
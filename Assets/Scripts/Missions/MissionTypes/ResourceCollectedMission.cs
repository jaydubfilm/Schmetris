using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class ResourceCollectedMission : Mission
    {
        public BIT_TYPE? m_resourceType;
        public bool m_isFromEnemyLoot;

        public ResourceCollectedMission(BIT_TYPE? resourceType, bool isFromEnemyLoot, string missionName, string missionDescription, List<IMissionUnlockCheck> missionUnlockData, float amountNeeded) : base(missionName, missionDescription, amountNeeded, missionUnlockData)
        {
            MissionEventType = MISSION_EVENT_TYPE.RESOURCE_COLLECTED;
            m_resourceType = resourceType;
            m_isFromEnemyLoot = isFromEnemyLoot;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData(BIT_TYPE resourceType, int amount, bool isFromEnemyLoot)
        {
            if (!isFromEnemyLoot && m_isFromEnemyLoot)
            {
                return;
            }
            
            if (!m_resourceType.HasValue || resourceType == m_resourceType)
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

                ResourceType = m_resourceType,
                IsFromEnemyLoot = m_isFromEnemyLoot
            };
        }
    }
}
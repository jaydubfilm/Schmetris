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

        public ResourceCollectedMission(MissionRemoteData missionRemoteData) : base(missionRemoteData)
        {
            MissionEventType = MISSION_EVENT_TYPE.RESOURCE_COLLECTED;
            m_resourceType = missionRemoteData.ResourceValue();
            m_isFromEnemyLoot = missionRemoteData.IsFromEnemyLoot;
        }

        public ResourceCollectedMission(MissionData missionData) : base(missionData)
        {
            MissionEventType = MISSION_EVENT_TYPE.RESOURCE_COLLECTED;
            m_resourceType = missionData.BitType;
            m_isFromEnemyLoot = missionData.BitDroppedFromEnemyLoot;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public override void ProcessMissionData(MissionProgressEventData missionProgressEventData)
        {
            BIT_TYPE bitType = missionProgressEventData.bitType.Value;
            int amount = missionProgressEventData.intAmount;
            bool fromEnemyLoot = missionProgressEventData.bitDroppedFromEnemyLoot;

            if (!fromEnemyLoot && m_isFromEnemyLoot)
            {
                return;
            }

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

                BitType = m_resourceType,
                BitDroppedFromEnemyLoot = m_isFromEnemyLoot
            };
        }
    }
}
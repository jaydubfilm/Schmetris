using StarSalvager.AI;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class CraftPartMission : Mission
    {
        public PART_TYPE m_partType;
        public int m_partLevel;

        public CraftPartMission(MissionRemoteData missionRemoteData) : base(missionRemoteData)
        {
            MissionEventType = MISSION_EVENT_TYPE.CRAFT_PART;
            m_partType = missionRemoteData.PartType;
            m_partLevel = missionRemoteData.PartLevel;
        }

        public CraftPartMission(MissionData missionData) : base(missionData)
        {
            MissionEventType = MISSION_EVENT_TYPE.CRAFT_PART;
            m_partType = missionData.PartType;
            m_partLevel = missionData.Level;
        }

        public override bool MissionComplete()
        {
            return currentAmount >= amountNeeded;
        }

        public override void ProcessMissionData(MissionProgressEventData missionProgressEventData)
        {
            PART_TYPE partType = missionProgressEventData.partType;
            int level = missionProgressEventData.level;

            if (partType == m_partType && level == m_partLevel)
            {
                currentAmount += 1;
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

                PartType = m_partType,
                Level = m_partLevel
            };
        }
    }
}
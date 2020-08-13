using StarSalvager.AI;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class WhiteBumperMission : Mission
    {
        public bool m_throughPart;
        public PART_TYPE m_partType;

        public WhiteBumperMission(bool throughPart, PART_TYPE partType, string missionName, List<IMissionUnlockCheck> missionUnlockData, int amountNeeded) : base(missionName, amountNeeded, missionUnlockData)
        {
            MissionEventType = MISSION_EVENT_TYPE.WHITE_BUMPER;
            m_throughPart = throughPart;
            m_partType = partType;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData(bool throughPart, PART_TYPE partType, int amount)
        {
            if (!m_throughPart || throughPart && m_throughPart)
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
                MissionUnlockChecks = missionUnlockChecks.ExportMissionUnlockParametersDatas(),

                ThroughPart = m_throughPart
            };
        }
    }
}
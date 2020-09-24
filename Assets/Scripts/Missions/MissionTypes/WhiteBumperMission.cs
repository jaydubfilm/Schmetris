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
        public bool m_orphanBit;
        public bool m_hasCombos;
        public PART_TYPE m_partType;

        public WhiteBumperMission(bool throughPart, bool orphanBit, bool hasCombos, PART_TYPE partType, string missionName, string missionDescription, List<IMissionUnlockCheck> missionUnlockData, float amountNeeded) : base(missionName, missionDescription, amountNeeded, missionUnlockData)
        {
            MissionEventType = MISSION_EVENT_TYPE.WHITE_BUMPER;
            m_throughPart = throughPart;
            m_orphanBit = orphanBit;
            m_hasCombos = hasCombos;
            m_partType = partType;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData(bool throughPart, bool orphanBit, bool hasCombos, PART_TYPE partType, int amount)
        {
            if (!throughPart && m_throughPart)
            {
                return;
            }

            if (!orphanBit && m_orphanBit)
            {
                return;
            }

            if (!hasCombos && m_hasCombos)
            {
                return;
            }
            
            m_currentAmount += amount;
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

                ThroughPart = m_throughPart,
                OrphanBit = m_orphanBit,
                HasCombos = m_hasCombos
            };
        }
    }
}
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

        public WhiteBumperMission(MissionRemoteData missionRemoteData) : base(missionRemoteData)
        {
            MissionEventType = MISSION_EVENT_TYPE.WHITE_BUMPER;
            m_throughPart = missionRemoteData.ThroughPart;
            m_orphanBit = missionRemoteData.OrphanBit;
            m_hasCombos = missionRemoteData.HasCombos;
            m_partType = missionRemoteData.PartType;
        }

        public WhiteBumperMission(MissionData missionData) : base(missionData)
        {
            MissionEventType = MISSION_EVENT_TYPE.WHITE_BUMPER;
            m_throughPart = missionData.BumperShiftedThroughPart;
            m_orphanBit = missionData.BumperOrphanedBits;
            m_hasCombos = missionData.BumperCausedCombos;
            m_partType = missionData.PartType;
        }

        public override bool MissionComplete()
        {
            return currentAmount >= amountNeeded;
        }

        public override void ProcessMissionData(MissionProgressEventData missionProgressEventData)
        {
            bool throughPart = missionProgressEventData.bumperShiftedThroughPart;
            bool orphanBit = missionProgressEventData.bumperOrphanedBits;
            bool hasCombos = missionProgressEventData.bumperCausedCombos;
            int amount = missionProgressEventData.intAmount;
            
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

            currentAmount += amount;
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

                BumperShiftedThroughPart = m_throughPart,
                BumperOrphanedBits = m_orphanBit,
                BumperCausedCombos = m_hasCombos
            };
        }
    }
}
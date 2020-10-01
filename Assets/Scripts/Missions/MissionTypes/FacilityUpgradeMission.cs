using StarSalvager.AI;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class FacilityUpgradeMission : Mission
    {
        public FACILITY_TYPE m_facilityType;
        public int m_facilityLevel;

        public FacilityUpgradeMission(MissionRemoteData missionRemoteData) : base(missionRemoteData)
        {
            MissionEventType = MISSION_EVENT_TYPE.FACILITY_UPGRADE;
            m_facilityType = missionRemoteData.FacilityType;
            m_facilityLevel = missionRemoteData.FacilityLevel;
        }

        public FacilityUpgradeMission(MissionData missionData) : base(missionData)
        {
            MissionEventType = MISSION_EVENT_TYPE.FACILITY_UPGRADE;
            m_facilityType = missionData.FacilityType;
            m_facilityLevel = missionData.Level;
        }

        public override bool MissionComplete()
        {
            return currentAmount >= amountNeeded;
        }

        public override void ProcessMissionData(MissionProgressEventData missionProgressEventData)
        {
            FACILITY_TYPE facilityType = missionProgressEventData.facilityType;
            int level = missionProgressEventData.level;
            
            if (facilityType == m_facilityType && level >= m_facilityLevel)
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

                FacilityType = m_facilityType,
                Level = m_facilityLevel
            };
        }
    }
}
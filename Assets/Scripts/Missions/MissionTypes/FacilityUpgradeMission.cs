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

        public FacilityUpgradeMission(FACILITY_TYPE facilityType, int facilitylevel, string missionName, string missionDescription, List<IMissionUnlockCheck> missionUnlockData, float amountNeeded = 1.0f) : base(missionName, missionDescription, amountNeeded, missionUnlockData)
        {
            MissionEventType = MISSION_EVENT_TYPE.FACILITY_UPGRADE;
            m_facilityType = facilityType;
            m_facilityLevel = facilitylevel - 1;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData(FACILITY_TYPE facilityType, int facilityLevel)
        {
            if (facilityType == m_facilityType && facilityLevel >= m_facilityLevel)
            {
                m_currentAmount += 1;
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

                FacilityType = m_facilityType,
                FacilityLevel = m_facilityLevel
            };
        }
    }
}
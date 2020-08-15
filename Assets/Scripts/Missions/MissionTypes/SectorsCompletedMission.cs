using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class SectorsCompletedMission : Mission
    {
        public SectorsCompletedMission(string missionName, List<IMissionUnlockCheck> missionUnlockData, float amountNeeded) : base(missionName, amountNeeded, missionUnlockData)
        {
            MissionEventType = MISSION_EVENT_TYPE.SECTORS_COMPLETED;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData()
        {
            m_currentAmount += 1;
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
            };
        }
    }
}
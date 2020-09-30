using StarSalvager.AI;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class LevelProgressMission : Mission
    {
        public int m_sectorNumber;
        public int m_waveNumber;

        public LevelProgressMission(MissionRemoteData missionRemoteData) : base(missionRemoteData)
        {
            MissionEventType = MISSION_EVENT_TYPE.LEVEL_PROGRESS;
            m_sectorNumber = missionRemoteData.SectorNumber;
            m_waveNumber = missionRemoteData.WaveNumber;
        }

        public LevelProgressMission(MissionData missionData) : base(missionData)
        {
            MissionEventType = MISSION_EVENT_TYPE.LEVEL_PROGRESS;
            m_sectorNumber = missionData.SectorNumber;
            m_waveNumber = missionData.WaveNumber;
        }

        public override bool MissionComplete()
        {
            return currentAmount >= amountNeeded;
        }

        public override void ProcessMissionData(MissionProgressEventData missionProgressEventData)
        {
            int sectorNumber = missionProgressEventData.sectorNumber;
            int waveNumber = missionProgressEventData.waveNumber;
            
            if (sectorNumber == m_sectorNumber && waveNumber == m_waveNumber)
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

                SectorNumber = m_sectorNumber,
                WaveNumber = m_waveNumber
            };
        }
    }
}
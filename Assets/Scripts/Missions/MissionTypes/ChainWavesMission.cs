using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class ChainWavesMission : Mission
    {
        public int m_waveNumber;

        public ChainWavesMission(MissionRemoteData missionRemoteData) : base(missionRemoteData)
        {
            MissionEventType = MISSION_EVENT_TYPE.CHAIN_WAVES;
            m_waveNumber = missionRemoteData.WaveNumber;
        }

        public ChainWavesMission(MissionData missionData) : base(missionData)
        {
            MissionEventType = MISSION_EVENT_TYPE.CHAIN_WAVES;
            m_waveNumber = missionData.WaveNumber;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public override void ProcessMissionData(MissionProgressEventData missionProgressEventData)
        {
            int waveNumber = missionProgressEventData.waveNumber;
            
            if (waveNumber == m_waveNumber)
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

                WaveNumber = m_waveNumber
            };
        }
    }
}
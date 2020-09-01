using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class ChainWavesMission : Mission
    {
        public int m_waveNumber;

        public ChainWavesMission(int waveNumber, string missionName, string missionDescription, List<IMissionUnlockCheck> missionUnlockData, float amountNeeded = 1.0f) : base(missionName, missionDescription, amountNeeded, missionUnlockData)
        {
            MissionEventType = MISSION_EVENT_TYPE.CHAIN_WAVES;
            m_waveNumber = waveNumber;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData(int waveNumber)
        {
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
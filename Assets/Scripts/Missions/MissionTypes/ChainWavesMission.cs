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
            return currentAmount >= amountNeeded;
        }

        public override void ProcessMissionData(MissionProgressEventData missionProgressEventData)
        {
            int wavesInRow = missionProgressEventData.intAmount;
            
            if (wavesInRow == m_waveNumber)
            {
                currentAmount += 1;
            }
        }

        public override string GetMissionProgressString()
        {
            if (MissionComplete())
            {
                return "";
            }

            int curAmount = 0;
            if (LevelManager.Instance != null && LevelManager.Instance.WaveEndSummaryData != null)
            {
                curAmount = LevelManager.Instance.NumWavesInRow;
            }

            if (curAmount == 0 && amountNeeded == 1)
            {
                return "";
            }

            return $" ({ +curAmount}/{ +m_waveNumber})";
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

                WaveNumber = m_waveNumber
            };
        }
    }
}
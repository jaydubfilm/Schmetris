using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Missions
{
    public struct LevelCompleteUnlockCheck : IMissionUnlockCheck
    {
        public bool IsComplete { get; private set; }
        public int m_sectorNumber;
        public int m_waveNumber;

        public LevelCompleteUnlockCheck(int sectorNumber, int waveNumber)
        {
            IsComplete = false;
            m_sectorNumber = sectorNumber;
            m_waveNumber = waveNumber;
        }
        
        public bool CheckUnlockParameters()
        {
            if (IsComplete)
                return true;
            
            if (MissionManager.RecentCompletedSectorName == m_sectorNumber && MissionManager.RecentCompletedWaveName == m_waveNumber)
            {
                IsComplete = true;
                return true;
            }

            return false;
        }

        public MissionUnlockCheckData ToMissionUnlockParameterData()
        {
            return new MissionUnlockCheckData
            {
                ClassType = GetType().Name,
                IsComplete = this.IsComplete,
                SectorNumber = m_sectorNumber,
                WaveNumber = m_waveNumber
            };
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class LevelCompleteMissionUnlockCheck : MissionUnlockCheck
    {
        public int m_sectorNumber;
        public int m_waveNumber;

        public LevelCompleteMissionUnlockCheck(int sectorNumber, int waveNumber) : base()
        {
            m_sectorNumber = sectorNumber;
            m_waveNumber = waveNumber;
        }
        
        public override bool CheckUnlockParameters()
        {
            if (IsComplete)
                return true;
            
            if (MissionManager.recentCompletedSectorName == m_sectorNumber && MissionManager.recentCompletedWaveName == m_waveNumber)
            {
                IsComplete = true;
                return true;
            }

            return false;
        }
    }
}
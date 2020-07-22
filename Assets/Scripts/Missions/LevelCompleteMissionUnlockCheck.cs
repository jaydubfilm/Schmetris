using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class LevelCompleteMissionUnlockCheck : MissionUnlockCheck
    {
        int m_sectorNumber;
        int m_waveNumber;

        public LevelCompleteMissionUnlockCheck(int sectorNumber, int waveNumber) : base()
        {
            m_sectorNumber = sectorNumber;
            m_waveNumber = waveNumber;
        }
        
        public override bool CheckUnlockParameters()
        {
            if (MissionManager.recentCompletedSectorName == m_sectorNumber && MissionManager.recentCompletedWaveName == m_waveNumber)
            {
                return true;
            }

            return false;
        }
    }
}
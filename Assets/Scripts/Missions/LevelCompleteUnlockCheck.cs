using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarSalvager.Missions
{
    public struct LevelCompleteUnlockCheck : IMissionUnlockCheck
    {
        public bool IsComplete { get; private set; }
        public int m_sectorNumber;

        public LevelCompleteUnlockCheck(int sectorNumber)
        {
            IsComplete = false;
            m_sectorNumber = sectorNumber;
        }
        
        public bool CheckUnlockParameters()
        {
            if (IsComplete)
                return true;

            int compareSector = m_sectorNumber;
            if (PlayerDataManager.GetPlayerPreviouslyCompletedNodes().Any(n => PlayerDataManager.GetLevelRingNodeTree().ConvertNodeIndexIntoSectorWave(n).Item1 == compareSector))
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
                SectorNumber = m_sectorNumber
            };
        }
    }
}
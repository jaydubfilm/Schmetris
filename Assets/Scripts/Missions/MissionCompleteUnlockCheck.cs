using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Missions
{
    public struct MissionCompleteUnlockCheck : IMissionUnlockCheck
    {
        public bool IsComplete { get; private set; }
        public string m_missionName;

        public MissionCompleteUnlockCheck(string missionName)
        {
            IsComplete = false;
            m_missionName = missionName;
        }
        
        public bool CheckUnlockParameters()
        {
            if (IsComplete)
                return true;
            
            if (MissionManager.recentCompletedMissionName == m_missionName)
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
                MissionName = m_missionName
            };
        }
    }
}
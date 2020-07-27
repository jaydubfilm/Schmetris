using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class MissionCompleteMissionUnlockCheck : MissionUnlockCheck
    {
        public string m_missionName;

        public MissionCompleteMissionUnlockCheck(string missionName) : base()
        {
            m_missionName = missionName;
        }
        
        public override bool CheckUnlockParameters()
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
    }
}
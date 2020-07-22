using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class MissionCompleteMissionUnlockCheck : MissionUnlockCheck
    {
        string m_missionName;

        public MissionCompleteMissionUnlockCheck(string missionName) : base()
        {
            m_missionName = missionName;
        }
        
        public override bool CheckUnlockParameters()
        {
            if (MissionManager.recentCompletedMissionName == m_missionName)
            {
                return true;
            }

            return false;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    [System.Serializable]
    public abstract class Mission
    {
        public string m_missionName;
        public int m_amountNeeded;
        public int m_currentAmount;
        public MISSION_EVENT_TYPE MissionEventType { get; protected set; }
        public MISSION_STATUS MissionStatus;

        public MISSION_UNLOCK_PARAMETERS MissionUnlockType { get; protected set; }

        public Mission(string missionName, int amountNeeded, MISSION_UNLOCK_PARAMETERS missionUnlockType)
        {
            m_currentAmount = 0;
            m_missionName = missionName;
            m_amountNeeded = amountNeeded;
            MissionUnlockType = missionUnlockType;
        }

        public float GetMissionProgress()
        {
            return (float)m_currentAmount / (float)m_amountNeeded;
        }

        public abstract bool MissionComplete();
    }
}
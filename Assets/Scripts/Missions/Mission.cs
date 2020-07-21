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

        public Mission(string missionName, int amountNeeded)
        {
            m_currentAmount = 0;
            m_missionName = missionName;
            m_amountNeeded = amountNeeded;
        }

        public abstract bool MissionComplete();
    }
}
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

        public MissionUnlockCheck missionUnlockCheck;

        public Mission(string missionName, int amountNeeded, Dictionary<string, object> missionUnlockData)
        {
            m_currentAmount = 0;
            m_missionName = missionName;
            m_amountNeeded = amountNeeded;


            switch (missionUnlockData["MissionUnlockType"])
            {
                case "Level Complete":
                    missionUnlockCheck = new LevelCompleteMissionUnlockCheck((int)missionUnlockData["SectorNumber"], (int)missionUnlockData["WaveNumber"]);
                    break;
                case "Mission Complete":
                    missionUnlockCheck = new MissionCompleteMissionUnlockCheck((string)missionUnlockData["MissionName"]);
                    break;
                default:
                    Debug.Log("Missing mission unlock check!");
                    missionUnlockCheck = new MissionAutoUnlockCheck();
                    break;
            }
        }

        public float GetMissionProgress()
        {
            return (float)m_currentAmount / (float)m_amountNeeded;
        }

        public abstract bool MissionComplete();
    }
}
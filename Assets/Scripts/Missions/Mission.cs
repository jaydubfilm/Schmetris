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

        public List<MissionUnlockCheck> missionUnlockChecks;

        public Mission(string missionName, int amountNeeded, List<Dictionary<string, object>> missionUnlockData)
        {
            m_currentAmount = 0;
            m_missionName = missionName;
            m_amountNeeded = amountNeeded;
            missionUnlockChecks = new List<MissionUnlockCheck>();

            if (missionUnlockData == null)
            {
                Debug.Log(m_missionName);
                return;
            }

            foreach (var data in missionUnlockData)
            {
                MissionUnlockCheck newCheck;
                switch (data["MissionUnlockType"])
                {
                    case "Level Complete":
                        newCheck = new LevelCompleteMissionUnlockCheck((int)data["SectorNumber"], (int)data["WaveNumber"]);
                        break;
                    case "Mission Complete":
                        newCheck = new MissionCompleteMissionUnlockCheck((string)data["MissionName"]);
                        break;
                    default:
                        Debug.Log("Missing mission unlock check!");
                        newCheck = new MissionAutoUnlockCheck();
                        break;
                }
                missionUnlockChecks.Add(newCheck);

            }
        }

        public bool CheckUnlockParameters()
        {
            bool needAll = true;
            foreach (var unlockCheck in missionUnlockChecks)
            {
                if (unlockCheck.CheckUnlockParameters())
                {
                    if (needAll)
                        continue;
                    else
                        return true;
                }
                else
                {
                    if (needAll)
                        return false;
                    else
                        continue;
                }
            }

            if (needAll || missionUnlockChecks.Count == 0)
                return true;
            else
                return false;

        }

        public float GetMissionProgress()
        {
            return (float)m_currentAmount / (float)m_amountNeeded;
        }

        public abstract bool MissionComplete();
    }
}
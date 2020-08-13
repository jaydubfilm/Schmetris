using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public abstract class Mission
    {
        public string m_missionName;
        public int m_amountNeeded;
        public int m_currentAmount;
        public MISSION_EVENT_TYPE MissionEventType { get; protected set; }
        public MISSION_STATUS MissionStatus;

        public List<IMissionUnlockCheck> missionUnlockChecks;

        public Mission(string missionName, int amountNeeded, List<IMissionUnlockCheck> missionUnlockData)
        {
            m_currentAmount = 0;
            m_missionName = missionName;
            m_amountNeeded = amountNeeded;
            missionUnlockChecks = missionUnlockData;
        }

        public bool CheckUnlockParameters()
        {
            bool needAll = true;

            if (missionUnlockChecks == null)
            {
                Debug.Log(m_missionName);
                return true;
            }
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

        public abstract MissionData ToMissionData();
    }
}
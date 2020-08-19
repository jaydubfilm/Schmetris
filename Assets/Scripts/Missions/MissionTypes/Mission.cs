using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public abstract class Mission : IEquatable<Mission>
    {
        public string m_missionName;
        public float m_amountNeeded;
        public float m_currentAmount;
        public MISSION_EVENT_TYPE MissionEventType { get; protected set; }
        public MISSION_STATUS MissionStatus;

        public List<IMissionUnlockCheck> missionUnlockChecks;

        public Mission(string missionName, float amountNeeded, List<IMissionUnlockCheck> missionUnlockData)
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

        public bool Equals(Mission other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return m_missionName == other.m_missionName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Mission)obj);
        }

        public override int GetHashCode()
        {
            return (m_missionName != null ? m_missionName.GetHashCode() : 0);
        }
    }
}
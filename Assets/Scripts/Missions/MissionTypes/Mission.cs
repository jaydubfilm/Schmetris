using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarSalvager.Missions
{
    [Serializable]
    public abstract class Mission : IEquatable<Mission>
    {
        [FormerlySerializedAs("m_missionName")] 
        public string missionName;
        [FormerlySerializedAs("m_missionDescription")] 
        public string missionDescription;
        [FormerlySerializedAs("m_amountNeeded")] 
        public float amountNeeded;
        [FormerlySerializedAs("m_currentAmount")] 
        public float currentAmount;
        public MISSION_EVENT_TYPE MissionEventType { get; protected set; }
        public MISSION_STATUS MissionStatus;

        public List<IMissionUnlockCheck> missionUnlockChecks;

        public Mission(MissionRemoteData missionRemoteData)
        {
            currentAmount = 0;
            missionName = missionRemoteData.MissionName;
            missionDescription = missionRemoteData.MissionDescription;
            amountNeeded = Mathf.Max(1, missionRemoteData.AmountNeeded);
            missionUnlockChecks = missionRemoteData.GetMissionUnlockData();
        }

        public Mission(MissionData missionData)
        {
            currentAmount = 0;
            missionName = missionData.MissionName;
            missionDescription = missionData.MissionDescription;
            amountNeeded = missionData.AmountNeeded;
            missionUnlockChecks = missionData.MissionUnlockChecks.ImportMissionUnlockParametersDatas();
        }

        public abstract void ProcessMissionData(MissionProgressEventData missionProgressEventData);

        public bool CheckUnlockParameters()
        {
            bool needAll = true;

            if (missionUnlockChecks == null)
            {
                Debug.Log(missionName);
                return true;
            }
            //FIXME This has a lot of redundant/unreachable code. This needs to be cleaned up
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
            return currentAmount / amountNeeded;
        }

        public virtual string GetMissionProgressString()
        {
            if (MissionComplete())
            {
                return "";
            }
            
            if (currentAmount == 0 && amountNeeded == 1)
            {
                return "";
            }
            
            return $" ({ + currentAmount}/{ + amountNeeded})";
        }

        public abstract bool MissionComplete();

        public abstract MissionData ToMissionData();

        #region IEquatable

        public bool Equals(Mission other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return missionName == other.missionName;
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
            return (missionName != null ? missionName.GetHashCode() : 0);
        }

        #endregion //IEquatable
    }
}
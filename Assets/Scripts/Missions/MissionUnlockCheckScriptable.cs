using Sirenix.OdinInspector;
using StarSalvager.Factories;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public struct MissionUnlockCheckScriptable
    {
        [SerializeField, FoldoutGroup("$MissionUnlockType"), ValueDropdown("MissionTypes")]
        public string MissionUnlockType;

        [SerializeField, FoldoutGroup("$MissionUnlockType"), ShowIf("MissionUnlockType", "Level Complete")]
        public int SectorUnlockNumber;

        [SerializeField, FoldoutGroup("$MissionUnlockType"), ShowIf("MissionUnlockType", "Level Complete")]
        public int WaveUnlockNumber;

        [SerializeField, FoldoutGroup("$MissionUnlockType"), ValueDropdown("GetMissionNames"), ShowIf("MissionUnlockType", "Mission Complete")]
        public string MissionUnlockName;

        [NonSerialized]
        public bool IsCompleted;

        private static IEnumerable<string> MissionTypes()
        {
            List<string> missionTypes = new List<string>();
            missionTypes.Add("Level Complete");
            missionTypes.Add("Mission Complete");

            return missionTypes;
        }

        private IEnumerable<string> GetMissionNames()
        {
            var missionDatas = UnityEngine.Object.FindObjectOfType<FactoryManager>().MissionRemoteData.m_missionRemoteData;
            List<string> missionNames = new List<string>();

            foreach (var missionData in missionDatas)
            {
                missionNames.Add(missionData.MissionName);
            }

            return missionNames;
        }
    }
}
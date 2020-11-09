﻿using Sirenix.OdinInspector;
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
        [SerializeField, FoldoutGroup("$MissionUnlockType"), ValueDropdown("GetMissionTypes")]
        public string MissionUnlockType;

        [SerializeField, FoldoutGroup("$MissionUnlockType"), ShowIf("MissionUnlockType", "Level Complete")]
        public int SectorUnlockNumber;

        [SerializeField, FoldoutGroup("$MissionUnlockType"), ValueDropdown("GetMissionNames"), ShowIf("MissionUnlockType", "Mission Complete")]
        public string MissionUnlockName;

        [NonSerialized]
        public bool IsCompleted;

        private static IEnumerable<string> GetMissionTypes()
        {
            List<string> missionTypes = new List<string>();
            missionTypes.Add("Level Complete");
            missionTypes.Add("Mission Complete");

            return missionTypes;
        }

        private IEnumerable GetMissionNames()
        {
            ValueDropdownList<string> missionTypes = new ValueDropdownList<string>();
            foreach (MissionRemoteData data in UnityEngine.Object.FindObjectOfType<FactoryManager>().MissionRemoteData.m_missionRemoteData)
            {
                missionTypes.Add(data.MissionName, data.MissionID);
            }
            return missionTypes;
        }
    }
}
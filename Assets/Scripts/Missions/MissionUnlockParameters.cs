using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct MissionUnlockParameters
    {
        [SerializeField, FoldoutGroup("$MissionUnlockType"), ValueDropdown("MissionTypes")]
        public string MissionUnlockType;

        [SerializeField, FoldoutGroup("$MissionUnlockType"), ShowIf("MissionUnlockType", "Level Complete")]
        public int SectorUnlockNumber;

        [SerializeField, FoldoutGroup("$MissionUnlockType"), ShowIf("MissionUnlockType", "Level Complete")]
        public int WaveUnlockNumber;

        [SerializeField, FoldoutGroup("$MissionUnlockType"), ShowIf("MissionUnlockType", "Mission Complete")]
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
    }
}
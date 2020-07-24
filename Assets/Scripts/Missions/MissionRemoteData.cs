using Sirenix.OdinInspector;
using StarSalvager.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct MissionRemoteData
    {
        [SerializeField, FoldoutGroup("$MissionName")]
        public MISSION_EVENT_TYPE MissionType;

        [SerializeField, FoldoutGroup("$MissionName")]
        public string MissionName;

        [SerializeField, FoldoutGroup("$MissionName"), ValueDropdown("MissionTypes")]
        public string MissionUnlockType;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionUnlockType", "Level Complete")]
        public int SectorUnlockNumber;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionUnlockType", "Level Complete")]
        public int WaveUnlockNumber;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionUnlockType", "Mission Complete")]
        public string MissionUnlockName;

        [SerializeField, FoldoutGroup("$MissionName"), HideIf("MissionType", MISSION_EVENT_TYPE.LEVEL_PROGRESS)]
        public int AmountNeeded;

        private bool ShowResourceType => MissionType == MISSION_EVENT_TYPE.RESOURCE_COLLECTED || MissionType == MISSION_EVENT_TYPE.COMBO_BLOCKS;
        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("ShowResourceType")]
        public BIT_TYPE ResourceType;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.ENEMY_KILLED)]
        public string EnemyType;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.LEVEL_PROGRESS)]
        public int SectorNumber;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.LEVEL_PROGRESS)]
        public int WaveNumber;

        public Dictionary<string, object> GetMissionUnlockData()
        {
            Dictionary<string, object> missionUnlockData = new Dictionary<string, object>();

            missionUnlockData.Add("MissionUnlockType", MissionUnlockType);

            switch(MissionUnlockType)
            {
                case "Level Complete":
                    missionUnlockData.Add("SectorNumber", SectorUnlockNumber);
                    missionUnlockData.Add("WaveNumber", WaveUnlockNumber);
                    break;
                case "Mission Complete":
                    missionUnlockData.Add("MissionName", MissionUnlockName);
                    break;
            }

            return missionUnlockData;
        }

        private static IEnumerable<string> MissionTypes()
        {
            List<string> missionTypes = new List<string>();
            missionTypes.Add("Level Complete");
            missionTypes.Add("Mission Complete");

            return missionTypes;
        }
    }
}
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

        [SerializeField, FoldoutGroup("$MissionName")]
        public MISSION_UNLOCK_PARAMETERS MissionUnlockType;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionUnlockType", MISSION_UNLOCK_PARAMETERS.MISSION_COMPLETE)]
        public string MissionUnlockName;

        [SerializeField, FoldoutGroup("$MissionName"), HideIf("MissionType", MISSION_EVENT_TYPE.LEVEL_PROGRESS)]
        public int AmountNeeded;

        private bool ShowResourceType => MissionType == MISSION_EVENT_TYPE.RESOURCE_COLLECTED || MissionType == MISSION_EVENT_TYPE.COMBO_BLOCKS;
        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("ShowResourceType")]
        public BIT_TYPE ResourceType;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.ENEMY_KILLED)]
        public ENEMY_TYPE EnemyType;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.LEVEL_PROGRESS)]
        public int SectorNumber;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.LEVEL_PROGRESS)]
        public int WaveNumber;
    }
}
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Factories.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public struct MissionRemoteData
    {
        [SerializeField, FoldoutGroup("$MissionName")]
        public MISSION_EVENT_TYPE MissionType;

        [SerializeField, FoldoutGroup("$MissionName")]
        public string MissionName;

        [SerializeField, FoldoutGroup("$MissionName")]
        public List<MissionUnlockCheckScriptable> MissionUnlockParameters;

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

        public List<MissionUnlockCheck> GetMissionUnlockData()
        {
            List<MissionUnlockCheck> missionUnlockData = new List<MissionUnlockCheck>();

            foreach (var missionUnlockParameters in MissionUnlockParameters)
            {
                switch (missionUnlockParameters.MissionUnlockType)
                {
                    case "Level Complete":
                        missionUnlockData.Add(new LevelCompleteMissionUnlockCheck(missionUnlockParameters.SectorUnlockNumber, missionUnlockParameters.WaveUnlockNumber));
                        break;
                    case "Mission Complete":
                        missionUnlockData.Add(new MissionCompleteMissionUnlockCheck(missionUnlockParameters.MissionUnlockName));
                        break;
                }
            }

            return missionUnlockData;
        }
    }
}
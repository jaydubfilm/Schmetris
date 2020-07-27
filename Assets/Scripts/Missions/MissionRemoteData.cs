using Sirenix.OdinInspector;
using StarSalvager.AI;
using System;
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
        public List<MissionUnlockParameters> MissionUnlockParameters;

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

        public List<Dictionary<string, object>> GetMissionUnlockData()
        {
            List<Dictionary<string, object>> missionUnlockData = new List<Dictionary<string, object>>();

            foreach (var missionUnlockParameters in MissionUnlockParameters)
            {
                Dictionary<string, object> data = new Dictionary<string, object>();
                data.Add("MissionUnlockType", missionUnlockParameters.MissionUnlockType);

                switch (missionUnlockParameters.MissionUnlockType)
                {
                    case "Level Complete":
                        data.Add("SectorNumber", missionUnlockParameters.SectorUnlockNumber);
                        data.Add("WaveNumber", missionUnlockParameters.WaveUnlockNumber);
                        break;
                    case "Mission Complete":
                        data.Add("MissionName", missionUnlockParameters.MissionUnlockName);
                        break;
                }
                missionUnlockData.Add(data);
            }

            return missionUnlockData;
        }
    }
}
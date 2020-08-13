﻿using Sirenix.OdinInspector;
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

        private bool ShowResources => MissionType == MISSION_EVENT_TYPE.RESOURCE_COLLECTED || MissionType == MISSION_EVENT_TYPE.COMBO_BLOCKS || MissionType == MISSION_EVENT_TYPE.ASTEROID_COLLISION;
        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("ShowResources")]
        public bool AnyResourceType;

        private bool ShowResourceType => !AnyResourceType && ShowResources;
        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("ShowResourceType")]
        public BIT_TYPE ResourceType;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.COMBO_BLOCKS)]
        public int ComboLevel;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.ENEMY_KILLED)]
        public string EnemyType;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.LEVEL_PROGRESS)]
        public int SectorNumber;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.LEVEL_PROGRESS)]
        public int WaveNumber;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.CRAFT_PART)]
        public PART_TYPE PartType;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.CRAFT_PART)]
        public int PartLevel;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.WHITE_BUMPER)]
        public bool ThroughPart;

        public BIT_TYPE? ResourceValue()
        {
            if (AnyResourceType)
                return null;

            return ResourceType;
        }

        public List<IMissionUnlockCheck> GetMissionUnlockData()
        {
            List<IMissionUnlockCheck> missionUnlockData = new List<IMissionUnlockCheck>();

            foreach (var missionUnlockParameters in MissionUnlockParameters)
            {
                switch (missionUnlockParameters.MissionUnlockType)
                {
                    case "Level Complete":
                        missionUnlockData.Add(new LevelCompleteUnlockCheck(missionUnlockParameters.SectorUnlockNumber, missionUnlockParameters.WaveUnlockNumber));
                        break;
                    case "Mission Complete":
                        missionUnlockData.Add(new MissionCompleteUnlockCheck(missionUnlockParameters.MissionUnlockName));
                        break;
                }
            }

            return missionUnlockData;
        }
    }
}
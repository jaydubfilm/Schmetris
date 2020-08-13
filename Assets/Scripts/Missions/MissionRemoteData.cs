using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Factories;
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

        private bool LevelTypeMission => MissionType == MISSION_EVENT_TYPE.LEVEL_PROGRESS || MissionType == MISSION_EVENT_TYPE.CHAIN_WAVES;
        private bool HideAmountNeeded => LevelTypeMission || MissionType == MISSION_EVENT_TYPE.FLIGHT_LENGTH;
        [SerializeField, FoldoutGroup("$MissionName"), HideIf("HideAmountNeeded")]
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
        public bool AnyEnemyType;

        private bool ShowEnemyType => !AnyEnemyType && MissionType == MISSION_EVENT_TYPE.ENEMY_KILLED;
        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("ShowEnemyType"), ValueDropdown("GetEnemyTypes")]
        public string EnemyType;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.LEVEL_PROGRESS)]
        public int SectorNumber;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("LevelTypeMission")]
        public int WaveNumber;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.CRAFT_PART)]
        public PART_TYPE PartType;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.CRAFT_PART)]
        public int PartLevel;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.WHITE_BUMPER)]
        public bool ThroughPart;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.FLIGHT_LENGTH)]
        public float FlightLength;

        public BIT_TYPE? ResourceValue()
        {
            if (AnyResourceType)
                return null;

            return ResourceType;
        }

        public string EnemyValue()
        {
            if (AnyEnemyType)
                return string.Empty;

            return EnemyType;
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

        private IEnumerable GetEnemyTypes()
        {
            ValueDropdownList<string> enemyTypes = new ValueDropdownList<string>();
            foreach (EnemyProfileData data in GameObject.FindObjectOfType<FactoryManager>().EnemyProfile.m_enemyProfileData)
            {
                enemyTypes.Add(data.EnemyType, data.EnemyTypeID);
            }
            return enemyTypes;
        }
    }
}
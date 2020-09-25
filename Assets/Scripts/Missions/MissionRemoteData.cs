using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class MissionRemoteData
    {
        [SerializeField, FoldoutGroup("$MissionName"), DisplayAsString]
        public string MissionID = System.Guid.NewGuid().ToString();

        [SerializeField, FoldoutGroup("$MissionName")]
        public MISSION_EVENT_TYPE MissionType;

        [SerializeField, FoldoutGroup("$MissionName")]
        public string MissionName;

        [SerializeField, FoldoutGroup("$MissionName")]
        public string MissionDescription;

        [SerializeField, FoldoutGroup("$MissionName")]
        public List<MissionUnlockCheckScriptable> MissionUnlockParameters;

        private bool LevelTypeMission => MissionType == MISSION_EVENT_TYPE.LEVEL_PROGRESS || MissionType == MISSION_EVENT_TYPE.CHAIN_WAVES;
        private bool HideAmountNeeded => LevelTypeMission || MissionType == MISSION_EVENT_TYPE.PLAYER_LEVEL || MissionType == MISSION_EVENT_TYPE.FACILITY_UPGRADE || MissionType == MISSION_EVENT_TYPE.FLIGHT_LENGTH || MissionType == MISSION_EVENT_TYPE.CHAIN_WAVES || MissionType == MISSION_EVENT_TYPE.CHAIN_BONUS_SHAPES;
        [SerializeField, FoldoutGroup("$MissionName"), HideIf("HideAmountNeeded")]
        public int AmountNeeded;

        private bool ShowResources => MissionType == MISSION_EVENT_TYPE.RESOURCE_COLLECTED || MissionType == MISSION_EVENT_TYPE.COMBO_BLOCKS || MissionType == MISSION_EVENT_TYPE.ASTEROID_COLLISION;
        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("ShowResources")]
        public bool AnyResourceType;

        private bool ShowResourceType => !AnyResourceType && ShowResources;
        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("ShowResourceType")]
        public BIT_TYPE ResourceType;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.COMPONENT_COLLECTED)]
        public bool AnyComponentType;

        private bool ShowComponentType => !AnyComponentType && MissionType == MISSION_EVENT_TYPE.COMPONENT_COLLECTED;
        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("ShowComponentType")]
        public COMPONENT_TYPE ComponentType;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.COMBO_BLOCKS)]
        public int ComboLevel;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.COMBO_BLOCKS)]
        public bool IsAdvancedCombo;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.ENEMY_KILLED)]
        public bool AnyEnemyType;

        private bool ShowEnemyType => !AnyEnemyType && MissionType == MISSION_EVENT_TYPE.ENEMY_KILLED;
        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("ShowEnemyType"), ValueDropdown("GetEnemyTypes")]
        public string EnemyType;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.LEVEL_PROGRESS)]
        public int SectorNumber;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("LevelTypeMission")]
        public int WaveNumber;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.CHAIN_BONUS_SHAPES)]
        public int BonusShapeNumber;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.CRAFT_PART)]
        public PART_TYPE PartType;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.CRAFT_PART)]
        public int PartLevel;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.FACILITY_UPGRADE)]
        public FACILITY_TYPE FacilityType;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.FACILITY_UPGRADE)]
        public int FacilityLevel;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.PLAYER_LEVEL)]
        public int PlayerLevel;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.WHITE_BUMPER)]
        public bool ThroughPart;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.WHITE_BUMPER)]
        public bool OrphanBit;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.WHITE_BUMPER)]
        public bool HasCombos;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.FLIGHT_LENGTH)]
        public float FlightLength;

        [SerializeField, FoldoutGroup("$MissionName"), ShowIf("MissionType", MISSION_EVENT_TYPE.RESOURCE_COLLECTED)]
        public bool IsFromEnemyLoot;

        [SerializeField, FoldoutGroup("$MissionName")]
        private int maxDrops = 1;

        [SerializeField, FoldoutGroup("$MissionName")]
        private List<RDSLootData> RDSLoot = new List<RDSLootData>();

        public RDSTable rdsTable;

        public BIT_TYPE? ResourceValue()
        {
            if (AnyResourceType)
                return null;

            return ResourceType;
        }

        public COMPONENT_TYPE? ComponentValue()
        {
            if (AnyComponentType)
                return null;

            return ComponentType;
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

        public void ConfigureLootTable()
        {
            rdsTable = new RDSTable();
            rdsTable.rdsCount = maxDrops;

            foreach (var rdsData in RDSLoot)
            {
                if (rdsData.rdsData == RDSLootData.TYPE.Bit)
                {
                    BlockData bitBlockData = new BlockData
                    {
                        ClassType = nameof(Bit),
                        Type = rdsData.type,
                        Level = rdsData.level
                    };
                    rdsTable.AddEntry(new RDSValue<BlockData>(bitBlockData, rdsData.Probability, rdsData.IsUniqueSpawn, rdsData.IsAlwaysSpawn, true));
                }
                else if (rdsData.rdsData == RDSLootData.TYPE.Component)
                {
                    BlockData componentBlockData = new BlockData
                    {
                        ClassType = nameof(Component),
                        Type = rdsData.type,
                    };
                    rdsTable.AddEntry(new RDSValue<BlockData>(componentBlockData, rdsData.Probability, rdsData.IsUniqueSpawn, rdsData.IsAlwaysSpawn, true));
                }
                else if (rdsData.rdsData == RDSLootData.TYPE.Blueprint)
                {
                    Blueprint blueprintData = new Blueprint
                    {
                        name = (PART_TYPE)rdsData.type + " " + rdsData.level,
                        partType = (PART_TYPE)rdsData.type,
                        level = rdsData.level
                    };
                    rdsTable.AddEntry(new RDSValue<Blueprint>(blueprintData, rdsData.Probability, rdsData.IsUniqueSpawn, rdsData.IsAlwaysSpawn, true));
                }
                else if (rdsData.rdsData == RDSLootData.TYPE.FacilityBlueprint)
                {
                    FacilityBlueprint facilityBlueprintData = new FacilityBlueprint
                    {
                        facilityType = (FACILITY_TYPE)rdsData.type,
                        level = rdsData.level
                    };
                    rdsTable.AddEntry(new RDSValue<FacilityBlueprint>(facilityBlueprintData, rdsData.Probability, rdsData.IsUniqueSpawn, rdsData.IsAlwaysSpawn, true));
                }
                else if (rdsData.rdsData == RDSLootData.TYPE.Gears)
                {
                    rdsTable.AddEntry(new RDSValue<Vector2Int>(rdsData.GearDropRange, rdsData.Probability, rdsData.IsUniqueSpawn, rdsData.IsAlwaysSpawn, true));
                }
                else if (rdsData.rdsData == RDSLootData.TYPE.Null)
                {
                    rdsTable.AddEntry(new RDSNullValue(rdsData.Probability));
                }
            }
        }


#if UNITY_EDITOR
        private IEnumerable GetEnemyTypes()
        {
            return Object.FindObjectOfType<FactoryManager>().EnemyProfile.GetEnemyTypes();
        }
#endif
    }
}
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [Serializable]
    public class PlayerLevelRemoteData
    {
        public int GearsToLevelUp;

        [SerializeField]
        private int maxDrops;

        [SerializeField]
        private List<RDSLootData> RDSLevelUpLoot = new List<RDSLootData>();

        public RDSTable rdsTable;

        public void ConfigureLootTable()
        {
            rdsTable = new RDSTable();
            rdsTable.rdsCount = maxDrops;

            foreach (var rdsData in RDSLevelUpLoot)
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
            }
        }
    }
}
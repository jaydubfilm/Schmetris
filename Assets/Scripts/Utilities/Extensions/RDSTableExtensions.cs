using StarSalvager.AI;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class RDSTableExtensions
    {
        public static void SetupRDSTable(this RDSTable rdsTable, int rdsCount, List<RDSLootData> rdsLootDatas)
        {
            rdsTable.rdsCount = rdsCount;

            foreach (var rdsData in rdsLootDatas)
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
                else if (rdsData.rdsData == RDSLootData.TYPE.ResourcesRefined)
                {
                    rdsTable.AddEntry(new RDSValue<(BIT_TYPE, int)>(((BIT_TYPE)rdsData.type, rdsData.amount), rdsData.Probability, rdsData.IsUniqueSpawn, rdsData.IsAlwaysSpawn, true));
                }
                else if (rdsData.rdsData == RDSLootData.TYPE.Asteroid)
                {
                    rdsTable.AddEntry(new RDSValue<ASTEROID_SIZE>((ASTEROID_SIZE)rdsData.type, rdsData.Probability, rdsData.IsUniqueSpawn, rdsData.IsAlwaysSpawn, true));
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
                        partType = (PART_TYPE)rdsData.type
                    };
                    rdsTable.AddEntry(new RDSValue<Blueprint>(blueprintData, rdsData.Probability, rdsData.IsUniqueSpawn, rdsData.IsAlwaysSpawn, true));
                }
                else if (rdsData.rdsData == RDSLootData.TYPE.Gears)
                {
                    if (rdsData.IsGearRange)
                    {
                        rdsTable.AddEntry(new RDSValue<Vector2Int>(rdsData.GearDropRange, rdsData.Probability, rdsData.IsUniqueSpawn, rdsData.IsAlwaysSpawn, true));
                    }
                    else
                    {
                        rdsTable.AddEntry(new RDSValue<Vector2Int>(new Vector2Int(rdsData.GearValue, rdsData.GearValue), rdsData.Probability, rdsData.IsUniqueSpawn, rdsData.IsAlwaysSpawn, true));
                    }
                }
                else if (rdsData.rdsData == RDSLootData.TYPE.Null)
                {
                    rdsTable.AddEntry(new RDSNullValue(rdsData.Probability));
                }
            }
        }
    }
}

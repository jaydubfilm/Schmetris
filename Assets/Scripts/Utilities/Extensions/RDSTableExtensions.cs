using StarSalvager.AI;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class RDSTableExtensions
    {
        public static void SetupRDSTable(this RDSTable rdsTable, Vector2 rdsCount, List<RDSLootData> rdsLootDatas, bool isEvenWeighting)
        {
            rdsCount.y = rdsCount.y + 1;
            rdsTable.rdsCount = rdsCount.ToVector2Int();

            foreach (var rdsData in rdsLootDatas)
            {
                int probability;
                if (isEvenWeighting)
                {
                    probability = 1;
                }
                else
                {
                    probability = rdsData.Weight;
                }

                if (rdsData.lootType == RDSLootData.DROP_TYPE.Bit)
                {
                    BlockData bitBlockData = new BlockData
                    {
                        ClassType = nameof(Bit),
                        Type = rdsData.type,
                        Level = rdsData.lvl
                    };
                    if (rdsData.rng)
                    {
                        rdsTable.AddEntry(new RDSValue<BlockData>(bitBlockData, probability, new Vector2Int(rdsData.min, rdsData.max), false, false, true));
                    }
                    else
                    {
                        rdsTable.AddEntry(new RDSValue<BlockData>(bitBlockData, probability, rdsData.count, false, false, true));
                    }
                }
                else if (rdsData.lootType == RDSLootData.DROP_TYPE.Asteroid)
                {
                    if (rdsData.rng)
                    {
                        rdsTable.AddEntry(new RDSValue<ASTEROID_SIZE>((ASTEROID_SIZE)rdsData.type, probability, new Vector2Int(rdsData.min, rdsData.max), false, false, true));
                    }
                    else
                    {
                        rdsTable.AddEntry(new RDSValue<ASTEROID_SIZE>((ASTEROID_SIZE)rdsData.type, probability, rdsData.count, false, false, true));
                    }
                }
                else if (rdsData.lootType == RDSLootData.DROP_TYPE.Gears)
                {
                    rdsTable.AddEntry(rdsData.rng
                        ? new RDSValue<int>(rdsData.value, probability, new Vector2Int(rdsData.min, rdsData.max), false,
                            false, true)
                        : new RDSValue<int>(rdsData.value, probability, rdsData.count, false, false, true));
                }
                else if (rdsData.lootType == RDSLootData.DROP_TYPE.Null)
                {
                    rdsTable.AddEntry(new RDSNullValue(probability));
                }
            }
        }
    }
}

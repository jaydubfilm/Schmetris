using StarSalvager.AI;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class RDSTableExtensions
    {
        public static void SetupRDSTable(this RDSTable rdsTable, int rdsCount, List<RDSLootData> rdsLootDatas, bool isEvenWeighting)
        {
            rdsTable.rdsCount = rdsCount;

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
                        Level = rdsData.level
                    };
                    rdsTable.AddEntry(new RDSValue<BlockData>(bitBlockData, probability, false, false, true));
                }
                else if (rdsData.lootType == RDSLootData.DROP_TYPE.Asteroid)
                {
                    rdsTable.AddEntry(new RDSValue<ASTEROID_SIZE>((ASTEROID_SIZE)rdsData.type, probability, false, false, true));
                }
                else if (rdsData.lootType == RDSLootData.DROP_TYPE.Gears)
                {
                    rdsTable.AddEntry(new RDSValue<int>(rdsData.value, probability, false, false, true));
                }
                else if (rdsData.lootType == RDSLootData.DROP_TYPE.Null)
                {
                    rdsTable.AddEntry(new RDSNullValue(probability));
                }
            }
        }
    }
}

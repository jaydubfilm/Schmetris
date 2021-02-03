using System.Collections.Generic;
using Newtonsoft.Json;
using StarSalvager;
using StarSalvager.Utilities.Converters;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

public class BlockTesting : MonoBehaviour
{
    private void Start()
    {
        IBlockData[] blocksNull = null;
        var blocks = new IBlockData[]
        {
            //new PartData
            //{
            //    Coordinate = Vector2Int.zero,
            //    Type = (int)PART_TYPE.CORE,
            //    Patches = new PatchData[2]
            //},
            new PartData
            {
                Coordinate = new Vector2Int(-1, 0),
                Type = (int)PART_TYPE.GUN,
                Patches = new []
                {
                    new PatchData
                    {
                        Type = (int)PATCH_TYPE.RANGE,
                        Level = 0
                    },
                    new PatchData
                    {
                        Type = (int)PATCH_TYPE.DAMAGE,
                        Level = 1
                    }
                }
            },
            new PartData
            {
                Coordinate = new Vector2Int(1, 0),
                Type = (int)PART_TYPE.ARMOR,
                Patches = new []
                {
                    new PatchData
                    {
                        Type = (int)PATCH_TYPE.EFFICIENCY,
                        Level = 2
                    }
                }
            },
            new BitData
            {
                Coordinate = new Vector2Int(-1, 1),
                Type = (int)BIT_TYPE.RED,
                Health = 30,
                Level = 0
            },
            new BitData
            {
                Coordinate = new Vector2Int(0, 1),
                Type = (int)BIT_TYPE.BLUE,
                Health = 30,
                Level = 1
            },
            new BitData
            {
                Coordinate = new Vector2Int(1, 1),
                Type = (int)BIT_TYPE.YELLOW,
                Health = 30,
                Level = 0
            }
        };

        var json = JsonConvert.SerializeObject(blocks);
        var json2 = JsonConvert.SerializeObject(new IBlockData[0]);
        var json3 = JsonConvert.SerializeObject(blocksNull);
        var json2_1 = JsonConvert.SerializeObject(new List<IBlockData>());

        var test = JsonConvert.DeserializeObject<IBlockData[]>(json, new IBlockDataArrayConverter());
        var test2 = JsonConvert.DeserializeObject<IBlockData[]>(json2, new IBlockDataArrayConverter());
        var test3 = JsonConvert.DeserializeObject<IBlockData[]>(json3, new IBlockDataArrayConverter());
        
        var test4 = JsonConvert.DeserializeObject<List<IBlockData>>(json, new IBlockDataArrayConverter());
        var test5 = JsonConvert.DeserializeObject<List<IBlockData>>(json2_1, new IBlockDataArrayConverter());
        var test6 = JsonConvert.DeserializeObject<List<IBlockData>>(json3, new IBlockDataArrayConverter());
        
        Debug.Log("Test");
    }
}
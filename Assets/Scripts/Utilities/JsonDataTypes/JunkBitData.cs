using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Utilities.JsonDataTypes
{
    public struct JunkBitData : IBlockData
    {
        public string ClassType => nameof(JunkBit);
        [JsonConverter(typeof(Vector2IntConverter))]
        public Vector2Int Coordinate { get; set; }
        public int Type { get; set; }
    }
}

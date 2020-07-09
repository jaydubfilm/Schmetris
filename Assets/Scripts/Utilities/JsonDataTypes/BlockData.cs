using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace StarSalvager.Utilities.JsonDataTypes
{
    [System.Serializable]
    public struct BlockData
    {
        public string ClassType { get; set; }
        
        [JsonConverter(typeof(Vector2IntConverter))]
        public Vector2Int Coordinate { get; set; }
        public int Type { get; set; }
        
        public int Level { get; set; }
    }
}

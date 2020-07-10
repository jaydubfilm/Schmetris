using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Utilities.JsonDataTypes
{
    [System.Serializable]
    public struct BlockData
    {
        [ShowInInspector]
        public string ClassType { get; set; }
        
        [ShowInInspector, JsonConverter(typeof(Vector2IntConverter))]
        public Vector2Int Coordinate { get; set; }
        [ShowInInspector]
        public int Type { get; set; }
        [ShowInInspector]
        public int Level { get; set; }
    }
}

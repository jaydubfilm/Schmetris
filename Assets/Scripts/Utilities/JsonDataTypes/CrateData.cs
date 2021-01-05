using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Utilities.JsonDataTypes
{
    [Serializable]
    public struct CrateData : IBlockData
    {
        public string ClassType => nameof(Crate);
        [JsonConverter(typeof(Vector2IntConverter))]
        public Vector2Int Coordinate { get; set; }
        public int Type { get; set; }
        public int Level { get; set; }
    }
}

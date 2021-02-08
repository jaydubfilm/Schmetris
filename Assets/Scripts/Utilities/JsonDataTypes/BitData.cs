using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Utilities.JsonDataTypes
{
    [Serializable]
    public struct BitData : IBlockData, IEquatable<BitData>
    {
        [ShowInInspector] public string ClassType => nameof(Bit);

        [ShowInInspector, JsonConverter(typeof(Vector2IntConverter))]
        public Vector2Int Coordinate { get; set; }

        public int Type { get; set; }
        [ShowInInspector] public int Level { get; set; }
        [ShowInInspector] public float Health { get; set; }

        #region IEquatable

        public bool Equals(BitData other)
        {
            return Type == other.Type && Level == other.Level && Coordinate.Equals(other.Coordinate);
        }

        public override bool Equals(object obj)
        {
            return obj is BitData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion //IEquatable
    }
}

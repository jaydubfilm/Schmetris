using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Utilities.JsonDataTypes
{
    [Serializable]
    public struct BlockData : IEquatable<BlockData>
    {
        [ShowInInspector]
        public string ClassType { get; set; }
        
        [ShowInInspector, JsonConverter(typeof(Vector2IntConverter))]
        public Vector2Int Coordinate { get; set; }
        [ShowInInspector]
        public int Type { get; set; }
        [ShowInInspector]
        public int Level { get; set; }
        [ShowInInspector]
        public float Health { get; set; }

        public BlockData(string classType, Vector2Int coordinate, int type, int level, float health)
        {
            ClassType = classType;
            Coordinate = coordinate;
            Type = type;
            Level = level;
            Health = health;
        }

        #region IEquatable

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(BlockData other)
        {
            return ClassType == other.ClassType
                && Type == other.Type
                && Level == other.Level;
        }

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is BlockData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Type * 397) ^ Level;
            }
        }

        #endregion //IEquatable
    }
}

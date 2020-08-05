using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Utilities.JsonDataTypes
{
    [System.Serializable]
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

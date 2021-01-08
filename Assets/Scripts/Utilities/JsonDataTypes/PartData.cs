﻿using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Utilities.JsonDataTypes
{
    [Serializable]
    public struct PartData : IBlockData, IEquatable<PartData>
    {
        [ShowInInspector] public string ClassType => nameof(Part);

        [ShowInInspector, JsonConverter(typeof(Vector2IntConverter))]
        public Vector2Int Coordinate { get; set; }

        public int Type { get; set; }

        public PatchData[] Patches { get; set; }

        #region IEquatable

        public bool Equals(PartData other)
        {
            return Coordinate.Equals(other.Coordinate) && Type == other.Type && Equals(Patches, other.Patches);
        }

        public override bool Equals(object obj)
        {
            return obj is PartData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Coordinate.GetHashCode();
                hashCode = (hashCode * 397) ^ Type;
                hashCode = (hashCode * 397) ^ (Patches != null ? Patches.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion //IEquatable
    }
}
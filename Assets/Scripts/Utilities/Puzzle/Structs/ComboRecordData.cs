using System;
using Newtonsoft.Json;
using StarSalvager.Utilities.JSON.Converters;
using StarSalvager.Utilities.Puzzle.Data;

namespace StarSalvager.Utilities.Puzzle.Structs
{
    [Serializable]
    public struct ComboRecordData : IEquatable<ComboRecordData>
    {
        public int FromLevel;
        public BIT_TYPE BitType;
        public COMBO ComboType;

        #region IEquatable

        public bool Equals(ComboRecordData other)
        {
            return FromLevel == other.FromLevel && BitType == other.BitType && ComboType == other.ComboType;
        }

        public override bool Equals(object obj)
        {
            return obj is ComboRecordData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FromLevel;
                hashCode = (hashCode * 397) ^ (int) BitType;
                hashCode = (hashCode * 397) ^ (int) ComboType;
                return hashCode;
            }
        }

        #endregion //IEquatable
    }
}

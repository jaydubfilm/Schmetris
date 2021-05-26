using System;

namespace StarSalvager.PatchTrees.Data
{
    public struct PatchNodeJson : IEquatable<PatchNodeJson>
    {
        public int Type;
        public int Level;
        public int Tier;
        public int[] PreReqs;

        #region IEquatable

        public bool Equals(PatchNodeJson other)
        {
            return Type == other.Type && Level == other.Level && Tier == other.Tier && Equals(PreReqs, other.PreReqs);
        }

        public override bool Equals(object obj)
        {
            return obj is PatchNodeJson other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Type;
                hashCode = (hashCode * 397) ^ Level;
                hashCode = (hashCode * 397) ^ Tier;
                hashCode = (hashCode * 397) ^ (PreReqs != null ? PreReqs.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion //IEquatable
    }
}

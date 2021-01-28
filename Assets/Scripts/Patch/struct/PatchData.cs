using System;

namespace StarSalvager
{
    [Serializable]
    public struct PatchData : IEquatable<PatchData>
    {
        public int Type;
        public int Level;

        public bool Equals(PatchData other)
        {
            return Type == other.Type && Level == other.Level;
        }

        public override bool Equals(object obj)
        {
            return obj is PatchData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Type * 397) ^ Level;
            }
        }
    }
}

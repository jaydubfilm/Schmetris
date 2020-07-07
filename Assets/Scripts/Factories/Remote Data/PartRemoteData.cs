using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class PartRemoteData: IEquatable<PartRemoteData>
    {
        [FoldoutGroup("$name")]
        public string name;
        
        [FoldoutGroup("$name")]
        public PART_TYPE partType;

        [FoldoutGroup("$name"), ListDrawerSettings(ShowIndexLabels = true)]
        public float[] health;

        [FoldoutGroup("$name")]
        public List<LevelCost> costs;

        [FoldoutGroup("$name"), ListDrawerSettings(ShowIndexLabels = true)]
        public int[] data;

        #region IEquatable
        
        public bool Equals(PartRemoteData other)
        {
            return other != null && partType == other.partType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PartRemoteData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (name != null ? name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) partType;
                hashCode = (hashCode * 397) ^ (health != null ? health.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (costs != null ? costs.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (data != null ? data.GetHashCode() : 0);
                return hashCode;
            }
        }
        #endregion //IEquatable
    }

    [Serializable]
    public class LevelCost
    {
        public List<ResourceAmount> levelCosts;
    }

    [Serializable]
    public struct ResourceAmount: IEquatable<ResourceAmount>
    {
        public BIT_TYPE type;
        public int amount;
        
        #region IEquatable
        
        public bool Equals(ResourceAmount other)
        {
            return type == other.type;
        }

        public override bool Equals(object obj)
        {
            return obj is ResourceAmount other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) type * 397) ^ amount;
            }
        }
        
        #endregion //IEquatable
    }
}



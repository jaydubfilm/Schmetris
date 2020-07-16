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

        [FoldoutGroup("$name"), ListDrawerSettings(ShowIndexLabels = true)]
        public ResourceAmount[] burnRates;

        //This only compares Type and not all individual properties
        #region IEquatable
        
        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(PartRemoteData other)
        {
            return other != null && partType == other.partType;
        }

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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


}



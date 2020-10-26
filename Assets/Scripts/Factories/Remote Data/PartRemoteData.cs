using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class PartRemoteData: RemoteDataBase
    {
        [FoldoutGroup("$name")]
        public string name;
        
        
        [FoldoutGroup("$name")]
        public PART_TYPE partType;

        [FoldoutGroup("$name")]
        public bool lockRotation;

        [FoldoutGroup("$name")]
        public bool canSell = true;

        [TextArea, FoldoutGroup("$name")]
        public string description;
        
        [FoldoutGroup("$name")]
        public int priority;

        [FoldoutGroup("$name")]
        public BIT_TYPE burnType;
        
        [FoldoutGroup("$name")]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = false, ListElementLabelName = "Name")]
        public List<PartLevelData> levels;


        //This only compares Type and not all individual properties
        #region IEquatable
        
        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(RemoteDataBase other)
        {
            if (other is PartRemoteData partRemote)
                return other != null && partType == partRemote.partType;
            else
                return false;
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
            //unchecked
            //{
            //    var hashCode = (name != null ? name.GetHashCode() : 0);
            //    hashCode = (hashCode * 397) ^ (int) partType;
            //    hashCode = (hashCode * 397) ^ (health != null ? health.GetHashCode() : 0);
            //    hashCode = (hashCode * 397) ^ (costs != null ? costs.GetHashCode() : 0);
            //    hashCode = (hashCode * 397) ^ (data != null ? data.GetHashCode() : 0);
            //    return hashCode;
            //}
            return base.GetHashCode();
        }
        #endregion //IEquatable
    }

    [Serializable]
    public class LevelCost
    {
        public List<ResourceAmount> levelCosts;
    }


}



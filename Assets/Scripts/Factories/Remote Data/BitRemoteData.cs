﻿using System;
using Sirenix.OdinInspector;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class BitRemoteData : RemoteDataBase
    {
        [FoldoutGroup("$name")]
        public string name;
        //public string Name => name;
        [FoldoutGroup("$name")]
        public BIT_TYPE bitType;
       //[FoldoutGroup("$name")]
       //public float[] health;
       //public float[] Health => health;
       //[FoldoutGroup("$name")]
       //public int[] resource;

       [FoldoutGroup("$name"), ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "Name")]
       public BitLevelData[] levels;

        #region IEquatable

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(RemoteDataBase other)
        {
            if (other is BitRemoteData bitRemote)
                return other != null && bitType == bitRemote.bitType;
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
            return obj.GetType() == GetType() && Equals((PartRemoteData)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
            /*unchecked
            {
                var hashCode = (name != null ? name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)bitType;
                hashCode = (hashCode * 397) ^ (health != null ? health.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (resource != null ? resource.GetHashCode() : 0);
                return hashCode;
            }*/
        }
        #endregion //IEquatable
    }
}


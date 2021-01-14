using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Parts.Data;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class PartRemoteData : RemoteDataBase
    {
        [Serializable]
        public struct PartGrade
        {
            public List<BIT_TYPE> Types;
            
            //public BIT_TYPE Type;
            public int minBitLevel;
            
            public bool needsBitsToFunction;
            public float[] values;
        }

        [FoldoutGroup("$name")]
        public string name;
        
        [FoldoutGroup("$name")]
        public PART_TYPE partType;

        [FoldoutGroup("$name")]
        public bool lockRotation;

        [TextArea, FoldoutGroup("$name")]
        public string description;

        [FoldoutGroup("$name")]
        public BIT_TYPE burnType;

        [FoldoutGroup("$name")]
        public PartProperties[] dataTest;

        [FoldoutGroup("$name")] 
        public int PatchSockets = 2;

        [FoldoutGroup("$name")]
        public PartGrade partGrade;


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

        public T GetDataValue<T>(PartProperties.KEYS key)
        {
            var keyString = PartProperties.Names[(int)key];
            var dataValue = dataTest.FirstOrDefault(d => d.key.Equals(keyString));

            if (dataValue.Equals(null))
                return default;

            if (!(dataValue.GetValue() is T i))
                return default;

            return i;
        }

        public bool TryGetValue<T>(PartProperties.KEYS key, out T value)
        {
            value = default;

            var keyString = PartProperties.Names[(int)key];
            var dataValue = dataTest.FirstOrDefault(d => d.key.Equals(keyString));

            if (dataValue.Equals(null))
                return false;

            if (!(dataValue.GetValue() is T i))
                return false;

            value = i;

            return true;
        }

        public bool HasPartGrade(in int maxBitLevel, out float value)
        {
            value = 0.0f;
            if (partGrade.minBitLevel > maxBitLevel)
            {
                if (!partGrade.needsBitsToFunction)
                {
                    value = partGrade.values[0];
                    return true;
                }
                return false;
            }

            int index = maxBitLevel - partGrade.minBitLevel;
            if (!partGrade.needsBitsToFunction)
            {
                index++;
            }

            value = partGrade.values[index];
            return true;
        }

        public bool HasPartGrade(in int maxBitLevel)
        {
            if (partGrade.minBitLevel > maxBitLevel)
            {
                if (!partGrade.needsBitsToFunction)
                {
                    return true;
                }
                return false;
            }

            return true;
        }
    }
}



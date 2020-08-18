using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public struct ResourceAmount : IEquatable<ResourceAmount>
    {
        public BIT_TYPE type;
        public int amount;
        public int capacity;

        //This only compares Type and not all individual properties

        #region IEquatable

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ResourceAmount other)
        {
            return type == other.type;
        }

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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
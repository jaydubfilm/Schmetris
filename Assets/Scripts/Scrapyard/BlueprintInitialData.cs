﻿using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    [Serializable]
    public class BlueprintInitialData
    {
        [FoldoutGroup("$Name"), ValueDropdown("GetTypes")]
        public int type;

        //This only compares Type and not all individual properties

        #region IEquatable

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(BlueprintInitialData other)
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
            return obj is BlueprintInitialData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
            //unchecked
            //{
            //    return ((int) type * 397) ^ amount;
            //}
        }

        #endregion //IEquatable

        #region UNITY_EDITOR

#if UNITY_EDITOR

        private string Name => GetName();

        private string GetName()
        {
            var value = $"{(PART_TYPE)type}";

            return value;
        }

        private IEnumerable GetTypes()
        {
            var types = new ValueDropdownList<int>();

            Type valueType = typeof(PART_TYPE);

            foreach (var value in Enum.GetValues(valueType))
            {
                types.Add($"{value}", (int)value);
            }

            return types;
        }

        private void UpdateValue()
        {
            type = 0;
        }

#endif

        #endregion //UNITY_EDITOR
    }
}
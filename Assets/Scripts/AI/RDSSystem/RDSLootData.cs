using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    [Serializable]
    public class RDSLootData : IEquatable<RDSLootData>
    {
        public enum TYPE
        {
            Bit,
            Component,
            Blueprint
        }

        [FoldoutGroup("$Name"), EnumToggleButtons, LabelWidth(125), OnValueChanged("UpdateValue")]
        public TYPE rdsData;

        [FoldoutGroup("$Name"), ValueDropdown("GetTypes")]
        public int type;

        [FoldoutGroup("$Name"), HideIf("rdsData", TYPE.Component)]
        public int level;

        [FoldoutGroup("$Name")] public int probability;

        [FoldoutGroup("$Name")] public bool isUniqueSpawn;

        [FoldoutGroup("$Name")] public bool isAlwaysSpawn;

        //This only compares Type and not all individual properties

        #region IEquatable

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(RDSLootData other)
        {
            return type == other.type && rdsData == other.rdsData;
        }

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is RDSLootData other && Equals(other);
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
            var value = string.Empty;
            switch (rdsData)
            {
                case TYPE.Bit:
                    value = $"{(BIT_TYPE)type}";
                    break;
                case TYPE.Component:
                    value = $"{(COMPONENT_TYPE)type}";
                    break;
                case TYPE.Blueprint:
                    value = $"{(PART_TYPE)type}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return $"{rdsData} - {value} - {probability}";
        }

        private IEnumerable GetTypes()
        {
            var types = new ValueDropdownList<int>();

            Type valueType;
            switch (rdsData)
            {
                case TYPE.Bit:
                    valueType = typeof(BIT_TYPE);
                    break;
                case TYPE.Component:
                    valueType = typeof(COMPONENT_TYPE);
                    break;
                case TYPE.Blueprint:
                    valueType = typeof(PART_TYPE);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

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
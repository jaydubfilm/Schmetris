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
            Blueprint,
            FacilityBlueprint,
            Gears
        }

        [FoldoutGroup("$Name"), EnumToggleButtons, LabelWidth(75), OnValueChanged("UpdateValue")]
        public TYPE rdsData;

        [FoldoutGroup("$Name"), ValueDropdown("GetTypes"), HideIf("rdsData", TYPE.Gears)]
        public int type;

        [FoldoutGroup("$Name"), HideIf("rdsData", TYPE.Component), HideIf("rdsData", TYPE.Gears)]
        public int level;

        [SerializeField, FoldoutGroup("$Name"), HideIf("rdsData", TYPE.Gears)]
        private int probability;
        public int Probability => probability;

        [SerializeField, FoldoutGroup("$Name"), ShowIf("rdsData", TYPE.Gears)]
        private Vector2Int gearDropRange;
        public Vector2Int GearDropRange => gearDropRange;

        [SerializeField, FoldoutGroup("$Name"), HideIf("rdsData", TYPE.Gears)]
        private bool isUniqueSpawn;
        public bool IsUniqueSpawn => isUniqueSpawn || rdsData == TYPE.Gears;

        [SerializeField, FoldoutGroup("$Name"), HideIf("rdsData", TYPE.Gears)]
        private bool isAlwaysSpawn;
        public bool IsAlwaysSpawn => isAlwaysSpawn || rdsData == TYPE.Gears;

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
                case TYPE.FacilityBlueprint:
                    value = $"{(FACILITY_TYPE)type}";
                    break;
                case TYPE.Gears:
                    value = "Gears";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return $"{rdsData} - {value} - {Probability}";
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
                case TYPE.FacilityBlueprint:
                    valueType = typeof(FACILITY_TYPE);
                    break;
                case TYPE.Gears:
                    return null;
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
using Sirenix.OdinInspector;
using StarSalvager.AI;
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
            ResourcesRefined,
            Asteroid,
            Component,
            Blueprint,
            FacilityBlueprint,
            Gears,
            Null
        }

        [FoldoutGroup("$Name"), EnumToggleButtons, LabelWidth(75), OnValueChanged("UpdateValue")]
        public TYPE rdsData;

        [FoldoutGroup("$Name"), ValueDropdown("GetTypes"), HideIf("rdsData", TYPE.Gears), HideIf("rdsData", TYPE.Null)]
        public int type;

        [FoldoutGroup("$Name"), ShowIf("rdsData", TYPE.ResourcesRefined)]
        public int amount;

        [FoldoutGroup("$Name"), HideIf("rdsData", TYPE.ResourcesRefined), HideIf("rdsData", TYPE.Asteroid),
         HideIf("rdsData", TYPE.Component), HideIf("rdsData", TYPE.Gears), HideIf("rdsData", TYPE.Null)]
        public int level;

        [SerializeField, FoldoutGroup("$Name"), HideIf("rdsData", TYPE.Gears)]
        private int probability;

        public int Probability => probability;

        [SerializeField, FoldoutGroup("$Name"), ShowIf("rdsData", TYPE.Gears), HideIf("rdsData", TYPE.Null)]
        private bool isGearRange;

        public bool IsGearRange => isGearRange;

        bool showGearValue => rdsData == TYPE.Gears && isGearRange == false;

        [SerializeField, FoldoutGroup("$Name"), ShowIf("showGearValue"), HideIf("rdsData", TYPE.Null)]
        private int gearValue;

        public int GearValue => gearValue;

        bool showGearRange => rdsData == TYPE.Gears && isGearRange == true;

        [SerializeField, FoldoutGroup("$Name"), ShowIf("showGearRange"), HideIf("rdsData", TYPE.Null)]
        private Vector2Int gearDropRange;

        public Vector2Int GearDropRange => gearDropRange;

        [SerializeField, FoldoutGroup("$Name"), HideIf("rdsData", TYPE.Gears), HideIf("rdsData", TYPE.Null)]
        private bool isUniqueSpawn;

        public bool IsUniqueSpawn => isUniqueSpawn || rdsData == TYPE.Gears;

        [SerializeField, FoldoutGroup("$Name"), HideIf("rdsData", TYPE.Gears), HideIf("rdsData", TYPE.Null)]
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
                case TYPE.ResourcesRefined:
                    value = $"{(BIT_TYPE) type}";
                    break;
                case TYPE.Asteroid:
                    value = $"{(ASTEROID_SIZE) type}";
                    break;
                case TYPE.Component:
                    value = $"{(COMPONENT_TYPE) type}";
                    break;
                case TYPE.Blueprint:
                    value = $"{(PART_TYPE) type}";
                    break;
                case TYPE.FacilityBlueprint:
                    value = $"{(FACILITY_TYPE) type}";
                    break;
                case TYPE.Gears:
                    value = "Gears";
                    break;
                case TYPE.Null:
                    value = "Null";
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
                case TYPE.ResourcesRefined:
                    valueType = typeof(BIT_TYPE);
                    break;
                case TYPE.Asteroid:
                    valueType = typeof(ASTEROID_SIZE);
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
                case TYPE.Null:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach (var value in Enum.GetValues(valueType))
            {
                if (rdsData == TYPE.ResourcesRefined && (BIT_TYPE) value == BIT_TYPE.WHITE)
                    continue;

                if (rdsData == TYPE.ResourcesRefined)
                {
                    switch (value)
                    {
                        case BIT_TYPE.BLUE:
                            types.Add($"Water", (int)value);
                            break;
                        case BIT_TYPE.RED:
                            types.Add($"Fuel", (int)value);
                            break;
                        case BIT_TYPE.YELLOW:
                            types.Add($"Power", (int)value);
                            break;
                        case BIT_TYPE.GREEN:
                            types.Add($"Plasma", (int)value);
                            break;
                        case BIT_TYPE.GREY:
                            types.Add($"Scrap", (int)value);
                            break;
                    }
                }
                else
                {
                    types.Add($"{value}", (int)value);
                }
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
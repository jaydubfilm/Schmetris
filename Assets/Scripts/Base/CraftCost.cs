using System;
using System.Collections;
using Sirenix.OdinInspector;
using StarSalvager.Factories.Data;

//FIXME This should be under a more specific namespace
namespace StarSalvager
{
    [Serializable]
    public struct CraftCost : IEquatable<CraftCost>
    {
        public enum TYPE
        {
            Bit,
            Component,
            Part,
            PatchPoint
        }

        [FoldoutGroup("$Name"), EnumToggleButtons, LabelWidth(125), OnValueChanged("UpdateValue")]
        public TYPE resourceType;

        [FoldoutGroup("$Name"), ValueDropdown("GetTypes")]
        public int type;

        [FoldoutGroup("$Name"), ShowIf("resourceType", TYPE.Part)]
        public int partPrerequisiteLevel;

        [FoldoutGroup("$Name")] public int amount;

        //This only compares Type and not all individual properties

        #region IEquatable

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(CraftCost other)
        {
            return type == other.type && resourceType == other.resourceType;
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
            switch (resourceType)
            {
                case TYPE.Bit:
                    value = $"{(BIT_TYPE) type}";
                    break;
                case TYPE.Component:
                    value = $"{(COMPONENT_TYPE) type}";
                    break;
                case TYPE.Part:
                    value = $"{(PART_TYPE) type}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return $"{resourceType} - {value} - {amount}";
        }

        private IEnumerable GetTypes()
        {
            var types = new ValueDropdownList<int>();

            Type valueType;
            switch (resourceType)
            {
                case TYPE.Bit:
                    valueType = typeof(BIT_TYPE);
                    break;
                case TYPE.Component:
                    valueType = typeof(COMPONENT_TYPE);
                    break;
                case TYPE.Part:
                    valueType = typeof(PART_TYPE);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach (var value in Enum.GetValues(valueType))
            {
                types.Add($"{value}", (int) value);
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
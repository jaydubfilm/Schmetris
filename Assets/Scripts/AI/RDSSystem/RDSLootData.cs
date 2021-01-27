using Sirenix.OdinInspector;
using StarSalvager.AI;
using System;
using System.Collections;
using System.Collections.Generic;
using StarSalvager.Factories;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager
{
    [Serializable]
    public class RDSLootData : IEquatable<RDSLootData>
    {
        public enum DROP_TYPE
        {
            Bit,
            Asteroid,
            Gears,
            Null
        }

        [FoldoutGroup("$Name"), EnumToggleButtons, LabelWidth(75), OnValueChanged("UpdateValue")]
        public DROP_TYPE dropType;

        [FoldoutGroup("$Name"), ValueDropdown("GetTypes"), HideIf("dropType", DROP_TYPE.Gears), HideIf("dropType", DROP_TYPE.Null)]
        public int type;

        [FoldoutGroup("$Name"), ShowIf("dropType", DROP_TYPE.Gears)]
        public int amount;

        [FoldoutGroup("$Name"), ShowIf("dropType", DROP_TYPE.Bit)]
        public int level;

        [SerializeField, FoldoutGroup("$Name")]
        private int probability = 1;

        public int Probability => probability;

        /*[SerializeField, FoldoutGroup("$Name"), HideIf("rdsData", TYPE.Gears), HideIf("rdsData", TYPE.Null)]
        private bool isUniqueSpawn;

        public bool IsUniqueSpawn => isUniqueSpawn || type == TYPE.Gears;

        [SerializeField, FoldoutGroup("$Name"), HideIf("rdsData", TYPE.Gears), HideIf("rdsData", TYPE.Null)]
        private bool isAlwaysSpawn;

        public bool IsAlwaysSpawn => isAlwaysSpawn || type == TYPE.Gears;*/

        //This only compares Type and not all individual properties

        #region IEquatable

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(RDSLootData other)
        {
            return type == other.type && type == other.type;
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
            switch (dropType)
            {
                case DROP_TYPE.Bit:
                case DROP_TYPE.Asteroid:
                    value = $"{(ASTEROID_SIZE) type}";
                    break;
                case DROP_TYPE.Gears:
                    value = "Gears";
                    break;
                case DROP_TYPE.Null:
                    value = "Null";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return $"{type} - {value} - {Probability}";
        }

        private IEnumerable GetTypes()
        {
            var types = new ValueDropdownList<int>();

            Type valueType;
            switch (dropType)
            {
                case DROP_TYPE.Bit:
                    valueType = typeof(BIT_TYPE);
                    break;
                case DROP_TYPE.Asteroid:
                    valueType = typeof(ASTEROID_SIZE);
                    break;
                case DROP_TYPE.Gears:
                case DROP_TYPE.Null:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            
            
            foreach (var value in Enum.GetValues(valueType))
            {
                var name = Convert.ChangeType(value, valueType).ToString();
                
                types.Add(name, (int)value);
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
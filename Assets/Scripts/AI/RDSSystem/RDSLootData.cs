using Sirenix.OdinInspector;
using StarSalvager.AI;
using System;
using System.Collections;
using System.Collections.Generic;
using StarSalvager.Factories;
using UnityEngine;
using UnityEngine.Serialization;
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

        [HideInInspector]
        public bool showProbability;

        [FormerlySerializedAs("dropType")] [/*FoldoutGroup("$Name"), */ValueDropdown("DropTypeValues"), LabelWidth(75), OnValueChanged("UpdateValue"), TableColumnWidth(75)]
        public DROP_TYPE lootType;

        [/*FoldoutGroup("$Name"), */ShowIf("lootType", DROP_TYPE.Bit)]
        public int level;
        
        [/*FoldoutGroup("$Name"), */ValueDropdown("GetTypes"), HideIf("lootType", DROP_TYPE.Gears), HideIf("lootType", DROP_TYPE.Null), TableColumnWidth(75)]
        public int type;

        [FormerlySerializedAs("amount")] [/*FoldoutGroup("$Name"), */ShowIf("lootType", DROP_TYPE.Gears)]
        public int value;

        [FormerlySerializedAs("probability")] 
        [SerializeField, /*FoldoutGroup("$Name"),*/ ShowIf("showProbability"), OnValueChanged("UpdateChance")]
        private int weight = 1;


        [ShowInInspector, DisplayAsString]
        public string Chance => $"{percentChance:P2}";
        [HideInTables, NonSerialized]
        public float percentChance;

        public int Weight => weight;

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

        [NonSerialized]
        private RDSTableData _parentTable;
        public void UpdateChance()
        {
            _parentTable.UpdateChance();
        }

        public void EditorSetParentContainer(in RDSTableData parentTable)
        {
            _parentTable = parentTable;
        }
        
        private IEnumerable DropTypeValues = new ValueDropdownList<DROP_TYPE>
        {
            { "Bit", DROP_TYPE.Bit },
            { "Asteroid", DROP_TYPE.Asteroid },
            { "Gears", DROP_TYPE.Gears },
        };

#if UNITY_EDITOR

        private string Name => GetName();

        private string GetName()
        {
            var value = string.Empty;
            switch (lootType)
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

            return $"{type} - {value} - {Weight}";
        }

        private IEnumerable GetTypes()
        {
            var types = new ValueDropdownList<int>();

            Type valueType;
            switch (lootType)
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
                if(valueType == typeof(BIT_TYPE) && (BIT_TYPE)value == BIT_TYPE.NONE)
                    continue;
                
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
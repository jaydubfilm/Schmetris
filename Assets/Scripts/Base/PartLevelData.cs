using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

//FIXME This should be under a more specific namespace
namespace StarSalvager
{
    [Serializable]
    public struct PartLevelData
    {
        #if UNITY_EDITOR
        [JsonIgnore]
        public string Name =>
            $"Health: {health} - Data: {data} - Burn Rate: {(burnRate == 0f ? "None" : $"{burnRate}/s")} - lvl req: {unlockLevel}";
        #endif

        public int unlockLevel;
        
        public float health;

        public int data;

        [ShowInInspector]
        public DataTest[] dataTest;

        [SuffixLabel("/sec", true)]
        public float burnRate;
        
        public List<CraftCost> cost;

        public float GetDataValue(string key)
        {
            var dataValue = dataTest.FirstOrDefault(d => d.key.Equals(key));
            return dataValue.Equals(null) ? 0f : dataValue.value;
        }
    }

    [Serializable]
    public struct DataTest: IEquatable<DataTest>
    {
        private static readonly string[] TestList = 
        {
            "Magnet",
            "Capacity",
            "Heal",
            "Radius",
            "Absorb",
            "Boost",
            "Time",
            "Damage",
            "Cooldown",
        };
        
        [ValueDropdown(nameof(TestList)), HorizontalGroup("row1"), LabelWidth(30)]
        public string key;
        [HorizontalGroup("row1"), LabelWidth(40)]
        public float value;

        public bool Equals(DataTest other)
        {
            return key == other.key && value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            return obj is DataTest other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((key != null ? key.GetHashCode() : 0) * 397) ^ value.GetHashCode();
            }
        }
    }
}

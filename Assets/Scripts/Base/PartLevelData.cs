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
            $"Health: {health} - Data: {dataTest.Length} - Burn Rate: {(burnRate == 0f ? "None" : $"{burnRate}/s")} - lvl req: {unlockLevel}";
#endif

        public int unlockLevel;

        public float health;

        [SuffixLabel("/sec", true)] public float powerDraw;

        //public int data;

        [ShowInInspector] public DataTest[] dataTest;

        [SuffixLabel("/sec", true)] public float burnRate;

        public List<CraftCost> cost;

        public T GetDataValue<T>(DataTest.TEST_KEYS key)
        {
            var keyString = DataTest.TestList[(int) key];
            var dataValue = dataTest.FirstOrDefault(d => d.key.Equals(keyString));

            if (dataValue.Equals(null))
                return default;

            if (!(dataValue.GetValue() is T i))
                return default;

            return i;
        }

        public bool TryGetValue<T>(DataTest.TEST_KEYS key, out T value)
        {
            value = default;

            var keyString = DataTest.TestList[(int) key];
            var dataValue = dataTest.FirstOrDefault(d => d.key.Equals(keyString));

            if (dataValue.Equals(null))
                return false;

            if (!(dataValue.GetValue() is T i))
                return false;

            value = i;

            return true;
        }
    }

    [Serializable]
    public struct DataTest : IEquatable<DataTest>
    {
        public enum TEST_KEYS
        {
            Magnet,
            Capacity,
            Heal,
            Radius,
            Absorb,
            Boost,
            Time,
            Damage,
            Cooldown,
            Projectile,
            SMRTCapacity,
            Probability,
            PartCapacity,
            Multiplier,
        }

        public static readonly string[] TestList =
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
            "Projectile",
            "SMRTCapacity",
            "Probability",
            "PartCapacity",
            "Multiplier",
        };

        [ValueDropdown(nameof(TestList)), HorizontalGroup("row1", Width = 120), HideLabel]
        public string key;

        //TODO I'll need a way of preventing editors from entering incorrect values here
        [InfoBox("$GetRequiredType", InfoMessageType.Error, "IsWrongType")]
        [SerializeField, HorizontalGroup("row1"), LabelWidth(40)]
        private string value;

        public object GetValue()
        {
            if (!Enum.TryParse(key, out TEST_KEYS _out))
                return default;

            if (string.IsNullOrEmpty(value))
                return default;

            switch (_out)
            {
                case TEST_KEYS.Radius:
                case TEST_KEYS.Capacity:
                case TEST_KEYS.Magnet:
                case TEST_KEYS.SMRTCapacity:
                case TEST_KEYS.PartCapacity:
                    return int.Parse(value);

                case TEST_KEYS.Heal:
                case TEST_KEYS.Absorb:
                case TEST_KEYS.Boost:
                case TEST_KEYS.Time:
                case TEST_KEYS.Damage:
                case TEST_KEYS.Cooldown:
                case TEST_KEYS.Probability:
                case TEST_KEYS.Multiplier:
                    return float.Parse(value);

                case TEST_KEYS.Projectile:
                    return value;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(key), _out, null);
            }
        }

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
            return base.GetHashCode();
            /*unchecked
            {
                return ((key != null ? key.GetHashCode() : 0) * 397) ^ value.GetHashCode();
            }*/
        }

#if UNITY_EDITOR

        private bool IsWrongType()
        {
            try
            {
                GetValue();
            }
            catch (Exception)
            {
                return true;
            }

            return false;
        }

        private string GetRequiredType()
        {
            if (!Enum.TryParse(key, out TEST_KEYS _out))
                return default;

            switch (_out)
            {
                case TEST_KEYS.Radius:
                case TEST_KEYS.Capacity:
                case TEST_KEYS.Magnet:
                case TEST_KEYS.PartCapacity:
                    return $"{_out} should be of type int";

                case TEST_KEYS.Heal:
                case TEST_KEYS.Absorb:
                case TEST_KEYS.Boost:
                case TEST_KEYS.Time:
                case TEST_KEYS.Damage:
                case TEST_KEYS.Cooldown:
                case TEST_KEYS.Probability:
                case TEST_KEYS.Multiplier:
                    return $"{_out} should be of type float";

                case TEST_KEYS.Projectile:
                    return $"{_out} should be of type string";
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(key), _out, null);
            }
        }

#endif

    }
}

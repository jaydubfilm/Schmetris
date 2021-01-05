using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class PartRemoteData: RemoteDataBase
    {
        [FoldoutGroup("$name")]
        public string name;
        
        [FoldoutGroup("$name")]
        public PART_TYPE partType;

        [FoldoutGroup("$name")]
        public bool lockRotation;

        [FoldoutGroup("$name")]
        public bool canSell = true;

        [TextArea, FoldoutGroup("$name")]
        public string description;
        
        [FoldoutGroup("$name")]
        public int priority;

        [FoldoutGroup("$name")]
        public BIT_TYPE burnType;

        [FoldoutGroup("$name")]
        public float powerDraw;

        [FoldoutGroup("$name")]
        public DataTest[] dataTest;

        [FoldoutGroup("$name")] 
        public float burnRate;


        //This only compares Type and not all individual properties
        #region IEquatable

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(RemoteDataBase other)
        {
            if (other is PartRemoteData partRemote)
                return other != null && partType == partRemote.partType;
            else
                return false;
        }

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PartRemoteData) obj);
        }

        public override int GetHashCode()
        {
            //unchecked
            //{
            //    var hashCode = (name != null ? name.GetHashCode() : 0);
            //    hashCode = (hashCode * 397) ^ (int) partType;
            //    hashCode = (hashCode * 397) ^ (health != null ? health.GetHashCode() : 0);
            //    hashCode = (hashCode * 397) ^ (costs != null ? costs.GetHashCode() : 0);
            //    hashCode = (hashCode * 397) ^ (data != null ? data.GetHashCode() : 0);
            //    return hashCode;
            //}
            return base.GetHashCode();
        }
        #endregion //IEquatable

        public T GetDataValue<T>(DataTest.TEST_KEYS key)
        {
            var keyString = DataTest.TestList[(int)key];
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

            var keyString = DataTest.TestList[(int)key];
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



﻿using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Parts.Data
{
     [Serializable]
    public struct PartProperties : IEquatable<PartProperties>
    {
        public enum KEYS
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

        public static readonly string[] Names =
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

        [ValueDropdown(nameof(Names)), HorizontalGroup("row1", Width = 120), HideLabel]
        public string key;

        //TODO I'll need a way of preventing editors from entering incorrect values here
        [ValidateInput("IsCorrectType", "Is wrong type")]
        [SerializeField, HorizontalGroup("row1"), LabelWidth(40)]
        private string value;

        public object GetValue()
        {
            if (!Enum.TryParse(key, out KEYS @out))
                return default;

            if (string.IsNullOrEmpty(value))
                return default;

            switch (@out)
            {
                case KEYS.Radius:
                case KEYS.Capacity:
                case KEYS.Magnet:
                case KEYS.SMRTCapacity:
                case KEYS.PartCapacity:
                    return int.Parse(value);

                case KEYS.Heal:
                case KEYS.Absorb:
                case KEYS.Boost:
                case KEYS.Time:
                case KEYS.Damage:
                case KEYS.Cooldown:
                case KEYS.Probability:
                case KEYS.Multiplier:
                    return float.Parse(value);

                case KEYS.Projectile:
                    return value;

                default:
                    throw new ArgumentOutOfRangeException(nameof(key), @out, null);
            }
        }

        public bool Equals(PartProperties other)
        {
            return key == other.key && value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            return obj is PartProperties other && Equals(other);
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

        private bool IsCorrectType(string value, ref string errorMessage)
        {
            try
            {
                GetValue();
            }
            catch (Exception)
            {
                errorMessage = GetRequiredType();
                return false;
            }

            return true;
        }
        private string GetRequiredType()
        {
            if (!Enum.TryParse(key, out KEYS @out))
                return default;

            switch (@out)
            {
                case KEYS.Radius:
                case KEYS.Capacity:
                case KEYS.Magnet:
                case KEYS.PartCapacity:
                    return $"{@out} should be of type int";

                case KEYS.Heal:
                case KEYS.Absorb:
                case KEYS.Boost:
                case KEYS.Time:
                case KEYS.Damage:
                case KEYS.Cooldown:
                case KEYS.Probability:
                case KEYS.Multiplier:
                    return $"{@out} should be of type float";

                case KEYS.Projectile:
                    return $"{@out} should be of type string";

                default:
                    throw new ArgumentOutOfRangeException(nameof(key), @out, null);
            }
        }

#endif

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Parts.Data;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class PartRemoteData : RemoteDataBase
    {
        [Serializable]
        public class PartGradev2
        {
            [Serializable, Flags]
            public enum BIT_TYPE_FLAG
            {
                NONE = 0,
                BLUE = 1 << 0,
                GREEN = 1 << 1,
                GREY = 1 << 2,
                RED = 1 << 3,
                YELLOW = 1 << 4,
                ALL = BLUE | GREEN | GREY | RED | YELLOW 

            }
            
            public List<BIT_TYPE> Types => GetBitTypes();

            [SerializeField, EnumToggleButtons, LabelWidth(55), LabelText("Types")]
            private BIT_TYPE_FLAG useTypes = BIT_TYPE_FLAG.NONE;
            
            [SerializeField, ToggleLeft]
            private bool requireBit;
            
            [SerializeField, Range(1, 4), EnableIf("requireBit"), LabelText("Min Lvl"), LabelWidth(75)]
            private int minBitLevel;

            [HorizontalGroup("row1")]
            [SerializeField, BoxGroup("row1/Default"), HideLabel, DisableIf("requireBit")]
            private float Default;
            [SerializeField, BoxGroup("row1/Lvl 1"), HideLabel, DisableIf("@requireBit && minBitLevel > 1")]
            private float lvl1;
            [SerializeField, BoxGroup("row1/Lvl 2"), HideLabel, DisableIf("@requireBit && minBitLevel > 2")]
            private float lvl2;
            [SerializeField, BoxGroup("row1/Lvl 3"), HideLabel, DisableIf("@requireBit && minBitLevel > 3")]
            private float lvl3;
            [SerializeField, BoxGroup("row1/Lvl 4"), HideLabel, DisableIf("@requireBit && minBitLevel > 4")]
            private float lvl4;
            //[SerializeField, BoxGroup("row1/Lvl 5"), HideLabel]
            //private float lvl5;

            //====================================================================================================================//

            private List<BIT_TYPE> GetBitTypes()
            {
                var outData = new List<BIT_TYPE>();
                
                foreach (BIT_TYPE_FLAG bitTypeFlag in Enum.GetValues(typeof(BIT_TYPE_FLAG)))
                {
                    if(!useTypes.HasFlag(bitTypeFlag))
                        continue;

                    switch (bitTypeFlag)
                    {
                        case BIT_TYPE_FLAG.BLUE:
                            outData.Add(BIT_TYPE.BLUE);
                            break;
                        case BIT_TYPE_FLAG.GREEN:
                            outData.Add(BIT_TYPE.GREEN);
                            break;
                        case BIT_TYPE_FLAG.GREY:
                            outData.Add(BIT_TYPE.GREY);
                            break;
                        case BIT_TYPE_FLAG.RED:
                            outData.Add(BIT_TYPE.RED);
                            break;
                        case BIT_TYPE_FLAG.YELLOW:
                            outData.Add(BIT_TYPE.YELLOW);
                            break;
                    }
                }

                return outData;
            }
            public bool HasPartGrade(in int maxBitLevel, out float value)
            {
                value = 0.0f;

                var hasGrade = HasPartGrade(maxBitLevel);

                if (!hasGrade)
                    return false;

                switch (maxBitLevel)
                {
                    case 0:
                        value = lvl1;
                        break;
                    case 1:
                        value = lvl2;
                        break;
                    case 2:
                        value = lvl3;
                        break;
                    case 3:
                        value = lvl4;
                        break;
                    //case 4:
                    //    value = lvl5;
                    //    break;
                    default:
                        value = requireBit ? 0.0f : Default;
                        break;
                }

                return true;
            }

            public bool HasPartGrade(in int maxBitLevel)
            {
                if (!requireBit)
                    return true;

                return maxBitLevel >= (minBitLevel - 1);
            }
        }


        
        /*[Serializable]
        public struct PartGrade
        {
            public List<BIT_TYPE> Types;
            
            //public BIT_TYPE Type;
            public int minBitLevel;
            
            public bool needsBitsToFunction;
            public float[] values;
        }*/

        [FoldoutGroup("$name")]
        public string name;
        
        [FoldoutGroup("$name")]
        public PART_TYPE partType;

        [FoldoutGroup("$name")]
        public bool lockRotation;

        [TextArea, FoldoutGroup("$name")]
        public string description;

        [FoldoutGroup("$name")]
        public BIT_TYPE burnType;

        [FoldoutGroup("$name")]
        public PartProperties[] dataTest;

        [FoldoutGroup("$name")] 
        public int PatchSockets = 2;

        /*[BoxGroup("$name/OLD Part Grade"), HideLabel]
        public PartGrade partGrade;*/
        
        [BoxGroup("$name/Part Grade"), HideLabel]
        public PartGradev2 partGrade2;


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

        public T GetDataValue<T>(PartProperties.KEYS key)
        {
            var keyString = PartProperties.Names[(int)key];
            var dataValue = dataTest.FirstOrDefault(d => d.key.Equals(keyString));

            if (dataValue.Equals(null))
                return default;

            if (!(dataValue.GetValue() is T i))
                return default;

            return i;
        }

        public bool TryGetValue<T>(PartProperties.KEYS key, out T value)
        {
            value = default;

            var keyString = PartProperties.Names[(int)key];
            var dataValue = dataTest.FirstOrDefault(d => d.key.Equals(keyString));

            if (dataValue.Equals(null))
                return false;

            if (!(dataValue.GetValue() is T i))
                return false;

            value = i;

            return true;
        }

        public bool HasPartGrade(in int maxBitLevel, out float value)
        {
            return partGrade2.HasPartGrade(maxBitLevel, out value);
            /*value = 0.0f;

            if (partGrade.values.IsNullOrEmpty())
                return false;
            
            if (partGrade.minBitLevel > maxBitLevel)
            {
                if (!partGrade.needsBitsToFunction)
                {
                    value = partGrade.values[0];
                    return true;
                }
                return false;
            }

            int index = maxBitLevel - partGrade.minBitLevel;
            if (!partGrade.needsBitsToFunction)
            {
                index++;
            }

            value = partGrade.values[index];
            return true;*/
        }

        public bool HasPartGrade(in int maxBitLevel)
        {
            return partGrade2.HasPartGrade(maxBitLevel);
            
            /*if (partGrade.values.IsNullOrEmpty())
                return false;
            
            if (partGrade.minBitLevel > maxBitLevel)
            {
                if (!partGrade.needsBitsToFunction)
                {
                    return true;
                }
                return false;
            }

            return true;*/
        }
    }
}



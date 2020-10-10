using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Utilities.JsonDataTypes
{
    [Serializable]
    public struct SectorWaveModifier : IEquatable<SectorWaveModifier>
    {
        public int Sector { get; set; }
        public int Wave { get; set; }
        public float Modifier { get; set; }

        public SectorWaveModifier(int sector, int wave, float modifier)
        {
            Sector = sector;
            Wave = wave;
            Modifier = modifier;
        }

        #region IEquatable

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(SectorWaveModifier other)
        {
            return Sector == other.Sector
                && Wave == other.Wave;
        }

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is SectorWaveModifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Sector * 397) ^ Wave;
            }
        }

        #endregion //IEquatable
    }
}

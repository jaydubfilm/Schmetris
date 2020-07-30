using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities.JsonDataTypes
{
    public class ScrapyardLayout : IEquatable<ScrapyardLayout>
    {
        public string Name;
        public List<BlockData> BlockData;

        public ScrapyardLayout(string name, List<BlockData> blockData)
        {
            Name = name;
            BlockData = blockData;
        }

        #region IEquatable

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ScrapyardLayout other)
        {
            return Name == other.Name;
        }

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is ScrapyardLayout other && Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion //IEquatable
    }
}
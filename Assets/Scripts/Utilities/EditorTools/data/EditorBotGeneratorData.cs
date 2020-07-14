﻿using Sirenix.OdinInspector;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct EditorBotGeneratorData : IEquatable<EditorBotGeneratorData>
    {
        [SerializeField, BoxGroup("Name")]
        private string m_name;

        [SerializeField, BoxGroup("Name")]
        private List<BlockData> m_blockData;

        public EditorBotGeneratorData(string name, List<BlockData> blockData)
        {
            m_name = name;
            m_blockData = blockData;
        }

        public string Name => m_name;

        public List<BlockData> BlockData => m_blockData;

        #region IEquatable

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(EditorBotGeneratorData other)
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
            return obj is EditorBotGeneratorData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion //IEquatable
    }
}
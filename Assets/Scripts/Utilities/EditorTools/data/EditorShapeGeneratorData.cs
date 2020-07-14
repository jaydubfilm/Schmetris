using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct EditorShapeGeneratorData : IEquatable<EditorShapeGeneratorData>
    {
        [SerializeField, BoxGroup("Name")]
        private string m_name;

        [SerializeField, BoxGroup("Name")]
        private List<BlockData> m_blockData;

        public EditorShapeGeneratorData(string name, List<BlockData> blockData)
        {
            m_name = name;
            m_blockData = blockData;
        }

        public string Name => m_name;

        public List<BlockData> BlockData => m_blockData;

        public ASTEROID_SIZE AsteroidSize()
        {
            switch(BlockData.Count)
            {
                case 1:
                    return ASTEROID_SIZE.Bit;
                case 2:
                case 3:
                    return ASTEROID_SIZE.Small;
                case 4:
                case 5:
                    return ASTEROID_SIZE.Medium;
                case 6:
                case 7:
                case 8:
                case 9:
                default:
                    return ASTEROID_SIZE.Large;
            }
        }

        #region IEquatable

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(EditorShapeGeneratorData other)
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
            return obj is EditorShapeGeneratorData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion //IEquatable
    }
}
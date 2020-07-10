using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [System.Serializable]
    public struct EditorShapeGeneratorData
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
    }
}
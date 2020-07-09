using Sirenix.OdinInspector;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [System.Serializable]
    public struct EditorBotGeneratorData
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
    }
}
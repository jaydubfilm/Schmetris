using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Factories.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Space Junk Remote", menuName = "Star Salvager/Scriptable Objects/Space Junk Remote Data")]
    public class SpaceJunkRemoteDataScriptableObject : ScriptableObject
    {
        [SerializeField]
        private int m_maxDrops;

        [SerializeField, LabelText("Loot Drops")]
        private List<RDSLootData> m_rdsLootData;

        public int MaxDrops => m_maxDrops;

        public List<RDSLootData> rdsLootData => m_rdsLootData;
    }
}


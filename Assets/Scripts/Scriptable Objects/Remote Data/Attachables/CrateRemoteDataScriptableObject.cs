using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Factories.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Crate Remote", menuName = "Star Salvager/Scriptable Objects/Crate Remote Data")]
    public class CrateRemoteDataScriptableObject : ScriptableObject
    {
        [SerializeField]
        private int m_maxDrops;

        [SerializeField, LabelText("Loot Drops")]
        private List<RDSLootData> m_rdsLootData;

        public int MaxDrops => m_maxDrops;

        public List<RDSLootData> rdsLootData => m_rdsLootData;

        public List<Sprite> CrateLevelSprites;
    }
}


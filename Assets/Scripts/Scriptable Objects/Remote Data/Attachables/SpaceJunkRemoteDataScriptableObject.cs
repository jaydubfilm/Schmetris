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
        [SerializeField, FoldoutGroup("$Name"), LabelText("Loot Tables")]
        private List<RDSTableData> m_rdsTableData;

        public List<RDSTableData> RDSTableData => m_rdsTableData;
    }
}


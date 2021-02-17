using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Part Remote", menuName = "Star Salvager/Scriptable Objects/Part Remote Data")]
    public class RemotePartProfileScriptableObject : ScriptableObject
    {

        [FoldoutGroup("Part Drops")]
        public List<PART_TYPE> basicWeapons;
        [FoldoutGroup("Part Drops")]
        public List<PART_TYPE> powerWeapons;
        [FormerlySerializedAs("AnyParts")] 
        [FoldoutGroup("Part Drops")]
        public List<PART_TYPE> anyParts;

        //====================================================================================================================//
        
        [Space(10f)]
        public List<PartRemoteData> partRemoteData = new List<PartRemoteData>();

        public PartRemoteData GetRemoteData(PART_TYPE Type)
        {
            return partRemoteData
                .FirstOrDefault(p => p.partType == Type);
        }
        
        public PART_TYPE[] GetTriggerParts()
        {
            return partRemoteData
                .Where(p => p.isManual).Select(p => p.partType).ToArray();
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Part Remote", menuName = "Star Salvager/Scriptable Objects/Part Remote Data")]
    public class RemotePartProfileScriptableObject : ScriptableObject
    {

        [BoxGroup("Starter Parts"), ValueDropdown("GetGreenParts")]
        public PART_TYPE starterGreen;
        [BoxGroup("Starter Parts"), ValueDropdown("GetBlueParts")]
        public PART_TYPE starterBlue;
        [BoxGroup("Starter Parts"), ValueDropdown("GetYellowParts")]
        public PART_TYPE starterYellow;
        
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

        public PartRemoteData GetRemoteData(in int Type)
        {
            return GetRemoteData((PART_TYPE) Type);
        }
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
        
        #if UNITY_EDITOR

        private IEnumerable GetGreenParts()
        {
            return GetPartTypesInCategory(BIT_TYPE.GREEN);
        }
        private IEnumerable GetBlueParts()
        {
            return GetPartTypesInCategory(BIT_TYPE.BLUE);
        }
        private IEnumerable GetYellowParts()
        {
            return GetPartTypesInCategory(BIT_TYPE.YELLOW);
        }
        
        private IEnumerable GetPartTypesInCategory(BIT_TYPE bitType)
        {
            var partRemote = FindObjectOfType<FactoryManager>().PartsRemoteData;
            
            var projectileTypes = new ValueDropdownList<PART_TYPE>
            {
                {$"{PART_TYPE.EMPTY}", PART_TYPE.EMPTY}
            };
            
            foreach (var data in partRemote.partRemoteData.Where(x => x.category == bitType))
            {
                projectileTypes.Add($"{data.partType}", data.partType);
            }
            return projectileTypes;
        }
        
        #endif
    }
}


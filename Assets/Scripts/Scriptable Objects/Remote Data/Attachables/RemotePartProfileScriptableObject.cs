using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Extensions;
using UnityEditor;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Part Remote", menuName = "Star Salvager/Scriptable Objects/Part Remote Data")]
    public class RemotePartProfileScriptableObject : ScriptableObject
    {
        //PartDrop Struct
        //====================================================================================================================//
        
        #region PartDrop Struct

        [Serializable]
        public struct PartDrop
        {
            [TableColumnWidth(30, false)]
            public bool canDrop;
            [ValueDropdown("GetPartTypes"), GUIColor("GetColor")]
            public PART_TYPE PartType;
            

#if UNITY_EDITOR

            [HideInTables]
            public Color color;
            
            private static IEnumerable GetPartTypes() => RemotePartProfileScriptableObject.GetPartTypes();

            
            private Color GetColor() => canDrop ? color : Color.gray;
#endif
        }

        #endregion //PartDrop Struct
        
        //Properties
        //====================================================================================================================//
        
        [BoxGroup("Starter Parts"), ValueDropdown("GetGreenParts")]
        public PART_TYPE starterGreen;
        [BoxGroup("Starter Parts"), ValueDropdown("GetBlueParts")]
        public PART_TYPE starterBlue;
        [BoxGroup("Starter Parts"), ValueDropdown("GetYellowParts")]
        public PART_TYPE starterYellow;
        
        [FoldoutGroup("Part Drops"), ValueDropdown("GetPartTypes")]
        public List<PART_TYPE> basicWeapons;

        public List<PART_TYPE> AnyParts => partDrops.Where(x => x.canDrop).Select(x => x.PartType).ToList();
        
        [SerializeField, FoldoutGroup("Part Drops"),TitleGroup("Part Drops/Part Drops"), ValueDropdown("GetPartTypes"), TableList(AlwaysExpanded = true, HideToolbar = true), OnInspectorInit("CheckPartList")]
        private List<PartDrop> partDrops;

        //====================================================================================================================//
        
        [Space(10f), ListDrawerSettings(ShowPaging  = false, Expanded = true)]
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

        //UNITY EDITOR
        //====================================================================================================================//

        #region Unity Editor

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
            
            var partTypes = new ValueDropdownList<PART_TYPE>
            {
                {"NONE", PART_TYPE.EMPTY}
            };
            
            foreach (var data in partRemote.partRemoteData.Where(x => x.category == bitType && x.isImplemented))
            {
                partTypes.Add($"{data.name}", data.partType);
            }
            return partTypes;
        }
        
        private static IEnumerable GetPartTypes()
        {
            var partRemote = FindObjectOfType<FactoryManager>().PartsRemoteData;

            var partTypes = new ValueDropdownList<PART_TYPE>();
            
            foreach (var data in partRemote.partRemoteData.Where(x => x.isImplemented))
            {
                partTypes.Add(data.partType == PART_TYPE.EMPTY ? "NONE" : $"{data.name}", data.partType);
            }
            return partTypes;
        }
        
        public static IEnumerable GetDropPartTypes()
        {
            var partRemote = FindObjectOfType<FactoryManager>().PartsRemoteData;

            var partTypes = new ValueDropdownList<PART_TYPE>();
            
            foreach (var partType in partRemote.AnyParts)
            {
                var data = partRemote.GetRemoteData(partType);
                partTypes.Add(data.partType == PART_TYPE.EMPTY ? "NONE" : $"{data.name}", data.partType);
            }
            return partTypes;
        }

        private void CheckPartList()
        {
            var partProfile = FindObjectOfType<FactoryManager>().PartsProfileData;
            var bitProfile = FindObjectOfType<FactoryManager>().BitProfileData;

            foreach (PART_TYPE partType in Enum.GetValues(typeof(PART_TYPE)))
            {
                if (partType == PART_TYPE.EMPTY)
                    continue;

                var remoteData = GetRemoteData(partType);

                if (remoteData == null || partProfile.GetProfile(partType) == null || remoteData.isImplemented == false)
                {
                    //TODO Make sure its not currently in the list
                    //TODO Remove if it is
                    continue;
                }

                if (partDrops.Any(x => x.PartType == partType))
                    continue;

                partDrops.Add(new PartDrop
                {
                    canDrop = true,
                    PartType = partType,
                    color = remoteData.category.GetColor()
                });
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();


        }

#endif

        #endregion //Unity Editor

        //====================================================================================================================//
        
    }
}


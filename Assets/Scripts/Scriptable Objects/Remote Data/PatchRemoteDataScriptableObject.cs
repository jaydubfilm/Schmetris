using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using System.Collections;
using UnityEditor;

#endif


namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Patch Remote", menuName = "Star Salvager/Scriptable Objects/Patch Remote Data")]
    public class PatchRemoteDataScriptableObject : ScriptableObject
    {
        //Structs
        //====================================================================================================================//
        
        #region PartDrop Struct

        [Serializable]
        public struct PatchDrop
        {
            [HideInTables, OnValueChanged("UpdateName")]
            public PART_TYPE PartType;

            [TableColumnWidth(40), ShowIf("@this.IsImplemented(PATCH_TYPE.POWER)")]
            public bool POWER;
            [TableColumnWidth(40), ShowIf("@this.IsImplemented(PATCH_TYPE.AOE)")]
            public bool AOE;
            [TableColumnWidth(40), ShowIf("@this.IsImplemented(PATCH_TYPE.FIRE_RATE)")]
            public bool RATE;
            [TableColumnWidth(40), ShowIf("@this.IsImplemented(PATCH_TYPE.EFFICIENCY)")]
            public bool EFF;
            [TableColumnWidth(40), ShowIf("@this.IsImplemented(PATCH_TYPE.RANGE)")]
            public bool RANGE;
            [TableColumnWidth(40), ShowIf("@this.IsImplemented(PATCH_TYPE.BURN)")]
            public bool BURN;
            [TableColumnWidth(40), ShowIf("@this.IsImplemented(PATCH_TYPE.REINFORCED)")]
            public bool RFRCD;

#if UNITY_EDITOR

            [ShowInInspector, GUIColor("color"), TableColumnWidth(100), PropertyOrder(-100)]
            public string part;

            [HideInTables]
            public Color color;
            
            [OnInspectorInit]
            private void UpdateName()
            {
                part = FindObjectOfType<FactoryManager>().PartsRemoteData.GetRemoteData(PartType).name;
            }

            private bool IsImplemented(PATCH_TYPE patchType)
            {
                return FindObjectOfType<FactoryManager>().PatchRemoteData.GetRemoteData(patchType).isImplemented;
            }
#endif
        }

        #endregion //PartDrop Struct
        
        //Properties
        //====================================================================================================================//
        
        [SerializeField, FoldoutGroup("Patch Table"), TableList(AlwaysExpanded = true, DrawScrollView = false, HideToolbar = true), OnInspectorInit("CheckPartList")]
        private List<PatchDrop> allowedParts;
        
        public List<PatchRemoteData> patchRemoteData = new List<PatchRemoteData>();


        //====================================================================================================================//
        
        public PatchRemoteData GetRemoteData(int Type)
        {
            return GetRemoteData((PATCH_TYPE)Type);
        }
        public PatchRemoteData GetRemoteData(PATCH_TYPE Type)
        {
            return patchRemoteData
                .FirstOrDefault(p => p.type == Type);
        }

        //====================================================================================================================//
        
        public PART_TYPE[] GetAllowedParts(in PATCH_TYPE patchType)
        {
            var patch = patchType;
            IEnumerable<PatchDrop> allowed;
            switch (patch)
            {
                case PATCH_TYPE.POWER:
                    allowed = allowedParts.Where(x => x.POWER);
                    break;
                case PATCH_TYPE.AOE:
                    allowed = allowedParts.Where(x => x.AOE);
                    break;
                case PATCH_TYPE.FIRE_RATE:
                    allowed = allowedParts.Where(x => x.RATE);
                    break;
                case PATCH_TYPE.EFFICIENCY:
                    allowed = allowedParts.Where(x => x.EFF);
                    break;
                case PATCH_TYPE.RANGE:
                    allowed = allowedParts.Where(x => x.RANGE);
                    break;
                case PATCH_TYPE.BURN:
                    allowed = allowedParts.Where(x => x.BURN);
                    break;
                case PATCH_TYPE.REINFORCED:
                    allowed = allowedParts.Where(x => x.POWER);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return allowed.Select(x => x.PartType).ToArray();
        }
        
        public bool IsPartPatchAllowed(in PATCH_TYPE patchType, in PART_TYPE partType)
        {
            var patch = patchType;
            var part = partType;
            
            IEnumerable<PatchDrop> allowed;
            switch (patch)
            {
                case PATCH_TYPE.POWER:
                    allowed = allowedParts.Where(x => x.POWER);
                    break;
                case PATCH_TYPE.AOE:
                    allowed = allowedParts.Where(x => x.AOE);
                    break;
                case PATCH_TYPE.FIRE_RATE:
                    allowed = allowedParts.Where(x => x.RATE);
                    break;
                case PATCH_TYPE.EFFICIENCY:
                    allowed = allowedParts.Where(x => x.EFF);
                    break;
                case PATCH_TYPE.RANGE:
                    allowed = allowedParts.Where(x => x.RANGE);
                    break;
                case PATCH_TYPE.BURN:
                    allowed = allowedParts.Where(x => x.BURN);
                    break;
                case PATCH_TYPE.REINFORCED:
                    allowed = allowedParts.Where(x => x.POWER);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return allowed.Any(x => x.PartType == part);
        }

        //====================================================================================================================//

        public IEnumerable<PatchData> GetImplementedPatchData()
        {
            return patchRemoteData
                .Where(x => x.isImplemented)
                .SelectMany(x => x.Levels
                    .Select(y => new PatchData
                    {
                        Type = (int)x.type,
                        Level = y.level - 1 
                    }))
                .ToList();
        }

        //Unity Editor
        //====================================================================================================================//
#if UNITY_EDITOR
        
        private void CheckPartList()
        {
            var partsRemote = Object.FindObjectOfType<FactoryManager>().PartsRemoteData;
            var requiresUpdate = false;
            
            foreach (PART_TYPE partType in Enum.GetValues(typeof(PART_TYPE)))
            {
                if (partType == PART_TYPE.EMPTY)
                    continue;

                var remoteData = partsRemote.GetRemoteData(partType);

                if (remoteData == null || remoteData.isImplemented == false)
                {
                    requiresUpdate = true;
                    allowedParts.RemoveAll(x => x.PartType == partType);                    
                    continue;
                }

                if (allowedParts.Any(x => x.PartType == partType))
                    continue;

                allowedParts.Add(new PatchDrop
                {
                    PartType = partType,
                    color = remoteData.category.GetColor()
                });
                requiresUpdate = true;
            }

            if (!requiresUpdate)
                return;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        public static ValueDropdownList<PATCH_TYPE> GetImplementedPatches(in bool includeNone = true)
        {
            var partRemote = FindObjectOfType<FactoryManager>().PatchRemoteData;

            var patchTypes = new ValueDropdownList<PATCH_TYPE>();
            
            foreach (var data in partRemote.patchRemoteData.Where(x => x.isImplemented))
            {
                if (includeNone == false && data.type == PATCH_TYPE.EMPTY)
                    continue;
                
                patchTypes.Add(data.type == PATCH_TYPE.EMPTY ? "NONE" : $"{data.name}", data.type);
            }
            return patchTypes;
        }
        public static ValueDropdownList<int> GetImplementedPatchesInt(in bool includeNone = true)
        {
            var partRemote = FindObjectOfType<FactoryManager>().PatchRemoteData;

            var patchTypes = new ValueDropdownList<int>();
            
            foreach (var data in partRemote.patchRemoteData.Where(x => x.isImplemented))
            {
                if (includeNone == false && data.type == PATCH_TYPE.EMPTY)
                    continue;
                
                patchTypes.Add(data.type == PATCH_TYPE.EMPTY ? "NONE" : $"{data.name}", (int)data.type);
            }
            return patchTypes;
        }
        
#endif
        
    }
}

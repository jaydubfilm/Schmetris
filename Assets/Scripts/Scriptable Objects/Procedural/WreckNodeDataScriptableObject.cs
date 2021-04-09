using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.ScriptableObjects.Procedural
{
    [CreateAssetMenu(fileName = "Wreck Node Data", menuName = "Star Salvager/Scriptable Objects/Wreck Node Data")]
    public class WreckNodeDataScriptableObject : BaseMapNodeScriptableObject
    {
        //Drop Data Structs
        //====================================================================================================================//

        #region Drop Data Structs

        [Serializable]
        public struct PartDropData
        {
            [TableColumnWidth(50), ValueDropdown("GetDropPartTypes")]
            public PART_TYPE partType;

            [Range(1, 10)] public int weight;



#if UNITY_EDITOR
            [DisplayAsString, TableColumnWidth(35)]
            public string chance;

            [HideInTables] public float chanceValue;

            private IEnumerable GetDropPartTypes() => RemotePartProfileScriptableObject.GetDropPartTypes();

#endif
        }

        [Serializable]
        public struct PatchDropData
        {
            [TableColumnWidth(50), ValueDropdown("GetDropPatchTypes")]
            public PATCH_TYPE patchType;

            [PropertyRange(1, "GetLevelCount"), SuffixLabel("lvl", true), ShowIf("HasPatchSimple")]
            public int level;

            [Range(1, 10)] public int weight;

#if UNITY_EDITOR

            [DisplayAsString, TableColumnWidth(35)]
            public string chance;

            [HideInTables] public float chanceValue;
            private IEnumerable GetDropPatchTypes() => PatchRemoteDataScriptableObject.GetDropPatchTypes();

            private int GetLevelCount()
            {
                if (!HasPatch(out var patchRemoteData))
                    return 0;

                return patchRemoteData.Levels.Count;
            }

            private bool HasPatchSimple()
            {
                return FindObjectOfType<FactoryManager>().PatchRemoteData.GetRemoteData(patchType) != null;
            }
            private bool HasPatch(out PatchRemoteData patchRemoteData)
            {
                patchRemoteData=  FindObjectOfType<FactoryManager>().PatchRemoteData.GetRemoteData(patchType);

                return patchRemoteData != null;
            }
#endif
        }

        #endregion //Drop Data Structs

        //====================================================================================================================//

        [TitleGroup("Parts"), InfoBox("Must have at least 2 parts", InfoMessageType.Error, "InsufficientPartCount")]
        [Space(10f), TableList(AlwaysExpanded = true), OnValueChanged("UpdateChances", true)]
        public List<PartDropData> partsAvailable;

        [TitleGroup("Patches"), MinMaxSlider(0, 10, true), DisableIf("InsufficientPatchCount")]
        public Vector2Int patchDrops;

        [TitleGroup("Patches"), Space(10f), TableList(AlwaysExpanded = true), OnValueChanged("UpdateChances", true)]
        public List<PatchDropData> patchesAvailable;

        //Editor Functionality
        //====================================================================================================================//

        #region Editor

#if UNITY_EDITOR

        private bool InsufficientPartCount() => partsAvailable.Count < 2;
        
        private bool InsufficientPatchCount() =>patchesAvailable.Count < 1;
        

        private void UpdateChances()
        {
            var sum = partsAvailable.Sum(x => x.weight);

            for (int i = 0; i < partsAvailable.Count; i++)
            {
                var dropData = partsAvailable[i];
                dropData.chanceValue = dropData.weight / (float) sum;
                dropData.chance = $"{dropData.chanceValue:P1}";


                partsAvailable[i] = dropData;
            }

            sum = patchesAvailable.Sum(x => x.weight);

            for (int i = 0; i < patchesAvailable.Count; i++)
            {
                var dropData = patchesAvailable[i];
                dropData.chanceValue = dropData.weight / (float) sum;
                dropData.chance = $"{dropData.chanceValue:P1}";


                patchesAvailable[i] = dropData;
            }

        }

#endif

        #endregion //Editor

        //====================================================================================================================//

    }
}

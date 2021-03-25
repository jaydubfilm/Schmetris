using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
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
            [TableColumnWidth(50)]
            public PART_TYPE partType;
            [Range(1, 10)]
            public int weight;

            [DisplayAsString, TableColumnWidth(35)] 
            public string chance;

            [HideInTables]
            public float chanceValue;
        }
        
        [Serializable]
        public struct PatchDropData
        {
            [TableColumnWidth(50)]
            public PATCH_TYPE patchType;
            [Range(1, 10)]
            public int weight;

            [DisplayAsString, TableColumnWidth(35)] 
            public string chance;

            [HideInTables]
            public float chanceValue;
        }

        #endregion //Drop Data Structs

        //====================================================================================================================//
        
        [Space(10f), TableList, OnValueChanged("UpdateChances", true)]
        public List<PartDropData> partsAvailable;
        [Space(10f), TableList, OnValueChanged("UpdateChances", true)]
        public List<PatchDropData> patchesAvailable;

        //Editor Functionality
        //====================================================================================================================//

        #region Editor

#if UNITY_EDITOR

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

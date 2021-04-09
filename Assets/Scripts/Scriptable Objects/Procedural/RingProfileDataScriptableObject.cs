using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.ScriptableObjects.Procedural
{
    [CreateAssetMenu(fileName = "Ring Data", menuName = "Star Salvager/Scriptable Objects/Ring Data")]
    public class RingProfileDataScriptableObject : ScriptableObject
    {

        //Spawn Structs
        //====================================================================================================================//

        #region Spawn Structs

        [Serializable]
        public struct WreckSpawnData
        {
            [OnValueChanged("UpdateName")] public WreckNodeDataScriptableObject asset;

            [Range(1, 10)] public int weight;


#if UNITY_EDITOR

            [DisplayAsString, PropertyOrder(-100), TableColumnWidth(150, false)]
            public string Name;

            [DisplayAsString, TableColumnWidth(75, Resizable = false)]
            public string chance;

            [HideInTables] public float chanceValue;

            /*[Button("Edit"), HideLabel]
            private void Edit()
            {
                //TODO Show the InLine Editor?
            }*/

            private void UpdateName()
            {
                Name = asset == null ? string.Empty : asset.name;
            }

#endif
        }

        [Serializable]
        public struct WaveSpawnData
        {
            [HideInTables, OnInspectorInit("UpdateType")]
            public WaveProfileDataScriptableObject.WAVE_TYPE type;

            [OnValueChanged("UpdateType", true)] public WaveProfileDataScriptableObject asset;

            [Range(1, 10)] public int weight;


#if UNITY_EDITOR

            [DisplayAsString, TableColumnWidth(150, false), PropertyOrder(-100)]
            public string name;

            [DisplayAsString, TableColumnWidth(75, Resizable = false)]
            public string chance;

            [HideInTables] public float chanceValue;

            private void UpdateType()
            {
                if (asset == null)
                {
                    name = string.Empty;
                }
                else
                {
                    type = asset.waveType;
                    name = $"{asset.name} - {type}";
                }
            }

            private bool IsWaveType(WaveProfileDataScriptableObject obj)
            {
                return obj.waveType == type;
            }
#endif
        }

        #endregion //Spawn Structs


        //Properties
        //====================================================================================================================//

        [MinMaxSlider(3, 10, true), OnValueChanged("BalanceNodes")] 
        public Vector2Int sectorsRange = Vector2Int.one;
        [MinMaxSlider(1, 5, true), OnValueChanged("BalanceNodes")] 
        public Vector2Int nodesRange = Vector2Int.one;

        [MinMaxSlider(1, 3, true), ReadOnly] 
        public Vector2Int pathsRange = Vector2Int.one;



        [SerializeField, Required, Range(0, 100), OnValueChanged("BalanceNodes"), TitleGroup("Wreck Spawn"), HorizontalGroup("Wreck Spawn/row1"), SuffixLabel("%", true)]
        private int wreckSpawnBalance;



        [TableList(AlwaysExpanded = true), OnValueChanged("UpdateWreckChances", true)]
        [TitleGroup("Wreck Spawn"), HideLabel]
        public List<WreckSpawnData> wreckSpawnDatas;


        [SerializeField, Required, Range(0, 100), OnValueChanged("BalanceNodes"), TitleGroup("Wave Spawn"), HorizontalGroup("Wave Spawn/row1"), SuffixLabel("%", true)]
        private int waveSpawnBalance;
        


        [TableList(AlwaysExpanded = true), TitleGroup("Wave Spawn"), HideLabel,
         OnValueChanged("UpdateWaveChances", true)]
        public List<WaveSpawnData> waveSpawnDatas;


        //Unity Editor
        //====================================================================================================================//
#if UNITY_EDITOR
        
        [SerializeField, HorizontalGroup("Wreck Spawn/row1", 80), HideLabel, DisplayAsString]
        private string wreckNodeCount;
        
        [SerializeField, HorizontalGroup("Wave Spawn/row1", 80), HideLabel, DisplayAsString]
        private string waveNodeCount;

        private void UpdateWreckChances()
        {
            var sum = wreckSpawnDatas.Sum(x => x.weight);

            for (int i = 0; i < wreckSpawnDatas.Count; i++)
            {
                var dropData = wreckSpawnDatas[i];
                dropData.chanceValue = dropData.weight / (float) sum;
                dropData.chance = $"{dropData.chanceValue:P1}";


                wreckSpawnDatas[i] = dropData;
            }
        }

        private void UpdateWaveChances()
        {
            var sum = waveSpawnDatas.Sum(x => x.weight);

            for (int i = 0; i < waveSpawnDatas.Count; i++)
            {
                var dropData = waveSpawnDatas[i];
                dropData.chanceValue = dropData.weight / (float) sum;
                dropData.chance = $"{dropData.chanceValue:P1}";


                waveSpawnDatas[i] = dropData;
            }
        }

        private void BalanceNodes()
        {
            var sum = waveSpawnBalance + wreckSpawnBalance;

            waveSpawnBalance = Mathf.RoundToInt(((float) waveSpawnBalance / sum) * 100f);
            wreckSpawnBalance = Mathf.RoundToInt(((float) wreckSpawnBalance / sum) * 100f);

            var minNodes = sectorsRange.x * nodesRange.x;
            var maxNodes = sectorsRange.y* nodesRange.y;

            var wavePercent = (waveSpawnBalance / 100f);
            var wreckPercent = (wreckSpawnBalance / 100f);

            wreckNodeCount =
                $"{Mathf.FloorToInt(minNodes * wreckPercent)}-{Mathf.FloorToInt(maxNodes * wreckPercent)} nodes";
            
            waveNodeCount =
                $"{Mathf.FloorToInt(minNodes * wavePercent)}-{Mathf.FloorToInt(maxNodes * wavePercent)} nodes";

        }

#endif
    }
}

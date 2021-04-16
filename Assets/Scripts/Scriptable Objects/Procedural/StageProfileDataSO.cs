using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager;
using StarSalvager.AI;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

namespace StarSalvager.ScriptableObjects.Procedural
{
    [CreateAssetMenu(fileName = "Stage Profile", menuName = "Star Salvager/Procedural/Stage Profile")]
    public class StageProfileDataSO : ScriptableObject, IHasName
    {
        //Enums
        //====================================================================================================================//

        public enum TYPE
        {
            ASTEROID,
            BUMPER,
            CLOUD
        }

        //Structs
        //====================================================================================================================//

        #region Structs

        [Serializable]
        public class StageData
        {
            [TableColumnWidth(150)]
            [InfoBox("Clouds are not yet implemented", InfoMessageType.Warning, VisibleIf = "@type == TYPE.CLOUD")]
            [HorizontalGroup("Type"), HideLabel]
            public TYPE type;

            [HorizontalGroup("Type"), ShowIf("type", TYPE.ASTEROID), LabelWidth(30)]
            public ASTEROID_SIZE size;

            [HorizontalGroup("Spawns Per Min"), HideLabel, TableColumnWidth(200)]
            public RangeFixed spawnsPerMin = new RangeFixed(0, 500);

#if UNITY_EDITOR
            private bool HideCheck() => type == TYPE.ASTEROID;

#endif
        }

        [Serializable]
        public class BitSpawnData : WeightedChanceBase
        {
            [HideInTables] public BIT_TYPE bitType;

#if UNITY_EDITOR
            [GUIColor("GetColor"), PropertyOrder(-1000), TableColumnWidth(120, false)]
            public string BitType;

            private Color GetColor() => bitType.GetColor();

            [OnInspectorInit]
            private void SetName() => BitType = $"{bitType}";
#endif
        }

        #endregion //Structs

        //Properties
        //====================================================================================================================//
        public string Name => name;
        
        public string name;

        [BoxGroup("Bits Per Min"), HideLabel] public RangeFixed bitSpawns = new RangeFixed(0, 500);

        [Tooltip("Dynamic spawning takes into consideration the players current state to decide what to spawn")]
        public bool useDynamicBitSpawning = true;

        #region Fixed Bit Spawing

        [HideIf("useDynamicBitSpawning"), TableList(AlwaysExpanded = true),
         OnValueChanged("UpdateBitSpawnChances", true), HideLabel]
        public List<BitSpawnData> BitSpawnDatas = new List<BitSpawnData>()
        {
            new BitSpawnData {bitType = BIT_TYPE.RED},
            new BitSpawnData {bitType = BIT_TYPE.BLUE},
            new BitSpawnData {bitType = BIT_TYPE.GREEN},
            new BitSpawnData {bitType = BIT_TYPE.YELLOW},
            new BitSpawnData {bitType = BIT_TYPE.GREY},
        };

        #endregion //Fixed Bit Spawing

        [TableList(AlwaysExpanded = true)] public List<StageData> stageData;

        //Unity Editor
        //====================================================================================================================//

#if UNITY_EDITOR

        [OnInspectorInit]
        private void UpdateBitSpawnChances()
        {
            var sum = BitSpawnDatas.Sum(x => x.weight);

            for (int i = 0; i < BitSpawnDatas.Count; i++)
            {
                var dropData = BitSpawnDatas[i];
                dropData.chanceValue = dropData.weight / (float) sum;
                dropData.chance = $"{dropData.chanceValue:P1}";

                BitSpawnDatas[i] = dropData;
            }
        }

#endif

        //====================================================================================================================//

    }
}
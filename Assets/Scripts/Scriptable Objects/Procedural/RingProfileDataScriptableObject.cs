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

        public enum TYPE
        {
            WEIGHTED,
            RANGE
        }


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

        [Serializable]
        public struct WaveSpawnDataALT
        {
            [HideInTables] public WaveProfileDataScriptableObject.WAVE_TYPE type;

            [AssetList(CustomFilterMethod = "IsWaveType"), OnValueChanged("UpdateName")]
            public WaveProfileDataScriptableObject asset;

            [Range(1, 10), HideIf("@asset == null")]
            public int weight;


#if UNITY_EDITOR

            [DisplayAsString, TableColumnWidth(150, false), PropertyOrder(-100)]
            public string name;

            [DisplayAsString, TableColumnWidth(75, Resizable = false), HideIf("@asset == null")]
            public string chance;

            [HideInTables] public float chanceValue;

            private bool IsWaveType(WaveProfileDataScriptableObject obj)
            {
                return obj.waveType == type;
            }

            [OnInspectorInit]
            private void UpdateName()
            {
                name = asset == null ? string.Empty : $"{asset.name}";
                weight = asset == null ? 0 : weight;
            }
#endif
        }

        #endregion //Spawn Structs

        //Properties
        //====================================================================================================================//

        [EnumToggleButtons, PropertyOrder(-1000)]
        public TYPE type;

        #region Weighted

        [MinMaxSlider(3, 10, true), OnValueChanged("BalanceNodes"), ShowIf("@type == TYPE.WEIGHTED")]
        public Vector2Int sectorsRange = Vector2Int.one;

        [MinMaxSlider(1, 5, true), OnValueChanged("BalanceNodes"), ShowIf("@type == TYPE.WEIGHTED")]
        public Vector2Int nodesRange = Vector2Int.one;

        [MinMaxSlider(1, 3, true), ReadOnly, ShowIf("@type == TYPE.WEIGHTED")]
        public Vector2Int pathsRange = Vector2Int.one;

        [SerializeField, Required, Range(0, 100), OnValueChanged("BalanceNodes"), TitleGroup("Wreck Spawn"),
         HorizontalGroup("Wreck Spawn/row1"), SuffixLabel("%", true), ShowIf("@type == TYPE.WEIGHTED")]
        private int wreckSpawnBalance;

        [TableList(AlwaysExpanded = true), OnValueChanged("UpdateWreckChances", true)]
        [TitleGroup("Wreck Spawn"), HideLabel, ShowIf("@type == TYPE.WEIGHTED")]
        public List<WreckSpawnData> wreckSpawnDatas;

        [SerializeField, Required, Range(0, 100), OnValueChanged("BalanceNodes"), TitleGroup("Wave Spawn"),
         HorizontalGroup("Wave Spawn/row1"), SuffixLabel("%", true), ShowIf("@type == TYPE.WEIGHTED")]
        private int waveSpawnBalance;

        [TableList(AlwaysExpanded = true), TitleGroup("Wave Spawn"), HideLabel,
         OnValueChanged("UpdateWaveChances", true), ShowIf("@type == TYPE.WEIGHTED")]
        public List<WaveSpawnData> waveSpawnDatas;

        #endregion //Weighted

        //====================================================================================================================//

        #region Range

        [MinMaxSlider(1, 5, true), ShowIf("@type == TYPE.RANGE"), OnValueChanged("DetermineExpectedSectors"),
         PropertyOrder(-100)]
        public Vector2Int nodesPerSector;

        [ShowIf("@type == TYPE.RANGE"), DisplayAsString, PropertyOrder(-10)]
        public string expectedSectors;


        [TableList(AlwaysExpanded = true), OnValueChanged("UpdateRangeChances", true)]
        [TitleGroup("Wreck Spawn"), HideLabel, ShowIf("@type == TYPE.RANGE"), MinMaxSlider(0, 10, true),
         OnValueChanged("DetermineExpectedSectors")]
        public Vector2Int wreckSpawns;

        [TableList(AlwaysExpanded = true), OnValueChanged("UpdateRangeChances", true)]
        [TitleGroup("Wreck Spawn"), HideLabel, ShowIf("@type == TYPE.RANGE")]
        public List<WreckSpawnData> wreckSpawnDataRANGE;

        [TitleGroup("Wave Spawns"), SerializeField, FoldoutGroup("Wave Spawns/Survival Waves"),
         EnableIf("@HasWaveType(WaveProfileDataScriptableObject.WAVE_TYPE.SURVIVAL)"), MinMaxSlider(0, 10, true)
         , ShowIf("@type == TYPE.RANGE"), OnValueChanged("DetermineExpectedSectors")]
        private Vector2Int survivalSpawns;

        [TableList, FoldoutGroup("Wave Spawns/Survival Waves"), ShowIf("@type == TYPE.RANGE"),
         ListDrawerSettings(CustomAddFunction = "AddSurvivalWave"), OnValueChanged("UpdateRangeChances", true)]
        public List<WaveSpawnDataALT> survivalWaveSpawnData;

        [SerializeField, FoldoutGroup("Wave Spawns/Defence Waves"),
         EnableIf("@HasWaveType(WaveProfileDataScriptableObject.WAVE_TYPE.DEFENCE)"), MinMaxSlider(0, 10, true),
         ShowIf("@type == TYPE.RANGE"), OnValueChanged("DetermineExpectedSectors")]
        private Vector2Int defenceSpawns;

        [TableList, FoldoutGroup("Wave Spawns/Defence Waves"), ShowIf("@type == TYPE.RANGE"),
         ListDrawerSettings(CustomAddFunction = "AddDefenceWave"), OnValueChanged("UpdateRangeChances", true)]
        public List<WaveSpawnDataALT> defenceWaveSpawnData;

        [SerializeField, FoldoutGroup("Wave Spawns/Bonus Waves"),
         EnableIf("@HasWaveType(WaveProfileDataScriptableObject.WAVE_TYPE.BONUS)"), MinMaxSlider(0, 10, true),
         ShowIf("@type == TYPE.RANGE"), OnValueChanged("DetermineExpectedSectors")]
        private Vector2Int BonusSpawns;

        [TableList, FoldoutGroup("Wave Spawns/Bonus Waves"), ShowIf("@type == TYPE.RANGE"),
         ListDrawerSettings(CustomAddFunction = "AddBonusWave"), OnValueChanged("UpdateRangeChances", true)]
        public List<WaveSpawnDataALT> bonusWaveSpawnData;

        [SerializeField, FoldoutGroup("Wave Spawns/Boss Waves"),
         EnableIf("@HasWaveType(WaveProfileDataScriptableObject.WAVE_TYPE.MINI_BOSS)"), MinMaxSlider(0, 10, true),
         ShowIf("@type == TYPE.RANGE"), OnValueChanged("DetermineExpectedSectors")]
        private Vector2Int bossSpawns;

        [TableList, FoldoutGroup("Wave Spawns/Boss Waves"), ShowIf("@type == TYPE.RANGE"),
         ListDrawerSettings(CustomAddFunction = "AddBossWave"), OnValueChanged("UpdateRangeChances", true)]
        public List<WaveSpawnDataALT> bossWaveSpawnData;

        #endregion //Range

        //Unity Editor
        //====================================================================================================================//
#if UNITY_EDITOR

        #region Weighted

        [SerializeField, HorizontalGroup("Wreck Spawn/row1", 80), HideLabel, DisplayAsString,
         ShowIf("@type == TYPE.WEIGHTED")]
        private string wreckNodeCount;

        [SerializeField, HorizontalGroup("Wave Spawn/row1", 80), HideLabel, DisplayAsString,
         ShowIf("@type == TYPE.WEIGHTED")]
        private string waveNodeCount;

        private bool HasWaveType(WaveProfileDataScriptableObject.WAVE_TYPE waveType) =>
            waveSpawnDatas.Any(x => x.type == waveType);

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
            var maxNodes = sectorsRange.y * nodesRange.y;

            var wavePercent = (waveSpawnBalance / 100f);
            var wreckPercent = (wreckSpawnBalance / 100f);

            wreckNodeCount =
                $"{Mathf.FloorToInt(minNodes * wreckPercent)}-{Mathf.FloorToInt(maxNodes * wreckPercent)} nodes";

            waveNodeCount =
                $"{Mathf.FloorToInt(minNodes * wavePercent)}-{Mathf.FloorToInt(maxNodes * wavePercent)} nodes";

        }

        #endregion //Weighted

        //====================================================================================================================//

        #region Range

        private WaveSpawnDataALT AddSurvivalWave() => new WaveSpawnDataALT
            {type = WaveProfileDataScriptableObject.WAVE_TYPE.SURVIVAL};

        private WaveSpawnDataALT AddDefenceWave() => new WaveSpawnDataALT
            {type = WaveProfileDataScriptableObject.WAVE_TYPE.DEFENCE};

        private WaveSpawnDataALT AddBonusWave() => new WaveSpawnDataALT
            {type = WaveProfileDataScriptableObject.WAVE_TYPE.BONUS};

        private WaveSpawnDataALT AddBossWave() => new WaveSpawnDataALT
            {type = WaveProfileDataScriptableObject.WAVE_TYPE.MINI_BOSS};

        [OnInspectorInit]
        private void UpdateRangeChances()
        {
            var lists = new[]
            {
                survivalWaveSpawnData,
                defenceWaveSpawnData,
                bonusWaveSpawnData,
                bossWaveSpawnData,
            };

            int sum;
            foreach (var spawnDataAltList in lists)
            {
                sum = spawnDataAltList.Sum(x => x.weight);

                for (int i = 0; i < spawnDataAltList.Count; i++)
                {
                    var dropData = spawnDataAltList[i];
                    dropData.chanceValue = dropData.weight / (float) sum;
                    dropData.chance = $"{dropData.chanceValue:P1}";


                    spawnDataAltList[i] = dropData;
                }
            }

            sum = wreckSpawnDataRANGE.Sum(x => x.weight);

            for (int i = 0; i < wreckSpawnDataRANGE.Count; i++)
            {
                var dropData = wreckSpawnDataRANGE[i];
                dropData.chanceValue = dropData.weight / (float) sum;
                dropData.chance = $"{dropData.chanceValue:P1}";


                wreckSpawnDataRANGE[i] = dropData;
            }
        }

        [OnInspectorInit]
        private void DetermineExpectedSectors()
        {
            var nodes = new[]
            {
                wreckSpawns,
                survivalSpawns,
                defenceSpawns,
                BonusSpawns,
                bossSpawns
            };

            var minNodes = nodes.Sum(x => x.x);
            var maxNodes = nodes.Sum(x => x.y);

            var minSectors = nodesPerSector.y == 0 ? 0 : maxNodes / nodesPerSector.y;
            var maxSectors = nodesPerSector.x == 0 ? 0 : minNodes / nodesPerSector.x;


            expectedSectors = $"{minSectors} - {maxSectors}";
        }

        #endregion //Range

#endif
    }
}

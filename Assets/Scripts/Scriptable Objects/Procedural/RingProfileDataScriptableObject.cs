using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using UnityEditor;
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
        public class WreckSpawnData : WeightedChanceAssetBase<WreckNodeDataScriptableObject> { }

        [Serializable]
        public class WaveSpawnData : WeightedChanceAssetBase<WaveProfileDataScriptableObject>
        {
#if UNITY_EDITOR
            [HideInTables, OnInspectorInit("UpdateName")]
            public WaveProfileDataScriptableObject.WAVE_TYPE type;
            
            [OnInspectorInit]
            protected override void UpdateName() =>
                name = asset is IHasName ihn ? $"{ihn.Name} - {asset.waveType}" : string.Empty;
#endif
        }

        [Serializable]
        public class WaveSpawnDataALT : WeightedChanceAssetBase<WaveProfileDataScriptableObject>
        {
            [HideInTables] public WaveProfileDataScriptableObject.WAVE_TYPE type;

#if UNITY_EDITOR

            private bool IsWaveType(WaveProfileDataScriptableObject obj)
            {
                return obj.waveType == type;
            }

            [OnInspectorInit]
            protected override void UpdateName()
            {
                name = asset is IHasName ihs ? $"{ihs.Name}" : string.Empty;
                weight = asset == null ? 0 : weight;
            }
#endif
        }
        
        [Serializable]
        public class EnemySpawnData : WeightedChanceBase
        {
            [ValueDropdown("GetEnemies"), PropertyOrder(-100), OnValueChanged("UpdateValues"), HorizontalGroup("Enemy"),
             HideLabel]
            public string enemy;

#if UNITY_EDITOR

            [ShowInInspector, PreviewField(Height = 35, Alignment = ObjectFieldAlignment.Center), HideLabel, PropertyOrder(-1000),
             ReadOnly, TableColumnWidth(50,false)]
            public Sprite Sprite => !HasProfile(out var profile) ? null : profile.Sprite;
            
            [DisplayAsString, TableColumnWidth(45, Resizable = false), PropertyOrder(-90)]
            public int cost;

            private IEnumerable GetEnemies() => EnemyRemoteDataScriptableObject.GetEnemyTypes();

            [OnInspectorInit]
            private void UpdateValues()
            {
                if (string.IsNullOrEmpty(enemy))
                    return;

                cost = FindObjectOfType<FactoryManager>().EnemyRemoteData.GetEnemyRemoteData(enemy).Cost;
            }

            [Button, HorizontalGroup("Enemy")]
            private void Edit()
            {
                var path = AssetDatabase.GetAssetPath(FindObjectOfType<FactoryManager>().EnemyRemoteData);
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
            }
            
            private bool HasProfile(out EnemyProfileData enemyProfileData)
            {
                enemyProfileData = FindObjectOfType<FactoryManager>().EnemyProfile.GetEnemyProfileData(enemy);

                return !(enemyProfileData is null);
            }
#endif
        }

        #endregion //Spawn Structs

        //Properties
        //====================================================================================================================//

        public AnimationCurve difficultyCurve = new AnimationCurve
        {
            keys = new[]
            {
                new Keyframe(0, 0.5f, 0f, 0f),
                new Keyframe(1, 2f,0f, 0f)
            }
        };
        
        [EnumToggleButtons, PropertyOrder(-1000)]
        public TYPE type;

        #region Weighted

        [BoxGroup("Sectors"), HideLabel, OnValueChanged("BalanceNodes", true), ShowIf("@type == TYPE.WEIGHTED")]
        public RangeFixed sectors = new RangeFixed(3,10);

        [BoxGroup("Nodes Per Sector"), HideLabel, OnValueChanged("BalanceNodes"), ShowIf("@type == TYPE.WEIGHTED")]
        public RangeFixed nodes = new RangeFixed(1, 5);

        [BoxGroup("Paths"), HideLabel, ReadOnly, ShowIf("@type == TYPE.WEIGHTED")]
        public RangeFixed pathsRange = new RangeFixed(1, 3);

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

        //Enemies
        //====================================================================================================================//
        
        [TitleGroup("Enemies"), TableList(AlwaysExpanded = true), OnValueChanged("UpdateEnemyChances", true)]
        public List<EnemySpawnData> enemies;

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

            var minNodes = sectors.range.x * nodes.range.x;
            var maxNodes = sectors.range.y * nodes.range.y;

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

        //Enemies
        //====================================================================================================================//
        
        [OnInspectorInit]
        private void UpdateEnemyChances()
        {
            var sum = enemies.Sum(x => x.weight);

            for (int i = 0; i < enemies.Count; i++)
            {
                var dropData = enemies[i];
                dropData.chanceValue = dropData.weight / (float) sum;
                dropData.chance = $"{dropData.chanceValue:P1}";

                enemies[i] = dropData;
            }
        }
#endif
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Factories;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarSalvager.ScriptableObjects
{
    //For descriptions of what each of these values represent, see the comments in WaveConfigurationData
    [CreateAssetMenu(fileName = "Ring Wave Generator Remote", menuName = "Star Salvager/Scriptable Objects/Ring Wave Generator Remote")]
    public class RingWaveGeneratorRemoteDataScriptableObject : ScriptableObject
    {
        #region Spawn Data

        [Serializable]
        private struct EnemySpawn
        {
            [ValueDropdown("GetEnemyTypes"), TableColumnWidth(40), OnValueChanged("GetCost")]
            public string enemyID;

            [TableColumnWidth(25), ReadOnly] public int cost;
            [TableColumnWidth(35)] public int weight;

#if UNITY_EDITOR

            [ShowInInspector, DisplayAsString, TableColumnWidth(40)]
            public string Chance => $"{percentChance:P2}";

            [HideInTables, NonSerialized] public float percentChance;

            private void GetCost()
            {
                var cost = string.IsNullOrEmpty(enemyID)
                    ? 0
                    : FindObjectOfType<FactoryManager>().EnemyRemoteData.GetEnemyRemoteData(enemyID).Cost;
                
                this.cost = cost;
            }

            private static IEnumerable GetEnemyTypes() => EnemyRemoteDataScriptableObject.GetEnemyTypes();

#endif
        }

        [Serializable]
        private struct ObstacleSpawn
        {
            [TableColumnWidth(40)] public string obstacleID;
            [TableColumnWidth(35)] public int weight;

#if UNITY_EDITOR

            [ShowInInspector, DisplayAsString, TableColumnWidth(40)]
            public string Chance => $"{percentChance:P2}";

            [HideInTables, NonSerialized] public float percentChance;

#endif
        }

        [Serializable]
        private struct EventSpawn
        {
            [TableColumnWidth(40)] public string eventID;
            [TableColumnWidth(35)] public int weight;

#if UNITY_EDITOR

            [ShowInInspector, DisplayAsString, TableColumnWidth(40)]
            public string Chance => $"{percentChance:P2}";

            [HideInTables, NonSerialized] public float percentChance;

#endif
        }

        #endregion //Spawn Data

        //Public Properties
        //====================================================================================================================//

        public Vector2Int WaveDurationRange => mWaveDurationRange;
        public Vector2Int GridWidthRange => mGridWidthRange;

        public Vector2Int EnemyBudgetRange => mEnemyBudgetRange;

        public Vector2Int BitsPerMinuteRange => mBitsPerMinuteRange;
        public float RedBitsPercentage => redBitsPercentage / 100f;
        public float BlueBitsPercentage => blueBitsPercentage / 100f;
        public float GreenBitsPercentage => greenBitsPercentage / 100f;
        public float YellowBitsPercentage => yellowBitsPercentage / 100f;
        public float GreyBitsPercentage => greyBitsPercentage / 100f;

        //Seed Setup
        //====================================================================================================================//

        [SerializeField, BoxGroup("Seed"), HorizontalGroup("Seed/Row1"), LabelWidth(40)]
        private int seed;

        [Button, HorizontalGroup("Seed/Row1")]
        private void GenerateSeed()
        {
            Random.InitState((int) (DateTime.Now.ToUniversalTime() - new DateTime (1970, 1, 1)).TotalSeconds);
            seed = Random.Range(int.MinValue, int.MaxValue);
        }

        //Ring Properties
        //====================================================================================================================//

        [BoxGroup("Ring Wave Properties"), LabelText("Duration Range"), Tooltip("Duration is measured in Seconds")]
        [SerializeField, Required, MinMaxSlider(30, 300, true), SuffixLabel("s", true)]
        private Vector2Int mWaveDurationRange;

        [BoxGroup("Ring Wave Properties"), LabelText("Width Range")]
        [SerializeField, Required, MinMaxSlider(30, 150, true)]
        private Vector2Int mGridWidthRange;

        //Enemies
        //====================================================================================================================//

        [FoldoutGroup("Enemies"), LabelText("Enemy Budget")] [SerializeField, Required, MinMaxSlider(0, 100, true)]
        private Vector2Int mEnemyBudgetRange;

        [FoldoutGroup("Enemies")] [SerializeField, TableList]
        private List<EnemySpawn> enemySpawns;

        //Bits Properties
        //====================================================================================================================//

        [FoldoutGroup("Collectable Bits"), LabelText("Bits per/min"), LabelWidth(75)]
        [SerializeField, Required, MinMaxSlider(0, 500, true)]
        private Vector2Int mBitsPerMinuteRange;

        [FoldoutGroup("Collectable Bits"), LabelText("Red Bit %"), LabelWidth(75), Space(10f)]
        [SerializeField, Required, ProgressBar(0, 100, R = 1.0f, G = 0.3f, B = 0.3f), OnValueChanged("BalanceBits")]
        private int redBitsPercentage;

        [FoldoutGroup("Collectable Bits"), LabelText("Blue Bit %"), LabelWidth(75)]
        [SerializeField, Required, ProgressBar(0, 100, R = 0.3f, G = 0.3f, B = 1.0f), OnValueChanged("BalanceBits")]
        private int blueBitsPercentage;

        [FoldoutGroup("Collectable Bits"), LabelText("Green Bit %"), LabelWidth(75)]
        [SerializeField, Required, ProgressBar(0, 100, R = 0.3f, G = 1.0f, B = 0.3f), OnValueChanged("BalanceBits")]
        private int greenBitsPercentage;

        [FoldoutGroup("Collectable Bits"), LabelText("Yellow Bit %"), LabelWidth(75)]
        [SerializeField, Required, ProgressBar(0, 100, R = 1.0f, G = 1.0f, B = 0.3f), OnValueChanged("BalanceBits")]
        private int yellowBitsPercentage;

        [FoldoutGroup("Collectable Bits"), LabelText("Grey Bit %"), LabelWidth(75)]
        [SerializeField, Required, ProgressBar(0, 100, R = 0.9f, G = 0.9f, B = 0.9f), OnValueChanged("BalanceBits")]
        private int greyBitsPercentage;

        //Obstacle Properties
        //====================================================================================================================//

        [FoldoutGroup("Obstacles")]
        [SerializeField, TableList, InfoBox("Obstacle Spawns not yet functional", InfoMessageType.Warning), ReadOnly]
        private List<ObstacleSpawn> obstacleSpawns;

        //Event Properties
        //====================================================================================================================//

        [FoldoutGroup("Events")]
        [SerializeField, TableList, InfoBox("Event Spawns not yet functional", InfoMessageType.Warning), ReadOnly]
        private List<EventSpawn> eventSpawns;

        //Generate Wave Data
        //====================================================================================================================//

        [SerializeField, BoxGroup("Generation")]
        private WaveRemoteDataScriptableObject waveRemoteDataScriptableObject;


        //RingRemoteDataScriptableObject Functions
        //====================================================================================================================//

        public WaveConfigurationData GenerateNewWaveConfigurationData()
        {
            WaveConfigurationData waveConfigurationData = new WaveConfigurationData
            {
                WaveDuration = Random.Range(WaveDurationRange.x, WaveDurationRange.y + 1),
                GridWidth = Random.Range(GridWidthRange.x, GridWidthRange.y + 1),
                EnemyBudget = Random.Range(EnemyBudgetRange.x, EnemyBudgetRange.y + 1),
                BitsPerMinute = Random.Range(BitsPerMinuteRange.x, BitsPerMinuteRange.y + 1)
            };

            return waveConfigurationData;
        }

        //====================================================================================================================//



#if UNITY_EDITOR

        [BoxGroup("Generation"), Button]
        private void PopulateWaveData()
        {
            //--------------------------------------------------------------------------------------------------------//

            List<StageObstacleData> GetStageObstacleDatas(in int bitsPerMinute,
                in float redBits,
                in float blueBits,
                in float greenBits,
                in float greyBits,
                in float yellowBits)
            {
                var nav = new Dictionary<BIT_TYPE, float>
                {
                    [BIT_TYPE.RED] = redBits,
                    [BIT_TYPE.BLUE] = blueBits,
                    [BIT_TYPE.GREEN] = greenBits,
                    [BIT_TYPE.GREY] = greyBits,
                    [BIT_TYPE.YELLOW] = yellowBits
                };

                var obstacleDatas = new List<StageObstacleData>();

                var sum = nav.Values.Sum();

                foreach (var f in nav)
                {
                    var spm = Mathf.RoundToInt((f.Value / sum) * bitsPerMinute);
                    obstacleDatas.Add(new StageObstacleData(SELECTION_TYPE.BIT, f.Key, spm));
                }

                return obstacleDatas;
            }

            //--------------------------------------------------------------------------------------------------------//

            if (waveRemoteDataScriptableObject == null)
                return;

            
            Random.InitState(seed);
            
            //--------------------------------------------------------------------------------------------------------//

            waveRemoteDataScriptableObject.WaveSeed = seed;
            var splitTime = Random.Range(WaveDurationRange.x, WaveDurationRange.y + 1) / 5;
            
            //--------------------------------------------------------------------------------------------------------//

            var enemyBudget = Random.Range(EnemyBudgetRange.x, EnemyBudgetRange.y + 1);
            var enemySplit_0 = Mathf.RoundToInt(enemyBudget * 0.33f);
            var enemySplit_1 = enemyBudget - enemySplit_0;
            var enemyBudgets = new[]
            {
                enemySplit_0,
                enemySplit_1
            };
            
            //--------------------------------------------------------------------------------------------------------//

            var stageRemoteDatas = new List<StageRemoteData>();

            var stages = new[]
            {
                "bits",
                "enemy",
                "rest",
                "bits",
                "enemy"
            };
            
            //--------------------------------------------------------------------------------------------------------//

            for (int i = 0, j = 0; i < 5; i++)
            {
                var stageObstacleDatas = new List<StageObstacleData>();
                var stageEnemyDatas = new List<StageEnemyData>();

                switch (stages[i])
                {
                    case "bits":
                        stageObstacleDatas.AddRange(GetStageObstacleDatas(
                            Random.Range(BitsPerMinuteRange.x, BitsPerMinuteRange.y + 1),
                            redBitsPercentage, blueBitsPercentage, greenBitsPercentage, greyBitsPercentage,
                            yellowBitsPercentage));
                        break;
                    case "enemy":
                        //TODO I should loop through until I've run out of an available budget
                        var waveBudget = enemyBudgets[j++];
                        
                        //TODO I should roll on which enemy should be added
                        var selectedEnemy = enemySpawns.First();

                        var count = waveBudget / selectedEnemy.cost;
                        
                        stageEnemyDatas.Add(new StageEnemyData(selectedEnemy.enemyID, count));
                        break;
                    case "rest":
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(stages), stages[i], null);
                }


                //TODO Need to get the actual Obstacles
                stageRemoteDatas.Add(new StageRemoteData(splitTime,
                    Random.Range(GridWidthRange.x, GridWidthRange.y + 1),
                    stageObstacleDatas,
                    stageEnemyDatas
                ));
            }
            
            //--------------------------------------------------------------------------------------------------------//


            waveRemoteDataScriptableObject.StageRemoteData = new List<StageRemoteData>(stageRemoteDatas);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            
            //--------------------------------------------------------------------------------------------------------//
        }

        private void BalanceBits()
        {
            var sum = redBitsPercentage + blueBitsPercentage + greenBitsPercentage + yellowBitsPercentage +
                      greyBitsPercentage;

            redBitsPercentage = Mathf.RoundToInt(((float) redBitsPercentage / sum) * 100f);
            blueBitsPercentage = Mathf.RoundToInt(((float) blueBitsPercentage / sum) * 100f);
            greenBitsPercentage = Mathf.RoundToInt(((float) greenBitsPercentage / sum) * 100f);
            yellowBitsPercentage = Mathf.RoundToInt(((float) yellowBitsPercentage / sum) * 100f);
            greyBitsPercentage = Mathf.RoundToInt(((float) greyBitsPercentage / sum) * 100f);

        }

        [Button("Balance Bits"), FoldoutGroup("Collectable Bits")]
        private void Reset()
        {
            redBitsPercentage =
                blueBitsPercentage = greenBitsPercentage = yellowBitsPercentage = greyBitsPercentage = 20;
        }

#endif

    }
}
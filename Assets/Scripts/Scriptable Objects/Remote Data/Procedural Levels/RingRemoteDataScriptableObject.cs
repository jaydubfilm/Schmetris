using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarSalvager.ScriptableObjects
{
    //For descriptions of what each of these values represent, see the comments in WaveConfigurationData
    [CreateAssetMenu(fileName = "Ring Remote", menuName = "Star Salvager/Scriptable Objects/Ring Remote Data")]
    public class RingRemoteDataScriptableObject : ScriptableObject
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

            private int GetCost()
            {
                return string.IsNullOrEmpty(enemyID)
                    ? 0
                    : FindObjectOfType<FactoryManager>().EnemyRemoteData.GetEnemyRemoteData(enemyID).Cost;
            }

            private static IEnumerable GetEnemyTypes()
            {
                return FindObjectOfType<FactoryManager>().EnemyRemoteData.GetEnemyTypes();
            }

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

        //Ring Properties
        //====================================================================================================================//

        [BoxGroup("Ring Wave Properties"), LabelText("Duration Range"), Tooltip("Duration is measured in Seconds")]
        [SerializeField, Required, MinMaxSlider(30, 240, true), SuffixLabel("s", true)]
        private Vector2Int mWaveDurationRange;

        [BoxGroup("Ring Wave Properties"), LabelText("Width Range")]
        [SerializeField, Required, MinMaxSlider(30, 70, true)]
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
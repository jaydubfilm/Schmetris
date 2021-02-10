using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    //For descriptions of what each of these values represent, see the comments in WaveConfigurationData
    [CreateAssetMenu(fileName = "Ring Remote", menuName = "Star Salvager/Scriptable Objects/Ring Remote Data")]
    public class RingRemoteDataScriptableObject : ScriptableObject
    {
        //Public Properties
        //====================================================================================================================//
        
        public Vector2Int WaveDurationRange => mWaveDurationRange;
        public Vector2Int GridWidthRange => mGridWidthRange;

        public List<StageEnemyData> StageEnemyData => mStageEnemyData;
        public Vector2Int EnemyBudgetRange => mEnemyBudgetRange;

        public Vector2Int BitsPerMinuteRange => mBitsPerMinuteRange;
        public Vector2 RedBitsPercentageRange => mRedBitsPercentageRange;
        public Vector2 BlueBitsPercentageRange => mBlueBitsPercentageRange;
        public Vector2 GreenBitsPercentageRange => mGreenBitsPercentageRange;
        public Vector2 YellowBitsPercentageRange => mYellowBitsPercentageRange;
        public Vector2 GreyBitsPercentageRange => mGreyBitsPercentageRange;

        //====================================================================================================================//
        
        [SerializeField, Required, MinMaxSlider(30, 240, true), SuffixLabel("s", true)]
        private Vector2Int mWaveDurationRange = new Vector2Int(30, 240);

        [SerializeField, Required, MinMaxSlider(30, 70, true)]
        private Vector2Int mGridWidthRange = new Vector2Int(30, 70);

        [SerializeField]
        private List<StageEnemyData> mStageEnemyData;

        [SerializeField, Required, MinMaxSlider(0, 50, true)]
        private Vector2Int mEnemyBudgetRange;

        //Bits Properties
        //====================================================================================================================//
        
        [FoldoutGroup("Bits"), LabelText("Bits per/min"), LabelWidth(75)]
        [SerializeField, Required, MinMaxSlider(0, 500, true)]
        private Vector2Int mBitsPerMinuteRange;

        [FoldoutGroup("Bits"), LabelText("Red Bit %"), LabelWidth(75), Space(10f)]
        [SerializeField, Required, MinMaxSlider(0, 100, true), GUIColor(1.0f, 0.3f, 0.3f)]
        private Vector2Int mRedBitsPercentageRange;

        [FoldoutGroup("Bits"), LabelText("Blue Bit %"), LabelWidth(75)]
        [SerializeField, Required, MinMaxSlider(0, 100, true), GUIColor(0.3f, 0.3f, 1.0f)]
        private Vector2Int mBlueBitsPercentageRange;

        [FoldoutGroup("Bits"), LabelText("Green Bit %"), LabelWidth(75)]
        [SerializeField, Required, MinMaxSlider(0, 100, true), GUIColor(0.3f, 1.0f, 0.3f)]
        private Vector2Int mGreenBitsPercentageRange;

        [FoldoutGroup("Bits"), LabelText("Yellow Bit %"), LabelWidth(75)]
        [SerializeField, Required, MinMaxSlider(0, 100, true), GUIColor(1.0f, 1.0f, 0.3f)]
        private Vector2Int mYellowBitsPercentageRange;

        [FoldoutGroup("Bits"), LabelText("Grey Bit %"), LabelWidth(75)]
        [SerializeField, Required, MinMaxSlider(0, 100, true)]
        private Vector2Int mGreyBitsPercentageRange;


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
        
    }
}
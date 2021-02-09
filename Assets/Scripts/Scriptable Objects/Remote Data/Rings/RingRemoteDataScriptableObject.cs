using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Factories;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    //For descriptions of what each of these values represent, see the comments in WaveConfigurationData
    [CreateAssetMenu(fileName = "Ring Remote", menuName = "Star Salvager/Scriptable Objects/Ring Remote Data")]
    public class RingRemoteDataScriptableObject : ScriptableObject
    {
        [SerializeField, Required, MinMaxSlider(30, 240)]
        private Vector2Int m_waveDurationRange = new Vector2Int(30, 240);

        [SerializeField, Required, MinMaxSlider(30, 70)]
        private Vector2Int m_gridWidthRange = new Vector2Int(30, 70);

        [SerializeField]
        private List<StageEnemyData> m_stageEnemyData;

        [SerializeField, Required, MinMaxSlider(0, 50)]
        private Vector2Int m_enemyBudgetRange;

        [SerializeField, Required, MinMaxSlider(0, 500)]
        private Vector2Int m_bitsPerMinuteRange = new Vector2Int(0, 500);

        [SerializeField, Required, MinMaxSlider(0, 1)]
        private Vector2 m_redBitsPercentageRange = new Vector2(0, 1);

        [SerializeField, Required, MinMaxSlider(0, 1)]
        private Vector2 m_blueBitsPercentageRange = new Vector2(0, 1);

        [SerializeField, Required, MinMaxSlider(0, 1)]
        private Vector2 m_greenBitsPercentageRange = new Vector2(0, 1);

        [SerializeField, Required, MinMaxSlider(0, 1)]
        private Vector2 m_yellowBitsPercentageRange = new Vector2(0, 1);

        [SerializeField, Required, MinMaxSlider(0, 1)]
        private Vector2 m_greyBitsPercentageRange = new Vector2(0, 1);

        //====================================================================================================================//

        public Vector2Int WaveDurationRange => m_waveDurationRange;
        public Vector2Int GridWidthRange => m_gridWidthRange;

        public List<StageEnemyData> StageEnemyData => m_stageEnemyData;
        public Vector2Int EnemyBudgetRange => m_enemyBudgetRange;

        public Vector2Int BitsPerMinuteRange => m_bitsPerMinuteRange;
        public Vector2 RedBitsPercentageRange => m_redBitsPercentageRange;
        public Vector2 BlueBitsPercentageRange => m_blueBitsPercentageRange;
        public Vector2 GreenBitsPercentageRange => m_greenBitsPercentageRange;
        public Vector2 YellowBitsPercentageRange => m_yellowBitsPercentageRange;
        public Vector2 GreyBitsPercentageRange => m_greyBitsPercentageRange;

        public WaveConfigurationData GenerateNewWaveConfigurationData()
        {
            WaveConfigurationData waveConfigurationData = new WaveConfigurationData();

            waveConfigurationData.WaveDuration = Random.Range(WaveDurationRange.x, WaveDurationRange.y + 1);
            waveConfigurationData.GridWidth = Random.Range(GridWidthRange.x, GridWidthRange.y + 1);

            waveConfigurationData.EnemyBudget = Random.Range(EnemyBudgetRange.x, EnemyBudgetRange.y + 1);

            waveConfigurationData.BitsPerMinute = Random.Range(BitsPerMinuteRange.x, BitsPerMinuteRange.y + 1);


            return waveConfigurationData;
        }
    }
}
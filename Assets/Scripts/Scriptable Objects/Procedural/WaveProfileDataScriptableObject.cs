using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.ScriptableObjects.Procedural
{

    
    [CreateAssetMenu(fileName = "Wave Profile Data", menuName = "Star Salvager/Scriptable Objects/Wave Profile Data")]
    public class WaveProfileDataScriptableObject : BaseMapNodeScriptableObject
    {
        //Articles to Review:
        //https://riskofrain2.fandom.com/wiki/Directors
        //https://gamedev.stackexchange.com/questions/60700/how-do-i-write-a-wave-spawning-system-for-a-shoot-em-up
        //https://gamedev.stackexchange.com/questions/153840/how-can-i-spawn-items-based-on-probabilities
        //https://www.gamasutra.com/view/news/316020/Procedurally_generating_enemies_places_and_loot_in_State_of_Decay_2.php
        
        
        //Enums
        //====================================================================================================================//

        public enum WAVE_TYPE
        {
            SURVIVAL,
            DEFENCE,
            BONUS,
            MINI_BOSS
        }

        //Structs
        //====================================================================================================================//

        #region Structs

        [Serializable]
        public class StageSpawnData : WeightedChanceAssetBase<StageProfileDataSO> { }
        


        #endregion //Structs

        //Properties
        //====================================================================================================================//

        [EnumToggleButtons, LabelWidth(90), VerticalGroup("row1/col2"), Space(10f)]
        public WAVE_TYPE waveType;

        [VerticalGroup("row1/col2"), BoxGroup("row1/col2/Wave Time"), HideLabel, HideIf("DisableTime")]
        public RangeFixed waveTime = new RangeFixed(10, 300);
        
        public AnimationCurve excitementCurve;

        [TitleGroup("Stages"),TableList(AlwaysExpanded = true), OnValueChanged("UpdateStageChances", true), HideLabel]
        public List<StageSpawnData> stages;

        [TitleGroup("Enemies"), BoxGroup("Enemies/Enemy Budget"), HideLabel, OnValueChanged("UpdateEnemyChances")]
        public RangeFixed enemyBudget = new RangeFixed(0,100);


        
        //public Vector2Int enemyBudget;




        //Unity Editor
        //====================================================================================================================//

        #region Unity Editor

#if UNITY_EDITOR

        private bool DisableTime => !(waveType == WAVE_TYPE.BONUS || waveType == WAVE_TYPE.SURVIVAL);


        
        [OnInspectorInit]
        private void UpdateStageChances()
        {
            var sum = stages.Sum(x => x.weight);

            for (int i = 0; i < stages.Count; i++)
            {
                var dropData = stages[i];
                dropData.chanceValue = dropData.weight / (float) sum;
                dropData.chance = $"{dropData.chanceValue:P1}";

                stages[i] = dropData;
            }
        }
#endif

        #endregion //Unity Editor

        //====================================================================================================================//
        
    }
}

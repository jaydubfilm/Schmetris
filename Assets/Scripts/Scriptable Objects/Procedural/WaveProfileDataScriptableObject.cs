using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

namespace StarSalvager.ScriptableObjects.Procedural
{
    [CreateAssetMenu(fileName = "Wave Profile Data", menuName = "Star Salvager/Scriptable Objects/Wave Profile Data")]
    public class WaveProfileDataScriptableObject : BaseMapNodeScriptableObject
    {
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

        [Serializable]
        public struct EnemySpawnData
        {
            [ValueDropdown("GetEnemies"), PropertyOrder(-100), OnValueChanged("UpdateValues")]
            public string enemy;

            [Range(1,10)]
            public int weight;

#if UNITY_EDITOR
            
            [DisplayAsString, TableColumnWidth(75, Resizable = false), PropertyOrder(-90)]
            public string cost;
            
            [DisplayAsString, TableColumnWidth(75, Resizable = false)]
            public string chance;

            [HideInTables] 
            public float chanceValue;

            private IEnumerable GetEnemies() => EnemyRemoteDataScriptableObject.GetEnemyTypes();

            [OnInspectorInit]
            private void UpdateValues()
            {
                if (string.IsNullOrEmpty(enemy))
                    return;

                cost = $"{FindObjectOfType<FactoryManager>().EnemyRemoteData.GetEnemyRemoteData(enemy).Cost}";
            }


#endif
        }

        //Properties
        //====================================================================================================================//
        
        [EnumToggleButtons, LabelWidth(90)] public WAVE_TYPE WaveType;

        [MinMaxSlider(10, 300), Tooltip("Time is in Seconds"), ShowIf("ShowTime")]
        public Vector2Int waveTimeRange;

        public List<StageProfileDataScriptableObject> stages;

        [MinMaxSlider(0,100, true)]
        public Vector2Int enemyBudget;

        [TableList, OnValueChanged("UpdateEnemyChances", true)]
        public List<EnemySpawnData> enemies;


        //Unity Editor
        //====================================================================================================================//
        
#if UNITY_EDITOR
        
        private bool ShowTime => WaveType == WAVE_TYPE.BONUS || WaveType == WAVE_TYPE.SURVIVAL;

        
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using UnityEditor;
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
        public struct StageSpawnData
        {
            [OnValueChanged("UpdateName", true)]
            public StageProfileDataScriptableObject asset;
            
            [Range(1, 10), HideIf("@asset == null")] public int weight;
            
#if UNITY_EDITOR
            [PropertyOrder(-100), DisplayAsString]
            public string name;

            
            [DisplayAsString, TableColumnWidth(75, Resizable = false), HideIf("@asset == null")]
            public string chance;

            [HideInTables] public float chanceValue;

            [OnInspectorInit]
            private void UpdateName() => name = asset == null ? string.Empty : asset.name;
#endif
        }
        
        [Serializable]
        public struct EnemySpawnData
        {
            [ValueDropdown("GetEnemies"), PropertyOrder(-100), OnValueChanged("UpdateValues"), HorizontalGroup("Enemy"),
             HideLabel]
            public string enemy;

            [Range(1, 10)] public int weight;

#if UNITY_EDITOR

            [ShowInInspector, PreviewField(Height = 35, Alignment = ObjectFieldAlignment.Center), HideLabel, PropertyOrder(-1000),
             ReadOnly, TableColumnWidth(50,false)]
            public Sprite Sprite => !HasProfile(out var profile) ? null : profile.Sprite;
            
            [DisplayAsString, TableColumnWidth(45, Resizable = false), PropertyOrder(-90)]
            public int cost;

            [DisplayAsString, TableColumnWidth(75, Resizable = false)]
            public string chance;

            [HideInTables] public float chanceValue;

            [DisplayAsString, TableColumnWidth(95, Resizable = false)]
            public string spawns;

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

        #endregion //Structs

        //Properties
        //====================================================================================================================//

        [EnumToggleButtons, LabelWidth(90), VerticalGroup("row1/col2"), Space(10f)]
        public WAVE_TYPE waveType;

        [MinMaxSlider(10, 300), Tooltip("Time is in Seconds"), ShowIf("ShowTime"), VerticalGroup("row1/col2")]
        public Vector2Int waveTimeRange;

        [TitleGroup("Stages"),TableList(AlwaysExpanded = true), OnValueChanged("UpdateStageChances", true), HideLabel]
        public List<StageSpawnData> stages;

        [TitleGroup("Enemies"), MinMaxSlider(0, 100, true), OnValueChanged("UpdateEnemyChances")]
        public Vector2Int enemyBudget;

        [TitleGroup("Enemies"), TableList(AlwaysExpanded = true), OnValueChanged("UpdateEnemyChances", true)]
        public List<EnemySpawnData> enemies;


        //Unity Editor
        //====================================================================================================================//

        #region Unity Editor

#if UNITY_EDITOR

        private bool ShowTime => waveType == WAVE_TYPE.BONUS || waveType == WAVE_TYPE.SURVIVAL;

        [OnInspectorInit]
        private void UpdateEnemyChances()
        {
            var sum = enemies.Sum(x => x.weight);

            for (int i = 0; i < enemies.Count; i++)
            {
                var dropData = enemies[i];
                dropData.chanceValue = dropData.weight / (float) sum;
                dropData.chance = $"{dropData.chanceValue:P1}";

                if (enemyBudget.y == 0 || dropData.cost == 0)
                    dropData.spawns = "Infinite";
                else
                {
                    var min = (enemyBudget.x * dropData.chanceValue) / dropData.cost;
                    var max = enemyBudget.y * dropData.chanceValue / dropData.cost;
                    
                    dropData.spawns = $"{Mathf.FloorToInt( min)} - {Mathf.CeilToInt(max)}";
                    
                }

                enemies[i] = dropData;
            }
        }
        
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

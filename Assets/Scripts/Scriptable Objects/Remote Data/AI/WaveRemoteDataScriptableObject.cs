using StarSalvager.AI;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Wave Remote", menuName = "Star Salvager/Scriptable Objects/Wave Remote Data")]
    public class WaveRemoteDataScriptableObject : ScriptableObject
    {
        public int WaveSeed;
        
        public List<StageObstacleShapeData> BonusShapes = new List<StageObstacleShapeData>();
        
        public List<StageRemoteData> StageRemoteData = new List<StageRemoteData>();

        [SerializeField]
        private int maxDrops;

        [SerializeField]
        private List<RDSLootData> RDSEndOfWaveLoot = new List<RDSLootData>();

        public RDSTable rdsTable;

        public float BonusShapeFrequency => GetWaveDuration() / (BonusShapes.Count + 1);

        public void ConfigureLootTable()
        {
            rdsTable = new RDSTable();
            rdsTable.SetupRDSTable(maxDrops, RDSEndOfWaveLoot);
        }

        public StageRemoteData GetRemoteData(int waveNumber)
        {
            return StageRemoteData[waveNumber];
        }

        public bool TrySetCurrentStage(float stageTimer, out int currentStage)
        {
            currentStage = 0;

            while (stageTimer >= StageRemoteData[currentStage].StageDuration)
            {
                if (StageRemoteData[currentStage].WaitUntilAllEnemiesDefeatedToBegin &&
                    LevelManager.Instance.EnemyManager.HasEnemiesRemaining())
                {
                    return true;
                }

                stageTimer -= StageRemoteData[currentStage].StageDuration;
                currentStage++;

                if (currentStage >= StageRemoteData.Count)
                {
                    return false;
                }
            }

            return true;
        }

        public float GetWaveDuration()
        {
            float waveDuration = 0;

            for (int i = 0; i < StageRemoteData.Count; i++)
            {
                waveDuration += StageRemoteData[i].StageDuration;   
            }

            return waveDuration;
        }

        public (Dictionary<string, int> Enemies, Dictionary<BIT_TYPE, float> Bits) GetWaveSummaryData()
        {
            var enemies = new Dictionary<string, int>();
            var bits = new Dictionary<BIT_TYPE, float>
            {
                [BIT_TYPE.RED] = 0.5f,
                [BIT_TYPE.GREY] = 0.3f,
                [BIT_TYPE.BLUE] = 0.1f,
                [BIT_TYPE.GREEN] = 0.1f,
            };
            foreach (var stageRemoteData in StageRemoteData)
            {
                foreach (var enemyData in stageRemoteData.StageEnemyData)
                {
                    var enemyType = enemyData.EnemyType;
                    
                    if(!enemies.ContainsKey(enemyType))
                        enemies.Add(enemyType, 0);

                    enemies[enemyType] += enemyData.EnemyCount;
                }
                
                /*foreach (var obstacleData in stageRemoteData.StageObstacleData)
                {
                    //TODO Need to get the shape data here, to determine what is in the wave
                    //obstacleData.
                }*/
            }

            return (enemies, bits);
        }
    }
}


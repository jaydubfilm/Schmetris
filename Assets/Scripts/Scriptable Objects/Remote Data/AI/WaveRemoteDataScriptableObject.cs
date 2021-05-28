using StarSalvager.AI;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Waves;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Wave Remote", menuName = "Star Salvager/Scriptable Objects/Wave Remote Data")]
    public class WaveRemoteDataScriptableObject : ScriptableObject
    {
        public int WaveSeed;
        
        public int WaveXP;

        public WAVE_TYPE Type = WAVE_TYPE.SURVIVAL;
        
        public List<StageObstacleShapeData> BonusShapes = new List<StageObstacleShapeData>();
        
        public List<StageRemoteData> StageRemoteData = new List<StageRemoteData>();

        [SerializeField]
        private int maxDrops;

        [SerializeField]
        private List<RDSLootData> RDSEndOfWaveLoot = new List<RDSLootData>();

        //public RDSTable rdsTable;

        public float BonusShapeFrequency => GetWaveDuration() / (BonusShapes.Count + 1);

        public void ConfigureLootTable()
        {
            //rdsTable = new RDSTable();
            //rdsTable.SetupRDSTable(maxDrops, RDSEndOfWaveLoot);
        }

        public StageRemoteData GetRemoteData(int waveNumber)
        {
            if (waveNumber >= StageRemoteData.Count)
            {
                return StageRemoteData[StageRemoteData.Count - 1];
            }
            
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

        public (Dictionary<string, int> Enemies, Dictionary<BIT_TYPE, float> Bits) GetWaveSummaryData(bool useSum)
        {
            var enemies = new Dictionary<string, int>();
            var bits = new Dictionary<BIT_TYPE, float>();
            foreach (var stageRemoteData in StageRemoteData)
            {
                foreach (var enemyData in stageRemoteData.StageEnemyData)
                {
                    var enemyType = enemyData.EnemyType;
                    
                    if(!enemies.ContainsKey(enemyType))
                        enemies.Add(enemyType, 0);

                    enemies[enemyType] += enemyData.EnemyCount;
                }
                
                foreach (var obstacleData in stageRemoteData.StageObstacleData)
                {
                    //TODO Need to get the shape data here, to determine what is in the wave
                    switch(obstacleData.SelectionType)
                    {
                        case SELECTION_TYPE.ASTEROID:
                        case SELECTION_TYPE.BUMPER:
                            continue;
                        case SELECTION_TYPE.SHAPE:
                            List<IBlockData> shapeBlockData = FactoryManager.Instance.GetFactory<ShapeFactory>().GetByName(obstacleData.ShapeName).BlockData;
                            foreach (var blockData in shapeBlockData)
                            {
                                BIT_TYPE bitType = (BIT_TYPE)blockData.Type;
                                if (!bits.ContainsKey(bitType))
                                {
                                    bits.Add(bitType, 0.0f);
                                }

                                bits[bitType] += 1.0f * obstacleData.Density() * stageRemoteData.StageDuration;
                            }

                            break;
                        case SELECTION_TYPE.CATEGORY:
                            List<EditorShapeGeneratorData> shapesInCategory = FactoryManager.Instance.GetFactory<ShapeFactory>().GetCategoryData(obstacleData.Category);
                            int numShapesInCategory = shapesInCategory.Count;
                            foreach (var shapeInCategory in shapesInCategory)
                            {
                                foreach (var blockData in shapeInCategory.BlockData)
                                {
                                    BIT_TYPE bitType = (BIT_TYPE)blockData.Type;
                                    if (!bits.ContainsKey(bitType))
                                    {
                                        bits.Add(bitType, 0.0f);
                                    }

                                    bits[bitType] += (1.0f * obstacleData.Density() * stageRemoteData.StageDuration) / numShapesInCategory;
                                }
                            }

                            break;
                    }
                }
            }

            if(useSum)
                return (enemies, bits);
            
            float totalValueBits = 0.0f;
            foreach (var keyValuePair in bits)
            {
                totalValueBits += keyValuePair.Value;
            }

            foreach (var key in bits.Keys.ToList())
            {
                bits[key] *= (1.0f / totalValueBits);
            }

            return (enemies, bits);
        }

        public List<BIT_TYPE> GetBitTypesInWave()
        {
            List<BIT_TYPE> bitTypes = new List<BIT_TYPE>();

            foreach (var stageRemoteData in StageRemoteData)
            {
                foreach (var obstacleData in stageRemoteData.StageObstacleData)
                {
                    //TODO Need to get the shape data here, to determine what is in the wave
                    switch (obstacleData.SelectionType)
                    {
                        case SELECTION_TYPE.ASTEROID:
                        case SELECTION_TYPE.BUMPER:
                            continue;
                        case SELECTION_TYPE.SHAPE:
                            List<IBlockData> shapeBlockData = FactoryManager.Instance.GetFactory<ShapeFactory>().GetByName(obstacleData.ShapeName).BlockData;
                            foreach (var blockData in shapeBlockData)
                            {
                                BIT_TYPE bitType = (BIT_TYPE)blockData.Type;
                                if (!bitTypes.Contains(bitType))
                                {
                                    bitTypes.Add(bitType);
                                }
                            }

                            break;
                        case SELECTION_TYPE.CATEGORY:
                            List<EditorShapeGeneratorData> shapesInCategory = FactoryManager.Instance.GetFactory<ShapeFactory>().GetCategoryData(obstacleData.Category);
                            int numShapesInCategory = shapesInCategory.Count;
                            foreach (var shapeInCategory in shapesInCategory)
                            {
                                foreach (var blockData in shapeInCategory.BlockData)
                                {
                                    BIT_TYPE bitType = (BIT_TYPE)blockData.Type;
                                    if (!bitTypes.Contains(bitType))
                                    {
                                        bitTypes.Add(bitType);
                                    }
                                }
                            }

                            break;
                    }
                }
            }

            return bitTypes;
        }


#if UNITY_EDITOR

        /*[Button]
        private void UpdateData()
        {
            for (int i = 0; i < StageRemoteData.Count; i++)
            {
                for (int k = 0; k < StageRemoteData[i].StageObstacleData.Count; k++)
                {
                    StageRemoteData[i].StageObstacleData[k].UpdateDensity();
                }
            }
        }*/

        public void OnValidate()
        {
            for (int i = 0; i < StageRemoteData.Count; i++)
            {
                for (int k = 0; k < StageRemoteData[i].StageObstacleData.Count; k++)
                {
                    StageRemoteData[i].StageObstacleData[k].spawningMultiplier = StageRemoteData[i].SpawningObstacleMultiplier;
                }
            }
        }
#endif
    }
}


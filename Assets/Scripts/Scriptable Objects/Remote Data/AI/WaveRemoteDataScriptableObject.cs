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
    }
}


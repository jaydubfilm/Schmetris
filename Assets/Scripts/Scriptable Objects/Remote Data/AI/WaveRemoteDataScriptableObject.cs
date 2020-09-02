using System.Collections.Generic;
using System.Linq;
using StarSalvager.AI;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Wave Remote", menuName = "Star Salvager/Scriptable Objects/Wave Remote Data")]
    public class WaveRemoteDataScriptableObject : ScriptableObject
    {
        [Range(3, 1000)]
        public int ColumnRepresentation;
        
        public List<StageRemoteData> StageRemoteData = new List<StageRemoteData>();

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


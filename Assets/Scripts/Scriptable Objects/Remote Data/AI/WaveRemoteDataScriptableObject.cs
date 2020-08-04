using System.Collections.Generic;
using System.Linq;
using StarSalvager.AI;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Wave Remote", menuName = "Star Salvager/Scriptable Objects/Wave Remote Data")]
    public class WaveRemoteDataScriptableObject : ScriptableObject
    {
        public List<StageRemoteData> StageRemoteData = new List<StageRemoteData>();

        public StageRemoteData GetRemoteData(int waveNumber)
        {
            return StageRemoteData[waveNumber];
        }

        public int GetCurrentStage(float stageTimer)
        {
            int currentStage = 0;

            while (stageTimer >= StageRemoteData[currentStage].StageDuration)
            {
                if (StageRemoteData[currentStage].WaitUntilAllEnemiesDefeatedToBegin &&
                    LevelManager.Instance.EnemyManager.HasEnemiesRemaining())
                {
                    return currentStage;
                }

                stageTimer -= StageRemoteData[currentStage].StageDuration;
                currentStage++;

                if (currentStage >= StageRemoteData.Count)
                {
                    return -1;
                }
            }

            return currentStage;
        }
    }
}


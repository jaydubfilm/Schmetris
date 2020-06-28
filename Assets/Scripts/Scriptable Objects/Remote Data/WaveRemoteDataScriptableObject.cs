using System.Collections.Generic;
using System.Linq;
using StarSalvager.AI;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Wave Remote", menuName = "Star Salvager/Scriptable Objects/Wave Remote Data")]
    public class WaveRemoteDataScriptableObject : ScriptableObject
    {
        private bool m_orderedByWaveNumber = false;
        
        public List<StageRemoteData> StageRemoteData = new List<StageRemoteData>();

        public StageRemoteData GetRemoteData(int waveNumber)
        {
            return StageRemoteData
                .FirstOrDefault(p => p.StageNumber == waveNumber);
        }

        public int GetCurrentStage(float stageTimer)
        {
            int currentStage = 0;

            if (!m_orderedByWaveNumber)
            {
                StageRemoteData.OrderBy(p => p.StageNumber);
                m_orderedByWaveNumber = true;
            }

            while (stageTimer >= StageRemoteData[currentStage].StageDuration)
            {
                stageTimer -= StageRemoteData[currentStage].StageDuration;
                currentStage++;
            }

            return currentStage;
        }
    }
}


using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.AI
{
    [Serializable]
    public class StageRemoteData
    {
        [SerializeField]
        private float m_stageDuration;
        [SerializeField]
        private float m_stageBlendPeriod;
        [SerializeField]
        private bool m_waitUntilAllEnemiesDefeatedToBegin;
        [SerializeField]
        private List<StageEnemyData> m_stageEnemyData;
        [SerializeField]
        private List<StageColumnGroupObstacleData> m_stageColumnGroupObstacleData;

        public float StageDuration => m_stageDuration;
        public float StageBlendPeriod => m_stageBlendPeriod;
        public bool WaitUntilAllEnemiesDefeatedToBegin => m_waitUntilAllEnemiesDefeatedToBegin;
        public List<StageEnemyData> StageEnemyData => m_stageEnemyData;
        public List<StageColumnGroupObstacleData> StageColumnGroupObstacleData => m_stageColumnGroupObstacleData;
    }
}
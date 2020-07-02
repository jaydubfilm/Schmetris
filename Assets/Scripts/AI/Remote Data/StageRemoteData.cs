using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.AI
{
    [Serializable]
    public class StageRemoteData
    {
        [SerializeField, FoldoutGroup("$m_stageNumber")]
        private int m_stageNumber;
        [SerializeField, FoldoutGroup("$m_stageNumber")]
        private float m_stageDuration;
        [SerializeField, FoldoutGroup("$m_stageNumber")]
        private float m_stageBlendPeriod;
        [SerializeField, FoldoutGroup("$m_stageNumber")]
        private bool m_waitUntilAllEnemiesDefeatedToBegin;
        [SerializeField, FoldoutGroup("$m_stageNumber")]
        private List<StageEnemyData> m_stageEnemyData;
        [SerializeField, FoldoutGroup("$m_stageNumber")]
        private List<StageObstacleData> m_stageObstacleData;

        public int StageNumber => m_stageNumber;
        public float StageDuration => m_stageDuration;
        public float StageBlendPeriod => m_stageBlendPeriod;
        public bool WaitUntilAllEnemiesDefeatedToBegin => m_waitUntilAllEnemiesDefeatedToBegin;
        public List<StageEnemyData> StageEnemyData => m_stageEnemyData;
        public List<StageObstacleData> StageObstacleData => m_stageObstacleData;
    }
}
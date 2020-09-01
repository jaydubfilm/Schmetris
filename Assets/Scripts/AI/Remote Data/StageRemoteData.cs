using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    [Serializable]
    public class StageRemoteData
    {
        [SerializeField, Min(1.0f)]
        private float m_stageDuration;
        [SerializeField]
        private float m_stageBlendPeriod;
        [SerializeField]
        private bool m_waitUntilAllEnemiesDefeatedToBegin;
        [SerializeField]
        private List<StageEnemyData> m_stageEnemyData;
        [SerializeField]
        private STAGE_TYPE m_stageType;
        [SerializeField]
        private float m_spawningObstacleMultiplier = 1.0f;

        [SerializeField, ShowIf("m_stageType", STAGE_TYPE.STANDARD), Required, Range(0.0f, 1.0f)]
        private float m_centerColumnWidth = 0.5f;
        [SerializeField, HideIf("m_stageType", STAGE_TYPE.CUSTOM)]
        private List<StageObstacleData> m_stageObstacleData;

        [SerializeField, ShowIf("m_stageType", STAGE_TYPE.CUSTOM)]
        private List<StageColumnGroupObstacleData> m_stageColumnGroupObstacleData;

        public float StageDuration => m_stageDuration;
        public float StageBlendPeriod => m_stageBlendPeriod;
        public bool WaitUntilAllEnemiesDefeatedToBegin => m_waitUntilAllEnemiesDefeatedToBegin;
        public STAGE_TYPE StageType => m_stageType;
        public float SpawningObstacleMultiplier => m_spawningObstacleMultiplier;
        public List<StageEnemyData> StageEnemyData => m_stageEnemyData;

        public List<StageObstacleData> StageObstacleData => m_stageObstacleData;
        public float CenterColumnWidth => m_centerColumnWidth;

        public List<StageColumnGroupObstacleData> StageColumnGroupObstacleData => m_stageColumnGroupObstacleData;
    }
}
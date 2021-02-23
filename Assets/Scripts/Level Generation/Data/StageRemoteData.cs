using Sirenix.OdinInspector;
using StarSalvager.Values;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    [Serializable]
    public class StageRemoteData
    {
        public float StageDuration => Mathf.Max(1.0f, m_stageDuration);
        public float StageBlendPeriod => m_stageBlendPeriod;
        public bool WaitUntilAllEnemiesDefeatedToBegin => m_waitUntilAllEnemiesDefeatedToBegin;

        public List<StageEnemyData> StageEnemyData => m_stageEnemyData;

        public List<StageObstacleData> StageObstacleData => m_stageObstacleData;
        
        public int testWidth;

        //====================================================================================================================//
        
        [SerializeField, Min(1.0f)]
        private float m_stageDuration;
        [SerializeField]
        private float m_stageBlendPeriod;
        [SerializeField]
        private bool m_waitUntilAllEnemiesDefeatedToBegin;
        [SerializeField]
        private List<StageEnemyData> m_stageEnemyData;

        [SerializeField]
        private List<StageObstacleData> m_stageObstacleData;

        //====================================================================================================================//
        
        public StageRemoteData(in int stageTime, in int stageWidth, in List<StageObstacleData> obstacleData, in List<StageEnemyData> enemyData)
        {
            m_stageDuration = stageTime;
            testWidth = stageWidth;
            
            m_stageObstacleData = new List<StageObstacleData>(obstacleData);
            m_stageEnemyData = new List<StageEnemyData>(enemyData);
        }
    }
}
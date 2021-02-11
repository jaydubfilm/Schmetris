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
        [SerializeField, Min(1.0f)]
        private float m_stageDuration;
        [SerializeField]
        private float m_stageBlendPeriod;
        [SerializeField]
        private bool m_waitUntilAllEnemiesDefeatedToBegin;
        [SerializeField]
        private List<StageEnemyData> m_stageEnemyData;
        //[SerializeField]
        //private STAGE_TYPE m_stageType;
        //[SerializeField]
        //private float m_spawningObstacleMultiplier = 1.0f;

        //[SerializeField, ShowIf("m_stageType", STAGE_TYPE.STANDARD), Required, Range(0.0f, 1.0f)]
        //private float m_centerChannelWidth = 0.5f;
        //[ShowInInspector, ShowIf("m_stageType", STAGE_TYPE.STANDARD), DisplayAsString]
        //private string m_numColumns => (Globals.GridSizeX * m_centerChannelWidth).ToString();
        [SerializeField/*, HideIf("m_stageType", STAGE_TYPE.CUSTOM)*/]
        private List<StageObstacleData> m_stageObstacleData;

        //[SerializeField, ShowIf("m_stageType", STAGE_TYPE.CUSTOM)]
        //private List<StageColumnGroupObstacleData> m_stageColumnGroupObstacleData;

        public float StageDuration => Mathf.Max(1.0f, m_stageDuration);
        public float StageBlendPeriod => m_stageBlendPeriod;
        public bool WaitUntilAllEnemiesDefeatedToBegin => m_waitUntilAllEnemiesDefeatedToBegin;
        //public STAGE_TYPE StageType => STAGE_TYPE.STANDARD;
        //public float SpawningObstacleMultiplier => 1f;
        public List<StageEnemyData> StageEnemyData => m_stageEnemyData;

        public List<StageObstacleData> StageObstacleData => m_stageObstacleData;
        //public float CenterChannelWidth => 0.5f;

        //public List<StageColumnGroupObstacleData> StageColumnGroupObstacleData => m_stageColumnGroupObstacleData;
    }
}
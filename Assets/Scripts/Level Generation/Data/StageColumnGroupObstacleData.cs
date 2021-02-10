using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    [Serializable]
    public class StageColumnGroupObstacleData
    {
        [SerializeField, Required, Range(0.0f, 1.0f)]
        public float m_columnGroupMinimum;

        [SerializeField, Required, Range(0.0f, 1.0f)]
        public float m_columnGroupMaximum;

        [SerializeField]
        public bool m_isBlendZone;

        [SerializeField]
        public bool m_allowOverlap;
        [SerializeField]
        public bool m_forceSpawn;

        [SerializeField, HideIf("IsBlendZone")]
        public List<StageObstacleData> m_stageObstacleData;

        public float ColumnGroupMinimum => m_columnGroupMinimum;
        public float ColumnGroupMaximum => m_columnGroupMaximum;
        public bool IsBlendZone => m_isBlendZone;

        public List<StageObstacleData> StageObstacleData => m_stageObstacleData;
    }
}
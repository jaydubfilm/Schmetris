﻿using Sirenix.OdinInspector;
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
        private float m_columnGroupMinimum;

        [SerializeField, Required, Range(0.0f, 1.0f)]
        private float m_columnGroupMaximum;

        [SerializeField]
        private List<StageObstacleData> m_stageObstacleData;

        public float ColumnGroupMinimum => m_columnGroupMinimum;
        public float ColumnGroupMaximum => m_columnGroupMaximum;

        public List<StageObstacleData> StageObstacleData => m_stageObstacleData;
    }
}
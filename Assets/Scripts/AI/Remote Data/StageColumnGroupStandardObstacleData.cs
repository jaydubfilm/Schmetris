using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    [Serializable]
    public class StageColumnGroupStandardObstacleData
    {
        [SerializeField]
        private List<StageObstacleData> m_centerChannelObstacleData;
    }
}
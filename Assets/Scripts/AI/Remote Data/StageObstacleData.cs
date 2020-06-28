using System;
using Boo.Lang;
using Sirenix.OdinInspector;
using StarSalvager.Constants;
using UnityEngine;

namespace StarSalvager.AI
{
    [Serializable]
    public class StageObstacleData
    {
        [SerializeField, FoldoutGroup("$m_bitType")]
        private BIT_TYPE m_bitType;
        [SerializeField, FoldoutGroup("$m_bitType")]
        private ASTEROID_SIZE m_asteroidSize;
        [SerializeField, FoldoutGroup("$m_bitType")]
        private int m_asteroidCountPerMinute;

        public BIT_TYPE BitType => m_bitType;
        public ASTEROID_SIZE AsteroidSize => m_asteroidSize;
        public int AsteroidCountPerMinute => m_asteroidCountPerMinute;

        public float AsteroidPerRowAverage => (m_asteroidCountPerMinute / 60.0f) * Values.timeForAsteroidsToFall;
    }
}
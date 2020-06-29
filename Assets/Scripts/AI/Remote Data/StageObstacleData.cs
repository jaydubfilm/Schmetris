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
        [SerializeField, FoldoutGroup("$m_selectionType")]
        private SELECTION_TYPE m_selectionType;
        [SerializeField, FoldoutGroup("$m_selectionType"), ShowIf("m_selectionType", SELECTION_TYPE.BITTYPE)]
        private BIT_TYPE m_bitType;
        [SerializeField, FoldoutGroup("$m_selectionType")]
        private ASTEROID_SIZE m_asteroidSize;
        [SerializeField, FoldoutGroup("$m_selectionType")]
        private int m_asteroidCountPerMinute;

        public SELECTION_TYPE SelectionType => m_selectionType;
        public BIT_TYPE BitType => GetBitType();
        private BIT_TYPE GetBitType()
        {
            if (m_selectionType == SELECTION_TYPE.BITTYPE)
                return m_bitType;
            else
                return (BIT_TYPE)UnityEngine.Random.Range(0, 7);
        }
        public ASTEROID_SIZE AsteroidSize => m_asteroidSize;
        public int AsteroidCountPerMinute => m_asteroidCountPerMinute;

        public float AsteroidPerRowAverage => (m_asteroidCountPerMinute / 60.0f) * Values.timeForAsteroidsToFall;
    }
}
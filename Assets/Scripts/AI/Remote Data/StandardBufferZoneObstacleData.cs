using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    [CreateAssetMenu(fileName = "Buffer Data", menuName = "Star Salvager/Scriptable Objects/Buffer Data")]
    public class StandardBufferZoneObstacleData : ScriptableObject
    {
        [SerializeField, Range(0.0f, 1.0f)]
        private float m_portionOfEdgesUsedForBlend = 0.5f;
        [SerializeField]
        private List<StageObstacleData> m_bufferObstacleData;
        [SerializeField]
        private List<StageObstacleData> m_wallObstacleData;

        public float PortionOfEdgesUsedForBlend => m_portionOfEdgesUsedForBlend;
        public List<StageObstacleData> BufferObstacleData => m_bufferObstacleData;
        public List<StageObstacleData> WallObstacleData => m_wallObstacleData;

        [NonSerialized]
        public Vector2 WallBufferLeft = new Vector2(0, 0.02f);
        [NonSerialized]
        public Vector2 WallBufferRight = new Vector2(0.98f, 1.0f);
    }
}
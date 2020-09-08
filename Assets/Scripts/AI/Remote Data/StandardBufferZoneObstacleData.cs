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
        private float m_centerColumnWidth = 0.0f;
        [NonSerialized]
        private Vector2 m_centerColumnFieldRange;
        [NonSerialized]
        private Vector2 m_wallFieldLeft = new Vector2(0, 0.02f);
        [NonSerialized]
        private Vector2 m_wallFieldRight = new Vector2(0.98f, 1.0f);
        [NonSerialized]
        private Vector2 m_bufferFieldLeft;
        [NonSerialized]
        private Vector2 m_bufferFieldRight;
        [NonSerialized]
        private Vector2 m_blendFieldLeft;
        [NonSerialized]
        private Vector2 m_blendFieldRight;
        [NonSerialized]
        private Vector2 m_wallBlendFieldLeft;
        [NonSerialized]
        private Vector2 m_wallBlendFieldRight;

        /*public Vector2 CenterColumnFieldRange => m_centerColumnFieldRange;
        public Vector2 WallFieldLeft => m_wallFieldLeft;
        public Vector2 WallFieldRight => m_wallFieldRight;
        public Vector2 BufferFieldLeft => m_bufferFieldLeft;
        public Vector2 BufferFieldRight => m_bufferFieldRight;
        public Vector2 BlendFieldLeft => m_blendFieldLeft;
        public Vector2 BlendFieldRight => m_blendFieldRight;
        public Vector2 WallBlendFieldLeft => m_wallBlendFieldLeft;
        public Vector2 WallBlendFieldRight => m_wallBlendFieldRight;*/

        public void SetObstacleDataSpawns(StageRemoteData stageRemoteData, bool isPrevious, ObstacleManager obstacleManager)
        {
            if (stageRemoteData.CenterChannelWidth != m_centerColumnWidth)
            {
                m_centerColumnWidth = stageRemoteData.CenterChannelWidth;
                m_centerColumnFieldRange = new Vector2(0.5f - m_centerColumnWidth / 2, 0.5f + m_centerColumnWidth / 2);
                float sidesWidth = (1 - m_centerColumnWidth) / 2;
                float sidesBlend = sidesWidth * LevelManager.Instance.StandardBufferZoneObstacleData.PortionOfEdgesUsedForBlend;
                m_bufferFieldLeft = new Vector2(sidesBlend / 2, sidesWidth - (sidesBlend / 2));
                m_bufferFieldRight = new Vector2(sidesWidth + m_centerColumnWidth + (sidesBlend / 2), 1 - (sidesBlend / 2));
                m_blendFieldLeft = new Vector2(m_bufferFieldLeft.y, m_centerColumnFieldRange.x);
                m_blendFieldRight = new Vector2(m_centerColumnFieldRange.y, m_bufferFieldRight.x);
                m_wallBlendFieldLeft = new Vector2(m_wallFieldLeft.y, m_bufferFieldLeft.x);
                m_wallBlendFieldRight = new Vector2(m_bufferFieldRight.y, m_wallFieldRight.x);
                Debug.Log("IMOO");
                Debug.Log(m_bufferFieldLeft.x + " / " + m_bufferFieldLeft.y + " -- BufferFieldLeft");
                Debug.Log(m_bufferFieldRight.x + " / " + m_bufferFieldRight.y + " -- BufferFieldRight");
                Debug.Log(m_blendFieldLeft.x + " / " + m_blendFieldLeft.y + " -- BlendFieldLeft");
                Debug.Log(m_blendFieldRight.x + " / " + m_blendFieldRight.y + " -- BlendFieldRight");
                Debug.Log(m_wallBlendFieldLeft.x + " / " + m_wallBlendFieldLeft.y + " -- WallBlendFieldLeft");
                Debug.Log(m_wallBlendFieldRight.x + " / " + m_wallBlendFieldRight.y + " -- WallBlendFieldRight");
            }

            obstacleManager.SpawnObstacleData(stageRemoteData.StageObstacleData, m_centerColumnFieldRange, stageRemoteData.SpawningObstacleMultiplier, isPrevious);

            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_bufferFieldLeft, stageRemoteData.SpawningObstacleMultiplier, isPrevious);
            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_bufferFieldRight, stageRemoteData.SpawningObstacleMultiplier, isPrevious);
            obstacleManager.SpawnObstacleData(m_wallObstacleData, m_wallFieldLeft, 1, isPrevious);
            obstacleManager.SpawnObstacleData(m_wallObstacleData, m_wallFieldRight, 1, isPrevious);

            obstacleManager.SpawnObstacleData(m_wallObstacleData, m_wallBlendFieldLeft, 0.5f, isPrevious);
            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_wallBlendFieldLeft, stageRemoteData.SpawningObstacleMultiplier / 2, isPrevious);
            obstacleManager.SpawnObstacleData(m_wallObstacleData, m_wallBlendFieldRight, 0.5f, isPrevious);
            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_wallBlendFieldRight, stageRemoteData.SpawningObstacleMultiplier / 2, isPrevious);
            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_blendFieldLeft, stageRemoteData.SpawningObstacleMultiplier / 2, isPrevious);
            obstacleManager.SpawnObstacleData(stageRemoteData.StageObstacleData, m_blendFieldLeft, stageRemoteData.SpawningObstacleMultiplier / 2, isPrevious);
            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_blendFieldRight, stageRemoteData.SpawningObstacleMultiplier / 2, isPrevious);
            obstacleManager.SpawnObstacleData(stageRemoteData.StageObstacleData, m_blendFieldRight, stageRemoteData.SpawningObstacleMultiplier / 2, isPrevious);
        }
    }
}
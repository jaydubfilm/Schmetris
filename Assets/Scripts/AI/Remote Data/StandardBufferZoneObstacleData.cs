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
        public Vector2 m_wallFieldLeft = new Vector2(0, 0.02f);
        [NonSerialized]
        public Vector2 m_wallFieldRight = new Vector2(0.98f, 1.0f);
        [NonSerialized]
        private Vector2 m_bufferFieldLeft;
        [NonSerialized]
        private Vector2 m_bufferFieldRight;
        [NonSerialized]
        private Vector2 m_blendFieldLeft;
        [NonSerialized]
        private Vector2 m_blendFieldRight;
        [NonSerialized]
        public Vector2 m_wallBlendFieldLeft;
        [NonSerialized]
        public Vector2 m_wallBlendFieldRight;

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
            }

            obstacleManager.SpawnResourcesData(stageRemoteData, m_centerColumnFieldRange, false, true, stageRemoteData.SpawningObstacleMultiplier, isPrevious);
            obstacleManager.SpawnObstacleData(stageRemoteData.StageObstacleData, m_centerColumnFieldRange, false, true, stageRemoteData.SpawningObstacleMultiplier, isPrevious);

            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_bufferFieldLeft, true, false, stageRemoteData.SpawningObstacleMultiplier, isPrevious);
            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_bufferFieldRight, true, false, stageRemoteData.SpawningObstacleMultiplier, isPrevious);
            obstacleManager.SpawnObstacleData(m_wallObstacleData, m_wallFieldLeft, true, false, 1, isPrevious);
            obstacleManager.SpawnObstacleData(m_wallObstacleData, m_wallFieldRight, true, false, 1, isPrevious);

            obstacleManager.SpawnObstacleData(m_wallObstacleData, m_wallBlendFieldLeft, true, false, 0.5f, isPrevious);
            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_wallBlendFieldLeft, true, false, stageRemoteData.SpawningObstacleMultiplier / 2, isPrevious);
            obstacleManager.SpawnObstacleData(m_wallObstacleData, m_wallBlendFieldRight, true, false, 0.5f, isPrevious);
            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_wallBlendFieldRight, true, false, stageRemoteData.SpawningObstacleMultiplier / 2, isPrevious);
            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_blendFieldLeft, false, false, stageRemoteData.SpawningObstacleMultiplier / 2, isPrevious);
            obstacleManager.SpawnObstacleData(stageRemoteData.StageObstacleData, m_blendFieldLeft, false, false, stageRemoteData.SpawningObstacleMultiplier / 2, isPrevious);
            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_blendFieldRight, false, false, stageRemoteData.SpawningObstacleMultiplier / 2, isPrevious);
            obstacleManager.SpawnObstacleData(stageRemoteData.StageObstacleData, m_blendFieldRight, false, false, stageRemoteData.SpawningObstacleMultiplier / 2, isPrevious);
        }

        public void PrespawnWalls(StageRemoteData stageRemoteData, bool isPrevious, ObstacleManager obstacleManager)
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
            }

            for (int i = 0; i < StarSalvager.Values.Globals.GridSizeY; i++)
            {
                obstacleManager.SpawnObstacleData(m_wallObstacleData, m_wallFieldLeft, true, false, 1, isPrevious, true);
                obstacleManager.SpawnObstacleData(m_wallObstacleData, m_wallFieldRight, true, false, 1, isPrevious, true);
            }
        }
    }
}
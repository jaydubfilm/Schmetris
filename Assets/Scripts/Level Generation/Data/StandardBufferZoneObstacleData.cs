using System;
using System.Collections;
using System.Collections.Generic;
using StarSalvager.Values;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarSalvager.AI
{
    [CreateAssetMenu(fileName = "Buffer Data", menuName = "Star Salvager/Scriptable Objects/Buffer Data")]
    public class StandardBufferZoneObstacleData : ScriptableObject
    {
        private const float CenterWidth = 0.5f;
        private const float Multiplier = 1f;

        [SerializeField, Range(0.0f, 1.0f)]
        private float m_portionOfEdgesUsedForBlend = 0.5f;
        [SerializeField]
        private List<StageObstacleData> m_bufferObstacleData;
        [SerializeField]
        private List<StageObstacleData> m_wallObstacleData;

        public float PortionOfEdgesUsedForBlend => m_portionOfEdgesUsedForBlend;
        public List<StageObstacleData> BufferObstacleData => m_bufferObstacleData;
        public List<StageObstacleData> WallObstacleData => m_wallObstacleData;

        [NonSerialized] private float m_centerColumnWidth = 0.0f;
        [NonSerialized] private Vector2 m_centerColumnFieldRange;
        [NonSerialized] public Vector2 m_wallFieldLeft = new Vector2(0, 0.02f);
        [NonSerialized] public Vector2 m_wallFieldRight = new Vector2(0.98f, 1.0f);
        [NonSerialized] private Vector2 m_bufferFieldLeft;
        [NonSerialized] private Vector2 m_bufferFieldRight;
        [NonSerialized] private Vector2 m_blendFieldLeft;
        [NonSerialized] private Vector2 m_blendFieldRight;
        [NonSerialized] public Vector2 m_wallBlendFieldLeft;
        [NonSerialized] public Vector2 m_wallBlendFieldRight;

        /*public Vector2 CenterColumnFieldRange => m_centerColumnFieldRange;
        public Vector2 WallFieldLeft => m_wallFieldLeft;
        public Vector2 WallFieldRight => m_wallFieldRight;
        public Vector2 BufferFieldLeft => m_bufferFieldLeft;
        public Vector2 BufferFieldRight => m_bufferFieldRight;
        public Vector2 BlendFieldLeft => m_blendFieldLeft;
        public Vector2 BlendFieldRight => m_blendFieldRight;
        public Vector2 WallBlendFieldLeft => m_wallBlendFieldLeft;
        public Vector2 WallBlendFieldRight => m_wallBlendFieldRight;*/

        private void CheckCenterChannelDimensions(in StageRemoteData stageRemoteData)
        {
            if (CenterWidth == m_centerColumnWidth) 
                return;
            
            m_centerColumnWidth = CenterWidth;
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

        public void SetObstacleDataSpawns(
            StageRemoteData stageRemoteData,
            bool isPrevious,
            ObstacleManager obstacleManager,
            int yLevel = -1)
        {
            CheckCenterChannelDimensions(stageRemoteData);


            obstacleManager.SpawnObstacleData(stageRemoteData.StageObstacleData, m_centerColumnFieldRange, false, true, Multiplier, isPrevious, yLevel);
            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_bufferFieldLeft, true, false, Multiplier, isPrevious, yLevel);
            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_bufferFieldRight, true, false, Multiplier, isPrevious, yLevel);
            obstacleManager.SpawnObstacleData(m_wallObstacleData, m_wallFieldLeft, true, false, 1, isPrevious, yLevel);
            obstacleManager.SpawnObstacleData(m_wallObstacleData, m_wallFieldRight, true, false, 1, isPrevious, yLevel);
            obstacleManager.SpawnObstacleData(m_wallObstacleData, m_wallBlendFieldLeft, true, false, 0.5f, isPrevious, yLevel);
            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_wallBlendFieldLeft, true, false, Multiplier / 2, isPrevious, yLevel);
            obstacleManager.SpawnObstacleData(m_wallObstacleData, m_wallBlendFieldRight, true, false, 0.5f, isPrevious, yLevel);
            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_wallBlendFieldRight, true, false, Multiplier / 2, isPrevious, yLevel);
            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_blendFieldLeft, false, false, Multiplier / 2, isPrevious, yLevel);
            obstacleManager.SpawnObstacleData(stageRemoteData.StageObstacleData, m_blendFieldLeft, false, false, Multiplier / 2, isPrevious, yLevel);
            obstacleManager.SpawnObstacleData(m_bufferObstacleData, m_blendFieldRight, false, false, Multiplier / 2, isPrevious, yLevel);
            obstacleManager.SpawnObstacleData(stageRemoteData.StageObstacleData, m_blendFieldRight, false, false, Multiplier / 2, isPrevious, yLevel);
        }

        public void PrespawnWalls(StageRemoteData stageRemoteData, bool isPrevious, ObstacleManager obstacleManager)
        {
            if (CenterWidth != m_centerColumnWidth)
            {
                m_centerColumnWidth = CenterWidth;
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
        }

        public void PrespawnRows(StageRemoteData stageRemoteData, bool isPrevious, ObstacleManager obstacleManager)
        {
            CheckCenterChannelDimensions(stageRemoteData);

            //TODO Need to finish getting this setup
            for (int i = 1; i <= Globals.PreSpawnedRows; i++)
            {
                var yLevel = Globals.GridSizeY - (1 + i);
                SetObstacleDataSpawns(stageRemoteData, isPrevious, obstacleManager, yLevel);
            }

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class WorldGrid
    {
        private int m_width;
        private int m_height;
        private float m_gridCellSize;
        private int[,] m_gridArray;
        private Vector2 m_anchorPoint;

        public WorldGrid(int width, int height, float gridCellSize)
        {
            m_width = width;
            m_height = height;
            m_gridCellSize = gridCellSize;
            m_anchorPoint = new Vector2(0, 0);

            m_gridArray = new int[m_width, m_height];

            for (int x = 0; x < m_gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < m_gridArray.GetLength(1); y++)
                {
                    Debug.DrawLine(m_anchorPoint + new Vector2(x, y) * m_gridCellSize, m_anchorPoint + new Vector2(x, y + 1) * m_gridCellSize, new Color(255, 0, 0), 300f);
                    Debug.DrawLine(m_anchorPoint + new Vector2(x, y) * m_gridCellSize, m_anchorPoint + new Vector2(x + 1, y) * m_gridCellSize, new Color(255, 0, 0), 300f);
                }
            }
            Debug.DrawLine(m_anchorPoint + new Vector2(0, height) * m_gridCellSize, m_anchorPoint + new Vector2(width, height) * m_gridCellSize, new Color(255, 0, 0), 300f);
            Debug.DrawLine(m_anchorPoint + new Vector2(width, 0) * m_gridCellSize, m_anchorPoint + new Vector2(width, height) * m_gridCellSize, new Color(255, 0, 0), 300f);
        }

        public Vector2 GetRandomGridWorldPosition()
        {
            return GetCenterOfGridPointInWorldPosition(Random.Range(0, m_width), Random.Range(0, m_height));
        }

        private Vector2 GetCenterOfGridPointInWorldPosition (int x, int y)
        {
            return m_anchorPoint + new Vector2(x + 0.5f, y + 0.5f) * m_gridCellSize;
        }

        public int CheckGridDistanceBetweenPoints(Vector2 pointA, Vector2 pointB)
        {
            Vector2Int gridPositionPointA = GetGridPositionOfVector(pointA);
            Vector2Int gridPositionPointB = GetGridPositionOfVector(pointB);
            
            return Mathf.Abs(gridPositionPointA.x - gridPositionPointB.x) + Mathf.Abs(gridPositionPointA.y - gridPositionPointB.y);
        }

        private Vector2Int GetGridPositionOfVector(Vector2 worldLocation)
        {
            return new Vector2Int((int) ((worldLocation.x - m_anchorPoint.x) / m_gridCellSize), (int) ((worldLocation.y - m_anchorPoint.y) / m_gridCellSize));
        }
    }
}
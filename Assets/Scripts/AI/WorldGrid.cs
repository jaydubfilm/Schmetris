using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace StarSalvager
{
    public class WorldGrid
    {
        public int m_width { get; private set; }
        public int m_height { get; private set; }
        private float m_gridCellSize;
        private GridSquare[] m_gridArray;
        private Vector2 m_anchorPoint;
        private Vector2 m_getMiddleOfGridPoint = new Vector2(0.5f, 0.5f);

        public WorldGrid(int width, int height, float gridCellSize)
        {
            m_width = width;
            m_height = height;
            m_gridCellSize = gridCellSize;
            m_anchorPoint = Vector2.zero;

            m_gridArray = new GridSquare[m_width * m_height];

            for (int i = 0; i < m_gridArray.Length; i++)
            {
                m_gridArray[i] = new GridSquare();
            }


#if UNITY_EDITOR
            //Draw debug lines to show the area of the grid
            for (int x = 0; x < m_width; x++)
            {
                for (int y = 0; y < m_height; y++)
                {
                    Vector2 tempVector = new Vector2(x, y);
                    
                    Debug.DrawLine(m_anchorPoint + tempVector * m_gridCellSize, m_anchorPoint + new Vector2(x, y + 1) * m_gridCellSize, new Color(255, 0, 0), 300f);
                    Debug.DrawLine(m_anchorPoint + tempVector * m_gridCellSize, m_anchorPoint + new Vector2(x + 1, y) * m_gridCellSize, new Color(255, 0, 0), 300f);
                }
            }
            Debug.DrawLine(m_anchorPoint + new Vector2(0, height) * m_gridCellSize, m_anchorPoint + new Vector2(width, height) * m_gridCellSize, new Color(255, 0, 0), 300f);
            Debug.DrawLine(m_anchorPoint + new Vector2(width, 0) * m_gridCellSize, m_anchorPoint + new Vector2(width, height) * m_gridCellSize, new Color(255, 0, 0), 300f);
#endif
        }

        public void AddObstacleToGridSquare(GameObject obstacle)
        {
            GetGridSquareAtWorldPosition(obstacle.transform.position).AddObstacleToSquare(obstacle);
        }

        public void RemoveObstacleFromGridSquare(GameObject obstacle)
        {
            GetGridSquareAtWorldPosition(obstacle.transform.position).RemoveObstacleFromSquare(obstacle);
        }

        public GridSquare GetGridSquareAtPosition(Vector2Int gridPosition)
        {
            return m_gridArray[gridPosition.x + (gridPosition.y * m_width)];
        }

        public GridSquare GetGridSquareAtPosition(int x, int y)
        {
            return m_gridArray[x + (y * m_width)];
        }

        private Vector2 GetCenterOfGridPointInWorldPosition(Vector2Int worldPositionVector)
        {
            return m_anchorPoint + ((new Vector2(worldPositionVector.x, worldPositionVector.y) + m_getMiddleOfGridPoint) * m_gridCellSize);
        }

        private Vector2 GetCenterOfGridPointInWorldPosition(int x, int y)
        {
            return m_anchorPoint + ((new Vector2(x, y) + m_getMiddleOfGridPoint) * m_gridCellSize);
        }

        private GridSquare GetGridSquareAtWorldPosition(Vector2 worldPosition)
        {
            return GetGridSquareAtPosition(GetGridPositionOfVector(worldPosition));
        }

        public Vector2 GetRandomGridWorldPosition()
        {
            return GetCenterOfGridPointInWorldPosition(UnityEngine.Random.Range(0, m_width), UnityEngine.Random.Range(0, m_height));
        }

        public Vector2Int GetGridPositionOfVector(Vector2 worldLocation)
        {
            return new Vector2Int((int) ((worldLocation.x - m_anchorPoint.x) / m_gridCellSize), (int) ((worldLocation.y - m_anchorPoint.y) / m_gridCellSize));
        }
    }
}
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
        private float m_gridSquareSize;
        private GridSquare[] m_gridArray;
        private Vector2 m_anchorPoint;
        private Vector2 m_adjustToMiddleOfGridSquare = new Vector2(0.5f, 0.5f);

        public WorldGrid(int width, int height, float gridSquareSize)
        {
            m_width = width;
            m_height = height;
            m_gridSquareSize = gridSquareSize;
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
                    
                    Debug.DrawLine(m_anchorPoint + tempVector * m_gridSquareSize, m_anchorPoint + new Vector2(x, y + 1) * m_gridSquareSize, new Color(255, 0, 0), 300f);
                    Debug.DrawLine(m_anchorPoint + tempVector * m_gridSquareSize, m_anchorPoint + new Vector2(x + 1, y) * m_gridSquareSize, new Color(255, 0, 0), 300f);
                }
            }
            Debug.DrawLine(m_anchorPoint + new Vector2(0, height) * m_gridSquareSize, m_anchorPoint + new Vector2(width, height) * m_gridSquareSize, new Color(255, 0, 0), 300f);
            Debug.DrawLine(m_anchorPoint + new Vector2(width, 0) * m_gridSquareSize, m_anchorPoint + new Vector2(width, height) * m_gridSquareSize, new Color(255, 0, 0), 300f);
#endif
        }

        public void MoveObstacleMarkersDownwardOnGrid()
        {
            for (int y = 0; y < m_height; y++)
            {
                for (int x = 0; x < m_width; x++)
                {
                    if (y + 1 == m_height)
                    {
                        SetObstacleInGridSquare(x, y, false);
                    }
                    else
                    {
                        SetObstacleInGridSquare(x, y, GetGridSquareAtPosition(x, y + 1).m_obstacleInSquare);
                    }
                }
            }
        }

        public void SetObstacleInGridSquare(Vector2 obstaclePosition, bool occupied)
        {
            GetGridSquareAtWorldPosition(obstaclePosition).SetObstacleInSquare(occupied);
        }

        public void SetObstacleInGridSquare(int x, int y, bool occupied)
        {
            GetGridSquareAtPosition(x, y).SetObstacleInSquare(occupied);
        }

        public GridSquare GetGridSquareAtPosition(Vector2Int gridPosition)
        {
            return m_gridArray[gridPosition.x + (gridPosition.y * m_width)];
        }

        public GridSquare GetGridSquareAtPosition(int x, int y)
        {
            return m_gridArray[x + (y * m_width)];
        }

        public Vector2 GetCenterOfGridSquareInGridPosition(Vector2Int gridPosition)
        {
            return m_anchorPoint + ((new Vector2(gridPosition.x, gridPosition.y) + m_adjustToMiddleOfGridSquare) * m_gridSquareSize);
        }

        public Vector2 GetCenterOfGridSquareInGridPosition(int x, int y)
        {
            return m_anchorPoint + ((new Vector2(x, y) + m_adjustToMiddleOfGridSquare) * m_gridSquareSize);
        }

        private Vector2 GetCenterOfGridSquareInWorldPosition(Vector2Int worldPositionVector)
        {
            return m_anchorPoint + ((new Vector2(worldPositionVector.x, worldPositionVector.y) + m_adjustToMiddleOfGridSquare) * m_gridSquareSize);
        }

        private GridSquare GetGridSquareAtWorldPosition(Vector2 worldPosition)
        {
            return GetGridSquareAtPosition(GetGridPositionOfVector(worldPosition));
        }

        public Vector2 GetRandomGridSquareWorldPosition()
        {
            return GetCenterOfGridSquareInGridPosition(UnityEngine.Random.Range(0, m_width), UnityEngine.Random.Range(0, m_height));
        }

        public Vector2Int GetGridPositionOfVector(Vector2 worldLocation)
        {
            return new Vector2Int((int) ((worldLocation.x - m_anchorPoint.x) / m_gridSquareSize), (int) ((worldLocation.y - m_anchorPoint.y) / m_gridSquareSize));
        }
    }
}
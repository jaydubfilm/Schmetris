using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using StarSalvager.Constants;

namespace StarSalvager
{
    public class WorldGrid
    {
        private GridSquare[] m_gridArray;
        private Vector2 m_anchorPoint;
        private Vector2 m_adjustToMiddleOfGridSquare = new Vector2(0.5f, 0.5f);

        public WorldGrid()
        {
            m_anchorPoint = Vector2.left * 
                ((Values.gridSizeX / 2) * Values.gridCellSize - 
                ((Camera.main.orthographicSize * Screen.width / Screen.height)));

            m_gridArray = new GridSquare[Values.gridSizeX * Values.gridSizeY];

            for (int i = 0; i < m_gridArray.Length; i++)
            {
                m_gridArray[i] = new GridSquare();
            }


#if UNITY_EDITOR
            //Draw debug lines to show the area of the grid
            /*for (int x = 0; x < Values.gridSizeX; x++)
            {
                for (int y = 0; y < Values.gridSizeY; y++)
                {
                    Vector2 tempVector = new Vector2(x, y);
                    
                    Debug.DrawLine(m_anchorPoint + tempVector * Values.gridCellSize, m_anchorPoint + new Vector2(x, y + 1) * Values.gridCellSize, new Color(255, 0, 0), 300f);
                    Debug.DrawLine(m_anchorPoint + tempVector * Values.gridCellSize, m_anchorPoint + new Vector2(x + 1, y) * Values.gridCellSize, new Color(255, 0, 0), 300f);
                }
            }
            Debug.DrawLine(m_anchorPoint + new Vector2(0, Values.gridSizeY) * Values.gridCellSize, m_anchorPoint + new Vector2(Values.gridSizeX, Values.gridSizeY) * Values.gridCellSize, new Color(255, 0, 0), 300f);
            Debug.DrawLine(m_anchorPoint + new Vector2(Values.gridSizeX, 0) * Values.gridCellSize, m_anchorPoint + new Vector2(Values.gridSizeX, Values.gridSizeY) * Values.gridCellSize, new Color(255, 0, 0), 300f);*/
#endif
        }

        public void MoveObstacleMarkersDownwardOnGrid()
        {
            for (int y = 0; y < Values.gridSizeY; y++)
            {
                for (int x = 0; x < Values.gridSizeX; x++)
                {
                    if (y + 1 == Values.gridSizeY)
                    {
                        SetObstacleInGridSquare(x, y, GetGridSquareAtPosition(x, 0).m_obstacleInSquare);
                    }
                    else
                    {
                        SetObstacleInGridSquare(x, y, GetGridSquareAtPosition(x, y + 1).m_obstacleInSquare);
                    }
                }
            }
        }

        public void MoveObstacleMarkersLeftOnGrid(int amount)
        {
            for (int x = 0; x < Values.gridSizeX; x++)
            {
                for (int y = 0; y < Values.gridSizeY; y++)
                {
                    if (x + amount >= Values.gridSizeX)
                    {
                        SetObstacleInGridSquare(x, y, GetGridSquareAtPosition(x + amount - Values.gridSizeX, y).m_obstacleInSquare);
                    }
                    else
                    {
                        SetObstacleInGridSquare(x, y, GetGridSquareAtPosition(x + amount, y).m_obstacleInSquare);
                    }
                }
            }
        }

        public void MoveObstacleMarkersRightOnGrid(int amount)
        {
            for (int x = Values.gridSizeX - 1; x >= 0; x--)
            {
                for (int y = 0; y < Values.gridSizeY; y++)
                {
                    if (x - amount < 0)
                    {
                        SetObstacleInGridSquare(x, y, GetGridSquareAtPosition(x - amount + Values.gridSizeX, y).m_obstacleInSquare);
                    }
                    else
                    {
                        SetObstacleInGridSquare(x, y, GetGridSquareAtPosition(x - amount, y).m_obstacleInSquare);
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
            return m_gridArray[gridPosition.x + (gridPosition.y * Values.gridSizeX)];
        }

        public GridSquare GetGridSquareAtPosition(int x, int y)
        {
            return m_gridArray[x + (y * Values.gridSizeX)];
        }

        public Vector2 GetCenterOfGridSquareInGridPosition(Vector2Int gridPosition)
        {
            return m_anchorPoint + ((new Vector2(gridPosition.x, gridPosition.y) + m_adjustToMiddleOfGridSquare) * Values.gridCellSize);
        }

        public Vector2 GetCenterOfGridSquareInGridPosition(int x, int y)
        {
            return m_anchorPoint + ((new Vector2(x, y) + m_adjustToMiddleOfGridSquare) * Values.gridCellSize);
        }

        private Vector2 GetCenterOfGridSquareInWorldPosition(Vector2Int worldPositionVector)
        {
            return m_anchorPoint + ((new Vector2(worldPositionVector.x, worldPositionVector.y) + m_adjustToMiddleOfGridSquare) * Values.gridCellSize);
        }

        public GridSquare GetGridSquareAtWorldPosition(Vector2 worldPosition)
        {
            return GetGridSquareAtPosition(GetGridPositionOfVector(worldPosition));
        }

        public Vector2 GetRandomGridSquareWorldPosition()
        {
            return GetCenterOfGridSquareInGridPosition(UnityEngine.Random.Range(0, Values.gridSizeX), UnityEngine.Random.Range(0, Values.gridSizeY));
        }

        public Vector2 GetRandomTopGridSquareWorldPosition()
        {
            return GetCenterOfGridSquareInGridPosition(UnityEngine.Random.Range(0, Values.gridSizeX), Values.gridSizeY - 1);
        }

        public Vector2Int GetGridPositionOfVector(Vector2 worldLocation)
        {
            return new Vector2Int((int)Mathf.Floor((worldLocation.x - m_anchorPoint.x) / Values.gridCellSize), (int)Mathf.Floor((worldLocation.y - m_anchorPoint.y) / Values.gridCellSize));
        }
    }
}
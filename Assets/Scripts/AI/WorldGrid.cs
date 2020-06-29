using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using StarSalvager.Constants;
using StarSalvager.AI;
using UnityEngine.InputSystem.Interactions;

namespace StarSalvager
{
    public class WorldGrid
    {
        private GridSquare[] m_gridArray;
        private Vector2 m_anchorPoint;
        private Vector2 m_adjustToMiddleOfGridSquare = new Vector2(0.5f, 0.5f);
        private Vector2Int m_screenGridCellRange;
        private Vector2Int m_botGridPosition;

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

            float height = Camera.main.orthographicSize * 2.0f;
            float width = height * Screen.width / Screen.height;
            m_screenGridCellRange = new Vector2Int((int)(width / Values.gridCellSize), (int)(height / Values.gridCellSize));
            m_botGridPosition = GetGridPositionOfVector(LevelManager.Instance.BotGameObject.transform.position);

#if UNITY_EDITOR
            //Draw debug lines to show the area of the grid
            for (int x = 0; x < Values.gridSizeX; x++)
            {
                for (int y = 0; y < Values.gridSizeY; y++)
                {
                    Vector2 tempVector = new Vector2(x, y);
                    
                    Debug.DrawLine(m_anchorPoint + tempVector * Values.gridCellSize, m_anchorPoint + new Vector2(x, y + 1) * Values.gridCellSize, new Color(255, 0, 0), 300f);
                    Debug.DrawLine(m_anchorPoint + tempVector * Values.gridCellSize, m_anchorPoint + new Vector2(x + 1, y) * Values.gridCellSize, new Color(255, 0, 0), 300f);
                }
            }
            Debug.DrawLine(m_anchorPoint + new Vector2(0, Values.gridSizeY) * Values.gridCellSize, m_anchorPoint + new Vector2(Values.gridSizeX, Values.gridSizeY) * Values.gridCellSize, new Color(255, 0, 0), 300f);
            Debug.DrawLine(m_anchorPoint + new Vector2(Values.gridSizeX, 0) * Values.gridCellSize, m_anchorPoint + new Vector2(Values.gridSizeX, Values.gridSizeY) * Values.gridCellSize, new Color(255, 0, 0), 300f);
#endif
        }

        public void DrawDebugMarkedGridPoints()
        {
            for (int x = 0; x < Values.gridSizeX; x++)
            {
                for (int y = 0; y < Values.gridSizeY; y++)
                {
                    if (GetGridSquareAtPosition(x, y).m_obstacleInSquare == true)
                    {
                        Debug.DrawLine(GetCenterOfGridSquareInGridPosition(x, y), GetCenterOfGridSquareInGridPosition(x, y) + new Vector2(0, 0.25f), new Color(255, 0, 0), 300f);
                    }
                }
            }
            Debug.Break();
        }

        public void MoveObstacleMarkersDownwardOnGrid()
        {
            for (int y = 0; y < Values.gridSizeY; y++)
            {
                for (int x = 0; x < Values.gridSizeX; x++)
                {
                    if (y + 1 == Values.gridSizeY)
                    {
                        //SetObstacleInGridSquare(x, y, GetGridSquareAtPosition(x, 0).m_obstacleInSquare);
                        SetObstacleInGridSquare(x, y, false);
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

        public void SetObstacleInGridSquare(Vector2Int gridPosition, bool occupied)
        {
            GetGridSquareAtPosition(gridPosition.x, gridPosition.y).SetObstacleInSquare(occupied);
        }

        public void SetObstacleInGridSquare(int x, int y, bool occupied)
        {
            GetGridSquareAtPosition(x, y).SetObstacleInSquare(occupied);
        }

        public GridSquare GetGridSquareAtPosition(Vector2Int gridPosition)
        {
            if (gridPosition.x >= Values.gridSizeX)
                gridPosition.x -= Values.gridSizeX;
            else if (gridPosition.x < 0)
                gridPosition.x += Values.gridSizeX;

            return m_gridArray[gridPosition.x + (gridPosition.y * Values.gridSizeX)];
        }

        public GridSquare GetGridSquareAtPosition(int x, int y)
        {
            if (x >= Values.gridSizeX)
                x -= Values.gridSizeX;
            else if (x < 0)
                x += Values.gridSizeX;
            
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

        public GridSquare GetGridSquareAtWorldPosition(Vector2 worldPosition)
        {
            return GetGridSquareAtPosition(GetGridPositionOfVector(worldPosition));
        }

        public Vector2 GetRandomGridSquareWorldPosition()
        {
            return GetCenterOfGridSquareInGridPosition(UnityEngine.Random.Range(0, Values.gridSizeX), UnityEngine.Random.Range(0, Values.gridSizeY));
        }

        public Vector2 GetSpawnPositionForEnemy(ENEMY_MOVETYPE moveType)
        {
            if (moveType == ENEMY_MOVETYPE.Horizontal ||
                moveType == ENEMY_MOVETYPE.HorizontalDescend ||
                moveType == ENEMY_MOVETYPE.OscillateHorizontal)
            {
                //Spawn to the edges of the screen
                return GetHorizontalSpawnPositionForEnemy();
            }
            else
            {
                //Spawn above the screen
                return GetVerticalSpawnPositionForEnemy();
            }
        }

        //TODO: When the screen size and camera size and grid cell size systems all start working in a scaling fashion, this will need to adjust
        private Vector2 GetHorizontalSpawnPositionForEnemy()
        {
            return GetCenterOfGridSquareInGridPosition(
                m_botGridPosition.x + ((UnityEngine.Random.Range(0, 2) * 2 - 1) * UnityEngine.Random.Range(m_screenGridCellRange.x / 2, m_screenGridCellRange.x)), 
                UnityEngine.Random.Range(m_screenGridCellRange.y / 2, m_screenGridCellRange.y));
        }

        //TODO: When the screen size and camera size and grid cell size systems all start working in a scaling fashion, this will need to adjust
        private Vector2 GetVerticalSpawnPositionForEnemy()
        {
            return GetCenterOfGridSquareInGridPosition(
                m_botGridPosition.x + ((UnityEngine.Random.Range(0, 2) * 2 - 1) * UnityEngine.Random.Range(0, m_screenGridCellRange.x / 2)),
                UnityEngine.Random.Range(m_screenGridCellRange.y, Values.gridSizeY));
        }

        private Vector2 GetRandomTopGridSquareWorldPosition()
        {
            return GetCenterOfGridSquareInGridPosition(UnityEngine.Random.Range(0, Values.gridSizeX), Values.gridSizeY - 1);
        }

        private Vector2Int GetRandomTopGridSquareGridPosition()
        {
            return new Vector2Int(UnityEngine.Random.Range(0, Values.gridSizeX), Values.gridSizeY - 1);
        }

        public Vector2 GetAvailableRandomTopGridSquareWorldPosition()
        {
            int numTries = 20;
            for (int i = 0; i < numTries; i++)
            {
                Vector2Int randomTop = GetRandomTopGridSquareGridPosition();
                bool isFreeSpace = true;
                Vector2Int obstacleGridScanMinimum = new Vector2Int(
                    Math.Max(0, randomTop.x - Values.enemyGridScanRadius),
                    Math.Max(0, randomTop.y - Values.enemyGridScanRadius));
                Vector2Int obstacleGridScanMaximum = new Vector2Int(
                    Math.Min(Values.gridSizeX - 1, randomTop.x + Values.enemyGridScanRadius),
                    Math.Min(Values.gridSizeY - 1, randomTop.y + Values.enemyGridScanRadius));
                //Check each position in the box for whether an obstacle is there
                for (int j = obstacleGridScanMinimum.x; j <= obstacleGridScanMaximum.x; j++)
                {
                    for (int k = obstacleGridScanMinimum.y; k <= obstacleGridScanMaximum.y; k++)
                    {
                        if (LevelManager.Instance.WorldGrid.GetGridSquareAtPosition(j, k).m_obstacleInSquare)
                        {
                            isFreeSpace = false;
                            break;
                        }
                    }
                    if (!isFreeSpace)
                        break;
                }

                if (isFreeSpace)
                {
                    return GetCenterOfGridSquareInGridPosition(randomTop);
                }
            }

            return GetRandomTopGridSquareWorldPosition();
        }

        public Vector2Int GetGridPositionOfVector(Vector2 worldLocation)
        {
            return new Vector2Int((int)Mathf.Floor((worldLocation.x - m_anchorPoint.x) / Values.gridCellSize), (int)Mathf.Floor((worldLocation.y - m_anchorPoint.y) / Values.gridCellSize));
        }
    }
}
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using StarSalvager.Values;
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
        }

        public void SetupGrid()
        {
            m_anchorPoint = Vector2.left *
                ((Values.Globals.GridSizeX / 1.5f) * Constants.gridCellSize -
                ((Camera.main.orthographicSize * Screen.width / Screen.height)))
                + Vector2.down * Values.Constants.gridCellSize / 2;

            m_gridArray = new GridSquare[Values.Globals.GridSizeX * Values.Globals.GridSizeY];

            for (int i = 0; i < m_gridArray.Length; i++)
            {
                m_gridArray[i] = new GridSquare();
            }

            float height = Camera.main.orthographicSize * 2.0f;
            float width = height * Screen.width / Screen.height;
            m_screenGridCellRange = new Vector2Int((int)(width / Constants.gridCellSize), (int)(height / Constants.gridCellSize));
            m_botGridPosition = GetGridPositionOfVector(LevelManager.Instance.BotGameObject.transform.position);


        }

        /*public void DrawDebugMarkedGridPoints()
        {
            for (int x = 0; x < Values.Globals.GridSizeX; x++)
            {
                for (int y = 0; y < Values.Globals.GridSizeY; y++)
                {
                    if (GetGridSquareAtPosition(x, y).ObstacleInSquare == true)
                    {
                        Debug.DrawLine(GetCenterOfGridSquareInGridPosition(x, y), GetCenterOfGridSquareInGridPosition(x, y) + new Vector2(0, 0.25f), new Color(255, 0, 0), 300f);
                    }
                }
            }
            Debug.Break();
        }*/

        public void MoveObstacleMarkersDownwardOnGrid()
        {
            for (int y = 0; y < Values.Globals.GridSizeY; y++)
            {
                for (int x = 0; x < Values.Globals.GridSizeX; x++)
                {
                    if (y + 1 == Values.Globals.GridSizeY)
                    {
                        SetObstacleInGridSquare(x, y, 0, false);
                        
                    }
                    else
                    {
                        SetObstacleInGridSquare(x, y, GetGridSquareAtPosition(x, y + 1).RadiusMarkAround, GetGridSquareAtPosition(x, y + 1).ObstacleInSquare);
                    }
                }
            }

            for (int y = 0; y < Values.Globals.GridSizeY; y++)
            {
                for (int x = 0; x < Values.Globals.GridSizeX; x++)
                {
                    MarkObjectsAroundGridSquare(x, y, GetGridSquareAtPosition(x, y).RadiusMarkAround);
                }
            }
        }

        public void MoveObstacleMarkersLeftOnGrid(int amount)
        {
            for (int x = 0; x < Values.Globals.GridSizeX; x++)
            {
                for (int y = 0; y < Values.Globals.GridSizeY; y++)
                {
                    if (x + amount >= Values.Globals.GridSizeX)
                    {
                        SetObstacleInGridSquare(x, y, GetGridSquareAtPosition(x + amount - Values.Globals.GridSizeX, y).RadiusMarkAround, GetGridSquareAtPosition(x + amount - Values.Globals.GridSizeX, y).ObstacleInSquare);
                    }
                    else
                    {
                        SetObstacleInGridSquare(x, y, GetGridSquareAtPosition(x + amount, y).RadiusMarkAround, GetGridSquareAtPosition(x + amount, y).ObstacleInSquare);
                    }
                }
            }

            for (int y = 0; y < Values.Globals.GridSizeY; y++)
            {
                for (int x = 0; x < Values.Globals.GridSizeX; x++)
                {
                    //MarkObjectsAroundGridSquare(x, y, GetGridSquareAtPosition(x, y).RadiusMarkAround);
                }
            }
        }

        public void MoveObstacleMarkersRightOnGrid(int amount)
        {
            for (int x = Values.Globals.GridSizeX - 1; x >= 0; x--)
            {
                for (int y = 0; y < Values.Globals.GridSizeY; y++)
                {
                    if (x - amount < 0)
                    {
                        SetObstacleInGridSquare(x, y, GetGridSquareAtPosition(x - amount + Values.Globals.GridSizeX, y).RadiusMarkAround, GetGridSquareAtPosition(x - amount + Values.Globals.GridSizeX, y).ObstacleInSquare);
                    }
                    else
                    {
                        SetObstacleInGridSquare(x, y, GetGridSquareAtPosition(x - amount, y).RadiusMarkAround, GetGridSquareAtPosition(x - amount, y).ObstacleInSquare);
                    }
                }
            }

            for (int y = 0; y < Values.Globals.GridSizeY; y++)
            {
                for (int x = 0; x < Values.Globals.GridSizeX; x++)
                {
                    //MarkObjectsAroundGridSquare(x, y, GetGridSquareAtPosition(x, y).RadiusMarkAround);
                }
            }
        }

        public void SetObstacleInGridSquare(Vector2 obstaclePosition, int radius, bool occupied)
        {
            GetGridSquareAtWorldPosition(obstaclePosition).SetObstacleInSquare(occupied);
            GetGridSquareAtWorldPosition(obstaclePosition).SetRadiusMarkAround(radius);
        }

        public void SetObstacleInGridSquare(Vector2Int gridPosition, int radius, bool occupied)
        {
            GetGridSquareAtPosition(gridPosition.x, gridPosition.y).SetObstacleInSquare(occupied);
            GetGridSquareAtPosition(gridPosition.x, gridPosition.y).SetRadiusMarkAround(radius);
        }

        public void SetObstacleInGridSquare(int x, int y, int radius, bool occupied)
        {
            GetGridSquareAtPosition(x, y).SetObstacleInSquare(occupied);
            GetGridSquareAtPosition(x, y).SetRadiusMarkAround(radius);
        }

        public void MarkObjectsAroundGridSquare(int x, int y, int radiusAround)
        {
            if (radiusAround == 0)
                return;

            for (int i = Mathf.Max(0, x - radiusAround); i <= Mathf.Min(Globals.GridSizeX - 1, x + radiusAround); i++)
            {
                for (int k = Mathf.Max(0, y - radiusAround); k <= Mathf.Min(Globals.GridSizeY - 1, y + radiusAround); k++)
                {
                    GetGridSquareAtPosition(i, k).SetObstacleInSquare(true);
                }
            }
        }

        public GridSquare GetGridSquareAtPosition(Vector2Int gridPosition)
        {
            if (gridPosition.x >= Values.Globals.GridSizeX)
                gridPosition.x -= Values.Globals.GridSizeX;
            else if (gridPosition.x < 0)
                gridPosition.x += Values.Globals.GridSizeX;

            if (gridPosition.y < 0)
            {
                gridPosition.y = 0;
            }

            return m_gridArray[gridPosition.x + (gridPosition.y * Values.Globals.GridSizeX)];
        }

        public GridSquare GetGridSquareAtPosition(int x, int y)
        {
            if (x >= Values.Globals.GridSizeX)
                x -= Values.Globals.GridSizeX;
            else if (x < 0)
                x += Values.Globals.GridSizeX;
            
            return m_gridArray[x + (y * Values.Globals.GridSizeX)];
        }

        public Vector2 GetCenterOfGridSquareInGridPosition(Vector2Int gridPosition)
        {
            return m_anchorPoint + ((new Vector2(gridPosition.x, gridPosition.y) + m_adjustToMiddleOfGridSquare) * Constants.gridCellSize);
        }

        public Vector2 GetCenterOfGridSquareInGridPosition(int x, int y)
        {
            return m_anchorPoint + ((new Vector2(x, y) + m_adjustToMiddleOfGridSquare) * Constants.gridCellSize);
        }

        public GridSquare GetGridSquareAtWorldPosition(Vector2 worldPosition)
        {
            return GetGridSquareAtPosition(GetGridPositionOfVector(worldPosition));
        }

        public Vector2 GetRandomGridSquareWorldPosition()
        {
            return GetCenterOfGridSquareInGridPosition(UnityEngine.Random.Range(0, Values.Globals.GridSizeX), UnityEngine.Random.Range(0, Values.Globals.GridSizeY));
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
                UnityEngine.Random.Range(m_screenGridCellRange.y - 2, m_screenGridCellRange.y));
        }

        //TODO: When the screen size and camera size and grid cell size systems all start working in a scaling fashion, this will need to adjust
        private Vector2 GetVerticalSpawnPositionForEnemy()
        {
            return GetCenterOfGridSquareInGridPosition(
                m_botGridPosition.x + ((UnityEngine.Random.Range(0, 2) * 2 - 1) * UnityEngine.Random.Range(0, m_screenGridCellRange.x / 2)),
                UnityEngine.Random.Range(m_screenGridCellRange.y, Values.Globals.GridSizeY));
        }

        private Vector2 GetRandomTopGridSquareWorldPosition()
        {
            return GetCenterOfGridSquareInGridPosition(UnityEngine.Random.Range(0, Values.Globals.GridSizeX), Values.Globals.GridSizeY - 1);
        }

        private Vector2Int GetRandomTopGridSquareGridPosition()
        {
            return new Vector2Int(UnityEngine.Random.Range(0, Values.Globals.GridSizeX), Values.Globals.GridSizeY - 1);
        }

        public Vector2 GetAvailableRandomTopGridSquareWorldPosition(int scanRadius)
        {
            int numTries = 100;
            for (int i = 0; i < numTries; i++)
            {
                Vector2Int randomTop = GetRandomTopGridSquareGridPosition();
                bool isFreeSpace = true;
                Vector2Int obstacleGridScanMinimum = new Vector2Int(
                    Math.Max(0, randomTop.x - scanRadius),
                    Math.Max(0, randomTop.y - scanRadius));
                Vector2Int obstacleGridScanMaximum = new Vector2Int(
                    Math.Min(Values.Globals.GridSizeX - 1, randomTop.x + scanRadius),
                    Math.Min(Values.Globals.GridSizeY - 1, randomTop.y + scanRadius));
                //Check each position in the box for whether an obstacle is there
                for (int j = obstacleGridScanMinimum.x; j <= obstacleGridScanMaximum.x; j++)
                {
                    for (int k = obstacleGridScanMinimum.y; k <= obstacleGridScanMaximum.y; k++)
                    {
                        if (LevelManager.Instance.WorldGrid.GetGridSquareAtPosition(j, k).ObstacleInSquare)
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

            return GetAvailableRandomTopGridSquareWorldPosition(scanRadius - 1);
        }

        public Vector2Int GetGridPositionOfVector(Vector2 worldLocation)
        {
            return new Vector2Int((int)Mathf.Floor((worldLocation.x - m_anchorPoint.x) / Constants.gridCellSize), (int)Mathf.Floor((worldLocation.y - m_anchorPoint.y) / Constants.gridCellSize));
        }

        public Vector2Int[] SelectBitExplosionPositions(Vector2 startingLocation, int numBits, int verticalExplosionRange, int horizontalExplosionRange)
        {
            Vector2Int[] bitExplosionPositions = new Vector2Int[numBits];

            Vector2Int startingPoint = GetGridPositionOfVector(startingLocation);

            for (int i = 0; i < numBits; i++)
            {
                Vector2Int bitPosition = startingPoint +
                    (Vector2Int.up * UnityEngine.Random.Range(verticalExplosionRange / 2, verticalExplosionRange + 1)) +
                    (Vector2Int.left * UnityEngine.Random.Range(0, horizontalExplosionRange + 1) * (UnityEngine.Random.Range(0, 2) * 2 - 1));

                if (GetGridSquareAtPosition(bitPosition).ObstacleInSquare)
                {
                    continue;
                }
                bitExplosionPositions[i] = bitPosition;
            }

            return bitExplosionPositions;
        }
        
        
        #if UNITY_EDITOR

        public void OnDrawGizmos()
        {
            
            //Draw debug lines to show the area of the grid
            for (int x = 0; x < Globals.GridSizeX; x++)
            {
                for (int y = 0; y < Globals.GridSizeY; y++)
                {
                    Gizmos.color = Color.red;
                    Vector2 tempVector = new Vector2(x, y);

                    Gizmos.DrawLine(m_anchorPoint + tempVector * Constants.gridCellSize, m_anchorPoint + new Vector2(x, y + 1) * Constants.gridCellSize);
                    Debug.DrawLine(m_anchorPoint + tempVector * Constants.gridCellSize, m_anchorPoint + new Vector2(x + 1, y) * Constants.gridCellSize);
                }
            }
            Debug.DrawLine(m_anchorPoint + new Vector2(0, Globals.GridSizeY) * Constants.gridCellSize, m_anchorPoint + new Vector2(Globals.GridSizeX, Globals.GridSizeY) * Constants.gridCellSize);
            Debug.DrawLine(m_anchorPoint + new Vector2(Globals.GridSizeX, 0) * Constants.gridCellSize, m_anchorPoint + new Vector2(Globals.GridSizeX, Globals.GridSizeY) * Constants.gridCellSize);
        }
        
        #endif
        
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using StarSalvager.Values;
using StarSalvager.AI;
using UnityEngine.InputSystem.Interactions;
using StarSalvager.ScriptableObjects;
using StarSalvager.Cameras;

namespace StarSalvager
{
    public class WorldGrid
    {
        private GridSquare[] m_gridArray;
        private Vector2 m_anchorPoint;
        private Vector2 m_adjustToMiddleOfGridSquare = new Vector2(0.5f, 0.5f);
        private Vector2Int m_screenGridCellRange;
        private Vector2Int m_botGridPosition;

        private int m_gridSizeX;
        private int m_gridSizeY;

        //============================================================================================================//

        #region Setup

        public void SetupGrid()
        {
            m_gridSizeX = Globals.GridSizeX;
            m_gridSizeY = Globals.GridSizeY;

            m_anchorPoint = Vector2.left *
                ((m_gridSizeX / 2.0f) * Constants.gridCellSize)
                + (Vector2.left * Constants.gridCellSize * 0.5f)
                + (Vector2.down * Values.Constants.gridCellSize / 2);

            m_gridArray = new GridSquare[m_gridSizeX * m_gridSizeY];

            for (int i = 0; i < m_gridArray.Length; i++)
            {
                m_gridArray[i] = new GridSquare();
            }

            float height = Camera.main.orthographicSize * 2.0f;
            float width = height * Screen.width / Screen.height;
            m_screenGridCellRange = new Vector2Int((int)(width / Constants.gridCellSize), (int)(height / Constants.gridCellSize));
            m_botGridPosition = GetCoordinatesOfGridSquareAtLocalPosition(LevelManager.Instance.BotObject.transform.position);
        }

        public void MoveObstacleMarkersDownwardOnGrid(List<IObstacle> obstacles, StageRemoteData stageData)
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                if (obstacles[i] == null)
                    continue;
                

                //TODO: Consider whether this should be using screen padding
                bool onScreen = CameraController.IsPointInCameraRect(obstacles[i].transform.position);

                if (!onScreen && !obstacles[i].IsMarkedOnGrid)
                {
                    continue;
                }

                Vector2Int gridCoordinatesAbove = GetCoordinatesOfGridSquareAtLocalPosition(obstacles[i].transform.localPosition + (Vector3.up * Constants.gridCellSize));
                GridSquare gridSquareAbove = GetGridSquareAtCoordinates(gridCoordinatesAbove);
                int radiusMarkAround = gridSquareAbove.RadiusMarkAround;
                SetObstacleInGridSquare(gridSquareAbove, 0, false);
                SetObstacleInSquaresAroundCoordinates(gridCoordinatesAbove.x, gridCoordinatesAbove.y, radiusMarkAround, false);

                if (!onScreen)
                {
                    obstacles[i].IsMarkedOnGrid = false;
                    continue;
                }

                Vector2Int gridCoordinates = GetCoordinatesOfGridSquareAtLocalPosition(obstacles[i].transform.localPosition);
                GridSquare gridSquare = GetGridSquareAtCoordinates(gridCoordinates);
                SetObstacleInGridSquare(gridSquare, radiusMarkAround, true);
                SetObstacleInSquaresAroundCoordinates(gridCoordinates.x, gridCoordinates.y, radiusMarkAround, true);
                obstacles[i].IsMarkedOnGrid = true;
            }
        }

        /*public void MoveObstacleMarkersDownwardOnGridArchived()
        {
            for (int y = 0; y < m_gridSizeY; y++)
            {
                for (int x = 0; x < m_gridSizeX; x++)
                {
                    if (y + 1 == m_gridSizeY)
                    {
                        SetObstacleInGridSquareAtCoordiantes(x, y, 0, false);
                        
                    }
                    else
                    {
                        SetObstacleInGridSquareAtCoordiantes(x, y, GetGridSquareAtCoordinates(x, y + 1).RadiusMarkAround, GetGridSquareAtCoordinates(x, y + 1).ObstacleInSquare);
                    }
                }
            }

            for (int y = 0; y < m_gridSizeY; y++)
            {
                for (int x = 0; x < m_gridSizeX; x++)
                {
                    SetObstacleInSquaresAroundCoordinates(x, y, GetGridSquareAtCoordinates(x, y).RadiusMarkAround, true);
                }
            }
        }*/

        #endregion //Setup

        //============================================================================================================//

        #region Get Grid Positions

        private GridSquare GetGridSquareAtCoordinates(Vector2Int gridPosition)
        {
            if (gridPosition.x >= m_gridSizeX)
                gridPosition.x -= m_gridSizeX;
            else if (gridPosition.x < 0)
                gridPosition.x += m_gridSizeX;

            if (gridPosition.y < 0)
            {
                gridPosition.y = 0;
            }

            return m_gridArray[gridPosition.x + (gridPosition.y * m_gridSizeX)];
        }

        public GridSquare GetGridSquareAtCoordinates(int x, int y)
        {
            if (x >= m_gridSizeX)
                x -= m_gridSizeX;
            else if (x < 0)
                x += m_gridSizeX;

            return m_gridArray[x + (y * m_gridSizeX)];
        }

        public Vector2 GetLocalPositionOfCenterOfGridSquareAtCoordinates(Vector2Int gridPosition)
        {
            Vector2 anchorPoint = m_anchorPoint;

            return anchorPoint + ((new Vector2(gridPosition.x, gridPosition.y) + m_adjustToMiddleOfGridSquare) * Constants.gridCellSize);
        }

        public Vector2 GetLocalPositionOfCenterOfGridSquareAtCoordinates(int x, int y)
        {
            Vector2 anchorPoint = m_anchorPoint;

            return anchorPoint + ((new Vector2(x, y) + m_adjustToMiddleOfGridSquare) * Constants.gridCellSize);
        }

        private GridSquare GetGridSquareAtLocalPosition(Vector2 localPosition)
        {
            Vector2Int coordinates = GetCoordinatesOfGridSquareAtLocalPosition(localPosition);

            return GetGridSquareAtCoordinates(coordinates);
        }

        public Vector2Int GetCoordinatesOfGridSquareAtLocalPosition(Vector2 worldLocation)
        {
            return new Vector2Int((int)Mathf.Floor((worldLocation.x - m_anchorPoint.x) / Constants.gridCellSize), (int)Mathf.Floor((worldLocation.y - m_anchorPoint.y) / Constants.gridCellSize));
        }

        #endregion

        //============================================================================================================//

        #region Set Grid Obstacles

        public void SetObstacleInGridSquare(GridSquare gridSquare, int radius, bool occupied)
        {
            gridSquare.SetObstacleInSquare(occupied);
            gridSquare.SetRadiusMarkAround(radius);
        }

        public void SetObstacleInGridSquareAtLocalPosition(Vector2 obstaclePosition, int radius, bool occupied)
        {
            GridSquare gridSquare = GetGridSquareAtLocalPosition(obstaclePosition);

            gridSquare.SetObstacleInSquare(occupied);
            gridSquare.SetRadiusMarkAround(radius);
        }

        public void SetObstacleInGridSquareAtCoordinates(Vector2Int gridPosition, int radius, bool occupied)
        {
            GridSquare gridSquare = GetGridSquareAtCoordinates(gridPosition.x, gridPosition.y);

            gridSquare.SetObstacleInSquare(occupied);
            gridSquare.SetRadiusMarkAround(radius);
        }

        public void SetObstacleInGridSquareAtCoordiantes(int x, int y, int radius, bool occupied)
        {
            GridSquare gridSquare = GetGridSquareAtCoordinates(x, y);

            gridSquare.SetObstacleInSquare(occupied);
            gridSquare.SetRadiusMarkAround(radius);
        }

        public void SetObstacleInSquaresAroundCoordinates(int x, int y, int radiusAround, bool occupied)
        {
            if (radiusAround == 0)
                return;

            int iMin = Mathf.Max(0, x - radiusAround);
            int iMax = Mathf.Min(m_gridSizeX - 1, x + radiusAround);
            int kMin = Mathf.Max(0, y - radiusAround);
            int kMax = Mathf.Min(m_gridSizeY - 1, y + radiusAround);

            for (int i = iMin; i <= iMax; i++)
            {
                for (int k = kMin; k <= kMax; k++)
                {
                    GetGridSquareAtCoordinates(i, k).SetObstacleInSquare(occupied);
                }
            }
        }

        #endregion //Set Grid Obstacles

        //============================================================================================================//

        #region Enemy Spawn Positions

        public Vector2 GetLocalPositionOfSpawnPositionForEnemy(ENEMY_MOVETYPE moveType)
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
            return GetLocalPositionOfCenterOfGridSquareAtCoordinates(
                m_botGridPosition.x + ((UnityEngine.Random.Range(0, 2) * 2 - 1) * UnityEngine.Random.Range(m_screenGridCellRange.x / 2, m_screenGridCellRange.x)), 
                UnityEngine.Random.Range(m_screenGridCellRange.y - 2, m_screenGridCellRange.y));
        }

        //TODO: When the screen size and camera size and grid cell size systems all start working in a scaling fashion, this will need to adjust
        private Vector2 GetVerticalSpawnPositionForEnemy()
        {
            return GetLocalPositionOfCenterOfGridSquareAtCoordinates(
                m_botGridPosition.x + ((UnityEngine.Random.Range(0, 2) * 2 - 1) * UnityEngine.Random.Range(0, m_screenGridCellRange.x / 2)),
                UnityEngine.Random.Range(m_screenGridCellRange.y, m_gridSizeY));
        }

        #endregion

        //============================================================================================================//

        private Vector2Int GetCoordinatesOfRandomGridSquareInGridRegion(Vector2 gridRegion, bool inRandomYLevel)
        {
            if (inRandomYLevel)
            {
                return new Vector2Int(UnityEngine.Random.Range((int)(m_gridSizeX * gridRegion.x), (int)(m_gridSizeX * gridRegion.y)), UnityEngine.Random.Range(0, m_gridSizeY - 1));
            }
            else
            {
                return new Vector2Int(UnityEngine.Random.Range((int)(m_gridSizeX * gridRegion.x), (int)(m_gridSizeX * gridRegion.y)), m_gridSizeY - 1);
            }
        }

        public Vector2 GetLocalPositionOfRandomGridSquareInGridRegion(int scanRadius, Vector2 gridRegion, bool inRandomYLevel)
        {
            int numTries = 10;
            for (int i = 0; i < numTries; i++)
            {
                Vector2Int randomTop = GetCoordinatesOfRandomGridSquareInGridRegion(gridRegion, inRandomYLevel);
                //Vector2Int randomTop = GetRandomTopGridSquareGridPosition(gridRegion) + (Vector2Int.right * m_positionsShiftedHorizontally);
                bool isFreeSpace = true;
                Vector2Int obstacleGridScanMinimum = new Vector2Int(
                    Math.Max(0, randomTop.x - scanRadius),
                    Math.Max(0, randomTop.y - scanRadius));
                Vector2Int obstacleGridScanMaximum = new Vector2Int(
                    Math.Min(m_gridSizeX - 1, randomTop.x + scanRadius),
                    Math.Min(m_gridSizeY - 1, randomTop.y + scanRadius));
                //Check each position in the box for whether an obstacle is there
                for (int j = obstacleGridScanMinimum.x; j <= obstacleGridScanMaximum.x; j++)
                {
                    for (int k = obstacleGridScanMinimum.y; k <= obstacleGridScanMaximum.y; k++)
                    {
                        if (LevelManager.Instance.WorldGrid.GetGridSquareAtCoordinates(j, k).ObstacleInSquare)
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
                    return GetLocalPositionOfCenterOfGridSquareAtCoordinates(randomTop);
                }
            }

            return GetLocalPositionOfRandomGridSquareInGridRegion(scanRadius - 1, gridRegion, inRandomYLevel);
        }

        public Vector2Int[] SelectBitExplosionPositions(Vector2 startingLocation, int numBits, int verticalExplosionRange, int horizontalExplosionRange)
        {
            Vector2Int[] bitExplosionPositions = new Vector2Int[numBits];

            Vector2Int startingPoint = GetCoordinatesOfGridSquareAtLocalPosition(startingLocation);

            for (int i = 0; i < numBits; i++)
            {
                Vector2Int bitPosition = startingPoint +
                    (Vector2Int.up * UnityEngine.Random.Range(verticalExplosionRange / 2, verticalExplosionRange + 1)) +
                    (Vector2Int.left * UnityEngine.Random.Range(0, horizontalExplosionRange + 1) * (UnityEngine.Random.Range(0, 2) * 2 - 1));

                if (GetGridSquareAtCoordinates(bitPosition).ObstacleInSquare)
                {
                    continue;
                }
                bitExplosionPositions[i] = bitPosition;
            }

            return bitExplosionPositions;
        }

        //============================================================================================================//

        #region Test Functions

#if UNITY_EDITOR

        public void OnDrawGizmos()
        {
            
            //Draw debug lines to show the area of the grid
            for (int x = 0; x < m_gridSizeX; x++)
            {
                for (int y = 0; y < m_gridSizeY; y++)
                {
                    Gizmos.color = Color.red;
                    Vector2 tempVector = new Vector2(x, y);

                    Gizmos.DrawLine(m_anchorPoint + tempVector * Constants.gridCellSize, m_anchorPoint + new Vector2(x, y + 1) * Constants.gridCellSize);
                    Debug.DrawLine(m_anchorPoint + tempVector * Constants.gridCellSize, m_anchorPoint + new Vector2(x + 1, y) * Constants.gridCellSize);
                }
            }
            Debug.DrawLine(m_anchorPoint + new Vector2(0, m_gridSizeY) * Constants.gridCellSize, m_anchorPoint + new Vector2(m_gridSizeX, m_gridSizeY) * Constants.gridCellSize);
            Debug.DrawLine(m_anchorPoint + new Vector2(m_gridSizeX, 0) * Constants.gridCellSize, m_anchorPoint + new Vector2(m_gridSizeX, m_gridSizeY) * Constants.gridCellSize);
        }

#endif

        #endregion

        //============================================================================================================//

        #region Defunct Functions

        /*public void MoveObstacleMarkersLeftOnGrid(int amount)
        {
            m_positionsShiftedHorizontally--;
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
        }*/

        /*public void MoveObstacleMarkersRightOnGrid(int amount)
        {
            m_positionsShiftedHorizontally++;
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
        }*/

        public void DrawDebugMarkedGridPoints()
        {
            for (int x = 0; x < m_gridSizeX; x++)
            {
                for (int y = 0; y < m_gridSizeY; y++)
                {
                    if (GetGridSquareAtCoordinates(x, y).ObstacleInSquare == true)
                    {
                        Debug.DrawLine(GetLocalPositionOfCenterOfGridSquareAtCoordinates(x, y) + (Vector2)LevelManager.Instance.ObstacleManager.WorldElementsRoot.transform.position, 
                            GetLocalPositionOfCenterOfGridSquareAtCoordinates(x, y) + (Vector2)LevelManager.Instance.ObstacleManager.WorldElementsRoot.transform.position + new Vector2(0, 0.25f), new Color(255, 0, 0), 300f);
                    }
                }
            }
            Debug.Break();
        }

        #endregion //Defunct Functions
    }
}
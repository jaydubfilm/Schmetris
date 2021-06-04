using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using StarSalvager.Values;
using StarSalvager.AI;
using UnityEngine.InputSystem.Interactions;
using StarSalvager.ScriptableObjects;
using StarSalvager.Cameras;
using System.Linq;

namespace StarSalvager
{
    public class WorldGrid
    {
        private GridSquare[] m_gridArray;
        private Vector2 m_anchorPoint;
        private Vector2 m_adjustToMiddleOfGridSquare = new Vector2(0.5f, 0.5f);
        public Vector2Int m_screenGridCellRange { get; private set; }
        private Vector2Int m_botGridPosition;

        private int m_gridSizeX;
        private int m_gridSizeY;

        private Dictionary<Vector2, List<int>> randomPositionFindingLists;

        //============================================================================================================//

        #region Setup

        //Sets up the world grid with an anchor point at the bottom left of the grid, 0,0 is the bottom left corner of it
        public void SetupGrid()
        {
            m_gridSizeX = Globals.GridSizeX;
            m_gridSizeY = Globals.GridSizeY;

            m_anchorPoint = Vector2.left *
                ((m_gridSizeX / 2.0f) * Constants.gridCellSize)
                + (Vector2.left * (Constants.gridCellSize * 0.5f))
                + (Vector2.down * Constants.gridCellSize / 2);

            m_gridArray = new GridSquare[m_gridSizeX * m_gridSizeY];

            for (int i = 0; i < m_gridArray.Length; i++)
            {
                m_gridArray[i] = new GridSquare();
            }

            //Used to ensure the CameraVisibleRect is updated
            CameraController.IsPointInCameraRect(Vector2.zero, Constants.VISIBLE_GAME_AREA);

            var cameraRect = CameraController.VisibleCameraRect;

            float width = cameraRect.xMax - cameraRect.xMin;
            float height = cameraRect.yMax - cameraRect.yMin;
            m_screenGridCellRange = new Vector2Int((int)(width / Constants.gridCellSize), (int)(height / Constants.gridCellSize));
            m_botGridPosition = GetCoordinatesOfGridSquareAtLocalPosition(LevelManager.Instance.BotInLevel.transform.position);
            randomPositionFindingLists = new Dictionary<Vector2, List<int>>();
            randomPositionFindingLists.Clear();
        }

        //Find all markers with a "has obstacle", and shift that marker 1 space downwards on the grid
        public void MoveObstacleMarkersDownwardOnGrid(List<IObstacle> obstacles, StageRemoteData stageData)
        {

            for (int i = 0; i < obstacles.Count; i++)
            {
                if (obstacles[i] == null)
                    continue;


                if (obstacles[i] is Shape shape)
                {
                    for (int k = 0; k < shape.AttachedBits.Count; k++)
                    {
                        Vector2Int gridCoordinatesAbove = GetCoordinatesOfGridSquareAtLocalPosition(obstacles[i].transform.localPosition + shape.AttachedBits[k].transform.localPosition + (Vector3.up * Constants.gridCellSize));
                        GridSquare gridSquareAbove = GetGridSquareAtCoordinates(gridCoordinatesAbove);
                        int radiusMarkAround = gridSquareAbove.RadiusMarkAround;
                        SetObstacleInGridSquare(gridSquareAbove, 0, false);
                        SetObstacleInSquaresAroundCoordinates(gridCoordinatesAbove.x, gridCoordinatesAbove.y, radiusMarkAround, false);

                        Vector2Int gridCoordinates = GetCoordinatesOfGridSquareAtLocalPosition(obstacles[i].transform.localPosition + shape.AttachedBits[k].transform.localPosition);
                        GridSquare gridSquare = GetGridSquareAtCoordinates(gridCoordinates);
                        SetObstacleInGridSquare(gridSquare, radiusMarkAround, true);
                        SetObstacleInSquaresAroundCoordinates(gridCoordinates.x, gridCoordinates.y, radiusMarkAround, true);
                    }
                    obstacles[i].IsMarkedOnGrid = true;
                }
                else
                {
                    Vector2Int gridCoordinatesAbove = GetCoordinatesOfGridSquareAtLocalPosition(obstacles[i].transform.localPosition + (Vector3.up * Constants.gridCellSize));
                    GridSquare gridSquareAbove = GetGridSquareAtCoordinates(gridCoordinatesAbove);
                    int radiusMarkAround = gridSquareAbove.RadiusMarkAround;
                    SetObstacleInGridSquare(gridSquareAbove, 0, false);
                    SetObstacleInSquaresAroundCoordinates(gridCoordinatesAbove.x, gridCoordinatesAbove.y, radiusMarkAround, false);

                    Vector2Int gridCoordinates = GetCoordinatesOfGridSquareAtLocalPosition(obstacles[i].transform.localPosition);
                    GridSquare gridSquare = GetGridSquareAtCoordinates(gridCoordinates);
                    SetObstacleInGridSquare(gridSquare, radiusMarkAround, true);
                    SetObstacleInSquaresAroundCoordinates(gridCoordinates.x, gridCoordinates.y, radiusMarkAround, true);
                    obstacles[i].IsMarkedOnGrid = true;
                }
            }
        }

        #endregion //Setup

        //============================================================================================================//

        #region Get Grid Positions

        private GridSquare GetGridSquareAtCoordinates(Vector2Int gridPosition)
        {
            return GetGridSquareAtCoordinates(gridPosition.x, gridPosition.y);
        }

        public GridSquare GetGridSquareAtCoordinates(int x, int y)
        {
            if (x >= m_gridSizeX)
            {
                x = m_gridSizeX - 1;
            }
            else if (x < 0)
            {
                x = 0;
            }

            if (y >= m_gridSizeY)
            {
                y = m_gridSizeY - 1;
            }
            else if (y < 0)
            {
                y = 0;
            }

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

        public GridSquare GetGridSquareAtLocalPosition(Vector2 localPosition)
        {
            Vector2Int coordinates = GetCoordinatesOfGridSquareAtLocalPosition(localPosition);

            return GetGridSquareAtCoordinates(coordinates);
        }

        public Vector2 GetLocalPositionOfCenterOfGridSquareAtLocalPosition(Vector2 localPosition)
        {
            return GetLocalPositionOfCenterOfGridSquareAtCoordinates(GetCoordinatesOfGridSquareAtLocalPosition(localPosition));
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
            gridSquare.ObstacleInSquare = occupied;
            gridSquare.RadiusMarkAround = radius;
        }

        public void SetObstacleInGridSquareAtLocalPosition(Vector2 obstaclePosition, int radius, bool occupied)
        {
            GridSquare gridSquare = GetGridSquareAtLocalPosition(obstaclePosition);

            gridSquare.ObstacleInSquare = occupied;
            gridSquare.RadiusMarkAround = radius;
        }

        public void SetObstacleInGridSquareAtCoordinates(Vector2Int gridPosition, int radius, bool occupied)
        {
            GridSquare gridSquare = GetGridSquareAtCoordinates(gridPosition.x, gridPosition.y);

            gridSquare.ObstacleInSquare = occupied;
            gridSquare.RadiusMarkAround = radius;
        }

        public void SetObstacleInGridSquareAtCoordiantes(int x, int y, int radius, bool occupied)
        {
            GridSquare gridSquare = GetGridSquareAtCoordinates(x, y);

            gridSquare.ObstacleInSquare = occupied;
            gridSquare.RadiusMarkAround = radius;
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
                    GetGridSquareAtCoordinates(i, k).ObstacleInSquare = occupied;
                }
            }
        }

        #endregion //Set Grid Obstacles

        //============================================================================================================//

        #region Enemy Spawn Positions

        public Vector2 GetLocalPositionOfSpawnPositionForEnemy(Enemy enemy)
        {
            return enemy.SpawnAboveScreen ? GetVerticalSpawnPositionForEnemy() : GetHorizontalSpawnPositionForEnemy();
        }

        //TODO: When the screen size and camera size and grid cell size systems all start working in a scaling fashion, this will need to adjust
        private Vector2 GetHorizontalSpawnPositionForEnemy()
        {
            return GetLocalPositionOfCenterOfGridSquareAtCoordinates(
                m_botGridPosition.x + ((UnityEngine.Random.Range(0, 2) * 2 - 1) * UnityEngine.Random.Range(m_screenGridCellRange.x / 2, m_screenGridCellRange.x)), 
                UnityEngine.Random.Range((int)(m_screenGridCellRange.y * 0.4f), (int)(m_screenGridCellRange.y * 0.8f)));
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

        //Finds a random available position in the grid region that is somewhat spaced from other currently existing objects
        /*public Vector2? GetLocalPositionOfRandomGridSquareInGridRegion(int scanRadius, int minScanRadius, Vector2 gridRegion, bool allowOverlap, bool forceSpawn, bool inRandomYLevel)
        {
            if (!randomPositionFindingLists.ContainsKey(gridRegion))
            {
                randomPositionFindingLists.Add(gridRegion, new List<int>());

                int beginIndex = (int)(m_gridSizeX * (double)gridRegion.x);
                int endIndex = (int)(m_gridSizeX * (double)gridRegion.y);

                for (int i = beginIndex; i <= endIndex && i < m_gridSizeX; i++)
                {
                    randomPositionFindingLists[gridRegion].Add(i);
                }
            }

            List<int> randomGridRegion = randomPositionFindingLists[gridRegion];

            for (int i = 0; i < randomGridRegion.Count; i++)
            {
                int temp = randomGridRegion[i];
                int randomIndex = UnityEngine.Random.Range(i, randomGridRegion.Count);
                randomGridRegion[i] = randomGridRegion[randomIndex];
                randomGridRegion[randomIndex] = temp;
            }

            if (allowOverlap)
            {
                if (inRandomYLevel)
                {
                    return GetLocalPositionOfCenterOfGridSquareAtCoordinates(new Vector2Int(randomGridRegion[0], UnityEngine.Random.Range(0, m_gridSizeY)));
                }
                else
                {
                    return GetLocalPositionOfCenterOfGridSquareAtCoordinates(new Vector2Int(randomGridRegion[0], m_gridSizeY - 1));
                }
            }

            for (int i = 0; i < randomGridRegion.Count; i++)
            {
                Vector2Int topPosition = new Vector2Int(randomGridRegion[i], m_gridSizeY - 1);

                if (inRandomYLevel)
                {
                    topPosition.y = UnityEngine.Random.Range(0, m_gridSizeY);
                }

                bool isFreeSpace = true;
                Vector2Int obstacleGridScanMinimum = new Vector2Int(
                    Math.Max(0, topPosition.x - scanRadius),
                    Math.Max(0, topPosition.y - scanRadius));
                Vector2Int obstacleGridScanMaximum = new Vector2Int(
                    Math.Min(m_gridSizeX - 1, topPosition.x + scanRadius),
                    Math.Min(m_gridSizeY - 1, topPosition.y + scanRadius));
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
                    return GetLocalPositionOfCenterOfGridSquareAtCoordinates(topPosition);
                }
            }

            if (scanRadius > minScanRadius)
            {
                return GetLocalPositionOfRandomGridSquareInGridRegion(scanRadius - 1, minScanRadius, gridRegion, allowOverlap, forceSpawn, inRandomYLevel);
            }
            else
            {
                if (forceSpawn)
                {
                    Debug.LogError("Couldn't find position to spawn. Possible overlap occurring in grid region " + allowOverlap + (double)gridRegion.x + ", " + (double)gridRegion.y);
                    return GetLocalPositionOfCenterOfGridSquareAtCoordinates(new Vector2Int(randomGridRegion[0], m_gridSizeY - 1));
                }
                else
                {
                    return null;
                }
            }
        }*/
        
        public Vector2? GetLocalPositionOfRandomGridSquareInGridRegion(int scanRadius, int minScanRadius, Vector2 gridRegion, bool allowOverlap, bool forceSpawn, int yLevel)
        {
            if (!randomPositionFindingLists.ContainsKey(gridRegion))
            {
                randomPositionFindingLists.Add(gridRegion, new List<int>());

                int beginIndex = (int)(m_gridSizeX * (double)gridRegion.x);
                int endIndex = (int)(m_gridSizeX * (double)gridRegion.y);

                for (int i = beginIndex; i <= endIndex && i < m_gridSizeX; i++)
                {
                    randomPositionFindingLists[gridRegion].Add(i);
                }
            }

            List<int> randomGridRegion = randomPositionFindingLists[gridRegion];

            for (int i = 0; i < randomGridRegion.Count; i++)
            {
                int temp = randomGridRegion[i];
                int randomIndex = UnityEngine.Random.Range(i, randomGridRegion.Count);
                randomGridRegion[i] = randomGridRegion[randomIndex];
                randomGridRegion[randomIndex] = temp;
            }

            if (allowOverlap)
            {
                return GetLocalPositionOfCenterOfGridSquareAtCoordinates(new Vector2Int(randomGridRegion[0], yLevel));
            }

            for (int i = 0; i < randomGridRegion.Count; i++)
            {
                Vector2Int topPosition = new Vector2Int(randomGridRegion[i], yLevel);

                bool isFreeSpace = true;
                Vector2Int obstacleGridScanMinimum = new Vector2Int(
                    Math.Max(0, topPosition.x - scanRadius),
                    Math.Max(0, topPosition.y - scanRadius));
                Vector2Int obstacleGridScanMaximum = new Vector2Int(
                    Math.Min(m_gridSizeX - 1, topPosition.x + scanRadius),
                    Math.Min(m_gridSizeY - 1, topPosition.y + scanRadius));
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
                    return GetLocalPositionOfCenterOfGridSquareAtCoordinates(topPosition);
                }
            }

            if (scanRadius > minScanRadius)
            {
                return GetLocalPositionOfRandomGridSquareInGridRegion(scanRadius - 1, minScanRadius, gridRegion, false, forceSpawn, yLevel);
            }

            if (!forceSpawn) 
                return null;
            
            var log = $"Couldn't find position to spawn. Possible overlap occurring in grid region {false} ({(double)gridRegion.x}, {(double)gridRegion.y})";
            
#if UNITY_EDITOR
            Debug.LogError(log);
#else
            Debug.Log(log);
#endif
            
            return GetLocalPositionOfCenterOfGridSquareAtCoordinates(new Vector2Int(randomGridRegion[0], m_gridSizeY - 1));
        }

        public Vector2Int[] SelectBitExplosionPositions(Vector2 startingLocation, int numBits, int verticalExplosionRange, int horizontalExplosionRange)
        {
            Vector2Int[] bitExplosionPositions = new Vector2Int[numBits];

            Vector2Int startingPoint = GetCoordinatesOfGridSquareAtLocalPosition(startingLocation);

            int numRetries = 20;
            int numRetriesUsed = 0;

            for (int i = 0; i < numBits; i++)
            {
                Vector2Int bitPosition = startingPoint +
                    (Vector2Int.up * UnityEngine.Random.Range(verticalExplosionRange - verticalExplosionRange / 3, verticalExplosionRange + 1)) +
                    (Vector2Int.left * (UnityEngine.Random.Range(0, horizontalExplosionRange + 1) * (UnityEngine.Random.Range(0, 2) * 2 - 1)));

                if (GetGridSquareAtCoordinates(bitPosition).ObstacleInSquare || GetGridSquareAtCoordinates(bitPosition + Vector2Int.up).ObstacleInSquare || GetGridSquareAtCoordinates(bitPosition + Vector2Int.up * 2).ObstacleInSquare || bitExplosionPositions.Contains(bitPosition))
                {
                    if (numRetriesUsed < numRetries)
                    {
                        numRetriesUsed++;
                        i--;
                    }
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
    }
}
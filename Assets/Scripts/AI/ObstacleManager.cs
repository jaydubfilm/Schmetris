using System.Collections.Generic;
using UnityEngine;
using StarSalvager.Values;
using StarSalvager.Factories;
using Recycling;
using StarSalvager.AI;
using UnityEngine.UIElements;

namespace StarSalvager
{
    public class ObstacleManager : MonoBehaviour
    {
        private List<IObstacle> m_obstacles;
        private List<Shape> m_notFullyInGridShapes;

        //Input Manager variables - -1.0f for left, 0 for nothing, 1.0f for right
        private float m_currentInput;

        //Variables to spawn obstacles throughout a stage
        private StageRemoteData m_currentStageData = null;
        private StageRemoteData m_previousStageData = null;
        private float m_blendTimer = 0.0f;
        private int m_nextStageToSpawn;

        private float m_distanceHorizontal = 0.0f;

        // Start is called before the first frame update
        void Start()
        {
            m_obstacles = new List<IObstacle>();
            m_notFullyInGridShapes = new List<Shape>();

            SetupStage(0);
        }


        // Update is called once per frame
        void Update()
        {
            if (m_blendTimer < m_currentStageData.StageBlendPeriod)
            {
                m_blendTimer += Time.deltaTime;
            }
            if (LevelManager.Instance.CurrentStage == m_nextStageToSpawn)
            {
                SetupStage(m_nextStageToSpawn);
            }

            HandleObstacleMovement();
        }

        private void HandleObstacleMovement()
        {
            Vector3 amountShift = Vector3.up * ((Constants.gridCellSize * Time.deltaTime) / Constants.timeForAsteroidsToFall);

            if (m_distanceHorizontal != 0)
            {
                int gridPositionXPrevious = (int)Mathf.Ceil(m_distanceHorizontal + (Constants.gridCellSize / 2) / Constants.gridCellSize);

                if (m_distanceHorizontal > 0)
                {
                    float toMove = Mathf.Min(m_distanceHorizontal, Constants.botHorizontalSpeed * Time.deltaTime);
                    amountShift += Vector3.right * toMove;
                    m_distanceHorizontal -= toMove;
                }
                else if (m_distanceHorizontal < 0)
                {
                    float toMove = Mathf.Min(Mathf.Abs(m_distanceHorizontal), Constants.botHorizontalSpeed * Time.deltaTime);
                    amountShift += Vector3.left * toMove;
                    m_distanceHorizontal += toMove;
                }

                int gridPositionXCurrent = (int)Mathf.Ceil(m_distanceHorizontal + (Constants.gridCellSize / 2) / Constants.gridCellSize);
                if (gridPositionXPrevious > gridPositionXCurrent)
                {
                    LevelManager.Instance.WorldGrid.MoveObstacleMarkersLeftOnGrid(gridPositionXPrevious - gridPositionXCurrent);
                }
                else if (gridPositionXPrevious < gridPositionXCurrent)
                {
                    LevelManager.Instance.WorldGrid.MoveObstacleMarkersRightOnGrid(gridPositionXCurrent - gridPositionXPrevious);
                }
            }

            for (int i = m_obstacles.Count - 1; i >= 0; i--)
            {
                var obstacle = m_obstacles[i];
                if (obstacle == null)
                {
                    m_obstacles.RemoveAt(i);
                    continue;
                }

                //Check if currently recycled
                //TODO: Think of a better way to check if this is in the recycler
                if (!obstacle.gameObject.activeInHierarchy)
                {
                    m_obstacles.RemoveAt(i);
                }

                if (!obstacle.CanMove)
                {
                    continue;
                }

                var pos = obstacle.transform.position;
                Vector2 gridPosition = LevelManager.Instance.WorldGrid.GetGridPositionOfVector(obstacle.transform.position);
                pos -= amountShift;

                if (gridPosition.y < -10)
                {
                    var temp = m_obstacles[i];
                    m_obstacles.RemoveAt(i);

                    switch (obstacle)
                    {
                        case Bit _:
                            Recycler.Recycle<Bit>(temp.gameObject);
                            break;
                        case Shape shape:
                            //Recycler.Recycle<Shape>(temp.gameObject);
                            shape.Destroy();
                            break;
                    }
                    continue;
                }

                if (gridPosition.x < 0)
                    pos += Vector3.right * (Values.Globals.GridSizeX * Constants.gridCellSize);
                else if (gridPosition.x >= Values.Globals.GridSizeX)
                    pos += Vector3.left * (Values.Globals.GridSizeX * Constants.gridCellSize);

                obstacle.transform.position = pos;
            }

            if (m_currentInput != 0.0f && Mathf.Abs(m_distanceHorizontal) <= 0.2f)
            {
                Move(m_currentInput);
            }
        }

        private void SetupStage(int waveNumber)
        {
            m_previousStageData = m_currentStageData;
            m_currentStageData = LevelManager.Instance.WaveRemoteData.GetRemoteData(waveNumber);
            m_nextStageToSpawn = waveNumber + 1;
            m_blendTimer = 0;
        }

        public void TryMarkNewShapesOnGrid()
        {
            for (int i = m_notFullyInGridShapes.Count - 1; i >= 0; i--)
            {
                bool fullyInGrid = true;
                foreach (Bit bit in m_notFullyInGridShapes[i].AttachedBits)
                {
                    Vector2Int gridPosition = LevelManager.Instance.WorldGrid.GetGridPositionOfVector
                        (bit.transform.position);
                    if (gridPosition.y >= Values.Globals.GridSizeY)
                    {
                        fullyInGrid = false;
                    }
                    else
                    {
                        LevelManager.Instance.WorldGrid.SetObstacleInGridSquare(bit.transform.position, true);
                    }
                }
                if (fullyInGrid)
                {
                    m_notFullyInGridShapes.RemoveAt(i);
                }
            }
        }

        public void Move(float direction)
        {
            if (UnityEngine.Input.GetKey(KeyCode.LeftAlt))
            {
                m_currentInput = 0f;
                return;
            }

            m_currentInput = direction;

            m_distanceHorizontal += direction * Constants.gridCellSize;
        }

        public void SpawnNewRowOfObstacles()
        {
            foreach (StageObstacleData stageObstacleData in m_currentStageData.StageObstacleData)
            {
                float spawnVariable = stageObstacleData.AsteroidPerRowAverage;
                if (m_previousStageData != null && m_blendTimer < m_currentStageData.StageBlendPeriod)
                {
                    spawnVariable *= Mathf.Lerp(0, 1, m_blendTimer / m_currentStageData.StageBlendPeriod);
                }

                while (spawnVariable >= 1)
                {
                    SpawnObstacle(stageObstacleData.SelectionType, stageObstacleData.BitType, stageObstacleData.AsteroidSize);
                    spawnVariable -= 1;
                }

                if (spawnVariable == 0)
                    continue;

                float random = Random.Range(0.0f, 1.0f);

                if (random <= spawnVariable)
                {
                    SpawnObstacle(stageObstacleData.SelectionType, stageObstacleData.BitType, stageObstacleData.AsteroidSize);
                }
            }

            if (m_previousStageData == null || m_blendTimer > m_currentStageData.StageBlendPeriod)
                return;

            foreach (StageObstacleData stageObstacleData in m_previousStageData.StageObstacleData)
            {
                float spawnVariable = stageObstacleData.AsteroidPerRowAverage * Mathf.Lerp(1, 0, m_blendTimer / m_currentStageData.StageBlendPeriod);

                while (spawnVariable >= 1)
                {
                    SpawnObstacle(stageObstacleData.SelectionType, stageObstacleData.BitType, stageObstacleData.AsteroidSize);
                    spawnVariable -= 1;
                }

                if (spawnVariable == 0)
                    continue;

                float random = Random.Range(0.0f, 1.0f);

                if (random <= spawnVariable)
                {
                    SpawnObstacle(stageObstacleData.SelectionType, stageObstacleData.BitType, stageObstacleData.AsteroidSize);
                }
            }
        }

        private void SpawnObstacle(SELECTION_TYPE selectionType, BIT_TYPE bitType, ASTEROID_SIZE asteroidSize, bool inRandomYLevel = false)
        {
            //Temp to translate Asteroid size into # of bits
            int numBitsInObstacle;
            switch (asteroidSize)
            {
                case ASTEROID_SIZE.Bit:
                default:
                    numBitsInObstacle = 1;
                    break;
                case ASTEROID_SIZE.Small:
                    numBitsInObstacle = Random.Range(2, 4);
                    break;
                case ASTEROID_SIZE.Medium:
                    numBitsInObstacle = Random.Range(4, 6);
                    break;
                case ASTEROID_SIZE.Large:
                    numBitsInObstacle = Random.Range(6, 9);
                    break;
            }

            if (numBitsInObstacle == 1)
            {
                //Make bit and push to list
                Bit newBit = FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateGameObject(bitType).GetComponent<Bit>();
                AddMovableToList(newBit);
                PlaceMovableOnGrid(newBit);
            }
            else
            {
                Shape newShape = FactoryManager.Instance.GetFactory<ShapeFactory>().CreateObject<Shape>(selectionType, bitType, numBitsInObstacle);
                AddMovableToList(newShape);
                foreach (Bit bit in newShape.AttachedBits)
                {
                    AddMovableToList(bit);
                }
                PlaceMovableOnGrid(newShape);
            }
        }

        public void AddMovableToList(IObstacle movable)
        {
            //TODO: Find a more elegant solution for this if statement. This is catching the scenario where a bit is recycled and reused in the same frame, before it can be removed by the update loop, resulting in it being in the list twice.
            if (!m_obstacles.Contains(movable))
                m_obstacles.Add(movable);
        }

        private void PlaceMovableOnGrid(IObstacle movable)
        {
            movable.transform.parent = LevelManager.Instance.gameObject.transform;
            Vector2 position = LevelManager.Instance.WorldGrid.GetAvailableRandomTopGridSquareWorldPosition();
            movable.transform.position = position;
            switch(movable)
            {
                case Bit _:
                    LevelManager.Instance.WorldGrid.SetObstacleInGridSquare(position, true);
                    break;
                case Shape shape:
                    m_notFullyInGridShapes.Add(shape);
                    break;
            }
        }
    }
}

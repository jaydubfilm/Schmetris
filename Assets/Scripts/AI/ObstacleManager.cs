using System.Collections.Generic;
using UnityEngine;
using StarSalvager.Values;
using StarSalvager.Factories;
using Recycling;
using StarSalvager.AI;
using UnityEngine.UIElements;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;

namespace StarSalvager
{
    public class ObstacleManager : MonoBehaviour, IReset, IPausable, IMoveOnInput
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

        public bool isPaused => GameTimer.IsPaused;

        // Start is called before the first frame update
        private void Start()
        {
            m_obstacles = new List<IObstacle>();
            m_notFullyInGridShapes = new List<Shape>();
            GameTimer.AddPausable(this);

            SetupStage(0);

            RegisterMoveOnInput();
        }


        // Update is called once per frame
        private void Update()
        {
            if (isPaused)
                return;

            if (m_blendTimer < m_currentStageData.StageBlendPeriod)
            {
                m_blendTimer += Time.deltaTime;
            }
            if (LevelManager.Instance.CurrentStage == m_nextStageToSpawn)
            {
                SetupStage(m_nextStageToSpawn);
            }

            HandleObstacleMovement();
            
            //Set the movement direction 
            Globals.MovingDirection = Mathf.Abs(m_distanceHorizontal) <= 0.2f ? DIRECTION.NULL: m_distanceHorizontal.GetHorizontalDirection();
        }

        public void Activate()
        {
            //Spawn enemies from wave 0
            SetupStage(0);
        }

        public void Reset()
        {
            for (int i = m_obstacles.Count - 1; i >= 0; i--)
            {
                var obstacle = m_obstacles[i];
                if (obstacle == null)
                {
                    m_obstacles.RemoveAt(i);
                    continue;
                }

                /*if (obstacle is IRecycled recycled && recycled.IsRecycled)
                {
                    m_obstacles.RemoveAt(i);
                    continue;
                }*/

                switch (obstacle)
                {
                    case Bit bit:
                        Recycler.Recycle<Bit>(bit);
                        break;
                    case Shape shape:
                        Recycler.Recycle<Shape>(shape, new
                        {
                            recycleBits = false
                        });
                        break;
                }
                m_obstacles.RemoveAt(i);
            }
            for (int i = m_notFullyInGridShapes.Count - 1; i >= 0; i--)
            {
                Recycler.Recycle<Shape>(m_notFullyInGridShapes[i].gameObject, new
                {
                    recycleBits = false
                });
                m_notFullyInGridShapes.RemoveAt(i);
            }
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

                //TODO: These lines may be necessary, may not be. 
                /*if (obstacle is IRecycled recycled && recycled.IsRecycled)
                {
                    m_obstacles.RemoveAt(i);
                    continue;
                }*/

                if (!obstacle.CanMove)
                {
                    continue;
                }

                var pos = obstacle.transform.position;
                Vector2 gridPosition = LevelManager.Instance.WorldGrid.GetGridPositionOfVector(pos);
                pos -= amountShift;

                if (gridPosition.y < -10)
                {
                    var temp = m_obstacles[i];

                    switch (temp)
                    {
                        case Bit bit:
                            if (!bit.Attached)
                            {
                                Recycler.Recycle<Bit>(bit);
                                m_obstacles[i] = null;
                            }
                            break;
                        case Shape shape:
                            foreach (var attachedBit in shape.AttachedBits)
                            {
                                if(m_obstacles.Contains(attachedBit))
                                {
                                    m_obstacles[m_obstacles.IndexOf(attachedBit)] = null;
                                }
                            }
                            Recycler.Recycle<Shape>(shape);
                            m_obstacles[i] = null;
                            break;
                    }

                    m_obstacles.RemoveAt(i);
                    continue;
                }

                if (gridPosition.x < 0)
                    pos += Vector3.right * (Values.Globals.GridSizeX * Constants.gridCellSize);
                else if (gridPosition.x >= Values.Globals.GridSizeX)
                    pos += Vector3.left * (Values.Globals.GridSizeX * Constants.gridCellSize);

                obstacle.transform.position = pos;
            }

            if (Mathf.Abs(m_distanceHorizontal) > 0.2f)
                return;
            
            if (m_currentInput != 0f)
            {
                Move(m_currentInput);
            }
        }

        public void MoveToNewWave()
        {
            SetupStage(0);
        }

        private void SetupStage(int waveNumber)
        {
            m_previousStageData = m_currentStageData;
            m_currentStageData = LevelManager.Instance.CurrentWaveData.GetRemoteData(waveNumber);
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

        //IMoveOnInput functions
        //================================================================================================================//
        
        public void RegisterMoveOnInput()
        {
            InputManager.RegisterMoveOnInput(this);
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
        
        //================================================================================================================//

        public void SpawnNewRowOfObstacles()
        {
            if (isPaused)
                return;
            
            foreach (StageObstacleData stageObstacleData in m_currentStageData.StageObstacleData)
            {
                float spawnVariable = stageObstacleData.AsteroidPerRowAverage;
                if (m_previousStageData != null && m_blendTimer < m_currentStageData.StageBlendPeriod)
                {
                    spawnVariable *= Mathf.Lerp(0, 1, m_blendTimer / m_currentStageData.StageBlendPeriod);
                }

                while (spawnVariable >= 1)
                {
                    SpawnObstacle(stageObstacleData.SelectionType, stageObstacleData.BitType, stageObstacleData.CustomMade, stageObstacleData.Category, stageObstacleData.AsteroidSize);
                    spawnVariable -= 1;
                }

                if (spawnVariable == 0)
                    continue;

                float random = Random.Range(0.0f, 1.0f);

                if (random <= spawnVariable)
                {
                    SpawnObstacle(stageObstacleData.SelectionType, stageObstacleData.BitType, stageObstacleData.CustomMade, stageObstacleData.Category, stageObstacleData.AsteroidSize);
                }
            }

            if (m_previousStageData == null || m_blendTimer > m_currentStageData.StageBlendPeriod)
                return;

            foreach (StageObstacleData stageObstacleData in m_previousStageData.StageObstacleData)
            {
                float spawnVariable = stageObstacleData.AsteroidPerRowAverage * Mathf.Lerp(1, 0, m_blendTimer / m_currentStageData.StageBlendPeriod);

                while (spawnVariable >= 1)
                {
                    SpawnObstacle(stageObstacleData.SelectionType, stageObstacleData.BitType, stageObstacleData.CustomMade, stageObstacleData.Category, stageObstacleData.AsteroidSize);
                    spawnVariable -= 1;
                }

                if (spawnVariable == 0)
                    continue;

                float random = Random.Range(0.0f, 1.0f);

                if (random <= spawnVariable)
                {
                    SpawnObstacle(stageObstacleData.SelectionType, stageObstacleData.BitType, stageObstacleData.CustomMade, stageObstacleData.Category, stageObstacleData.AsteroidSize);
                }
            }
        }

        private void SpawnObstacle(SELECTION_TYPE selectionType, BIT_TYPE bitType, bool customMade, string category, ASTEROID_SIZE asteroidSize, bool inRandomYLevel = false)
        {
            if (customMade)
            {
                Shape newShape = FactoryManager.Instance.GetFactory<ShapeFactory>().CreateObject<Shape>(selectionType, bitType, category);
                AddMovableToList(newShape);
                foreach (Bit bit in newShape.AttachedBits)
                {
                    AddMovableToList(bit);
                }
                PlaceMovableOnGrid(newShape);
                return;
            }
            
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

        //============================================================================================================//

        public void OnResume()
        {

        }

        public void OnPause()
        {

        }

        //============================================================================================================//
    }
}

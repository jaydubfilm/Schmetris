using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using StarSalvager.Values;
using StarSalvager.Factories;
using Recycling;
using StarSalvager.AI;
using UnityEngine.UIElements;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.UI.Scrapyard;
using Random = UnityEngine.Random;

namespace StarSalvager
{
    public class ObstacleManager : MonoBehaviour, IReset, IPausable, IMoveOnInput
    {
        private List<IObstacle> m_obstacles;
        private List<Shape> m_notFullyInGridShapes;
        private List<OffGridMovement> m_offGridMovingObstacles;

        //Input Manager variables - -1.0f for left, 0 for nothing, 1.0f for right
        private float m_currentInput;

        //Variables to spawn obstacles throughout a stage
        private StageRemoteData m_currentStageData = null;
        private StageRemoteData m_previousStageData = null;
        private float m_blendTimer = 0.0f;
        private int m_nextStageToSpawn;

        private float m_distanceHorizontal = 0.0f;

        public bool isPaused => GameTimer.IsPaused;

        public bool HasNoActiveObstacles
        {
            get
            {
                if (m_obstacles == null || m_offGridMovingObstacles == null)
                    return false;

                return !m_obstacles.Any(o => o != null && o.CanMove) && m_offGridMovingObstacles.Count == 0;

                //m_obstacles.Any(o => o.CanMove) && 
                //m_obstacles.FindAll(o => o.CanMove == true).Count == 0 && m_offGridMovingObstacles.Count == 0;
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
            m_obstacles = new List<IObstacle>();
            m_notFullyInGridShapes = new List<Shape>();
            m_offGridMovingObstacles = new List<OffGridMovement>();
            RegisterPausable();

            SetupStage(0);

            RegisterMoveOnInput();
        }


        // Update is called once per frame
        private void Update()
        {
            if (isPaused)
                return;

            //Simulate the speed of downward movement for obstacles and move the prefabs on screen downward
            Globals.AsteroidFallTimer += Time.deltaTime;
            if (Globals.AsteroidFallTimer >= Constants.timeForAsteroidsToFall)
            {
                Globals.AsteroidFallTimer -= Constants.timeForAsteroidsToFall;
                LevelManager.Instance.WorldGrid.MoveObstacleMarkersDownwardOnGrid();
                if (!LevelManager.Instance.EndWaveState)
                {
                    SpawnNewRowOfObstacles();
                    TryMarkNewShapesOnGrid();
                }
            }

            if (m_blendTimer < m_currentStageData.StageBlendPeriod)
            {
                m_blendTimer += Time.deltaTime;
            }

            if (!LevelManager.Instance.EndWaveState && LevelManager.Instance.CurrentStage == m_nextStageToSpawn)
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
                    case Component component:
                        Recycler.Recycle<Component>(component);
                        break;
                    case Shape shape:
                        Recycler.Recycle<Shape>(shape, new
                        {
                            recycleBits = false
                        });
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(obstacle), obstacle, null);
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

            for (int i = m_offGridMovingObstacles.Count - 1; i >= 0; i--)
            {
                m_offGridMovingObstacles[i].LerpTimer += Time.deltaTime / m_offGridMovingObstacles[i].LerpSpeed;
                if (m_offGridMovingObstacles[i].LerpTimer >= 1)
                {
                    switch(m_offGridMovingObstacles[i].Bit)
                    {
                        case Bit bit:
                            if (m_offGridMovingObstacles[i].DespawnOnEnd)
                            {
                                Recycler.Recycle<Bit>(bit);
                            }
                            else
                            {
                                PlaceMovableOnGrid(bit, m_offGridMovingObstacles[i].EndPosition);
                                bit.SetColliderActive(true);
                            }
                            break;
                        case Component component:
                            if (m_offGridMovingObstacles[i].DespawnOnEnd)
                            {
                                Recycler.Recycle<Component>(component);
                            }
                            else
                            {
                                PlaceMovableOnGrid(component, m_offGridMovingObstacles[i].EndPosition);
                                component.SetColliderActive(true);
                            }
                            break;
                        case Shape shape:
                            if (m_offGridMovingObstacles[i].DespawnOnEnd)
                            {
                                Recycler.Recycle<Shape>(shape);
                            }
                            else
                            {
                                PlaceMovableOnGrid(shape, m_offGridMovingObstacles[i].EndPosition);
                                shape.SetColliderActive(true);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(OffGridMovement.Bit), m_offGridMovingObstacles[i].Bit, null);
                    }
                    
                    m_offGridMovingObstacles.RemoveAt(i);
                    continue;
                }

                m_offGridMovingObstacles[i].Move(-amountShift);
                m_offGridMovingObstacles[i].Spin();
            }

            for (int i = m_obstacles.Count - 1; i >= 0; i--)
            {
                var obstacle = m_obstacles[i];
                if (obstacle == null)
                {
                    m_obstacles.RemoveAt(i);
                    continue;
                }

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
                        case Component component:
                            if (!component.Attached)
                            {
                                Recycler.Recycle<Component>(component);
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
                        default:
                            throw new ArgumentOutOfRangeException(nameof(obstacle), obstacle, null);
                    }

                    m_obstacles.RemoveAt(i);
                    continue;
                }

                if (gridPosition.x < 0)
                    pos += Vector3.right * (Values.Globals.GridSizeX * Constants.gridCellSize);
                else if (gridPosition.x >= Values.Globals.GridSizeX)
                    pos += Vector3.left * (Values.Globals.GridSizeX * Constants.gridCellSize);

                obstacle.transform.position = pos;

                if (obstacle is IRotate rotate && rotate.Rotating)
                {
                    rotate.transform.Rotate(Vector3.forward * Time.deltaTime * 15.0f * rotate.RotateDirection);
                }
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
                float spawnVariable = stageObstacleData.CountPerRowAverage;
                if (m_previousStageData != null && m_blendTimer < m_currentStageData.StageBlendPeriod)
                {
                    spawnVariable *= Mathf.Lerp(0, 1, m_blendTimer / m_currentStageData.StageBlendPeriod);
                }

                while (spawnVariable >= 1)
                {
                    SpawnObstacle(stageObstacleData.SelectionType, stageObstacleData.ShapeName, stageObstacleData.Category, stageObstacleData.AsteroidSize, stageObstacleData.Rotation());
                    spawnVariable -= 1;
                }

                if (spawnVariable == 0)
                    continue;

                float random = Random.Range(0.0f, 1.0f);

                if (random <= spawnVariable)
                {
                    SpawnObstacle(stageObstacleData.SelectionType, stageObstacleData.ShapeName, stageObstacleData.Category, stageObstacleData.AsteroidSize, stageObstacleData.Rotation());
                }
            }

            if (m_previousStageData == null || m_blendTimer > m_currentStageData.StageBlendPeriod)
                return;

            foreach (StageObstacleData stageObstacleData in m_previousStageData.StageObstacleData)
            {
                float spawnVariable = stageObstacleData.CountPerRowAverage * Mathf.Lerp(1, 0, m_blendTimer / m_currentStageData.StageBlendPeriod);

                while (spawnVariable >= 1)
                {
                    SpawnObstacle(stageObstacleData.SelectionType, stageObstacleData.ShapeName, stageObstacleData.Category, stageObstacleData.AsteroidSize, stageObstacleData.Rotation());
                    spawnVariable -= 1;
                }

                if (spawnVariable == 0)
                    continue;

                float random = Random.Range(0.0f, 1.0f);

                if (random <= spawnVariable)
                {
                    SpawnObstacle(stageObstacleData.SelectionType, stageObstacleData.ShapeName, stageObstacleData.Category, stageObstacleData.AsteroidSize, stageObstacleData.Rotation());
                }
            }
        }

        public void SpawnBitExplosion(Vector2 startingLocation, List<IRDSObject> rdsObjects)
        {
            for (int i = rdsObjects.Count - 1; i >= 0; i--)
            {
                if (rdsObjects[i] is RDSValue<TEST_Blueprint> rdsValueBlueprint)
                {
                    PlayerPersistentData.PlayerData.UnlockBlueprint(rdsValueBlueprint.rdsValue);
                    Toast.AddToast("Unlocked Blueprint!");
                    rdsObjects.RemoveAt(i);
                }
                //Remove objects that aren't going on screen
            }
            
            Vector2Int[] bitExplosionPositions = LevelManager.Instance.WorldGrid.SelectBitExplosionPositions(startingLocation, rdsObjects.Count, 5, 5);

            for (int i = 0; i < bitExplosionPositions.Length; i++)
            {
                if (rdsObjects[i] is RDSValue<BlockData> rdsValueBlockData)
                {
                    switch(rdsValueBlockData.rdsValue.ClassType)
                    {
                        case nameof(Bit):
                            Bit newBit = FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateObject<Bit>((BIT_TYPE)rdsValueBlockData.rdsValue.Type, rdsValueBlockData.rdsValue.Level);
                            AddMovableToList(newBit);
                            PlaceMovableOffGrid(newBit, startingLocation, bitExplosionPositions[i], 0.5f);
                            break;
                        case nameof(Component):
                            Component newComponent = FactoryManager.Instance.GetFactory<ComponentAttachableFactory>().CreateObject<Component>((COMPONENT_TYPE)rdsValueBlockData.rdsValue.Type);
                            AddMovableToList(newComponent);
                            PlaceMovableOffGrid(newComponent, startingLocation, bitExplosionPositions[i], 0.5f);
                            break;
                    }
                }
                
                /*Bit newBit = FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateGameObject((BIT_TYPE)Random.Range(1, 6)).GetComponent<Bit>();
                AddMovableToList(newBit);
                PlaceMovableOffGrid(newBit, startingLocation, bitPosition, 0.5f);*/
            }
        }

        private void SpawnObstacle(SELECTION_TYPE selectionType, string shapeName, string category, ASTEROID_SIZE asteroidSize, int numRotations, bool inRandomYLevel = false)
        {
            if (selectionType == SELECTION_TYPE.CATEGORY)
            {
                Shape newShape = FactoryManager.Instance.GetFactory<ShapeFactory>().CreateObject<Shape>(selectionType, category, numRotations);
                
                if (LevelManager.Instance != null)
                    LevelManager.Instance.ObstacleManager.AddMovableToList(newShape);
                
                AddMovableToList(newShape);
                foreach (Bit bit in newShape.AttachedBits)
                {
                    AddMovableToList(bit);
                }
                PlaceMovableOnGrid(newShape);
                return;
            }
            else if (selectionType == SELECTION_TYPE.SHAPE)
            {
                Shape newShape = FactoryManager.Instance.GetFactory<ShapeFactory>().CreateObject<Shape>(selectionType, shapeName, numRotations);

                if (LevelManager.Instance != null)
                    LevelManager.Instance.ObstacleManager.AddMovableToList(newShape);

                AddMovableToList(newShape);
                foreach (Bit bit in newShape.AttachedBits)
                {
                    AddMovableToList(bit);
                }
                PlaceMovableOnGrid(newShape);
                return;
            }
            else if (selectionType == SELECTION_TYPE.ASTEROID)
            {
                Bit newBit = FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateLargeAsteroid<Bit>(asteroidSize);
                AddMovableToList(newBit);
                PlaceMovableOnGrid(newBit);

                return;
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
            Vector2 position = LevelManager.Instance.WorldGrid.GetAvailableRandomTopGridSquareWorldPosition();
            movable.transform.parent = LevelManager.Instance.gameObject.transform;
            movable.transform.position = position;
            switch (movable)
            {
                case Bit _:
                case Component _:
                    LevelManager.Instance.WorldGrid.SetObstacleInGridSquare(position, true);
                    break;
                case Shape shape:
                    m_notFullyInGridShapes.Add(shape);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(movable), movable, null);
            }
        }

        private void PlaceMovableOnGrid(IObstacle movable, Vector2 position)
        {
            movable.transform.parent = LevelManager.Instance.gameObject.transform;
            movable.transform.position = position;
            switch (movable)
            {
                case Bit _:
                case Component _:
                    LevelManager.Instance.WorldGrid.SetObstacleInGridSquare(position, true);
                    break;
                case Shape shape:
                    m_notFullyInGridShapes.Add(shape);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(movable), movable, null);
            }
        }

        private float m_bounceTravelDistance = 40.0f;
        private float m_bounceSpeedAdjustment = 0.5f;
        public void BounceObstacle(IObstacle bit, Vector2 direction, float spinSpeed, bool despawnOnEnd, bool spinning, bool arc)
        {
            m_obstacles.Remove(bit);
            Vector2 destination = (Vector2)bit.transform.position + direction * m_bounceTravelDistance;
            PlaceMovableOffGrid(bit, bit.transform.position, destination, Vector2.Distance(bit.transform.position, destination) / (m_bounceTravelDistance * m_bounceSpeedAdjustment), spinSpeed, despawnOnEnd, spinning, arc);
        }

        private void PlaceMovableOffGrid(IObstacle obstacle, Vector2 startingPosition, Vector2Int gridEndPosition, float lerpSpeed, float spinSpeed = 0.0f, bool despawnOnEnd = false, bool spinning = false, bool arc = false)
        {
            Vector2 endPosition = LevelManager.Instance.WorldGrid.GetCenterOfGridSquareInGridPosition(gridEndPosition);
            PlaceMovableOffGrid(obstacle, startingPosition, endPosition, lerpSpeed, spinSpeed, despawnOnEnd, spinning, arc);
        }

        private void PlaceMovableOffGrid(IObstacle obstacle, Vector2 startingPosition, Vector2 endPosition, float lerpSpeed, float spinSpeed, bool despawnOnEnd, bool spinning, bool arc)
        {
            obstacle.SetColliderActive(false);
            obstacle.transform.parent = LevelManager.Instance.gameObject.transform;
            obstacle.transform.position = startingPosition;

            if (!arc)
                m_offGridMovingObstacles.Add(new OffGridMovementLerp(obstacle, startingPosition, endPosition, lerpSpeed, spinSpeed, despawnOnEnd, spinning));
            else
                m_offGridMovingObstacles.Add(new OffGridMovementArc(obstacle, startingPosition, Vector2.down * 25, endPosition, lerpSpeed, spinSpeed, despawnOnEnd, spinning));
        }

        //============================================================================================================//

        public void RegisterPausable()
        {
            GameTimer.AddPausable(this);
        }

        public void OnResume()
        {

        }

        public void OnPause()
        {

        }

        //============================================================================================================//
    }
}

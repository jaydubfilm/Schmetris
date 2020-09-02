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
using UnityEngine.SceneManagement;
using StarSalvager.Cameras;

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

        private GameObject m_worldElementsRoot;
        public GameObject WorldElementsRoot => m_worldElementsRoot;

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
            m_worldElementsRoot = new GameObject("WorldElementRoot");
            SceneManager.MoveGameObjectToScene(m_worldElementsRoot, gameObject.scene);

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
            if (Globals.AsteroidFallTimer >= Globals.TimeForAsteroidToFallOneSquare)
            {
                Globals.AsteroidFallTimer -= Globals.TimeForAsteroidToFallOneSquare;
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
            Vector3 amountShift = Vector3.up * ((Constants.gridCellSize * Time.deltaTime) / Globals.TimeForAsteroidToFallOneSquare);

            if (m_distanceHorizontal != 0)
            {
                int gridPositionXPrevious = (int)Mathf.Ceil(m_distanceHorizontal + (Constants.gridCellSize / 2) / Constants.gridCellSize);

                if (m_distanceHorizontal > 0)
                {
                    float toMove = Mathf.Min(m_distanceHorizontal, Globals.BotHorizontalSpeed * Time.deltaTime);
                    m_distanceHorizontal -= toMove;
                    m_worldElementsRoot.transform.position += Vector3.left * toMove;
                    LevelManager.Instance.CameraController.MoveCameraWithObstacles(Vector3.left * toMove);
                }
                else if (m_distanceHorizontal < 0)
                {
                    float toMove = Mathf.Min(Mathf.Abs(m_distanceHorizontal), Globals.BotHorizontalSpeed * Time.deltaTime);
                    m_distanceHorizontal += toMove;
                    m_worldElementsRoot.transform.position += Vector3.right * toMove;
                    LevelManager.Instance.CameraController.MoveCameraWithObstacles(Vector3.right * toMove);
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
                                PlaceMovableOnGridSpecific(bit, m_offGridMovingObstacles[i].EndPosition);
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
                                PlaceMovableOnGridSpecific(component, m_offGridMovingObstacles[i].EndPosition);
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
                                PlaceMovableOnGridSpecific(shape, m_offGridMovingObstacles[i].EndPosition);
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

                var pos = obstacle.transform.localPosition;
                Vector2 gridPosition = LevelManager.Instance.WorldGrid.GetCoordinatesOfGridSquareAtLocalPosition(pos);
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

                obstacle.transform.localPosition = pos;

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
            if (waveNumber > 0)
                m_previousStageData = LevelManager.Instance.CurrentWaveData.GetRemoteData(waveNumber - 1);
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
                    Vector2Int gridPosition = LevelManager.Instance.WorldGrid.GetCoordinatesOfGridSquareAtLocalPosition
                        (bit.transform.localPosition + m_notFullyInGridShapes[i].transform.localPosition);
                    if (gridPosition.y >= Values.Globals.GridSizeY)
                    {
                        fullyInGrid = false;
                    }
                    else
                    {
                        LevelManager.Instance.WorldGrid.SetObstacleInGridSquareAtCoordinates(gridPosition, 0, true);
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
            //print(LevelManager.Instance.WorldGrid.GetGridPositionOfVector(LevelManager.Instance.BotGameObject.transform.position));

            if (isPaused)
                return;

            //TODO: Find a better approach. This line is causing the stageblendperiod on the last stage of a wave to prevent spawning for that last portion of the wave. Temporary approach to the waveendsequence.
            if (LevelManager.Instance.CurrentWaveData.GetWaveDuration() <= LevelManager.Instance.WaveTimer + m_currentStageData.StageBlendPeriod)
                return;

            switch (m_currentStageData.StageType)
            {
                case STAGE_TYPE.STANDARD:
                    LevelManager.Instance.StandardBufferZoneObstacleData.SetObstacleDataSpawns(m_currentStageData, false, this);
                    break;
                case STAGE_TYPE.FULLSCREEN:
                    SpawnObstacleData(m_currentStageData.StageObstacleData, new Vector2(0, 1), m_currentStageData.SpawningObstacleMultiplier, false);
                    break;
                case STAGE_TYPE.CUSTOM:
                    foreach (StageColumnGroupObstacleData stageColumnGroupObstacleData in m_currentStageData.StageColumnGroupObstacleData)
                    {
                        Vector2 columnFieldRange = new Vector2(stageColumnGroupObstacleData.ColumnGroupMinimum, stageColumnGroupObstacleData.ColumnGroupMaximum);
                        if (stageColumnGroupObstacleData.IsBlendZone)
                        {
                            IEnumerable<StageColumnGroupObstacleData> columnsLeft = m_currentStageData.StageColumnGroupObstacleData.Where(s => s.ColumnGroupMaximum <= columnFieldRange.x && !s.IsBlendZone);
                            IEnumerable<StageColumnGroupObstacleData> columnsRight = m_currentStageData.StageColumnGroupObstacleData.Where(s => s.ColumnGroupMinimum >= columnFieldRange.y && !s.IsBlendZone);
                            if (columnsLeft.Count() > 0 && columnsRight.Count() > 0)
                            {
                                float columnGroupLeftPosition = columnsLeft.Max(s => s.ColumnGroupMaximum);
                                StageColumnGroupObstacleData columnGroupLeft = m_currentStageData.StageColumnGroupObstacleData.FirstOrDefault(s => s.ColumnGroupMaximum == columnGroupLeftPosition);
                                float columnGroupRightPosition = columnsRight.Min(s => s.ColumnGroupMinimum);
                                StageColumnGroupObstacleData columnGroupRight = m_currentStageData.StageColumnGroupObstacleData.FirstOrDefault(s => s.ColumnGroupMinimum == columnGroupRightPosition);

                                if (columnGroupLeft != null && columnGroupRight != null)
                                {
                                    SpawnObstacleData(columnGroupLeft.StageObstacleData, columnFieldRange, m_currentStageData.SpawningObstacleMultiplier / 2, false);
                                    SpawnObstacleData(columnGroupRight.StageObstacleData, columnFieldRange, m_currentStageData.SpawningObstacleMultiplier / 2, false);
                                }
                            }
                        }
                        else
                        {
                            SpawnObstacleData(stageColumnGroupObstacleData.StageObstacleData, columnFieldRange, m_currentStageData.SpawningObstacleMultiplier, false);
                        }
                    }
                    break;
            }

            if (m_previousStageData == null || m_blendTimer > m_currentStageData.StageBlendPeriod)
                return;

            switch(m_previousStageData.StageType)
            {
                case STAGE_TYPE.STANDARD:
                    LevelManager.Instance.StandardBufferZoneObstacleData.SetObstacleDataSpawns(m_previousStageData, true, this);
                    break;
                case STAGE_TYPE.FULLSCREEN:
                    SpawnObstacleData(m_previousStageData.StageObstacleData, new Vector2(0, 1), m_previousStageData.SpawningObstacleMultiplier, true);
                    break;
                case STAGE_TYPE.CUSTOM:
                    foreach (StageColumnGroupObstacleData stageColumnGroupObstacleData in m_previousStageData.StageColumnGroupObstacleData)
                    {
                        Vector2 columnFieldRange = new Vector2(stageColumnGroupObstacleData.ColumnGroupMinimum, stageColumnGroupObstacleData.ColumnGroupMaximum);
                        if (stageColumnGroupObstacleData.IsBlendZone)
                        {
                            IEnumerable<StageColumnGroupObstacleData> columnsLeft = m_currentStageData.StageColumnGroupObstacleData.Where(s => s.ColumnGroupMaximum <= columnFieldRange.x && !s.IsBlendZone);
                            IEnumerable<StageColumnGroupObstacleData> columnsRight = m_currentStageData.StageColumnGroupObstacleData.Where(s => s.ColumnGroupMinimum >= columnFieldRange.y && !s.IsBlendZone);
                            if (columnsLeft.Count() > 0 && columnsRight.Count() > 0)
                            {
                                float columnGroupLeftPosition = columnsLeft.Max(s => s.ColumnGroupMaximum);
                                StageColumnGroupObstacleData columnGroupLeft = m_previousStageData.StageColumnGroupObstacleData.FirstOrDefault(s => s.ColumnGroupMaximum == columnGroupLeftPosition);
                                float columnGroupRightPosition = columnsRight.Min(s => s.ColumnGroupMinimum);
                                StageColumnGroupObstacleData columnGroupRight = m_previousStageData.StageColumnGroupObstacleData.FirstOrDefault(s => s.ColumnGroupMinimum == columnGroupRightPosition);

                                if (columnGroupLeft != null && columnGroupRight != null)
                                {
                                    SpawnObstacleData(columnGroupLeft.StageObstacleData, columnFieldRange, m_previousStageData.SpawningObstacleMultiplier / 2, true);
                                    SpawnObstacleData(columnGroupRight.StageObstacleData, columnFieldRange, m_previousStageData.SpawningObstacleMultiplier / 2, true);
                                }
                            }
                        }
                        else
                        {
                            SpawnObstacleData(stageColumnGroupObstacleData.StageObstacleData, columnFieldRange, m_previousStageData.SpawningObstacleMultiplier, true);
                        }
                    }
                    break;
            }
        }

        public void SpawnObstacleData(List<StageObstacleData> obstacleData, Vector2 columnFieldRange, float spawningMultiplier, bool isPrevious)
        {
            foreach (StageObstacleData stageObstacleData in obstacleData)
            {
                float spawnVariable = stageObstacleData.Density * spawningMultiplier * ((columnFieldRange.y - columnFieldRange.x) * Globals.GridSizeX);
                if (isPrevious)
                {
                    spawnVariable *= Mathf.Lerp(1, 0, m_blendTimer / m_currentStageData.StageBlendPeriod);
                }
                else if (m_previousStageData != null && m_blendTimer <= m_currentStageData.StageBlendPeriod)
                {
                    spawnVariable *= Mathf.Lerp(0, 1, m_blendTimer / m_currentStageData.StageBlendPeriod);
                }

                while (spawnVariable >= 1)
                {
                    SpawnObstacle(stageObstacleData.SelectionType, stageObstacleData.ShapeName, stageObstacleData.Category, stageObstacleData.AsteroidSize, stageObstacleData.Rotation(), columnFieldRange);
                    spawnVariable -= 1;
                }

                if (spawnVariable == 0)
                    continue;

                float random = Random.Range(0.0f, 1.0f);

                if (random <= spawnVariable)
                {
                    SpawnObstacle(stageObstacleData.SelectionType, stageObstacleData.ShapeName, stageObstacleData.Category, stageObstacleData.AsteroidSize, stageObstacleData.Rotation(), columnFieldRange);
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
            }
        }

        private void SpawnObstacle(SELECTION_TYPE selectionType, string shapeName, string category, ASTEROID_SIZE asteroidSize, int numRotations, Vector2 gridRegion, bool inRandomYLevel = false)
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
                PlaceMovableOnGrid(newShape, gridRegion);
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
                PlaceMovableOnGrid(newShape, gridRegion);
                return;
            }
            else if (selectionType == SELECTION_TYPE.ASTEROID)
            {
                Bit newBit = FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateLargeAsteroid<Bit>(asteroidSize);
                AddMovableToList(newBit);

                int radiusAround = 0;
                switch(asteroidSize)
                {
                    case ASTEROID_SIZE.Small:
                    case ASTEROID_SIZE.Medium:
                        radiusAround = 1;
                        break;
                    case ASTEROID_SIZE.Large:
                        radiusAround = 1;
                        break;
                }  

                PlaceMovableOnGrid(newBit, gridRegion, radiusAround);

                return;
            }
            else if (selectionType == SELECTION_TYPE.BUMPER)
            {
                Bit newBit = FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateObject<Bit>(BIT_TYPE.WHITE, 0);
                AddMovableToList(newBit);

                PlaceMovableOnGrid(newBit, gridRegion);
                return;
            }
        }

        public void AddMovableToList(IObstacle movable)
        {
            //TODO: Find a more elegant solution for this if statement. This is catching the scenario where a bit is recycled and reused in the same frame, before it can be removed by the update loop, resulting in it being in the list twice.
            if (!m_obstacles.Contains(movable))
                m_obstacles.Add(movable);
        }

        private void PlaceMovableOnGrid(IObstacle movable, Vector2 gridRegion, int radius = 0)
        {
            Vector2 position = LevelManager.Instance.WorldGrid.GetLocalPositionOfRandomTopGridSquareInGridRegion(Constants.enemyGridScanRadius, gridRegion);
            movable.transform.parent = m_worldElementsRoot.transform;
            movable.transform.localPosition = position;
            switch (movable)
            {
                case Bit _:
                case Component _:
                    LevelManager.Instance.WorldGrid.SetObstacleInGridSquareAtLocalPosition(position, radius, true);
                    break;
                case Shape shape:
                    foreach (Bit bit in shape.AttachedBits)
                    {
                        Vector2Int gridPosition = LevelManager.Instance.WorldGrid.GetCoordinatesOfGridSquareAtLocalPosition
                            ((Vector2)bit.transform.localPosition + position);
                        if (gridPosition.y < Values.Globals.GridSizeY)
                        {
                            LevelManager.Instance.WorldGrid.SetObstacleInGridSquareAtLocalPosition((Vector2)bit.transform.localPosition + position, 0, true);
                        }
                    }
                    m_notFullyInGridShapes.Add(shape);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(movable), movable, null);
            }
        }

        private void PlaceMovableOnGridSpecific(IObstacle movable, Vector2 position, int radius = 0)
        {
            movable.transform.parent = m_worldElementsRoot.transform;
            movable.transform.localPosition = position;
            switch (movable)
            {
                case Bit _:
                case Component _:
                    LevelManager.Instance.WorldGrid.SetObstacleInGridSquareAtLocalPosition(position, radius, true);
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
            Vector2 endPosition = LevelManager.Instance.WorldGrid.GetLocalPositionOfCenterOfGridSquareAtCoordinates(gridEndPosition);
            PlaceMovableOffGrid(obstacle, startingPosition, endPosition, lerpSpeed, spinSpeed, despawnOnEnd, spinning, arc);
        }

        private void PlaceMovableOffGrid(IObstacle obstacle, Vector2 startingPosition, Vector2 endPosition, float lerpSpeed, float spinSpeed, bool despawnOnEnd, bool spinning, bool arc)
        {
            obstacle.SetColliderActive(false);
            obstacle.transform.parent = m_worldElementsRoot.transform;
            obstacle.transform.localPosition = startingPosition;

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

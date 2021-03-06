﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using StarSalvager.Values;
using StarSalvager.Factories;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.JsonDataTypes;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using StarSalvager.Cameras;
using StarSalvager.Prototype;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.Helpers;
using StarSalvager.Utilities.Interfaces;
using StarSalvager.Utilities.Saving;

namespace StarSalvager
{
    public class ObstacleManager : MonoBehaviour, IReset, IPausable
    {

        public static Action NewShapeOnScreen;

        public static float MOVE_DELTA { get; private set; }

        #region Properties

        private List<IObstacle> m_obstacles;
        private List<IObstacle> m_wallObstacles;
        private List<Shape> m_bonusShapes;
        private List<Shape> m_notFullyInGridShapes;
        private List<OffGridMovement> m_offGridMovingObstacles;

        public List<Asteroid> Asteroids { get; private set; }

        //Input Manager variables - -1.0f for left, 0 for nothing, 1.0f for right
        //private float m_currentInput;

        //Variables to spawn obstacles throughout a stage
        private StageRemoteData m_currentStageData;
        private StageRemoteData m_previousStageData;
        private float m_blendTimer;
        private int m_nextStageToSpawn;

        //private float m_distanceHorizontal;

        public Transform WorldElementsRoot => m_worldElementsRoot;
        private Transform m_worldElementsRoot;

        private float m_bonusShapeTimer;
        private int m_bonusShapesSpawned;

        public bool HasActiveBonusShapes => m_bonusShapes != null &&
                                            m_bonusShapes.Count > 0 &&
                                            m_bonusShapes.Any(ObstacleInCameraRect);

        public IEnumerable<Shape> ActiveBonusShapes => m_bonusShapes
            .Where(ObstacleInCameraRect);

        private static bool ObstacleInCameraRect(IObstacle obstacle)
        {
            return CameraController.IsPointInCameraRect(obstacle.transform.position, Constants.VISIBLE_GAME_AREA);
        }

        public bool isPaused => GameTimer.IsPaused;

        public bool AnyAttachableBitOnScreen => m_obstacles
            .OfType<Bit>()
            .Any(x => 
                x.IsRecycled == false && 
                x.Type != BIT_TYPE.BUMPER && 
                x.Attached == false &&
                ObstacleInCameraRect(x));

        #endregion //Properties

        //Unity Functions
        //====================================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            m_obstacles = new List<IObstacle>();
            m_wallObstacles = new List<IObstacle>();
            m_bonusShapes = new List<Shape>();
            m_notFullyInGridShapes = new List<Shape>();
            m_offGridMovingObstacles = new List<OffGridMovement>();

            Asteroids = new List<Asteroid>();

            RegisterPausable();
            m_worldElementsRoot = new GameObject("WorldElementRoot").transform;
            SceneManager.MoveGameObjectToScene(m_worldElementsRoot.gameObject, gameObject.scene);

            SetupStage(0);
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
                LevelManager.Instance.WorldGrid.MoveObstacleMarkersDownwardOnGrid(m_obstacles, m_currentStageData);
                if (GameManager.IsState(GameState.LevelActive))
                {
                    SpawnNewRowOfObstacles();
                    TryMarkNewShapesOnGrid();
                }
            }

            if (m_blendTimer < m_currentStageData.StageBlendPeriod)
            {
                m_blendTimer += Time.deltaTime;
            }

            if (GameManager.IsState(GameState.LevelActive) && LevelManager.Instance.CurrentStage == m_nextStageToSpawn)
            {
                SetupStage(m_nextStageToSpawn);
            }

            HandleObstacleMovement();

            if (GameManager.IsState(GameState.LevelActive))
            {
                TrySpawnBonusShape();
            }

            //Set the movement direction
            //THIS IS ESSENTIAL FOR THE BOT TO KNOW WHAT DIRECTION THE WORLD IS MOVING
            /*Globals.MovingDirection = Mathf.Abs(m_distanceHorizontal) <= 0.2f
                ? DIRECTION.NULL
                : m_distanceHorizontal.GetHorizontalDirection();*/
        }

        //====================================================================================================================//

        public void Activate()
        {
            //Spawn enemies from wave 0
            SetupStage(0);

            WorldElementsRoot.transform.position = Vector3.zero;
            if (m_currentStageData.StageType == STAGE_TYPE.STANDARD)
            {
                LevelManager.Instance.StandardBufferZoneObstacleData.PrespawnWalls(m_currentStageData, false, this);
                LevelManager.Instance.StandardBufferZoneObstacleData.PrespawnRows(m_currentStageData, false, this);
            }
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

                switch (obstacle)
                {
                    case Bit bit:
                        Recycler.Recycle<Bit>(bit);
                        break;
                    case Asteroid asteroid:
                        Recycler.Recycle<Asteroid>(asteroid);
                        break;
                    case GearCollectable component:
                        Recycler.Recycle<GearCollectable>(component);
                        break;
                    case Shape shape:
                        Recycler.Recycle<Shape>(shape, new
                        {
                            recycleBits = false
                        });
                        break;
                    case SpaceJunk spaceJunk:
                        Recycler.Recycle<SpaceJunk>(spaceJunk);
                        break;
                    case JunkBit junkBit:
                        Recycler.Recycle<JunkBit>(junkBit);
                        break;
                    case BlackHole blackHole:
                        Recycler.Recycle<BlackHole>(blackHole);
                        break;
                    case MoveWithObstacles _:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(obstacle), obstacle, null);
                }

                RemoveObstacleFromList(obstacle);
            }

            for (int i = m_wallObstacles.Count - 1; i >= 0; i--)
            {
                var obstacle = m_wallObstacles[i];
                if (obstacle == null)
                {
                    m_wallObstacles.RemoveAt(i);
                    continue;
                }

                switch (obstacle)
                {
                    case Bit bit:
                        Recycler.Recycle<Bit>(bit);
                        break;
                    case Asteroid asteroid:
                        Recycler.Recycle<Asteroid>(asteroid);
                        break;
                    case GearCollectable component:
                        Recycler.Recycle<GearCollectable>(component);
                        break;
                    case Shape shape:
                        Recycler.Recycle<Shape>(shape, new
                        {
                            recycleBits = false
                        });
                        break;
                    case SpaceJunk spaceJunk:
                        Recycler.Recycle<SpaceJunk>(spaceJunk);
                        break;
                    case JunkBit junkBit:
                        Recycler.Recycle<JunkBit>(junkBit);
                        break;
                    case BlackHole blackHole:
                        Recycler.Recycle<BlackHole>(blackHole);
                        break;
                    case MoveWithObstacles _:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(obstacle), obstacle, null);
                }

                RemoveObstacleFromList(obstacle);
            }

            if (m_offGridMovingObstacles.Count > 0)
            {
                for (int i = m_offGridMovingObstacles.Count - 1; i >= 0; i--)
                {
                    var obstacle = m_offGridMovingObstacles[i];
                    if (obstacle == null)
                    {
                        m_offGridMovingObstacles.RemoveAt(i);
                        continue;
                    }

                    switch (obstacle.Obstacle)
                    {
                        case Bit bit:
                            Recycler.Recycle<Bit>(bit);
                            break;
                        case Asteroid asteroid:
                            Recycler.Recycle<Asteroid>(asteroid);
                            break;
                        case GearCollectable component:
                            Recycler.Recycle<GearCollectable>(component);
                            break;
                        case Shape shape:
                            Recycler.Recycle<Shape>(shape);
                            break;
                        case SpaceJunk spaceJunk:
                            Recycler.Recycle<SpaceJunk>(spaceJunk);
                            break;
                        case JunkBit junkBit:
                            Recycler.Recycle<JunkBit>(junkBit);
                            break;
                        case BlackHole blackHole:
                            Recycler.Recycle<BlackHole>(blackHole);
                            break;
                        case MoveWithObstacles _:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(obstacle), obstacle, null);
                    }
                }
            }

            for (int i = m_notFullyInGridShapes.Count - 1; i >= 0; i--)
            {
                Recycler.Recycle<Shape>(m_notFullyInGridShapes[i].gameObject);
                m_notFullyInGridShapes.RemoveAt(i);
            }

            for (int i = m_bonusShapes.Count - 1; i >= 0; i--)
            {
                Recycler.Recycle<Shape>(m_bonusShapes[i].gameObject);
                m_bonusShapes.RemoveAt(i);
            }

            m_bonusShapes.Clear();
            previousShapesInLevel.Clear();
            m_offGridMovingObstacles.Clear();
            m_bonusShapesSpawned = 0;
            m_bonusShapeTimer = 0;

            Asteroids.Clear();
        }

        //====================================================================================================================//

        public Bit TryGetBitInColumn(in Vector2 position)
        {
            var xPos = position.x;
            var yPos = position.y;
            var bit = m_obstacles
                .OfType<Bit>()
                .Where(x => x.IsRecycled == false)
                .Where(x => x.Type != BIT_TYPE.BUMPER)
                .Where(x => x.Attached == false)
                .Where(x => Mathf.Abs(xPos - x.transform.position.x) < 0.5f)
                .Where(x => x.transform.position.y > yPos)
                .OrderBy(x => x.transform.position.y)
                .FirstOrDefault();

            return bit;
        }
        
        public List<Bit> TryGetBitsOnScreen()
        {
            var bits = m_obstacles
                .OfType<Bit>()
                .Where(x => x.IsRecycled == false)
                .Where(x => x.Type != BIT_TYPE.BUMPER)
                .Where(x => x.Attached == false)
                .Where(x => CameraController.IsPointInCameraRect(x.transform.position))
                .ToList();

            return bits;
        }
        

        public List<Bit> TryGetBumpersOnScreen()
        {
            var bits = m_obstacles
                .OfType<Bit>()
                .Where(x => x.IsRecycled == false)
                .Where(x => x.Type == BIT_TYPE.BUMPER)
                .Where(x => x.Attached == false)
                .Where(x => CameraController.IsPointInCameraRect(x.transform.position))
                .ToList();

            return bits;
        }
        //====================================================================================================================//
        
        private void HandleObstacleMovement()
        {
            Vector3 amountShift = Vector3.up *
                                  ((Constants.gridCellSize * Time.deltaTime) / Globals.TimeForAsteroidToFallOneSquare);

            //TryMoveElements();

            //Off grid moving obstacles are things like the previous bonus shapes or objects flying out from a loot drop, that the obstaclemanager manages but don't fall the normal amount
            for (int i = m_offGridMovingObstacles.Count - 1; i >= 0; i--)
            {
                if (m_offGridMovingObstacles[i].Obstacle is null)
                {
                    m_offGridMovingObstacles.RemoveAt(i);
                    continue;
                }
                
                m_offGridMovingObstacles[i].LerpTimer += Time.deltaTime / m_offGridMovingObstacles[i].LerpSpeed *
                                                         m_offGridMovingObstacles[i].SpeedUpModifier;

                if (m_offGridMovingObstacles[i].LerpTimer >= 1)
                {
                    switch (m_offGridMovingObstacles[i].Obstacle)
                    {
                        case Bit bit:
                            if (m_offGridMovingObstacles[i].DespawnOnEnd)
                            {
                                Recycler.Recycle<Bit>(bit);
                            }
                            else
                            {
                                //If its destination has something in it currently, it instead will extend its current movement pattern and go further
                                if (LevelManager.Instance.WorldGrid.GetGridSquareAtLocalPosition(m_offGridMovingObstacles[i].Obstacle.transform.position).ObstacleInSquare)
                                {
                                    m_offGridMovingObstacles[i].Extend();
                                    continue;
                                }

                                AddObstacleToList(m_offGridMovingObstacles[i].Obstacle);
                                PlaceMovableOnGridSpecific(bit, m_offGridMovingObstacles[i].EndPosition);
                                bit.SetColliderActive(true);
                            }

                            break;
                        case Asteroid asteroid:
                            if (m_offGridMovingObstacles[i].DespawnOnEnd)
                            {
                                Recycler.Recycle<Asteroid>(asteroid);
                            }
                            else
                            {
                                //If its destination has something in it currently, it instead will extend its current movement pattern and go further
                                if (LevelManager.Instance.WorldGrid.GetGridSquareAtLocalPosition(m_offGridMovingObstacles[i].Obstacle.transform.position).ObstacleInSquare)
                                {
                                    m_offGridMovingObstacles[i].Extend();
                                    continue;
                                }

                                AddObstacleToList(m_offGridMovingObstacles[i].Obstacle);
                                PlaceMovableOnGridSpecific(asteroid, m_offGridMovingObstacles[i].EndPosition);
                                asteroid.SetColliderActive(true);
                            }

                            break;
                        case GearCollectable component:
                            if (m_offGridMovingObstacles[i].DespawnOnEnd)
                            {
                                Recycler.Recycle<GearCollectable>(component);
                            }
                            else
                            {
                                //If its destination has something in it currently, it instead will extend its current movement pattern and go further
                                if (LevelManager.Instance.WorldGrid.GetGridSquareAtLocalPosition(m_offGridMovingObstacles[i].Obstacle.transform.position).ObstacleInSquare)
                                {
                                    m_offGridMovingObstacles[i].Extend();
                                    continue;
                                }

                                AddObstacleToList(m_offGridMovingObstacles[i].Obstacle);
                                PlaceMovableOnGridSpecific(component, m_offGridMovingObstacles[i].EndPosition);
                                component.SetColliderActive(true);
                            }

                            break;
                        case Shape shape:
                            if (m_bonusShapes.Contains(shape))
                            {
                                m_bonusShapes.Remove(shape);
                            }

                            if (m_offGridMovingObstacles[i].DespawnOnEnd)
                            {
                                Recycler.Recycle<Shape>(shape);
                            }
                            else
                            {
                                AddObstacleToList(m_offGridMovingObstacles[i].Obstacle);
                                PlaceMovableOnGridSpecific(shape, m_offGridMovingObstacles[i].EndPosition);
                                shape.SetColliderActive(true);
                            }

                            break;
                        case JunkBit junktBit:
                            if (m_offGridMovingObstacles[i].DespawnOnEnd)
                            {
                                Recycler.Recycle<JunkBit>(junktBit);
                            }
                            else
                            {
                                //If its destination has something in it currently, it instead will extend its current movement pattern and go further
                                if (LevelManager.Instance.WorldGrid.GetGridSquareAtLocalPosition(m_offGridMovingObstacles[i].Obstacle.transform.position).ObstacleInSquare)
                                {
                                    m_offGridMovingObstacles[i].Extend();
                                    continue;
                                }

                                AddObstacleToList(m_offGridMovingObstacles[i].Obstacle);
                                PlaceMovableOnGridSpecific(junktBit, m_offGridMovingObstacles[i].EndPosition);
                                junktBit.SetColliderActive(true);
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(OffGridMovement.Obstacle),
                                m_offGridMovingObstacles[i].Obstacle, null);
                    }

                    m_offGridMovingObstacles.RemoveAt(i);
                    continue;
                }

                if (m_offGridMovingObstacles[i].ParentToGrid)
                    m_offGridMovingObstacles[i].Move(-amountShift);
                else
                    m_offGridMovingObstacles[i].Move(Vector3.zero);


                m_offGridMovingObstacles[i].Spin();


                //Determines if a new bonus shape is now visible on screen, notifies those who care about the change
                //----------------------------------------------------------------------------------------------------//

                //if (!m_offGridMovingObstacles[i].isVisible &&
                //    m_offGridMovingObstacles[i].Obstacle is Shape checkShape &&
                //    m_bonusShapes.Contains(checkShape))
                //{
                //    if (ObstacleInCameraRect(checkShape))
                //    {
                //        m_offGridMovingObstacles[i].isVisible = true;
                //        NewShapeOnScreen?.Invoke();
//
                //        if(HintManager.CanShowHint(HINT.BONUS))
                //            HintManager.TryShowHint(HINT.BONUS, 1.35f);
                //    }
                //}

                //----------------------------------------------------------------------------------------------------//
            }

            //These are the regular falling obstacles
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

                if (obstacle is IAdditiveMove additiveMove)
                    pos += (Vector3)additiveMove.AddMove * Time.deltaTime;

                if (gridPosition.y < -4 * Constants.gridCellSize)
                {
                    var temp = m_obstacles[i];

                    switch (temp)
                    {
                        case Bit bit:
                            if (!bit.Attached)
                            {
                                bit.IsRegistered = false;
                                Recycler.Recycle<Bit>(bit);
                                m_obstacles[i] = null;
                            }

                            break;
                        case Asteroid asteroid:
                            asteroid.IsRegistered = false;
                            Recycler.Recycle<Asteroid>(asteroid);
                            m_obstacles[i] = null;
                            break;
                        case GearCollectable component:
                            component.IsRegistered = false;
                            Recycler.Recycle<GearCollectable>(component);
                            m_obstacles[i] = null;

                            break;
                        case Shape shape:
                            foreach (var attachedBit in shape.AttachedBits.Where(attachedBit =>
                                m_obstacles.Contains(attachedBit)))
                            {
                                attachedBit.IsRegistered = false;
                                m_obstacles[m_obstacles.IndexOf(attachedBit)] = null;
                            }

                            Recycler.Recycle<Shape>(shape);
                            m_obstacles[i].IsRegistered = false;
                            m_obstacles[i] = null;
                            break;
                        case SpaceJunk spaceJunk:
                            spaceJunk.IsRegistered = false;
                            Recycler.Recycle<SpaceJunk>(spaceJunk);
                            m_obstacles[i] = null;
                            break;
                        case MoveWithObstacles _:
                            break;
                        case JunkBit junkBit:
                            if (!junkBit.Attached)
                            {
                                junkBit.IsRegistered = false;
                                Recycler.Recycle<JunkBit>(junkBit);
                                m_obstacles[i] = null;
                            }

                            break;
                        case Crate crate:
                            if (!crate.Attached)
                            {
                                crate.IsRegistered = false;
                                Recycler.Recycle<Crate>(crate);
                                m_obstacles[i] = null;
                            }

                            break;
                        case BlackHole blackHole:
                            Recycler.Recycle<BlackHole>(blackHole);
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
                    rotate.transform.localRotation *=
                        Quaternion.Euler(0.0f, 0.0f, Time.deltaTime * 15.0f * rotate.RotateDirection);
                }
            }
        }

        public List<BlackHole> GetAllBlackHoles()
        {
            return new List<BlackHole>();
            
            List<BlackHole> blackHoles = new List<BlackHole>();

            for (int i = 0; i < m_obstacles.Count; i++)
            {
                if (m_obstacles[i] is BlackHole blackHole)
                {
                    blackHoles.Add(blackHole);
                }
            }

            for (int i = 0; i < m_offGridMovingObstacles.Count; i++)
            {
                if (m_obstacles[i] is BlackHole blackHole)
                {
                    blackHoles.Add(blackHole);
                }
            }

            return blackHoles;
        }

        public void IncreaseSpeedAllOffGridMoving(float speedModifier)
        {
            for (int i = 0; i < m_offGridMovingObstacles.Count; i++)
            {
                m_offGridMovingObstacles[i].SpeedUpModifier = speedModifier;
            }
        }

        //====================================================================================================================//


        private void TrySpawnBonusShape()
        {
            if (m_bonusShapesSpawned >= LevelManager.Instance.CurrentWaveData.BonusShapes.Count) 
                return;
            
            m_bonusShapeTimer += Time.deltaTime;
            
            if (!(m_bonusShapeTimer >= LevelManager.Instance.CurrentWaveData.BonusShapeFrequency)) 
                return;
            
            m_bonusShapeTimer -= LevelManager.Instance.CurrentWaveData.BonusShapeFrequency;
            
            var bonusObstacleShapeData = LevelManager.Instance.CurrentWaveData.BonusShapes[m_bonusShapesSpawned];

            if (!Globals.UsingTutorial && (LevelManager.Instance.CurrentWaveData.GetWaveDuration() <= LevelManager.Instance.WaveTimer + m_currentStageData.StageBlendPeriod))
                return;

            SpawnBonusShape(
                bonusObstacleShapeData.SelectionType,
                bonusObstacleShapeData.ShapeName,
                bonusObstacleShapeData.Category,
                bonusObstacleShapeData.Rotation);
            
            m_bonusShapesSpawned++;
        }

        //====================================================================================================================//
        

        public void MoveToNewWave()
        {
            SetupStage(0);
            m_bonusShapesSpawned = 0;
        }

        public void SetupStage(int stageNumber)
        {
            if (GameManager.IsState(GameState.LevelBotDead))
            {
                return;
            }

            if (stageNumber > 0)
                m_previousStageData = LevelManager.Instance.CurrentWaveData.GetRemoteData(stageNumber - 1);

            m_currentStageData = LevelManager.Instance.CurrentWaveData.GetRemoteData(stageNumber);
            m_nextStageToSpawn = stageNumber + 1;
            m_blendTimer = 0;

            CreateEdgeSprites();
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

        //================================================================================================================//

        public void AddTransformToRoot(Transform toReParent)
        {
            toReParent.SetParent(m_worldElementsRoot, true);
        }
        public void AddToRoot(GameObject gameObject)
        {
            gameObject.transform.SetParent(m_worldElementsRoot, true);
        }

        public void AddToRoot(MonoBehaviour monoBehaviour)
        {
            monoBehaviour.transform.SetParent(m_worldElementsRoot, true);
        }

        //This is called every time the grid has fallen enough to empty the top row
        private void SpawnNewRowOfObstacles()
        {
            if (isPaused)
                return;

            //TODO: Find a better approach. This line is causing the stageblendperiod on the last stage of a wave to prevent spawning for that last portion of the wave. Temporary approach to the waveendsequence.
            if (!Globals.UsingTutorial && (LevelManager.Instance.CurrentWaveData.GetWaveDuration() <= LevelManager.Instance.WaveTimer + m_currentStageData.StageBlendPeriod))
                return;

            switch (m_currentStageData.StageType)
            {
                case STAGE_TYPE.STANDARD:
                    LevelManager.Instance.StandardBufferZoneObstacleData.SetObstacleDataSpawns(m_currentStageData,
                        false, this);
                    break;
                case STAGE_TYPE.FULLSCREEN:
                    SpawnObstacleData(m_currentStageData.StageObstacleData, 
                        new Vector2(0, 1), 
                        false, 
                        true,
                        m_currentStageData.SpawningObstacleMultiplier, 
                        false);
                    break;
                case STAGE_TYPE.CUSTOM:
                    foreach (StageColumnGroupObstacleData stageColumnGroupObstacleData in m_currentStageData
                        .StageColumnGroupObstacleData)
                    {
                        Vector2 columnFieldRange = new Vector2(stageColumnGroupObstacleData.ColumnGroupMinimum,
                            stageColumnGroupObstacleData.ColumnGroupMaximum);
                        if (stageColumnGroupObstacleData.IsBlendZone)
                        {
                            IEnumerable<StageColumnGroupObstacleData> columnsLeft =
                                m_currentStageData.StageColumnGroupObstacleData.Where(s =>
                                    s.ColumnGroupMaximum <= columnFieldRange.x && !s.IsBlendZone);
                            IEnumerable<StageColumnGroupObstacleData> columnsRight =
                                m_currentStageData.StageColumnGroupObstacleData.Where(s =>
                                    s.ColumnGroupMinimum >= columnFieldRange.y && !s.IsBlendZone);
                            if (columnsLeft.Any() && columnsRight.Any())
                            {
                                float columnGroupLeftPosition = columnsLeft.Max(s => s.ColumnGroupMaximum);
                                StageColumnGroupObstacleData columnGroupLeft =
                                    m_currentStageData.StageColumnGroupObstacleData.FirstOrDefault(s =>
                                        s.ColumnGroupMaximum == columnGroupLeftPosition);
                                float columnGroupRightPosition = columnsRight.Min(s => s.ColumnGroupMinimum);
                                StageColumnGroupObstacleData columnGroupRight =
                                    m_currentStageData.StageColumnGroupObstacleData.FirstOrDefault(s =>
                                        s.ColumnGroupMinimum == columnGroupRightPosition);

                                if (columnGroupLeft != null && columnGroupRight != null)
                                {
                                    SpawnObstacleData(columnGroupLeft.StageObstacleData, columnFieldRange,
                                        stageColumnGroupObstacleData.m_allowOverlap,
                                        stageColumnGroupObstacleData.m_forceSpawn,
                                        m_currentStageData.SpawningObstacleMultiplier / 2, false);
                                    SpawnObstacleData(columnGroupRight.StageObstacleData, columnFieldRange,
                                        stageColumnGroupObstacleData.m_allowOverlap,
                                        stageColumnGroupObstacleData.m_forceSpawn,
                                        m_currentStageData.SpawningObstacleMultiplier / 2, false);
                                }
                            }
                        }
                        else
                        {
                            SpawnObstacleData(stageColumnGroupObstacleData.StageObstacleData, columnFieldRange,
                                stageColumnGroupObstacleData.m_allowOverlap, stageColumnGroupObstacleData.m_forceSpawn,
                                m_currentStageData.SpawningObstacleMultiplier, false);
                        }
                    }

                    break;
            }

            if (m_previousStageData == null || m_currentStageData.StageBlendPeriod == 0 ||
                m_blendTimer > m_currentStageData.StageBlendPeriod)
                return;

            switch (m_previousStageData.StageType)
            {
                case STAGE_TYPE.STANDARD:
                    LevelManager.Instance.StandardBufferZoneObstacleData.SetObstacleDataSpawns(m_previousStageData,
                        true, this);
                    break;
                case STAGE_TYPE.FULLSCREEN:
                    SpawnObstacleData(m_previousStageData.StageObstacleData, new Vector2(0, 1), false, true,
                        m_previousStageData.SpawningObstacleMultiplier, true);
                    break;
                case STAGE_TYPE.CUSTOM:
                    foreach (StageColumnGroupObstacleData stageColumnGroupObstacleData in m_previousStageData
                        .StageColumnGroupObstacleData)
                    {
                        Vector2 columnFieldRange = new Vector2(stageColumnGroupObstacleData.ColumnGroupMinimum,
                            stageColumnGroupObstacleData.ColumnGroupMaximum);
                        if (stageColumnGroupObstacleData.IsBlendZone)
                        {
                            IEnumerable<StageColumnGroupObstacleData> columnsLeft =
                                m_currentStageData.StageColumnGroupObstacleData.Where(s =>
                                    s.ColumnGroupMaximum <= columnFieldRange.x && !s.IsBlendZone);
                            IEnumerable<StageColumnGroupObstacleData> columnsRight =
                                m_currentStageData.StageColumnGroupObstacleData.Where(s =>
                                    s.ColumnGroupMinimum >= columnFieldRange.y && !s.IsBlendZone);
                            if (!columnsLeft.Any() || !columnsRight.Any())
                                continue;
                            {
                                float columnGroupLeftPosition = columnsLeft.Max(s => s.ColumnGroupMaximum);
                                StageColumnGroupObstacleData columnGroupLeft =
                                    m_previousStageData.StageColumnGroupObstacleData.FirstOrDefault(s =>
                                        s.ColumnGroupMaximum == columnGroupLeftPosition);
                                float columnGroupRightPosition = columnsRight.Min(s => s.ColumnGroupMinimum);
                                StageColumnGroupObstacleData columnGroupRight =
                                    m_previousStageData.StageColumnGroupObstacleData.FirstOrDefault(s =>
                                        s.ColumnGroupMinimum == columnGroupRightPosition);

                                if (columnGroupLeft != null && columnGroupRight != null)
                                {
                                    SpawnObstacleData(columnGroupLeft.StageObstacleData, columnFieldRange,
                                        stageColumnGroupObstacleData.m_allowOverlap,
                                        stageColumnGroupObstacleData.m_forceSpawn,
                                        m_previousStageData.SpawningObstacleMultiplier / 2, true);
                                    SpawnObstacleData(columnGroupRight.StageObstacleData, columnFieldRange,
                                        stageColumnGroupObstacleData.m_allowOverlap,
                                        stageColumnGroupObstacleData.m_forceSpawn,
                                        m_previousStageData.SpawningObstacleMultiplier / 2, true);
                                }
                            }
                        }
                        else
                        {
                            SpawnObstacleData(stageColumnGroupObstacleData.StageObstacleData, columnFieldRange,
                                stageColumnGroupObstacleData.m_allowOverlap, stageColumnGroupObstacleData.m_forceSpawn,
                                m_previousStageData.SpawningObstacleMultiplier, true);
                        }
                    }

                    break;
            }
        }

        public void SpawnObstacleData(List<StageObstacleData> obstacleData, Vector2 columnFieldRange, bool allowOverlap,
            bool forceSpawn, float spawningMultiplier, bool isPrevious, int yLevel = -1)
        {
            //If we're not explicitely setting the y Pos, fallback to the default
            if (yLevel < 0) yLevel = Globals.GridSizeY - 1;
            
            foreach (StageObstacleData stageObstacleData in obstacleData)
            {
                //This spawnVariable defines "how much of this thing should be spawned"
                float spawnVariable = stageObstacleData.Density() * spawningMultiplier * ((columnFieldRange.y - columnFieldRange.x) * Globals.GridSizeX);

                if (m_currentStageData.StageBlendPeriod > 0)
                {
                    if (isPrevious)
                    {
                        spawnVariable *= Mathf.Lerp(1, 0, m_blendTimer / m_currentStageData.StageBlendPeriod);
                    }
                    else if (m_previousStageData != null && m_blendTimer <= m_currentStageData.StageBlendPeriod)
                    {
                        spawnVariable *= Mathf.Lerp(0, 1, m_blendTimer / m_currentStageData.StageBlendPeriod);
                    }
                }
                else if (isPrevious)
                {
                    return;
                }

                while (spawnVariable >= 1)
                {
                    SpawnObstacle(stageObstacleData.SelectionType, stageObstacleData.ShapeName,
                        stageObstacleData.Category, stageObstacleData.AsteroidSize, stageObstacleData.Rotation,
                        columnFieldRange, allowOverlap, forceSpawn, yLevel);
                    spawnVariable -= 1;
                }

                if (spawnVariable == 0)
                    continue;

                float random = Random.value;

                if (random <= spawnVariable)
                {
                    SpawnObstacle(stageObstacleData.SelectionType, stageObstacleData.ShapeName,
                        stageObstacleData.Category, stageObstacleData.AsteroidSize, stageObstacleData.Rotation,
                        columnFieldRange, allowOverlap, forceSpawn, yLevel);
                }
            }
        }

        public void SpawnObstacleExplosion(Vector2 startingLocation, List<IRDSObject> rdsObjects, bool isFromEnemyLoot)
        {
            var spawnedObstacles = new List<IObstacle>();

            for (int i = 0; i < rdsObjects.Count; i++)
            {
                if (rdsObjects[i] is RDSValue<IBlockData> rdsValueBlockData)
                {
                    int count = rdsValueBlockData.GetCount();
                    for (int k = 0; k < count; k++)
                    {
                        switch (rdsValueBlockData.rdsValue)
                        {
                            case BitData bitData:
                                Bit newBit = FactoryManager.Instance.GetFactory<BitAttachableFactory>()
                                    .CreateObject<Bit>((BIT_TYPE) bitData.Type, bitData.Level);
                                //AddObstacleToList(newBit);
                                /*PlaceMovableOffGrid(newBit, startingLocation, bitExplosionPositions[0], 0.5f);
                                bitExplosionPositions.RemoveAt(0);*/
                                newBit.toBeCollected = isFromEnemyLoot;
                                spawnedObstacles.Add(newBit);
                                break;
                            /*case nameof(Asteroid):
                                Asteroid newAsteroid = FactoryManager.Instance.GetFactory<AsteroidFactory>()
                                    .CreateAsteroidRandom<Asteroid>();
                                //AddObstacleToList(newAsteroid);
                                /*PlaceMovableOffGrid(newAsteroid, startingLocation, bitExplosionPositions[0], 0.5f);
                                bitExplosionPositions.RemoveAt(0);#1#
                                _obstacles.Add(newAsteroid);
                                break;*/
                            default:
                                Debug.LogError(rdsValueBlockData.rdsValue.ClassType +
                                               " in SpawnBitExplosion and not handled");
                                break;
                        }
                    }
                }
                else if (rdsObjects[i] is RDSValue<ASTEROID_SIZE> rdsValueAsteroidSize)
                {
                    int count = rdsValueAsteroidSize.GetCount();
                    for (int k = 0; k < count; k++)
                    {
                        Asteroid newAsteroid = FactoryManager.Instance.GetFactory<AsteroidFactory>()
                            .CreateAsteroid<Asteroid>(rdsValueAsteroidSize.rdsValue);
                        /*PlaceMovableOffGrid(newAsteroid, startingLocation, bitExplosionPositions[0], 0.5f);
                        bitExplosionPositions.RemoveAt(0);*/
                        spawnedObstacles.Add(newAsteroid);
                    }
                }
                else if (rdsObjects[i] is RDSValue<int> rdsValueGearsAmount)
                {
                    var multiplier = PlayerDataManager.GetCurrentUpgradeValue(UPGRADE_TYPE.GEAR_DROP);
                    var count = rdsValueGearsAmount.GetCount();
                    for (var k = 0; k < count; k++)
                    {
                        var gears = Mathf.RoundToInt(rdsValueGearsAmount.rdsValue * multiplier);
                        var newComponent = FactoryManager.Instance.GetFactory<ComponentFactory>()
                            .CreateObject<GearCollectable>(gears);

                        spawnedObstacles.Add(newComponent);
                    }

                    if (count > 0 && HintManager.CanShowHint(HINT.GEARS))
                    {
                        var gearBounds = spawnedObstacles.OfType<IHasBounds>().FirstOrDefault();
                        
                        HintManager.TryShowHint(HINT. GEARS, 0.75f, () =>
                        {
                            var gears = m_obstacles.OfType<GearCollectable>().ToArray();

                            foreach (var gearCollectable in gears)
                            {
                                gearCollectable.SimpleAnimator.RestartAnimation();
                            }
                        }, gearBounds);
                    }
                }
                else
                {
                    Debug.LogError($"{rdsObjects[i]} in SpawnBitExplosion and not handled");
                }
            }


            var explosionPositions =
                LevelManager.Instance.WorldGrid.SelectBitExplosionPositions(startingLocation, spawnedObstacles.Count, 15, 6);

            for (var i = spawnedObstacles.Count - 1; i >= 0; i--)
            {
                PlaceMovableOffGrid(spawnedObstacles[i], startingLocation, explosionPositions[i], 0.5f);
            }

        }

        private void SpawnObstacle(SELECTION_TYPE selectionType, 
            string shapeName, 
            string category,
            ASTEROID_SIZE asteroidSize, 
            int numRotations, 
            Vector2 gridRegion, 
            bool allowOverlap, 
            bool forceSpawn,
            int yLevel)
        {
            IObstacle obstacle;
            int radiusAround = 0;

            switch (selectionType)
            {
                case SELECTION_TYPE.CATEGORY:
                {
                    IObstacle newObstacle = FactoryManager.Instance.GetFactory<ShapeFactory>()
                        .CreateObject<IObstacle>(selectionType, category, numRotations);

                    if (LevelManager.Instance != null)
                        LevelManager.Instance.ObstacleManager.AddObstacleToList(newObstacle);

                    AddObstacleToList(newObstacle);

                    obstacle = newObstacle;
                    break;
                }
                case SELECTION_TYPE.SHAPE:
                {
                    IObstacle newObstacle = FactoryManager.Instance.GetFactory<ShapeFactory>()
                        .CreateObject<IObstacle>(selectionType, shapeName, numRotations);

                    if (LevelManager.Instance != null)
                        LevelManager.Instance.ObstacleManager.AddObstacleToList(newObstacle);

                    AddObstacleToList(newObstacle);
                    
                    obstacle = newObstacle;
                    break;
                }
                case SELECTION_TYPE.ASTEROID:
                {
                    Asteroid newAsteroid = FactoryManager.Instance.GetFactory<AsteroidFactory>()
                        .CreateAsteroid<Asteroid>(asteroidSize);
                    AddObstacleToList(newAsteroid);

                    switch (asteroidSize)
                    {
                        case ASTEROID_SIZE.Bit:
                            break;
                        case ASTEROID_SIZE.Small:
                        case ASTEROID_SIZE.Medium:
                        case ASTEROID_SIZE.Large:
                            radiusAround = 1;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(asteroidSize), asteroidSize, null);
                    }

                    Asteroids.Add(newAsteroid);
                    obstacle = newAsteroid;
                    break;
                }
                case SELECTION_TYPE.BUMPER:
                    Bit newBit = FactoryManager.Instance.GetFactory<BitAttachableFactory>()
                        .CreateObject<Bit>(BIT_TYPE.BUMPER, 0);
                    AddObstacleToList(newBit);

                    obstacle = newBit;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selectionType), selectionType, null);
            }

            PlaceMovableOnGrid(obstacle, gridRegion, allowOverlap, forceSpawn, yLevel, radiusAround);
        }

        public void AddObstacleToListAndParentToWorldRoot(IObstacle obstacle)
        {
            obstacle.transform.parent = WorldElementsRoot.transform;
            AddObstacleToList(obstacle);
        }

        public void AddObstacleToList(IObstacle obstacle)
        {
            //TODO: Find a more elegant solution for this if statement. This is catching the scenario where a bit is recycled and reused in the same frame, before it can be removed by the update loop, resulting in it being in the list twice.
            if (obstacle.IsRegistered)
                return;

            m_obstacles.Add(obstacle);
            if (obstacle is Shape newShape)
            {
                foreach (Bit bit in newShape.AttachedBits)
                {
                    AddObstacleToList(bit);
                }
            }

            obstacle.IsRegistered = true;
        }

        public void ForceRemoveObstacleFromList(IObstacle obstacle)
        {
            RemoveObstacleFromList(obstacle);
        }

        private void RemoveObstacleFromList(IObstacle obstacle)
        {
            //TODO: Find a more elegant solution for this if statement. This is catching the scenario where a bit is recycled and reused in the same frame, before it can be removed by the update loop, resulting in it being in the list twice.
            if (obstacle != null)
            {
                m_obstacles.Remove(obstacle);
                obstacle.IsRegistered = false;
            }
            else
            {
                Debug.LogError("RemoveObstacleFromList received null value");
            }
        }

        //This finds the proper position for the new obstacle
        private void PlaceMovableOnGrid(IObstacle obstacle, Vector2 gridRegion, bool allowOverlap, bool forceSpawn,
            int yLevel, int radius = 0)
        {
            var minScanRadius = obstacle is Bit ? 0 : 1;

            Vector2? positionNullable = LevelManager.Instance.WorldGrid.GetLocalPositionOfRandomGridSquareInGridRegion(
                Constants.gridPositionSpacing, minScanRadius, gridRegion, allowOverlap, forceSpawn, yLevel);
            if (!positionNullable.HasValue)
            {
                switch (obstacle)
                {
                    case Bit bit:
                        Recycler.Recycle<Bit>(bit);
                        break;
                    case Asteroid asteroid:
                        Recycler.Recycle<Asteroid>(asteroid);
                        break;
                    case GearCollectable component:
                        Recycler.Recycle<GearCollectable>(component);
                        break;
                    case Shape shape:
                        Recycler.Recycle<Shape>(shape);
                        break;
                    case BlackHole blackHole:
                        Recycler.Recycle<BlackHole>(blackHole);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(obstacle), obstacle, null);
                }

                return;
            }

            Vector2 position = positionNullable.Value;
            obstacle.transform.parent = m_worldElementsRoot;
            obstacle.transform.localPosition = position;
            obstacle.transform.rotation = Quaternion.identity;
            switch (obstacle)
            {
                case Bit _:
                case Asteroid _:
                case GearCollectable _:
                case SpaceJunk _:
                case BlackHole _:
                    LevelManager.Instance.WorldGrid.SetObstacleInGridSquareAtLocalPosition(position, radius, true);
                    break;
                case Shape shape:
                    foreach (Bit bit in shape.AttachedBits)
                    {
                        Vector2Int gridPosition =
                            LevelManager.Instance.WorldGrid.GetCoordinatesOfGridSquareAtLocalPosition
                                ((Vector2) bit.transform.localPosition + position);
                        if (gridPosition.y < Values.Globals.GridSizeY)
                        {
                            LevelManager.Instance.WorldGrid.SetObstacleInGridSquareAtLocalPosition(
                                (Vector2) bit.transform.localPosition + position, 0, true);
                        }
                    }

                    m_notFullyInGridShapes.Add(shape);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(obstacle), obstacle, null);
            }
        }

        //This is used to place a moveable on a specific grid position, instead of randomly choosing one
        private void PlaceMovableOnGridSpecific(IObstacle obstacle, Vector2 position, int radius = 0)
        {
            obstacle.transform.parent = m_worldElementsRoot;
            obstacle.transform.localPosition = position;
            obstacle.transform.rotation = Quaternion.identity;
            switch (obstacle)
            {
                case Bit _:
                case Asteroid _:
                case GearCollectable _:
                case JunkBit _:
                    LevelManager.Instance.WorldGrid.SetObstacleInGridSquareAtLocalPosition(position, radius, true);
                    break;
                case Shape shape:
                    m_notFullyInGridShapes.Add(shape);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(obstacle), obstacle, null);
            }
        }

        private float m_bounceTravelDistance = 20.0f;
        private float m_bounceSpeedAdjustment = 1.0f;

        public void BounceObstacle(IObstacle obstacle, Vector2 direction, float spinSpeed, bool despawnOnEnd, bool spinning,
            bool arc)
        {
            bool isBumper = obstacle is Bit bit && bit.Type == BIT_TYPE.BUMPER;
            
            //RemoveObstacleFromList(obstacle);
            float randomFactor = Random.Range(0.75f, 1.25f);
            float bounceTravelDistance = m_bounceTravelDistance * randomFactor;
            float bounceSpeedAdjustment = 1.0f;

            if (isBumper)
            {
                bounceTravelDistance *= 5;
                bounceSpeedAdjustment /= 5;
                despawnOnEnd = true;
            }

            var localPosition = obstacle.transform.localPosition;
            Vector2 destination = (Vector2) localPosition + direction * bounceTravelDistance;
            float distance = Vector2.Distance(obstacle.transform.localPosition, destination);
            destination = LevelManager.Instance.WorldGrid.GetLocalPositionOfCenterOfGridSquareAtLocalPosition(destination);
            float newDistance = Vector2.Distance(obstacle.transform.localPosition, destination);
            spinSpeed *= (distance / newDistance);

            PlaceMovableOffGrid(obstacle, localPosition, destination,
                Vector2.Distance(localPosition, destination) /
                (bounceTravelDistance * bounceSpeedAdjustment), spinSpeed, despawnOnEnd, spinning, arc);
        }

        //Place an obstacle on an off grid moving path, with specifying a grid position instead of world position as the end destination
        private void PlaceMovableOffGrid(IObstacle obstacle, Vector3 startingPosition, Vector2Int gridEndPosition,
            float lerpSpeed, float spinSpeed = 0.0f, bool despawnOnEnd = false, bool spinning = false, bool arc = false,
            bool parentToGrid = true)
        {
            Vector3 endPosition =
                LevelManager.Instance.WorldGrid.GetLocalPositionOfCenterOfGridSquareAtCoordinates(gridEndPosition);
            PlaceMovableOffGrid(obstacle, startingPosition, endPosition, lerpSpeed, spinSpeed, despawnOnEnd, spinning,
                arc, parentToGrid);
        }

        //Place an obstacle on an off grid moving path, with a world position as the destination
        private void PlaceMovableOffGrid(IObstacle obstacle, Vector3 startingPosition, Vector3 endPosition,
            float lerpSpeed, float spinSpeed = 0.0f, bool despawnOnEnd = false, bool spinning = false, bool arc = false,
            bool parentToGrid = true)
        {
            obstacle.SetColliderActive(false);
            if (parentToGrid)
                obstacle.transform.parent = m_worldElementsRoot;
            obstacle.transform.localPosition = startingPosition;

            if (!arc)
                m_offGridMovingObstacles.Add(new OffGridMovementLerp(obstacle, startingPosition, endPosition, lerpSpeed,
                    spinSpeed, despawnOnEnd, spinning, parentToGrid));
            else
                m_offGridMovingObstacles.Add(new OffGridMovementArc(obstacle, startingPosition, Vector2.down * 25,
                    endPosition, lerpSpeed, spinSpeed, despawnOnEnd, spinning, parentToGrid));
        }

        //Bonus Shapes
        //====================================================================================================================//

        #region Bonus Shapes

        List<List<BitData>> previousShapesInLevel = new List<List<BitData>>();

        private void SpawnBonusShape(SELECTION_TYPE selectionType, string shapeName, string category, int numRotations)
        {
            IObstacle newObstacle;
            switch (selectionType)
            {
                case SELECTION_TYPE.CATEGORY:
                    newObstacle = FactoryManager.Instance.GetFactory<ShapeFactory>()
                        .CreateObject<IObstacle>(selectionType, category, numRotations, previousShapesInLevel, LevelManager.Instance.CurrentWaveData.GetBitTypesInWave());
                    break;

                case SELECTION_TYPE.SHAPE:
                    newObstacle = FactoryManager.Instance.GetFactory<ShapeFactory>()
                        .CreateObject<IObstacle>(selectionType, shapeName, numRotations, null, LevelManager.Instance.CurrentWaveData.GetBitTypesInWave());
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(selectionType), selectionType, null);
            }

            if (newObstacle is CollidableBase collidableBase)
                collidableBase.SetSortingLayer(LayerHelper.OVERLAY, 100);

            if (newObstacle is Shape shape)
            {
                List<BitData> newObstacleData = new List<BitData>();
                for (int i = 0; i < shape.AttachedBits.Count; i++)
                {
                    newObstacleData.Add(shape.AttachedBits[i].ToBlockData());
                }
                previousShapesInLevel.Add(newObstacleData);
                shape.FlashBits();
            }

            newObstacle.gameObject.name += "_BonusShape";

            PlaceBonusShapeInLevel(newObstacle);
        }

        private void PlaceBonusShapeInLevel(IObstacle obstacle)
        {
            int tryFlipSides = Random.Range(0, 2) * 2 - 1;

            float screenOffset = Globals.ColumnsOnScreen * Constants.gridCellSize * 0.35f;
            //float height = Camera.main.orthographicSize * 0.5f;
            float height = Constants.gridCellSize * Random.Range(6.0f, 8.0f);

            Vector3 startingPosition = new Vector3(screenOffset * tryFlipSides, height, 11);
            Vector3 endPosition = new Vector3(screenOffset * -tryFlipSides, height, 11);

            obstacle.SetColliderActive(false);
            obstacle.transform.parent = LevelManager.Instance.CameraController.transform;
            obstacle.transform.localPosition = startingPosition;

            PlaceMovableOffGrid(obstacle, startingPosition, endPosition, Globals.BonusShapeDuration, despawnOnEnd: true,
                parentToGrid: false);
            m_bonusShapes.Add((Shape) obstacle);
            LevelManager.Instance.WaveEndSummaryData.NumTotalBonusShapesSpawned++;
        }

        public void MatchBonusShape(Shape shape)
        {
            if (!m_bonusShapes.Contains(shape))
            {
                return;
            }

            m_bonusShapes.Remove(shape);
            m_notFullyInGridShapes.Remove(shape);
            m_offGridMovingObstacles.Remove(m_offGridMovingObstacles.FirstOrDefault(s =>
                s.Obstacle is Shape offGridShape && offGridShape == shape));
            Recycler.Recycle<Shape>(shape);
            LevelManager.Instance.WaveEndSummaryData.NumBonusShapesMatched++;
        }

        #endregion //Bonus Shapes


        //====================================================================================================================//

        public CollidableBase GetClosestDestructableCollidable(Vector2 position, float range)
        {
            var shortestDist = 999f;
            CollidableBase closestDestructableObstacle = null;
            foreach (var obstacle in m_obstacles)
            {
                if (obstacle == null)
                    continue;

                if (!(obstacle is CollidableBase collidableBase))
                {
                    continue;
                }

                if (!(obstacle is SpaceJunk))
                {
                    continue;
                }

                if (!CameraController.IsPointInCameraRect(collidableBase.transform.position))
                    continue;

                var dist = Vector2.Distance(position, collidableBase.transform.position);

                if (dist > range)
                    continue;
                if (dist > shortestDist)
                    continue;

                shortestDist = dist;
                closestDestructableObstacle = collidableBase;
            }

            return closestDestructableObstacle;
        }

        //====================================================================================================================//

        [Required] public Sprite edgeSprite;

        public Color edgeSpriteColor;
        private SpriteRenderer[] _edgeSprites;

        private void CreateEdgeSprites()
        {
            const int X_SCALE = 55;
            //TODO Create the sprite Objects
            if (_edgeSprites == null || _edgeSprites.Length == 0)
            {
                _edgeSprites = new SpriteRenderer[2];

                for (var i = 0; i < 2; i++)
                {
                    var temp = new GameObject($"EdgeSprite_{i}").AddComponent<SpriteRenderer>();
                    temp.sprite = edgeSprite;
                    temp.color = edgeSpriteColor;
                    temp.sortingLayerName = LayerHelper.DEFAULT;
                    temp.sortingOrder = 0;
                    _edgeSprites[i] = temp;

                    temp.transform.SetParent(WorldElementsRoot);
                }
            }

            //TODO Get Grid Size
            var gridSize = new Vector2Int
            {
                x = Globals.GridSizeX,
                y = Globals.GridSizeY
            };
            //TODO Get Cell Size
            var cellSize = Constants.gridCellSize;
            var xOffset = (gridSize.x * cellSize) / 2f;
            var orthoSize = CameraController.Camera.orthographicSize;


            //TODO Place on either side of the
            for (var i = 0; i < 2; i++)
            {
                var isLeft = i == 0;

                _edgeSprites[i].flipX = !isLeft;

                var trans = _edgeSprites[i].transform;
                var xPos = xOffset + orthoSize - X_SCALE / 2f;

                var yPos = gridSize.y / 3f;
                trans.localPosition = new Vector3
                {
                    x = xPos * (isLeft ? -1f : 1f),
                    y = yPos,
                    z = 0f
                };

                trans.localScale = new Vector3
                {
                    x = X_SCALE,
                    y = gridSize.y,
                    z = 1f
                };
            }
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

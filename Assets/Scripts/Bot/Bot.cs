using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Audio;
using StarSalvager.Cameras;
using StarSalvager.Values;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Prototype;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Puzzle;
using UnityEngine;
using GameUI = StarSalvager.UI.GameUI;
using StarSalvager.Utilities;
using StarSalvager.Missions;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Math;
using StarSalvager.Utilities.Particles;
using AudioController = StarSalvager.Audio.AudioController;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Interfaces;
using Input = UnityEngine.Input;
using Random = UnityEngine.Random;

namespace StarSalvager
{
    [RequireComponent(typeof(BotPartsLogic))]
    public class Bot : MonoBehaviour, ICustomRecycle, IRecycled, ICanBeHit, IPausable, ISetSpriteLayer, IMoveOnInput, IHasBounds
    {
        private readonly struct ShiftData
        {
            public readonly IAttachable Target;
            public readonly Vector2Int TargetCoordinate;

            public ShiftData(IAttachable target, Vector2Int targetCoordinate)
            {
                Target = target;
                TargetCoordinate = targetCoordinate;
            }
        }
        
        private struct WeldData
        {
            public IAttachable target;
            public IAttachable attachedTo;

            public DIRECTION Direction => target == null || attachedTo == null
                ? DIRECTION.NULL
                : (target.Coordinate - attachedTo.Coordinate).ToDirection();
        }
        
        public static Action<Bot, string> OnBotDied;

        public Action OnCombo;
        public Action OnFullMagnet;
        public Action OnBitShift;

        [BoxGroup("Smoke Particles")]
        public ParticleSystem TEST_ParticleSystem;
        [BoxGroup("Smoke Particles")]
        public ParticleSystemForceField TEST_ParticleSystemForceField;

        //============================================================================================================//
        
        public bool IsRecycled { get; set; }
        
        public bool isPaused => GameTimer.IsPaused;

        //====================================================================================================================//
        [SerializeField, BoxGroup("PROTOTYPE")]
        private bool PROTO_autoRefineFuel = true;
        //[SerializeField, Range(0.1f, 2f), BoxGroup("PROTOTYPE"), SuffixLabel("Sec", true)]
        //public float TEST_MergeTime = 0.6f;
        
        [SerializeField, BoxGroup("PROTOTYPE/Magnet")]
        public float TEST_DetachTime = 1f;
        [SerializeField, BoxGroup("PROTOTYPE/Magnet")]
        public bool TEST_SetDetachColor = true;
        
        //[SerializeField, BoxGroup("PROTOTYPE")]
        //public bool PROTO_GodMode;
        
        //============================================================================================================//

        public List<IAttachable> attachedBlocks => _attachedBlocks ?? (_attachedBlocks = new List<IAttachable>());

        [SerializeField, ReadOnly, Space(10f), ShowInInspector] 
        private List<IAttachable> _attachedBlocks;

        //Input Manager variables - -1.0f for left, 0 for nothing, 1.0f for right
        private float m_currentInput;
        private float m_distanceHorizontal = 0.0f;

        //============================================================================================================//

        public bool IsInvulnerable
        {
            get => !CanBeDamaged && !CanUseResources;
            set => CanUseResources = CanBeDamaged = !value;
        }
        
        [ShowInInspector, ReadOnly]
        public bool CanBeDamaged { get; set; }
        [ShowInInspector, ReadOnly]
        public bool CanUseResources { get; set; }
        

        //====================================================================================================================//
        
        public bool Destroyed => _isDestroyed;
        private bool _isDestroyed;

        public bool HasFullMagnet => IsMagnetFull();

        private Vector2 targetPosition;
        private float _currentInput;

        public bool Rotating => _rotating;
        public ROTATION MostRecentRotate;

        private bool _rotating;
        private float targetRotation;

        private bool _needToCheckMagnet;

        private List<WeldData> _weldDatas;


        //====================================================================================================================//

        private GameObject _lastGearText;
        private int _combosMade;

        //============================================================================================================//

        public BotPartsLogic BotPartsLogic
        {
            get
            {
                if (_botPartsLogic == null)
                    _botPartsLogic = GetComponent<BotPartsLogic>();
                
                return _botPartsLogic;
            }
        }
        private BotPartsLogic _botPartsLogic;

        public Collider2D Collider => CompositeCollider2D;
        private CompositeCollider2D CompositeCollider2D
        {
            get
            {
                if (!_compositeCollider2D)
                    _compositeCollider2D = GetComponent<CompositeCollider2D>();
                
                return _compositeCollider2D;
            }
        }
        private CompositeCollider2D _compositeCollider2D;

        private new Rigidbody2D rigidbody
        {
            get
            {
                if (!_rigidbody)
                    _rigidbody = GetComponent<Rigidbody2D>();
                
                return _rigidbody;
            }
        }
        private Rigidbody2D _rigidbody;
        
        public new Transform transform
        {
            get
            {
                if (_transform == null)
                    _transform = gameObject.transform;
                
                return _transform;
            }
        }
        private Transform _transform;

        public CinemachineImpulseSource cinemachineImpulseSource
        {
            get
            {
                if (_cinemachineImpulseSource == null)
                    _cinemachineImpulseSource = gameObject.GetComponent<CinemachineImpulseSource>();

                return _cinemachineImpulseSource;
            }
        }
        private CinemachineImpulseSource _cinemachineImpulseSource;

        private GameUI GameUi => GameUI.Instance;

        private float previousDirection;
        private bool isContinuousRotation;

        //Particle Tests
        //====================================================================================================================//

        private void SetParticles()
        {
            if (Destroyed)
            {
                TEST_ParticleSystem.Stop();
                return;
            }
            
            //This should be the core
            if (!(attachedBlocks[0] is IHealth iHealth))
                return;

            var health = iHealth.CurrentHealth / iHealth.StartingHealth;
            
            if(health < 0.25f && !TEST_ParticleSystem.isPlaying)
                TEST_ParticleSystem.Play();
            else if (health >= 1f)
            {
                TEST_ParticleSystem.Stop();
                return;
            }


            //FIXME This is only here as a proof of concept
            switch (Globals.MovingDirection)
            {
                case DIRECTION.NULL:
                    TEST_ParticleSystemForceField.directionX = new ParticleSystem.MinMaxCurve(0f);
                    break;
                case DIRECTION.LEFT:
                    TEST_ParticleSystemForceField.directionX = new ParticleSystem.MinMaxCurve(10f);
                    break;
                case DIRECTION.RIGHT:
                    TEST_ParticleSystemForceField.directionX = new ParticleSystem.MinMaxCurve(-10f);
                    break;
            }
            
            
            //var emission = TEST_ParticleSystem.sizeOverLifetime;
            //emission.sizeMultiplier = 1f - healthValue;
        }
        
        //============================================================================================================//

        #region Unity Functions

        private void Start()
        {
            RegisterPausable();
            RegisterMoveOnInput();
        }
        

        private void Update()
        {
            if (isPaused)
                return;

            
            SetParticles();
            
            //See if the bot has completed the current wave
            //FIXME I Don't like accessing the external value here. I should consider other ways of checking this value
            if (GameManager.IsState(GameState.LevelEndWave))
                return;
            
            if (Destroyed)
                return;
            
            TryMovement();

            if (Rotating)
            {
                RotateBot();
            }
            
            //TODO Once all done testing, remove this
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftShift))
            {
                Time.timeScale = Time.timeScale == 0.1f ? 1f : 0.1f;
            }
            
            BotPartsLogic.PartsUpdateLoop();

            if (m_currentInput != 0.0f && Mathf.Abs(m_distanceHorizontal) <= 0.2f)
            {
                Move(m_currentInput);
            }
        }

        private void LateUpdate()
        {
            if (_needToCheckMagnet)
            {

                if (IsMagnetFull())
                    OnFullMagnet?.Invoke();

                AudioController.PlaySound(CheckHasMagnetOverage() ? SOUND.BIT_RELEASE : SOUND.BIT_SNAP);
                _needToCheckMagnet = false;
            }

            if (!_weldDatas.IsNullOrEmpty())
            {
                ShouldShowEffect(ref _weldDatas);
            }
        }

        private void OnEnable()
        {
            CompositeCollider2D.GenerateGeometry();
        }

        #endregion //Unity Functions

        //IMoveOnInput
        //============================================================================================================//

        #region IMoveOnInput Functions

        private void TryMovement()
        {
            var xPos = transform.position.x;

            var distHorizontal = Mathf.Abs(m_distanceHorizontal);
            DIRECTION direction;


            bool canMove;
            if (m_distanceHorizontal < 0)
            {
                direction = DIRECTION.LEFT;
                canMove = xPos > -0.5f * Constants.gridCellSize * Globals.GridSizeX;
            }
            else if (m_distanceHorizontal > 0)
            {
                direction = DIRECTION.RIGHT;
                canMove = xPos < 0.5f * Constants.gridCellSize * Globals.GridSizeX;
            }
            else
            {
                canMove = false;
                distHorizontal = 0f;
                direction = DIRECTION.NULL;
            }

            //--------------------------------------------------------------------------------------------------------//


            Globals.MovingDirection = distHorizontal <= 0.2f
                ? DIRECTION.NULL
                : direction;

            if (!canMove)
                return;
            
            var toMove = Mathf.Min(distHorizontal, Globals.BotHorizontalSpeed * Time.deltaTime);

            var moveDirection = direction.ToVector2();

            m_distanceHorizontal -= toMove * moveDirection.x;
            transform.position += (Vector3)moveDirection * toMove;

            //--------------------------------------------------------------------------------------------------------//

        }

        public void RegisterMoveOnInput()
        {
            InputManager.RegisterMoveOnInput(this);
        }

        public void Move(float direction)
        {
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                m_currentInput = 0f;
                return;
            }

            m_currentInput = direction;

            m_distanceHorizontal += direction * Constants.gridCellSize;
        }

        #endregion //IMoveOnInput Functions

        //============================================================================================================//

        #region Init Bot 

        public void InitBot()
        {
            _weldDatas = new List<WeldData>();
            
            var partFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();
            
            _isDestroyed = false;
            CompositeCollider2D.enabled = true;

            BotPartsLogic.coreHeat = 0f;

            var startingHealth = FactoryManager.Instance.PartsRemoteData.GetRemoteData(PART_TYPE.CORE).levels[0].health;
            //Add core component
            var core = partFactory.CreateObject<Part>(
                new BlockData
                {
                    Type = (int)PART_TYPE.CORE,
                    Coordinate = Vector2Int.zero,
                    Level = 0,
                    Health = startingHealth
                });
            
            if(Globals.IsRecoveryBot) partFactory.SetOverrideSprite(core, PART_TYPE.RECOVERY);

            AttachNewBlock(Vector2Int.zero, core, updateMissions: false);

            ObstacleManager.NewShapeOnScreen += CheckForBonusShapeMatches;
            
            GameUi.SetHealthValue(1f);

            var camera = CameraController.Camera.GetComponent<CameraController>();
            camera.SetLookAtFollow(transform);
            camera.ResetCameraPosition();
        }
        
        public void InitBot(IEnumerable<IAttachable> botAttachables)
        {
            _weldDatas = new List<WeldData>();
            
            _isDestroyed = false;
            CompositeCollider2D.enabled = true;
            
            BotPartsLogic.coreHeat = 0f;
            
            //Only want to update the parts list after everyone has loaded
            foreach (var attachable in botAttachables)
            {
                if (attachable is Part part && part.Type == PART_TYPE.CORE)
                {
                    if(Globals.IsRecoveryBot)
                        FactoryManager.Instance.GetFactory<PartAttachableFactory>().SetOverrideSprite(part, PART_TYPE.RECOVERY);
                    
                    GameUi.SetHealthValue(part.CurrentHealth / part.BoostedHealth);
                }

                AttachNewBlock(attachable.Coordinate, attachable, updateMissions: false, updatePartList: false);
            }



            var camera = CameraController.Camera.GetComponent<CameraController>();
            camera.SetLookAtFollow(transform);
            camera.ResetCameraPosition();

            BotPartsLogic.PopulatePartsList();

        }

        public void DisplayHints()
        {
            if(HintManager.CanShowHint(HINT.GUN) && attachedBlocks.HasPartAttached(PART_TYPE.GUN))
                HintManager.TryShowHint(HINT.GUN);
        }


        #endregion // Init Bot 

        //============================================================================================================//

        #region Input Solver

        public void Rotate(float direction)
        {
            if (GameTimer.IsPaused) 
                return;

            if (direction != 0 && GameManager.IsState(GameState.LevelBotDead))
            {
                isContinuousRotation = false;
                return;
            }

            if (previousDirection == direction && direction != 0)
            {
                isContinuousRotation = true;
            }

            previousDirection = direction;

            if (direction < 0)
                Rotate(ROTATION.CCW);
            else if (direction > 0)
                Rotate(ROTATION.CW);
            else
            {
                isContinuousRotation = false;
                return;
            }
            
            AudioController.PlaySound(SOUND.BOT_ROTATE);
        }

        [ShowInInspector, ReadOnly] 
        public float rotationTarget { get; private set; }

        /// <summary>
        /// Triggers a rotation 90deg in the specified direction. If the player is already rotating, it adds 90deg onto
        /// the target rotation.
        /// </summary>
        /// <param name="rotation"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Rotate(ROTATION rotation)
        {
            float toRotate = rotation.ToAngle();
            MostRecentRotate = rotation;
            
            //TODO Need to do the angle clamps here to prevent TargetRotation from going over bounds

            //If we're already rotating, we need to add the direction to the target
            if (Rotating)
            {
                targetRotation += toRotate;
            }
            else
            {
                targetRotation = rigidbody.rotation + toRotate;
            }

            rotationTarget += toRotate;
            rotationTarget = MathS.ClampAngle(targetRotation);

            targetRotation = MathS.ClampAngle(targetRotation);

            foreach (var attachedBlock in attachedBlocks)
            {
                attachedBlock.RotateCoordinate(rotation);
            }

            _rotating = true;
        }

        public void TrySelfDestruct()
        {
            if (GameManager.IsState(GameState.LevelEndWave))
            {
                return;
            }

            if (!_botPartsLogic.CanSelfDestruct)
                return;
            
            Destroy("Self Destruct");
        }

        #endregion //Input Solver

        //============================================================================================================//

        #region Rotation

        private void RotateBot()
        {
            var rotation = transform.eulerAngles.z;

            //Rotates towards the target rotation.
            var rotationAmount = isContinuousRotation ? Globals.BotContinuousRotationSpeed : Globals.BotRotationSpeed;
            
            rotation = Mathf.MoveTowardsAngle(rotation, targetRotation, rotationAmount * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0,0,rotation);
            
            //FIXME Remove this when ready
            TEST_ParticleSystem.transform.rotation = Quaternion.identity;

            //Here we check how close to the final rotation we are.
            var remainingDegrees = Mathf.Abs(Mathf.DeltaAngle(rotation, targetRotation));
            
            //TODO Here we'll need to rotate the sprites & Coordinates after a certain threshold is met for that rotation

            if (remainingDegrees > 10f)
                return;

            RotateAttachableSprites();
            
            
            //If we're within 1deg we will count it as complete, otherwise continue to rotate.
            if (remainingDegrees > 1f)
                return;
            
            //Ensures that the Attachables are correctly rotated
            //NOTE: This is a strict order-of-operations as changing will cause rotations to be incorrect
            //--------------------------------------------------------------------------------------------------------//
            //Force set the rotation to the target, in case the bot is not exactly on target
            transform.rotation = Quaternion.Euler(0,0,rotationTarget);
            targetRotation = 0f;
            
            
            RotateAttachableSprites();
            _rotating = false;
            
            //--------------------------------------------------------------------------------------------------------//
            
            //Should only be called after the rotation finishes
            CheckForBonusShapeMatches();
        }

        public void ForceCompleteRotation()
        {
            var rotation = transform.eulerAngles.z;
            var remainingDegrees = Mathf.DeltaAngle(rotation, targetRotation);

            if (Mathf.Abs(remainingDegrees) < 75)
            {
                transform.rotation = Quaternion.Euler(0, 0, targetRotation);
            }
            else
            {
                if (remainingDegrees > 0) rotationTarget -= 90;
                else if (remainingDegrees < 0) targetRotation += 90;
                
                transform.rotation = remainingDegrees > 0
                    ? Quaternion.Euler(0, 0, targetRotation - 90)
                    : Quaternion.Euler(0, 0, targetRotation + 90);
            }

            RotateAttachableSprites();
        }

        private void RotateAttachableSprites()
        {
            //Try and remove any potential floating point issues
            var check = (int) rotationTarget;
            
            
            Quaternion counterRotation;
            if (check == 180)
            {
                counterRotation = Quaternion.Euler(0,0, 180);
            }
            else if (check == 0f || check == 360)
            {
                counterRotation = Quaternion.identity;
            }
            else
            {
                counterRotation = Quaternion.Euler(0,0, MathS.ClampAngle(rotationTarget + 180));
            }

            foreach (var attachedBlock in attachedBlocks)
            {
                if (attachedBlock is ICustomRotate customRotate)
                {
                    customRotate.CustomRotate(counterRotation);
                    continue;
                }
                
                attachedBlock.transform.localRotation = counterRotation;
            }

        }


        #endregion //Rotation

        //============================================================================================================//

        #region TryAddNewAttachable

        public bool TryAddNewAttachable(IAttachable attachable, DIRECTION connectionDirection, Vector2 collisionPoint)
        {
            if (_isDestroyed)
                return false;
            
            if (Rotating)
                return false;

            IAttachable closestAttachable = null;

            switch (attachable)
            {
                case Bit bit:
                {
                   
                    bool legalDirection = true;

                    //------------------------------------------------------------------------------------------------//

                    closestAttachable = attachedBlocks.GetClosestAttachable(collisionPoint);
                    
                    /*
                    //Get the coordinate of the collision
                    var bitCoordinate = GetRelativeCoordinate(bit.transform.position);

                    legalDirection = CheckLegalCollision(bitCoordinate, closestAttachable.Coordinate, out _);
                    
                    if (!legalDirection)
                    {
                        //Make sure that the attachable isn't overlapping the bot before we say its impossible to 
                        if (!CompositeCollider2D.OverlapPoint(attachable.transform.position))
                            return false;
                    }*/

                    
                    //------------------------------------------------------------------------------------------------//


                    //Check if its legal to attach (Within threshold of connection)
                    switch (bit.Type)
                    {
                        case BIT_TYPE.BLUE:
                        case BIT_TYPE.GREEN:
                        case BIT_TYPE.GREY:
                        case BIT_TYPE.RED:
                        case BIT_TYPE.YELLOW:
                            //TODO This needs to bounce off instead of being destroyed
                            if (closestAttachable is EnemyAttachable ||
                                closestAttachable is Part part && part.Destroyed)
                            {
                                if (attachable is IObstacle obstacle)
                                    obstacle.Bounce(collisionPoint, transform.position);

                                return false;
                            }

                            PlayerDataManager.RecordBitConnection(bit.Type);
                            //Add these to the block depending on its relative position
                            AttachAttachableToExisting(bit, closestAttachable, connectionDirection);

                            CheckForBonusShapeMatches();

                            AudioController.PlayBitConnectSound(bit.Type);
                            SessionDataProcessor.Instance.BitCollected(bit.Type);
                            break;
                        case BIT_TYPE.WHITE:
                            //bounce white bit off of bot
                            var bounce = true;
                            if (bounce)
                            {
                                bit.Bounce(collisionPoint, transform.position);
                            }

                            ////We don't want to move a row if it hit an enemy instead of a bit
                            //if (closestAttachable is EnemyAttachable)
                            //    break;
                            
                            //Try and shift collided row (Depending on direction)
                            var shift = TryShift(connectionDirection.Reflected(), closestAttachable);
                            AudioController.PlaySound(shift ? SOUND.BUMPER_BONK_SHIFT : SOUND.BUMPER_BONK_NOSHIFT);
                            SessionDataProcessor.Instance.HitBumper();
                            
                            if(shift) OnBitShift?.Invoke();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(bit.Type), bit.Type, null);
                    }

                    break;
                }
                case Component component:
                {
                    bool legalDirection = true;

                    //----------------------------------------------------------------------------------------------------//

                    closestAttachable = attachedBlocks.GetClosestAttachable(collisionPoint);
                    
                    /*
                    //Get the coordinate of the collision
                    var bitCoordinate = GetRelativeCoordinate(component.transform.position);

                    legalDirection = CheckLegalCollision(bitCoordinate, closestAttachable.Coordinate, out _);

                    if (!legalDirection)
                    {
                        //Make sure that the attachable isn't overlapping the bot before we say its impossible to 
                        if (!CompositeCollider2D.OverlapPoint(attachable.transform.position))
                            return false;
                    }*/

                    //Check if its legal to attach (Within threshold of connection)
                    //TODO This needs to bounce off instead of being destroyed
                    if (closestAttachable is EnemyAttachable ||
                        closestAttachable is Part part && part.Destroyed)
                    {
                        if (attachable is IObstacle obstacle)
                            obstacle.Bounce(collisionPoint, transform.position);

                        return false;
                    }

                    //Add these to the block depending on its relative position
                    AttachAttachableToExisting(component, closestAttachable, connectionDirection);
                    SessionDataProcessor.Instance.ComponentCollected(component.Type);

                    //CheckForCombosAround();
                    
                    break;
                }
                //FIXME This seems to be wanting to attach to the wrong direction
                case EnemyAttachable enemyAttachable:
                {
                    bool legalDirection;


                    //Get the coordinate of the collision
                    var bitCoordinate = GetRelativeCoordinate(enemyAttachable.transform.position);

                    //----------------------------------------------------------------------------------------------------//

                    closestAttachable = attachedBlocks.GetClosestAttachable(collisionPoint, true);
                    
                    switch (closestAttachable)
                    {
                        case EnemyAttachable _:
                        case Part part when part.Destroyed:
                            return false;
                    }
                    
                    //FIXME This isn't sufficient to prevent multiple parasites using the same location
                    var potentialCoordinate = closestAttachable.Coordinate + connectionDirection.ToVector2Int();
                    if (attachedBlocks.Count(x => x.Coordinate == potentialCoordinate) > 1)
                        return false;

                    legalDirection = CheckLegalCollision(bitCoordinate, closestAttachable.Coordinate, out _);

                    //----------------------------------------------------------------------------------------------------//

                    if (!legalDirection)
                    {
                        //Make sure that the attachable isn't overlapping the bot before we say its impossible to 
                        if (!CompositeCollider2D.OverlapPoint(attachable.transform.position))
                            return false;
                    }

                    //Add these to the block depending on its relative position
                    AttachAttachableToExisting(enemyAttachable, closestAttachable, connectionDirection);
                    break;
                }
                case JunkBit junkBit:
                {
                    bool legalDirection = true;

                    //----------------------------------------------------------------------------------------------------//

                    closestAttachable = attachedBlocks.GetClosestAttachable(collisionPoint);

                    //Check if its legal to attach (Within threshold of connection)
                    if (closestAttachable is EnemyAttachable ||
                        closestAttachable is Part part && part.Destroyed)
                    {
                        if (attachable is IObstacle obstacle)
                            obstacle.Bounce(collisionPoint, transform.position);

                        return false;
                    }

                    //Add these to the block depending on its relative position
                    AttachAttachableToExisting(junkBit, closestAttachable, connectionDirection);

                    //CheckForCombosAround();
                    break;
                }
            }

            if (!(attachable is EnemyAttachable) && (attachable is Bit bitCheck && bitCheck.Type != BIT_TYPE.WHITE))
            {
                _weldDatas.Add(new WeldData
                {
                    target = attachable,
                    attachedTo = closestAttachable
                });
            }

            return true;
        }

        public IAttachable GetClosestAttachable(Vector2Int checkCoordinate, float maxDistance = 999f)
        {
            IAttachable selected = null;

            var smallestDist = 999f;

            foreach (var attached in attachedBlocks)
            {
                //attached.SetColor(Color.white);
                if (attached.CountAsConnectedToCore == false)
                    continue;

                var dist = Vector2Int.Distance(attached.Coordinate, checkCoordinate);

                if (dist > maxDistance)
                    continue;
                //TODO: Make a new function for "closest to an attachable" and then remove the second part of this if statement
                if (dist > smallestDist || dist == 0)
                    continue;

                smallestDist = dist;
                selected = attached;
            }

            //selected.SetColor(Color.magenta);

            return selected;
        }

        private Vector2Int GetRelativeCoordinate(Vector2 worldPosition)
        {
            var botPosition = (Vector2) transform.position;

            var calculated = (worldPosition - botPosition) / Constants.gridCellSize;
            return new Vector2Int(
                Mathf.RoundToInt(calculated.x),
                Mathf.RoundToInt(calculated.y));
        }

        private bool CheckLegalCollision(Vector2Int lhs, Vector2Int rhs, out DIRECTION direction)
        {
            direction = (lhs - rhs).ToDirection();

            //Debug.Log($"Checking Direction: {direction}");

            switch (direction)
            {
                case DIRECTION.NULL:
                    return false;
                case DIRECTION.LEFT:
                case DIRECTION.RIGHT:
                    return Globals.MovingDirection == direction;
                case DIRECTION.UP:
                    return Globals.MovingDirection == DIRECTION.NULL;
                case DIRECTION.DOWN:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            //return direction != DIRECTION.NULL;
        }

        public bool CoordinateHasPathToCore(Vector2Int coordinate)
        {
            return _attachedBlocks.HasPathToCore(coordinate);
        }
        
        public bool CoordinateOccupied(Vector2Int coordinate)
        {
            return _attachedBlocks.Any(x => x.Coordinate == coordinate && !(x is Part part && part.Destroyed));
        }

        #endregion //Check For Legal Attach

        //============================================================================================================//
        
        #region Check for Legal Shape Attach

        public bool TryAddNewShape(Shape shape, IAttachable closestShapeBit, DIRECTION connectionDirection, Vector2 collisionPoint)
        {
            if (_isDestroyed)
                return false;
            
            if (Rotating)
                return false;
            
            var closestOnBot= attachedBlocks.GetClosestAttachable(collisionPoint);

            

            if (closestShapeBit is Bit closeBitOnShape)
            {
                switch (closeBitOnShape.Type)
                {
                    case BIT_TYPE.BLUE:
                    case BIT_TYPE.GREEN:
                    case BIT_TYPE.GREY:
                    case BIT_TYPE.RED:
                    case BIT_TYPE.YELLOW:
                        
                        //TODO This needs to bounce off instead of being destroyed
                        if (closestOnBot is EnemyAttachable ||
                            closestOnBot is Part part && part.Destroyed)
                        {
                            if (shape is IObstacle obstacle)
                                obstacle.Bounce(collisionPoint, transform.position);

                            return false;
                        }
                        
                        //--------------------------------------------------------------------------------------------//

                        var vectorDirection = connectionDirection.ToVector2Int();
                        var newBotCoordinate = closestOnBot.Coordinate + vectorDirection;
                        
                        var closestCoordinate = closestShapeBit.Coordinate;
                        
                        //Order the bits to add based on distance to the connection point
                        //--------------------------------------------------------------------------------------------//

                        var bitsToAdd = shape.AttachedBits
                            .OrderBy(x => Vector2Int.Distance(closestCoordinate, x.Coordinate))
                            .ToArray();

                        for (int i = 0; i < bitsToAdd.Length; i++)
                        {
                            PlayerDataManager.RecordBitConnection(bitsToAdd[i].Type);
                        }

                        var differences = bitsToAdd.Select(x => x.Coordinate - closestCoordinate).ToArray();
                        
                        //--------------------------------------------------------------------------------------------//

                        //Get the coordinate that the shape will be able to fit in
                        ShapeOverlapCoordinateSolver(bitsToAdd, differences, vectorDirection, ref newBotCoordinate);


                        //Add the entire shape to the Bot
                        for (var i = 0; i < bitsToAdd.Length; i++)
                        {
                            //FIXME This will need to be removed once i've confirmed the solver works correctly
                            if (attachedBlocks.Any(x => x.Coordinate == newBotCoordinate + differences[i]))
                            {
                                
                                Debug.LogError($"Conflict found at {newBotCoordinate + differences[i]}");
                                //Debug.Break();
                                //Recycler.Recycle<Shape>(shape);
                                
                                return false;
                            }
                            
                            AttachNewBlock(newBotCoordinate + differences[i], bitsToAdd[i], false, false);
                            SessionDataProcessor.Instance.BitCollected(bitsToAdd[i].Type);
                        }
                        
                        //Recycle the Shape, without also recycling the Bits since they were just attached to the bot
                        Recycler.Recycle<Shape>(shape, new
                        {
                            recycleBits = false
                        });

                        CheckForBonusShapeMatches();
                        
                        CheckForCombosAround(bitsToAdd);

                        _needToCheckMagnet = true;
                        //AudioController.PlaySound(CheckHasMagnetOverage() ? SOUND.BIT_RELEASE : SOUND.BIT_SNAP);
                        
                        CompositeCollider2D.GenerateGeometry();

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(closeBitOnShape.Type), closeBitOnShape.Type, null);
                }
            }

            return true;
        }

        /// <summary>
        /// Tries to place the shape at the desired location. If there are overlap issues it will check the up direction.
        /// If that also fails it will move 1 in the specified direction, and continue that loop until a legal spot for the shape is found.
        /// </summary>
        /// <param name="bitsToAdd"></param>
        /// <param name="differences"></param>
        /// <param name="vectorDirection"></param>
        /// <param name="attachCoordinate"></param>
        private void ShapeOverlapCoordinateSolver(IReadOnlyCollection<Bit> bitsToAdd, IReadOnlyList<Vector2Int> differences,
            Vector2Int vectorDirection, ref Vector2Int attachCoordinate)
        {

            var upDir = DIRECTION.UP.ToVector2Int();
            while (true)
            {
                if (!HasOverlap(bitsToAdd, attachCoordinate, differences))
                    break;

                if (!HasOverlap(bitsToAdd, attachCoordinate + upDir, differences))
                {
                    attachCoordinate += upDir;
                    break;
                }
                    
                attachCoordinate += vectorDirection;
            }
        }

        /// <summary>
        /// Checks to see if the shape data has an overlap at the specified location
        /// </summary>
        /// <param name="bitsToAdd"></param>
        /// <param name="attachCoordinate"></param>
        /// <param name="differences"></param>
        /// <returns></returns>
        private bool HasOverlap(IReadOnlyCollection<Bit> bitsToAdd, Vector2Int attachCoordinate, IReadOnlyList<Vector2Int> differences)
        {
            for (var i = 0; i < bitsToAdd.Count; i++)
            {
                var check = attachedBlocks.FirstOrDefault(x =>
                    x.Coordinate == attachCoordinate + differences[i]);

                if (check == null) 
                    continue;
                
                //Debug.Log($"Found overlap at {attachCoordinate + differences[i]} on {check.gameObject.name}", check.gameObject);
                return true;
            }

            return false;
        }
        
        private bool HasOverlap()
        {
            throw new NotImplementedException();
        }
        
        #endregion //Check for Legal Shape Attach
        
        //============================================================================================================//
        
        #region TryHitAt

        /// <summary>
        /// Decides if the Attachable closest to the hit position should be destroyed or damaged on the bounce
        /// </summary>
        /// <param name="hitPosition"></param>
        /// <param name="destroyed"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool TryBounceAt(Vector2 hitPosition, out bool destroyed)
        {
            destroyed = false;
            
            if(!GameManager.IsState(GameState.LEVEL_ACTIVE))
                return false;
            
            var closestAttachable = attachedBlocks.GetClosestAttachable(hitPosition);

            switch (closestAttachable)
            {
                //Don't want any bounce on Bit collisions: https://trello.com/c/jgOMp2eX/1071-asteroid-bit-collisions
                case Bit _:
                case EnemyAttachable _:
                    AsteroidDamageAt(closestAttachable);
                    return false;
                case Component _:
                    break;
                case Part part:
                    if (part.Destroyed) return false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(closestAttachable), closestAttachable, null);
            }
            
            TryHitAt(closestAttachable, Globals.AsteroidDamage);

            if (closestAttachable is IHealth iHealth && iHealth.CurrentHealth <= 0)
                destroyed = true;
            
            return true;
        }

        public bool TryProjectileTriggerAt(Vector2 worldPosition, string projectileName)
        {
            Debug.Log(projectileName);

            if (projectileName == "Junk Bit")
            {
                Debug.Log("Try attach junk bit");
                JunkBit junkBit = FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateJunkObject<JunkBit>();
                TryAttachNewBlock(attachedBlocks.GetClosestAttachable(worldPosition).Coordinate, junkBit, false);
            }

            return true;
        }

        public bool TryHitAt(Vector2 worldPosition, float damage)
        {
            SessionDataProcessor.Instance.ReceivedDamage(damage);
            
            if(!GameManager.IsState(GameState.LEVEL_ACTIVE))
                return false;
            
            var closestAttachable = attachedBlocks.GetClosestAttachable(worldPosition);

            switch (closestAttachable)
            {
                // Enemies attached should not be hit by other enemy projectiles
                case EnemyAttachable _:
                case Part part when part.Destroyed:
                    return false;
            }

            /*var explosion = FactoryManager.Instance.GetFactory<EffectFactory>().CreateObject<Explosion>();
            explosion.transform.position = worldPosition;*/
            
            TryHitAt(closestAttachable, damage);

            return true;
        }

        public void TryHitAt(IAttachable closestAttachable, float damage, bool withSound = true)
        {
            if (!CanBeDamaged && closestAttachable.Coordinate == Vector2Int.zero)
                return;

            if (!(closestAttachable is IHealth closestHealth))
                return;
            
            //--------------------------------------------------------------------------------------------------------//

            //Don't want to apply shields to the Enemy
            if (!(closestAttachable is EnemyAttachable))
                damage = BotPartsLogic.TryHitShield(closestAttachable.Coordinate, damage);

            if (damage <= 0f)
                return;

            closestHealth.ChangeHealth(-Mathf.Abs(damage));
            
            if(closestAttachable is Part part && part.Type == PART_TYPE.CORE)
                GameUi.SetHealthValue(part.CurrentHealth / part.BoostedHealth);

            var attachableDestroyed = closestHealth.CurrentHealth <= 0f;

            switch (closestAttachable)
            {
                case Bit _ when withSound:
                    AudioController.PlaySound(attachableDestroyed ? SOUND.BIT_EXPLODE : SOUND.BIT_DAMAGE);
                    break;
                case Part _ when withSound:
                    AudioController.PlaySound(attachableDestroyed ? SOUND.PART_EXPLODE : SOUND.PART_DAMAGE);
                    break;
            }
            
            if(withSound && !attachableDestroyed)
                AudioController.PlaySound(SOUND.PART_DAMAGE);

            if (!attachableDestroyed)
                return;
            
            if(withSound)
                AudioController.PlaySound(SOUND.PART_EXPLODE);
            
            //Things to do if the attachable is destroyed
            //--------------------------------------------------------------------------------------------------------//

            switch (closestAttachable)
            {
                //----------------------------------------------------------------------------------------------------//
                case Bit bit:
                    CreateBitDeathEffect(bit.Type, bit.transform.position);
                    RemoveAttachable(closestAttachable);
                    break;
                //----------------------------------------------------------------------------------------------------//
                case Part deadPart when deadPart.Type == PART_TYPE.CORE:
                    CreateCoreDeathEffect();

                    cinemachineImpulseSource.GenerateImpulse(5);
                    GameUi.FlashBorder();
                    
                    Destroy("Core Destroyed");
                    break;
                case Part _:
                    CreateExplosionEffect(closestAttachable.transform.position);

                    cinemachineImpulseSource.GenerateImpulse(5);
                    GameUi.FlashBorder();
                    
                    BotPartsLogic.PopulatePartsList();
                    break;
                //----------------------------------------------------------------------------------------------------//
                default:
                    RemoveAttachable(closestAttachable);
                    break;
            }
            
            if(closestAttachable.CountTowardsMagnetism)
                ForceCheckMagnets();



            if (closestAttachable.Coordinate != Vector2Int.zero)
                CheckForDisconnects();

            //------------------------------------------------------------------------------------------------//
        }



        #endregion //TryHitAt
        
        #region Asteroid Collision
        
        public bool TryAsteroidDamageAt(Vector2 collisionPoint)
        {
            if(!GameManager.IsState(GameState.LEVEL_ACTIVE))
                return false;
            
            var closestAttachable = attachedBlocks.GetClosestAttachable(collisionPoint);

            //------------------------------------------------------------------------------------------------//

            switch (closestAttachable)
            {
                case Part part when part.Destroyed:
                    return false;
                case Bit _:
                    AsteroidDamageAt(closestAttachable);
                    return false;
            }


            AsteroidDamageAt(closestAttachable);
            return true;
        }

        /// <summary>
        /// Applies pre-determine asteroid damage to the specified IAttachable
        /// </summary>
        /// <param name="attachable"></param>
        private void AsteroidDamageAt(IAttachable attachable)
        {

            TryHitAt(attachable, 10000);
            AudioController.PlaySound(SOUND.ASTEROID_CRUSH);

            BIT_TYPE? type = null;
            switch (attachable)
            {
                case Part _ :
                    FrameStop.Milliseconds(75);
                    break;
                case Bit bit:
                    type = bit.Type;
                    break;
                case EnemyAttachable enemyAttachable:
                    enemyAttachable.SetAttached(false);
                    break;
            }

            AsteroidMissionUpdate(type);

            //FIXME This value should not be hardcoded
            BotPartsLogic.AddCoreHeat(20f);

            if ((attachedBlocks.Count == 0 || ((IHealth) attachedBlocks[0])?.CurrentHealth <= 0) && CanBeDamaged)
            {
                Destroy("Core Destroyed by Asteroid");
            }
            else if (BotPartsLogic.coreHeat >= 100 && CanBeDamaged)
            {
                Destroy("Core Overheated");
            }
        }

        private static void AsteroidMissionUpdate(BIT_TYPE? bitType)
        {
            var missionProgressEventData = new MissionProgressEventData
            {
                bitType = bitType,
                intAmount = 1
            };
            MissionManager.ProcessMissionData(typeof(AsteroidCollisionMission), missionProgressEventData);
        }

        #endregion //Asteroid Collision

        //============================================================================================================//

        #region Attach Blocks
        
        public bool TryAttachNewBlock(Vector2Int coordinate, IAttachable newAttachable, 
            bool checkForCombo = true, 
            bool updateColliderGeometry = true, 
            bool updateMissions = true,
            bool updatePartList = true)
        {
            if (Destroyed) 
                return false;
            
            if (attachedBlocks.Any(x => x.Coordinate == coordinate))
                return false;
            
            newAttachable.Coordinate = coordinate;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * coordinate * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            //newAttachable.gameObject.name = $"Block {attachedBlocks.Count}";
            
            //We want to avoid having the same element multiple times in the list
            if(!attachedBlocks.Contains(newAttachable)) 
                attachedBlocks.Add(newAttachable);
            
            switch (newAttachable)
            {
                case Bit bit:
                    if (updateMissions)
                    {
                        MissionProgressEventData missionProgressEventData = new MissionProgressEventData
                        {
                            bitType = bit.Type,
                            intAmount = 1,
                            bitDroppedFromEnemyLoot = bit.IsFromEnemyLoot
                        };
                        
                        MissionManager.ProcessMissionData(typeof(ResourceCollectedMission), missionProgressEventData);
                    }
                        
                    if(checkForCombo) CheckForCombosAround<BIT_TYPE>(coordinate);
                        
                    break;
                case Component _ when checkForCombo:
                    CheckForCombosAround<COMPONENT_TYPE>(coordinate);
                    break;
                case Part _ when updatePartList:
                    BotPartsLogic.PopulatePartsList();
                    break;

                //This can NEVER happen as Shape is not IAttachable
                /*case Shape shape:
                    if (updateMissions)
                    {
                        foreach (var attachedBit in shape.AttachedBits)
                        {
                            MissionManager.ProcessResourceCollectedMissionData(attachedBit.Type,
                                FactoryManager.Instance.GetFactory<BitAttachableFactory>().GetBitRemoteData(attachedBit.Type).levels[attachedBit.level].resources);
                        }
                    }
                    break;*/
            }

            if (newAttachable.CountTowardsMagnetism)
                _needToCheckMagnet = true;//AudioController.PlaySound(CheckHasMagnetOverage() ? SOUND.BIT_RELEASE : SOUND.BIT_SNAP);

            if(updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();

            return true;
        }

        public void AttachNewBlock(Vector2Int coordinate, IAttachable newAttachable, 
            bool checkForCombo = true, 
            bool updateColliderGeometry = true, 
            bool updateMissions = true, 
            bool checkMagnet = true, 
            bool playSound = true,
            bool updatePartList = true)
        {
            if (Destroyed) 
                return;
            
            newAttachable.Coordinate = coordinate;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * coordinate * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            //newAttachable.gameObject.name = $"Block {attachedBlocks.Count}";
            
            //We want to avoid having the same element multiple times in the list
            if(!attachedBlocks.Contains(newAttachable)) 
                attachedBlocks.Add(newAttachable);

            switch (newAttachable)
            {
                case Bit bit:
                    if (updateMissions)
                    {
                        MissionProgressEventData missionProgressEventData = new MissionProgressEventData
                        {
                            bitType = bit.Type,
                            intAmount = 1,
                            bitDroppedFromEnemyLoot = bit.IsFromEnemyLoot
                        };

                        MissionManager.ProcessMissionData(typeof(ResourceCollectedMission), missionProgressEventData);
                    }

                    if(checkForCombo) CheckForCombosAround<BIT_TYPE>(coordinate);
                    break;
                case Component _ when checkForCombo:
                    CheckForCombosAround<COMPONENT_TYPE>(coordinate);
                    break;
                case Part _ when updatePartList:
                    BotPartsLogic.PopulatePartsList();
                    break;
            }

            if (newAttachable.CountTowardsMagnetism && checkMagnet)
            {
                _needToCheckMagnet = true;
                //var check = CheckHasMagnetOverage();
                //if(playSound)
                //    AudioController.PlaySound(check ? SOUND.BIT_RELEASE : SOUND.BIT_SNAP);
            }

            /*if (updateMissions)
            {
                if (newAttachable is Bit bit)
                {
                    MissionManager.ProcessResourceCollectedMissionData(bit.Type, 
                        FactoryManager.Instance.GetFactory<BitAttachableFactory>().GetBitRemoteData(bit.Type).levels[bit.level].resources);
                }
                else if (newAttachable is Shape shape)
                {
                    foreach (var attachedBit in shape.AttachedBits)
                    {
                        MissionManager.ProcessResourceCollectedMissionData(attachedBit.Type,
                            FactoryManager.Instance.GetFactory<BitAttachableFactory>().GetBitRemoteData(attachedBit.Type).levels[attachedBit.level].resources);
                    }
                }
            }*/

            
            /*if(checkForCombo)
                CheckForCombosAround(coordinate);*/

            if(updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();
        }

        public void AttachAttachableToExisting(IAttachable newAttachable, IAttachable existingAttachable,
            DIRECTION direction, 
            bool checkForCombo = true, 
            bool updateColliderGeometry = true,
            bool updateMissions = true, 
            bool checkMagnet = true, 
            bool playSound = true,
            bool updatePartList = true)
        {
            if (Destroyed) 
                return;
            
            var coordinate = existingAttachable.Coordinate + direction.ToVector2Int();

            //Checks for attempts to add attachable to occupied location
            if (attachedBlocks.Any(a => a.Coordinate == coordinate && !(a is Part part && part.Destroyed)))
            {
                var onAttachable = attachedBlocks.FirstOrDefault(a => a.Coordinate == coordinate);
                Debug.LogError(
                    $"Prevented attaching {newAttachable.gameObject.name} to occupied location {coordinate}\n Occupied by {onAttachable.gameObject.name}",
                    newAttachable.gameObject);

                AttachToClosestAvailableCoordinate(coordinate, 
                    newAttachable, 
                    direction,
                    checkForCombo, 
                    updateColliderGeometry, 
                    updateMissions);
                
                /*//I don't want the enemies to push to the end of the arm, I want it just attach to the closest available space
                if (newAttachable is EnemyAttachable)
                    AttachToClosestAvailableCoordinate(coordinate, 
                        newAttachable, 
                        direction,
                        checkForCombo, 
                        updateColliderGeometry, 
                        updateMissions);
                else
                    PushNewAttachable(newAttachable, direction, existingAttachable.Coordinate);*/

                return;
            }

            newAttachable.Coordinate = coordinate;

            newAttachable.SetAttached(true);
            newAttachable.transform.position =
                transform.position + (Vector3) (Vector2.one * coordinate * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            //We want to avoid having the same element multiple times in the list
            if(!attachedBlocks.Contains(newAttachable)) 
                attachedBlocks.Add(newAttachable);

            switch (newAttachable)
            {
                case Bit bit:
                    if (updateMissions)
                    {
                        MissionProgressEventData missionProgressEventData = new MissionProgressEventData
                        {
                            bitType = bit.Type,
                            intAmount = 1,
                            bitDroppedFromEnemyLoot = bit.IsFromEnemyLoot
                        };

                        MissionManager.ProcessMissionData(typeof(ResourceCollectedMission), missionProgressEventData);
                    }

                    if (checkForCombo)
                        CheckForCombosAround<BIT_TYPE>(coordinate);

                    if(existingAttachable is Part part)
                        TryAutoProcessBit(bit, part);

                    break;
                case Component _ when checkForCombo:
                    CheckForCombosAround<COMPONENT_TYPE>(coordinate);
                    break;
                case Part _ when updatePartList:
                    BotPartsLogic.PopulatePartsList();
                    break;
            }
            
            if (newAttachable.CountTowardsMagnetism && checkMagnet)
            {
                _needToCheckMagnet = true;
                //var check = CheckHasMagnetOverage();
                //if(playSound)
                //    AudioController.PlaySound(check ? SOUND.BIT_RELEASE : SOUND.BIT_SNAP);
            }



            /*if (updateMissions)
            {
                if (newAttachable is Bit bit)
                {
                    MissionManager.ProcessResourceCollectedMissionData(bit.Type,
                        FactoryManager.Instance.GetFactory<BitAttachableFactory>().GetBitRemoteData(bit.Type)
                            .levels[bit.level].resources);
                }
                /*else if (newAttachable is Shape shape)
                {
                    foreach (var attachedBit in shape.AttachedBits)
                    {
                        MissionManager.ProcessResourceCollectedMissionData(attachedBit.Type,
                            FactoryManager.Instance.GetFactory<BitAttachableFactory>()
                                .GetBitRemoteData(attachedBit.Type).levels[attachedBit.level].resources);
                    }
                }#1#
            }

            if (checkForCombo)
            {
                CheckForCombosAround(coordinate);
                CheckHasMagnetOverage();
            }*/

            if (updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();
        }


        private void TryAutoProcessBit(Bit bit, IPart part)
        {
            switch (part.Type)
            {
                //case PART_TYPE.CORE when PROTO_autoRefineFuel && bit.Type == BIT_TYPE.RED:
                case PART_TYPE.REFINER when !part.Disabled:
                    
                    break;
                default:
                    return;
            }
            
            var hasProcessed = BotPartsLogic.ProcessBit((Part)part, bit) > 0;
            
            if(hasProcessed && part.Type == PART_TYPE.REFINER)
                PlayRefineSound(bit.Type);
            
            CheckForDisconnects();
        }

        private void PlayRefineSound(BIT_TYPE bitType)
        {
            SOUND sound;

            switch (bitType)
            {
                case BIT_TYPE.BLUE:
                    sound = SOUND.REFINE_BLUE;
                    break;
                case BIT_TYPE.GREEN:
                    sound = SOUND.REFINE_GREEN;
                    break;
                case BIT_TYPE.GREY:
                    sound = SOUND.REFINE_GREY;
                    break;
                case BIT_TYPE.RED:
                    sound = SOUND.REFINE_RED;
                    break;
                case BIT_TYPE.YELLOW:
                    sound = SOUND.REFINE_YELLOW;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bitType), bitType, null);
            }
            AudioController.PlaySound(sound);

        }

        //FIXME Ensure that I have a version of this function without the desiredDirection, and one that accounts for corners
        /// <summary>
        /// Attaches the newAttachable to the closest available location LEFT, UP, RIGHT, DOWN in an incrementing radius
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="newAttachable"></param>
        /// <param name="desiredDirection"></param>
        /// <param name="checkForCombo"></param>
        /// <param name="updateColliderGeometry"></param>
        /// <param name="updateMissions"></param>
        public void AttachToClosestAvailableCoordinate(Vector2Int coordinate, IAttachable newAttachable, DIRECTION desiredDirection, bool checkForCombo, 
            bool updateColliderGeometry, bool updateMissions)
        {
            if (Destroyed) 
                return;
            
            var directions = new[]
            {
                //Cardinal Directions
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                
                //Corners
                new Vector2Int(-1,-1), 
                new Vector2Int(-1,1), 
                new Vector2Int(1,-1), 
                new Vector2Int(1,1), 
            };

            var avoid = desiredDirection.Reflected().ToVector2Int();
            
            var dist = 1;
            while (true)
            {
                for (var i = 0; i < directions.Length; i++)
                {
                    //if (avoid == directions[i])
                    //    continue;
                    
                    var check = coordinate + (directions[i] * dist);
                    if (attachedBlocks.Any(x => x.Coordinate == check))
                        continue;

                    //We need to make sure that the piece wont be floating
                    if (!attachedBlocks.HasPathToCore(check))
                        continue;
                    //Debug.Log($"Found available location for {newAttachable.gameObject.name}\n{coordinate} + ({directions[i]} * {dist}) = {check}");
                    AttachNewBlock(check, newAttachable, checkForCombo, updateColliderGeometry, updateMissions);
                    return;
                }

                if (dist++ > 10)
                    break;

            }
        }

        public void PushNewAttachable(IAttachable newAttachable, DIRECTION direction, bool checkForCombo = true, bool updateColliderGeometry = true, bool checkMagnet = true, bool playSound = true)
        {
            if (Destroyed) 
                return;
            
            var newCoord = direction.ToVector2Int();

            attachedBlocks.FindUnoccupiedCoordinate(direction, ref newCoord);

            newAttachable.Coordinate = newCoord;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * newCoord * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            attachedBlocks.Add(newAttachable);

            switch (newAttachable)
            {
                case Bit _ when checkForCombo:
                    CheckForCombosAround<BIT_TYPE>(newCoord);
                    break;
                case Component _ when checkForCombo:
                    CheckForCombosAround<COMPONENT_TYPE>(newCoord);
                    break;
            }
            
            if (newAttachable.CountTowardsMagnetism && checkMagnet)
            {
                _needToCheckMagnet = true;
                //var check = CheckHasMagnetOverage();
                //if(playSound)
                //    AudioController.PlaySound(check ? SOUND.BIT_RELEASE : SOUND.BIT_SNAP);
            }

            /*if (checkForCombo)
            {
                CheckForCombosAround(newCoord);
                CheckHasMagnetOverage();
            }*/

            if(updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();
        }

        public void PushNewAttachable(IAttachable newAttachable, DIRECTION direction, Vector2Int startCoord, 
            bool checkForCombo = true, 
            bool updateColliderGeometry = true, 
            bool checkMagnet = true, 
            bool playSound = true)
        {
            if (Destroyed) 
                return;
            
            var newCoord = startCoord + direction.ToVector2Int();

            attachedBlocks.FindUnoccupiedCoordinate(direction, ref newCoord);

            newAttachable.Coordinate = newCoord;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * newCoord * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            attachedBlocks.Add(newAttachable);

            /*if (checkForCombo)
            {
                CheckForCombosAround(newCoord);

            }*/
            
            switch (newAttachable)
            {
                case Bit _ when checkForCombo:
                    CheckForCombosAround<BIT_TYPE>(newCoord);
                    
                    break;
                case Component _ when checkForCombo:
                    CheckForCombosAround<COMPONENT_TYPE>(newCoord);
                    break;
            }
            
            if (newAttachable.CountTowardsMagnetism && checkMagnet)
            {
                _needToCheckMagnet = true;
                //var check = CheckHasMagnetOverage();
                //if(playSound)
                //    AudioController.PlaySound(check ? SOUND.BIT_RELEASE : SOUND.BIT_SNAP);
            }
            
            if(updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();
        }

        #endregion //Attach Bits

        #region Detach Bits

        public void ForceDetach(ICanDetach attachable)
        {
            DetachSingleBlock(attachable);
        }

        private void DetachBlocks(IEnumerable<ICanDetach> detachingBits, bool delayedCollider = false,
            bool isMagnetDetach = false)
        {
            DetachBlocks(detachingBits.ToArray(), delayedCollider, isMagnetDetach);
        }
        
        //FIXME This should detach ICanDetach objects, not IAttachable
        private void DetachBlocks(IReadOnlyCollection<ICanDetach> detachingBits, bool delayedCollider = false, bool isMagnetDetach = false)
        {
            Vector3 leftOffset = Vector3.left * Constants.gridCellSize;
            
            foreach (var canDetach in detachingBits)
            {
                attachedBlocks.Remove(canDetach.iAttachable);
            }
            
            var bits = detachingBits.OfType<Bit>().ToList();
            var others = detachingBits.Where(x => !(x is Bit)).ToList();

            //Function should make a shape out of all attached bits, any floaters would remain individual
            while (bits.Count > 0)
            {
                if (TryGetShapeBits(bits[0], bits, out var shapeBits))
                {
                    foreach (var bit in shapeBits)
                    {
                        bits.Remove(bit);
                    }
                    
                    var shape = FactoryManager.Instance.GetFactory<ShapeFactory>().CreateObject<Shape>(shapeBits);

                    if (LevelManager.Instance != null)
                        LevelManager.Instance.ObstacleManager.AddObstacleToListAndParentToWorldRoot(shape);
                    
                    if (delayedCollider)
                    {
                        shape.DisableColliderTillLeaves(_compositeCollider2D);
                    }

                    if (isMagnetDetach)
                    {
                        //TODO use shapeBits to create a single magnet thing
                        var shapeCenter = (Vector3) shapeBits.GetCollectionCenterPosition() - shape.transform.position;
                        var left = shape.AttachedBits
                            .Select(x => x.Coordinate.x)
                            .OrderByDescending(x => x)
                            .FirstOrDefault() * Vector3.left;
                        
                        ConnectedSpriteObject.Create(shape.transform, shapeCenter + left + leftOffset);
                        //foreach (var attachable in attachablesToDetach)
                        //{
                        //    ConnectedSpriteObject.Create(attachable.transform, leftOffset);
                        //}
                    }
                }
                else
                {
                    var bit = bits[0];
                    
                    bit.SetAttached(false);
                    bit.SetColor(Color.white);
                    
                    bit.transform.rotation = Quaternion.identity;

                    if (LevelManager.Instance != null)
                        LevelManager.Instance.ObstacleManager.AddObstacleToListAndParentToWorldRoot(bit);

                    bits.RemoveAt(0);
                    
                    if(delayedCollider)
                        bit.DisableColliderTillLeaves(_compositeCollider2D);
                    
                    if (isMagnetDetach)
                    {
                        ConnectedSpriteObject.Create(bit.transform, leftOffset);
                    }
                }
            }
            
            foreach (var canDetach in others)
            {
                if (delayedCollider && canDetach is CollidableBase collidableBase)
                {
                    collidableBase.DisableColliderTillLeaves(_compositeCollider2D);
                }
                
                if(LevelManager.Instance && canDetach is IObstacle obstacle)
                    LevelManager.Instance.ObstacleManager.AddObstacleToListAndParentToWorldRoot(obstacle);
                

                canDetach.iAttachable.SetAttached(false);
                
                if (isMagnetDetach)
                {
                    ConnectedSpriteObject.Create(canDetach.transform, leftOffset);
                }
            }

            CheckForDisconnects();
            
            CompositeCollider2D.GenerateGeometry();

        }

        /// <summary>
        /// Returns a bool representing whether or not the originBit is attached to a group that can be used to create
        /// a shape. If false, the out shapeBits will return null.
        /// </summary>
        /// <param name="originBit"></param>
        /// <param name="bits"></param>
        /// <param name="shapeBits"></param>
        /// <returns></returns>
        private bool TryGetShapeBits(Bit originBit, List<Bit> bits, out List<Bit> shapeBits)
        {
            shapeBits = new List<Bit>();

            bits.GetAllConnectedDetachables(originBit, null, ref shapeBits);

            if (shapeBits.Count > 1)
            {
                return true;
            }

            shapeBits = null;
            return false;
        }
        
        private void DetachSingleBlock(ICanDetach canDetach)
        {
            canDetach.transform.parent = null;

            if (LevelManager.Instance && canDetach is IObstacle obstacle)
                LevelManager.Instance.ObstacleManager.AddObstacleToListAndParentToWorldRoot(obstacle);

            RemoveAttachable(canDetach.iAttachable);
        }
        
        private void RemoveAttachable(IAttachable attachable)
        {
            attachedBlocks.Remove(attachable);
            attachable.SetAttached(false);
            
            CompositeCollider2D.GenerateGeometry();
            CheckForBonusShapeMatches();
        }
        
        public void DestroyAttachable(IAttachable attachable)
        {
            switch (attachable)
            {
                case Bit _:
                    DestroyAttachable<Bit>(attachable);
                    break;
                case Part _:
                    DestroyAttachable<Part>(attachable);
                    BotPartsLogic.PopulatePartsList();
                    break;
                case EnemyAttachable _:
                    DestroyAttachable<EnemyAttachable>(attachable);
                    break;
            }
        }
        
        /// <summary>
        /// Removes the attachable, and will recycle it under the T bin.
        /// </summary>
        /// <param name="attachable"></param>
        /// <typeparam name="T"></typeparam>
        public void DestroyAttachable<T>(IAttachable attachable) where T: IAttachable
        {
            attachedBlocks.Remove(attachable);
            attachable.SetAttached(false);

            Recycler.Recycle<T>(attachable.gameObject);
            
            CheckForDisconnects();
            
            CompositeCollider2D.GenerateGeometry();
        }
        
        #endregion //Detach Bits

        public void MarkAttachablePendingRemoval(IAttachable attachable)
        {
            attachedBlocks.Remove(attachable);

            CheckForDisconnects();
        }

        //============================================================================================================//
        
        #region Check for New Disconnects
        
        /// <summary>
        /// Function will review and detach any blocks that no longer have a connection to the core.
        /// </summary>
        private bool CheckForDisconnects()
        {
            var toSolve = new List<ICanDetach>(attachedBlocks.OfType<ICanDetach>());
            bool hasDetached = false;
            
            foreach (var canDetach in toSolve)
            {
                /*if (!attachedBlocks.Contains(attachable))
                    continue;*/

                var hasPathToCore = attachedBlocks.HasPathToCore(canDetach.iAttachable);
                
                if(hasPathToCore)
                    continue;

                hasDetached = true;

                var detachables = new List<ICanDetach>();
                attachedBlocks.GetAllConnectedDetachables(canDetach, null, ref detachables);

                foreach (var attachedBit in detachables.OfType<Bit>())
                {
                    SessionDataProcessor.Instance.BitDetached(attachedBit.Type);
                }

                if (detachables.Count == 1)
                {
                    DetachSingleBlock(detachables[0]);
                    continue;
                }

                DetachBlocks(detachables);
            }

            return hasDetached;
        }

        /// <summary>
        /// Checks to see if removing the list wantToRemove causes disconnects on the bot. Returns true on any disconnect.
        /// Returns false if all is okay.
        /// </summary>
        /// <param name="wantToRemove"></param>
        /// <param name="disconnectList"></param>
        /// <returns></returns>
        private bool RemovalCausesDisconnects(ICollection<ICanDetach> wantToRemove, out string disconnectList)
        {
            disconnectList = string.Empty;
            var toSolve = new List<ICanDetach>(attachedBlocks.OfType<ICanDetach>());
            var ignoreCoordinates = wantToRemove?.Select(x => x.Coordinate).ToList();
            
            foreach (var canDetach in toSolve)
            {
                if (!canDetach.iAttachable.CountAsConnectedToCore)
                    continue;
                
                //if (!attachedBlocks.Contains(attachable))
                //    continue;

                if (wantToRemove != null && wantToRemove.Contains(canDetach))
                    continue;
                
                var hasPathToCore = attachedBlocks.HasPathToCore(canDetach.iAttachable, ignoreCoordinates);
                
                if(hasPathToCore)
                    continue;

                disconnectList += $"{canDetach.gameObject.name} will disconnect\n";

                return true;
            }

            return false;
        }
        
        #endregion //Check for New Disconnects

        //============================================================================================================//

        #region Shifting Bits
        
        /// <summary>
        /// Shits an entire row or column based on the direction and the bit selected. Returns true if anything was shifted
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="attachable"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private bool TryShift(DIRECTION direction, IAttachable attachable)
        {
            List<IAttachable> inLine;
            switch (direction)
            {
                case DIRECTION.LEFT:
                case DIRECTION.RIGHT:
                    inLine = attachedBlocks.Where(ab => ab.Coordinate.y == attachable.Coordinate.y).ToList();
                    break;
                case DIRECTION.UP:
                case DIRECTION.DOWN:
                    inLine = attachedBlocks.Where(ab => ab.Coordinate.x == attachable.Coordinate.x).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            var toShift = new List<ShiftData>();
            var dir = direction.ToVector2Int();
            var currentCoordinate = attachable.Coordinate;
            
            var passedCore = false;

            for (var i = 0; i < inLine.Count; i++)
            {
                var targetAttachable = inLine.FirstOrDefault(x => x.Coordinate == currentCoordinate);

                if (targetAttachable == null)
                    break;

                if (targetAttachable.CanShift)
                {
                    IAttachable nextCheck;

                    var noShiftOffset = 1;
                    do
                    {
                        var coordinate = currentCoordinate + (dir * noShiftOffset);
                        //TODO I think that I can combine both the While Loop and the Linq expression
                        nextCheck = inLine.FirstOrDefault(x => x.Coordinate == coordinate);


                        
                        if (nextCheck is null || nextCheck.CanShift) break;

                        if (!passedCore && coordinate == Vector2Int.zero)
                            passedCore = true;

                        noShiftOffset++;
                        
                    } while (!nextCheck.CanShift);

                    currentCoordinate += dir * noShiftOffset;
                }
                else
                {
                    //FIXME This will work for parasites, but nothing else
                    //Checks to see if there are things stacked on top of the destroyed part
                    if (targetAttachable is Part part && part.Destroyed)
                    {
                        var stackedList = inLine
                            .Where(x =>
                            x.Coordinate == targetAttachable.Coordinate)
                            .ToList();

                        if (stackedList.Count > 1)
                        {
                            foreach (var stacked in stackedList.Where(stacked => stacked != targetAttachable))
                            {
                                switch (stacked)
                                {
                                    case EnemyAttachable _ when stacked.CanShift:
                                        toShift.Add(new ShiftData(stacked, currentCoordinate));
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException(nameof(stacked), stacked, null);
                                }
                            }
                        }
                    }
                    
                    currentCoordinate += dir;
                    continue;
                }

                toShift.Add(new ShiftData(targetAttachable, currentCoordinate));
            }

            if (toShift.Count == 0)
                return false;

            bool hasDetached = false;
            bool hasCombos = false;
            
            StartCoroutine(ShiftInDirectionCoroutine(toShift, 
                Globals.BitShiftTime,
                () =>
            {
                //Checks for floaters
                hasDetached = CheckForDisconnects();
                
                //TODO May want to consider that Enemies may still attack while being shifted
                //This needs to happen before checking for disconnects because otherwise attached will be set to false
                foreach (var wasBumped in toShift.Select(x => x.Target).OfType<IWasBumped>())
                {
                    wasBumped.OnBumped();
                }
                
                

                /*var comboCheckGroup = toShift.Select(x => x.Target).Where(x => attachedBlocks.Contains(x) && x is ICanCombo)
                    .OfType<ICanCombo>().ToArray();*/
                
                hasCombos = CheckAllForCombos();
                
                /*switch (attachable)
                {
                    case Bit _:
                        hasCombos = CheckForCombosAround<BIT_TYPE>(comboCheckGroup);
                        break;
                    case Component _:
                        hasCombos = CheckForCombosAround<COMPONENT_TYPE>(comboCheckGroup);

                        break;
                }*/

                CheckForBonusShapeMatches();
                ForceCheckMagnets();

                MissionProgressEventData missionProgressEventData = new MissionProgressEventData
                {
                    intAmount = toShift.Count,
                    bumperShiftedThroughPart = passedCore,
                    bumperOrphanedBits = hasDetached,
                    bumperCausedCombos = hasCombos
                };
                MissionManager.ProcessMissionData(typeof(WhiteBumperMission), missionProgressEventData);
            }));


            return true;
        }

        #endregion //Shifting Bits
        
        //============================================================================================================//

        #region Check For Bonus Shape Matches

        /// <summary>
        /// Searches Bot for any matches to Active Bonus Shapes. Solves if any matches are found. Assumes that all matches are Bits.
        /// </summary>
        private void CheckForBonusShapeMatches()
        {
            var obstacleManager = LevelManager.Instance.ObstacleManager;

            if (!obstacleManager.HasActiveBonusShapes)
                return;
            
            IEnumerable<Shape> shapesToCheck = obstacleManager.ActiveBonusShapes;

            foreach (var shape in shapesToCheck)
            {
                if (!attachedBlocks.Contains(shape.AttachedBits, out var upgrading))
                    continue;
                
                //Bonus Shape Effects
                //----------------------------------------------------------------------------------------------------//
                
                foreach (var attachable in shape.AttachedBits)
                {
                    CreateBonusShapeEffect(attachable.transform.position);
                }

                var blocks = attachedBlocks.Where(x => upgrading.Contains(x.Coordinate));
                CreateBonusShapeEffect(blocks);
                
                /*foreach (var coordinate in upgrading)
                {
                    
                    CreateBonusShapeEffect(block.transform);
                }*/
                
                //----------------------------------------------------------------------------------------------------//
                
                AudioController.PlaySound(SOUND.BONUS_SHAPE_MATCH);
                
                AudioController.PlaySound(SOUND.BONUS_SHAPE_UPG);
                //Upgrade the pieces matched
                foreach (var coordinate in upgrading)
                {
                    var toUpgrade = attachedBlocks.OfType<Bit>().FirstOrDefault(x => x.Coordinate == coordinate);

                    toUpgrade?.IncreaseLevel();
                    
                    
                }

                List<BIT_TYPE> numTypes = new List<BIT_TYPE>();
                for (int i = 0; i < shape.AttachedBits.Count; i++)
                {
                    if (!numTypes.Contains(shape.AttachedBits[i].Type))
                    {
                        numTypes.Add(shape.AttachedBits[i].Type);
                    }
                }

                var gears = Globals.GetBonusShapeGearRewards(shape.AttachedBits.Count, numTypes.Count);

                
                
                //Remove the Shape
                PlayerDataManager.ChangeGears(gears);
                obstacleManager.MatchBonusShape(shape);
                
                
                
                //FIXME We'll need to double check the position here
                FloatingText.Create($"+{gears}",
                    attachedBlocks.Find(upgrading).GetCollectionCenterCoordinateWorldPosition(),
                    Color.white);


                //Check for Combos
                CheckForCombosAround<BIT_TYPE>(attachedBlocks);
                //CheckForCombosAround<COMPONENT_TYPE>(attachedBlocks);

                //Call this function again
                CheckForBonusShapeMatches();
                break;

            }
        }

        #endregion //CheckForBonusShapeMatches

        //Creating Effects
        //====================================================================================================================//

        #region Creating Effects

        private void ShouldShowEffect(ref List<WeldData> weldDatas)
        {
            bool IsBusy(IAttachable attachable)
            {
                switch (attachable)
                {
                    case ICanCombo iCanCombo when iCanCombo.IsBusy:
                    case ICanDetach iCanDetach when iCanDetach.PendingDetach:
                        return true;
                    default:
                        return false;
                }
            }
            var copy = new List<WeldData>(weldDatas);
            weldDatas = new List<WeldData>();

            for (int i = copy.Count - 1; i >= 0; i--)
            {
                var data = copy[i];
                var direction = data.Direction;

                if (direction == DIRECTION.NULL)
                    continue;
                
                if(!data.target.Attached || !data.attachedTo.Attached)
                    continue;

                if (IsBusy(data.target) || IsBusy(data.attachedTo))
                    continue;

                CreateWeldEffect(data.target.Coordinate, direction);
            }
        }

        private void CreateWeldEffect(Vector2Int coordinate, DIRECTION direction)
        {
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>().CreateEffect(EffectFactory.EFFECT.WELD);
            var effectTransform = effect.transform;
            effectTransform.SetParent(transform);

            var position = coordinate + (direction.Reflected().ToVector2() / 2f);
            effectTransform.localPosition = transform.InverseTransformPoint(transform.position + (Vector3)position);

            switch (direction)
            {
                case DIRECTION.LEFT:
                case DIRECTION.RIGHT:
                    effectTransform.eulerAngles = Vector3.forward * 90f;
                    break;
                case DIRECTION.UP:
                case DIRECTION.DOWN:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            var time = effect.GetComponent<ScaleColorSpriteAnimation>().AnimationTime;
            
            Destroy(effect, time);
        }
        
        private void CreateExplosionEffect(Vector2 worldPosition, float scale = 1f)
        {
            var explosion = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreateEffect(EffectFactory.EFFECT.EXPLOSION);
            LevelManager.Instance.ObstacleManager.AddToRoot(explosion);
            explosion.transform.position = worldPosition;

            var particleScaling = explosion.GetComponent<ParticleSystemGroupScaling>();
            var time = particleScaling.AnimationTime;
            
            if(scale != 1f)
                particleScaling.SetSimulationSize(scale);
            
            Destroy(explosion, time);
        }
        
        private void CreateBitDeathEffect(BIT_TYPE bitType, Vector2 worldPosition)
        {
            var explosion = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreateEffect(EffectFactory.EFFECT.BIT_DEATH, bitType);
            LevelManager.Instance.ObstacleManager.AddToRoot(explosion);
            explosion.transform.position = worldPosition;

            var particleScaling = explosion.GetComponent<ParticleSystemGroupScaling>();
            var time = particleScaling.AnimationTime;
            
            Destroy(explosion, time);
        }

        private void CreateMergeEffect(Transform parent, float animationTime, Color color)
        {
            var startColor = color;
            startColor.a = 0f;
            var newTime = animationTime * 2f;

            var effect = FactoryManager.Instance.GetFactory<EffectFactory>().CreateEffect(EffectFactory.EFFECT.MERGE);
            var animationComponent = effect.GetComponent<ScaleColorSpriteAnimation>();
            
            animationComponent.SetAnimationTime(newTime);
            animationComponent.SetAllElementColors(startColor, color);
            
            
            effect.transform.SetParent(parent, false);
            
            
            Destroy(effect, newTime);
        }

        private void CreateBonusShapeEffect(Vector3 worldPosition)
        {
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreateEffect(EffectFactory.EFFECT.BONUS_SHAPE);
            
            effect.transform.position = worldPosition;
            var time = effect.GetComponent<ScaleColorSpriteAnimation>().AnimationTime;
            
            Destroy(effect, time);
        }
        private void CreateBonusShapeEffect(IEnumerable<IAttachable> actors)
        {
            foreach (var actor2DBase in actors)
            {
                var position = actor2DBase.transform.position;

                CreateBonusShapeEffect(position);
            }

            var center = actors.GetCollectionCenterPosition();

            CreateBonusShapeParticleEffect(center);

        }
        /*private void CreateBonusShapeEffect(Transform parent)
        {
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreateEffect(EffectFactory.EFFECT.BONUS_SHAPE);
            
            effect.transform.SetParent(parent, false);
            var time = effect.GetComponent<ScaleColorSpriteAnimation>().AnimationTime;
            
            Destroy(effect, time);
        }*/
        private void CreateBonusShapeParticleEffect(Vector3 position)
        {
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreateEffect(EffectFactory.EFFECT.BONUS_SHAPE_PARTICLE);
            
            //effect.transform.SetParent(parent, false);
            effect.transform.position = position;
            var time = effect.GetComponent<ParticleSystemGroupScaling>().AnimationTime;
            
            Destroy(effect, time);
        }
        
        private void CreateBombEffect(in Vector2 worldPosition, in float range)
        {
           
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreatePartEffect(EffectFactory.PART_EFFECT.BOMB);

            effect.transform.position = worldPosition;
            
            var effectAnimationComponent = effect.GetComponent<ParticleSystemGroupScaling>();
            
            effectAnimationComponent.SetSimulationSize(range);
            
            Destroy(effect, effectAnimationComponent.AnimationTime);
        }

        private void CreateCoreDeathEffect()
        {
            StartCoroutine(CoreDeathEffectCoroutine());
        }

        private IEnumerator CoreDeathEffectCoroutine()
        {
            //TODO Create main shockwave
            CreateBombEffect(transform.position, 20f);
            
            //TODO Create 3-5 explosions around core
            var count = Random.Range(3, 6);
            for (int i = 0; i < count; i++)
            {
                var corePosition = (Vector2)transform.position;
                var offset = Random.insideUnitCircle * 2f;
                CreateExplosionEffect(corePosition + offset, Random.Range(3f,10f));
                AudioController.PlaySound(SOUND.PART_EXPLODE);
                
                yield return new WaitForSeconds(Random.Range(0.1f, 1f));
            }
            
        }

        #endregion //Creating Effects
        
        //============================================================================================================//

        #region Puzzle Checks

        #region Check for Combos from List
        public bool CheckAllForCombos()
        {
            bool bitCombos = CheckForCombosAround<BIT_TYPE>(attachedBlocks);
            bool componentCombos = CheckForCombosAround<COMPONENT_TYPE>(attachedBlocks);

            return bitCombos || componentCombos;
        }

        private bool CheckForCombosAround<T>(IEnumerable<IAttachable> iAttachables) where T : Enum
        {
            return CheckForCombosAround(iAttachables.OfType<ICanCombo<T>>());
        }
        private bool CheckForCombosAround<T>(IEnumerable<ICanCombo> iCanCombos) where T : Enum
        {
            return CheckForCombosAround(iCanCombos.OfType<ICanCombo<T>>());
        }
        
        private bool CheckForCombosAround<T>(IEnumerable<ICanCombo<T>> iCanCombos) where T: Enum
        {
            List<PendingCombo> pendingCombos = null;
            bool hasCombos;
            
            
            foreach (var iCanCombo in iCanCombos)
            {
                if (iCanCombo == null)
                    continue;
            
                if (iCanCombo.level >= 4)
                    continue;

                if (!PuzzleChecker.TryGetComboData(this, iCanCombo, out var temp))
                    continue;

                if (pendingCombos == null)
                    pendingCombos = new List<PendingCombo>();

                if (pendingCombos.Contains(iCanCombo, out var index))
                {
                    if (pendingCombos[index].ComboData.points <= temp.comboData.points)
                        continue;
                    
                    pendingCombos.RemoveAt(index);
                    
                    pendingCombos.Add(new PendingCombo(temp));
                    
                }
                else
                {
                    pendingCombos.Add(new PendingCombo(temp));
                }
            }

            if (pendingCombos.IsNullOrEmpty())
                return false;
            
            var comboFactory = FactoryManager.Instance.GetFactory<ComboFactory>();

            hasCombos = true;

            //TODO Need to figure out the multi-combo scores
            foreach (var pendingCombo in pendingCombos)
            {
                if (pendingCombo.ToMove[0] is Bit bit)
                {
                    MissionProgressEventData missionProgressEventData = new MissionProgressEventData
                    {
                        bitType = bit.Type,
                        intAmount = 1,
                        level = bit.level + 1,
                        comboType = pendingCombo.ComboData.type
                    };
                    MissionManager.ProcessMissionData(typeof(ComboBlocksMission), missionProgressEventData);
                }

                var multiplier = comboFactory.GetGearMultiplier(pendingCombos.Count, pendingCombo.ToMove.Count);
                SimpleComboSolver(pendingCombo, multiplier);
            }

            return hasCombos;
        }

        #endregion //Check for Combos from List

        //====================================================================================================================//

        #region Check for Combos Around Single

        

        private void CheckForCombosAround<T>(Vector2Int coordinate) where T: Enum
        {
            CheckForCombosAround<T>(attachedBlocks
                .FirstOrDefault(a => a.Coordinate == coordinate));
        }
        
        private void CheckForCombosAround<T>(IAttachable iAttachable) where T: Enum
        {
            if (!(iAttachable is ICanCombo iCanCombo))
                return;
            
            CheckForCombosAround(iCanCombo as ICanCombo<T>);
        }

        private void CheckForCombosAround<T>(ICanCombo<T> iCanCombo) where T: Enum
        {
            if (iCanCombo == null)
                return;
            
            if (iCanCombo.level >= 4)
                return;

            if (!PuzzleChecker.TryGetComboData(this, iCanCombo, out var data))
                return;

            if (iCanCombo is Bit bit)
            {
                MissionProgressEventData missionProgressEventData = new MissionProgressEventData
                {
                    bitType = bit.Type,
                    intAmount = 1,
                    level = iCanCombo.level + 1,
                    comboType = data.comboData.type
                };
                MissionManager.ProcessMissionData(typeof(ComboBlocksMission), missionProgressEventData);
            }
            
            var multiplier = FactoryManager.Instance.GetFactory<ComboFactory>().GetGearMultiplier(1, data.toMove.Count);
            SimpleComboSolver(data.comboData, data.toMove, multiplier);
        }

        #endregion //Check for Combos Around Single

        //============================================================================================================//
        
        #region Combo Solvers

        
        
        private void SimpleComboSolver(PendingCombo pendingCombo, float gearMultiplier)
        {
            SimpleComboSolver(pendingCombo.ComboData, pendingCombo.ToMove, gearMultiplier);
        }

        /// <summary>
        /// Solves movement and upgrade logic to do with simple combos of blocks.
        /// </summary>
        /// <param name="comboData"></param>
        /// <param name="canCombos"></param>
        /// <param name="gearMultiplier"></param>
        /// <exception cref="Exception"></exception>
        private void SimpleComboSolver(ComboRemoteData comboData, IReadOnlyCollection<ICanCombo> canCombos, float gearMultiplier)
        {
            ICanCombo closestToCore = null;
            var shortest = 999f;

            //Decide who gets to upgrade
            //--------------------------------------------------------------------------------------------------------//

            foreach (ICanCombo canCombo in canCombos)
            {
                var attachable = canCombo.iAttachable;
                //Need to make sure that if we choose this block, that it is connected to the core one way or another
                var hasPath = attachedBlocks.HasPathToCore(attachable,
                    canCombos
                        .Where(ab => ab != attachable)
                        .Select(b => b.Coordinate)
                        .ToList());

                //If there's no path, we cannot use this bit
                if (!hasPath)
                    continue;


                var dist = Vector2Int.Distance(canCombo.Coordinate, Vector2Int.zero);
                if (!(dist < shortest))
                    continue;

                shortest = dist;
                closestToCore = canCombo;
            }

            //Make sure that things are working
            //--------------------------------------------------------------------------------------------------------//

            //If no block was selected, then we've had a problem
            if (closestToCore == null)
                throw new Exception("No Closest Core Found");

            //See if anyone else needs to move
            //--------------------------------------------------------------------------------------------------------//

            //Get a list of Bits that will be moving (Blocks that are not the chosen closest to core)
            var movingBits = canCombos
                .Where(ab => ab != closestToCore).ToArray();

            //Get a list of orphans that may need move when we are moving our bits
            var orphans = new List<OrphanMoveData>();
            //CheckForOrphans(movingBits.OfType<IAttachable>(), closestToCore.iAttachable, ref orphans);

            attachedBlocks.CheckForOrphansFromCombo(
                movingBits.OfType<IAttachable>(),
                closestToCore.iAttachable,
                ref orphans);

            //Move everyone who we've determined need to move
            //--------------------------------------------------------------------------------------------------------//
            

            closestToCore.IncreaseLevel(comboData.addLevels);


            //TODO May want to place this in the coroutine
            //Plays the sound for the new level achieved by the bit
            switch (closestToCore.level)
            {
                case 1:
                    AudioController.PlaySound(SOUND.BIT_LVL1MERGE);
                    break;
                case 2:
                    AudioController.PlaySound(SOUND.BIT_LVL2MERGE);
                    break;
                case 3:
                    AudioController.PlaySound(SOUND.BIT_LVL3MERGE);
                    break;
                case 4:
                    AudioController.PlaySound(SOUND.BIT_LVL4MERGE);
                    break;
            }
            
            

            //Debug.Break();
            //Move all of the components that need to be moved
            StartCoroutine(MoveComboPiecesCoroutine(
                movingBits,
                closestToCore,
                orphans.ToArray(),
                Globals.ComboMergeTime,
                () =>
                {
                    var gearsToAdd = Mathf.RoundToInt(comboData.points * gearMultiplier);
                    //Waits till after combo finishes combining to add the points 
                    PlayerDataManager.ChangeGears(gearsToAdd);
                    
                    _lastGearText = FloatingText.Create($"+{gearsToAdd}", closestToCore.transform.position, Color.white);

                    //Show the gears hint, after the third time
                    if (_lastGearText && _combosMade++ > 2 && HintManager.CanShowHint(HINT.GEARS))
                    {
                        var iHasBounds = _lastGearText.GetComponent<IHasBounds>().GetBounds();
                        
                        Debug.Log($"Center: {iHasBounds.center}, Extents: {iHasBounds.extents}");
                        
                        HintManager.TryShowHint(HINT.GEARS, iHasBounds);
                    }

                    //We need to update the positions and level before we move them in case we interact with bits while they're moving
                    switch (closestToCore)
                    {
                        case Bit _:
                            CheckForCombosAround<BIT_TYPE>(attachedBlocks);
                            break;
                        case Component _:
                            CheckForCombosAround<COMPONENT_TYPE>(attachedBlocks);
                            break;
                    }
                    
                    CheckForBonusShapeMatches();
                    
                    OnCombo?.Invoke();
                }));
                
            
            CheckForDisconnects();
            //--------------------------------------------------------------------------------------------------------//
        }

        /*private void AdvancedComboSolver(ComboRemoteData comboData, IReadOnlyList<IAttachable> comboBits)
        {
            IAttachable bestAttachableOption = null;

            //Decide who gets to upgrade
            //--------------------------------------------------------------------------------------------------------//

            foreach (var bit in comboBits)
            {
                //Need to make sure that if we choose this block, that it is connected to the core one way or another
                var hasPath = attachedBlocks.HasPathToCore(bit as Bit,
                    comboBits.Where(ab => ab != bit)
                        .Select(b => b.Coordinate)
                        .ToList());

                //If there's no path, we cannot use this bit
                if (!hasPath)
                    continue;


                bestAttachableOption = bit;
            }

            //Make sure that things are working
            //--------------------------------------------------------------------------------------------------------//

            //If no block was selected, then we've had a problem
            if (bestAttachableOption == null)
                throw new Exception("No Closest Core Found");

            //See if anyone else needs to move
            //--------------------------------------------------------------------------------------------------------//

            //Get a list of Bits that will be moving (Blocks that are not the chosen closest to core)
            var movingBits = comboBits
                .Where(ab => ab != bestAttachableOption).ToArray();

            //Get a list of orphans that may need move when we are moving our bits
            var orphans = new List<OrphanMoveData>();
            CheckForOrphans(movingBits, bestAttachableOption, ref orphans);

            //Move everyone who we've determined need to move
            //--------------------------------------------------------------------------------------------------------//
            
            //if(orphans.Count > 0)
            //    Debug.Break();
            
            (bestAttachableOption as Bit)?.IncreaseLevel(comboData.addLevels);

            //Move all of the components that need to be moved
            StartCoroutine(MoveComboPiecesCoroutine(
                movingBits,
                bestAttachableOption,
                orphans.ToArray(),
                TEST_MergeSpeed,
                () =>
                {
                    var bit = bestAttachableOption as Bit;

                    //We need to update the positions and level before we move them in case we interact with bits while they're moving

                    //bit.IncreaseLevel();

                    CheckForCombosAround(bit);
                    CheckForCombosAround(orphans.Select(x => x.attachableBase as Bit));
                }));

            //--------------------------------------------------------------------------------------------------------//
        }*/
        
        #endregion //Combo Solvers
        
        //============================================================================================================//

        #endregion //Puzzle Checks

        //============================================================================================================//

        #region Magnet Checks

        public void ForceCheckMagnets()
        {
            _needToCheckMagnet = true;
        }

        public void ForceDisconnectAllDetachables()
        {
            DetachBlocks(attachedBlocks.OfType<ICanDetach>(), true, true);
            
            ForceCheckMagnets();
        }

        private bool CheckHasMagnetOverage()
        {
            return CheckHasMagnetOverage(BotPartsLogic.currentMagnet);
        }

        private int magnetCounter = 0;
        
        /// <summary>
        /// Determines based on the total of magnet slots which pieces must be removed to fit within the expected capacity
        /// </summary>
        private bool CheckHasMagnetOverage(MAGNET type)
        {
            if (!BotPartsLogic.useMagnet)
                return false;


            var magnetCount = BotPartsLogic.MagnetCount;
            var magnetDetachables = attachedBlocks.Where(x => x.CountTowardsMagnetism).OfType<ICanDetach>().ToList();
            
            if(GameUi) 
                GameUi.SetCarryCapacity(magnetDetachables.Count / (float)magnetCount, magnetCount);
            
            //Checks here if the total of attached blocks (Minus the Core) change
            if (magnetDetachables.Count <= magnetCount)
                return false;
            
            //--------------------------------------------------------------------------------------------------------//

            var toRemoveCount = magnetDetachables.Count - magnetCount;
            var toDetach = new List<ICanDetach>();

            //--------------------------------------------------------------------------------------------------------//

            //float time;
            Action onDetach;
            
            switch (type)
            {
                //----------------------------------------------------------------------------------------------------//
                case MAGNET.DEFAULT:
                    DefaultMagnetCheck(magnetDetachables, out toDetach, in toRemoveCount);
                    //time = 1f;
                    onDetach = () =>
                    {
                        TryProcessDetachingBits(toDetach);
                        DetachBlocks(toDetach, true, true);
                        
                    };
                    break;
                //----------------------------------------------------------------------------------------------------//
                case MAGNET.BUMP:
                    BumpMagnetCheck(magnetDetachables, out toDetach, in toRemoveCount);
                    //time = 0f;
                    onDetach = () =>
                    {
                        TryProcessDetachingBits(toDetach);
                        DetachBlocks(toDetach, true, true);
                    };
                    break;
                //----------------------------------------------------------------------------------------------------//
                case MAGNET.LOWEST:
                    LowestMagnetCheckSimple(magnetDetachables, ref toDetach, ref toRemoveCount);
                    //time = 1f;
                    onDetach = () =>
                    {
                        TryProcessDetachingBits(toDetach);
                        DetachBlocks(toDetach, true, true);
                    };
                    break;
                //----------------------------------------------------------------------------------------------------//
                default:
                    throw new ArgumentOutOfRangeException();
                //----------------------------------------------------------------------------------------------------//
            }

            //if (PendingDetach == null)
            //    PendingDetach = new List<IAttachable>();
            //
            //PendingDetach.AddRange(attachablesToDetach);

            foreach (var iCanDetach in toDetach)
            {
                iCanDetach.PendingDetach = true;
            }
            
            onDetach.Invoke();

            /*var offset = Vector3.left * Constants.gridCellSize;
            foreach (var attachable in attachablesToDetach)
            {
                ConnectedSpriteObject.Create(attachable.transform, offset);
            }*/
            
            /*//Visually show that the bits will fall off by changing their color
            if (TEST_SetDetachColor)
            {
                foreach (var bit in attachablesToDetach)
                {
                    bit.SetColor(Color.gray);
                } 
            }
            
            if(TEST_DetachTime == 0f)
                onDetach.Invoke();
            else
                this.DelayedCall(TEST_DetachTime, onDetach);*/
            //--------------------------------------------------------------------------------------------------------//
            
            GameUi.FlashMagnet();

            if(magnetCounter++ >= 2 && HintManager.CanShowHint(HINT.MAGNET)) 
                HintManager.TryShowHint(HINT.MAGNET);

            return true;
        }

        private void TryProcessDetachingBits(List<ICanDetach> toDetach)
        {
            if (toDetach.Count <= 0)
                return;
            
            for (int i = toDetach.Count - 1; i >= 0; i--)
            {
                if (!(toDetach[i] is Bit bit)) 
                    continue;

                if (!_botPartsLogic.CurrentlyUsedBitTypes.Contains(bit.Type))
                    continue;

                var core = attachedBlocks[0] as Part;

                float resourceCapacityLiquid = PlayerDataManager.GetResource(bit.Type).liquidCapacity;
                
                if (_botPartsLogic.ProcessBit(core, bit, resourceCapacityLiquid * Globals.GameUIResourceThreshold) > 0)
                {
                    toDetach.RemoveAt(i);
                }
            }
        }

        private bool IsMagnetFull()
        {
            var magnetCount = BotPartsLogic.MagnetCount;
            var magnetAttachables = attachedBlocks.Where(x => x.CountTowardsMagnetism).ToList();
            
            if(GameUi) 
                GameUi.SetCarryCapacity(magnetAttachables.Count / (float)magnetCount, magnetCount);
            
            return magnetAttachables.Count == magnetCount;
        }
        

        private void DefaultMagnetCheck(List<ICanDetach> detachables, out List<ICanDetach> toDetach, in int toRemoveCount)
        {
            var magnetCount = BotPartsLogic.MagnetCount;
            
            //Gets the last added overage to remove
            toDetach = detachables.GetRange(magnetCount, toRemoveCount);
            
            //Get the coordinates of the blocks leaving. This is used to determine if anyone will be left floating
            var leavingCoordinates = toDetach.Select(a => a.Coordinate).ToList();

            //Go through the bots Blocks to make sure no one will be floating when we detach the parts.
            for (var i = attachedBlocks.Count - 1; i >= 0; i--)
            {
                if (toDetach.Any(x => x.iAttachable == attachedBlocks[i]))
                    continue;

                if (attachedBlocks.HasPathToCore(attachedBlocks[i], leavingCoordinates))
                    continue;

                Debug.LogError(
                    $"Found a potential floater {attachedBlocks[i].gameObject.name} at {attachedBlocks[i].Coordinate}",
                    attachedBlocks[i].gameObject);
            }
        }

        private void BumpMagnetCheck(List<ICanDetach> detachables, out List<ICanDetach> toDetach, in int toRemoveCount)
        {
            var magnetCount = BotPartsLogic.MagnetCount;
            
            //Gets the last added overage to remove
            toDetach = detachables.GetRange(magnetCount, toRemoveCount);
            
            //Get the coordinates of the blocks leaving. This is used to determine if anyone will be left floating
            var leavingCoordinates = toDetach.Select(a => a.Coordinate).ToList();

            //Go through the bots Blocks to make sure no one will be floating when we detach the parts.
            for (var i = attachedBlocks.Count - 1; i >= 0; i--)
            {
                if (toDetach.Any(x => x.iAttachable == attachedBlocks[i]))
                    continue;

                if (attachedBlocks.HasPathToCore(attachedBlocks[i], leavingCoordinates))
                    continue;

                Debug.LogError(
                    $"Found a potential floater {attachedBlocks[i].gameObject.name} at {attachedBlocks[i].Coordinate}",
                    attachedBlocks[i].gameObject);
            }

            
        }

        private void LowestMagnetCheckSimple(List<ICanDetach> detachables, ref List<ICanDetach> toDetach, ref int toRemoveCount)
        {
            var checkedBits = new List<ICanDetach>();
            var debug = string.Empty;
            while (toRemoveCount > 0)
            {
                var toRemove = FindLowestDetachable(detachables, checkedBits);

                if (toRemove == null)
                {
                    //Debug.LogError($"toRemove is NULL, {toRemoveCount} remaining bits unsolved");
                    break;
                }

                checkedBits.Add(toRemove);

                if (detachables.Count == checkedBits.Count)
                {
                    //Debug.LogError($"Left with {toRemoveCount} bits unsolved");
                    break;
                }
                
                if (RemovalCausesDisconnects(new List<ICanDetach>(toDetach){toRemove}, out debug))
                    continue;

                //Debug.Log($"Found Lowest {toRemove.gameObject.name}", toRemove);
                
                toDetach.Add(toRemove);

                toRemoveCount--;
            }

            if (toRemoveCount <= 0) 
                return;
            
            //Find alternative pieces if we weren't able to find all lowest
            foreach (var bit in toDetach)
            {
                detachables.Remove(bit);
            }

            var hasIssue = false;
            while (toRemoveCount > 0)
            {
                var toRemove = FindFurthestRemovableBlock(detachables, toDetach, ref debug);

                if (toRemove == null)
                {
                    hasIssue = true;
                    //throw new Exception($"Unable to find alternative pieces\n{debug}");
                    toRemoveCount--;
                    continue;
                }
                    
                toDetach.Add(toRemove);
                detachables.Remove(toRemove);
                toRemoveCount--;
            }

            if (hasIssue)
            {
                CheckHasMagnetOverage(MAGNET.DEFAULT);
            }
        }
        
        /*private void LowestMagnetCheck2(List<Bit> bits, ref List<Bit> bitsToRemove, ref int toRemoveCount)
        {
            var checkedBits = new List<Bit>();
            
            while (toRemoveCount > 0)
            {
                var toRemove = FindLowestBit(bits, checkedBits);

                if (toRemove == null)
                {
                    Debug.LogError($"toRemove is NULL, {toRemoveCount} remaining bits unsolved");
                    break;
                }

                checkedBits.Add(toRemove);

                if (bits.Count == checkedBits.Count)
                {
                    Debug.LogError($"Left with {toRemoveCount} bits unsolved");
                    break;
                }
                
                if (RemovalCausesDisconnects(checkedBits.OfType<IAttachable>().ToList()))
                    continue;
                
                bitsToRemove.Add(toRemove);

                toRemoveCount--;
            }
            
            if (toRemoveCount > 0)
            {
                Debug.Break();
            }

        }*/

        //TODO This will likely need to move to the attachable List extensions
        private ICanDetach FindLowestDetachable(IReadOnlyCollection<ICanDetach> detachables, ICollection<ICanDetach> toIgnore)
        {
            //I Want the last Bit to be the fallback/default, if I can't find anything
            ICanDetach selectedDetachable = null;
            //var lowestLevel = 999;
            //The lowest Y coordinate
            var lowestCoordinate = 999;
            var lowestPriority = 9999;

            foreach (var canDetach in detachables)
            {
                if (toIgnore.Contains(canDetach))
                    continue;
                
                if(!(canDetach is ICanDetach detach))
                    continue;
                
                if(detach.AttachPriority > lowestPriority)
                    continue;
                
                //if (!(attachable is ILevel HasLevel))
                //{
                //    continue;
                //}
                //
                //if(HasLevel.level > lowestLevel)
                //    continue;

                //Checks if the piece is higher, and if it is, that the level is not higher than the currently selected Bit
                //This ensures that even if the lowest Bit is of high level, the lowest will always be selected
                if (canDetach.Coordinate.y > lowestCoordinate && !(detach.AttachPriority < lowestPriority))
                        continue;

                if (RemovalCausesDisconnects(new List<ICanDetach>(/*toIgnore*/) {canDetach}, out _))
                    continue;

                selectedDetachable = canDetach;
                //lowestLevel = HasLevel.level;
                lowestCoordinate = canDetach.Coordinate.y;
                lowestPriority = detach.AttachPriority;

            }

            if (selectedDetachable != null) 
                return selectedDetachable;
            
            
            foreach (var canDetach in detachables)
            {
                if (toIgnore.Contains(canDetach))
                    continue;
                
                if(!(canDetach is ICanDetach detach))
                    continue;
                
                if(detach.AttachPriority > lowestPriority)
                    continue;
                
                //if (!(attachable is ILevel hasLevel))
                //{
                //    continue;
                //}
            //
                //if(hasLevel.level > lowestLevel)
                //    continue;

                //Checks if the piece is higher, and if it is, that the level is not higher than the currently selected Bit
                //This ensures that even if the lowest Bit is of high level, the lowest will always be selected
                if (canDetach.Coordinate.y > lowestCoordinate)
                    continue;

                if (RemovalCausesDisconnects(new List<ICanDetach>(/*toIgnore*/) {canDetach}, out _))
                    continue;

                selectedDetachable = canDetach;
                //lowestLevel = hasLevel.level;
                lowestCoordinate = canDetach.Coordinate.y;
                lowestPriority = detach.AttachPriority;

            }

            return selectedDetachable;
        }

        private ICanDetach FindFurthestRemovableBlock(IEnumerable<ICanDetach> detachables, ICollection<ICanDetach> toIgnore, ref string debug)
        {
            //I Want the last Bit to be the fallback/default, if I can't find anything
            ICanDetach selectedDetachable = null;
            var furthestDistance = -999f;
            var lowestLevel = 999f;

            foreach (var canDetach in detachables)
            {
                if (toIgnore.Contains(canDetach))
                    continue;
                
                if (!(canDetach is ILevel hasLevel))
                {
                    continue;
                }

                var distance = Vector2Int.Distance(canDetach.Coordinate, Vector2Int.zero);
                
                if(distance < furthestDistance)
                    continue;

                if (lowestLevel < hasLevel.level)
                    continue;

                if (RemovalCausesDisconnects(new List<ICanDetach>(toIgnore) { canDetach }, out debug))
                    continue;

                selectedDetachable = canDetach;
                furthestDistance = distance;
                lowestLevel = hasLevel.level;

            }

            return selectedDetachable;
        }
        
        #endregion //Magnet Checks
        
        //============================================================================================================//

        #region Destroy Bot

        private void Destroy(string deathMethod)
        {
            if (_isDestroyed)
                return;
            
            _isDestroyed = true;
            CompositeCollider2D.enabled = false;
            GameUi.ShowAbortWindow(false);

            StartCoroutine(DestroyCoroutine(deathMethod));
        }
        
        #endregion //Destroy Bot
        
        //============================================================================================================//
        
        #region Coroutines
        
        /// <summary>
        /// Coroutine used to move all of the relevant Bits (Bits to be upgraded, orphans) to their appropriate locations
        /// at the specified speed, and when finished trigger the Callback.
        /// </summary>
        /// <param name="movingComboBlocks"></param>
        /// <param name="target"></param>
        /// <param name="orphans"></param>
        /// <param name="seconds"></param>
        /// <param name="onFinishedCallback"></param>
        /// <returns></returns>
        private IEnumerator MoveComboPiecesCoroutine(ICanCombo[] movingComboBlocks,
            ICanCombo target,
            IReadOnlyList<OrphanMoveData> orphans,
            float seconds,
            Action onFinishedCallback)
        {
            target.IsBusy = true;
            
            //Prepare Bits to be moved
            //--------------------------------------------------------------------------------------------------------//

            var mergeColor = Color.white;

            if (target is Bit bitColor)
            {
                mergeColor = FactoryManager.Instance.BitProfileData.GetProfile(bitColor.Type).color;
            }


            CreateMergeEffect(target.transform, seconds, mergeColor);
            foreach (var movingComboBlock in movingComboBlocks)
            {
                CreateMergeEffect(movingComboBlock.transform, seconds, mergeColor);
            }
            
            foreach (var canCombo in movingComboBlocks)
            {
                //We need to disable the collider otherwise they can collide while moving
                //I'm also assuming that if we've confirmed the upgrade, and it cannot be cancelled
                attachedBlocks.Remove(canCombo as IAttachable);
                canCombo.IsBusy = true;
                
                if(canCombo is CollidableBase collidableBase)
                    collidableBase.SetColliderActive(false);
                
            }

            foreach (var omd in orphans)
            {
                omd.attachableBase.Coordinate = omd.intendedCoordinates;
                (omd.attachableBase as Bit)?.SetColliderActive(false);
                
                if (omd.attachableBase is ICanCombo iCanCombo)
                    iCanCombo.IsBusy = true;
            }
            
            //We're going to want to regenerate the shape while things are moving
            CompositeCollider2D.GenerateGeometry();
            
            //--------------------------------------------------------------------------------------------------------//

            var t = 0f;
            var targetTransform = target.transform;

            //Obtain lists of both Transforms to manipulate & their current local positions
            //--------------------------------------------------------------------------------------------------------//

            var bitTransforms = movingComboBlocks.Select(ab => ab.transform).ToArray();
            var bitTransformPositions = bitTransforms.Select(bt => bt.localPosition).ToArray();
            
            //Same as above but for Orphans
            //--------------------------------------------------------------------------------------------------------//

            var orphanTransforms = orphans.Select(bt => bt.attachableBase.transform).ToArray();
            var orphanTransformPositions = orphanTransforms.Select(bt => bt.localPosition).ToArray();
            var orphanTargetPositions = orphans.Select(o =>
                transform.InverseTransformPoint((Vector2) transform.position +
                                                (Vector2) o.intendedCoordinates * Constants.gridCellSize)).ToArray();
            //--------------------------------------------------------------------------------------------------------//


            //Move bits towards target
            while (t / seconds <= 1f)
            {
                var td = t / seconds;
                //Move the main blocks related to the upgrading
                //----------------------------------------------------------------------------------------------------//
                
                for (var i = 0; i < bitTransforms.Length; i++)
                {
                    var bt = bitTransforms[i];
                    
                    if (bt == null)
                    {
                        Debug.LogError("TRANSFORM LOST WHILE MOVING");
                        continue;
                    }
                    
                    //Lerp to destination based on the starting position NOT the current position
                    bt.localPosition =
                        Vector2.Lerp(bitTransformPositions[i], targetTransform.localPosition, td);
                    
                    SSDebug.DrawArrow(bt.position,targetTransform.position, Color.green);
                }

                //Move the orphans into their new positions
                //----------------------------------------------------------------------------------------------------//
                
                for (var i = 0; i < orphans.Count; i++)
                {
                    var bitTransform = orphanTransforms[i];
                   
                    //Debug.Log($"Start {bitTransform.position} End {position}");

                    bitTransform.localPosition = Vector2.Lerp(orphanTransformPositions[i],
                        orphanTargetPositions[i], td);
                    
                    SSDebug.DrawArrow(bitTransform.position,transform.TransformPoint(orphanTargetPositions[i]), Color.red);
                }
                
                //----------------------------------------------------------------------------------------------------//

                t += Time.deltaTime;

                yield return null;
            }
            
            //Wrap up things now that everyone is in place
            //--------------------------------------------------------------------------------------------------------//

            //Once all bits are moved, remove from list and dispose
            foreach (var canCombo in movingComboBlocks)
            {
                if(canCombo is IAttachable attachable)
                    attachable.SetAttached(false);

                switch (canCombo)
                {
                    case Bit bit:
                        Recycler.Recycle<Bit>(bit);

                        break;
                    case Component component:
                        Recycler.Recycle<Component>(component);

                        break;
                }
                
            }

            //Re-enable the colliders on our orphans, and ensure they're in the correct position
            for (var i = 0; i < orphans.Count; i++)
            {
                var attachable = orphans[i].attachableBase;
                orphanTransforms[i].localPosition = orphanTargetPositions[i];
                
                if(attachable is CollidableBase collidableBase)
                    collidableBase.SetColliderActive(true);

                if (attachable is ICanCombo canCombo)
                    canCombo.IsBusy = false;
            }
            
            //Now that everyone is where they need to be, wrap things up
            //--------------------------------------------------------------------------------------------------------//

            CompositeCollider2D.GenerateGeometry();
            target.IsBusy = false;

            onFinishedCallback?.Invoke();
            
            //--------------------------------------------------------------------------------------------------------//
        }
                
                
        /// <summary>
        /// Moves a collection of AttachableBase 1 unit in the specified direction. Callback is triggered before the update
        /// to the Composite collider
        /// </summary>
        /// <param name="toMove"></param>
        /// <param name="seconds"></param>
        /// <param name="OnFinishedCallback"></param>
        /// <returns></returns>
        private IEnumerator ShiftInDirectionCoroutine(IReadOnlyList<ShiftData> toMove, float seconds, Action OnFinishedCallback)
        {
            var count = toMove.Count;
            
            var transforms = new Transform[count];
            var startPositions = new Vector3[count];
            var targetPositions = new Vector3[count];
            
            var skipsCoordinate = new bool[count];

            for (var i = 0; i < count; i++)
            {
                transforms[i] = toMove[i].Target.transform;
                startPositions[i] = toMove[i].Target.transform.localPosition;

                targetPositions[i] = transform.InverseTransformPoint((Vector2) transform.position +
                                                                     (Vector2)toMove[i].TargetCoordinate *
                                                                     Constants.gridCellSize);

                var distance = Math.Round(Vector2.Distance(startPositions[i], targetPositions[i]), 2);
                skipsCoordinate[i] = distance > Math.Round(Constants.gridCellSize, 2);

                if (skipsCoordinate[i])
                {
                    var spriteRenderer = toMove[i].Target.gameObject.GetComponent<SpriteRenderer>();
                    spriteRenderer.enabled = false;
                }
            }

            foreach (var shiftData in toMove)
            {
                (shiftData.Target as CollidableBase)?.SetColliderActive(false);
                shiftData.Target.Coordinate = shiftData.TargetCoordinate;
            }
            
            CompositeCollider2D.GenerateGeometry();

            var t = 0f;

            while (t / seconds < 1f)
            {
                var td = t / seconds;
                
                for (var i = 0; i < transforms.Length; i++)
                {
                    if (toMove[i].Target.Attached == false)
                        continue;
                    
                    transforms[i].localPosition = Vector2.Lerp(startPositions[i], targetPositions[i], td);
                }

                t += Time.deltaTime;
                
                yield return null;
            }
            
            for (var i = 0; i < toMove.Count; i++)
            {
                transforms[i].localPosition = targetPositions[i];
                (toMove[i].Target as CollidableBase)?.SetColliderActive(true);

                if (skipsCoordinate[i])
                {
                    var spriteRenderer = toMove[i].Target.gameObject.GetComponent<SpriteRenderer>();
                    spriteRenderer.enabled = true;
                }
            }
            
            OnFinishedCallback?.Invoke();

            CompositeCollider2D.GenerateGeometry();
        }
        
        private IEnumerator DestroyCoroutine(string deathMethod)
        {
            var index = 1;
            
            yield return new WaitForSeconds(0.3f);
            
            //TODO I think I can utilize this function in the extensions, just need to offset for coordinate location
            while (true)
            {
                var toDestroy = attachedBlocks.GetAttachablesAroundInRadius<IAttachable>(Vector2Int.zero, index);
                
                if(toDestroy.Count == 0)
                    break;

                foreach (var attachable in toDestroy)
                {
                    switch (attachable)
                    {
                        case Bit _:
                        case Component _:
                            attachable.gameObject.SetActive(false);

                            break;
                        case Part part:
                            part.ChangeHealth(-10000);
                            break;
                    }
                }

                yield return new WaitForSeconds(0.35f);

                index++;
            }
            
            OnBotDied?.Invoke(this, deathMethod);
        }

        #endregion //Coroutines

        //============================================================================================================//

        #region Pausable
        
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
        
        #endregion //Pausable

        //====================================================================================================================//
        
        public void SetSortingLayer(string sortingLayerName, int sortingOrder = 0)
        {
            foreach (var setSpriteLayer in attachedBlocks.OfType<ISetSpriteLayer>())
            {
                setSpriteLayer.SetSortingLayer(sortingLayerName, sortingOrder);
            }
        }

        //====================================================================================================================//
        
        public Bounds GetBounds()
        {
            throw new NotImplementedException("Refer to shape for how to implement this");
        }

        //============================================================================================================//

        #region Custom Recycle

        public void CustomRecycle(params object[] args)
        {
            transform.localScale = Vector2.one;
            isContinuousRotation = false;
            m_distanceHorizontal = 0;
            targetRotation = 0;
            _rotating = false;

            foreach (var attachable in attachedBlocks)
            {
                switch (attachable)
                {
                    case Bit _:
                        Recycler.Recycle<Bit>(attachable.gameObject);
                        break;
                    case Part _:
                        Recycler.Recycle<Part>(attachable.gameObject);
                        break;
                    case EnemyAttachable _:
                        Recycler.Recycle<EnemyAttachable>(attachable.gameObject);
                        break;
                    case Component _:
                        Recycler.Recycle<Component>(attachable.gameObject);
                        break;
                    default:
                        //throw new Exception($"No solver to recycle object {attachable.gameObject.name}");
                        throw new ArgumentOutOfRangeException(nameof(attachable), attachable.gameObject.name, null);
                }
            }
            
            attachedBlocks.Clear();
            BotPartsLogic.ClearList();
            //_parts.Clear();
            
            ObstacleManager.NewShapeOnScreen -= CheckForBonusShapeMatches;
        }
        
        #endregion //Custom Recycle
        
        //============================================================================================================//

        #region UNITY EDITOR

#if UNITY_EDITOR

        [Button]
        private void TestSpaceChange()
        {
            var target = new Vector2Int(-1, 3);

            var z = transform.rotation.eulerAngles.z;
            //var dif = z % rotationTarget;
            
            //var rotation = Quaternion.Inverse(Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z % 90));
            var rotation = Quaternion.Euler(0, 0, z);
            var newPosition = rotation * (Vector2)target;
            
            Debug.Log($"z: {z}");
            
            //SSDebug.DrawArrowRay(transform.position, newPosition, Color.yellow);
            Debug.DrawLine(transform.position, transform.position + newPosition, Color.yellow, 2f);

        }
        
        [Button]
        private void SetupTest()
        {
            var blocks = new List<BlockData>
            {
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(1, 0),
                    Level = 1,
                    Type = (int) BIT_TYPE.GREY,
                    Health = 50
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(2,0),
                    Level = 1,
                    Type = (int) BIT_TYPE.GREEN,
                    Health = 50
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(2,1),
                    Level = 1,
                    Type = (int) BIT_TYPE.GREY,
                    Health = 50
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(1,1),
                    Level = 0,
                    Type = (int) BIT_TYPE.BLUE,
                    Health = 50
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(1,2),
                    Level = 0,
                    Type = (int) BIT_TYPE.GREEN,
                    Health = 50
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(3, 1),
                    Level = 0,
                    Type = (int) BIT_TYPE.BLUE,
                    Health = 50
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(4, 1),
                    Level = 1,
                    Type = (int) BIT_TYPE.GREEN,
                    Health = 50
                },
                

            };

            AddMorePieces(blocks, false);
            CheckForCombosAround<BIT_TYPE>(attachedBlocks.OfType<ICanCombo>().ToList());
        }

        /*[Button]
        private void AddComboTestPieces()
        {
            var blocks = new List<BlockData>
            {
                new BlockData
                {
                    ClassType = nameof(Part),
                    Coordinate = new Vector2Int(0,-1),
                    Level =0,
                    Type = (int)PART_TYPE.STORERED
                },
                new BlockData
                {
                    ClassType = nameof(Part),
                    Coordinate = new Vector2Int(-1,0),
                    Level =0,
                    Type = (int)PART_TYPE.MAGNET
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(0,-2),
                    Level =0,
                    Type = (int)BIT_TYPE.YELLOW
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(1,-2),
                    Level =0,
                    Type = (int)BIT_TYPE.YELLOW
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(-1,-1),
                    Level =0,
                    Type = (int)BIT_TYPE.YELLOW
                },
            };
            
            AddMorePieces(blocks, true);
        }*/
        
        private void AddMorePieces(IEnumerable<BlockData> blocks, bool checkForCombos)
        {
            var toAdd = new List<IAttachable>();
            foreach (var blockData in blocks)
            {
                IAttachable attachable;

                switch (blockData.ClassType)
                {
                    case nameof(Bit):
                        attachable = FactoryManager.Instance.GetFactory<BitAttachableFactory>()
                            .CreateObject<Bit>(blockData);
                        break;
                    case nameof(Part):
                        attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                            .CreateObject<Part>(blockData);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(blockData.ClassType), blockData.ClassType, null);
                }
                
                toAdd.Add(attachable);
            }

            AddMorePieces(toAdd, checkForCombos);
        }
        
        private void AddMorePieces(IReadOnlyCollection<IAttachable> attachables, bool checkForCombos)
        {
            foreach (var attachable in attachables)
            {
                AttachNewBlock(attachable.Coordinate, attachable, updateMissions: false, checkForCombo: false, checkMagnet:false);
            }

            if(checkForCombos)
                CheckForCombosAround<BIT_TYPE>(attachables);
        }
        
        [Button]
        private void TestContains()
        {
            var testBlockData = new List<BlockData>
            {
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Type = (int)BIT_TYPE.RED,
                    Level = 0,
                    Coordinate = new Vector2Int(0,0)
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Type = (int)BIT_TYPE.RED,
                    Level = 0,
                    Coordinate = new Vector2Int(1,0)
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Type = (int)BIT_TYPE.RED,
                    Level = 0,
                    Coordinate = new Vector2Int(1,-1)
                },
            };
            
            var result =attachedBlocks.Contains<Bit>(testBlockData, out _);
            
            Debug.LogError($"{nameof(attachedBlocks)} contains match: {result}");


        }
#endif

        #endregion //UNITY EDITOR

        //====================================================================================================================//

    }
    
    public struct PendingCombo
    {
        public readonly ComboRemoteData ComboData;
        public readonly List<ICanCombo> ToMove;

        public PendingCombo(ComboRemoteData comboData, List<ICanCombo> toMove)
        {
            ComboData = comboData;
            ToMove = toMove;
        }
        public PendingCombo((ComboRemoteData comboData, List<ICanCombo> toMove) data)
        {
            var (comboData, toMove) = data;
            ComboData = comboData;
            ToMove = toMove;
        }

        public bool Contains(ICanCombo canCombo)
        {
            if (ToMove == null || ToMove.Count == 0)
                return false;
                
            return ToMove.Contains(canCombo);
        }

    }

    public static class PendingComboListExtensions
    {
        public static bool Contains(this IEnumerable<PendingCombo> list, ICanCombo canCombo)
        {
            return list.Any(pendingCombo => pendingCombo.Contains(canCombo));
        }
        
        public static bool Contains(this List<PendingCombo> list, ICanCombo canCombo, out int index)
        {
            index = -1;
            
            var temp = list.ToArray();
            for (var i = 0; i < temp.Length; i++)
            {
                if (!temp[i].Contains(canCombo))
                    continue;
                
                index = i;
                return true;
            }

            return false;
        }
    }
}


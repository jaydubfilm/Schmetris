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
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Analytics.SessionTracking;
using StarSalvager.Utilities.Math;
using StarSalvager.Utilities.Particles;
using AudioController = StarSalvager.Audio.AudioController;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Interfaces;
using StarSalvager.Utilities.Puzzle.Structs;
using Input = UnityEngine.Input;
using Random = UnityEngine.Random;

namespace StarSalvager
{
    [RequireComponent(typeof(BotPartsLogic))]
    public class Bot : BotBase, ICustomRecycle, IRecycled, IPausable, ISetSpriteLayer, IMoveOnInput, IHasBounds
    {
        //Structs
        //====================================================================================================================//

        #region Structs

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

        #endregion //Structs

        //Properties
        //====================================================================================================================//

        #region Properties

        public static Action<Bot, string> OnBotDied;

        public Action OnCombo;
        public Action OnFullMagnet;
        public Action OnBitShift;

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

        //Input Manager variables - -1.0f for left, 0 for nothing, 1.0f for right
        private float m_currentInput;
        private float m_distanceHorizontal = 0.0f;

        [SerializeField]
        public GameObject _decoyDronePrefab;

        [NonSerialized]
        public DecoyDrone DecoyDrone;

        //============================================================================================================//

        public Vector2 ShootAtPosition => DecoyDrone != null ? DecoyDrone.transform.position : transform.position;
        

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

        public override bool Rotating => _rotating;
        public bool ContinousRotation => isContinuousRotation;
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

        private float _dashCooldown;

        #endregion //Properties

        //IHealth Test
        //====================================================================================================================//

        public override void SetupHealthValues(float startingHealth, float currentHealth)
        {
            base.SetupHealthValues(startingHealth, currentHealth);

            GameUi.SetHealthValue(CurrentHealth / StartingHealth);
        }

        public override void ChangeHealth(float amount)
        {
            if (amount == 0) return;
            
            var addsHealth = amount > 0;
            
            CurrentHealth += amount;

            //TODO Need to update UI
            GameUi.SetHealthValue(CurrentHealth / StartingHealth);

            //Here we check to make sure to not display tiny values of damage
            var check = Mathf.Abs(amount);
            if(!(check > 0 && check < 1f))
                FloatingText.Create($"{amount}", transform.position, addsHealth ? Color.green : Color.red);

            //Display hint if damaged & has resources to heal
            //--------------------------------------------------------------------------------------------------------//
            
            if (addsHealth == false && HintManager.CanShowHint(HINT.HEALTH))
            {
                if (PlayerDataManager.GetResource(BIT_TYPE.GREEN).Ammo > 0)
                    HintManager.TryShowHint(HINT.HEALTH, 0.5f);
            }

            //--------------------------------------------------------------------------------------------------------//
            
            if (CurrentHealth > 0)
                return;

            CreateCoreDeathEffect();

            cinemachineImpulseSource.GenerateImpulse(5);
            GameUi.FlashNeonBorder();

            Destroy("Core Destroyed");
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

            //See if the bot has completed the current wave
            //FIXME I Don't like accessing the external value here. I should consider other ways of checking this value
            if (GameManager.IsState(GameState.LevelEndWave))
                return;

            if (Destroyed)
                return;

            TryMovement();

            UpdateFollowTarget(transform.position);

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

            if (_dashCooldown > 0)
            {
                _dashCooldown -= Time.deltaTime;
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

        //Core Shuffle Prototype
        //====================================================================================================================//

        private bool _isShifting;

        public void CoreShuffle(DIRECTION direction)
        {
            //FIXME Might want to store this at the start, so I don't have to keep searching for it
            var corePart = AttachedBlocks.OfType<Part>().FirstOrDefault(x => x.Type == PART_TYPE.CORE);
            
            var start = AttachedBlocks.GetAttachableInDirection(corePart, direction.Reflected());

            //Checks to see if the shuffle will cause a disconnect, and prevents it
            if (Globals.ShuffleCanDisconnect == false && start is ISaveable saveable && DoesShiftCauseDisconnect(direction, saveable.ToBlockData()))
                return;
            
            TryShift(direction, start);
        }

        //IMoveOnInput
        //============================================================================================================//

        #region IMoveOnInput Functions

        private void TryMovement()
        {
            float blackHoleAmount = GetBlackHoleMovementImpact() * Time.deltaTime;
            transform.position += Vector3.left * blackHoleAmount;
            m_distanceHorizontal -= blackHoleAmount;

            var currentPosition = transform.position;

            //var xPos = transform.position.x;

            var distHorizontal = Mathf.Abs(m_distanceHorizontal);
            DIRECTION direction;


            bool canMove;
            if (m_distanceHorizontal < 0)
            {
                direction = DIRECTION.LEFT;
                canMove = currentPosition.x > -0.5f * Constants.gridCellSize * Globals.GridSizeX;
            }
            else if (m_distanceHorizontal > 0)
            {
                direction = DIRECTION.RIGHT;
                canMove = currentPosition.x < 0.5f * Constants.gridCellSize * Globals.GridSizeX;
            }
            else
            {
                canMove = false;
                distHorizontal = 0f;
                direction = DIRECTION.NULL;
            }

            distHorizontal = canMove ? distHorizontal : 0f;
            
            //--------------------------------------------------------------------------------------------------------//


            Globals.MovingDirection = distHorizontal <= 0.2f
                ? DIRECTION.NULL
                : direction;

            if (_isDashing && Globals.MovingDirection == DIRECTION.NULL)
            {
                _isDashing = false;
                CanBeDamaged = true;
                SetColliderActive(true);
                _dashCooldown = Globals.DashCooldown;
            }

            if (!canMove)
            {
                currentPosition.x = Mathf.RoundToInt(currentPosition.x);
                transform.position = currentPosition;
                return;
            }

            var moveSpeed = _isDashing ? Globals.DashSpeed : Globals.BotHorizontalSpeed;

            var toMove = Mathf.Min(distHorizontal, moveSpeed * Time.deltaTime);

            var moveDirection = direction.ToVector2();

            m_distanceHorizontal -= toMove * moveDirection.x;

            transform.position += (Vector3)moveDirection * toMove;

            //--------------------------------------------------------------------------------------------------------//

        }

        public float GetBlackHoleMovementImpact()
        {
            float blackHoldImpact = 0.0f;
            List<BlackHole> blackHoles = LevelManager.Instance.ObstacleManager.GetAllBlackHoles();

            float maxPull = FactoryManager.Instance.GetFactory<BlackHoleFactory>().GetBlackHoleMaxPull();
            float maxDistance = FactoryManager.Instance.GetFactory<BlackHoleFactory>().GetBlackHoleMaxDistance();
            for (int i = 0; i < blackHoles.Count; i++)
            {
                float distance = Vector2.Distance(blackHoles[i].transform.position, transform.position);
                if (distance > maxDistance)
                {
                    continue;
                }

                if (blackHoles[i].transform.position.x > transform.position.x)
                {
                    blackHoldImpact -= distance;
                }
                else
                {
                    blackHoldImpact += distance;
                }
            }

            return blackHoldImpact;
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
            var toAdd = direction * Constants.gridCellSize;

            if (_isDashing)
                return;

            m_distanceHorizontal += toAdd;
        }

        public bool IsDashing => _isDashing;
        private bool _isDashing;

        public void Dash(in DIRECTION direction, in int distance)
        {
            switch (direction)
            {
                case DIRECTION.LEFT:
                    Dash(-1, distance);
                    break;
                case DIRECTION.RIGHT:
                    Dash(1, distance);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public void Dash(in float direction, in int distance)
        {
            if (_isDashing)
                return;

            if (_dashCooldown > 0f)
                return;
            
            //Check to see if the player is able to dash in the intended Direction
            //--------------------------------------------------------------------------------------------------------//
            
            var currentPosition = transform.position;
            bool canDash = true;
            
            if (direction < 0)
            {
                canDash = currentPosition.x > (Constants.gridCellSize * -Globals.GridSizeX) + distance;
            }
            else if (direction > 0)
            {
                canDash = currentPosition.x < (Constants.gridCellSize * Globals.GridSizeX) - distance;
            }
            
            //TODO An alternative to consider is to set the dash distance to the remaining distance
            if(canDash == false)
                return;

            //--------------------------------------------------------------------------------------------------------//
            
            _isDashing = true;
            CanBeDamaged = false;
            SetColliderActive(false);

            m_distanceHorizontal += direction * Constants.gridCellSize * distance;
        }

        private void SetColliderActive(bool state)
        {
            AttachedBlocks.OfType<CollidableBase>().ToList().ForEach(x => x.SetColliderActive(state));
        }


        #endregion //IMoveOnInput Functions

        //============================================================================================================//

        #region Init Bot

        public void InitBot()
        {
            CreateFollowTarget();

            _weldDatas = new List<WeldData>();

            var partFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();

            _isDestroyed = false;
            CompositeCollider2D.enabled = true;

            //Add core component
            //var patchSockets = partFactory.GetRemoteData(PART_TYPE.CORE).PatchSockets;
            var core = partFactory.CreateObject<Part>(
                new PartData
                {
                    Type = (int)PART_TYPE.EMPTY,
                    Coordinate = Vector2Int.zero,
                    //Patches = new PatchData[patchSockets]
                });

            AttachNewBlock(Vector2Int.zero, core);

            var botLayout = PlayerDataManager.GetBotLayout();
            for (int i = 0; i < botLayout.Count; i++)
            {
                if (botLayout[i] == Vector2Int.zero)
                    continue;
                
                if (AttachedBlocks != null && AttachedBlocks.Any(b => b.Coordinate == botLayout[i]))
                {
                    continue;
                }

                var emptyPart = partFactory.CreateObject<Part>(
                    new PartData
                    {
                        Type = (int)PART_TYPE.EMPTY,
                        Coordinate = botLayout[i],
                    });
                emptyPart.gameObject.name = $"{PART_TYPE.EMPTY}_{botLayout[i]}";

                AttachNewBlock(botLayout[i], emptyPart);
            }

            ObstacleManager.NewShapeOnScreen += CheckForBonusShapeMatches;

            GameUi.SetHealthValue(1f);
            //GameUi.SetPartImages(BotPartsLogic.GetPartStates());

            var camera = CameraController.Camera.GetComponent<CameraController>();
            camera.SetLookAtFollow(_followTarget.transform);
            camera.ResetCameraPosition();
        }

        public void InitBot(IEnumerable<IAttachable> botAttachables)
        {
            CreateFollowTarget();

            _weldDatas = new List<WeldData>();

            _isDestroyed = false;
            CompositeCollider2D.enabled = true;

            //Only want to update the parts list after everyone has loaded
            foreach (var attachable in botAttachables)
            {
                AttachNewBlock(attachable.Coordinate, attachable, updatePartList: false, checkForCombo: false);
            }



            var camera = CameraController.Camera.GetComponent<CameraController>();
            camera.SetLookAtFollow(_followTarget.transform);
            camera.ResetCameraPosition();

            BotPartsLogic.PopulatePartsList();

            CheckAllForCombos();
            //GameUi.SetPartImages(BotPartsLogic.GetPartStates());

        }

        public void DisplayHints()
        {
            /*if(HintManager.CanShowHint(HINT.GUN) && AttachedBlocks.HasPartAttached(PART_TYPE.GUN))
                HintManager.TryShowHint(HINT.GUN);*/
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

            foreach (var attachedBlock in AttachedBlocks)
            {
                attachedBlock.RotateCoordinate(rotation);
            }

            _rotating = true;
        }

        public void ResetRotationToIdentity()
        {
            var rotate = false;
            foreach (var part in AttachedBlocks.OfType<Part>())
            {
                if(part.Type == PART_TYPE.EMPTY)
                    continue;
                if (part.category == PlayerDataManager.GetCategoryAtCoordinate(part.Coordinate)) 
                    continue;
                
                rotate = true;
                break;
            }
            
            if(rotate == false)
                return;
            
            /*if (!AttachedBlocks.Any(b => b is Part part &&
                                         part.Type != PART_TYPE.EMPTY &&
                                         part.category !=
                                         PlayerDataManager.GetCategoryAtCoordinate(part.Coordinate))) 
                return;*/
            
            foreach (var attachedBlock in AttachedBlocks)
            {
                attachedBlock.RotateCoordinate(ROTATION.CW);
            }

            ResetRotationToIdentity();
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

            foreach (var attachedBlock in AttachedBlocks)
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

        //====================================================================================================================//

        #region Follow Target

        private static GameObject _followTarget;

        private static void CreateFollowTarget()
        {
            if(_followTarget == null)
                _followTarget = new GameObject("Bot_Camera-Follow-Target");

            _followTarget.transform.position = Vector3.up * 5f;
        }

        private static void UpdateFollowTarget(Vector3 desiredPosition)
        {
            var followPos = _followTarget.transform.position;
            followPos.x = desiredPosition.x;
            _followTarget.transform.position = followPos;
        }

        #endregion //Follow Target

        //============================================================================================================//

        #region TryAddNewAttachable

        public override bool TryAddNewAttachable(IAttachable attachable, DIRECTION connectionDirection, Vector2 collisionPoint)
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
                    //------------------------------------------------------------------------------------------------//

                    closestAttachable = AttachedBlocks.GetClosestAttachable(collisionPoint);

                    //------------------------------------------------------------------------------------------------//


                    //Check if its legal to attach (Within threshold of connection)
                    switch (bit.Type)
                    {
                        case BIT_TYPE.GREEN:
                        case BIT_TYPE.BLUE:
                        case BIT_TYPE.GREY:
                        case BIT_TYPE.RED:
                        case BIT_TYPE.YELLOW:
                        case BIT_TYPE.WHITE:
                            //TODO This needs to bounce off instead of being destroyed
                            if (closestAttachable is EnemyAttachable /*||
                                closestAttachable is Part part && part.Destroyed*/)
                            {
                                if (attachable is IObstacle obstacle)
                                    obstacle.Bounce(collisionPoint, transform.position);

                                return false;
                            }

                            //Add these to the block depending on its relative position
                            AttachAttachableToExisting(bit, closestAttachable, connectionDirection);

                            //If this bit was dropped by an enemy, gain ammo (& points?) for having collected it 
                            //--------------------------------------------------------------------------------------------------------//
                            
                            if (bit.toBeCollected)
                            {
                                //TODO Get the value and add
                                var ammoEarned = Mathf.CeilToInt(FactoryManager.Instance.ComboRemoteData.ComboAmmos
                                    .FirstOrDefault(x => x.level == bit.level)
                                    .ammoEarned * Globals.BitDropCollectionMultiplier);

                                if (ammoEarned != 0)
                                {
                                    //PlayerDataManager.GetResource(bit.Type).AddAmmo(ammoEarned);
                                    GameUi.CreateAmmoEffect(bit.Type, ammoEarned, bit.Position);
                                    FloatingText.Create($"+{ammoEarned}", bit.Position, bit.Type.GetColor());
                                }

                                bit.toBeCollected = false;
                            }

                            //--------------------------------------------------------------------------------------------------------//
                            
                            CheckForBonusShapeMatches();

                            AudioController.PlayBitConnectSound(bit.Type);

                            SessionDataProcessor.Instance.RecordBitConnected(bit.ToBlockData());
                            PlayerDataManager.RecordBitConnection(bit.Type);

                            break;
                        case BIT_TYPE.BUMPER:
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
                            //AudioController.PlayDelayedSound(SOUND.SLIDING_BITS, 0.6f);
                            SessionDataProcessor.Instance.HitBumper();

                            if(shift) OnBitShift?.Invoke();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(bit.Type), bit.Type, null);
                    }

                    break;
                }
                //FIXME This seems to be wanting to attach to the wrong direction
                case EnemyAttachable enemyAttachable:
                {
                    //Get the coordinate of the collision
                    var bitCoordinate = GetRelativeCoordinate(enemyAttachable.transform.position);

                    //----------------------------------------------------------------------------------------------------//

                    closestAttachable = AttachedBlocks.GetClosestAttachable(collisionPoint);

                    if (closestAttachable is EnemyAttachable)
                    {
                        return false;
                    }

                    if (enemyAttachable is BorrowerEnemy borrowerEnemy && !(closestAttachable is Bit bit))
                    {
                        closestAttachable = borrowerEnemy.FindClosestBitOnBot();
                        if (closestAttachable == null)
                        {
                            return false;
                        }
                    }

                    //FIXME This isn't sufficient to prevent multiple parasites using the same location
                    var potentialCoordinate = closestAttachable.Coordinate + connectionDirection.ToVector2Int();
                    if (AttachedBlocks.Count(x => x.Coordinate == potentialCoordinate) > 1)
                        return false;

                    /*legalDirection = CheckLegalCollision(bitCoordinate, closestAttachable.Coordinate, out _);

                    //----------------------------------------------------------------------------------------------------//

                    if (!legalDirection)
                    {
                        //Make sure that the attachable isn't overlapping the bot before we say its impossible to
                        if (!CompositeCollider2D.OverlapPoint(attachable.transform.position))
                            return false;
                    }*/

                    //Add these to the block depending on its relative position
                    AttachAttachableToExisting(enemyAttachable, closestAttachable, connectionDirection);
                    break;
                }
                case JunkBit junkBit:
                {
                    bool legalDirection = true;

                    //----------------------------------------------------------------------------------------------------//

                    closestAttachable = AttachedBlocks.GetClosestAttachable(collisionPoint);

                    //Check if its legal to attach (Within threshold of connection)
                    if (closestAttachable is EnemyAttachable /*||
                        closestAttachable is Part part && part.Destroyed*/)
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
                case Crate crate:
                {
                    bool legalDirection = true;

                    //----------------------------------------------------------------------------------------------------//

                    closestAttachable = AttachedBlocks.GetClosestAttachable(collisionPoint);

                    //Check if its legal to attach (Within threshold of connection)
                    if (closestAttachable is EnemyAttachable /*||
                        closestAttachable is Part part && part.Destroyed*/)
                    {
                        if (attachable is IObstacle obstacle)
                            obstacle.Bounce(collisionPoint, transform.position);

                        return false;
                    }

                    //Add these to the block depending on its relative position
                    AttachAttachableToExisting(crate, closestAttachable, connectionDirection);

                    //CheckForCombosAround();
                    break;
                }
            }

            if (!(attachable is EnemyAttachable) && (attachable is Bit bitCheck && bitCheck.Type != BIT_TYPE.BUMPER))
            {
                _weldDatas.Add(new WeldData
                {
                    target = attachable,
                    attachedTo = closestAttachable
                });
            }

            return true;
        }

        public IAttachable GetClosestAttachable(Vector2 worldPosition, float maxDistance = 999f)
        {
            IAttachable selected = null;

            var smallestDist = 999f;

            foreach (var attached in AttachedBlocks)
            {
                //attached.SetColor(Color.white);
                if (attached.CountAsConnectedToCore == false)
                    continue;

                var dist = Vector2.Distance(attached.transform.position, worldPosition);

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

        public override IAttachable GetClosestAttachable(Vector2Int checkCoordinate, float maxDistance = 999f)
        {
            IAttachable selected = null;

            var smallestDist = 999f;

            foreach (var attached in AttachedBlocks)
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

        #endregion //Check For Legal Attach

        //============================================================================================================//

        #region Check for Legal Shape Attach

        public bool TryAddNewShape(Shape shape, IAttachable closestShapeBit, DIRECTION connectionDirection, Vector2 collisionPoint)
        {
            if (_isDestroyed)
                return false;

            if (Rotating)
                return false;

            var closestOnBot= AttachedBlocks.GetClosestAttachable(collisionPoint);



            if (closestShapeBit is Bit closeBitOnShape)
            {
                switch (closeBitOnShape.Type)
                {
                    case BIT_TYPE.BLUE:
                    case BIT_TYPE.GREEN:
                    case BIT_TYPE.GREY:
                    case BIT_TYPE.RED:
                    case BIT_TYPE.YELLOW:
                    case BIT_TYPE.WHITE:

                        //TODO This needs to bounce off instead of being destroyed
                        if (closestOnBot is EnemyAttachable /*||
                            closestOnBot is Part part && part.Destroyed*/)
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
                            if (AttachedBlocks.Any(x => x.Coordinate == newBotCoordinate + differences[i]))
                            {

                                Debug.LogError($"Conflict found at {newBotCoordinate + differences[i]}");
                                //Debug.Break();
                                //Recycler.Recycle<Shape>(shape);

                                return false;
                            }

                            AttachNewBlock(newBotCoordinate + differences[i], bitsToAdd[i], false, false);
                            SessionDataProcessor.Instance.RecordBitConnected(bitsToAdd[i].ToBlockData());
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
                        AttachedChanged();

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
                var check = AttachedBlocks.FirstOrDefault(x =>
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
        public bool TryAsteroidBounceAt(in Vector2 hitPosition, in float damage, out bool destroyed)
        {
            destroyed = false;

            var closestAttachable = AttachedBlocks.GetClosestAttachable(hitPosition);

            switch (closestAttachable)
            {
                case EnemyAttachable enemyAttachable:
                    //If the enemy is knocked, but not killed we want them to act as if they were bumped
                    enemyAttachable.OnBumped();
                    break;
            }

            TryHitAt(closestAttachable, damage);

            if (!(closestAttachable is IHealth iHealth) || iHealth.CurrentHealth > 0) 
                return true;
            
            destroyed = true;
            AudioController.PlaySound(SOUND.ASTEROID_CRUSH);

            return true;
        }

        public void TryHitAt(in float damage)
        {
            var part = AttachedBlocks.FirstOrDefault(x => x is Part);
            TryHitAt(part, damage);
        }
        
        public override bool TryHitAt(Vector2 worldPosition, float damage)
        {
            SessionDataProcessor.Instance.ReceivedDamage(damage);

            if(!GameManager.IsState(GameState.LEVEL_ACTIVE))
                return false;

            var closestAttachable = AttachedBlocks.GetClosestAttachable(worldPosition);

            switch (closestAttachable)
            {
                // Enemies attached should not be hit by other enemy projectiles
                case EnemyAttachable _:
                /*case Part part when part.Destroyed:*/
                    return false;
            }

            /*var explosion = FactoryManager.Instance.GetFactory<EffectFactory>().CreateObject<Explosion>();
            explosion.transform.position = worldPosition;*/

            TryHitAt(closestAttachable, damage);

            return true;
        }

        public override void TryHitAt(IAttachable closestAttachable, float damage, bool withSound = true)
        {
            if (!CanBeDamaged && closestAttachable.Coordinate == Vector2Int.zero)
                return;
            
            BotPartsLogic.TryHitArmor(ref damage);

            //--------------------------------------------------------------------------------------------------------//

            if (damage <= 0f) return;

            //--------------------------------------------------------------------------------------------------------//
            
            IHealth closestHealth;
            
            switch (closestAttachable)
            {
                case Part _:
                    closestHealth = this;
                    BotPartsLogic.ResetHealCooldown();
                    break;
                default:
                    closestHealth = (IHealth) closestAttachable;
                    break;
            }

            //--------------------------------------------------------------------------------------------------------//
            var applyDamage = -Mathf.Abs(damage);
            var attachableDestroyed = closestHealth.CurrentHealth + applyDamage <= 0f;

            switch (closestAttachable)
            {
                case Bit _ when withSound:
                    AudioController.PlaySound(attachableDestroyed ? SOUND.BIT_EXPLODE : SOUND.BIT_DAMAGE);
                    closestHealth.ChangeHealth(applyDamage);
                    break;
                case Part _:
                    if(withSound) AudioController.PlaySound(SOUND.PART_DAMAGE);
                    //If something hit a part, we actually want to damage the bot as a whole
                    ChangeHealth(applyDamage);
                    return;
                case EnemyAttachable enemyAttachable:

                    closestHealth.ChangeHealth(applyDamage);
                    break;
                default:
                    closestHealth.ChangeHealth(applyDamage);
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
                    break;
            }
            
            DestroyAttachable(closestAttachable);
            //After deleting see if there are new combds
            CheckAllForCombos();

            if (closestAttachable.CountTowardsMagnetism)
                ForceCheckMagnets();



            if (closestAttachable.Coordinate != Vector2Int.zero)
                CheckForDisconnects();

            //------------------------------------------------------------------------------------------------//
        }

        public void TryAOEDamageFrom(in Vector2 worldPosition, in float radius, in float damage, in bool partsOnly = false)
        {
            var blocksToDamage = AttachedBlocks.GetAttachablesWhichIntersectCircle(worldPosition, radius);

            if (blocksToDamage.IsNullOrEmpty())
                return;

            if (partsOnly)
            {
                var parts = blocksToDamage.OfType<Part>();
                foreach (var part in parts)
                {
                    TryHitAt(part, damage);
                }

                return;
            }

            //Dont want to stack damage for parts, so just pick the first part
            foreach (var attachable in blocksToDamage)
            {
                TryHitAt(attachable, damage);
            }

        }



        #endregion //TryHitAt

        public List<IAttachable> GetAttachablesInColumn(in Vector2 worldHitPoint)
        {
            var column = AttachedBlocks.GetClosestAttachable(worldHitPoint)?.Coordinate.x;

            return column.HasValue ? AttachedBlocks.Where(x => x.Coordinate.x == column.Value).ToList() : null;
        }

        //============================================================================================================//

        #region Attach Blocks

        public override bool TryAttachNewBlock(Vector2Int coordinate, IAttachable newAttachable,
            bool checkForCombo = true,
            bool updateColliderGeometry = true,
            bool updatePartList = true)
        {
            if (Destroyed)
                return false;

            if (AttachedBlocks.Any(x => x.Coordinate == coordinate))
                return false;

            newAttachable.Coordinate = coordinate;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * coordinate * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            //newAttachable.gameObject.name = $"Block {attachedBlocks.Count}";

            //We want to avoid having the same element multiple times in the list
            if(!AttachedBlocks.Contains(newAttachable))
                AttachedBlocks.Add(newAttachable);

            switch (newAttachable)
            {
                case Bit _:
                    if(checkForCombo) CheckForCombosAround(coordinate);

                    break;
                /*case Component _ when checkForCombo:
                    CheckForCombosAround<COMPONENT_TYPE>(coordinate);
                    break;*/
                case Part _ when updatePartList:
                    BotPartsLogic.PopulatePartsList();
                    break;
            }

            if (newAttachable.CountTowardsMagnetism)
                _needToCheckMagnet = true;//AudioController.PlaySound(CheckHasMagnetOverage() ? SOUND.BIT_RELEASE : SOUND.BIT_SNAP);

            if(updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();

            AttachedChanged();

            return true;
        }

        public override void AttachNewBlock(Vector2Int coordinate, IAttachable newAttachable,
            bool checkForCombo = true,
            bool updateColliderGeometry = true,
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
            if(!AttachedBlocks.Contains(newAttachable))
                AttachedBlocks.Add(newAttachable);

            switch (newAttachable)
            {
                case Bit _:
                    if(checkForCombo) CheckForCombosAround(coordinate);
                    break;
                /*case Component _ when checkForCombo:
                    CheckForCombosAround<COMPONENT_TYPE>(coordinate);
                    break;*/
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

            /*if(checkForCombo)
                CheckForCombosAround(coordinate);*/

            if(updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();

            AttachedChanged();
        }

        public override void AttachAttachableToExisting(IAttachable newAttachable, IAttachable existingAttachable,
            DIRECTION direction,
            bool checkForCombo = true,
            bool updateColliderGeometry = true,
            bool checkMagnet = true,
            bool playSound = true,
            bool updatePartList = true)
        {
            if (Destroyed)
                return;

            if (newAttachable is BorrowerEnemy)
            {
                direction = GetAvailableConnectionDirection(existingAttachable.Coordinate, direction);
            }

            var coordinate = existingAttachable.Coordinate + direction.ToVector2Int();

            //Checks for attempts to add attachable to occupied location
            if (AttachedBlocks.Any(a => a.Coordinate == coordinate /*&& !(a is Part part && part.Destroyed)*/))
            {
                var onAttachable = AttachedBlocks.FirstOrDefault(a => a.Coordinate == coordinate);
                Debug.Log(
                    $"Prevented attaching {newAttachable.gameObject.name} to occupied location {coordinate}\n Occupied by {onAttachable.gameObject.name}",
                    newAttachable.gameObject);

                if (newAttachable is BorrowerEnemy)
                {
                    return;
                }

                AttachToClosestAvailableCoordinate(coordinate,
                    newAttachable,
                    direction,
                    checkForCombo,
                    updateColliderGeometry);

                /*//I don't want the enemies to push to the end of the arm, I want it just attach to the closest available space
                if (newAttachable is EnemyAttachable)
                    AttachToClosestAvailableCoordinate(coordinate,
                        newAttachable,
                        direction,
                        checkForCombo,
                        updateColliderGeometry);
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
            if(!AttachedBlocks.Contains(newAttachable))
                AttachedBlocks.Add(newAttachable);

            switch (newAttachable)
            {
                case Bit _:
                    if (checkForCombo)
                        CheckForCombosAround(coordinate);

                    /*if(existingAttachable is Part part)
                        TryAutoProcessBit(bit, part);*/
                    
                    AttachedChanged();
                    break;
                case Part _ when updatePartList:
                    BotPartsLogic.PopulatePartsList();
                    break;
                case Crate _ when checkForCombo:
                    throw new NotImplementedException();
                    //CheckForCombosAround<CRATE_TYPE>(coordinate);
                    break;
            }

            if (newAttachable.CountTowardsMagnetism && checkMagnet)
            {
                _needToCheckMagnet = true;

            }


            if (updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();
        }

        private DIRECTION GetAvailableConnectionDirection(Vector2Int existingAttachableCoordinate, DIRECTION direction)
        {
            var coordinate = existingAttachableCoordinate + direction.ToVector2Int();
            //Checks for attempts to add attachable to occupied location
            if (!AttachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                return direction;
            }

            coordinate = existingAttachableCoordinate + DIRECTION.UP.ToVector2Int();
            //Checks for attempts to add attachable to occupied location
            if (!AttachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                return DIRECTION.UP;
            }

            coordinate = existingAttachableCoordinate + DIRECTION.RIGHT.ToVector2Int();
            //Checks for attempts to add attachable to occupied location
            if (!AttachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                return DIRECTION.RIGHT;
            }

            coordinate = existingAttachableCoordinate + DIRECTION.LEFT.ToVector2Int();
            //Checks for attempts to add attachable to occupied location
            if (!AttachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                return DIRECTION.LEFT;
            }

            coordinate = existingAttachableCoordinate + DIRECTION.DOWN.ToVector2Int();
            //Checks for attempts to add attachable to occupied location
            if (!AttachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                return DIRECTION.DOWN;
            }

            return direction;
        }


        /*private void TryAutoProcessBit(Bit bit, IPart part)
        {
            switch (part.Type)
            {
                //case PART_TYPE.CORE when PROTO_autoRefineFuel && bit.Type == BIT_TYPE.RED:
                case PART_TYPE.REFINER when !part.Disabled:

                    break;
                default:
                    float curLiquid = PlayerDataManager.GetResource(bit.Type).liquid;
                    float curLiquidCapacity = PlayerDataManager.GetResource(bit.Type).liquidCapacity;
                    if (curLiquid / curLiquidCapacity > 0.5f)
                    {
                        return;
                    }
                    break;
            }


            CheckForDisconnects();
        }*/

        //FIXME Ensure that I have a version of this function without the desiredDirection, and one that accounts for corners
        /// <summary>
        /// Attaches the newAttachable to the closest available location LEFT, UP, RIGHT, DOWN in an incrementing radius
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="newAttachable"></param>
        /// <param name="desiredDirection"></param>
        /// <param name="checkForCombo"></param>
        /// <param name="updateColliderGeometry"></param>
        public void AttachToClosestAvailableCoordinate(Vector2Int coordinate, IAttachable newAttachable,
            DIRECTION desiredDirection, bool checkForCombo,
            bool updateColliderGeometry)
        {
            if (Destroyed)
                return;

            var directions = new[]
            {
                //Cardinal Directions
                Vector2Int.left,
                new Vector2Int(-1, 1),
                
                Vector2Int.up,
                new Vector2Int(1, 1),
                
                Vector2Int.right,
                new Vector2Int(1, -1),
                
                Vector2Int.down,
                new Vector2Int(-1, -1),
            };

            var avoid = desiredDirection.Reflected().ToVector2Int();

            var dist = 1;
            while (true)
            {
                for (var i = 0; i < directions.Length; i++)
                {

                    var check = coordinate + (directions[i] * dist);
                    if (AttachedBlocks.Any(x => x.Coordinate == check))
                        continue;

                    //We need to make sure that the piece wont be floating
                    if (!AttachedBlocks.HasPathToCore(check))
                        continue;
                    //Debug.Log($"Found available location for {newAttachable.gameObject.name}\n{coordinate} + ({directions[i]} * {dist}) = {check}");
                    AttachNewBlock(check, newAttachable, checkForCombo, updateColliderGeometry);
                    return;
                }

                if (dist++ > 10)
                    break;

            }
        }
        
        public void AttachToClosestAvailableCoordinate(
            Vector2Int coordinate, 
            IAttachable newAttachable,
            Vector2 rawDirection, 
            bool checkForCombo,
            bool updateColliderGeometry)
        {
            /*if (Destroyed)
                return;
            
            var directions = new List<Vector2>();

            var avoid = desiredDirection.Reflected().ToVector2Int();

            var dist = 1;
            while (true)
            {
                for (var i = 0; i < directions.Length; i++)
                {

                    var check = coordinate + (directions[i] * dist);
                    if (attachedBlocks.Any(x => x.Coordinate == check))
                        continue;

                    //We need to make sure that the piece wont be floating
                    if (!attachedBlocks.HasPathToCore(check))
                        continue;
                    //Debug.Log($"Found available location for {newAttachable.gameObject.name}\n{coordinate} + ({directions[i]} * {dist}) = {check}");
                    AttachNewBlock(check, newAttachable, checkForCombo, updateColliderGeometry);
                    return;
                }

                if (dist++ > 10)
                    break;

            }*/
        }

        public void PushNewAttachable(IAttachable newAttachable, DIRECTION direction, bool checkForCombo = true, bool updateColliderGeometry = true, bool checkMagnet = true, bool playSound = true)
        {
            if (Destroyed)
                return;

            var newCoord = direction.ToVector2Int();

            AttachedBlocks.FindUnoccupiedCoordinate(direction, ref newCoord);

            newAttachable.Coordinate = newCoord;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * newCoord * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            AttachedBlocks.Add(newAttachable);

            switch (newAttachable)
            {
                case Bit _ when checkForCombo:
                    CheckForCombosAround(newCoord);
                    break;
                case Crate _ when checkForCombo:
                    throw new NotImplementedException();
                    //CheckForCombosAround<CRATE_TYPE>(newCoord);
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

            AttachedChanged();
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

            AttachedBlocks.FindUnoccupiedCoordinate(direction, ref newCoord);

            newAttachable.Coordinate = newCoord;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * newCoord * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            AttachedBlocks.Add(newAttachable);

            /*if (checkForCombo)
            {
                CheckForCombosAround(newCoord);

            }*/

            switch (newAttachable)
            {
                case Bit _ when checkForCombo:
                    CheckForCombosAround(newCoord);

                    break;
                case Crate _ when checkForCombo:
                    throw new NotImplementedException();
                    //CheckForCombosAround<CRATE_TYPE>(newCoord);
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

        public override void ForceDetach(ICanDetach canDetach)
        {
            DetachSingleBlock(canDetach);
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
                AttachedBlocks.Remove(canDetach.iAttachable);
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
                        shape.DisableColliderTillLeaves(CompositeCollider2D);
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
                        bit.DisableColliderTillLeaves(CompositeCollider2D);

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
                    collidableBase.DisableColliderTillLeaves(CompositeCollider2D);
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
            AttachedChanged();

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
            AttachedBlocks.Remove(attachable);
            attachable.SetAttached(false);
            
            CheckForDisconnects();

            CompositeCollider2D.GenerateGeometry();
            CheckForBonusShapeMatches();
            AttachedChanged();
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
            AttachedBlocks.Remove(attachable);
            attachable.SetAttached(false);

            Recycler.Recycle<T>(attachable.gameObject);

            CheckForDisconnects();

            CompositeCollider2D.GenerateGeometry();

            AttachedChanged();
        }

        #endregion //Detach Bits

        public void MarkAttachablePendingRemoval(IAttachable attachable)
        {
            AttachedBlocks.Remove(attachable);

            CheckForDisconnects();
        }

        [Obsolete]
        private void AttachedChanged()
        {
            //var bitTypes = new[]
            //{
            //    BIT_TYPE.RED,
            //    BIT_TYPE.YELLOW,
            //    BIT_TYPE.GREY,
            //    BIT_TYPE.BLUE,
            //    BIT_TYPE.GREEN
            //};
//
            //BotPartsLogic.PopulatePartsList();
            //var outData = new Dictionary<BIT_TYPE, int>();
            //foreach (var bitType in bitTypes)
            //{
            //    var level = attachedBlocks.GetHighestLevelBit(bitType);
//
            //    outData.Add(bitType, level);
            //}
//
            //GameUi.SetBitLevelImages(outData);
            //GameUi.SetPartImages(BotPartsLogic.GetPartStates());
        }

        //============================================================================================================//

        #region Check for New Disconnects

        /// <summary>
        /// Function will review and detach any blocks that no longer have a connection to the core.
        /// </summary>
        private bool CheckForDisconnects()
        {
            var toSolve = new List<ICanDetach>(AttachedBlocks.OfType<ICanDetach>());
            bool hasDetached = false;

            foreach (var canDetach in toSolve)
            {
                /*if (!attachedBlocks.Contains(attachable))
                    continue;*/

                var hasPathToCore = AttachedBlocks.HasPathToCore(canDetach.iAttachable);

                if(hasPathToCore)
                    continue;

                hasDetached = true;

                var detachables = new List<ICanDetach>();
                AttachedBlocks.GetAllConnectedDetachables(canDetach, null, ref detachables);

                foreach (var attachedBit in detachables.OfType<Bit>())
                {
                    SessionDataProcessor.Instance.RecordBitDetached(attachedBit.ToBlockData());
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
            var toSolve = new List<ICanDetach>(AttachedBlocks.OfType<ICanDetach>());
            var ignoreCoordinates = wantToRemove?.Select(x => x.Coordinate).ToList();

            foreach (var canDetach in toSolve)
            {
                if (!canDetach.iAttachable.CountAsConnectedToCore)
                    continue;

                //if (!attachedBlocks.Contains(attachable))
                //    continue;

                if (wantToRemove != null && wantToRemove.Contains(canDetach))
                    continue;

                var hasPathToCore = AttachedBlocks.HasPathToCore(canDetach.iAttachable, ignoreCoordinates);

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
            if (attachable is EnemyAttachable enemyAttachable)
            {
                enemyAttachable.OnBumped();
                return true;
            }
            
            List<IAttachable> inLine;
            switch (direction)
            {
                case DIRECTION.LEFT:
                case DIRECTION.RIGHT:
                    inLine = AttachedBlocks.Where(ab => ab.Coordinate.y == attachable.Coordinate.y).ToList();
                    break;
                case DIRECTION.UP:
                case DIRECTION.DOWN:
                    inLine = AttachedBlocks.Where(ab => ab.Coordinate.x == attachable.Coordinate.x).ToList();
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
                    /*//FIXME This will work for parasites, but nothing else
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
                    }*/

                    currentCoordinate += dir;
                    continue;
                }

                toShift.Add(new ShiftData(targetAttachable, currentCoordinate));
            }

            if (toShift.Count == 0)
                return false;

            bool hasDetached = false;
            bool hasCombos = false;

_isShifting = true;
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

                _isShifting = false;
            }));


            return true;
        }

        private struct SimpleShiftData
        {
            public Vector2Int StartCoordinate;
            public Vector2Int TargetCoordinate;
        }
        private bool DoesShiftCauseDisconnect(in DIRECTION direction, in IBlockData blockData)
        {
            var startBlock = blockData;
            var blocks = AttachedBlocks.OfType<ISaveable>().Select(x => x.ToBlockData()).ToList();
            
            List<IBlockData> inLine;
            switch (direction)
            {
                case DIRECTION.LEFT:
                case DIRECTION.RIGHT:
                    inLine = blocks.Where(ab => ab.Coordinate.y == startBlock.Coordinate.y).ToList();
                    break;
                case DIRECTION.UP:
                case DIRECTION.DOWN:
                    inLine = blocks.Where(ab => ab.Coordinate.x == startBlock.Coordinate.x).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            var toShift = new List<SimpleShiftData>();
            var dir = direction.ToVector2Int();
            var currentCoordinate = blockData.Coordinate;

            for (var i = 0; i < inLine.Count; i++)
            {
                var targetBlockData = inLine.FirstOrDefault(x => x.Coordinate == currentCoordinate);

                if (targetBlockData == null)
                    break;

                switch (targetBlockData)
                {
                    case PartData _:
                        currentCoordinate += dir;
                        continue;
                    case JunkBitData _:
                    case BitData _:
                    {
                        IBlockData nextCheck;

                        var noShiftOffset = 1;
                        
                        do
                        {
                            var coordinate = currentCoordinate + (dir * noShiftOffset);
                            //TODO I think that I can combine both the While Loop and the Linq expression
                            nextCheck = inLine.FirstOrDefault(x => x.Coordinate == coordinate);

                            if (nextCheck is null || nextCheck is BitData || nextCheck is JunkBitData) 
                                break;


                            noShiftOffset++;

                        } while (nextCheck is PartData);

                        currentCoordinate += dir * noShiftOffset;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(targetBlockData), targetBlockData, null);
                }

                toShift.Add(new SimpleShiftData
                {
                    StartCoordinate = targetBlockData.Coordinate, 
                    TargetCoordinate = currentCoordinate
                });
            }

            if (toShift.Count == 0)
                return false;
            
            var blocksCopy = new List<IBlockData>(blocks);
            var indexesToCheck = new List<int>();
            for (int i = toShift.Count - 1; i >= 0; i--)
            {
                var shiftData = toShift[i];
                var index = blocksCopy.FindIndex(x => x.Coordinate == shiftData.StartCoordinate);

                blocks[index].Coordinate = shiftData.TargetCoordinate;
                indexesToCheck.Add(index);
            }
            
            //Look at all the blocks that can disconnect
            foreach (var index in indexesToCheck)
            {
                var hasPathToCore = blocks.HasPathToCore(blocks[index]);

                if(hasPathToCore)
                    continue;

                return true;
            }

            return false;
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

            var botBits = AttachedBlocks.OfType<Bit>().GetBlockDatas<BitData>();
            

            foreach (var shape in shapesToCheck)
            {
                var shapeBits = shape.AttachedBits.GetBlockDatas<BitData>();
                
                if (!botBits.Contains(shapeBits, out var upgrading))
                    continue;

                //Bonus Shape Effects
                //----------------------------------------------------------------------------------------------------//

                foreach (var attachable in shape.AttachedBits)
                {
                    CreateBonusShapeEffect(attachable.transform.position);
                }

                var blocks = AttachedBlocks.Where(x => upgrading.Contains(x.Coordinate));
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
                    var toUpgrade = AttachedBlocks.OfType<Bit>().FirstOrDefault(x => x.Coordinate == coordinate);

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
                PlayerDataManager.ChangeXP(gears);
                obstacleManager.MatchBonusShape(shape);



                //FIXME We'll need to double check the position here
                FloatingText.Create($"+{gears}",
                    AttachedBlocks.Find(upgrading).GetCollectionCenterCoordinateWorldPosition(),
                    Color.white);


                //Check for Combos
                CheckForCombosAround(AttachedBlocks.OfType<Bit>());
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
            if (explosion is null) return;
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
            bool bitCombos = CheckForCombosAround(AttachedBlocks.OfType<Bit>());

            return bitCombos;//|| crateCombos;
        }

        private bool CheckForCombosAround(IEnumerable<Bit> bits)
        {
            List<PendingCombov2> pendingCombos = null;
            bool hasCombos;

            foreach (var bit in bits)
            {
                if (bit == null)
                    continue;

                if (bit.level >= 4)
                    continue;

                //Get all basic info about bits available to combo
                //--------------------------------------------------------------------------------------------------------//
                var bitType = bit.Type;
                var bitsToCheck = AttachedBlocks.OfType<Bit>().Where(x => x.Type == bitType).ToArray();

                var checkData = new List<DataTest>();
                foreach (var attached in bitsToCheck)
                {
                    checkData.Add(new DataTest
                    {
                        Attachable = attached,
                        Type = attached.Type,
                        Level = attached.level,
                        Coordinate = attached.Coordinate,
                    });
                }

                //Gather any available Wildcards
                //--------------------------------------------------------------------------------------------------------//

                var wildCards = BotPartsLogic.GetWildcardParts();
                if (!wildCards.IsNullOrEmpty())
                {
                    foreach (var t in wildCards)
                    {
                        var wildCardData = t;
                        wildCardData.Level = bit.level;
                        wildCardData.Type = bit.Type;

                        checkData.Add(wildCardData);
                    }
                }

                //--------------------------------------------------------------------------------------------------------//

                if (!PuzzleChecker.TryGetComboData(bit, checkData, out var moveData))
                    continue;

                if (pendingCombos == null)
                    pendingCombos = new List<PendingCombov2>();

                if (pendingCombos.Contains(bit, out var index))
                {
                    if (pendingCombos[index].ComboData.points <= moveData.ComboData.points)
                        continue;

                    pendingCombos.RemoveAt(index);

                    pendingCombos.Add(new PendingCombov2(moveData));

                }
                else
                {
                    pendingCombos.Add(new PendingCombov2(moveData));
                }
            }

            if (pendingCombos.IsNullOrEmpty())
                return false;

            var comboFactory = FactoryManager.Instance.GetFactory<ComboFactory>();

            hasCombos = true;

            //TODO Need to figure out the multi-combo scores
            foreach (var pendingCombo in pendingCombos)
            {
                var multiplier = comboFactory.GetXPMultiplier(pendingCombos.Count, pendingCombo.ToMove.Count);
                SimpleComboSolver(pendingCombo, multiplier);
            }

            return hasCombos;
        }

        #endregion //Check for Combos from List

        //====================================================================================================================//

        #region Check for Combos Around Single

        [SerializeField]
        public struct DataTest
        {
            public IAttachable Attachable;

            public BIT_TYPE Type;
            public int Level;
            public Vector2Int Coordinate;

            public bool IsEndPiece;
        }



        private void CheckForCombosAround(Vector2Int coordinate)
        {
            CheckForCombosAround(
                AttachedBlocks
                .OfType<Bit>()
                .FirstOrDefault(a => a.Coordinate == coordinate));
        }

        private void CheckForCombosAround(in Bit bit)
        {
            if (bit == null)
                return;

            if (bit.level >= 4)
                return;

            //Get all basic info about bits available to combo
            //--------------------------------------------------------------------------------------------------------//
            var bitType = bit.Type;
            var bitsToCheck = AttachedBlocks.OfType<Bit>().Where(x => x.Type == bitType).ToArray();

            var checkData = new List<DataTest>();
            foreach (var attached in bitsToCheck)
            {
                checkData.Add(new DataTest
                {
                    Attachable = attached,
                    Type = attached.Type,
                    Level = attached.level,
                    Coordinate = attached.Coordinate,
                });
            }

            //Gather any available Wildcards
            //--------------------------------------------------------------------------------------------------------//

            var wildCards = BotPartsLogic.GetWildcardParts();
            if (!wildCards.IsNullOrEmpty())
            {
                foreach (var t in wildCards)
                {
                    var wildCardData = t;
                    wildCardData.Level = bit.level;
                    wildCardData.Type = bit.Type;

                    checkData.Add(wildCardData);
                }
            }

            //--------------------------------------------------------------------------------------------------------//


            if (!PuzzleChecker.TryGetComboData(bit, checkData, out var moveData))
                return;

            var multiplier = FactoryManager.Instance.GetFactory<ComboFactory>().GetXPMultiplier(1, moveData.ToMove.Count);
            SimpleComboSolver(moveData.ComboData, moveData.ToMove, multiplier);
        }

        #endregion //Check for Combos Around Single

        //============================================================================================================//

        #region Combo Solvers



        private void SimpleComboSolver(PendingCombov2 pendingCombo, float xpMultiplier)
        {
            SimpleComboSolver(pendingCombo.ComboData, pendingCombo.ToMove, xpMultiplier);
        }

        /// <summary>
        /// Solves movement and upgrade logic to do with simple combos of blocks.
        /// </summary>
        /// <param name="comboData"></param>
        /// <param name="canCombos"></param>
        /// <param name="xpMultiplier"></param>
        /// <exception cref="Exception"></exception>
        private void SimpleComboSolver(ComboRemoteData comboData, IReadOnlyCollection<ICanCombo> canCombos, float xpMultiplier)
        {
            ICanCombo closestToCore = null;
            var shortest = 999f;

            //Decide who gets to upgrade
            //--------------------------------------------------------------------------------------------------------//

            foreach (ICanCombo canCombo in canCombos)
            {
                var attachable = canCombo.iAttachable;
                //Need to make sure that if we choose this block, that it is connected to the core one way or another
                var hasPath = AttachedBlocks.HasPathToCore(attachable,
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

            AttachedBlocks.CheckForOrphansFromCombo(
                movingBits.OfType<IAttachable>(),
                closestToCore.iAttachable,
                ref orphans);

            //Move everyone who we've determined need to move
            //--------------------------------------------------------------------------------------------------------//

            var closestToCoreBit = (Bit)closestToCore;
            PlayerDataManager.RecordCombo(new ComboRecordData
            {
                BitType = closestToCoreBit.Type,
                ComboType = comboData.type,
                FromLevel = closestToCoreBit.level
            });
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
                enumType =>
                {
                    var position = closestToCore.transform.position;
                    var xpToAdd = Mathf.RoundToInt(comboData.points * xpMultiplier);
                    //Waits till after combo finishes combining to add the points
                    PlayerDataManager.ChangeXP(xpToAdd);

                    _lastGearText = FloatingText.Create($"+{xpToAdd}", position, Color.white);
                    
                    CreateBonusShapeParticleEffect(position);


                    var bit = closestToCore as Bit;

                    if (bit != null && bit.Type == BIT_TYPE.WHITE && bit.level == 1)
                    {
                        DestroyAttachable(bit);
                        
                        PlayerDataManager.AddSilver(1, false);
                        
                        var ammoEarned = FactoryManager.Instance
                            .ComboRemoteData
                            .ComboAmmos
                            .FirstOrDefault(x => x.level == bit.level)
                            .ammoEarned;
                        foreach (var bitType in Constants.BIT_ORDER)
                        {
                            GameUi.CreateAmmoEffect(bitType, ammoEarned, position);
                        }

                        HintManager.TryShowHint(HINT.SILVER, 0.25f, position);
                    }
                    else if (bit != null)
                    {
                        var bitType = (BIT_TYPE)enumType;
                        var bitLevel = bit.level;
                        switch (bit.level)
                        {
                            case 1:
                                CheckForCombosAround(AttachedBlocks.OfType<Bit>());
                                break;
                            case 2:
                                //This change must occur before checking for combos
                                bit.UpdateBitData(BIT_TYPE.WHITE, 0);

                                CheckForCombosAround(AttachedBlocks.OfType<Bit>());
                                //We have to override the level value here to ensure that the ammo given is
                                // reflective of the upgrade level 0 -> 1 -> white
                                bitLevel = 2;
                                
                                HintManager.TryShowHint(HINT.WHITE, 0.5f, bit);
                                break;
                        }

                        var ammoEarned = FactoryManager.Instance.ComboRemoteData.ComboAmmos
                            .FirstOrDefault(x => x.level == bitLevel)
                            .ammoEarned;
                        
                        GameUi.CreateAmmoEffect(bitType, ammoEarned, position);

                    }


                    //CheckForBonusShapeMatches();

                    OnCombo?.Invoke();
                    AttachedChanged();
                }));


            CheckForDisconnects();
            //--------------------------------------------------------------------------------------------------------//
        }

        #endregion //Combo Solvers

        //============================================================================================================//

        #endregion //Puzzle Checks

        //============================================================================================================//

        public void ForceUpdateColliderGeometry()
        {
            CompositeCollider2D.GenerateGeometry();
        }

        #region Magnet Checks

        public void ForceCheckMagnets()
        {
            _needToCheckMagnet = true;
        }

        public void ForceDisconnectAllDetachables()
        {
            DetachBlocks(AttachedBlocks.OfType<ICanDetach>(), true, true);

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
            var magnetDetachables = AttachedBlocks.Where(x => x.CountTowardsMagnetism).OfType<ICanDetach>().ToList();

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

                var core = AttachedBlocks[0] as Part;

                float resourceCapacityLiquid = PlayerDataManager.GetResource(bit.Type).AmmoCapacity;

                /*if (_botPartsLogic.ProcessBit(core, bit, resourceCapacityLiquid * Globals.GameUIResourceThreshold) > 0)
                {
                    toDetach.RemoveAt(i);
                }*/
            }
        }

        private bool IsMagnetFull()
        {
            var magnetCount = BotPartsLogic.MagnetCount;
            var magnetAttachables = AttachedBlocks.Where(x => x.CountTowardsMagnetism).ToList();

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
            for (var i = AttachedBlocks.Count - 1; i >= 0; i--)
            {
                if (toDetach.Any(x => x.iAttachable == AttachedBlocks[i]))
                    continue;

                if (AttachedBlocks.HasPathToCore(AttachedBlocks[i], leavingCoordinates))
                    continue;

                Debug.Log(
                    $"Found a potential floater {AttachedBlocks[i].gameObject.name} at {AttachedBlocks[i].Coordinate}",
                    AttachedBlocks[i].gameObject);
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
            for (var i = AttachedBlocks.Count - 1; i >= 0; i--)
            {
                if (toDetach.Any(x => x.iAttachable == AttachedBlocks[i]))
                    continue;

                if (AttachedBlocks.HasPathToCore(AttachedBlocks[i], leavingCoordinates))
                    continue;

                Debug.Log(
                    $"Found a potential floater {AttachedBlocks[i].gameObject.name} at {AttachedBlocks[i].Coordinate}",
                    AttachedBlocks[i].gameObject);
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
            Action<int> onFinishedCallback)
        {
            target.IsBusy = true;

            //Prepare Bits to be moved
            //--------------------------------------------------------------------------------------------------------//

            var mergeColor = Color.white;

            if (target is Bit targetBit)
            {
                mergeColor = targetBit.Type.GetColor();
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
                AttachedBlocks.Remove(canCombo as IAttachable);
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

            //Determine what type should be handled in the callback
            int typeValue;
            if (target is ICanCombo<Enum> iCanComboEnum)
                typeValue = (int) (object) iCanComboEnum.Type;
            else if (movingComboBlocks.FirstOrDefault() is Bit bitEnum)
                typeValue = (int) bitEnum.Type;
            else
                throw new NotImplementedException();

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
                   default:
                       throw new ArgumentOutOfRangeException(nameof(canCombo), canCombo, null);
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

            onFinishedCallback?.Invoke(typeValue);

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

            //FIXME Need to determine what the new death animations will be
            /*//TODO I think I can utilize this function in the extensions, just need to offset for coordinate location
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
                        //case Component _:
                            attachable.gameObject.SetActive(false);

                            break;
                        /*case Part part:
                            part.ChangeHealth(-10000);
                            break;#1#
                    }
                }

                yield return new WaitForSeconds(0.35f);

                index++;
            }*/

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
            foreach (var setSpriteLayer in AttachedBlocks.OfType<ISetSpriteLayer>())
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

            foreach (var attachable in AttachedBlocks)
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
                    case JunkBit _:
                        Recycler.Recycle<JunkBit>(attachable.gameObject);
                        break;
                    case Crate _:
                        Recycler.Recycle<Crate>(attachable.gameObject);
                        break;
                    default:
                        //throw new Exception($"No solver to recycle object {attachable.gameObject.name}");
                        throw new ArgumentOutOfRangeException(nameof(attachable), attachable.gameObject.name, null);
                }
            }

            AttachedBlocks.Clear();
            BotPartsLogic.ClearList();
            
            if(DecoyDrone) Destroy(DecoyDrone.gameObject);

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
            var blocks = new List<IBlockData>
            {
                new BitData
                {
                    Coordinate = new Vector2Int(-1, -1),
                    Level = 1,
                    Type = (int) BIT_TYPE.YELLOW,
                    Health = 50
                },
                new BitData
                {
                    Coordinate = new Vector2Int(-2, 0),
                    Level = 0,
                    Type = (int) BIT_TYPE.GREEN,
                    Health = 50
                },
                new BitData
                {
                    Coordinate = new Vector2Int(-2, 1),
                    Level = 0,
                    Type = (int) BIT_TYPE.RED,
                    Health = 50
                },
            };

            AddMorePieces(blocks, false);
            CheckForCombosAround(AttachedBlocks.OfType<Bit>());
        }

        private void AddMorePieces(IEnumerable<IBlockData> blocks, bool checkForCombos)
        {
            var toAdd = new List<IAttachable>();
            foreach (var blockData in blocks)
            {
                IAttachable attachable;

                switch (blockData)
                {
                    case BitData bitData:
                        attachable = FactoryManager.Instance.GetFactory<BitAttachableFactory>()
                            .CreateObject<Bit>(bitData);
                        break;
                    case PartData partData:
                        attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                            .CreateObject<Part>(partData);
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
                AttachNewBlock(attachable.Coordinate, attachable, checkForCombo: false, checkMagnet:false);
            }

            if(checkForCombos)
                CheckForCombosAround(attachables.OfType<Bit>());
        }

        [Button]
        private void TestContains()
        {
            var testBlockData = new List<BitData>
            {
                /*new BlockData
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
                },*/
            };

            var result = AttachedBlocks.OfType<Bit>().GetBlockDatas<BitData>().Contains(testBlockData, out _);

            Debug.LogError($"{nameof(AttachedBlocks)} contains match: {result}");


        }
#endif

        #endregion //UNITY EDITOR

        //====================================================================================================================//

    }
    public struct PendingCombov2
    {
        public readonly ComboRemoteData ComboData;
        public readonly List<Bit> ToMove;

        public PendingCombov2(ComboRemoteData comboData, List<Bit> toMove)
        {
            ComboData = comboData;
            ToMove = toMove;
        }
        public PendingCombov2(PuzzleChecker.MoveData moveData)
        {
            //var (comboData, toMove) = data;
            ComboData = moveData.ComboData;
            ToMove = moveData.ToMove;
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
        public static bool Contains(this IEnumerable<PendingCombov2> list, ICanCombo canCombo)
        {
            return list.Any(pendingCombo => pendingCombo.Contains(canCombo));
        }

        public static bool Contains(this List<PendingCombov2> list, ICanCombo canCombo, out int index)
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

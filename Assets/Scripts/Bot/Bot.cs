using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Audio;
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
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Animations;
using AudioController = StarSalvager.Audio.AudioController;
using FloatingText = StarSalvager.Utilities.FloatingText;
using Math = StarSalvager.Utilities.Math;

namespace StarSalvager
{
    [RequireComponent(typeof(BotPartsLogic))]
    public class Bot : MonoBehaviour, ICustomRecycle, IRecycled, ICanBeHit, IPausable
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
        
        public static Action<Bot, string> OnBotDied;

        [BoxGroup("Smoke Particles")]
        public ParticleSystem TEST_ParticleSystem;
        [BoxGroup("Smoke Particles")]
        public ParticleSystemForceField TEST_ParticleSystemForceField;

        //============================================================================================================//
        
        public bool IsRecycled { get; set; }
        
        public bool isPaused => GameTimer.IsPaused;

        //====================================================================================================================//
        
        [SerializeField, BoxGroup("PROTOTYPE")]
        public float TEST_RotSpeed;
        
        [SerializeField, Range(0.5f, 10f), BoxGroup("PROTOTYPE")]
        public float TEST_MergeSpeed = 2f;
        
        [SerializeField, BoxGroup("PROTOTYPE/Magnet")]
        public float TEST_DetachTime = 1f;
        [SerializeField, BoxGroup("PROTOTYPE/Magnet")]
        public bool TEST_SetDetachColor = true;

        //============================================================================================================//

        public List<IAttachable> attachedBlocks => _attachedBlocks ?? (_attachedBlocks = new List<IAttachable>());

        [SerializeField, ReadOnly, Space(10f), ShowInInspector] 
        private List<IAttachable> _attachedBlocks;
        
        /*private List<Part> _parts;*/

        public List<IAttachable> PendingDetach { get; private set; }


        //============================================================================================================//

        public bool Destroyed => _isDestroyed;
        private bool _isDestroyed;
        
        private Vector2 targetPosition;
        private float _currentInput;

        public bool Rotating => _rotating;
        public ROTATION MostRecentRotate;

        private bool _rotating;
        private float targetRotation;

        

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
        
        private GameUI GameUi
        {
            get
            {
                if (!_gameUi)
                    _gameUi = FindObjectOfType<GameUI>();
                
                return _gameUi;
            }
        }
        private GameUI _gameUi;

        //============================================================================================================//

        #region Unity Functions

        private void Start()
        {
            RegisterPausable();
        }

        private void Update()
        {
            if (isPaused)
                return;
            
            SetParticles();
            
            //See if the bot has completed the current wave
            //FIXME I Don't like accessing the external value here. I should consider other ways of checking this value
            if (LevelManager.Instance.EndWaveState)
                return;
            
            if (Destroyed)
                return;
            
            //TODO Once all done testing, remove this
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Time.timeScale = Time.timeScale == 0.1f ? 1f : 0.1f;
            }
            
            BotPartsLogic.PartsUpdateLoop();

            /*if (PlayerPersistentData.PlayerData.liquidResource[BIT_TYPE.YELLOW] <= 0)
            {
                Destroy("Ran out of power");
            }
            if (PlayerPersistentData.PlayerData.liquidResource[BIT_TYPE.BLUE] <= 0)
            {
                Destroy("Ran out of water");
            }*/
        }

        private void FixedUpdate()
        {
            if(Destroyed)
                return;
            
            /*if (Moving)
                MoveBot();*/

            if (Rotating)
                RotateBot();
        }

        private void OnEnable()
        {
            CompositeCollider2D.GenerateGeometry();
        }

        #endregion //Unity Functions

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

        #region Init Bot 

        public void InitBot()
        {
            _isDestroyed = false;
            CompositeCollider2D.enabled = true;

            BotPartsLogic.coreHeat = 0f;

            var startingHealth = FactoryManager.Instance.PartsRemoteData.GetRemoteData(PART_TYPE.CORE).levels[0].health;
            //Add core component
            var core = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateObject<IAttachable>(
                new BlockData
                {
                    Type = (int)PART_TYPE.CORE,
                    Coordinate = Vector2Int.zero,
                    Level = 0,
                    Health = startingHealth
                });

            AttachNewBit(Vector2Int.zero, core, updateMissions: false);

            ObstacleManager.NewShapeOnScreen += CheckForBonusShapeMatches;
        }
        
        public void InitBot(IEnumerable<IAttachable> botAttachables)
        {
            _isDestroyed = false;
            CompositeCollider2D.enabled = true;
            
            BotPartsLogic.coreHeat = 0f;
            
            foreach (var attachable in botAttachables)
            {
                AttachNewBit(attachable.Coordinate, attachable, updateMissions: false);
            }
        }


        
        #endregion // Init Bot 
        
        //============================================================================================================//

        #region Input Solver

        public void Rotate(float direction)
        {
            if (Input.GetKey(KeyCode.LeftAlt))
                return;
            
            if (direction < 0)
                Rotate(ROTATION.CCW);
            else if (direction > 0)
                Rotate(ROTATION.CW);
        }
        
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

            targetRotation = Math.ClampAngle(targetRotation);

            foreach (var attachedBlock in attachedBlocks)
            {
                attachedBlock.RotateCoordinate(rotation);
            }

            _rotating = true;

            

        }

        #endregion //Input Solver

        //============================================================================================================//

        #region Rotation

        private bool rotate;

        private void RotateBot()
        {
            var rotation = rigidbody.rotation;

            //Rotates towards the target rotation.
            rotation = Mathf.MoveTowardsAngle(rotation, targetRotation, TEST_RotSpeed * Time.fixedDeltaTime);
            rigidbody.rotation = rotation;
            
            //FIXME Remove this when ready
            TEST_ParticleSystem.transform.rotation = Quaternion.identity;
            
            //Here we check how close to the final rotation we are.
            var remainingDegrees = Mathf.Abs(Mathf.DeltaAngle(rotation, targetRotation));
            
            //TODO Here we'll need to rotate the sprites & Coordinates after a certain threshold is met for that rotation

            if (remainingDegrees > 10f)
                return;

            TryRotateBits();
            
            
            //If we're within 1deg we will count it as complete, otherwise continue to rotate.
            if (remainingDegrees > 1f)
                return;
            
            rotate = false;

            _rotating = false;

            //Force set the rotation to the target, in case the bot is not exactly on target
            rigidbody.rotation = targetRotation;
            targetRotation = 0f;
            
            //Should only be called after the rotation finishes
            CheckForBonusShapeMatches();
        }

        private void TryRotateBits()
        {
            if (rotate) 
                return;
            
            var check = (int) targetRotation;
            float deg;
            if (check == 180)
            {
                deg = 180;
            }
            else if (check == 0f || check == 360)
            {
                deg = 0;
            }
            else
            {
                deg = targetRotation + 180;
            }

            var rot = Quaternion.Euler(0, 0, deg);
                
            foreach (var attachedBlock in attachedBlocks)
            {
                if (attachedBlock is ICustomRotate rotate)
                {
                    rotate.CustomRotate(rot);
                    continue;
                }
                
                //attachedBlock.RotateSprite(MostRecentRotate);
                attachedBlock.transform.localRotation = rot;
            }

            rotate = true;
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

            switch (attachable)
            {
                case Bit bit:
                {
                    bool legalDirection;

                    //Get the coordinate of the collision
                    var bitCoordinate = GetRelativeCoordinate(bit.transform.position);

                    //------------------------------------------------------------------------------------------------//

                    var closestAttachable = attachedBlocks.GetClosestAttachable(collisionPoint);

                    legalDirection = CheckLegalCollision(bitCoordinate, closestAttachable.Coordinate, out _);

                    //------------------------------------------------------------------------------------------------//

                    if (!legalDirection)
                    {
                        //Make sure that the attachable isn't overlapping the bot before we say its impossible to 
                        if (!CompositeCollider2D.OverlapPoint(attachable.transform.position))
                            return false;
                    }
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
                                    obstacle.Bounce(collisionPoint);

                                return false;
                            }

                            //Add these to the block depending on its relative position
                            AttachAttachableToExisting(bit, closestAttachable, connectionDirection);

                            CheckForBonusShapeMatches();

                            AudioController.PlaySound(SOUND.BIT_SNAP);
                            SessionDataProcessor.Instance.BitCollected(bit.Type);
                            break;
                        case BIT_TYPE.WHITE:
                            //bounce white bit off of bot
                            var bounce = true;
                            if (bounce)
                            {
                                bit.Bounce(collisionPoint);
                            }

                            ////We don't want to move a row if it hit an enemy instead of a bit
                            //if (closestAttachable is EnemyAttachable)
                            //    break;
                            
                            //Try and shift collided row (Depending on direction)
                            var shift = TryShift(connectionDirection.Reflected(), closestAttachable);
                            AudioController.PlaySound(shift ? SOUND.BUMPER_BONK_SHIFT : SOUND.BUMPER_BONK_NOSHIFT);
                            SessionDataProcessor.Instance.HitBumper();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(bit.Type), bit.Type, null);
                    }

                    break;
                }
                case Component component:
                {
                    bool legalDirection;


                    //Get the coordinate of the collision
                    var bitCoordinate = GetRelativeCoordinate(component.transform.position);

                    //----------------------------------------------------------------------------------------------------//

                    var closestAttachable = attachedBlocks.GetClosestAttachable(collisionPoint);

                    legalDirection = CheckLegalCollision(bitCoordinate, closestAttachable.Coordinate, out _);

                    //----------------------------------------------------------------------------------------------------//

                    if (!legalDirection)
                    {
                        //Make sure that the attachable isn't overlapping the bot before we say its impossible to 
                        if (!CompositeCollider2D.OverlapPoint(attachable.transform.position))
                            return false;
                    }

                    //Check if its legal to attach (Within threshold of connection)
                    //TODO This needs to bounce off instead of being destroyed
                    if (closestAttachable is EnemyAttachable ||
                        closestAttachable is Part part && part.Destroyed)
                    {
                        if (attachable is IObstacle obstacle)
                            obstacle.Bounce(collisionPoint);

                        return false;
                    }

                    //Add these to the block depending on its relative position
                    AttachAttachableToExisting(component, closestAttachable, connectionDirection);
                    SessionDataProcessor.Instance.ComponentCollected(component.Type);

                    CheckForBonusShapeMatches();
                    
                    break;
                }
                //FIXME This seems to be wanting to attach to the wrong direction
                case EnemyAttachable enemyAttachable:
                {
                    bool legalDirection;


                    //Get the coordinate of the collision
                    var bitCoordinate = GetRelativeCoordinate(enemyAttachable.transform.position);

                    //----------------------------------------------------------------------------------------------------//

                    var closestAttachable = attachedBlocks.GetClosestAttachable(collisionPoint, true);
                    
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
                            if (closestOnBot is IObstacle obstacle)
                                obstacle.Bounce(collisionPoint);

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
                            
                            AttachNewBit(newBotCoordinate + differences[i], bitsToAdd[i], false, false);
                            SessionDataProcessor.Instance.BitCollected(bitsToAdd[i].Type);
                        }
                        
                        //Recycle the Shape, without also recycling the Bits since they were just attached to the bot
                        Recycler.Recycle<Shape>(shape, new
                        {
                            recycleBits = false
                        });

                        CheckForBonusShapeMatches();
                        
                        CheckForCombosAround(bitsToAdd);

                        AudioController.PlaySound(CheckHasMagnetOverage() ? SOUND.BIT_RELEASE : SOUND.BIT_SNAP);
                        
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

        [SerializeField, BoxGroup("PROTOTYPE")]
        public bool PROTO_GodMode;

        /// <summary>
        /// Decides if the Attachable closest to the hit position should be destroyed or damaged on the bounce
        /// </summary>
        /// <param name="hitPosition"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool TryBounceAt(Vector2 hitPosition)
        {
            if(LevelManager.Instance.EndWaveState)
                return false;
            
            var closestAttachable = attachedBlocks.GetClosestAttachable(hitPosition);

            switch (closestAttachable)
            {
                case EnemyAttachable _:
                    AsteroidDamageAt(closestAttachable);
                    return false;
                case Bit _:
                    break;
                case Component _:
                    break;
                case Part part:
                    if (part.Destroyed) 
                        return false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(closestAttachable), closestAttachable, null);
            }
            
            TryHitAt(closestAttachable, 10f);
            return true;
        }

        public bool TryHitAt(Vector2 hitPosition, float damage)
        {
            SessionDataProcessor.Instance.ReceivedDamage(damage);
            
            if(LevelManager.Instance.EndWaveState)
                return false;
            
            var closestAttachable = attachedBlocks.GetClosestAttachable(hitPosition);

            switch (closestAttachable)
            {
                // Enemies attached should not be hit by other enemy projectiles
                case EnemyAttachable _:
                case Part part when part.Destroyed:
                    return false;
            }

            var explosion = FactoryManager.Instance.GetFactory<ParticleFactory>().CreateObject<Explosion>();
            explosion.transform.position = hitPosition;
            
            TryHitAt(closestAttachable, damage);

            return true;
        }

        public void TryHitAt(IAttachable closestAttachable, float damage, bool withSound = true)
        {
            if (PROTO_GodMode && closestAttachable.Coordinate == Vector2Int.zero)
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

            var attachableDestroyed = closestHealth.CurrentHealth <= 0f;

            switch (withSound)
            {
                case true when closestAttachable is Bit:
                    AudioController.PlaySound(attachableDestroyed ? SOUND.BIT_EXPLODE : SOUND.BIT_DAMAGE);
                    break;
                case true when closestAttachable is Part:
                    AudioController.PlaySound(attachableDestroyed ? SOUND.PART_EXPLODE : SOUND.PART_DAMAGE);
                    break;
            }

            if (!attachableDestroyed)
                return;

            //Things to do if the attachable is destroyed
            //--------------------------------------------------------------------------------------------------------//

            switch (closestAttachable)
            {
                case Part part:
                    if (part.Type == PART_TYPE.CORE)
                        Destroy("Core Destroyed");
                    else
                        BotPartsLogic.UpdatePartsList();
                    break;
                default:
                    RemoveAttachable(closestAttachable);
                    break;
            }



            if (closestAttachable.Coordinate != Vector2Int.zero)
                CheckForDisconnects();

            //------------------------------------------------------------------------------------------------//
        }



        #endregion //TryHitAt
        
        #region Asteroid Collision
        
        public bool TryAsteroidDamageAt(Vector2 collisionPoint)
        {
            var closestAttachable = attachedBlocks.GetClosestAttachable(collisionPoint);

            //------------------------------------------------------------------------------------------------//

            //TODO Need to add animation/effects here 
            //Destroy both this and collided Bit
            //Recycler.Recycle<Bit>(attachable.gameObject);

            AsteroidDamageAt(closestAttachable);
            return true;
        }

        /// <summary>
        /// Applies pre-determine asteroid damage to the specified IAttachable
        /// </summary>
        /// <param name="attachable"></param>
        private void AsteroidDamageAt(IAttachable attachable)
        {
            FrameStop.Instance.Milliseconds(75);

            TryHitAt(attachable, 10000);
            AudioController.PlaySound(SOUND.ASTEROID_CRUSH);
            
            var explosion = FactoryManager.Instance.GetFactory<ParticleFactory>().CreateObject<Explosion>();
            explosion.transform.position = attachable.transform.position;

            switch (attachable)
            {
                case Bit bit:
                    MissionManager.ProcessAsteroidCollisionMissionData(bit.Type, 1);
                    break;
                case Component _:
                    MissionManager.ProcessAsteroidCollisionMissionData(null, 1);
                    break;
                case Part _:
                    MissionManager.ProcessAsteroidCollisionMissionData(null, 1);
                    break;
                case EnemyAttachable enemyAttachable:
                    MissionManager.ProcessAsteroidCollisionMissionData(null, 1);
                    enemyAttachable.SetAttached(false);
                    return;
            }

            //FIXME This value should not be hardcoded
            BotPartsLogic.AddCoreHeat(20f);
            //coolTimer = coolDelay;


            if ((attachedBlocks.Count == 0 || ((IHealth) attachedBlocks[0])?.CurrentHealth <= 0) && !PROTO_GodMode)
            {
                Destroy("Core Destroyed by Asteroid");
            }
            else if (BotPartsLogic.coreHeat >= 100 && !PROTO_GodMode)
            {
                Destroy("Core Overheated");
            }
        }

        #endregion //Asteroid Collision

        //============================================================================================================//

        #region Attach Bits
        
        public bool TryAttachNewBit(Vector2Int coordinate, IAttachable newAttachable, bool checkForCombo = true, 
            bool updateColliderGeometry = true, bool updateMissions = true)
        {
            if (Destroyed) 
                return false;
            
            if (attachedBlocks.Any(x => x.Coordinate == coordinate))
                return false;
            
            newAttachable.Coordinate = coordinate;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * coordinate * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            newAttachable.gameObject.name = $"Block {attachedBlocks.Count}";
            
            //We want to avoid having the same element multiple times in the list
            if(!attachedBlocks.Contains(newAttachable)) 
                attachedBlocks.Add(newAttachable);
            
            switch (newAttachable)
            {
                    case Bit bit:
                        if(updateMissions) MissionManager.ProcessResourceCollectedMissionData(bit.Type, 1, bit.IsFromEnemyLoot);
                        
                        if(checkForCombo) CheckForCombosAround<BIT_TYPE>(coordinate);
                        
                        break;
                    case Component _ when checkForCombo:
                        CheckForCombosAround<COMPONENT_TYPE>(coordinate);
                        break;
                    case Part _:
                        BotPartsLogic.UpdatePartsList();
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
            
            if(newAttachable.CountTowardsMagnetism)
                AudioController.PlaySound(CheckHasMagnetOverage() ? SOUND.BIT_RELEASE : SOUND.BIT_SNAP);

            if(updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();

            return true;
        }

        public void AttachNewBit(Vector2Int coordinate, IAttachable newAttachable, bool checkForCombo = true, 
            bool updateColliderGeometry = true, bool updateMissions = true, bool checkMagnet = true, bool playSound = true)
        {
            if (Destroyed) 
                return;
            
            newAttachable.Coordinate = coordinate;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * coordinate * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            newAttachable.gameObject.name = $"Block {attachedBlocks.Count}";
            
            //We want to avoid having the same element multiple times in the list
            if(!attachedBlocks.Contains(newAttachable)) 
                attachedBlocks.Add(newAttachable);

            switch (newAttachable)
            {
                case Bit bit:
                    if(checkForCombo) CheckForCombosAround<BIT_TYPE>(coordinate);
                    
                    if(updateMissions) MissionManager.ProcessResourceCollectedMissionData(bit.Type, 1, bit.IsFromEnemyLoot);
                    break;
                case Component _ when checkForCombo:
                    CheckForCombosAround<COMPONENT_TYPE>(coordinate);
                    break;
                case Part _:
                    BotPartsLogic.UpdatePartsList();
                    break;
            }

            if (newAttachable.CountTowardsMagnetism && checkMagnet)
            {
                var check = CheckHasMagnetOverage();
                if(playSound)
                    AudioController.PlaySound(check ? SOUND.BIT_RELEASE : SOUND.BIT_SNAP);
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
            DIRECTION direction, bool checkForCombo = true, bool updateColliderGeometry = true,
            bool updateMissions = true, bool checkMagnet = true, bool playSound = true)
        {
            if (Destroyed) 
                return;
            
            var coordinate = existingAttachable.Coordinate + direction.ToVector2Int();

            //Checks for attempts to add attachable to occupied location
            if (attachedBlocks.Any(a => a.Coordinate == coordinate && !(a is Part part && part.Destroyed)))
            {
                var on = attachedBlocks.FirstOrDefault(a => a.Coordinate == coordinate);
                Debug.LogError(
                    $"Prevented attaching {newAttachable.gameObject.name} to occupied location {coordinate}\n Occupied by {on.gameObject.name}",
                    newAttachable.gameObject);

                //I don't want the enemies to push to the end of the arm, I want it just attach to the closest available space
                if (newAttachable is EnemyAttachable)
                    AttachToClosestAvailableCoordinate(coordinate, newAttachable, direction,
                        checkForCombo, updateColliderGeometry, updateMissions);
                else
                    PushNewAttachable(newAttachable, direction, existingAttachable.Coordinate);

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
                    if (checkForCombo)
                        CheckForCombosAround<BIT_TYPE>(coordinate);
                    
                    if(updateMissions) MissionManager.ProcessResourceCollectedMissionData(bit.Type, 1, bit.IsFromEnemyLoot);
                    
                    break;
                case Component _ when checkForCombo:
                    CheckForCombosAround<COMPONENT_TYPE>(coordinate);
                    break;
                case Part _:
                    BotPartsLogic.UpdatePartsList();
                    break;
            }
            
            if (newAttachable.CountTowardsMagnetism && checkMagnet)
            {
                var check = CheckHasMagnetOverage();
                if(playSound)
                    AudioController.PlaySound(check ? SOUND.BIT_RELEASE : SOUND.BIT_SNAP);
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
                    Debug.Log($"Found available location for {newAttachable.gameObject.name}\n{coordinate} + ({directions[i]} * {dist}) = {check}");
                    AttachNewBit(check, newAttachable, checkForCombo, updateColliderGeometry, updateMissions);
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
                var check = CheckHasMagnetOverage();
                if(playSound)
                    AudioController.PlaySound(check ? SOUND.BIT_RELEASE : SOUND.BIT_SNAP);
            }

            /*if (checkForCombo)
            {
                CheckForCombosAround(newCoord);
                CheckHasMagnetOverage();
            }*/

            if(updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();
        }

        public void PushNewAttachable(IAttachable newAttachable, DIRECTION direction, Vector2Int startCoord, bool checkForCombo = true, bool updateColliderGeometry = true, bool checkMagnet = true, bool playSound = true)
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
                var check = CheckHasMagnetOverage();
                if(playSound)
                    AudioController.PlaySound(check ? SOUND.BIT_RELEASE : SOUND.BIT_SNAP);
            }
            
            if(updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();
        }

        #endregion //Attach Bits

        #region Detach Bits

        public void ForceDetach(IAttachable attachable)
        {
            DetachBit(attachable);
        }
        
        private void DetachBits(IReadOnlyCollection<IAttachable> detachingBits, bool delayedCollider = false)
        {
            //if (attachables.Count == 1)
            //{
            //    DetachBit(attachables.FirstOrDefault(), delayedCollider);
            //    return;
            //}
            
            foreach (var attachable in detachingBits)
            {
                PendingDetach?.Remove(attachable);
                attachedBlocks.Remove(attachable);
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
                        LevelManager.Instance.ObstacleManager.AddOrphanToObstacles(shape);
                    
                    if (delayedCollider)
                    {
                        shape.SetColliderActive(false);
                        this.DelayedCall(1f, () =>
                        {
                            shape.SetColor(Color.white);
                            shape.SetColliderActive(true);
                        });
                    }
                }
                else
                {
                    var bit = bits[0];
                    
                    bit.SetAttached(false);
                    bit.SetColor(Color.white);
                    bit.SetColliderActive(false);
                    bit.transform.parent = null;
                    bit.transform.rotation = Quaternion.identity;

                    if (LevelManager.Instance != null)
                        LevelManager.Instance.ObstacleManager.AddOrphanToObstacles(bit);

                    bits.RemoveAt(0);
                }
            }
            

            //FIXME THis seems to be troublesome. Bits that are not attached, still are part of the same shape. 
            //var shape = FactoryManager.Instance.GetFactory<ShapeFactory>().CreateObject<Shape>(bits);
            //foreach (var bit in bits)
            //{
            //    bit.SetAttached(false);
            //    bit.SetColor(Color.white);
            //    bit.SetColliderActive(false);
            //    bit.transform.parent = null;
            //    bit.transform.rotation = Quaternion.identity;
            //}

            
            foreach (var iAttachable in others)
            {
                iAttachable.SetAttached(false);
            }

            //FIXME THis seems to be troublesome. Bits that are not attached, still are part of the same shape. 
            //if (delayedCollider)
            //{
            //    shape.SetColliderActive(false);
//
            //    this.DelayedCall(1f, () =>
            //    {
            //        shape.SetColor(Color.white);
            //        shape.SetColliderActive(true);
            //    });
            //}

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

            bits.GetAllAttachedDetachables(originBit, null, ref shapeBits);

            if (shapeBits.Count > 1)
            {
                return true;
            }

            shapeBits = null;
            return false;
        }
        
        private void DetachBit(IAttachable attachable)
        {
            attachable.transform.parent = null;

            if (LevelManager.Instance != null && attachable is IObstacle obstacle)
                LevelManager.Instance.ObstacleManager.AddOrphanToObstacles(obstacle);

            RemoveAttachable(attachable);
        }
        
        private void RemoveAttachable(IAttachable attachable)
        {
            attachedBlocks.Remove(attachable);
            attachable.SetAttached(false);
            
            CompositeCollider2D.GenerateGeometry();
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
                    BotPartsLogic.UpdatePartsList();
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

        //============================================================================================================//
        
        #region Check for New Disconnects
        
        /// <summary>
        /// Function will review and detach any blocks that no longer have a connection to the core.
        /// </summary>
        private bool CheckForDisconnects()
        {
            var toSolve = new List<IAttachable>(attachedBlocks);
            bool hasDetached = false;
            
            foreach (var attachable in toSolve)
            {
                if (!attachedBlocks.Contains(attachable))
                    continue;

                var hasPathToCore = attachedBlocks.HasPathToCore(attachable);
                
                if(hasPathToCore)
                    continue;

                hasDetached = true;

                var attachables = new List<IAttachable>();
                attachedBlocks.GetAllAttachedDetachables(attachable, null, ref attachables);

                foreach (var attachedBit in attachables.OfType<Bit>())
                {
                    SessionDataProcessor.Instance.BitDetached(attachedBit.Type);
                }

                if (attachables.Count == 1)
                {
                    DetachBit(attachables[0]);
                    continue;
                }

                DetachBits(attachables);
            }

            return hasDetached;
        }

        /// <summary>
        /// Checks to see if removing the list wantToRemove causes disconnects on the bot. Returns true on any disconnect.
        /// Returns false if all is okay.
        /// </summary>
        /// <param name="wantToRemove"></param>
        /// <param name="toIgnore"></param>
        /// <returns></returns>
        private bool RemovalCausesDisconnects(ICollection<IAttachable> wantToRemove, out string disconnectList)
        {
            disconnectList = string.Empty;
            var toSolve = new List<IAttachable>(attachedBlocks);
            var ignoreCoordinates = wantToRemove?.Select(x => x.Coordinate).ToList();
            
            foreach (var attachable in toSolve)
            {
                if (!attachable.CountAsConnectedToCore)
                    continue;
                
                //if (!attachedBlocks.Contains(attachable))
                //    continue;

                if (wantToRemove != null && wantToRemove.Contains(attachable))
                    continue;
                
                var hasPathToCore = attachedBlocks.HasPathToCore(attachable, ignoreCoordinates);
                
                if(hasPathToCore)
                    continue;

                disconnectList += $"{attachable.gameObject.name} will disconnect\n";

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
                        //TODO I think that I can combine both the While Loop and the Linq expression
                        nextCheck = inLine.FirstOrDefault(x => x.Coordinate == currentCoordinate + (dir * noShiftOffset));


                        
                        if (nextCheck is null || nextCheck.CanShift) break;

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
                TEST_MergeSpeed,
                () =>
            {
                //TODO May want to consider that Enemies may still attack while being shifted
                //This needs to happen before checking for disconnects because otherwise attached will be set to false
                foreach (var wasBumped in toShift.Select(x => x.Target).OfType<IWasBumped>())
                {
                    wasBumped.OnBumped();
                }
                
                //Checks for floaters
                hasDetached = CheckForDisconnects();

                var comboCheckGroup = toShift.Select(x => x.Target).Where(x => attachedBlocks.Contains(x) && x is ICanCombo)
                    .OfType<ICanCombo>();
                
                switch (attachable)
                {
                    case Bit _:
                        hasCombos = CheckForCombosAround<BIT_TYPE>(comboCheckGroup);
                        break;
                    case Component _:
                        hasCombos = CheckForCombosAround<COMPONENT_TYPE>(comboCheckGroup);

                        break;
                }

                MissionManager.ProcessWhiteBumperMissionData(toShift.Count, passedCore, hasDetached, hasCombos);

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
                PlayerPersistentData.PlayerData.ChangeGears(gears);
                obstacleManager.MatchBonusShape(shape);
                
                //FIXME We'll need to double check the position here
                FloatingText.Create($"+{gears}",
                    attachedBlocks.Find(upgrading).GetCollectionCenterPosition(),
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
        
        //============================================================================================================//

        #region Puzzle Checks

        private void CheckForCombosAround<T>(Vector2Int coordinate) where T: Enum
        {
            CheckForCombosAround(attachedBlocks.FirstOrDefault(a => a.Coordinate == coordinate && a is ICanCombo) as ICanCombo<T>);
        }

        private bool CheckForCombosAround<T>(IEnumerable<IAttachable> iAttachables) where T : Enum
        {
            return CheckForCombosAround(iAttachables.OfType <ICanCombo<T>>());
        }
        private bool CheckForCombosAround<T>(IEnumerable<ICanCombo> iCanCombos) where T : Enum
        {
            return CheckForCombosAround(iCanCombos.OfType <ICanCombo<T>>());
        }
        

        
        private bool CheckForCombosAround<T>(IEnumerable<ICanCombo<T>> iCanCombos) where T: Enum
        {
            List<PendingCombo> pendingCombos = null;
            bool hasCombos = false;
            
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

                if (pendingCombos.Contains(iCanCombo as IAttachable, out var index))
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

            if (pendingCombos == null || pendingCombos.Count == 0)
                return false;

            hasCombos = true;

            //TODO Need to figure out the multi-combo scores
            foreach (var pendingCombo in pendingCombos)
            {
                if (pendingCombo.ToMove[0] is Bit bit)
                {
                    bool isAdvancedCombo = pendingCombo.ComboData.type == Utilities.Puzzle.Data.COMBO.TEE || pendingCombo.ComboData.type == Utilities.Puzzle.Data.COMBO.ANGLE;
                    MissionManager.ProcessComboBlocksMissionData(bit.Type, bit.level + 1, 1, isAdvancedCombo);
                }
                
                SimpleComboSolver(pendingCombo);
            }

            return hasCombos;
        }
        private void CheckForCombosAround<T>(ICanCombo<T> iCanCombo) where T: Enum
        {
            if (iCanCombo == null)
                return;
            
            if (iCanCombo.level >= 4)
                return;

            if (!PuzzleChecker.TryGetComboData(this, iCanCombo, out var data))
                return;

            //if (data.comboData.addLevels == 2)
            //{
            //    AdvancedComboSolver(data.comboData, data.toMove);
            //}
            //else
            if (iCanCombo is Bit bit)
            {
                bool isAdvancedCombo = data.comboData.type == Utilities.Puzzle.Data.COMBO.TEE || data.comboData.type == Utilities.Puzzle.Data.COMBO.ANGLE;
                MissionManager.ProcessComboBlocksMissionData(bit.Type, iCanCombo.level + 1, 1, isAdvancedCombo);
            }
            
            SimpleComboSolver(data.comboData, data.toMove);
        }

        //============================================================================================================//
        
        #region Combo Solvers

        private void SimpleComboSolver(PendingCombo pendingCombo)
        {
            SimpleComboSolver(pendingCombo.ComboData, pendingCombo.ToMove);
        }

        /// <summary>
        /// Solves movement and upgrade logic to do with simple combos of blocks.
        /// </summary>
        /// <param name="comboAttachables"></param>
        /// <exception cref="Exception"></exception>
        private void SimpleComboSolver(ComboRemoteData comboData, IReadOnlyCollection<IAttachable> comboAttachables)
        {
            IAttachable closestToCore = null;
            var shortest = 999f;

            //Decide who gets to upgrade
            //--------------------------------------------------------------------------------------------------------//

            foreach (IAttachable iAttachable in comboAttachables)
            {
                //Need to make sure that if we choose this block, that it is connected to the core one way or another
                var hasPath = attachedBlocks.HasPathToCore(iAttachable,
                    comboAttachables.Where(ab => ab != iAttachable)
                        .Select(b => b.Coordinate)
                        .ToList());

                //If there's no path, we cannot use this bit
                if (!hasPath)
                    continue;


                var dist = Vector2Int.Distance(iAttachable.Coordinate, Vector2Int.zero);
                if (!(dist < shortest))
                    continue;

                shortest = dist;
                closestToCore = iAttachable;
            }

            //Make sure that things are working
            //--------------------------------------------------------------------------------------------------------//

            //If no block was selected, then we've had a problem
            if (closestToCore == null)
                throw new Exception("No Closest Core Found");

            //See if anyone else needs to move
            //--------------------------------------------------------------------------------------------------------//

            //Get a list of Bits that will be moving (Blocks that are not the chosen closest to core)
            var movingBits = comboAttachables
                .Where(ab => ab != closestToCore).ToArray();

            //Get a list of orphans that may need move when we are moving our bits
            var orphans = new List<OrphanMoveData>();
            CheckForOrphans(movingBits, closestToCore, ref orphans);

            //Move everyone who we've determined need to move
            //--------------------------------------------------------------------------------------------------------//
            
            //if(orphans.Count > 0)
            //    Debug.Break();

            var iCanCombo = closestToCore as ICanCombo;
            iCanCombo.IncreaseLevel(comboData.addLevels);


            //TODO May want to place this in the coroutine
            //Plays the sound for the new level achieved by the bit
            switch (iCanCombo.level)
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
                TEST_MergeSpeed,
                () =>
                {
                    //Waits till after combo finishes combining to add the points 
                    PlayerPersistentData.PlayerData.ChangeGears(comboData.points);
                    
                    FloatingText.Create($"+{comboData.points}", closestToCore.transform.position, Color.white);

                    //We need to update the positions and level before we move them in case we interact with bits while they're moving
                    switch (iCanCombo)
                    {
                        case Bit _:
                            CheckForCombosAround<BIT_TYPE>(attachedBlocks);
                            break;
                        case Component _:
                            CheckForCombosAround<COMPONENT_TYPE>(attachedBlocks);
                            break;
                    }

                    
                }));
                
            
            CheckForDisconnects();
            //--------------------------------------------------------------------------------------------------------//
        }

        private void AdvancedComboSolver(ComboRemoteData comboData, IReadOnlyList<IAttachable> comboBits)
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
        }
        
        #endregion //Combo Solvers
        
        //============================================================================================================//

        /// <summary>
        /// Get any Bit/Bits that will be orphaned by the bits which will be moving
        /// </summary>
        /// <param name="movingBits"></param>
        /// <param name="bitToUpgrade"></param>
        /// <param name="orphanMoveData"></param>
        /// <returns></returns>
        private void CheckForOrphans(IAttachable[] movingBits,
            IAttachable bitToUpgrade, ref List<OrphanMoveData> orphanMoveData)
        {
            //List<OrphanMoveData> orphanMoveData = null;

            //Check against all the bits that will be moving
            //--------------------------------------------------------------------------------------------------------//

            foreach (var movingBit in movingBits)
            {
                //Get the basic data about the current movingBit
                //----------------------------------------------------------------------------------------------------//

                var dif = bitToUpgrade.Coordinate - movingBit.Coordinate;
                var travelDirection = dif.ToDirection();
                var travelDistance = dif.magnitude;

                //Debug.Log($"Travel Direction: {travelDirection} distance {travelDistance}");
                
                if(travelDirection == DIRECTION.NULL)
                    continue;
                


                //Check around moving bits (Making sure to exclude the one that doesn't move)
                //----------------------------------------------------------------------------------------------------//

                //Get all the attachableBases around the specified attachable
                var bitsAround = attachedBlocks.GetAttachablesAround(movingBit);

                //Don't want to bother checking the block that we know will not move
                if (bitsAround.Contains(bitToUpgrade))
                    bitsAround.Remove(bitToUpgrade);

                //Double check that the neighbors are connected to the core
                //----------------------------------------------------------------------------------------------------//

                foreach (var bit in bitsAround)
                {
                    //Ignore the ones that we know are good
                    //------------------------------------------------------------------------------------------------//
                    if (bit == null)
                        continue;

                    if (bit == bitToUpgrade)
                        continue;

                    if (movingBits.Contains(bit))
                        continue;

                    //Make sure that we haven't already determined this element to be moved
                    if (orphanMoveData != null && orphanMoveData.Any(omd => omd.attachableBase == bit))
                        continue;

                    //Check that we're connected to the core
                    //------------------------------------------------------------------------------------------------//

                    var hasPathToCore = attachedBlocks.HasPathToCore(bit,
                        movingBits
                            .Select(b => b.Coordinate)
                            .ToList());

                    if (hasPathToCore)
                        continue;

                    //We've got an orphan, record all of the necessary data
                    //------------------------------------------------------------------------------------------------//

                    var newOrphanCoordinate =
                        bit.Coordinate + travelDirection.ToVector2Int() * (int) travelDistance;

                    var attachedToOrphan = new List<IAttachable>();
                    attachedBlocks.GetAllAttachedDetachables(bit, movingBits, ref attachedToOrphan);

                    //Debug.LogError($"Orphan Attached Count: {attachedToOrphan.Count}");
                    //Debug.Break();

                    //Debug.Log($"{newOrphanCoordinate} = {bit.Coordinate} + {travelDirection.ToVector2Int()} * {(int) travelDistance}");

                    if (orphanMoveData == null)
                        orphanMoveData = new List<OrphanMoveData>();

                    //------------------------------------------------------------------------------------------------//

                    SolveOrphanGroupPositionChange(bit, attachedToOrphan, newOrphanCoordinate, travelDirection,
                        (int) travelDistance, movingBits, ref orphanMoveData);
                }

            }
        }

        /// <summary>
        /// Solve the position change required for a single orphan. If moving a group ensure you use SolveOrphanGroupPositionChange
        /// </summary>
        /// <param name="orphanedBit"></param>
        /// <param name="targetCoordinate"></param>
        /// <param name="travelDirection"></param>
        /// <param name="travelDistance"></param>
        /// <param name="movingBits"></param>
        /// <param name="orphanMoveData"></param>
        /// <param name="lastLocation"></param>
        private void SolveOrphanPositionChange(IAttachable orphanedBit, Vector2Int targetCoordinate, DIRECTION travelDirection,
            int travelDistance, IReadOnlyCollection<IAttachable> movingBits, ref List<OrphanMoveData> orphanMoveData)
        {
            //Loop ensures that the orphaned blocks which intend on moving, are able to reach their destination without any issues.

            //Check only the Bits on the Bot that wont be moving
            var stayingBlocks = new List<IAttachable>(attachedBlocks);
            foreach (var attachableBase in movingBits)
            {
                stayingBlocks.Remove(attachableBase);
            }

            //Checks to see if this orphan can travel unimpeded to the destination
            //If it cannot, set the destination to the block beside that which is blocking it.
            var hasClearPath = IsPathClear(stayingBlocks, movingBits, travelDistance, orphanedBit.Coordinate,
                travelDirection, targetCoordinate, out var clearCoordinate);

            //If there's no clear solution, then we will try and solve the overlap here
            if (!hasClearPath && clearCoordinate == Vector2Int.zero)
            {
                //Debug.LogError("Orphan has no clear path to intended Position");
                throw new Exception("NEED TO LOOK AT WHAT IS HAPPENING HERE");

                //Make sure that there's no overlap between orphans new potential positions & existing staying Bits
                //stayingBlocks.SolveCoordinateOverlap(travelDirection, ref desiredLocation);
            }
            else if (!hasClearPath)
            {
                //Debug.LogError($"Path wasn't clear. Setting designed location to {clearCoordinate} instead of {desiredLocation}");
                targetCoordinate = clearCoordinate;
            }
            
            //lastPosition = targetCoordinate;

            orphanMoveData.Add(new OrphanMoveData
            {
                attachableBase = orphanedBit,
                moveDirection = travelDirection,
                distance = travelDistance,
                intendedCoordinates = targetCoordinate
            });
        }


        private void SolveOrphanGroupPositionChange(IAttachable mainOrphan,
            IReadOnlyList<IAttachable> orphanGroup, Vector2Int targetCoordinate,
            DIRECTION travelDirection, int travelDistance, IReadOnlyCollection<IAttachable> movingBits,
            ref List<OrphanMoveData> orphanMoveData)
        {

            if (orphanGroup.Count == 1)
            {
                SolveOrphanPositionChange(mainOrphan, targetCoordinate, travelDirection, travelDistance, movingBits,
                    ref orphanMoveData);
                return;
            }
            
            
            //Debug.LogError($"Moving Orphan group, Count: {orphanGroup.Count}");

            //var lastLocation = Vector2Int.zero;

            var distances = new float[orphanGroup.Count];

            var index = -1;
            var shortestDistance = 999f;
            
            
            for (var i = 0; i < orphanGroup.Count; i++)
            {
                var orphan = orphanGroup[i];
                var relative = orphan.Coordinate - mainOrphan.Coordinate;
                var desiredLocation = targetCoordinate + relative;

                //Check only the Bits on the Bot that wont be moving
                var stayingBlocks = new List<IAttachable>(attachedBlocks);
                foreach (var attachableBase in movingBits)
                {
                    stayingBlocks.Remove(attachableBase);
                }

                //Checks to see if this orphan can travel unimpeded to the destination
                //If it cannot, set the destination to the block beside that which is blocking it.
                var hasClearPath = IsPathClear(stayingBlocks, movingBits, travelDistance, orphan.Coordinate,
                    travelDirection, desiredLocation, out var clearCoordinate);

                if (!hasClearPath && clearCoordinate == Vector2Int.zero)
                    distances[i] = 999f;
                else if (!hasClearPath)
                    distances[i] = Vector2Int.Distance(orphan.Coordinate, clearCoordinate);
                else
                    distances[i] = Vector2Int.Distance(orphan.Coordinate, desiredLocation);

                if (distances[i] > shortestDistance)
                    continue;

                //index = i;
                shortestDistance = distances[i];
            }
            
            //Debug.LogError($"Shortest to move {orphanGroup[index].gameObject.name}, Distance: {shortestDistance}");
            //Debug.Break();

            foreach (var orphan in orphanGroup)
            {
                //var relative = orphan.Coordinate - mainOrphan.Coordinate;
                //var desiredLocation = targetCoordinate + relative;

                var newCoordinate = orphan.Coordinate + travelDirection.ToVector2Int() * (int) shortestDistance;
                
                orphanMoveData.Add(new OrphanMoveData
                {
                    attachableBase = orphan,
                    moveDirection = travelDirection,
                    distance = shortestDistance,
                    intendedCoordinates = newCoordinate
                });
            }
        }
        
        private bool IsPathClear(List<IAttachable> stayingBlocks, IEnumerable<IAttachable> toIgnore, int distance, Vector2Int currentCoordinate, DIRECTION moveDirection, Vector2Int targetCoordinate, out Vector2Int clearCoordinate)
        {
            //var distance = (int) orphanMoveData.distance;
            var coordinate = currentCoordinate;
            
            clearCoordinate = Vector2Int.zero;
            
            while (distance > 0)
            {
                coordinate += moveDirection.ToVector2Int();
                var occupied = stayingBlocks.Where(x => !toIgnore.Contains(x)).FirstOrDefault(x => x.Coordinate == coordinate);

                //Debug.LogError($"Occupied: {occupied == null} at {coordinate} distance {distance}");
                
                if (occupied == null)
                    clearCoordinate = coordinate;
                
                //if(occupied != null)
                //    Debug.LogError($"{occupied.gameObject.name} is at {coordinate}", occupied);
                

                distance--;
            }

            return targetCoordinate == clearCoordinate;
        }

        #endregion //Puzzle Checks

        //============================================================================================================//

        #region Magnet Checks

        /// <summary>
        /// Determines based on the total of magnet slots which pieces must be removed to fit within the expected capacity
        /// </summary>
        public bool CheckHasMagnetOverage()
        {
            if (!BotPartsLogic.useMagnet)
                return false;


            var magnetCount = BotPartsLogic.MagnetCount;
            var magnetAttachables = attachedBlocks.Where(x => x.CountTowardsMagnetism).ToList();
            
            if(GameUi) 
                GameUi.SetCarryCapacity(magnetAttachables.Count / (float)magnetCount);
            
            //Checks here if the total of attached blocks (Minus the Core) change
            if (magnetAttachables.Count <= magnetCount)
                return false;
            
            //--------------------------------------------------------------------------------------------------------//

            var toRemoveCount = magnetAttachables.Count - magnetCount;
            var attachablesToDetach = new List<IAttachable>();

            //--------------------------------------------------------------------------------------------------------//

            //float time;
            Action onDetach;
            
            switch (BotPartsLogic.currentMagnet)
            {
                //----------------------------------------------------------------------------------------------------//
                case MAGNET.DEFAULT:
                    DefaultMagnetCheck(magnetAttachables, out attachablesToDetach, in toRemoveCount);
                    //time = 1f;
                    onDetach = () =>
                    {
                        DetachBits(attachablesToDetach, true);
                    };
                    break;
                //----------------------------------------------------------------------------------------------------//
                case MAGNET.BUMP:
                    BumpMagnetCheck(magnetAttachables, out attachablesToDetach, in toRemoveCount);
                    //time = 0f;
                    onDetach = () =>
                    {
                        DetachBits(attachablesToDetach, true);
                    };
                    break;
                //----------------------------------------------------------------------------------------------------//
                case MAGNET.LOWEST:
                    LowestMagnetCheckSimple(magnetAttachables, ref attachablesToDetach, ref toRemoveCount);
                    //time = 1f;
                    onDetach = () =>
                    {
                        DetachBits(attachablesToDetach, true);
                    };
                    break;
                //----------------------------------------------------------------------------------------------------//
                default:
                    throw new ArgumentOutOfRangeException();
                //----------------------------------------------------------------------------------------------------//
            }

            if (PendingDetach == null)
                PendingDetach = new List<IAttachable>();
            
            PendingDetach.AddRange(attachablesToDetach);
            
            onDetach.Invoke();
            
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

            return true;
        }

        private void DefaultMagnetCheck(List<IAttachable> attachables, out List<IAttachable> toDetach, in int toRemoveCount)
        {
            var magnetCount = BotPartsLogic.MagnetCount;
            
            //Gets the last added overage to remove
            toDetach = attachables.GetRange(magnetCount, toRemoveCount);
            
            //Get the coordinates of the blocks leaving. This is used to determine if anyone will be left floating
            var leavingCoordinates = toDetach.Select(a => a.Coordinate).ToList();

            //Go through the bots Blocks to make sure no one will be floating when we detach the parts.
            for (var i = attachedBlocks.Count - 1; i >= 0; i--)
            {
                if (toDetach.Contains(attachedBlocks[i]))
                    continue;

                if (attachedBlocks.HasPathToCore(attachedBlocks[i], leavingCoordinates))
                    continue;

                Debug.LogError(
                    $"Found a potential floater {attachedBlocks[i].gameObject.name} at {attachedBlocks[i].Coordinate}",
                    attachedBlocks[i].gameObject);
            }
        }

        private void BumpMagnetCheck(List<IAttachable> attachables, out List<IAttachable> toDetach, in int toRemoveCount)
        {
            var magnetCount = BotPartsLogic.MagnetCount;
            
            //Gets the last added overage to remove
            toDetach = attachables.GetRange(magnetCount, toRemoveCount);
            
            //Get the coordinates of the blocks leaving. This is used to determine if anyone will be left floating
            var leavingCoordinates = toDetach.Select(a => a.Coordinate).ToList();

            //Go through the bots Blocks to make sure no one will be floating when we detach the parts.
            for (var i = attachedBlocks.Count - 1; i >= 0; i--)
            {
                if (toDetach.Contains(attachedBlocks[i]))
                    continue;

                if (attachedBlocks.HasPathToCore(attachedBlocks[i], leavingCoordinates))
                    continue;

                Debug.LogError(
                    $"Found a potential floater {attachedBlocks[i].gameObject.name} at {attachedBlocks[i].Coordinate}",
                    attachedBlocks[i].gameObject);
            }

            
        }

        private void LowestMagnetCheckSimple(List<IAttachable> attachables, ref List<IAttachable> toDetach, ref int toRemoveCount)
        {
            var checkedBits = new List<IAttachable>();
            var debug = string.Empty;
            while (toRemoveCount > 0)
            {
                var toRemove = FindLowestAttachable(attachables, checkedBits);

                if (toRemove == null)
                {
                    //Debug.LogError($"toRemove is NULL, {toRemoveCount} remaining bits unsolved");
                    break;
                }

                checkedBits.Add(toRemove);

                if (attachables.Count == checkedBits.Count)
                {
                    //Debug.LogError($"Left with {toRemoveCount} bits unsolved");
                    break;
                }
                
                if (RemovalCausesDisconnects(new List<IAttachable>(toDetach){toRemove}, out debug))
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
                attachables.Remove(bit);
            }
                
            while (toRemoveCount > 0)
            {
                var toRemove = FindFurthestRemovableBit(attachables, toDetach, ref debug);
                    
                if(toRemove == null)
                    throw new Exception($"Unable to find alternative pieces\n{debug}");
                    
                toDetach.Add(toRemove);
                attachables.Remove(toRemove);
                toRemoveCount--;
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
        private IAttachable FindLowestAttachable(List<IAttachable> attachables, ICollection<IAttachable> toIgnore)
        {
            //I Want the last Bit to be the fallback/default, if I can't find anything
            IAttachable selectedAttachable = null;
            var lowestLevel = 999;
            //The lowest Y coordinate
            var lowestCoordinate = 999;

            foreach (var attachable in attachables)
            {
                if (toIgnore.Contains(attachable))
                    continue;

                if (!(attachable is ILevel HasLevel))
                {
                    continue;
                }
                
                if(HasLevel.level > lowestLevel)
                    continue;

                //Checks if the piece is higher, and if it is, that the level is not higher than the currently selected Bit
                //This ensures that even if the lowest Bit is of high level, the lowest will always be selected
                if (attachable.Coordinate.y > lowestCoordinate && !(HasLevel.level < lowestLevel))
                        continue;

                if (RemovalCausesDisconnects(new List<IAttachable>(/*toIgnore*/) {attachable}, out _))
                    continue;

                selectedAttachable = attachable;
                lowestLevel = HasLevel.level;
                lowestCoordinate = attachable.Coordinate.y;

            }

            if (selectedAttachable != null) 
                return selectedAttachable;
            
            
            foreach (var attachable in attachables)
            {
                if (toIgnore.Contains(attachable))
                    continue;
                
                if (!(attachable is ILevel HasLevel))
                {
                    continue;
                }
            
                if(HasLevel.level > lowestLevel)
                    continue;

                //Checks if the piece is higher, and if it is, that the level is not higher than the currently selected Bit
                //This ensures that even if the lowest Bit is of high level, the lowest will always be selected
                if (attachable.Coordinate.y > lowestCoordinate)
                    continue;

                if (RemovalCausesDisconnects(new List<IAttachable>(/*toIgnore*/) {attachable}, out _))
                    continue;

                selectedAttachable = attachable;
                lowestLevel = HasLevel.level;
                lowestCoordinate = attachable.Coordinate.y;

            }

            return selectedAttachable;
        }

        private IAttachable FindFurthestRemovableBit(List<IAttachable> attachables, ICollection<IAttachable> toIgnore, ref string debug)
        {
            //I Want the last Bit to be the fallback/default, if I can't find anything
            IAttachable selectedAttachable = null;
            var furthestDistance = -999f;
            var lowestLevel = 999f;

            foreach (var attachable in attachables)
            {
                if (toIgnore.Contains(attachable))
                    continue;
                
                if (!(attachable is ILevel HasLevel))
                {
                    continue;
                }

                var _dist = Vector2Int.Distance(attachable.Coordinate, Vector2Int.zero);
                
                if(_dist < furthestDistance)
                    continue;

                if (lowestLevel < HasLevel.level)
                    continue;

                if (RemovalCausesDisconnects(new List<IAttachable>(toIgnore) { attachable }, out debug))
                    continue;

                selectedAttachable = attachable;
                furthestDistance = _dist;
                lowestLevel = HasLevel.level;

            }

            return selectedAttachable;
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
        /// <param name="movingAttachables"></param>
        /// <param name="target"></param>
        /// <param name="orphans"></param>
        /// <param name="speed"></param>
        /// <param name="OnFinishedCallback"></param>
        /// <returns></returns>
        private IEnumerator MoveComboPiecesCoroutine(IAttachable[] movingAttachables, IAttachable target,
            IReadOnlyList<OrphanMoveData> orphans, float speed, Action OnFinishedCallback)
        {
            //Prepare Bits to be moved
            //--------------------------------------------------------------------------------------------------------//
            
            
            
            foreach (var bit in movingAttachables)
            {
                //We need to disable the collider otherwise they can collide while moving
                //I'm also assuming that if we've confirmed the upgrade, and it cannot be cancelled
                attachedBlocks.Remove(bit);
                (bit as Bit)?.SetColliderActive(false);
            }

            foreach (var omd in orphans)
            {
                omd.attachableBase.Coordinate = omd.intendedCoordinates;
                (omd.attachableBase as Bit)?.SetColliderActive(false);
            }
            
            //We're going to want to regenerate the shape while things are moving
            CompositeCollider2D.GenerateGeometry();
            
            //--------------------------------------------------------------------------------------------------------//

            var t = 0f;
            var targetTransform = target.transform;

            //Obtain lists of both Transforms to manipulate & their current local positions
            //--------------------------------------------------------------------------------------------------------//

            var bitTransforms = movingAttachables.Select(ab => ab.transform).ToArray();
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
            while (t <= 1f)
            {
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
                        Vector2.Lerp(bitTransformPositions[i], targetTransform.localPosition, t);
                    
                    SSDebug.DrawArrow(bt.position,targetTransform.position, Color.green);
                }

                //Move the orphans into their new positions
                //----------------------------------------------------------------------------------------------------//
                
                for (var i = 0; i < orphans.Count; i++)
                {
                    var bitTransform = orphanTransforms[i];
                   
                    //Debug.Log($"Start {bitTransform.position} End {position}");

                    bitTransform.localPosition = Vector2.Lerp(orphanTransformPositions[i],
                        orphanTargetPositions[i], t);
                    
                    SSDebug.DrawArrow(bitTransform.position,transform.TransformPoint(orphanTargetPositions[i]), Color.red);
                }
                
                //----------------------------------------------------------------------------------------------------//

                t += Time.deltaTime * speed;

                yield return null;
            }
            
            //Wrap up things now that everyone is in place
            //--------------------------------------------------------------------------------------------------------//

            //Once all bits are moved, remove from list and dispose
            foreach (var attachable in movingAttachables)
            {
                attachable.SetAttached(false);

                switch (attachable)
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
                orphanTransforms[i].localPosition = orphanTargetPositions[i];
                (orphans[i].attachableBase as CollidableBase)?.SetColliderActive(true);
            }
            
            //Now that everyone is where they need to be, wrap things up
            //--------------------------------------------------------------------------------------------------------//

            CompositeCollider2D.GenerateGeometry();


            OnFinishedCallback?.Invoke();
            
            //--------------------------------------------------------------------------------------------------------//
        }
                
                
        /// <summary>
        /// Moves a collection of AttachableBase 1 unit in the specified direction. Callback is triggered before the update
        /// to the Composite collider
        /// </summary>
        /// <param name="toMove"></param>
        /// <param name="direction"></param>
        /// <param name="speed"></param>
        /// <param name="OnFinishedCallback"></param>
        /// <returns></returns>
        private IEnumerator ShiftInDirectionCoroutine(IReadOnlyList<ShiftData> toMove, /*DIRECTION direction, */float speed, Action OnFinishedCallback)
        {
            //var dir = direction.ToVector2Int();
            /*var transforms = toMove.Select(x => x.transform).ToArray();
            var startPositions = transforms.Select(x => x.localPosition).ToArray();
            var targetPositions = toMove.Select(o =>
                transform.InverseTransformPoint((Vector2) transform.position +
                                                ((Vector2) o.Coordinate + dir)  * Constants.gridCellSize)).ToArray();*/
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

                var distance = System.Math.Round(Vector2.Distance(startPositions[i], targetPositions[i]), 2);
                skipsCoordinate[i] = distance > System.Math.Round(Constants.gridCellSize, 2);

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

            while (t < 1f)
            {
                for (var i = 0; i < transforms.Length; i++)
                {
                    if (toMove[i].Target.Attached == false)
                        continue;
                    
                    transforms[i].localPosition = Vector2.Lerp(startPositions[i], targetPositions[i], t);
                }

                t += Time.deltaTime * speed;
                
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
                    /*switch (attachable)
                    {
                        case Bit _:
                            Recycler.Recycle<Bit>(attachable.gameObject);
                            break;
                        case Component _:
                            Recycler.Recycle<Component>(attachable.gameObject);
                            break;
                        case Part _:
                            Recycler.Recycle<Part>(attachable.gameObject);
                            break;
                        case EnemyAttachable _:
                            Recycler.Recycle<EnemyAttachable>(attachable.gameObject);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }*/
                    attachable.gameObject.SetActive(false);
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

        //============================================================================================================//

        #region Custom Recycle

        public void CustomRecycle(params object[] args)
        {
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
            PendingDetach?.Clear();
            BotPartsLogic.ClearList();
            //_parts.Clear();
            
            ObstacleManager.NewShapeOnScreen -= CheckForBonusShapeMatches;
        }
        
        #endregion //Custom Recycle
        
        //============================================================================================================//

        #region UNITY EDITOR

#if UNITY_EDITOR
        [Button]
        private void SetupTest()
        {
            var blocks = new List<BlockData>
            {
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(2,-1),
                    Level =0,
                    Type = (int)BIT_TYPE.GREY
                },
                
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(1,0),
                    Level =0,
                    Type = (int)BIT_TYPE.GREEN
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(2,0),
                    Level =0,
                    Type = (int)BIT_TYPE.GREEN
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(3,-1),
                    Level =0,
                    Type = (int)BIT_TYPE.GREEN
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(3,-2),
                    Level =0,
                    Type = (int)BIT_TYPE.GREEN
                },

            };

            AddMorePieces(blocks, false);
        }
        [Button]
        private void AddComboTestPieces()
        {
            var blocks = new List<BlockData>
            {
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(2,1),
                    Level =0,
                    Type = (int)BIT_TYPE.GREEN
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(2,2),
                    Level =0,
                    Type = (int)BIT_TYPE.GREEN
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(3,1),
                    Level =0,
                    Type = (int)BIT_TYPE.GREEN
                },
                new BlockData
                {
                    ClassType = nameof(Bit),
                    Coordinate = new Vector2Int(3,2),
                    Level =0,
                    Type = (int)BIT_TYPE.GREEN
                },
            };
            
            AddMorePieces(blocks, true);
        }
        
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
                AttachNewBit(attachable.Coordinate, attachable, updateMissions: false, checkForCombo: false, checkMagnet:false);
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
        public readonly List<IAttachable> ToMove;

        public PendingCombo(ComboRemoteData comboData, List<IAttachable> toMove)
        {
            ComboData = comboData;
            ToMove = toMove;
        }
        public PendingCombo((ComboRemoteData comboData, List<IAttachable> toMove) data)
        {
            var (comboData, toMove) = data;
            ComboData = comboData;
            ToMove = toMove;
        }

        public bool Contains(IAttachable attachable)
        {
            if (ToMove == null || ToMove.Count == 0)
                return false;
                
            return ToMove.Contains(attachable);
        }

    }

    public static class PendingComboListExtensions
    {
        public static bool Contains(this List<PendingCombo> list, IAttachable attachable)
        {
            return list.Any(pendingCombo => pendingCombo.Contains(attachable));
        }
        
        public static bool Contains(this List<PendingCombo> list, IAttachable attachable, out int index)
        {
            index = -1;
            
            var temp = list.ToArray();
            for (var i = 0; i < temp.Length; i++)
            {
                if (!temp[i].Contains(attachable))
                    continue;
                
                index = i;
                return true;
            }

            return false;
        }
    }
}


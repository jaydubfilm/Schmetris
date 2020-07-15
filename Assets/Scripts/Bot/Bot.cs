using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Values;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Prototype;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Puzzle;
using UnityEngine;
using Enemy = StarSalvager.AI.Enemy;
using GameUI = StarSalvager.UI.GameUI;

namespace StarSalvager
{
    public class Bot : MonoBehaviour, ICustomRecycle, IRecycled, ICanBeHit
    {
        public static Action<Bot, string> OnBotDied;

        //============================================================================================================//
        
        public bool IsRecycled { get; set; }

        [SerializeField, BoxGroup("PROTOTYPE")]
        public float TEST_Speed;

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
        
        private List<Part> _parts;

        public List<IAttachable> BitsPendingDetach { get; private set; }


        //============================================================================================================//

        public bool Destroyed => _isDestroyed;
        private bool _isDestroyed;
        
       //public bool Moving => _moving;
        //private bool _moving;

        //public DIRECTION MoveDirection => Globals.MovingDirection;

        [SerializeField, ReadOnly]
        private DIRECTION _moveDirection = DIRECTION.NULL;
        //public bool HasValidInput => _currentInput != 0f;

        private Vector2 targetPosition;
        private float _currentInput;

        public float DelayedAutoStartTime = 0.2f;
        private float _dasTimer;

        public bool Rotating => _rotating;

        private bool _rotating;
        private float targetRotation;


        [SerializeField, BoxGroup("Magnets")]
        private bool useMagnet = true;
        [SerializeField, BoxGroup("Magnets")]
        private MAGNET currentMagnet = MAGNET.DEFAULT;
        
        [SerializeField, BoxGroup("BurnRates")]
        private bool useBurnRate = true;
        
        //============================================================================================================//

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

        private void Update()
        {
            //TODO Once all done testing, remove this
            _moveDirection = Globals.MovingDirection;
            
            if (Destroyed)
                return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Time.timeScale = Time.timeScale == 0.01f ? 1f : 0.01f;
            }
            
            PartsUpdateLoop();
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

        //============================================================================================================//

        #region Init Bot 

        public void InitBot()
        {
            _isDestroyed = false;
            CompositeCollider2D.enabled = true;

            coreHeat = 0f;
            
            //Add core component
            var core = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateObject<IAttachable>(
                new BlockData
                {
                    Type = (int)PART_TYPE.CORE,
                    Coordinate = Vector2Int.zero,
                    Level = 0,
                });

            AttachNewBit(Vector2Int.zero, core);
        }
        
        public void InitBot(IEnumerable<IAttachable> botAttachables)
        {
            _isDestroyed = false;
            CompositeCollider2D.enabled = true;
            
            coreHeat = 0f;
            
            foreach (var attachable in botAttachables)
            {
                AttachNewBit(attachable.Coordinate, attachable);
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

            //If we're already rotating, we need to add the direction to the target
            if (Rotating)
            {
                targetRotation += toRotate;
            }
            else
            {
                targetRotation = rigidbody.rotation + toRotate;
            }

            foreach (var attachedBlock in attachedBlocks)
            {
                attachedBlock.RotateCoordinate(rotation);
            }

            _rotating = true;

        }

        /*public void Move(float direction)
        {
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                _currentInput = 0f;
                return;
            }
            
            Debug.Log($"Set my direction to {_currentInput}", this);
            
            _currentInput = direction;
            DIRECTION moveDirection;

            if (direction < 0)
                moveDirection = DIRECTION.LEFT;
            else if (direction > 0)
                moveDirection = DIRECTION.RIGHT;
            else
            {
                return;
            }

            Move(moveDirection);
        }

        private void Move(DIRECTION direction)
        {
            Vector2 toMove;
            switch (direction)
            {
                case DIRECTION.LEFT:
                case DIRECTION.RIGHT:
                    toMove = direction.ToVector2Int();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            if (Moving)
            {
                targetPosition += toMove * Constants.gridCellSize;
            }
            else
            {
                targetPosition = (Vector2) transform.position + toMove * Constants.gridCellSize;
                _dasTimer = 0f;
            }

            _moving = true;
        }*/

        #endregion //Input Solver

        //============================================================================================================//

        #region Movement

        /*private void MoveBot()
        {
            var position = rigidbody.position;

            //TODO See if this will be enough for the current setup, or if we will need something more robust.
            position = Vector2.MoveTowards(position, targetPosition, TEST_Speed * Time.fixedDeltaTime);

            _movingDirection = (targetPosition - position).normalized.ToDirection();

            //Using MovePosition() for the kinematic object since I still want it to interpolate nicely there (In the physics) 
            rigidbody.MovePosition(position);

            var remainingDistance = Vector2.Distance(position, targetPosition);
            
            if (remainingDistance > 0.2f)
                return;



            if (_currentInput != 0)
            {
                if (_dasTimer < DelayedAutoStartTime)
                {
                    _dasTimer += Time.deltaTime;
                    return;
                }

                Move(_currentInput);
                return;
            }

            _moving = false;
            rigidbody.position = targetPosition;
            targetPosition = Vector2.zero;
            _movingDirection = DIRECTION.NULL;
            _dasTimer = 0f;
        }*/

        private void RotateBot()
        {
            var rotation = rigidbody.rotation;

            //Rotates towards the target rotation.
            //rotation = Quaternion.RotateTowards(rotation, targetRotation, TEST_RotSpeed * Time.deltaTime);
            rotation = Mathf.MoveTowardsAngle(rotation, targetRotation, TEST_RotSpeed * Time.fixedDeltaTime);
            rigidbody.rotation = rotation;

            //Here we check how close to the final rotation we are.
            var remainingDegrees = Mathf.Abs(Mathf.DeltaAngle(rotation, targetRotation));

            //If we're within 1deg we will count it as complete, otherwise continue to rotate.
            if (remainingDegrees > 1f)
                return;

            _rotating = false;

            //Force set the rotation to the target, in case the bot is not exactly on target
            rigidbody.rotation = targetRotation;
            targetRotation = 0f;
        }

        #endregion //Movement

        //============================================================================================================//

        #region Check For Legal Bit Attach

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
                    var direction = DIRECTION.NULL;


                    //Get the coordinate of the collision
                    var bitCoordinate = GetRelativeCoordinate(bit.transform.position);

                    //----------------------------------------------------------------------------------------------------//

                    var closestAttachable = attachedBlocks.GetClosestAttachable(collisionPoint);
                    legalDirection = CheckLegalCollision(bitCoordinate, closestAttachable.Coordinate, out direction);

                    //----------------------------------------------------------------------------------------------------//

                    if (!legalDirection)
                    {
                        //Make sure that the attachable isn't overlapping the bot before we say its impossible to 
                        if (!CompositeCollider2D.OverlapPoint(attachable.transform.position))
                            return false;
                    }

                    //Check if its legal to attach (Within threshold of connection)
                    switch (bit.Type)
                    {
                        case BIT_TYPE.BLACK:
                            //TODO Need to add animation/effects here 
                            //Destroy both this and collided Bit
                            Recycler.Recycle<Bit>(attachable.gameObject);

                            AsteroidDamageAt(closestAttachable);
                            break;
                        case BIT_TYPE.BLUE:
                        case BIT_TYPE.GREEN:
                        case BIT_TYPE.GREY:
                        case BIT_TYPE.RED:
                        case BIT_TYPE.YELLOW:

                            //Add these to the block depending on its relative position
                            AttachNewBitToExisting(bit, closestAttachable, connectionDirection);

                            break;
                        case BIT_TYPE.WHITE:
                            //Destroy collided Bit
                            Recycler.Recycle<Bit>(attachable.gameObject);
                        
                            //Try and shift collided row (Depending on direction)
                            TryShift(connectionDirection.Reflected(), closestAttachable);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(bit.Type), bit.Type, null);
                    }

                    break;
                }
                case EnemyAttachable enemyAttachable:
                {
                    bool legalDirection;
                    var direction = DIRECTION.NULL;


                    //Get the coordinate of the collision
                    var bitCoordinate = GetRelativeCoordinate(enemyAttachable.transform.position);

                    //----------------------------------------------------------------------------------------------------//

                    var closestAttachable = attachedBlocks.GetClosestAttachable(collisionPoint);
                    legalDirection = CheckLegalCollision(bitCoordinate, closestAttachable.Coordinate, out direction);

                    //----------------------------------------------------------------------------------------------------//

                    if (!legalDirection)
                    {
                        //Make sure that the attachable isn't overlapping the bot before we say its impossible to 
                        if (!CompositeCollider2D.OverlapPoint(attachable.transform.position))
                            return false;
                    }

                    //Add these to the block depending on its relative position
                    AttachNewBitToExisting(enemyAttachable, closestAttachable, connectionDirection);
                    break;
                }
            }


            return true;
        }

        public IAttachable GetClosestAttachable(Vector2Int checkCoordinate)
        {
            IAttachable selected = null;

            var smallestDist = 999f;

            foreach (var attached in attachedBlocks)
            {
                //attached.SetColor(Color.white);

                var dist = Vector2Int.Distance(attached.Coordinate, checkCoordinate);
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

        #endregion //Check For Legal Bit Attach

        //============================================================================================================//
        
        #region Check for Legal Shape Attach

        public bool TryAddNewShape(Shape shape, IAttachable closestShapeBit, DIRECTION connectionDirection, Vector2 collisionPoint)
        {
            if (_isDestroyed)
                return false;
            
            if (Rotating)
                return false;
            
            var closestOnBot= attachedBlocks.GetClosestAttachable(collisionPoint);

            if (closestShapeBit is Bit closeBit)
            {
                switch (closeBit.Type)
                {
                    case BIT_TYPE.BLACK:
                        //TODO Damage/Destroy Bits as required
                        shape.DestroyBit(closeBit);

                        AsteroidDamageAt(closestOnBot);
                        
                        break;
                    case BIT_TYPE.BLUE:
                    case BIT_TYPE.GREEN:
                    case BIT_TYPE.GREY:
                    case BIT_TYPE.RED:
                    case BIT_TYPE.YELLOW:
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
                        }
                        
                        //Recycle the Shape, without also recycling the Bits since they were just attached to the bot
                        Recycler.Recycle<Shape>(shape, new
                        {
                            recycleBits = false
                        });
                        
                        CheckForCombosAround(bitsToAdd);

                        CheckForMagnetOverage();
                        
                        CompositeCollider2D.GenerateGeometry();

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(closeBit.Type), closeBit.Type, null);
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

        public void TryHitAt(Vector2 hitPosition, float damage)
        {
            var closestAttachable = attachedBlocks.GetClosestAttachable(hitPosition);

            print("DAMAGE");

            //FIXME Need to see how to fix this
            if (closestAttachable is IHealth closestHealth)
            {
                closestHealth.ChangeHealth(-damage);

                if (closestHealth.CurrentHealth > 0) 
                    return;
            }
            
            
            RemoveAttachable(closestAttachable);
            CheckForDisconnects();
        }

        public void TryHitAt(IAttachable closestAttachable, float damage)
        {
            //FIXME Need to see how to fix this
            if (closestAttachable is IHealth closestHealth)
            {
                closestHealth.ChangeHealth(-damage);

                if (closestHealth.CurrentHealth > 0)
                    return;

                RemoveAttachable(closestAttachable);
                CheckForDisconnects();
            }
        }
        
        #endregion //TryHitAt
        
        #region Asteroid Collision
        
        /// <summary>
        /// Applies pre-determine asteroid damage to the specified IAttachable
        /// </summary>
        /// <param name="attachable"></param>
        private void AsteroidDamageAt(IAttachable attachable)
        {
            
            switch (attachable)
            {
                case Bit bit:
                    TryHitAt(attachable, bit.CurrentHealth);
                    break;
                case Part _:
                {
                    TryHitAt(attachable, 5f);
                    break;
                }
            }

            //FIXME This value should not be hardcoded
            coreHeat += 20;
            coolTimer = coolDelay;


            if (attachedBlocks.Count == 0 || ((IHealth) attachedBlocks[0])?.CurrentHealth <= 0)
            {
                Destroy("Core Destroyed");
            }
            else if (coreHeat >= 100)
            {
                Destroy("Core Overheated");
            }
        }

        #endregion //Asteroid Collision

        //============================================================================================================//

        #region Attach Bits

        public void AttachNewBit(Vector2Int coordinate, IAttachable newAttachable, bool checkForCombo = true, bool updateColliderGeometry = true)
        {
            newAttachable.Coordinate = coordinate;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * coordinate * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            newAttachable.gameObject.name = $"Block {attachedBlocks.Count}";
            attachedBlocks.Add(newAttachable);

            if (newAttachable is Part)
                UpdatePartsList();
            
            if(checkForCombo)
                CheckForCombosAround(coordinate);

            if(updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();
        }

        public void AttachNewBitToExisting(IAttachable newAttachable, IAttachable existingAttachable,
            DIRECTION direction, bool checkForCombo = true, bool updateColliderGeometry = true)
        {
            var coordinate = existingAttachable.Coordinate + direction.ToVector2Int();

            //Checks for attempts to add attachable to occupied location
            if (attachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                Debug.LogError($"Prevented attaching {newAttachable.gameObject.name} to occupied location {coordinate}",
                    newAttachable.gameObject);
                PushNewBit(newAttachable, direction, existingAttachable.Coordinate);
                return;
            }

            newAttachable.Coordinate = coordinate;

            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * coordinate * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            attachedBlocks.Add(newAttachable);

            if (checkForCombo)
            {
                CheckForCombosAround(coordinate);
                CheckForMagnetOverage();
            }

            if(updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();
        }

        public void PushNewBit(IAttachable newAttachable, DIRECTION direction, bool checkForCombo = true, bool updateColliderGeometry = true)
        {
            var newCoord = direction.ToVector2Int();

            attachedBlocks.FindUnoccupiedCoordinate(direction, ref newCoord);

            newAttachable.Coordinate = newCoord;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * newCoord * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            attachedBlocks.Add(newAttachable);

            if (checkForCombo)
            {
                CheckForCombosAround(newCoord);
                CheckForMagnetOverage();
            }

            if(updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();
        }

        public void PushNewBit(IAttachable newAttachable, DIRECTION direction, Vector2Int startCoord, bool checkForCombo = true, bool updateColliderGeometry = true)
        {
            var newCoord = startCoord + direction.ToVector2Int();

            attachedBlocks.FindUnoccupiedCoordinate(direction, ref newCoord);

            newAttachable.Coordinate = newCoord;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * newCoord * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            attachedBlocks.Add(newAttachable);

            if (checkForCombo)
            {
                CheckForCombosAround(newCoord);
                CheckForMagnetOverage();
            }
            
            if(updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();
        }

        #endregion //Attach Bits

        #region Detach Bits
        
        private void DetachBits(IReadOnlyCollection<IAttachable> detachingBits, bool delayedCollider = false)
        {
            //if (attachables.Count == 1)
            //{
            //    DetachBit(attachables.FirstOrDefault(), delayedCollider);
            //    return;
            //}
            
            foreach (var attachable in detachingBits)
            {
                attachedBlocks.Remove(attachable);
            }

            var bits = detachingBits.OfType<Bit>().ToList();

            var shape = FactoryManager.Instance.GetFactory<ShapeFactory>().CreateObject<Shape>(bits);

            if (delayedCollider)
            {
                shape.SetColliderActive(false);

                this.DelayedCall(1f, () =>
                {
                    shape.SetColor(Color.white);
                    shape.SetColliderActive(true);
                });
            }

            CheckForDisconnects();
            
            CompositeCollider2D.GenerateGeometry();

        }
        private void DetachBitsCheck(IReadOnlyCollection<IAttachable> detachingBits, bool delayedCollider = false)
        {
            foreach (var attachable in detachingBits)
            {
                attachedBlocks.Remove(attachable);
                BitsPendingDetach.Remove(attachable);
            }

            var removes = new List<IAttachable>(detachingBits);
            

            while (removes.Count > 0)
            {
                var toShape = new List<IAttachable>();
                removes.GetAllAttachedBits(removes[0], null , ref toShape);

                foreach (var bit in toShape)
                {
                    removes.Remove(bit);
                }

                var shape = FactoryManager.Instance.GetFactory<ShapeFactory>().CreateObject<Shape>(toShape.OfType<Bit>().ToList());

                if (delayedCollider)
                {
                    shape.SetColliderActive(false);

                    this.DelayedCall(3f, () =>
                    {
                        shape.SetColor(Color.white);
                        shape.SetColliderActive(true);
                    });
                }
            }

            CheckForDisconnects();            
            
            CompositeCollider2D.GenerateGeometry();

        }
        private void DetachBit(IAttachable attachable)
        {
            attachable.transform.parent = null;

            RemoveAttachable(attachable);
            
            //Debug.Log($"Detached {bit.gameObject.name}", bit);
        }
        
        private void RemoveAttachable(IAttachable attachable)
        {
            attachedBlocks.Remove(attachable);
            attachable.SetAttached(false);
            
            CompositeCollider2D.GenerateGeometry();
        }
        
        private void DestroyAttachable(IAttachable attachable)
        {
            attachedBlocks.Remove(attachable);
            attachable.SetAttached(false);

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
            }
                        
            CheckForDisconnects();
            
            CompositeCollider2D.GenerateGeometry();
        }
        
        /// <summary>
        /// Removes the attachable, and will recycle it under the T bin.
        /// </summary>
        /// <param name="attachable"></param>
        /// <typeparam name="T"></typeparam>
        private void DestroyAttachable<T>(IAttachable attachable) where T: IAttachable
        {
            attachedBlocks.Remove(attachable);
            attachable.SetAttached(false);

            Recycler.Recycle<T>(attachable.gameObject);
            
            CheckForDisconnects();
            
            CompositeCollider2D.GenerateGeometry();
        }
        
        #endregion //Detach Bits
        
        //============================================================================================================//

        #region Parts
        
        [SerializeField, BoxGroup("Bot Part Data"), ReadOnly]
        private float coreHeat;
        [SerializeField, BoxGroup("Bot Part Data"), DisableInPlayMode,SuffixLabel("/s", Overlay = true)]
        private float coolSpeed;
        [SerializeField, BoxGroup("Bot Part Data"), DisableInPlayMode, SuffixLabel("s", Overlay = true)]
        private float coolDelay;
        [SerializeField, BoxGroup("Bot Part Data"), ReadOnly]
        private float coolTimer;
        
        [SerializeField, BoxGroup("Bot Part Data"), ReadOnly, Space(10f)]
        private int magnetCount;

        private Dictionary<Part, float> projectileTimers;
        
        
        /// <summary>
        /// Called when new Parts are added to the attachable List. Allows for a short list of parts to exist to ease call
        /// cost for updating the Part behaviour
        /// </summary>
        private void UpdatePartsList()
        {
            _parts = attachedBlocks.OfType<Part>().ToList();
            
            UpdatePartData();
        }

        /// <summary>
        /// Called to update the bot about relevant data to function.
        /// </summary>
        private void UpdatePartData()
        {
            magnetCount = 0;
            
            foreach (var part in _parts)
            {
                var partData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(PART_TYPE.MAGNET);
                
                switch (part.Type)
                {
                    case PART_TYPE.MAGNET:
                    case PART_TYPE.CORE:
                        magnetCount += partData.data[part.level];
                        break;
                }
            }
        }

        //FIXME This needs to something more manageable
        private EnemyManager _enemyManager => FindObjectOfType<EnemyManager>();

        /// <summary>
        /// Parts specific update Loop. Updates all part information based on currently attached parts.
        /// </summary>
        private void PartsUpdateLoop()
        {
            const float damageGuess = 5f;
            
            List<Enemy> enemies = null;
            //Be careful to not use return here
            foreach (var part in _parts)
            {
                PartRemoteData partRemoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(part.Type);
                Bit targetBit = null;

                if(partRemoteData.burnRates.Length > 0)
                    targetBit = attachedBlocks.OfType<Bit>()
                        .FirstOrDefault(b => b.Type == partRemoteData.burnRates[part.level].type);
                
                switch (part.Type)
                {
                    case PART_TYPE.CORE:

                        //TODO This needs to lock the core from being able to move if none is found
                        if (targetBit)
                        {
                            //Reduce the health of the bit while we're using it up for fuel
                            targetBit.ChangeHealth(-partRemoteData.burnRates[part.level].amount * Time.deltaTime);
                        }
                        
                        //TODO Need to check on Heating values for the core
                        if (coreHeat <= 0)
                        {
                            GameUi.SetHeatSliderValue(0f);
                            break;
                        }

                        GameUi.SetHeatSliderValue(coreHeat / 100f);

                        
                        part.SetColor(Color.Lerp(Color.white, Color.red, coreHeat / 100f));
                        
                        if (coolTimer > 0f)
                        {
                            coolTimer -= Time.deltaTime;
                            break;
                        }
                        else
                            coolTimer = 0;

                        coreHeat -= coolSpeed * Time.deltaTime;
                        
                        if (coreHeat < 0)
                        {
                            coreHeat = 0;
                            part.SetColor(Color.white);
                        }
                        
                        break;
                    case PART_TYPE.REPAIR:

                        if (!targetBit)
                            break;
                        //TODO Determine if this heals Bits & parts or just parts
                        //TODO This needs to fire every x Seconds
                        var toRepair = attachedBlocks.GetAttachablesAroundInRadius<Part>(part, part.level + 1)
                            .FirstOrDefault(p => p.CurrentHealth < p.StartingHealth);

                        if (toRepair is null) break;
                        
                        //Reduce the health of the bit while we're using it up for fuel
                        targetBit.ChangeHealth(-partRemoteData.burnRates[part.level].amount * Time.deltaTime);
                        
                        //Increase the health of this part depending on the current level of the repairer
                        toRepair.ChangeHealth(partRemoteData.data[part.level] * Time.deltaTime);
                        
                        break;
                    case PART_TYPE.GUN:
                        //TODO Need to determine if the shoot type is looking for enemies or not
                        //--------------------------------------------------------------------------------------------//
                        if (projectileTimers == null)
                            projectileTimers = new Dictionary<Part, float>();
                        
                        if(!projectileTimers.ContainsKey(part))
                            projectileTimers.Add(part, 0f);
                        //TODO This needs to fire every x Seconds
                        
                        //--------------------------------------------------------------------------------------------//
                        
                        if (projectileTimers[part] < partRemoteData.data[part.level] / damageGuess)
                        {
                            projectileTimers[part] += Time.deltaTime;
                            break;
                        }
                        projectileTimers[part] = 0f;

                        //If we have the resources to shoot do so, otherwise break out 
                        if (targetBit)
                        {
                            //Reduce the health of the bit while we're using it up for resources
                            targetBit.ChangeHealth(-partRemoteData.burnRates[part.level].amount);
                        }
                        else 
                            break;
                        
                        //--------------------------------------------------------------------------------------------//
                        
                        var shootDirection = Vector2.up;
                        var enemy = _enemyManager.GetClosestEnemy(transform.position, 5 * Constants.gridCellSize);
                        //TODO Determine if this fires at all times or just when there are active enemies in range
                        if (enemy != null)
                        {
                            shootDirection = enemy.transform.position - transform.position;
                        }
                        
                        //Create projectile
                        //--------------------------------------------------------------------------------------------//
                        
                        
                        var projectile = FactoryManager.Instance.GetFactory<ProjectileFactory>().CreateObject<Projectile>(
                            PROJECTILE_TYPE.Projectile1,
                            shootDirection,
                            "Enemy");
                        projectile.transform.position = part.transform.position;
                        
                        LevelManager.Instance?.ProjectileManager.AddProjectile(projectile);
                        
                        //--------------------------------------------------------------------------------------------//
                        
                        break;
                }
            }
        }
        
        #endregion //Parts
        
        //============================================================================================================//
        
        #region Check for New Disconnects
        
        /// <summary>
        /// Function will review and detach any blocks that no longer have a connection to the core.
        /// </summary>
        private void CheckForDisconnects()
        {
            var toSolve = new List<IAttachable>(attachedBlocks);
            
            foreach (var attachableBase in toSolve)
            {
                if (!attachedBlocks.Contains(attachableBase))
                    continue;
                
                var hasPathToCore = attachedBlocks.HasPathToCore(attachableBase);
                
                if(hasPathToCore)
                    continue;

                var attachedBits = new List<IAttachable>();
                attachedBlocks.GetAllAttachedBits(attachableBase, null, ref attachedBits);

                if (attachedBits.Count == 1)
                {
                    DetachBit(attachedBits[0]);
                    continue;
                }
                
                
                DetachBits(attachedBits);
            }
        }

        /// <summary>
        /// Checks to see if removing the list wantToRemove causes disconnects on the bot. Returns true on any disconnect.
        /// Returns false if all is okay.
        /// </summary>
        /// <param name="wantToRemove"></param>
        /// <param name="toIgnore"></param>
        /// <returns></returns>
        private bool RemovalCausesDisconnects(ICollection<IAttachable> wantToRemove)
        {
            var toSolve = new List<IAttachable>(attachedBlocks);
            var ignoreCoordinates = wantToRemove?.Select(x => x.Coordinate).ToList();
            
            foreach (var attachable in toSolve)
            {
                //if (!attachedBlocks.Contains(attachable))
                //    continue;

                if (wantToRemove != null && wantToRemove.Contains(attachable))
                    continue;
                
                var hasPathToCore = attachedBlocks.HasPathToCore(attachable, ignoreCoordinates);
                
                if(hasPathToCore)
                    continue;

                return true;
            }

            return false;
        }
        
        #endregion //Check for New Disconnects

        //============================================================================================================//

        #region Shifting Bits
        
        /// <summary>
        /// Shits an entire row or column based on the direction and the bit selected.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="attachable"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void TryShift(DIRECTION direction, IAttachable attachable)
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

            var toShift = new List<IAttachable>();
            var dir = direction.ToVector2Int();
            var currentPos = attachable.Coordinate;
            
            //Debug.Log($"{inLine.Count} in line, moving {direction}");

            for (var i = 0; i < inLine.Count; i++)
            {
                var check = inLine.FirstOrDefault(x => x.Coordinate == currentPos);

                if (check == null)
                    break;
                
                if(check.CanShift)
                    toShift.Add(check);
                else
                    toShift.Clear();

                
                currentPos += dir;
            }

            //Debug.Log($"Shifting {toShift.Count} objects");
            //Debug.Break();

            StartCoroutine(ShiftInDirectionCoroutine(toShift, 
                direction,
                TEST_MergeSpeed,
                () =>
            {
                //Checks for floaters
                CheckForDisconnects();
                
                CheckForCombosAround(toShift.Where(x => attachedBlocks.Contains(x) && x is Bit).Select(x => x as Bit));
            }));

        }

        #endregion //Shifting Bits
        
        //============================================================================================================//

        #region Puzzle Checks

        private void CheckForCombosAround(Vector2Int coordinate)
        {
            CheckForCombosAround(attachedBlocks.FirstOrDefault(a => a.Coordinate == coordinate && a is Bit) as Bit);
        }

        private void CheckForCombosAround(IEnumerable<Bit> bits)
        {
            (ComboRemoteData comboData, List<Bit> toMove) data = (ComboRemoteData.zero, null);
            foreach (var bit in bits)
            {
                if (bit == null)
                    continue;
            
                if (bit.level >= 2)
                    continue;

                if (!PuzzleChecker.TryGetComboData(this, bit, out var temp))
                    continue;

                if (temp.comboData.points > data.comboData.points)
                    data = temp;

            }

            if (data.comboData.points == 0)
                return;

            SimpleComboSolver(data.comboData, data.toMove);
        }
        private void CheckForCombosAround(Bit bit)
        {
            if (bit == null)
                return;
            
            if (bit.level >= 2)
                return;

            if (!PuzzleChecker.TryGetComboData(this, bit, out var data))
                return;

            //if (data.comboData.addLevels == 2)
            //{
            //    AdvancedComboSolver(data.comboData, data.toMove);
            //}
            //else
                SimpleComboSolver(data.comboData, data.toMove);
        }

        //============================================================================================================//
        
        #region Combo Solvers
        
        /// <summary>
        /// Solves movement and upgrade logic to do with simple combos of blocks.
        /// </summary>
        /// <param name="comboBits"></param>
        /// <exception cref="Exception"></exception>
        private void SimpleComboSolver(ComboRemoteData comboData, IReadOnlyCollection<IAttachable> comboBits)
        {
            IAttachable closestToCore = null;
            var shortest = 999f;

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


                var dist = Vector2Int.Distance(bit.Coordinate, Vector2Int.zero);
                if (!(dist < shortest))
                    continue;

                shortest = dist;
                closestToCore = bit;
            }

            //Make sure that things are working
            //--------------------------------------------------------------------------------------------------------//

            //If no block was selected, then we've had a problem
            if (closestToCore == null)
                throw new Exception("No Closest Core Found");

            //See if anyone else needs to move
            //--------------------------------------------------------------------------------------------------------//

            //Get a list of Bits that will be moving (Blocks that are not the chosen closest to core)
            var movingBits = comboBits
                .Where(ab => ab != closestToCore).ToArray();

            //Get a list of orphans that may need move when we are moving our bits
            var orphans = new List<OrphanMoveData>();
            CheckForOrphans(movingBits, closestToCore, ref orphans);

            //Move everyone who we've determined need to move
            //--------------------------------------------------------------------------------------------------------//
            
            //if(orphans.Count > 0)
            //    Debug.Break();
            
            (closestToCore as Bit)?.IncreaseLevel(comboData.addLevels);

            //Move all of the components that need to be moved
            StartCoroutine(MoveComboPiecesCoroutine(
                movingBits,
                closestToCore,
                orphans.ToArray(),
                TEST_MergeSpeed,
                () =>
                {
                    var bit = closestToCore as Bit;

                    //We need to update the positions and level before we move them in case we interact with bits while they're moving

                    //bit.IncreaseLevel();

                    CheckForCombosAround(bit);
                    CheckForCombosAround(orphans.Select(x => x.attachableBase as Bit));
                    
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
                    attachedBlocks.GetAllAttachedBits(bit, movingBits, ref attachedToOrphan);

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
        public void CheckForMagnetOverage()
        {
            if (!useMagnet)
                return;
            
            var bits = attachedBlocks.OfType<Bit>().ToList();
            
            //Checks here if the total of attached blocks (Minus the Core) change
            if (bits.Count <= magnetCount)
                return;
            
            //--------------------------------------------------------------------------------------------------------//

            var toRemoveCount = bits.Count - magnetCount;
            var bitsToRemove = new List<Bit>();

            //--------------------------------------------------------------------------------------------------------//

            //float time;
            Action onDetach;
            
            switch (currentMagnet)
            {
                //----------------------------------------------------------------------------------------------------//
                case MAGNET.DEFAULT:
                    DefaultMagnetCheck(bits, out bitsToRemove, in toRemoveCount);
                    //time = 1f;
                    onDetach = () =>
                    {
                        DetachBitsCheck(bitsToRemove, true);
                    };
                    break;
                //----------------------------------------------------------------------------------------------------//
                case MAGNET.BUMP:
                    BumpMagnetCheck(bits, out bitsToRemove, in toRemoveCount);
                    //time = 0f;
                    onDetach = () =>
                    {
                        DetachBitsCheck(bitsToRemove, true);
                    };
                    break;
                //----------------------------------------------------------------------------------------------------//
                case MAGNET.LOWEST:
                    LowestMagnetCheckSimple(bits, ref bitsToRemove, ref toRemoveCount);
                    //time = 1f;
                    onDetach = () =>
                    {
                        DetachBitsCheck(bitsToRemove, true);
                    };
                    break;
                //----------------------------------------------------------------------------------------------------//
                default:
                    throw new ArgumentOutOfRangeException();
                //----------------------------------------------------------------------------------------------------//
            }

            if (BitsPendingDetach == null)
                BitsPendingDetach = new List<IAttachable>();
            
            BitsPendingDetach.AddRange(bitsToRemove);
            
            //Visually show that the bits will fall off by changing their color
            if (TEST_SetDetachColor)
            {
                foreach (var bit in bitsToRemove)
                {
                    bit.SetColor(Color.gray);
                } 
            }
            
            if(TEST_DetachTime == 0f)
                onDetach.Invoke();
            else
                this.DelayedCall(TEST_DetachTime, onDetach);
            //--------------------------------------------------------------------------------------------------------//
            
            
        }

        private void DefaultMagnetCheck(List<Bit> bits, out List<Bit> bitsToRemove, in int toRemoveCount)
        {
            //Gets the last added overage to remove
            bitsToRemove = bits.GetRange(magnetCount, toRemoveCount);
            
            //Get the coordinates of the blocks leaving. This is used to determine if anyone will be left floating
            var leavingCoordinates = bitsToRemove.Select(a => a.Coordinate).ToList();

            //Go through the bots Blocks to make sure no one will be floating when we detach the parts.
            for (var i = attachedBlocks.Count - 1; i >= 0; i--)
            {
                if (bitsToRemove.Contains(attachedBlocks[i]))
                    continue;

                if (attachedBlocks.HasPathToCore(attachedBlocks[i], leavingCoordinates))
                    continue;

                Debug.LogError(
                    $"Found a potential floater {attachedBlocks[i].gameObject.name} at {attachedBlocks[i].Coordinate}",
                    attachedBlocks[i].gameObject);
            }
        }

        private void BumpMagnetCheck(List<Bit> bits, out List<Bit> bitsToRemove, in int toRemoveCount)
        {
            //Gets the last added overage to remove
            bitsToRemove = bits.GetRange(magnetCount, toRemoveCount);
            
            //Get the coordinates of the blocks leaving. This is used to determine if anyone will be left floating
            var leavingCoordinates = bitsToRemove.Select(a => a.Coordinate).ToList();

            //Go through the bots Blocks to make sure no one will be floating when we detach the parts.
            for (var i = attachedBlocks.Count - 1; i >= 0; i--)
            {
                if (bitsToRemove.Contains(attachedBlocks[i]))
                    continue;

                if (attachedBlocks.HasPathToCore(attachedBlocks[i], leavingCoordinates))
                    continue;

                Debug.LogError(
                    $"Found a potential floater {attachedBlocks[i].gameObject.name} at {attachedBlocks[i].Coordinate}",
                    attachedBlocks[i].gameObject);
            }

            
        }

        private void LowestMagnetCheckSimple(List<Bit> bits, ref List<Bit> bitsToRemove, ref int toRemoveCount)
        {
            var checkedBits = new List<Bit>();
            
            while (toRemoveCount > 0)
            {
                var toRemove = FindLowestBit(bits, checkedBits);

                if (toRemove == null)
                {
                    //Debug.LogError($"toRemove is NULL, {toRemoveCount} remaining bits unsolved");
                    break;
                }

                checkedBits.Add(toRemove);

                if (bits.Count == checkedBits.Count)
                {
                    //Debug.LogError($"Left with {toRemoveCount} bits unsolved");
                    break;
                }
                
                if (RemovalCausesDisconnects(new List<IAttachable>(bitsToRemove){toRemove}))
                    continue;

                //Debug.Log($"Found Lowest {toRemove.gameObject.name}", toRemove);
                
                bitsToRemove.Add(toRemove);

                toRemoveCount--;
            }

            if (toRemoveCount <= 0) 
                return;
            
            //Find alternative pieces if we weren't able to find all lowest
            foreach (var bit in bitsToRemove)
            {
                bits.Remove(bit);
            }
                
            while (toRemoveCount > 0)
            {
                var toRemove = FindFurthestRemovableBit(bits, bitsToRemove);
                    
                if(toRemove == null)
                    throw new Exception("Unable to find alternative pieces");
                    
                bitsToRemove.Add(toRemove);
                bits.Remove(toRemove);
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
        private Bit FindLowestBit(List<Bit> bits, ICollection<Bit> toIgnore)
        {
            //I Want the last Bit to be the fallback/default, if I can't find anything
            Bit selectedBit = null;
            var lowestLevel = 999;
            //The lowest Y coordinate
            var lowestCoordinate = 999;

            foreach (var bit in bits)
            {
                if (toIgnore.Contains(bit))
                    continue;
                
                if(bit.level > lowestLevel)
                    continue;

                //Checks if the piece is higher, and if it is, that the level is not higher than the currently selected Bit
                //This ensures that even if the lowest Bit is of high level, the lowest will always be selected
                if (bit.Coordinate.y > lowestCoordinate && !(bit.level < lowestLevel))
                        continue;

                if (RemovalCausesDisconnects(new List<IAttachable>(/*toIgnore*/) {bit}))
                    continue;

                selectedBit = bit;
                lowestLevel = bit.level;
                lowestCoordinate = bit.Coordinate.y;

            }

            if (selectedBit != null) 
                return selectedBit;
            
            
            foreach (var bit in bits)
            {
                if (toIgnore.Contains(bit))
                    continue;
            
                if(bit.level > lowestLevel)
                    continue;

                //Checks if the piece is higher, and if it is, that the level is not higher than the currently selected Bit
                //This ensures that even if the lowest Bit is of high level, the lowest will always be selected
                if (bit.Coordinate.y > lowestCoordinate)
                    continue;

                if (RemovalCausesDisconnects(new List<IAttachable>(/*toIgnore*/) {bit}))
                    continue;

                selectedBit = bit;
                lowestLevel = bit.level;
                lowestCoordinate = bit.Coordinate.y;

            }

            return selectedBit;
        }

        private Bit FindFurthestRemovableBit(List<Bit> bits, ICollection<Bit> toIgnore)
        {
            //I Want the last Bit to be the fallback/default, if I can't find anything
            Bit selectedBit = null;
            var furthestDistance = -999f;
            var lowestLevel = 999f;

            foreach (var bit in bits)
            {
                if (toIgnore.Contains(bit))
                    continue;

                var _dist = Vector2Int.Distance(bit.Coordinate, Vector2Int.zero);
                
                if(_dist < furthestDistance)
                    continue;

                if (lowestLevel < bit.level)
                    continue;

                if (RemovalCausesDisconnects(new List<IAttachable>(toIgnore) { bit }))
                    continue;

                selectedBit = bit;
                furthestDistance = _dist;
                lowestLevel = bit.level;

            }

            return selectedBit;
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
        /// <param name="movingBits"></param>
        /// <param name="target"></param>
        /// <param name="orphans"></param>
        /// <param name="speed"></param>
        /// <param name="OnFinishedCallback"></param>
        /// <returns></returns>
        private IEnumerator MoveComboPiecesCoroutine(IAttachable[] movingBits, IAttachable target,
            IReadOnlyList<OrphanMoveData> orphans, float speed, Action OnFinishedCallback)
        {
            //Prepare Bits to be moved
            //--------------------------------------------------------------------------------------------------------//
            
            
            
            foreach (var bit in movingBits)
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

            var bitTransforms = movingBits.Select(ab => ab.transform).ToArray();
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
            foreach (var bit in movingBits)
            {
                bit.SetAttached(false);
                Recycler.Recycle<Bit>(bit.gameObject);
            }

            //Re-enable the colliders on our orphans, and ensure they're in the correct position
            for (var i = 0; i < orphans.Count; i++)
            {
                orphanTransforms[i].localPosition = orphanTargetPositions[i];
                (orphans[i].attachableBase as Bit)?.SetColliderActive(true);
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
        private IEnumerator ShiftInDirectionCoroutine(IReadOnlyList<IAttachable> toMove, DIRECTION direction, float speed, Action OnFinishedCallback)
        {
            var dir = direction.ToVector2Int();
            var transforms = toMove.Select(x => x.transform).ToArray();
            var startPositions = transforms.Select(x => x.localPosition).ToArray();
            var targetPositions = toMove.Select(o =>
                transform.InverseTransformPoint((Vector2) transform.position +
                                                ((Vector2) o.Coordinate + dir)  * Constants.gridCellSize)).ToArray();

            foreach (var attachableBase in toMove)
            {
                (attachableBase as Bit)?.SetColliderActive(false);
                attachableBase.Coordinate += dir;
            }
            
            CompositeCollider2D.GenerateGeometry();

            var t = 0f;

            while (t < 1f)
            {
                for (var i = 0; i < transforms.Length; i++)
                {
                    if (toMove[i].Attached == false)
                        continue;
                    
                    transforms[i].localPosition = Vector2.Lerp(startPositions[i], targetPositions[i], t);
                }

                t += Time.deltaTime * speed;
                
                yield return null;
            }
            
            for (var i = 0; i < toMove.Count; i++)
            {
                transforms[i].localPosition = targetPositions[i];
                (toMove[i] as Bit)?.SetColliderActive(true);
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
                    attachable.gameObject.SetActive(false);
                }

                yield return new WaitForSeconds(0.35f);

                index++;
            }
            
            OnBotDied?.Invoke(this, deathMethod);
        }
        
        #endregion //Coroutines
        
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
                    default:
                        Destroy(attachable.gameObject);
                        break;
                }
            }
            
            attachedBlocks.Clear();
            BitsPendingDetach?.Clear();
            _parts.Clear();
        }
        
        #endregion //Custom Recycle
    }
}
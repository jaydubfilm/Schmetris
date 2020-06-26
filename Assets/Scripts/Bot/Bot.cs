using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using StarSalvager.AI;
using StarSalvager.Constants;
using StarSalvager.Factories;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Puzzle;
using StarSalvager.Utilities.Puzzle.Data;
using UnityEngine;

namespace StarSalvager
{
    public class Bot : MonoBehaviour
    {
        

        //============================================================================================================//

        [SerializeField, BoxGroup("PROTOTYPE")]
        public float TEST_Speed;

        [SerializeField, BoxGroup("PROTOTYPE")]
        public float TEST_RotSpeed;
        
        [SerializeField, Range(0.5f, 10f), BoxGroup("PROTOTYPE")]
        public float TEST_MergeSpeed = 2f;

        //============================================================================================================//

        public List<IAttachable> attachedBlocks => _attachedBlocks ?? (_attachedBlocks = new List<IAttachable>());

        [SerializeField, ReadOnly, Space(10f)] private List<IAttachable> _attachedBlocks;

        //============================================================================================================//

        public bool Moving => _moving;
        private bool _moving;

        public DIRECTION MoveDirection => _moveDirection;

        private DIRECTION _moveDirection;
        //public bool HasValidInput => _currentInput != 0f;

        private Vector2 targetPosition;
        private float _currentInput;

        public float DelayedAutoStartTime = 0.2f;
        private float _dasTimer;


        public bool Rotating => _rotating;

        private bool _rotating;
        private float targetRotation;

        private CompositeCollider2D CompositeCollider2D;
        private new Rigidbody2D rigidbody;

        //============================================================================================================//

        #region Unity Functions

        // Start is called before the first frame update
        private void Start()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            CompositeCollider2D = GetComponent<CompositeCollider2D>();
            //useCollision = false;

            //Mark as Core coordinate
            //Coordinate = Vector2Int.zero;
            var core = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateObject<IAttachable>(
                new BlockData
                {
                    Type = (int)PART_TYPE.CORE,
                    Coordinate = Vector2Int.zero,
                    Level = 0,
                });

            AttachNewBit(Vector2Int.zero, core);

        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Equals))
                TEST_Speed += 100;
            else if (Input.GetKeyDown(KeyCode.Minus))
                TEST_Speed -= 100;
        }

        private void FixedUpdate()
        {
            if (Moving)
                MoveBot();

            if (Rotating)
                RotateBot();
        }

        #endregion //Unity Functions

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

        public void Move(float direction, bool move = false)
        {
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                _currentInput = 0f;
                return;
            }
            
            _currentInput = direction;

            if (direction < 0)
                _moveDirection = DIRECTION.LEFT;
            else if (direction > 0)
                _moveDirection = DIRECTION.RIGHT;
            else
            {
                _moveDirection = DIRECTION.NULL;
                return;
            }

            if (!move)
                return;

            Move(_moveDirection);
        }

        public void Move(DIRECTION direction)
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

            _moveDirection = direction;

            if (Moving)
            {
                targetPosition += toMove * Values.gridCellSize;
            }
            else
            {
                targetPosition = (Vector2) transform.position + toMove * Values.gridCellSize;
                _dasTimer = 0f;
            }

            _moving = true;
        }

        #endregion //Input Solver

        //============================================================================================================//

        #region Movement

        private void MoveBot()
        {
            
            

            var position = rigidbody.position;

            //TODO See if this will be enough for the current setup, or if we will need something more robust.
            position = Vector2.MoveTowards(position, targetPosition, TEST_Speed * Time.fixedDeltaTime);

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

                Move(_currentInput, true);
                return;
            }

            _moving = false;
            rigidbody.position = targetPosition;
            targetPosition = Vector2.zero;
            _moveDirection = DIRECTION.NULL;
            _dasTimer = 0f;
        }

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
            if (Rotating)
                return false;

            if (attachable is Bit bit)
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
                        //TODO Destroy both this and collided Bit
                        Recycler.Recycle<Bit>(attachable.gameObject);

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
            }
            else if (attachable is EnemyAttachable enemyAttachable)
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
                if (dist > smallestDist)
                    continue;

                smallestDist = dist;
                selected = attached;
            }

            //selected.SetColor(Color.magenta);

            return selected;
        }

        /*public AttachableBase GetClosestAttachable(Vector2 checkPosition)
        {
            AttachableBase selected = null;

            var smallestDist = 999f;

            foreach (var attached in attachedBlocks)
            {
                //attached.SetColor(Color.white);

                var dist = Vector2.Distance(attached.transform.position, checkPosition);
                if (dist > smallestDist)
                    continue;

                smallestDist = dist;
                selected = attached;
            }

            //selected.SetColor(Color.magenta);

            return selected;
        }*/

        /// <summary>
        /// Returns the 2 closest objects
        /// </summary>
        /// <param name="checkCoordinate"></param>
        /// <returns></returns>
        public IAttachable[] GetClosestAttachables(Vector2Int checkCoordinate)
        {
            IAttachable[] selected = new IAttachable[2];

            var smallestDist = 999f;

            foreach (var attached in attachedBlocks)
            {
                //attached.SetColor(Color.white);

                var dist = Vector2Int.Distance(attached.Coordinate, checkCoordinate);

                if (dist > smallestDist)
                    continue;



                smallestDist = dist;
                selected[1] = selected[0];
                selected[0] = attached;
            }

            //selected[0].SetColor(Color.magenta);
            //selected[1]?.SetColor(Color.cyan);



            return selected;
        }

        private Vector2Int GetRelativeCoordinate(Vector2 worldPosition)
        {
            var botPosition = (Vector2) transform.position;

            var calculated = (worldPosition - botPosition) / Values.gridCellSize;
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
                    return _moveDirection == direction;
                case DIRECTION.UP:
                    return _moveDirection == DIRECTION.NULL;
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
            var closestOnBot= attachedBlocks.GetClosestAttachable(collisionPoint);

            if (closestShapeBit is Bit closeBit)
            {
                switch (closeBit.Type)
                {
                    case BIT_TYPE.BLACK:
                        //TODO Damage/Destroy Bits as required
                        shape.DestroyBit(closeBit);
                        
                        break;
                    case BIT_TYPE.BLUE:
                    case BIT_TYPE.GREEN:
                    case BIT_TYPE.GREY:
                    case BIT_TYPE.RED:
                    case BIT_TYPE.YELLOW:
                        var newBotCoordinate = closestOnBot.Coordinate + connectionDirection.ToVector2Int();
                        
                        var closestCoordinate = closestShapeBit.Coordinate;
                        var bitsToAdd = shape.AttachedBits.ToArray();
                        var differences = bitsToAdd.Select(x => x.Coordinate - closestCoordinate).ToArray();

                        //Add the entire shape to the Bot
                        for (var i = 0; i < bitsToAdd.Length; i++)
                        {
                            AttachNewBit(newBotCoordinate + differences[i], bitsToAdd[i], false, false);
                        }
                        
                        Recycler.Recycle<Shape>(shape.gameObject);
                        
                        CheckForCombosAround(bitsToAdd);
                        CompositeCollider2D.GenerateGeometry();

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(closeBit.Type), closeBit.Type, null);
                }
            }

            return true;
        }
        
        #endregion //Check for Legal Shape Attach
        
        //============================================================================================================//

        public void TryHitAt(Vector2 hitPosition, float damage)
        {
            var closestAttachable = attachedBlocks.GetClosestAttachable(hitPosition);

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
        
        
        //============================================================================================================//

        #region Attach Bits

        public void AttachNewBit(Vector2Int coordinate, IAttachable newAttachable, bool checkForCombo = true, bool updateColliderGeometry = true)
        {
            newAttachable.Coordinate = coordinate;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * coordinate * Values.gridCellSize);
            newAttachable.transform.SetParent(transform);

            attachedBlocks.Add(newAttachable);
            
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
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * coordinate * Values.gridCellSize);
            newAttachable.transform.SetParent(transform);

            attachedBlocks.Add(newAttachable);

            if(checkForCombo)
                CheckForCombosAround(coordinate);

            if(updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();
        }

        public void PushNewBit(IAttachable newAttachable, DIRECTION direction, bool checkForCombo = true, bool updateColliderGeometry = true)
        {
            var newCoord = direction.ToVector2Int();

            attachedBlocks.CoordinateOccupied(direction, ref newCoord);

            newAttachable.Coordinate = newCoord;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * newCoord * Values.gridCellSize);
            newAttachable.transform.SetParent(transform);

            attachedBlocks.Add(newAttachable);
            
            if(checkForCombo)
                CheckForCombosAround(newCoord);

            if(updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();
        }

        public void PushNewBit(IAttachable newAttachable, DIRECTION direction, Vector2Int startCoord, bool checkForCombo = true, bool updateColliderGeometry = true)
        {
            var newCoord = startCoord + direction.ToVector2Int();

            attachedBlocks.CoordinateOccupied(direction, ref newCoord);

            newAttachable.Coordinate = newCoord;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * newCoord * Values.gridCellSize);
            newAttachable.transform.SetParent(transform);

            attachedBlocks.Add(newAttachable);
            
            if(checkForCombo)
                CheckForCombosAround(newCoord);

            if(updateColliderGeometry)
                CompositeCollider2D.GenerateGeometry();
        }

        #endregion //Attach Bits

        #region Detach Bits
        
        private void DetachBits(IReadOnlyCollection<IAttachable> attachables)
        {
            foreach (var attachable in attachables)
            {
                attachedBlocks.Remove(attachable);
                
                //Debug.Log($"Detached group member {bit.gameObject.name}", bit);
            }

            var bits = attachables.OfType<Bit>().ToList();

            FactoryManager.Instance.GetFactory<ShapeFactory>().CreateGameObject(bits);
            
            CompositeCollider2D.GenerateGeometry();

        }
        private void DetachBit(IAttachable attachable)
        {
            attachable.transform.parent = null;

            RemoveAttachable(attachable);
            
            //Debug.Log($"Detached {bit.gameObject.name}", bit);
        }
        
        private void RemoveAttachable(IAttachable attachableBase)
        {
            attachedBlocks.Remove(attachableBase);
            attachableBase.SetAttached(false);
            
            CompositeCollider2D.GenerateGeometry();
        }
        
        #endregion //Detach Bits
        
        //============================================================================================================//
        
        #region Check for Orphans
        
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
                
                var hasPathToCore = this.HasPathToCore(attachableBase);
                
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
        
        #endregion //Check for Orphans

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
            foreach (var bit in bits)
            {
                CheckForCombosAround(bit);
            }
        }
        private void CheckForCombosAround(Bit bit)
        {
            if (bit == null)
                return;
            
            if (bit.level >= 2)
                return;

            if (!PuzzleChecker.TryGetComboData(this, bit, out var data))
                return;

            if (data.comboData.addLevels == 2)
            {
                AdvancedComboSolver(data.comboData, data.toMove);
            }
            else
                SimpleComboSolver(data.comboData, data.toMove);
        }

        //============================================================================================================//
        
        #region Combo Solvers
        
        /// <summary>
        /// Solves movement and upgrade logic to do with simple combos of blocks.
        /// </summary>
        /// <param name="comboBits"></param>
        /// <exception cref="Exception"></exception>
        private void SimpleComboSolver(ComboData comboData, IReadOnlyCollection<IAttachable> comboBits)
        {
            IAttachable closestToCore = null;
            var shortest = 999f;

            //Decide who gets to upgrade
            //--------------------------------------------------------------------------------------------------------//

            foreach (var bit in comboBits)
            {
                //Need to make sure that if we choose this block, that it is connected to the core one way or another
                var hasPath = this.HasPathToCore(bit as Bit,
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

            //--------------------------------------------------------------------------------------------------------//
        }

        private void AdvancedComboSolver(ComboData comboData, IReadOnlyList<IAttachable> comboBits)
        {
            IAttachable bestAttachableOption = null;

            //Decide who gets to upgrade
            //--------------------------------------------------------------------------------------------------------//

            foreach (var bit in comboBits)
            {
                //Need to make sure that if we choose this block, that it is connected to the core one way or another
                var hasPath = this.HasPathToCore(bit as Bit,
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

                    var hasPathToCore = this.HasPathToCore(bit,
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
                                                (Vector2) o.intendedCoordinates * Values.gridCellSize)).ToArray();
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
                                                ((Vector2) o.Coordinate + dir)  * Values.gridCellSize)).ToArray();

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
        
        #endregion //Coroutines
        
        //============================================================================================================//

    }
}
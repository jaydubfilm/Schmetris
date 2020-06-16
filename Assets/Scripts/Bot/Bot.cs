﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;
using UnityEngine.InputSystem;
using Input = StarSalvager.Utilities.Inputs.Input;

namespace StarSalvager
{
    public class Bot : AttachableBase, IInput
    {
        
        //============================================================================================================//
        [SerializeField, BoxGroup("PROTOTYPE")]
        public float TEST_BitSize = 1.28f;

        [SerializeField, BoxGroup("PROTOTYPE")]
        public float TEST_Speed;

        [SerializeField, BoxGroup("PROTOTYPE")]
        public float TEST_RotSpeed;

        //============================================================================================================//

        public List<AttachableBase> attachedBlocks => _attachedBlocks ?? (_attachedBlocks = new List<AttachableBase>());

        [SerializeField, ReadOnly, Space(10f)] private List<AttachableBase> _attachedBlocks;

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
            useCollision = false;
            
            //Mark as Core coordinate
            Coordinate = Vector2Int.zero;
            attachedBlocks.Add(this);
            
            InitInput();
        }

        // Update is called once per frame
        private void Update()
        {
            

            if (UnityEngine.Input.GetKeyDown(KeyCode.Equals))
                TEST_Speed += 100;
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Minus))
                TEST_Speed -= 100;
        }

        private void FixedUpdate()
        {
            if (Moving)
                MoveBot();
            
            if (Rotating)
                RotateBot();
        }

        //private void OnGUI()
        //{
        //    GUI.Box(new Rect(10,10,100,50), $"Speed: {TEST_Speed}" );
        //}

        private void OnDestroy()
        {
            DeInitInput();
        }
        
        #endregion //Unity Functions

        //============================================================================================================//
        
        #region Input Solver
        
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
                if(attachedBlock is Bot)
                    continue;
                
                attachedBlock.RotateCoordinate(rotation);
            }
            
            _rotating = true;

        }

        public void Move(float direction)
        {
            if(direction < 0)
                Move(DIRECTION.LEFT);
            else if(direction > 0)
                Move(DIRECTION.RIGHT);
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
                targetPosition += toMove * TEST_BitSize;
            }
            else
            {
                targetPosition = (Vector2)transform.position + toMove * TEST_BitSize;
                _dasTimer = 0f;
            }

            _moving = true;
        }
        
        #endregion //Input Solver
        
        //============================================================================================================//
        
        //TODO Might want to use Rigidbody motion instead of Transform. Investigate.
        
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
                
                Move(_currentInput);
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

        //FIXME Might want to have it so that the checks don't consider ups & downs at all
        public bool TryAddNewAttachable(AttachableBase attachable, DIRECTION connectionDirection, Vector2 point)
        {
            if (Rotating)
                return false;
            
            if (attachable is Bit bit)
            {
                bool legalDirection = false;
                DIRECTION direction = DIRECTION.NULL;
                AttachableBase closestAttachable = null;
                
                
                //TODO Need to get the coordinate of the collision
                var bitCoordinate = GetRelativeCoordinate(bit.transform.position);
                
                //Debug.DrawRay((Vector2)pointCoordinate, Vector3.up * 1.28f, Color.yellow);
                //Debug.Break();

                //----------------------------------------------------------------------------------------------------//
                
                closestAttachable = GetClosestAttachable(point);
                legalDirection = CheckLegalCollision(bitCoordinate, closestAttachable.Coordinate, out direction);
                
                //----------------------------------------------------------------------------------------------------//

                //FIXME This is not working correctly, as the closestAttachable is not being assigned correctly.
                //var closestAttachables = GetClosestAttachables(bitCoordinate);
                //foreach (var attachableBase in closestAttachables)
                //{
                //    if(attachableBase == null)
                //        continue;
                //    
                //    legalDirection = CheckLegalCollision(bitCoordinate, attachableBase.Coordinate, out direction);
//
                //    if (!legalDirection) 
                //        continue;
                //    
                //    closestAttachable = attachableBase;
                //    break;
                //}
                
                //----------------------------------------------------------------------------------------------------//
                
                

                if (!legalDirection)
                {
                    if (!CompositeCollider2D.OverlapPoint(attachable.transform.position))
                    {
                        //Debug.Log($"Non-legal direction {direction}", attachable);
                        return false;
                    }
                }
                
                //TODO Need to check if its legal to attach (Within threshold of connection)
                switch (bit.Type)
                {
                    case BIT_TYPE.BLACK:
                        //TODO Destroy both this and collided Bit
                        Destroy(attachable.gameObject);
                        break;
                    case BIT_TYPE.BLUE:
                    case BIT_TYPE.GREEN:
                    case BIT_TYPE.GREY:
                    case BIT_TYPE.RED:
                    case BIT_TYPE.YELLOW:
                        
                        //var coordinate = closestAttachable.Coordinate + direction.ToVector2Int();
//
                        //if (attachedBlocks.Any(a => a.Coordinate == coordinate))
                        //{
                        //    Debug.Log($"Checking Coordinate: {bitCoordinate} with {attachable.gameObject.name}", attachable.gameObject);
                        //    //Debug.Log($"Closest: {closestAttachable.gameObject.name}\n([0] {closestAttachables[0].gameObject.name} = {CheckLegalCollision(bitCoordinate, closestAttachables[0].Coordinate, out _)}, [1] {closestAttachables[1]?.gameObject.name} = {CheckLegalCollision(bitCoordinate, closestAttachables[1].Coordinate, out _)})");
                        //    Debug.Log($"Attaching new bit [{coordinate}] to {closestAttachable.Coordinate}");
                        //    //throw new Exception();
                        //    Debug.Break();
                        //}
                        
                        //TODO Add these to the block depending on its relative position
                        AttachNewBitToExisting(bit, closestAttachable, connectionDirection);
                        
                        
                        
                        break;
                    case BIT_TYPE.WHITE:
                        //TODO Destroy collided Bit
                        //TODO Try and shift collided row (Depending on direction)
                        Destroy(attachable.gameObject);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            //TODO Need to add other options here (ie Enemy) 
            
            
            
            return true;
        }

        public AttachableBase GetClosestAttachable(Vector2Int checkCoordinate)
        {
            AttachableBase selected = null;

            var smallestDist = 999f;
                
            foreach (var attached in attachedBlocks)
            {
                attached.SetColor(Color.white);
                
                var dist = Vector2Int.Distance(attached.Coordinate, checkCoordinate);
                if (dist > smallestDist)
                    continue;

                smallestDist = dist;
                selected = attached;
            }

            selected.SetColor(Color.magenta);
            
            return selected;
        }
        
        public AttachableBase GetClosestAttachable(Vector2 checkPosition)
        {
            AttachableBase selected = null;

            var smallestDist = 999f;
                
            foreach (var attached in attachedBlocks)
            {
                attached.SetColor(Color.white);
                
                var dist = Vector2.Distance(attached.transform.position, checkPosition);
                if (dist > smallestDist)
                    continue;

                smallestDist = dist;
                selected = attached;
            }

            selected.SetColor(Color.magenta);
            
            return selected;
        }
        
        /// <summary>
        /// Returns the 2 closest objects
        /// </summary>
        /// <param name="checkCoordinate"></param>
        /// <returns></returns>
        public AttachableBase[] GetClosestAttachables(Vector2Int checkCoordinate)
        {
            AttachableBase[] selected = new AttachableBase[2];

            var smallestDist = 999f;
                
            foreach (var attached in attachedBlocks)
            {
                attached.SetColor(Color.white);
                
                var dist = Vector2Int.Distance(attached.Coordinate, checkCoordinate);
                
                if (dist > smallestDist)
                    continue;

                
                
                smallestDist = dist;
                selected[1] = selected[0];
                selected[0] = attached;
            }

            selected[0].SetColor(Color.magenta);
            selected[1]?.SetColor(Color.cyan);
            
            
            
            return selected;
        }

        private Vector2Int GetRelativeCoordinate(Vector2 worldPosition)
        {
            var botPosition = (Vector2)transform.position;

            var calculated = (worldPosition - botPosition) / TEST_BitSize;
            return new Vector2Int(
                Mathf.RoundToInt(calculated.x),
                Mathf.RoundToInt(calculated.y));
        }

        //FIXME Need to check if the spot that is being checked is already occupied
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
        
        #region Attach Bits

        public void AttachNewBit(Vector2Int coordinate, AttachableBase newAttachable)
        {
            newAttachable.Coordinate = coordinate;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3)(Vector2.one * coordinate * TEST_BitSize);
            newAttachable.transform.SetParent(transform);
            
            attachedBlocks.Add(newAttachable);
            
            CompositeCollider2D.GenerateGeometry();
        }
        public void AttachNewBitToExisting(AttachableBase newAttachable, AttachableBase existingAttachable, DIRECTION direction)
        {
            var coordinate = existingAttachable.Coordinate + direction.ToVector2Int();

            //Checks for attempts to add attachable to occupied location
            if (attachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                Debug.LogError($"Prevented attaching {newAttachable.gameObject.name} to occupied location {coordinate}", newAttachable);
                PushNewBit(newAttachable, direction, existingAttachable.Coordinate);
                return;
            }
            
            newAttachable.Coordinate = coordinate;
            
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3)(Vector2.one * coordinate * TEST_BitSize);
            newAttachable.transform.SetParent(transform);
            
            attachedBlocks.Add(newAttachable);

            CheckForCombosAround(coordinate);
            
            CompositeCollider2D.GenerateGeometry();
        }
        
        public void PushNewBit(AttachableBase newAttachable, DIRECTION direction)
        {
            var newCoord = direction.ToVector2Int();

            attachedBlocks.CoordinateOccupied(direction, ref newCoord);

            newAttachable.Coordinate = newCoord;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3)(Vector2.one * newCoord * TEST_BitSize);
            newAttachable.transform.SetParent(transform);
            
            attachedBlocks.Add(newAttachable);
            
            CompositeCollider2D.GenerateGeometry();
        }
        public void PushNewBit(AttachableBase newAttachable, DIRECTION direction, Vector2Int startCoord)
        {
            var newCoord = startCoord + direction.ToVector2Int();

            attachedBlocks.CoordinateOccupied(direction, ref newCoord);

            newAttachable.Coordinate = newCoord;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3)(Vector2.one * newCoord * TEST_BitSize);
            newAttachable.transform.SetParent(transform);
            
            attachedBlocks.Add(newAttachable);
            
            CompositeCollider2D.GenerateGeometry();
        }

        #endregion //Attach Bits

        //============================================================================================================//

        #region Puzzle Checks

        private void CheckForCombosAround(Vector2Int coordinate)
        {
            int left, right, up, down;
            //Start at 1 since we should include the original Coordinate
            left = right = up = down = 1;

            ComboCountAlgorithm(coordinate, DIRECTION.LEFT.ToVector2Int(), ref left);
            ComboCountAlgorithm(coordinate, DIRECTION.RIGHT.ToVector2Int(), ref right);
            ComboCountAlgorithm(coordinate, DIRECTION.UP.ToVector2Int(), ref up);
            ComboCountAlgorithm(coordinate, DIRECTION.DOWN.ToVector2Int(), ref down);
            
            
            //Debug.Log($"Combo Checks Returned Left: {left}, Right: {right}, Up: {up}, Down: {down}");

        }

        private bool ComboCountAlgorithm(Vector2Int coordinate, Vector2Int direction, ref int count)
        {
            var nextCoord = coordinate + direction;
            if (attachedBlocks.FirstOrDefault(a => a.Coordinate == nextCoord && a is Bit) == null)
                return false;

            count++;
            return ComboCountAlgorithm(nextCoord, direction, ref count);
        }
        

        #endregion
        
        //============================================================================================================//

        protected override void OnCollide(Bot bot) { }
        public override BlockData ToBlockData()
        {
            throw new NotImplementedException();
        }

        public override void LoadBlockData(BlockData blockData)
        {
            throw new NotImplementedException();
        }

        //============================================================================================================//
        
        //TODO This needs to be fleshed out further
        
        #region Input
        
        public void InitInput()
        {
            
            Input.Actions.Default.SideMovement.Enable();
            Input.Actions.Default.SideMovement.performed += SideMovement;
            
            Input.Actions.Default.Rotate.Enable();
            Input.Actions.Default.Rotate.performed += Rotate;

        }

        public void DeInitInput()
        {
            Input.Actions.Default.SideMovement.Disable();
            Input.Actions.Default.SideMovement.performed -= SideMovement;
            
            Input.Actions.Default.Rotate.Disable();
            Input.Actions.Default.Rotate.performed -= Rotate;
        }
        
        private void SideMovement(InputAction.CallbackContext ctx)
        {
            if (UnityEngine.Input.GetKey(KeyCode.LeftAlt))
            {
                _currentInput = 0f;
                return;
            }
                
            _currentInput = ctx.ReadValue<float>();
            
            Move(_currentInput);
            
        }
        private void Rotate(InputAction.CallbackContext ctx)
        {
            if (UnityEngine.Input.GetKey(KeyCode.LeftAlt))
                return;
                
            var rot = ctx.ReadValue<float>();
                
            if(rot < 0)
                Rotate(ROTATION.CCW);
            else if(rot > 0)
                Rotate(ROTATION.CW);
        }
        
        #endregion //Input
        
        //============================================================================================================//

        
    }
}
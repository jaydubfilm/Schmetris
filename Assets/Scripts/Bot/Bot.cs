using System;
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
        private static readonly Quaternion[] rotations = 
        {
            Quaternion.Euler(0, 0, -90),
            Quaternion.Euler(0, 0, 90)
        };
        
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

        public bool HasValidInput => _currentInput != 0f;
        private float _currentInput;


        public bool Rotating => _rotating;

        private bool _rotating;
        private Quaternion targetRotation;
        
        //============================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            //Mark as Core coordinate
            Coordinate = Vector2Int.zero;
            attachedBlocks.Add(this);
            
            InitInput();
        }

        // Update is called once per frame
        private void Update()
        {
            if (HasValidInput)
                MoveBot();

            if (Rotating)
                RotateBot();
        }

        private void OnDestroy()
        {
            DeInitInput();
        }

        //============================================================================================================//
        
        /// <summary>
        /// Triggers a rotation 90deg in the specified direction. If the player is already rotating, it adds 90deg onto
        /// the target rotation.
        /// </summary>
        /// <param name="rotation"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Rotate(ROTATION rotation)
        {
            Quaternion toRotate;
            switch (rotation)
            {
                case ROTATION.CW:
                case ROTATION.CCW:
                    toRotate = rotations[(int) rotation];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null);
            }

            //If we're already rotating, we need to add the direction to the target
            if (Rotating)
            {
                targetRotation *= toRotate;
            }
            else
            {
                targetRotation = transform.rotation * toRotate;
            }
            
            foreach (var attachedBlock in attachedBlocks)
            {
                attachedBlock.RotateCoordinate(rotation);
            }
            
            _rotating = true;

        }
        
        //============================================================================================================//
        
        //TODO Might want to use Rigidbody motion instead of Transform. Investigate.

        private void MoveBot()
        {
            transform.position += Vector3.right * (_currentInput * TEST_Speed * Time.deltaTime);
        }


        private void RotateBot()
        {
            var rotation = transform.rotation;

            //Rotates towards the target rotation.
            rotation = Quaternion.RotateTowards(rotation, targetRotation, TEST_RotSpeed * Time.deltaTime);
            transform.rotation = rotation;

            //Here we check how close to the final rotation we are.
            var remainingDegrees = Quaternion.Angle(rotation, targetRotation);

            //If we're within 1deg we will count it as complete, otherwise continue to rotate.
            if (remainingDegrees > 1f)
                return;

            _rotating = false;
            //Force set the rotation to the target, in case the bot is not exactly on target
            transform.rotation = targetRotation;
            targetRotation = Quaternion.identity;
        }

        //============================================================================================================//

        public bool TryAddNewAttachable(AttachableBase attachable)
        {
            if (Rotating)
                return false;
            
            if (attachable is Bit bit)
            {
                //TODO Need to get the coordinate of the collision
                var bitCoordinate = GetRelativeCoordinate(bit.transform.position);
                
                var closestAttachable = GetClosestAttachable(bitCoordinate);

                var legalDirection = CheckLegalCollision(bitCoordinate, closestAttachable.Coordinate, out var direction);

                if (!legalDirection)
                    return false;
                
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
                        //TODO Add these to the block depending on its relative position
                        AttachNewBitToExisting(bit, closestAttachable, direction);
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

        //FIXME I want to be able to return the Bot Core if I don't have any other attachable Bits
        public AttachableBase GetClosestAttachable(Vector2Int checkCoordinate)
        {
            AttachableBase selected = null;

            var smallestDist = 999f;
                
            foreach (var attached in attachedBlocks)
            {
                var dist = Vector2Int.Distance(attached.Coordinate, checkCoordinate);
                if (dist >= smallestDist)
                    continue;

                smallestDist = dist;
                selected = attached;
            }

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

        private bool CheckLegalCollision(Vector2Int lhs, Vector2Int rhs, out DIRECTION direction)
        {
            direction = (lhs - rhs).ToDirection();

            switch (direction)
            {
                case DIRECTION.NULL:
                    return false;
                case DIRECTION.LEFT:
                    return _currentInput < 0f;
                case DIRECTION.UP:
                    return true;
                case DIRECTION.RIGHT:
                    return _currentInput > 0f;
                case DIRECTION.DOWN:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            //return direction != DIRECTION.NULL;
        }

        
        //============================================================================================================//

        public void AttachNewBit(Vector2Int coordinate, AttachableBase newAttachable)
        {
            newAttachable.Coordinate = coordinate;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3)(Vector2.one * coordinate * TEST_BitSize);
            newAttachable.transform.SetParent(transform);
            
            attachedBlocks.Add(newAttachable);
        }
        
        public void AttachNewBitToExisting(AttachableBase newAttachable, AttachableBase existingAttachable, DIRECTION direction)
        {
            var coordinate = existingAttachable.Coordinate + direction.ToVector2Int();
            newAttachable.Coordinate = coordinate;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3)(Vector2.one * coordinate * TEST_BitSize);
            newAttachable.transform.SetParent(transform);
            
            attachedBlocks.Add(newAttachable);
        }
        
        #if DEVELOPMENT_BUILD || UNITY_EDITOR

        public void PushNewBit(AttachableBase newAttachable, DIRECTION direction)
        {
            var newCoord = direction.ToVector2Int();

            attachedBlocks.CoordinateOccupied(direction, ref newCoord);

            newAttachable.Coordinate = newCoord;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3)(Vector2.one * newCoord * TEST_BitSize);
            newAttachable.transform.SetParent(transform);
            
            attachedBlocks.Add(newAttachable);
        }

#endif
        

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
        
        //============================================================================================================//

        
    }
}
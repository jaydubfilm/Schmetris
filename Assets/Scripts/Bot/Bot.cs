using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using Input = StarSalvager.Utilities.Inputs.Input;

namespace StarSalvager
{
    public class Bot : MonoBehaviour, IHealth, IInput
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

        public float StartingHealth => _startingHealth;
        private float _startingHealth;
        public float CurrentHealth => _currentHealth;
        private float _currentHealth;

        //============================================================================================================//

        public bool HasValidInput => _currentInput != 0f;
        private float _currentInput;


        public bool Rotating => _rotating;

        private bool _rotating;
        private Quaternion targetRotation;
        
        //============================================================================================================//

        private new Transform transform;

        //============================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            InitInput();

            transform = gameObject.transform;
        }

        // Update is called once per frame
        private void Update()
        {
            if (HasValidInput)
                MoveBot();

            if (Rotating)
                RotateBot();
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
            if (attachable is Bit bit)
            {
                //TODO Need to get the coordinate of the collision
                GetClosestAttachable(bit.transform.position);
                
                switch (bit.Type)
                {
                    case BIT_TYPE.BLACK:
                        //TODO Destroy both this and collided Bit
                        break;
                    case BIT_TYPE.BLUE:
                    case BIT_TYPE.GREEN:
                    case BIT_TYPE.GREY:
                    case BIT_TYPE.RED:
                    case BIT_TYPE.YELLOW:
                        //TODO Add these to the block depending on its relative position
                        break;
                    case BIT_TYPE.WHITE:
                        //TODO Destroy collided Bit
                        //TODO Try and shift collided row (Depending on direction)
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            //TODO Need to add other options here (ie Enemy) 
            
            
            
            return false;
        }

        public void GetClosestAttachable(Vector2 worldPosition)
        {
            var botPosition = (Vector2)transform.position;

            var calculated = (worldPosition - botPosition) / TEST_BitSize;
            var coordinate = new Vector2Int(Mathf.RoundToInt(calculated.x), Mathf.RoundToInt(calculated.y));

            var smallestDist = 999f;
            AttachableBase selected = null;
            foreach (var attached in attachedBlocks)
            {
                var dist = Vector2Int.Distance(attached.Coordinate, coordinate);
                if (dist >= smallestDist)
                    continue;

                smallestDist = dist;
                selected = attached;
            }
            
            //FIXME Need to consider that there may not be any attached blocks
            
            Debug.Log($"Calculated: {calculated}, Coordinate: {coordinate}, Closest: {selected.gameObject.name} {selected.Coordinate}", selected);
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
        
        #if DEVELOPMENT_BUILD || UNITY_EDITOR

        public void PushNewBit(AttachableBase newAttachable, DIRECTION direction)
        {
            var newCoord = direction.ToVector2Int();

            CoordinateOccupied(direction, ref newCoord);

            newAttachable.Coordinate = newCoord;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3)(Vector2.one * newCoord * TEST_BitSize);
            newAttachable.transform.SetParent(transform);
            
            attachedBlocks.Add(newAttachable);
        }

        private bool CoordinateOccupied(DIRECTION direction, ref Vector2Int coordinate)
        {
            var check = coordinate;
            var exists = attachedBlocks
                .Any(b => b.Coordinate == check);

            if (!exists)
                return false;

            coordinate += direction.ToVector2Int();

            return CoordinateOccupied(direction, ref coordinate);
        }
        
        #endif
        

        //============================================================================================================//
        public void ChangeHealth(float amount)
        {
            _currentHealth += amount;
        }
        
        //============================================================================================================//
        
        //TODO This needs to be fleshed out further
        public void InitInput()
        {
            
            Input.Actions.Default.SideMovement.Enable();
            Input.Actions.Default.SideMovement.performed += ctx =>
            {
                _currentInput = ctx.ReadValue<float>();
            };
            
            Input.Actions.Default.Rotate.Enable();
            Input.Actions.Default.Rotate.performed += ctx =>
            {
                var rot = ctx.ReadValue<float>();
                
                if(rot < 0)
                    Rotate(ROTATION.CCW);
                else if(rot > 0)
                    Rotate(ROTATION.CW);
            };

        }

        public void DeInitInput()
        {
            throw new System.NotImplementedException();
        }
        
        //============================================================================================================//

    }
}
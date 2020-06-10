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
        [SerializeField]
        public float TEST_BitSize = 1.28f;

        [SerializeField] 
        public float TEST_Speed;

        [ReadOnly]
        public float TEST_PlayerInput;
        //============================================================================================================//

        
        public List<AttachableBase> attachedBlocks => _attachedBlocks ?? (_attachedBlocks = new List<AttachableBase>());

        [SerializeField, ReadOnly]
        private List<AttachableBase> _attachedBlocks;
        
        //============================================================================================================//

        public float StartingHealth => _startingHealth;
        private float _startingHealth;
        public float CurrentHealth => _currentHealth;
        private float _currentHealth;

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
            if (TEST_PlayerInput == 0f)
                return;
                
            transform.position += Vector3.right * (TEST_PlayerInput * TEST_Speed * Time.deltaTime);
        }
        
        //============================================================================================================//

        public void Rotate(ROTATION rotation)
        {
            Quaternion toRotate;
            switch (rotation)
            {
                case ROTATION.CW:
                    toRotate = Quaternion.Euler(0, 0, -90);
                    break;
                case ROTATION.CCW:
                    toRotate = Quaternion.Euler(0, 0, 90);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null);
            }
            transform.rotation = transform.rotation * toRotate;
            
            foreach (var attachedBlock in attachedBlocks)
            {
                attachedBlock.RotateCoordinate(rotation);
            }
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
                TEST_PlayerInput = ctx.ReadValue<float>();
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
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.Utilities;
using UnityEngine;

namespace StarSalvager
{
    public class Bot : MonoBehaviour, IHealth, IInput
    {
        [SerializeField]
        public float TESTBitSize;
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
            transform = gameObject.transform;
        }

        // Update is called once per frame
        private void Update()
        {

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
            newAttachable.transform.position = Vector2.one * coordinate * TESTBitSize;
            newAttachable.transform.SetParent(transform);
            
            attachedBlocks.Add(newAttachable);
        }

        public void PushNewBit(AttachableBase newAttachable, DIRECTION direction)
        {
            var newCoord = direction.ToVector2Int();

            CoordinateOccupied(direction, ref newCoord);

            newAttachable.Coordinate = newCoord;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = Vector2.one * newCoord * TESTBitSize;
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
        
        public void InitInput()
        {
            throw new System.NotImplementedException();
        }

        public void DeInitInput()
        {
            throw new System.NotImplementedException();
        }
        
        //============================================================================================================//

    }
}
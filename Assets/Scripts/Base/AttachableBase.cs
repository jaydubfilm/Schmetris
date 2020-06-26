using System;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager
{
    /// <summary>
    /// Any object which can be connected together should derive from attachable
    /// </summary>
    public abstract class AttachableBase : CollidableBase, IHealth
    {

        //============================================================================================================//
        
        /// <summary>
        /// Offset from center block
        /// </summary>
        [DisableInEditorMode]
        public Vector2Int Coordinate;

        [SerializeField, ReadOnly]
        private bool Attached;

        //============================================================================================================//

        public bool IsAttached => Attached;
        public float StartingHealth => _startingHealth;
        private float _startingHealth;
        public float CurrentHealth => _currentHealth;
        private float _currentHealth;
        
        //============================================================================================================//
        
        public void SetupHealthValues(float startingHealth, float currentHealth)
        {
            _startingHealth = startingHealth;
            _currentHealth = currentHealth;
        }

        public void ChangeHealth(float amount)
        {
            _currentHealth += amount;

            if (_currentHealth <= 0)
            {
                Recycler.Recycle(typeof(AttachableBase), this.gameObject);
            }
        }
        
        //============================================================================================================//

        //TODO Might want to consider storing who we're attached to here
        public virtual void SetAttached(bool isAttached)
        {
            Attached = isAttached;
            collider.usedByComposite = isAttached;
        }

        public void RotateCoordinate(ROTATION rotation)
        {
            var _temp = Vector2Int.zero;
            
            switch (rotation)
            {
                case ROTATION.CW:
                    _temp.x = Coordinate.y;
                    _temp.y = Coordinate.x * -1;
                    
                    break;
                case ROTATION.CCW:
                    _temp.x = Coordinate.y * -1;
                    _temp.y = Coordinate.x;
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null);
            }
            
            //Rotate opposite of the Core rotation 
            transform.localRotation *= rotation.ToInverseQuaternion();

            Coordinate = _temp;
        }
        
        //============================================================================================================//

        public abstract BlockData ToBlockData();
        
        public abstract void LoadBlockData(BlockData blockData);

    }
}
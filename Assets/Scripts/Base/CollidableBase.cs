using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarSalvager
{
    /// <summary>
    /// Any object that can touch a bot should use this base class
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class CollidableBase : Actor2DBase
    {
        private bool _useCollision = true;

        protected string CollisionTag { get; set; } = "Player";

        //============================================================================================================//

        public new Collider2D collider
        {
            get
            {
                if (_collider == null)
                    _collider = gameObject.GetComponent<Collider2D>();

                return _collider;
            }
        }
        private Collider2D _collider;

        //Unity Functions
        //====================================================================================================================//
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!_useCollision)
                return;
            
            if (!other.gameObject.CompareTag(CollisionTag))
                return;

            var point = CalculateContactPoint(other.contacts);

            Debug.DrawRay(point, Vector3.right, Color.red, 1f);

            //FIXME I should be able to store the bot, so i can reduce my calls to GetComponent
            OnCollide(other.gameObject, point);
        }
        
        //TODO Consider how best to avoid using the Collision Stay
        private void OnCollisionStay2D(Collision2D other)
        {
            if (!_useCollision)
                return;
            
            if (!other.gameObject.CompareTag(CollisionTag))
                return;

            var point = CalculateContactPoint(other.contacts);
            
            Debug.DrawRay(point, Vector3.right, Color.cyan, 0.5f);
            
            OnCollide(other.gameObject, point);
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (_waitCollider == null)
                return;

            if (other.collider != _waitCollider)
                return;

            _useCollision = true;
            _waitCollider = null;

        }

        //============================================================================================================//

        private Collider2D _waitCollider;
        public void DisableColliderTillLeaves(Collider2D waitCollider)
        {
            _useCollision = false;
            //SetColliderActive(false);
            _waitCollider = collider;
        }
        
        public virtual void SetColliderActive(bool state)
        {
            collider.enabled = state;
        }
        
        //============================================================================================================//
        
        protected virtual bool TryGetRayDirectionFromBot(DIRECTION direction, out Vector2 rayDirection)
        {
            rayDirection = Vector2.zero;
            //Returns the opposite direction based on the current players move direction.
            switch (direction)
            {
                case DIRECTION.NULL:
                    rayDirection = Vector2.down;
                    return true;
                case DIRECTION.LEFT:
                    rayDirection = Vector2.right;
                    return true;
                case DIRECTION.RIGHT:
                    rayDirection = Vector2.left;
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        //============================================================================================================//
        
        /// <summary>
        /// Called when the object contacts a bot
        /// </summary>
        protected abstract void OnCollide(GameObject gameObject, Vector2 hitPoint);

        private static Vector2 CalculateContactPoint(IEnumerable<ContactPoint2D> points)
        {
            var contactPoint2Ds = points.ToArray();
            var point = contactPoint2Ds.Aggregate(Vector2.zero, (current, contact) => current + contact.point) / contactPoint2Ds.Length;
            
            
            //var shortest = 999f;
            //var point = Vector2.zero;
            //foreach (var contact in points)
            //{
            //    Debug.DrawRay(contact.point, Vector3.up, Color.blue, 1f);
            //    
            //    if (contact.separation > shortest)
            //        continue;
            //    
            //    
            //    shortest = contact.separation;
            //    point = contact.point;
            //}

            return point;
        }
        
        //============================================================================================================//


        
    }
}
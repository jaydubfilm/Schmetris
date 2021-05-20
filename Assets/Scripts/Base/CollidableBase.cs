using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using StarSalvager.Factories;
using StarSalvager.Prototype;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Helpers;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager
{
    /// <summary>
    /// Any object that can touch a bot should use this base class
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class CollidableBase : Actor2DBase, ICustomRecycle
    {
        
        private bool _useCollision = true;

        protected virtual string[] CollisionTags { get; set; } = {TagsHelper.PLAYER};

        protected virtual bool useCollisionStay => true;

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
        
        private Collider2D _waitCollider;
        

        //Unity Functions
        //====================================================================================================================//
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!_useCollision)
                return;
            
            if (!other.gameObject.CompareTags(CollisionTags))
                return;

            var contacts = new ContactPoint2D[5];

            var count = other.GetContacts(contacts);
            
            var point = CalculateContactPoint(contacts.ToList().GetRange(0, count));

            Debug.DrawRay(point, Vector3.right, Color.red, 1f);

            //FIXME I should be able to store the bot, so i can reduce my calls to GetComponent
            OnCollide(other.gameObject, point);
        }
        
        //TODO Consider how best to avoid using the Collision Stay
        private void OnCollisionStay2D(Collision2D other)
        {
            if (!_useCollision || !useCollisionStay)
                return;
            
            if (!other.gameObject.CompareTags(CollisionTags))
                return;

            var contacts = new ContactPoint2D[5];
            var count = other.GetContacts(contacts);
            
            var point = CalculateContactPoint(contacts.ToList().GetRange(0, count));
            
            Debug.DrawRay(point, Vector3.right, Color.cyan, 0.5f);
            
            OnCollide(other.gameObject, point);
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (_waitCollider == null)
                return;

            if (other.collider != _waitCollider)
                return;

            if (IsRecycled)
                return;
            
            //FIXME I'd like to find a better solution to this, as the coroutine seems cumbersome 
            //Only reset these properties x sec after exit. This prevents checking collisions again too early
            StartCoroutine(WaitForCollisionResetCoroutine(this, 0.1f));
            
        }

        //============================================================================================================//

        public void DisableColliderTillLeaves(Collider2D waitCollider)
        {
            if (waitCollider == collider)
                return;
            
            _useCollision = false;
            //SetColliderActive(false);
            _waitCollider = waitCollider;
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
        protected abstract void OnCollide(GameObject gameObject, Vector2 worldHitPoint);

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

        private static IEnumerator WaitForCollisionResetCoroutine(CollidableBase collidableBase, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            
            collidableBase._useCollision = true;
            collidableBase._waitCollider = null;
        }
        
        protected bool TryFindClosestCollision(in DIRECTION direction, in LayerMask? layerMask, out Vector2 point)
        {
            const float collisionDistThreshold = 0.55f;
            const float rayLength = Constants.gridCellSize * 3f;
            
            point = Vector2.zero;
            
            var currentPosition = (Vector2)transform.position;
            var vectorDirection = direction.ToVector2();
            var startOffset = -vectorDirection * (rayLength / 2f);
            Vector2 positionOffset;
            
            switch (direction)
            {
                case DIRECTION.RIGHT:
                case DIRECTION.LEFT:
                    positionOffset = Vector2.up * 0.33f;
                    break;
                case DIRECTION.UP:
                case DIRECTION.DOWN:
                    positionOffset = Vector2.right * 0.33f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
            
            var startPositions = new[]
            {
                currentPosition + startOffset,
                (currentPosition - positionOffset) + startOffset,
                (currentPosition + positionOffset) + startOffset,
            };

            var shortestDis = 999f;
            RaycastHit2D? shortestHit = null;
            foreach (var rayStartPosition in startPositions)
            {
                var hit = layerMask.HasValue
                    ? Physics2D.Raycast(rayStartPosition, vectorDirection, rayLength, layerMask.Value.value)
                    : Physics2D.Raycast(rayStartPosition, vectorDirection, rayLength);

                //If nothing was hit, ray failed, thus no reason to continue
                if (hit.collider == null)
                {
                    //Debug.DrawRay(rayStartPosition, vectorDirection * rayLength, Color.yellow, 1f);
                    SSDebug.DrawArrowRay(rayStartPosition, vectorDirection * rayLength, Color.yellow);
                    continue;
                }

                Debug.DrawRay(hit.point, Vector2.up, Color.red);
                Debug.DrawRay(rayStartPosition, vectorDirection * rayLength, Color.green);
                
                if(Vector2.Distance(Position, hit.point) > collisionDistThreshold)
                    continue;

                if (hit.distance >= shortestDis)
                    continue;
                
                shortestDis = hit.distance;
                shortestHit = hit;
            }

            if (!shortestHit.HasValue)
                return false;

            point = shortestHit.Value.point;
            
            return true;
        }

        public virtual void CustomRecycle(params object[] args)
        {
            _useCollision = true;
            _waitCollider = null;
        }
        
        //============================================================================================================//

        protected static void CreateExplosionEffect(Vector2 worldPosition)
        {
            var explosion = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreateEffect(EffectFactory.EFFECT.EXPLOSION);
            LevelManager.Instance.ObstacleManager.AddToRoot(explosion);
            explosion.transform.position = worldPosition;

            var time = explosion.GetComponent<ParticleSystemGroupScaling>().AnimationTime;
            
            Destroy(explosion, time);
        }


    }
}
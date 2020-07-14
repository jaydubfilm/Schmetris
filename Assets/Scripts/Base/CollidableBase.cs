using System.Collections.Generic;
using System.Linq;
using Recycling;
using UnityEngine;

namespace StarSalvager
{
    /// <summary>
    /// Any object that can touch a bot should use this base class
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class CollidableBase : MonoBehaviour, IRecycled
    {
        public bool IsRecycled { get; set; }
        
        //private const int CHECK_FREQUENCY = 1;
//
        //private int checks;
        protected bool useCollision = true;


        protected string CollisionTag { get; set; } = "Player";

        //============================================================================================================//

        protected new Collider2D collider
        {
            get
            {
                if (_collider == null)
                    _collider = gameObject.GetComponent<Collider2D>();

                return _collider;
            }
        }
        private Collider2D _collider;

        protected new SpriteRenderer renderer
        {
            get
            {
                if (_renderer == null)
                    _renderer = gameObject.GetComponent<SpriteRenderer>();

                return _renderer;
            }
        }
        private SpriteRenderer _renderer;
        
        
        public new Transform transform
        {
            get
            {
                if (_transform == null)
                    _transform = gameObject.GetComponent<Transform>();

                return _transform;
            }
        }
        private Transform _transform;
        
        //============================================================================================================//

        private void OnCollisionEnter2D(Collision2D other)
        {
            
            if (!useCollision)
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
            if (!useCollision)
                return;
            
            if (!other.gameObject.CompareTag(CollisionTag))
                return;

            var point = CalculateContactPoint(other.contacts);
            
            Debug.DrawRay(point, Vector3.right, Color.cyan, 0.5f);
            
            OnCollide(other.gameObject, point);
        }
        
        //============================================================================================================//

        public void SetSprite(Sprite sprite)
        {
            renderer.sprite = sprite;
        }

        public virtual void SetColor(Color color)
        {
            renderer.color = color;
        }

        public virtual void SetColliderActive(bool state)
        {
            collider.enabled = state;
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
using System.Linq;
using UnityEngine;

namespace StarSalvager
{
    /// <summary>
    /// Any object that can touch a bot should use this base class
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class CollidableBase : MonoBehaviour
    {
        //private const int CHECK_FREQUENCY = 1;
//
        //private int checks;
        protected bool useCollision = true;

        protected virtual string CollisionTag => "Player";
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

            //other.contacts.Select(p => p.point).Sum(x => x);

            var averageContactPoint = other.contacts.Aggregate(Vector2.zero, (current, contact) => current + contact.point) / other.contactCount;
            
            //Debug.DrawRay(averageContactPoint, Vector3.right, Color.red, 1f);

            //FIXME I should be able to store the bot, so i can reduce my calls to GetComponent
            OnCollide(other.gameObject, averageContactPoint);
        }
        
        //TODO Consider how best to avoid using the Collision Stay
        private void OnCollisionStay2D(Collision2D other)
        {
            if (!useCollision)
                return;
            
            if (!other.gameObject.CompareTag(CollisionTag))
                return;

            var averageContactPoint = other.contacts.Aggregate(Vector2.zero, (current, contact) => current + contact.point) / other.contactCount;
            
            //Debug.DrawRay(averageContactPoint, Vector3.right, Color.cyan, 0.5f);
            
            OnCollide(other.gameObject, averageContactPoint);
        }
        
        //============================================================================================================//

        public void SetSprite(Sprite sprite)
        {
            renderer.sprite = sprite;
        }

        public void SetColor(Color color)
        {
            renderer.color = color;
        }

        public void SetColliderActive(bool state)
        {
            collider.enabled = state;
        }
        
        //============================================================================================================//

        /// <summary>
        /// Called when the object contacts a bot
        /// </summary>
        protected abstract void OnCollide(GameObject gameObject, Vector2 hitPoint);
        
        //============================================================================================================//
    }
}
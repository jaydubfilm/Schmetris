using UnityEngine;

namespace StarSalvager
{
    /// <summary>
    /// Any object that can touch a bot should use this base class
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class CollidableBase : MonoBehaviour
    {
        //private const int CHECK_FREQUENCY = 1;
//
        //private int checks;
        protected bool useCollision = true;
        //============================================================================================================//
        
        protected new BoxCollider2D collider
        {
            get
            {
                if (_collider == null)
                    _collider = gameObject.GetComponent<BoxCollider2D>();

                return _collider;
            }
        }
        private BoxCollider2D _collider;

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
        
        
        protected new Transform transform
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
            
            if (!other.gameObject.CompareTag("Player"))
                return;

            //FIXME I should be able to store the bot, so i can reduce my calls to GetComponent
            OnCollide(other.gameObject.GetComponent<Bot>());
        }
        
        //TODO Consider how best to avoid using the Collision Stay
        private void OnCollisionStay2D(Collision2D other)
        {
            if (!useCollision)
                return;

            //This currently reduces the amount of total GetComponent calls 
            //This isn't a long term solution, though solves the problem for now.
            //if (checks == CHECK_FREQUENCY)
            //{
            //    checks = 0;
            //    return;
            //}
            //
            //if (checks++ != 0)
            //    return;
            
            //Debug.Log($"{gameObject.name} Collided with {other.gameObject.name}");
            
            if (!other.gameObject.CompareTag("Player"))
                return;

            var bot = other.gameObject.GetComponent<Bot>();
            
            //FIXME This should trigger a falling state, not destroy the object
            if (bot.Rotating)
            {
                Destroy(gameObject);
                return;
            }
            
            
            OnCollide(bot);
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
        
        //============================================================================================================//

        /// <summary>
        /// Called when the object contacts a bot
        /// </summary>
        protected abstract void OnCollide(Bot bot);
        
        //============================================================================================================//
    }
}
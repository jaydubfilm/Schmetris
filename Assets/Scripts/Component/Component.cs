using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager
{
    public class Component : CollidableBase, IComponent, IAttachable, ICustomRecycle, IHealth, ICanBeHit, IObstacle
    {
        [SerializeField]
        private LayerMask collisionMask;
        
        private Damage _damage;
        
        //IObstacle Properties
        //============================================================================================================//

        public bool CanMove => !Attached;
        
        //IComponent Properties
        //============================================================================================================//
        public COMPONENT_TYPE Type { get; set; }

        //IAttachable Properties
        //============================================================================================================//
        
        [ShowInInspector, ReadOnly]
        public Vector2Int Coordinate { get; set; }
        [ShowInInspector, ReadOnly]
        public bool Attached { get; set; }
        public bool CountAsConnected => true;
        public bool CanDisconnect => true;
        public bool CanShift => true;
        
        //IHealth Properties
        //============================================================================================================//
        public float StartingHealth { get; private set; }
        [ShowInInspector, ReadOnly, ProgressBar(0,"StartingHealth")]
        public float CurrentHealth { get;  private set; }
        
        //============================================================================================================//
        
        protected override void OnCollide(GameObject gameObject, Vector2 hitPoint)
        {
            var bot = gameObject.GetComponent<Bot>();

            if (bot.Rotating)
            {
                float rotation = 180.0f;
                if (bot.MostRecentRotate == ROTATION.CW)
                {
                    rotation *= -1;
                }

                Vector2 direction = (Vector2)transform.position - hitPoint;
                direction.Normalize();
                /*if (direction != Vector2.up)
                {
                    Vector2 downVelocity = Vector2.down * Constants.gridCellSize / Globals.AsteroidFallTimer;
                    downVelocity.Normalize();
                    direction += downVelocity;
                    direction.Normalize();
                }*/
                
                LevelManager.Instance.ObstacleManager.BounceObstacle(this, direction, rotation, true, true, true);
                return;
            }

            var dir = (hitPoint - (Vector2)transform.position).ToVector2Int();

            //Checks to see if the player is moving in the correct direction to bother checking, and if so,
            //return the direction to shoot the ray
            if (!TryGetRayDirectionFromBot(Globals.MovingDirection, out var rayDirection))
                return;
            
            //Debug.Log($"Direction: {dir}, Ray Direction: {rayDirection}");

            if (dir != rayDirection && dir != Vector2Int.zero)
                return;

            //Long ray compensates for the players high speed
            var rayLength = Constants.gridCellSize * 3f;
            var rayStartPosition = (Vector2) transform.position + -rayDirection * (rayLength / 2f);


            //Checking ray against player layer mask
            var hit = Physics2D.Raycast(rayStartPosition, rayDirection, rayLength,  collisionMask.value);

            //If nothing was hit, ray failed, thus no reason to continue
            if (hit.collider == null)
            {
                Debug.DrawRay(rayStartPosition, rayDirection * rayLength, Color.yellow, 1f);
                SSDebug.DrawArrowRay(rayStartPosition, rayDirection * rayLength, Color.yellow);
                return;
            }

            Debug.DrawRay(hit.point, Vector2.up, Color.red);
            Debug.DrawRay(rayStartPosition, rayDirection * rayLength, Color.green);

            //Here we flip the direction of the ray so that we can tell the Bot where this piece might be added to
            var inDirection = (-rayDirection).ToDirection();
            bot.TryAddNewAttachable(this, inDirection, hit.point);
        }
        
        //ICanBeHit Functions
        //============================================================================================================//
        
        public void TryHitAt(Vector2 position, float damage)
        {
            ChangeHealth(-damage);
        }
        
        //IAttachableFunctions
        //============================================================================================================//
        
        public void SetAttached(bool isAttached)
        {
            Attached = isAttached;
            collider.usedByComposite = isAttached;
        }
        
        //IHealth Functions
        //============================================================================================================//
        
        public void SetupHealthValues(float startingHealth, float currentHealth)
        {
            StartingHealth = startingHealth;
            CurrentHealth = currentHealth;
        }

        public void ChangeHealth(float amount)
        {
            CurrentHealth += amount;

            if (CurrentHealth <= 0)
            {
                Recycler.Recycle<Component>(this);
                return;
            }

            if (_damage == null)
            {
                _damage = FactoryManager.Instance.GetFactory<DamageFactory>().CreateObject<Damage>();
                _damage.transform.SetParent(transform, false);
            }
                
            _damage.SetHealth(CurrentHealth/StartingHealth);
        }
        
        //ICustomRecycle Functions
        //============================================================================================================//

        public virtual void CustomRecycle(params object[] args)
        {
            SetAttached(false);

            if (_damage)
            {
                Recycler.Recycle<Damage>(_damage);
                _damage = null;
            }
        }
        
        //============================================================================================================//

    }
}


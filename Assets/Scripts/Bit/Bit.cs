using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Values;
using StarSalvager.Factories;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager
{
    public class Bit : CollidableBase, IAttachable, IBit, ISaveable, IHealth, IObstacle, ICustomRecycle, ICanBeHit, IRotate
    {
        //IAttachable properties
        //============================================================================================================//

        [ShowInInspector, ReadOnly]
        public Vector2Int Coordinate { get; set; }
        [ShowInInspector, ReadOnly]
        public bool Attached { get; set; }

        public bool Rotating => _rotating;
        private bool _rotating;

        public int RotateDirection => _rotateDirection;
        private int _rotateDirection = 1;

        public bool CountAsConnected => true;
        public bool CanDisconnect => true;

        [ShowInInspector, ReadOnly]
        public bool CanShift => true;

        //IHealth Properties
        //============================================================================================================//

        public float StartingHealth { get; private set; }
        [ShowInInspector, ReadOnly, ProgressBar(0,"StartingHealth")]
        public float CurrentHealth { get; private set; }

        //IObstacle Properties
        //============================================================================================================//
        public bool CanMove => !Attached;

        //Bit Properties
        //============================================================================================================//
        [ShowInInspector, ReadOnly]
        public BIT_TYPE Type { get; set; }
        [ShowInInspector, ReadOnly]
        public int level { get; private set; }

        [SerializeField]
        private LayerMask collisionMask;

        private Damage _damage;

        //IAttachable Functions
        //============================================================================================================//

        public void SetAttached(bool isAttached)
        {
            Attached = isAttached;
            collider.usedByComposite = isAttached;
        }

        //IRotate Functions
        //============================================================================================================//

        public void SetRotating(bool isRotating)
        {
            _rotating = isRotating;
            
            //Only need to set the rotation value when setting rotation to true
            if(_rotating)
                _rotateDirection = Random.Range(-1, 2);
        }

        //IHealth Functions
        //============================================================================================================//

        public void SetupHealthValues(float startingHealth, float currentHealth)
        {
            StartingHealth = startingHealth;
            CurrentHealth = currentHealth;
            
            SetColor(Color.white);
        }

        public void ChangeHealth(float amount)
        {
            //float previousHealth = _currentHealth;

            CurrentHealth += amount;

            if (CurrentHealth <= 0)
            {
                Recycler.Recycle<Bit>(this);
                return;
            }

            ////TODO - temporary demo color change, remove later
            //if (previousHealth > _currentHealth)
            //{
            //    SetColor(Color.Lerp(renderer.color, Color.black, 0.2f));
            //}
            
            if (_damage == null)
            {
                _damage = FactoryManager.Instance.GetFactory<DamageFactory>().CreateObject<Damage>();
                _damage.transform.SetParent(transform, false);
            }
                
            _damage.SetHealth(CurrentHealth/StartingHealth);
        }

        //ICanBeHit Functions
        //============================================================================================================//
        
        public void TryHitAt(Vector2 position, float damage)
        {
            ChangeHealth(-damage);
        }

        //Bit Functions
        //============================================================================================================//

        public void IncreaseLevel(int amount = 1)
        {
            level += amount;
            renderer.sortingOrder = level;

            //Sets the gameObject info (Sprite)
            var bit = this;
            FactoryManager.Instance.GetFactory<BitAttachableFactory>().UpdateBitData(Type, level, ref bit);
        }

        protected override void OnCollide(GameObject gameObject, Vector2 hitPoint)
        {
            //Debug.Break();
            
            
            var bot = gameObject.GetComponent<Bot>();

            if (bot.Rotating)
            {
                if (Type == BIT_TYPE.BLACK)
                {
                    //Recycler.Recycle<Bit>(this);
                    bot.Rotate(bot.MostRecentRotate.Invert());
                    bot.TryHitAt(hitPoint, 10);
                    return;
                }

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

            if (Type == BIT_TYPE.BLACK)
            {
                bot.TryAddNewAttachable(this, DIRECTION.UP, hitPoint);
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

        //ISaveable Functions
        //============================================================================================================//

        public BlockData ToBlockData()
        {
            return new BlockData
            {
                ClassType = GetType().Name,
                Coordinate = Coordinate,
                Type = (int)Type,
                Level = level
            };
        }

        public void LoadBlockData(BlockData blockData)
        {
            Coordinate = blockData.Coordinate;
            Type = (BIT_TYPE) blockData.Type;
            level = blockData.Level;
        }

        //============================================================================================================//

        public virtual void CustomRecycle(params object[] args)
        {
            SetAttached(false);
            transform.rotation = Quaternion.identity;
            SetRotating(false);

            if (_damage)
            {
                Recycler.Recycle<Damage>(_damage);
                _damage = null;
            }
            
        }

        
    }
}

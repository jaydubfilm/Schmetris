using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Values;
using TMPro;
using UnityEngine;

namespace StarSalvager
{
    public class Component : CollidableBase, IComponent, IAttachable, ICustomRecycle, IHealth, ICanBeHit, IObstacle, ISaveable, ICanCombo<COMPONENT_TYPE>, ICanDetach
    {
        [SerializeField]
        private LayerMask collisionMask;
        
        private Damage _damage;

        //ICanCombo Properties
        //====================================================================================================================//

        public IAttachable iAttachable => this;
        
        public bool IsBusy { get; set; }
        
        //IObstacle Properties
        //============================================================================================================//

        public bool CanMove => !Attached;

        public bool IsRegistered { get; set; }

        public bool IsMarkedOnGrid { get; set; }

        //IComponent Properties
        //============================================================================================================//
        public COMPONENT_TYPE Type { get; set; }

        //ILevel Properties
        //====================================================================================================================//
        
        [ShowInInspector, ReadOnly]
        public int level { get; private set; }
        
        
        //IAttachable Properties
        //============================================================================================================//
        
        [ShowInInspector, ReadOnly]
        public Vector2Int Coordinate { get; set; }


        [ShowInInspector, ReadOnly]
        public bool Attached { get; set; }
        public bool CountAsConnectedToCore => true;
        public bool CanShift => true;

        //public bool CanDisconnect => true;
        public int AttachPriority => (10 + (int) Type) * level;
        public bool PendingDetach { get; set; }
        public bool CountTowardsMagnetism => false;

        //IHealth Properties
        //============================================================================================================//
        public float StartingHealth { get; private set; }
        [ShowInInspector, ReadOnly, ProgressBar(0,"StartingHealth")]
        public float CurrentHealth { get;  private set; }
        
        //============================================================================================================//
        
        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            var bot = gameObject.GetComponent<Bot>();

            if (bot.Rotating)
            {
                this.Bounce(worldHitPoint, transform.position, bot.MostRecentRotate);
                return;
            }

            var dir = (worldHitPoint - (Vector2)transform.position).ToVector2Int();

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
        
        public bool TryHitAt(Vector2 worldPosition, float damage)
        {
            ChangeHealth(-damage);

            return true;
        }

        //ICanCombo Functions
        //====================================================================================================================//
        
        public void IncreaseLevel(int amount = 1)
        {
            level = Mathf.Clamp(level + amount, 0, 2);
            renderer.sortingOrder = level;
            
            //Sets the gameObject info (Sprite)
            var bit = this;
            FactoryManager.Instance.GetFactory<ComponentAttachableFactory>().UpdateComponentData(Type, level, ref bit);

            /*if (level == 0)
            {
                if (_label) _label.text = string.Empty;

                return;
            }

            if (!_label)
            {
                _label = FactoryManager.Instance.GetFactory<ParticleFactory>().CreateObject<TextMeshPro>();
                _label.transform.SetParent(transform, false);
                _label.transform.localPosition = Vector3.zero;
            }

            _label.text = $"{level * 3}";*/
        }
        
        //IAttachableFunctions
        //============================================================================================================//
        
        public void SetAttached(bool isAttached)
        {
            Attached = isAttached;
            collider.usedByComposite = isAttached;
            
            if (!isAttached) PendingDetach = false;

            //if (!isAttached)
            //{
            //    transform.SetParent(null);
            //}
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
                _damage = FactoryManager.Instance.GetFactory<EffectFactory>().CreateObject<Damage>();
                _damage.transform.SetParent(transform, false);
            }
                
            _damage.SetHealth(CurrentHealth/StartingHealth);
        }

        //ISaveable Functions
        //============================================================================================================//

        public BlockData ToBlockData()
        {
            return new BlockData
            {
                ClassType = nameof(Component),
                Coordinate = Coordinate,
                Type = (int)Type,
                Level = level,
            };
        }

        public void LoadBlockData(BlockData blockData)
        {
            Coordinate = blockData.Coordinate;
            Type = (COMPONENT_TYPE)blockData.Type;
            level = blockData.Level;
        }

        //ICustomRecycle Functions
        //============================================================================================================//

        public virtual void CustomRecycle(params object[] args)
        {
            SetAttached(false);
            PendingDetach = false;
            IsBusy = false;
            
            if (_damage)
            {
                Recycler.Recycle<Damage>(_damage);
                _damage = null;
            }
        }
        
        //============================================================================================================//
        
        //IHasBounds Functions
        //====================================================================================================================//
        
        public Bounds GetBounds()
        {
            return new Bounds
            {
                center = transform.position,
                size = Vector2.one * Constants.gridCellSize
            };
        }

        //====================================================================================================================//

    }
}


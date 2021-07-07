using System;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Values;
using StarSalvager.Factories;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Helpers;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarSalvager
{
    public class Bit : CollidableBase, IAttachable, IBit, ISaveable<BitData>, IHealth, IObstacle, ICanBeHit, IRotate, ICanCombo<BIT_TYPE>, ICanDetach, IAdditiveMove, ICanFreeze
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

        public bool CountAsConnectedToCore => true;
        public bool CanDisconnect => true;

        [ShowInInspector, ReadOnly]
        public bool CanShift => true;

        public int AttachPriority => level;
        public bool PendingDetach { get; set; }

        public bool CountTowardsMagnetism => true;

        public bool HasCollided;

        public bool toBeCollected;

        //ICanCombo Properties
        //====================================================================================================================//
        public IAttachable iAttachable => this;
        
        public bool IsBusy { get; set; }

        //IHealth Properties
        //============================================================================================================//

        public float StartingHealth { get; private set; }
        [ShowInInspector, ReadOnly, ProgressBar(0,"StartingHealth")]
        public float CurrentHealth { get; private set; }

        //IAdditiveMove Properties
        //====================================================================================================================//
        
        public Vector2 AddMove { get; set; }

        //IObstacle Properties
        //============================================================================================================//
        public bool CanMove => !Attached;

        public bool IsRegistered { get; set; } = false;

        public bool IsMarkedOnGrid { get; set; } = false;

        //ICanFreeze Properties
        //====================================================================================================================//

        public bool Frozen => FreezeTime > 0f;
        public float FreezeTime { get; private set; }

        //Bit Properties
        //============================================================================================================//
        [ShowInInspector, ReadOnly]
        public BIT_TYPE Type { get; set; }
        [ShowInInspector, ReadOnly]
        public int level { get; private set; }

        [SerializeField]
        private LayerMask collisionMask;

        private Damage _damage;

        //Unity Functions
        //====================================================================================================================//

        private void Update()
        {
            if (!Frozen) 
                return;
            
            FreezeTime -= Time.deltaTime;
            
            //If the change sets this no longer frozen, change the color back
            if(FreezeTime <= 0) SetSprite(Type.GetSprite(level));
        }

        //IAttachable Functions
        //============================================================================================================//

        public void SetAttached(bool isAttached)
        {
            Attached = isAttached;
            collider.usedByComposite = isAttached;

            if (!isAttached)
            {
                PendingDetach = false;
                SetRotating(false);
            }
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
                _damage = FactoryManager.Instance.GetFactory<EffectFactory>().CreateObject<Damage>();
                _damage.transform.SetParent(transform, false);
            }
                
            _damage.SetHealth(CurrentHealth/StartingHealth);
        }

        //ICanBeHit Functions
        //============================================================================================================//
        
        public bool TryHitAt(Vector2 worldPosition, float damage)
        {
            ChangeHealth(-damage);
            
            return true;
        }

        //Bit Functions
        //============================================================================================================//

        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            if (Frozen)
                return;
            
            var bot = gameObject.GetComponent<Bot>();

            if (bot is null)
                return;

            if (bot.Rotating)
            {
                HasCollided = true;
                this.Bounce(worldHitPoint, transform.position, bot.MostRecentRotate);
                AudioController.PlaySound(SOUND.BIT_BOUNCE);
                return;
            }

            var dir = (worldHitPoint - (Vector2)transform.position).ToVector2Int();
            //var direction = dir.ToDirection();

            //Checks to see if the player is moving in the correct direction to bother checking, and if so,
            //return the direction to shoot the ray
            if (!TryGetRayDirectionFromBot(Globals.MovingDirection, out var rayDirection))
                return;
            
            if (dir != rayDirection && dir != Vector2Int.zero)
                return;

            if (!TryFindClosestCollision(rayDirection.ToDirection(), collisionMask, out var point))
                return;

            //Here we flip the direction of the ray so that we can tell the Bot where this piece might be added to
            var inDirection = (-rayDirection).ToDirection();
            bot.TryAddNewAttachable(this, inDirection, point);
            HasCollided = true;


        }



        //ICanCombo Functions
        //====================================================================================================================//
        
        public void IncreaseLevel(int amount = 1)
        {
            level = Mathf.Clamp(level + amount, 0, 4);
            renderer.sortingOrder = level;

            UpdateBitData();
        }

        public void DecreaseLevel(int amount = 1)
        {
            level = Mathf.Clamp(level - amount, 0, 4);
            renderer.sortingOrder = level;

            UpdateBitData();
        }

        private void UpdateBitData()
        {
            UpdateBitData(Type, level);
        }
        public void UpdateBitData(in BIT_TYPE newType, in int newLevel)
        {
            Type = newType;
            level = newLevel;
            var bit = this;
            FactoryManager.Instance.GetFactory<BitAttachableFactory>().UpdateBitData(Type, level, ref bit);
        }
        

        //ICanFreeze Functions
        //====================================================================================================================//
        
        public void SetFrozen(in float time)
        {
            if (Frozen)
                return;
            
            FreezeTime = time;
            
            if(Frozen) SetSprite(FactoryManager.Instance.BitProfileData.FrozenSprite);
        }

        //ISaveable Functions
        //============================================================================================================//

        public BitData ToBlockData()
        {
            return new BitData
            {
                Coordinate = Coordinate,
                Type = (int)Type,
                Level = level
            };
        }

        public void LoadBlockData(IBlockData blockData)
        {
            if (!(blockData is BitData bitData))
                throw new Exception();
            
            Coordinate = bitData.Coordinate;
            Type = (BIT_TYPE)bitData.Type;
            level = bitData.Level;
        }

        //============================================================================================================//

        public override void CustomRecycle(params object[] args)
        {
            base.CustomRecycle(args);
            
            //Make sure we unfreeze if destroyed before solved
            FreezeTime = 0;
            SetColor(Color.white);
            
            
            SetAttached(false);
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            SetRotating(false);
            HasCollided = false;

            SetSortingLayer(LayerHelper.ACTORS);

            PendingDetach = false;
            IsBusy = false;
            
            AddMove = Vector2.zero;

            if (_damage)
            {
                Recycler.Recycle<Damage>(_damage);
                _damage = null;
            }
            
        }


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

        IBlockData ISaveable.ToBlockData()
        {
            return ToBlockData();
        }

    }
}

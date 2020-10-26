using System;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager
{
    public class Part : CollidableBase, IAttachable, ICustomRotate, ISaveable, IPart, IHealthBoostable, ICustomRecycle
    {
        //IAttachable Properties
        //============================================================================================================//
        [ShowInInspector, ReadOnly]
        public Vector2Int Coordinate { get; set; }

        public bool Attached
        {
            get => true;
            set { }
        }

        public bool CountAsConnectedToCore => !Destroyed;
        public bool CanShift => false;
        public bool CountTowardsMagnetism => false;

        //IHealth Properties
        //============================================================================================================//

        public float StartingHealth { get; private set; }

        [ShowInInspector, ReadOnly, ProgressBar(0, nameof(BoostedHealth))]
        public float CurrentHealth { get; private set; }

        //IHealthCanBoost Properties
        //====================================================================================================================//

        public float BoostedHealth => StartingHealth + BoostAmount;
        public float BoostAmount { get; private set; }
        private bool _boostIsSetup;

        //Part Properties
        //============================================================================================================//
        [ShowInInspector, ReadOnly]
        public PART_TYPE Type { get; set; }
        [ShowInInspector, ReadOnly]
        public int level { get; private set; }
        
        public bool Destroyed { get; private set; }

        public bool LockRotation { get; set; }

        public bool Disabled
        {
            get => _disabled;
            set
            {
                _disabled = value;
                SetColor(value ? Color.gray : Color.white);
            }
        }

        private bool _disabled;

        
        private Damage _damage;

        //Unity Functions
        //====================================================================================================================//
        
        private void Start()
        {
            collider.usedByComposite = true;
        }

        //IAttachable Functions
        //============================================================================================================//

        public void SetAttached(bool isAttached)
        {
        }

        //IHealth Functions
        //====================================================================================================================//
        
        public void SetupHealthValues(float startingHealth, float currentHealth)
        {
            StartingHealth = startingHealth;
            CurrentHealth = currentHealth;

            SetDestroyed(CurrentHealth <= 0f);
            
            if(CurrentHealth < BoostedHealth)
                UpdateDamage();
        }

        public void ChangeHealth(float amount)
        {
            if (Destroyed)
                return;
            
            CurrentHealth += amount;

            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                SetDestroyed(true);
                return;
            }

            UpdateDamage();
        }

        //IHealthCanBoost Functions
        //====================================================================================================================//
        
        public void SetHealthBoost(float boostAmount)
        {
            //Consider floating point errors
            if (Math.Abs(boostAmount - BoostAmount) < 0.01f)
                return;
            
            if (boostAmount < BoostAmount)
            {
                CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, StartingHealth + boostAmount);
            }
            else if (boostAmount > BoostAmount && !_boostIsSetup)
            {
                CurrentHealth = StartingHealth + boostAmount;
                _boostIsSetup = true;
            }
            
            BoostAmount = boostAmount;
        }
        
        //====================================================================================================================//
        

        private void UpdateDamage()
        {
            if (Destroyed)
                return;
            
            if (_damage == null)
            {
                _damage = FactoryManager.Instance.GetFactory<DamageFactory>().CreateObject<Damage>();
                _damage.transform.SetParent(transform, false);
            }

            _damage.SetHealth(CurrentHealth / BoostedHealth);
        }

        //Part Functions
        //============================================================================================================//

        protected override void OnCollide(GameObject gObj, Vector2 hitPoint)
        {
#if !UNITY_EDITOR
            //FIXME Need to find the cause of parts not despawning correctly
            if (IsRecycled)
                return;
            
            Recycler.Recycle<Part>(this);
#else
            throw new Exception("PARTS SHOULD NOT COLLIDE");
#endif

        }

        private void SetDestroyed(bool isDestroyed)
        {
            Destroyed = isDestroyed;

            //collider.enabled = !Destroyed;
            
            //TODO Need to update the sprite
            if (!Destroyed)
            {
                renderer.sprite = FactoryManager.Instance.PartsProfileData.GetProfile(Type).GetSprite(level);
                return;
            }

            RecycleDamageEffect();
            renderer.sprite = FactoryManager.Instance.PartsProfileData.GetDamageSprite(level);
            
        }

        //ICustomRotateFunctions
        //====================================================================================================================//
        
        public void CustomRotate(Quaternion rotation)
        {
            if (LockRotation)
                return;
            
            transform.localRotation = rotation;
        }
        
        //ISaveable Functions
        //============================================================================================================//

        public BlockData ToBlockData()
        {
            return new BlockData
            {
                ClassType = nameof(Part),
                Coordinate = Coordinate,
                Type = (int) Type,
                Level = level,
                Health = CurrentHealth
            };
        }

        public void LoadBlockData(BlockData blockData)
        {
            Coordinate = blockData.Coordinate;
            Type = (PART_TYPE) blockData.Type;
            level = blockData.Level;
            CurrentHealth = blockData.Health;

        }

        //============================================================================================================//


        public void CustomRecycle(params object[] args)
        {
            BoostAmount = 0f;
            _boostIsSetup = false;
            
            SetColor(Color.white);

            RecycleDamageEffect();
            Destroyed = false;
            Disabled = false;
            SetColliderActive(true);
            //collider.enabled = true;
        }

        private void RecycleDamageEffect()
        {
            if (!_damage) 
                return;
            
            Recycler.Recycle<Damage>(_damage);
            _damage = null;
        }



    }
}

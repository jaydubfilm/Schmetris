using System;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager
{
    public class Part : CollidableBase, IAttachable, ICustomRotate, ISaveable, IPart, IHealth, ICustomRecycle
    {
        //IAttachable Properties
        //============================================================================================================//
        [ShowInInspector, ReadOnly]
        public Vector2Int Coordinate { get; set; }
        [ShowInInspector, ReadOnly]
        public bool Attached { get; set; }

        public bool CountAsConnectedToCore => !Destroyed;
        public bool CanDisconnect => false;
        public bool CanShift => false;
        public bool CountTowardsMagnetism => false;

        //IHealth Properties
        //============================================================================================================//

        public float StartingHealth { get; private set; }

        [ShowInInspector, ReadOnly, ProgressBar(0,"StartingHealth")]
        public float CurrentHealth { get; private set; }

        //Part Properties
        //============================================================================================================//
        [ShowInInspector, ReadOnly]
        public PART_TYPE Type { get; set; }
        [ShowInInspector, ReadOnly]
        public int level { get; private set; }
        
        public bool Destroyed { get; private set; }

        
        private Damage _damage;


        //IAttachable Functions
        //============================================================================================================//

        public void SetAttached(bool isAttached)
        {
            if (Destroyed)
                return;
            
            Attached = isAttached;
            collider.usedByComposite = isAttached;
        }

        public void SetupHealthValues(float startingHealth, float currentHealth)
        {
            StartingHealth = startingHealth;
            CurrentHealth = currentHealth;

            SetDestroyed(CurrentHealth <= 0f);
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

            if (_damage == null)
            {
                _damage = FactoryManager.Instance.GetFactory<DamageFactory>().CreateObject<Damage>();
                _damage.transform.SetParent(transform, false);
            }
                
            _damage.SetHealth(CurrentHealth/StartingHealth);
        }

        //Part Functions
        //============================================================================================================//

        protected override void OnCollide(GameObject gObj, Vector2 hitPoint)
        {
            throw new System.NotImplementedException();
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
            if (Type == PART_TYPE.TRIPLE_SHOT)
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
            SetColor(Color.white);

            RecycleDamageEffect();
            Destroyed = false;
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

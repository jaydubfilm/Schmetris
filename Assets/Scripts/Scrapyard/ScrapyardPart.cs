using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager
{
    public class ScrapyardPart : MonoBehaviour, IAttachable, ISaveable, IPart, IHealth, ICustomRecycle
    {
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
        
        private Damage _damage;

        //IHealth Properties
        //====================================================================================================================//
        
        public float StartingHealth { get; private set; }
        [ShowInInspector, ReadOnly, ProgressBar(0, nameof(StartingHealth))]
        public float CurrentHealth { get; private set; }


        //IAttachable Properties
        //============================================================================================================//
        [ShowInInspector, ReadOnly]
        public Vector2Int Coordinate { get; set; }
        [ShowInInspector, ReadOnly]
        public bool Attached { get; set; }

        public bool CountAsConnectedToCore => !Destroyed;
        public bool CanDisconnect => false;

        [ShowInInspector, ReadOnly]
        public bool CanShift => false;

        public bool CountTowardsMagnetism => false;

        //Part Properties
        //============================================================================================================//
        public bool Destroyed => CurrentHealth <= 0f;
        public bool Disabled => false;

        [ShowInInspector, ReadOnly]
        public PART_TYPE Type { get; set; }
        [ShowInInspector, ReadOnly]
        public int level { get; private set; }

        //IAttachable Functions
        //============================================================================================================//

        public void SetAttached(bool isAttached)
        {
            Attached = isAttached;
        }

        //IHealth Functions
        //====================================================================================================================//
        
        public void SetupHealthValues(float startingHealth, float currentHealth)
        {
            StartingHealth = startingHealth;
            CurrentHealth = currentHealth;

            if (startingHealth == currentHealth && _damage == null)
                return;

            if(!Destroyed)
                UpdateDamage();
        }

        private void UpdateDamage()
        {
            if (_damage == null)
            {
                _damage = FactoryManager.Instance.GetFactory<DamageFactory>().CreateObject<Damage>();
                _damage.transform.SetParent(transform, false);
            }
                
            _damage.SetHealth(CurrentHealth/StartingHealth); 
        }

        public void ChangeHealth(float amount)
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, StartingHealth);
            
            if(!Destroyed)
                UpdateDamage();
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
                Level = level,
                Health = CurrentHealth
            };
        }

        public void LoadBlockData(BlockData blockData)
        {
            Coordinate = blockData.Coordinate;
            Type = (PART_TYPE)blockData.Type;
            level = blockData.Level;
            CurrentHealth = blockData.Health;
        }

        //============================================================================================================//

        public void SetSprite(Sprite sprite)
        {
            renderer.sprite = sprite;
        }

        public void SetLevel(int newLevel)
        {
            level = newLevel;
        }

        //ICustomRecycle Functions
        //====================================================================================================================//
        
        public void CustomRecycle(params object[] args)
        {
            if (!_damage) 
                return;
            
            Recycler.Recycle<Damage>(_damage);
            _damage = null;
        }
    }
}
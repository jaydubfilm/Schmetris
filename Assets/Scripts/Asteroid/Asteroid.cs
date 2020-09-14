using System;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Values;
using StarSalvager.Factories;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEditor.Tilemaps;
using StarSalvager.AI;

namespace StarSalvager
{
    public class Asteroid : CollidableBase, IHealth, IObstacle, ICustomRecycle, ICanBeHit, IRotate
    {
        private Damage _damage;

        //IRotate properties
        //============================================================================================================//

        public bool Rotating => _rotating;
        private bool _rotating;

        public int RotateDirection => _rotateDirection;
        private int _rotateDirection = 1;


        //IHealth Properties
        //============================================================================================================//

        public float StartingHealth { get; private set; }
        [ShowInInspector, ReadOnly, ProgressBar(0,"StartingHealth")]
        public float CurrentHealth { get; private set; }

        //IObstacle Properties
        //============================================================================================================//
        public bool CanMove => true;

        public bool IsRegistered
        {
            get { return m_isRegistered; }
            set { m_isRegistered = value; }
        }
        private bool m_isRegistered = false;

        public bool IsMarkedOnGrid
        {
            get { return m_isMarkedOnGrid; }
            set { m_isMarkedOnGrid = value; }
        }
        private bool m_isMarkedOnGrid = false;

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

        //CollidableBase Functions
        //============================================================================================================//

        protected override void OnCollide(GameObject gameObject, Vector2 hitPoint)
        {
            //Debug.Break();
            
            var bot = gameObject.GetComponent<Bot>();

            if (bot != null)
            {
                if (bot.Rotating)
                {
                    //Recycler.Recycle<Bit>(this);
                    bot.Rotate(bot.MostRecentRotate.Invert());
                    AudioController.PlaySound(SOUND.ASTEROID_BASH);
                    bot.TryHitAt(hitPoint, 10);
                    return;
                }

                var dir = (hitPoint - (Vector2)transform.position).ToVector2Int();
                var direction = dir.ToDirection();

                //If the player moves sideways into this asteroid, push them away, and damage them, to give them a chance
                switch (direction)
                {
                    case DIRECTION.LEFT:
                    case DIRECTION.RIGHT:
                        InputManager.Instance.ForceMove(direction);
                        bot.TryBounceAt(hitPoint);
                        break;
                    case DIRECTION.UP:
                    case DIRECTION.DOWN:
                        bot.TryAsteroidDamageAt(hitPoint);
                        break;
                        //default:
                        //    throw new ArgumentOutOfRangeException();
                }

                return;
            }

            var projectile = gameObject.GetComponent<Projectile>();

            if (projectile != null)
            {

            }
        }

        //============================================================================================================//

        public virtual void CustomRecycle(params object[] args)
        {
            transform.rotation = Quaternion.identity;
            SetRotating(false);

            renderer.sortingOrder = 0;

            if (_damage)
            {
                Recycler.Recycle<Damage>(_damage);
                _damage = null;
            }
        }
    }
}

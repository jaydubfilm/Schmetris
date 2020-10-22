using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using UnityEngine;
using Random = UnityEngine.Random;
using StarSalvager.AI;
using StarSalvager.Factories;
using StarSalvager.Utilities.Animations;
using StarSalvager.Values;
using StarSalvager.Factories.Data;
using System.Linq;

namespace StarSalvager
{
    public class Asteroid : CollidableBase, IHealth, IObstacle, ICustomRecycle, ICanBeHit, IRotate
    {
        public RDSTable rdsTable { get; set; }

        public float Radius { get; private set; }
        //IRotate properties
        //============================================================================================================//

        public bool Rotating { get; private set; }

        public int RotateDirection { get; private set; } = 1;


        //IHealth Properties
        //============================================================================================================//

        public float StartingHealth { get; private set; }
        [ShowInInspector, ReadOnly, ProgressBar(0,"StartingHealth")]
        public float CurrentHealth { get; private set; }

        //IObstacle Properties
        //============================================================================================================//
        public bool CanMove => true;

        public bool IsRegistered { get; set; }

        public bool IsMarkedOnGrid { get; set; } = false;

        //IRotate Functions
        //============================================================================================================//

        public void SetRotating(bool isRotating)
        {
            Rotating = isRotating;
            
            //Only need to set the rotation value when setting rotation to true
            if(Rotating)
                RotateDirection = Random.Range(-1, 2);
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
            CurrentHealth += amount;

            if (CurrentHealth > 0) 
                return;

            //Spawns loot
            if (rdsTable != null)
            {
                LevelManager.Instance.DropLoot(rdsTable.rdsResult.ToList(), transform.localPosition, true);
            }
            
            Recycler.Recycle<Asteroid>(this);
        }

        //ICanBeHit Functions
        //============================================================================================================//
        
        public bool TryHitAt(Vector2 worldPosition, float damage)
        {
            ChangeHealth(-damage);
            
            var explosion = FactoryManager.Instance.GetFactory<ParticleFactory>().CreateObject<Explosion>();
            LevelManager.Instance.ObstacleManager.AddToRoot(explosion);
            explosion.transform.position = worldPosition;

            return true;
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
                    //Recycler.Recycle<Asteroid>(this);
                    bot.Rotate(bot.MostRecentRotate.Invert());
                    AudioController.PlaySound(SOUND.ASTEROID_BASH);
                    bot.TryHitAt(hitPoint, Globals.AsteroidDamage);
                    return;
                }

                var dir = (hitPoint - (Vector2)transform.position).ToVector2Int();
                var direction = dir.ToDirection();

                //If the player moves sideways into this asteroid, push them away, and damage them, to give them a chance
                switch (direction)
                {
                    case DIRECTION.LEFT:
                    case DIRECTION.RIGHT:
                        //Only want to move the bot if we're legally allowed
                        if(bot.TryBounceAt(hitPoint))
                            InputManager.Instance.ForceMove(direction);

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

        //Asteroid Functions
        //============================================================================================================//

        public void SetRadius(float radius)
        {
            Radius = radius;
        }
        
        //ICustomRecycle Function
        //====================================================================================================================//
        
        public void CustomRecycle(params object[] args)
        {
            transform.rotation = Quaternion.identity;
            SetRotating(false);

            renderer.sortingOrder = 0;
            Radius = 0f;
        }

        #region UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (transform == null)
                return;
            
            Gizmos.DrawWireSphere(transform.position, Radius);
        }

        #endregion //UNITY_EDITOR
    }
}

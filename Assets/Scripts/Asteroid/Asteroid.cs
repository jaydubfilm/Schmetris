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

        public bool IsRegistered { get; set; } = false;

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
            
            Recycler.Recycle<Asteroid>(this);
        }

        //ICanBeHit Functions
        //============================================================================================================//
        
        public bool TryHitAt(Vector2 position, float damage)
        {
            ChangeHealth(-damage);

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

        //============================================================================================================//

        public virtual void CustomRecycle(params object[] args)
        {
            transform.rotation = Quaternion.identity;
            SetRotating(false);

            renderer.sortingOrder = 0;
        }
    }
}

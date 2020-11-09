﻿using UnityEngine;
using StarSalvager.Factories.Data;
using System;
using Recycling;
using StarSalvager.Cameras;
using StarSalvager.Utilities;

namespace StarSalvager.AI
{
    //TODO: Handle proper setting of the collision tag
    public class Projectile : CollidableBase, ICustomRecycle
    {
        private Vector3 TravelDirectionNormalized { get; set; }
        private Vector3 EnemyVelocityModifier { get; set; }
        private ProjectileProfileData ProjectileData { get; set; }

        private float _damageAmount;

        private bool _hasRange;
        private float _lifeTime;
        private CollidableBase _target;

        //============================================================================================================//

        // Update is called once per frame
        private void Update()
        {
            if (GameTimer.IsPaused)
                return;

            if (_hasRange)
                CheckLifeTime();


            if (!CameraController.IsPointInCameraRect(transform.position))
            {
                Recycler.Recycle<Projectile>(this);
                return;
            }

            ApplyMovement();
        }

        //============================================================================================================//

        public virtual void Init(ProjectileProfileData profileData, 
            CollidableBase target, 
            string collisionTag,
            float damage,
            float rangeBoost,
            Vector2 direction, Vector2 velocity)
        {
            ProjectileData = profileData;

            _target = target;

            CollisionTag = collisionTag;
            _damageAmount = damage;

            TravelDirectionNormalized = direction;
            EnemyVelocityModifier = velocity;

            transform.up = direction;

            if (ProjectileData.ProjectileRange > 0)
            {
                _hasRange = true;

                //Calculates the time it will take to travel the distance
                _lifeTime = ProjectileData.ProjectileRange * rangeBoost / ProjectileData.ProjectileSpeed;
            }
        }

        private void CheckLifeTime()
        {
            if (_lifeTime > 0f)
            {
                _lifeTime -= Time.deltaTime;
                return;
            }

            Recycler.Recycle<Projectile>(this);
        }

        private void ApplyMovement()
        {
            var newPosition = transform.position;

            switch (ProjectileData.AttackType)
            {
                //----------------------------------------------------------------------------------------------------//

                case ENEMY_ATTACKTYPE.Forward:
                case ENEMY_ATTACKTYPE.AtPlayer:
                case ENEMY_ATTACKTYPE.AtPlayerCone:
                case ENEMY_ATTACKTYPE.Down:
                case ENEMY_ATTACKTYPE.Random_Spray:
                case ENEMY_ATTACKTYPE.Spiral:
                case ENEMY_ATTACKTYPE.Fixed_Spray:
                    newPosition +=
                        (EnemyVelocityModifier + TravelDirectionNormalized * ProjectileData.ProjectileSpeed) *
                        Time.deltaTime;
                    break;

                //----------------------------------------------------------------------------------------------------//

                case ENEMY_ATTACKTYPE.Heat_Seeking:

                    if (_target != null)
                    {
                        if (_target is IRecycled iRecycled && !iRecycled.IsRecycled)
                        {
                            
                            var up = transform.up;
                            var direction = (_target.transform.position - transform.position).normalized;
                            var rotation = Vector3.Cross(up, direction).z;
                    
                            transform.rotation *= Quaternion.Euler(Vector3.forward * (rotation * 5f));
                        }
                        else
                        {
                            _target = null;
                        }
                    }

                    newPosition += transform.up.normalized * (ProjectileData.ProjectileSpeed * Time.deltaTime);
                    break;

                //----------------------------------------------------------------------------------------------------//

                default:
                    throw new ArgumentOutOfRangeException(nameof(ProjectileData.AttackType), ProjectileData.AttackType,
                        null);
            }

            transform.position = newPosition;
        }

        //============================================================================================================//

        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            var canBeHit = gameObject.GetComponent<ICanBeHit>();

            if (canBeHit == null)
                return;

            if (!ProjectileData.CanHitAsteroids && canBeHit is Asteroid)
                return;

            if (canBeHit.TryHitAt(transform.position, _damageAmount))
                Recycler.Recycle<Projectile>(this);
        }

        //====================================================================================================================//

        public void FlipSpriteX(bool state)
        {
            renderer.flipY = state;
        }

        public void FlipSpriteY(bool state)
        {
            renderer.flipY = state;
        }

        //============================================================================================================//

        public void CustomRecycle(params object[] args)
        {
            transform.rotation = Quaternion.identity;
            _target = null;
            _hasRange = false;
            _lifeTime = 0f;

            renderer.flipX = renderer.flipY = false;
        }
    }
}
using UnityEngine;
using StarSalvager.Factories.Data;
using System;
using Recycling;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Projectiles;
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

        private TrailRenderer _trailRenderer;

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
            Vector2 direction, 
            Vector2 velocity)
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

            if (profileData.UseTrail)
            {
                CreateTrailEffect(ProjectileData.Color);
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

            switch (ProjectileData.FireType)
            {
                //----------------------------------------------------------------------------------------------------//

                case FIRE_TYPE.FORWARD:
                case FIRE_TYPE.RANDOM_SPRAY:
                case FIRE_TYPE.SPIRAL:
                case FIRE_TYPE.FIXED_SPRAY:
                    newPosition +=
                        (EnemyVelocityModifier + TravelDirectionNormalized * ProjectileData.ProjectileSpeed) *
                        Time.deltaTime;
                    break;

                //----------------------------------------------------------------------------------------------------//

                case FIRE_TYPE.HEAT_SEEKING:

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
                    throw new ArgumentOutOfRangeException(nameof(ProjectileData.FireType), ProjectileData.FireType,
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


        //====================================================================================================================//

        private void CreateTrailEffect(Color color)
        {
            var endColor = color;
            endColor.a = 0f;
                
            if (!_trailRenderer)
            {
                _trailRenderer = FactoryManager.Instance
                    .GetFactory<EffectFactory>()
                    .CreateEffect(EffectFactory.EFFECT.TRAIL)
                    .GetComponent<TrailRenderer>();
                
                _trailRenderer.transform.SetParent(transform, false);
            }

            _trailRenderer.startColor = color;
            _trailRenderer.endColor = endColor;
            _trailRenderer.widthMultiplier = ((BoxCollider2D) collider).size.x / 2f;

            _trailRenderer.emitting = true;
        }

        //============================================================================================================//

        public void CustomRecycle(params object[] args)
        {
            transform.rotation = Quaternion.identity;
            _target = null;
            _hasRange = false;
            _lifeTime = 0f;

            renderer.flipX = renderer.flipY = false;

            if (_trailRenderer)
                _trailRenderer.emitting = false;

        }
    }
}
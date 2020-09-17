using UnityEngine;
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
        [NonSerialized]
        private Vector3 _mTravelDirectionNormalized = Vector3.zero;
        [NonSerialized]
        private Vector3 _mEnemyVelocityModifier = Vector3.zero;
        [NonSerialized]
        public ProjectileProfileData MProjectileData;

        private float _damageAmount;
        
        //============================================================================================================//

        // Update is called once per frame
        private void Update()
        {
            if(GameTimer.IsPaused)
                return;

            if (!CameraController.IsPointInCameraRect(transform.position))
            {
                Recycler.Recycle<Projectile>(this);
                return;
            }

            transform.position += (_mEnemyVelocityModifier + _mTravelDirectionNormalized * MProjectileData.ProjectileSpeed) * Time.deltaTime;
        }
        
        //============================================================================================================//
        
        public void Init(string collisionTag, float damage, Vector2 direction, Vector2 velocity)
        {
            CollisionTag = collisionTag;
            _damageAmount = damage;
            
            _mTravelDirectionNormalized = direction;
            _mEnemyVelocityModifier = velocity;

            transform.up = direction;
        }
        
        //============================================================================================================//

        protected override void OnCollide(GameObject gameObject, Vector2 hitPoint)
        {
            var canBeHit = gameObject.GetComponent<ICanBeHit>();

            if (canBeHit == null)
                return;

            if (!MProjectileData.CanHitAsteroids && canBeHit is Asteroid)
                return;
                
            if(canBeHit.TryHitAt(transform.position, _damageAmount))
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
            renderer.flipX = renderer.flipY = false;
        }
    }
}
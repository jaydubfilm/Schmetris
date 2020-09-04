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
        public Vector3 m_travelDirectionNormalized = Vector3.zero;
        public Vector3 m_enemyVelocityModifier = Vector3.zero;
        [NonSerialized]
        public ProjectileProfileData m_projectileData;

        public float DamageAmount { get; private set; }
        
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

            transform.position += (m_enemyVelocityModifier + m_travelDirectionNormalized * m_projectileData.ProjectileSpeed) * Time.deltaTime;
        }
        
        //============================================================================================================//

        public void SetDamage(float damage)
        {
            DamageAmount = damage;
        }
        
        //============================================================================================================//


        public void SetCollisionTag(string collisionTag)
        {
            CollisionTag = collisionTag;
        }

        protected override void OnCollide(GameObject gameObject, Vector2 hitPoint)
        {
            if (gameObject.GetComponent<ICanBeHit>() is ICanBeHit iCanBeHit)
            {
                iCanBeHit.TryHitAt(transform.position, DamageAmount);
            }

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
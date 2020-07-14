using UnityEngine;
using StarSalvager.Factories.Data;
using System;
using Recycling;

namespace StarSalvager.AI
{
    //TODO: Handle proper setting of the collision tag
    public class Projectile : CollidableBase
    {
        [NonSerialized]
        public Vector3 m_travelDirectionNormalized = Vector3.zero;
        public Vector3 m_enemyVelocityModifier = Vector3.zero;
        [NonSerialized]
        public ProjectileProfileData m_projectileData;

        public float DamageAmount;

        private void Start()
        {
            SetSprite(m_projectileData.Sprite);
        }

        // Update is called once per frame
        private void Update()
        {
            transform.position += (m_enemyVelocityModifier + m_travelDirectionNormalized * m_projectileData.ProjectileSpeed) * Time.deltaTime;
        }

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
    }
}
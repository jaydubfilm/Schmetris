using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.Factories.Data;
using System;
using Recycling;
using System.Runtime.CompilerServices;

namespace StarSalvager.AI
{
    //TODO: Handle proper setting of the collisiontag
    public class Projectile : CollidableBase
    {
        [NonSerialized]
        public Vector3 m_travelDirectionNormalized = Vector3.zero;
        public Vector3 m_enemyVelocityModifier = Vector3.zero;
        [NonSerialized]
        public ProjectileProfileData m_projectileData;

        protected override string CollisionTag => "Player";

        private void Start()
        {
            SetSprite(m_projectileData.Sprite);
        }

        // Update is called once per frame
        private void Update()
        {
            transform.position += (m_enemyVelocityModifier + (m_travelDirectionNormalized * m_projectileData.ProjectileSpeed)) * Time.deltaTime;
        }

        protected override void OnCollide(GameObject gameObject)
        {
            Recycler.Recycle(typeof(Projectile), this.gameObject);
        }
    }
}
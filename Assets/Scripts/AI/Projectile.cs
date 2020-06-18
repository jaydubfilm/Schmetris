using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.Factories.Data;
using System;

namespace StarSalvager
{
    public class Projectile : CollidableBase
    {
        [NonSerialized]
        public Vector3 m_travelDirectionNormalized = Vector3.zero;
        [NonSerialized]
        public ProjectileProfileData m_projectileData;

        public string m_collisionTag;
        protected virtual string CollisionTag => m_collisionTag;

        private void Start()
        {
            renderer.sprite = m_projectileData.Sprite;
        }

        // Update is called once per frame
        private void Update()
        {
            transform.position += m_travelDirectionNormalized * m_projectileData.ProjectileSpeed * Time.deltaTime;
        }

        protected override void OnCollide(GameObject gameObject)
        {

        }
    }
}
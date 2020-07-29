﻿using System;
using Recycling;
using StarSalvager.AI;
using UnityEngine;

namespace StarSalvager.Utilities.Enemies
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class EnemyDecoy : MonoBehaviour, IRecycled, ICustomRecycle, ICanBeHit
    {
        public bool IsRecycled { get; set; }

        private new BoxCollider2D collider => _collider ? _collider : _collider = GetComponent<BoxCollider2D>();
        private BoxCollider2D _collider;

        private Enemy _enemy;
        private Collider2D _ignoredCollider;

        private new Transform transform;

        private bool _ready;

        //============================================================================================================//

        private void Awake()
        {
            collider.enabled = false;
        }

        // Start is called before the first frame update
        private void Start()
        {
            transform = gameObject.transform;
        }
        
        private void Update()
        {
            if (!_ready)
                return;

            if (_enemy.IsRecycled)
            {
                Recycler.Recycle<EnemyDecoy>(this);
                return;
            }
            
            transform.position = _enemy.transform.position;
        }

        //============================================================================================================//

        public void Setup(Enemy enemy, Collider2D colliderToIgnore)
        {
            _ready = true;
            
            _enemy = enemy;
            _ignoredCollider = colliderToIgnore;


            //Set the size of this collider based on the collider that the enemy is using
            switch (enemy.collider)
            {
                case BoxCollider2D box:
                    collider.size = box.size;
                    collider.offset = box.offset;
                    break;
                case CircleCollider2D circle:
                    collider.size = Vector2.one * (circle.radius * 2f);
                    collider.offset = circle.offset;
                    break;
            }
            
            collider.enabled = true;

            if (_ignoredCollider == null)
                return;
            
            Physics2D.IgnoreCollision(collider, _ignoredCollider, true);
            
        }

        public void Disable()
        {
            _ready = false;
            collider.enabled = false;
        }


        
        //ICanBeHit functions
        //============================================================================================================//
        
        public void TryHitAt(Vector2 position, float damage)
        {
            if (_enemy == null)
                return;
            
            _enemy.TryHitAt(position, damage);
        }
        

        //ICustomRecycle Functions
        //============================================================================================================//
        public void CustomRecycle(params object[] args)
        {
            _ready = false;
            _enemy = null;
            collider.enabled = false;

            
            if (_ignoredCollider is null)
                return;

            Physics2D.IgnoreCollision(collider, _ignoredCollider, false);
            _ignoredCollider = null;
        }

        
    }

}


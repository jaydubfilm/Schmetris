using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    /// <summary>
    /// Any object that can touch a bot should use this base class
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class CollidableBase : MonoBehaviour
    {
        //============================================================================================================//
        
        protected new BoxCollider2D collider
        {
            get
            {
                if (_collider == null)
                    _collider = gameObject.GetComponent<BoxCollider2D>();

                return _collider;
            }
        }
        private BoxCollider2D _collider;

        protected new SpriteRenderer renderer
        {
            get
            {
                if (_renderer == null)
                    _renderer = gameObject.GetComponent<SpriteRenderer>();

                return _renderer;
            }
        }
        private SpriteRenderer _renderer;
        
        //============================================================================================================//

        private void OnCollisionEnter2D(Collision2D other)
        {
            Debug.Log($"{gameObject.name} Collided with {other.gameObject.name}");
        }
        
        //============================================================================================================//

        public void SetSprite(Sprite sprite)
        {
            renderer.sprite = sprite;
        }
        
        //============================================================================================================//

        /// <summary>
        /// Called when the object contacts a bot
        /// </summary>
        protected abstract void OnCollide();
        
        //============================================================================================================//
    }
}
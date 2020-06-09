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

        private void OnCollisionEnter2D(Collision2D other)
        {
            Debug.Log($"{gameObject.name} Collided with {other.gameObject.name}");
        }

        /// <summary>
        /// Called when the object contacts a bot
        /// </summary>
        protected abstract void OnCollide();
    }
}
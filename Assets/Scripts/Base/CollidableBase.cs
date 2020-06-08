using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    /// <summary>
    /// Any object that can touch a bot should use this base class
    /// </summary>
    public abstract class CollidableBase : MonoBehaviour
    {
        /// <summary>
        /// Called when the object contacts a bot
        /// </summary>
        protected abstract void OnCollide();
    }
}
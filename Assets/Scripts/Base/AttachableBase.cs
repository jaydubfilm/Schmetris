using UnityEngine;

namespace StarSalvager
{
    /// <summary>
    /// Any object which can be connected together should derive from attachable
    /// </summary>
    public abstract class AttachableBase : CollidableBase, IHealth
    {
        /// <summary>
        /// Offset from center block
        /// </summary>
        public Vector2Int Coordinate;


        public float StartingHealth => _startingHealth;
        private float _startingHealth;
        public float CurrentHealth => _currentHealth;
        private float _currentHealth;
        
        public void ChangeHealth(float amount)
        {
            _currentHealth += amount;
        }
    }
}
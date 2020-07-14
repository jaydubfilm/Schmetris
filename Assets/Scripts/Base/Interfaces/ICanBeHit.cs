using UnityEngine;

namespace StarSalvager
{
    public interface ICanBeHit
    {
        void TryHitAt(Vector2 position, float damage);
    }
}



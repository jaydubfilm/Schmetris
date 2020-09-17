using UnityEngine;

namespace StarSalvager
{
    public interface ICanBeHit
    {
        bool TryHitAt(Vector2 position, float damage);
    }
}



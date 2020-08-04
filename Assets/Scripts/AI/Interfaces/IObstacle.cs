using UnityEngine;

namespace StarSalvager
{
    public interface IObstacle
    {
        Transform transform { get; }
        GameObject gameObject { get; }
        bool CanMove { get; }

        void SetColliderActive(bool active);
    }
}

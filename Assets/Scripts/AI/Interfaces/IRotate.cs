using UnityEngine;

namespace StarSalvager
{
    public interface IRotate
    {
        Transform transform { get; }
        GameObject gameObject { get; }
        bool Rotating { get; }

        void SetRotating(bool isRotating);
    }
}

using UnityEngine;

namespace StarSalvager
{
    public interface IRotate
    {
        Transform transform { get; }
        GameObject gameObject { get; }
        bool Rotating { get; }
        int RotateDirection { get; }

        void SetRotating(bool isRotating);
    }
}

using StarSalvager.Cameras.Data;
using UnityEngine;

namespace StarSalvager.Utilities.Backgrounds
{
    public interface IBackground
    {
        float zDepth { get;}
        bool parentIsCamera { get;}

        GameObject gameObject { get; }

        void Init(Transform cameraTransform, float zDepth);
        void UpdatePosition(float moveAmount, bool ignoreInput = false);

        void SetOrientation(ORIENTATION newOrientation);
    }
}


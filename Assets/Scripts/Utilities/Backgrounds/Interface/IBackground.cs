using System.Collections;
using System.Collections.Generic;
using StarSalvager.Cameras.Data;
using UnityEngine;

namespace StarSalvager.Utilities.Backgrounds
{
    public interface IBackground
    {
        float zDepth { get;}
        bool parentIsCamera { get;}
        bool ignoreOrientationChanges { get;}

        void Init(Transform cameraTransform);
        void UpdatePosition();

        void SetOrientation(ORIENTATION newOrientation);
    }
}


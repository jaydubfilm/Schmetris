﻿using UnityEngine;

namespace StarSalvager
{
    public interface IObstacle
    {
        Transform transform { get; }
        GameObject gameObject { get; }
        bool CanMove { get; }

        bool IsRegistered { get; set; }
        bool IsMarkedOnGrid { get; set; }

        void SetColliderActive(bool active);
    }
}

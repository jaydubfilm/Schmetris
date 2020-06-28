using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public interface IMovable
    {
        Transform transform { get; }
        GameObject gameObject { get; }
        bool enabled { get; }
        bool ShouldMoveByObstacleManager();
    }
}

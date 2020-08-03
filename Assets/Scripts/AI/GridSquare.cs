using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class GridSquare
    {
        public bool ObstacleInSquare { get; private set; } = false;

        public void SetObstacleInSquare (bool occupied)
        {
            ObstacleInSquare = occupied;
        }
    }
}
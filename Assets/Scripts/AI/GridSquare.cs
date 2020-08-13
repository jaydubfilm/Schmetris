using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class GridSquare
    {
        public bool ObstacleInSquare { get; private set; } = false;

        public int RadiusMarkAround { get; private set; } = 0;

        public void SetObstacleInSquare (bool occupied)
        {
            ObstacleInSquare = occupied;
        }

        public void SetRadiusMarkAround (int radiusAround)
        {
            RadiusMarkAround = radiusAround;
        }
    }
}
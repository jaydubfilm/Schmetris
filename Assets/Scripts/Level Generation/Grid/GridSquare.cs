using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class GridSquare
    {
        public bool ObstacleInSquare { get; set; } = false;

        public int RadiusMarkAround { get; set; } = 0;

        /*public void SetObstacleInSquare (bool occupied)
        {
            ObstacleInSquare = occupied;
        }

        public void SetRadiusMarkAround (int radiusAround)
        {
            RadiusMarkAround = radiusAround;
        }*/
    }
}
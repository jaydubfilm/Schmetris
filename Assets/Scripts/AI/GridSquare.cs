using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class GridSquare
    {
        //Note - list isn't necessary, things move down at constant rate. Make single obstacle reference and design better
        public bool m_obstacleInSquare { get; private set; } = false;

        public void SetObstacleInSquare (bool occupied)
        {
            m_obstacleInSquare = occupied;
        }
    }
}
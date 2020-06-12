using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class GridSquare
    {
        //Note - list isn't necessary, things move down at constant rate. Make single obstacle reference and design better
        public List<GameObject> m_obstaclesInSquare { get; private set; } = new List<GameObject>();

        public void AddObstacleToSquare(GameObject obstacle)
        {
            m_obstaclesInSquare.Add(obstacle);
        }

        public void RemoveObstacleFromSquare(GameObject obstacle)
        {
            m_obstaclesInSquare.Remove(obstacle);
        }
    }
}
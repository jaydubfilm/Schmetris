using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class Testing : MonoBehaviour
    {
        private GameObject[] m_obstacles;
        private GameObject[] m_agents;
        private Vector2[] m_agentDestinations;
        private WorldGrid m_grid;
        
        void Start()
        {
            m_grid = new WorldGrid(100, 100, 2);
            m_obstacles = new GameObject[200];
            m_agents = new GameObject[20];
            m_agentDestinations = new Vector2[20];

            for (int i = 0; i < m_obstacles.Length; i++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Cube);
                m_obstacles[i] = sphere;
                m_obstacles[i].transform.position = m_grid.GetRandomGridWorldPosition();
            }

            for (int i = 0; i < m_agents.Length; i++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                m_agents[i] = sphere;
                m_agents[i].transform.position = m_grid.GetRandomGridWorldPosition();
                m_agentDestinations[i] = m_grid.GetRandomGridWorldPosition();
            }
        }

        void Update()
        {
            float velocity = 2.0f;
            
            for (int i = 0; i < m_agents.Length; i++)
            {
                Vector3 destination = m_agentDestinations[i];
                Vector2 direction = new Vector2(destination.x - m_agents[i].transform.position.x, destination.y - m_agents[i].transform.position.y);
                direction.Normalize();
                direction += calculateForceAtPoint(m_agents[i].transform.position, m_obstacles);
                direction.Normalize();
                direction *= velocity;
                Vector3 vec3Direction = direction;
                m_agents[i].transform.position = m_agents[i].transform.position + (vec3Direction * Time.deltaTime);
            }
        }

        private Vector2 calculateForceAtPoint(Vector2 agentPosition, GameObject[] obstacles)
        {
            Vector2 force = new Vector2(0, 0);
            foreach (GameObject obstacle in obstacles)
            {
                Vector2 obstacleForce = getForce(agentPosition, obstacle.transform.position);
                force.x += obstacleForce.x;
                force.y += obstacleForce.y;
            }
            return force;
        }

        private Vector2 getForce(Vector2 agentPosition, Vector2 obstaclePosition)
        {
            if (Mathf.Abs(agentPosition.x - obstaclePosition.x) + Mathf.Abs(agentPosition.y - obstaclePosition.y) > 20)
            {
                return new Vector2(0, 0);
            }

            float magnitude = 5.0f / Vector2.SqrMagnitude(obstaclePosition - agentPosition);
            Vector2 direction = new Vector2(agentPosition.x - obstaclePosition.x, agentPosition.y - obstaclePosition.y);
            direction.Normalize();
            direction *= magnitude;
            return direction;
        }
    }
}
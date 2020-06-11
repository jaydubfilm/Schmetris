using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class Testing : MonoBehaviour
    {
        public GameObject[] m_obstacles;
        public GameObject[] m_agents;
        
        void Start()
        {
            m_obstacles = new GameObject[5];

            for (int i = 0; i < m_obstacles.Length; i++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                m_obstacles[i] = sphere;
            }

            m_obstacles[0].transform.position = new Vector2(0, 0);
            m_obstacles[1].transform.position = new Vector2(-2, -1);
            m_obstacles[2].transform.position = new Vector2(4, 1);
            m_obstacles[3].transform.position = new Vector2(-1, -4);
            m_obstacles[4].transform.position = new Vector2(2, 3);

            m_agents = new GameObject[1];

            for (int i = 0; i < m_agents.Length; i++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                m_agents[i] = sphere;
            }

            m_agents[0].transform.position = new Vector2(-10, 0);
        }

        void Update()
        {
            Vector3 destination = new Vector3(10, 0);
            float velocity = 1.25f;
            
            for (int i = 0; i < m_agents.Length; i++)
            {
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
            /*if (Mathf.Abs(agentPosition.x - obstaclePosition.x) + Mathf.Abs(agentPosition.y - obstaclePosition.y) > 4)
            {
                return new Vector2(0, 0);
            }*/

            float magnitude = 1 / Vector2.SqrMagnitude(obstaclePosition - agentPosition);
            Vector2 direction = new Vector2(agentPosition.x - obstaclePosition.x, agentPosition.y - obstaclePosition.y);
            direction.Normalize();
            direction *= magnitude;
            return direction;
        }
    }
}
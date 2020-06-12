using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace StarSalvager
{
    public class AIObstacleAvoidance : MonoBehaviour
    {
        public AIAgentTest m_AIAgentTestPrefab;

        private GameObject[] m_obstacles;
        private AIAgentTest[] m_agents;
        private WorldGrid m_grid;

        private const float m_agentVelocity = 2.0f;
        private const int m_agentGridScanRadius = 3;
        private const float m_obstacleMass = 3.5f;
        
        void Start()
        {
            m_grid = new WorldGrid(100, 20, 2);
            m_obstacles = new GameObject[200];
            m_agents = new AIAgentTest[40];

            //Temporary for testing - instantiate large numbers of test agents and obstacles. In the future, this w
            for (int i = 0; i < m_obstacles.Length; i++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Cube);
                m_obstacles[i] = sphere;
                m_obstacles[i].transform.position = m_grid.GetRandomGridWorldPosition();
                m_grid.AddObstacleToGridSquare(m_obstacles[i]);
            }

            for (int i = 0; i < m_agents.Length; i++)
            {
                AIAgentTest newAgent = GameObject.Instantiate(m_AIAgentTestPrefab);
                m_agents[i] = newAgent;
                m_agents[i].transform.position = m_grid.GetRandomGridWorldPosition();
                m_agents[i].m_agentDestination = m_grid.GetRandomGridWorldPosition();
            }
        }

        void Update()
        {
            //Iterate through all agents, and for each one, add the forces from nearby obstacles to their current direction vector
            //After adding the forces, normalize and multiply by the velocity to ensure consistent speed
            for (int i = 0; i < m_agents.Length; i++)
            {
                Vector3 destination = m_agents[i].m_agentDestination;
                Vector2 direction = new Vector2(destination.x - m_agents[i].transform.position.x, destination.y - m_agents[i].transform.position.y);
                direction.Normalize();
                direction *= m_agentVelocity;

                direction += calculateForceAtPoint(m_agents[i].transform.position);
                direction.Normalize();
                direction *= m_agentVelocity;
                Vector3 vec3Direction = direction;
                m_agents[i].transform.position = m_agents[i].transform.position + (vec3Direction * Time.deltaTime);
            }
        }

        //Check all nearby squares to the agent to see if any contain an obstacle. For any obstacles in those squares, add the force they apply on the agent.
        private Vector2 calculateForceAtPoint(Vector2 agentPosition)
        {
            Vector2 force = new Vector2(0, 0);
            Vector2Int agentGridPosition = m_grid.GetGridPositionOfVector(agentPosition);
            Vector2Int agentGridScanMinimum = new Vector2Int (
                Math.Max(0, agentGridPosition.x - m_agentGridScanRadius), 
                Math.Max(0, agentGridPosition.y - m_agentGridScanRadius));
            Vector2Int agentGridScanMaximum = new Vector2Int (
                Math.Min(m_grid.m_width - 1, agentGridPosition.x + m_agentGridScanRadius), 
                Math.Min(m_grid.m_height - 1, agentGridPosition.y + m_agentGridScanRadius));

            for (int i = agentGridScanMinimum.x; i <= agentGridScanMaximum.x; i++)
            {
                for (int k = agentGridScanMinimum.y; k <= agentGridScanMaximum.y; k++)
                {
                    foreach (GameObject obstacle in m_grid.GetGridSquareAtPosition(i, k).m_obstaclesInSquare)
                    {
                        Vector2 obstacleForce = getForce(agentPosition, obstacle.transform.position);
                        force.x += obstacleForce.x;
                        force.y += obstacleForce.y;
                    }
                }
            }

            return force;
        }

        //Create a "reverse gravity" force for the agent from the obstacle, using a mass value and the distance between them
        private Vector2 getForce(Vector2 agentPosition, Vector2 obstaclePosition)
        {
            float magnitude = m_obstacleMass / Vector2.SqrMagnitude(obstaclePosition - agentPosition);
            Vector2 direction = new Vector2(agentPosition.x - obstaclePosition.x, agentPosition.y - obstaclePosition.y);
            direction.Normalize();
            direction *= magnitude;
            return direction;
        }
    }
}
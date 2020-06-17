using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;
using StarSalvager.Factories;

namespace StarSalvager
{
    public class AIObstacleAvoidance : MonoBehaviour
    {
        public AIObstacleTest m_AIObstacleTestPrefab;
        public Enemy m_EnemyPrefab;

        private AIObstacleTest[] m_obstacles;
        private Enemy[] m_enemies;
        private WorldGrid m_grid;

        private const float m_gridCellSize = 2.0f;
        private const int m_agentGridScanRadius = 3;
        private const float m_obstacleMass = 6.0f;

        //Temporary variables, simulating the movement speed of falling obstacles
        private const float m_timeToMoveBetweenCells = 3.0f;
        private float m_timer = 1.5f;
        private Vector2 m_obstaclePositionAdjuster = new Vector2(0.0f, m_gridCellSize);

        //Variables used for job scheduling system
        PositionUpdateJob m_positionUpdateJob;

        TransformAccessArray m_obstacleTransformAccessArray;


        void Start()
        {
            m_grid = new WorldGrid(50, 50, m_gridCellSize);
            m_obstacles = new AIObstacleTest[50];
            m_enemies = new Enemy[1];

            Transform[] transformArray = new Transform[m_obstacles.Length];

            //Temporary for testing - instantiate large numbers of test agents and obstacles. In the future, this w
            for (int i = 0; i < m_obstacles.Length; i++)
            {
                AIObstacleTest newObstacle = GameObject.Instantiate(m_AIObstacleTestPrefab);
                m_obstacles[i] = newObstacle;
                Vector2 position = m_grid.GetRandomGridSquareWorldPosition();
                m_obstacles[i].transform.position = position;
                transformArray[i] = m_obstacles[i].transform;
                m_grid.SetObstacleInGridSquare(position, true);
            }

            for (int i = 0; i < m_enemies.Length; i++)
            {
                Enemy newEnemy = FactoryManager.Instance.GetFactory<EnemyFactory>().CreateObject<Enemy>(ENEMY_TYPE.Enemy3);
                m_enemies[i] = newEnemy;
                m_enemies[i].transform.position = m_grid.GetRandomGridSquareWorldPosition();
                m_enemies[i].m_destination = m_grid.GetRandomGridSquareWorldPosition();
            }

            m_obstacleTransformAccessArray = new TransformAccessArray(transformArray);
            //End Temporary code
        }

        struct PositionUpdateJob : IJobParallelForTransform
        {
            public Vector3 distanceToMove;

            public void Execute(int i, TransformAccess transform)
            {
                transform.position -= distanceToMove;
            }
        }

        void Update()
        {
            //Temporary code to simulate the speed of downward movement for obstacles and move the prefabs on screen downward
            m_timer += Time.deltaTime;
            if (m_timer >= m_timeToMoveBetweenCells)
            {
                m_timer -= m_timeToMoveBetweenCells;
                m_grid.MoveObstacleMarkersDownwardOnGrid();
            }

            Vector3 amountShiftDown = new Vector3(0, (m_gridCellSize * Time.deltaTime) / m_timeToMoveBetweenCells, 0);
            m_positionUpdateJob = new PositionUpdateJob()
            {
                distanceToMove = amountShiftDown,
            };

            m_positionUpdateJob.Schedule(m_obstacleTransformAccessArray);

            //Iterate through all agents, and for each one, add the forces from nearby obstacles to their current direction vector
            //After adding the forces, normalize and multiply by the velocity to ensure consistent speed
            for (int i = 0; i < m_enemies.Length; i++)
            {
                Vector3 position = m_enemies[i].transform.position;
                Vector3 destination = m_enemies[i].GetDestination();

                Vector2 direction = new Vector2(destination.x - position.x, destination.y - position.y);
                direction.Normalize();

                if (!position.Equals(destination))
                {
                    direction += CalculateForceAtPoint(position);
                    direction.Normalize();
                }

                m_enemies[i].ProcessMovement(direction);
            }
        }

        //Check all nearby squares to the agent to see if any contain an obstacle. For any obstacles in those squares, add the force they apply on the agent.
        private Vector2 CalculateForceAtPoint(Vector2 agentPosition)
        {
            //Calculate the min and max grid positions of an m_agentGridScanRadius large box around the agent
            Vector2 force = new Vector2(0, 0);
            Vector2Int agentGridPosition = m_grid.GetGridPositionOfVector(agentPosition);
            Vector2Int agentGridScanMinimum = new Vector2Int (
                Math.Max(0, agentGridPosition.x - m_agentGridScanRadius), 
                Math.Max(0, agentGridPosition.y - m_agentGridScanRadius));
            Vector2Int agentGridScanMaximum = new Vector2Int (
                Math.Min(m_grid.m_width - 1, agentGridPosition.x + m_agentGridScanRadius), 
                Math.Min(m_grid.m_height - 1, agentGridPosition.y + m_agentGridScanRadius));

            //Check each position in the box for a marker for containing an obstacle
            for (int i = agentGridScanMinimum.x; i <= agentGridScanMaximum.x; i++)
            {
                for (int k = agentGridScanMinimum.y; k <= agentGridScanMaximum.y; k++)
                {
                    if (m_grid.GetGridSquareAtPosition(i, k).m_obstacleInSquare)
                    {
                        Vector2 obstacleForce = GetForce(agentPosition, CalculateObstaclePositionChange(i, k));
                        force.x += obstacleForce.x;
                        force.y += obstacleForce.y;
                    }
                }
            }

            return force;
        }

        //Returns the position of the obstacle at this location in the grid, by getting the grid center position and
        //infering where it is in relation to that based on the timer and the obstacles movement speed
        private Vector2 CalculateObstaclePositionChange(int x, int y)
        {
            return m_grid.GetCenterOfGridSquareInGridPosition(x, y) - m_obstaclePositionAdjuster * ((m_timer / m_timeToMoveBetweenCells) - 0.5f);
        }

        //Create a "reverse gravity" force for the agent from the obstacle, using a mass value and the distance between them
        private Vector2 GetForce(Vector2 agentPosition, Vector2 obstaclePosition)
        {
            float magnitude = m_obstacleMass / Vector2.SqrMagnitude(obstaclePosition - agentPosition);
            Vector2 direction = new Vector2(agentPosition.x - obstaclePosition.x, agentPosition.y - obstaclePosition.y);
            direction.Normalize();
            direction *= magnitude;
            return direction;
        }

        private void OnDestroy()
        {
            m_obstacleTransformAccessArray.Dispose();
        }
    }
}
 
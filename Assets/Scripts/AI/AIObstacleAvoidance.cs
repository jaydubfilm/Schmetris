using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;
using StarSalvager.Factories;
using StarSalvager.Constants;
using UnityEngine.UI;

namespace StarSalvager.AI
{
    public class AIObstacleAvoidance : MonoBehaviour
    {
        //Temporary variables, simulating the movement speed of falling obstacles
        private float m_timer = Values.timeForAsteroidsToFall / 2;
        private Vector2 m_obstaclePositionAdjuster = new Vector2(0.0f, Values.gridCellSize);

        void Start()
        {

        }

        void Update()
        {
            //Temporary code to simulate the speed of downward movement for obstacles and move the prefabs on screen downward
            m_timer += Time.deltaTime;
            if (m_timer >= Values.timeForAsteroidsToFall)
            {
                m_timer -= Values.timeForAsteroidsToFall;
                //TODO: move these lines to a more appropriate location. Also, ensure that this order of operations stays the same, it is important.
                LevelManager.Instance.WorldGrid.MoveObstacleMarkersDownwardOnGrid();
                LevelManager.Instance.ObstacleManager.SpawnNewRowOfObstacles();
                LevelManager.Instance.ObstacleManager.TryMarkNewShapesOnGrid();
            }
        }

        //Check all nearby squares to the agent to see if any contain an obstacle. For any obstacles in those squares, add the force they apply on the agent.
        public Vector2 CalculateForceAtPoint(Vector2 agentPosition)
        {
            //Calculate the min and max grid positions of a Values.enemyGridScanRadius large box around the agent
            Vector2 force = new Vector2(0, 0);
            Vector2Int agentGridPosition = LevelManager.Instance.WorldGrid.GetGridPositionOfVector(agentPosition);
            Vector2Int agentGridScanMinimum = new Vector2Int (
                Math.Max(0, agentGridPosition.x - Values.enemyGridScanRadius), 
                Math.Max(0, agentGridPosition.y - Values.enemyGridScanRadius));
            Vector2Int agentGridScanMaximum = new Vector2Int (
                Math.Min(Values.gridSizeX - 1, agentGridPosition.x + Values.enemyGridScanRadius), 
                Math.Min(Values.gridSizeY - 1, agentGridPosition.y + Values.enemyGridScanRadius));

            //Check each position in the box for a marker for containing an obstacle
            for (int i = agentGridScanMinimum.x; i <= agentGridScanMaximum.x; i++)
            {
                for (int k = agentGridScanMinimum.y; k <= agentGridScanMaximum.y; k++)
                {
                    if (LevelManager.Instance.WorldGrid.GetGridSquareAtPosition(i, k).m_obstacleInSquare)
                    {
                        Vector2 obstacleForce = GetForce(agentPosition, CalculateObstaclePositionChange(i, k));
                        force.x += obstacleForce.x;
                        force.y += obstacleForce.y;
                    }
                }
            }

            return force;
        }

        //Create a "reverse gravity" force for the agent from the obstacle, using a mass value and the distance between them
        private Vector2 GetForce(Vector2 agentPosition, Vector2 obstaclePosition)
        {
            float magnitude = Values.obstacleMass / Vector2.SqrMagnitude(obstaclePosition - agentPosition);
            Vector2 direction = new Vector2(agentPosition.x - obstaclePosition.x, agentPosition.y - obstaclePosition.y);
            direction.Normalize();
            direction *= magnitude;
            return direction;
        }

        //Returns the position of the obstacle at this location in the grid, by getting the grid center position and
        //infering where it is in relation to that based on the timer and the obstacles movement speed
        private Vector2 CalculateObstaclePositionChange(int x, int y)
        {
            return LevelManager.Instance.WorldGrid.GetCenterOfGridSquareInGridPosition(x, y) - m_obstaclePositionAdjuster * ((m_timer / Values.timeForAsteroidsToFall) - 0.5f);
        }
    }
}
 
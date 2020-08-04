using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;
using StarSalvager.Factories;
using StarSalvager.Values;
using UnityEngine.UI;

namespace StarSalvager.AI
{
    public class AIObstacleAvoidance : MonoBehaviour
    {
        //Temporary variables, simulating the movement speed of falling obstacles
        private Vector2 m_obstaclePositionAdjuster = new Vector2(0.0f, Constants.gridCellSize);

        //Check all nearby squares to the agent to see if any contain an obstacle. For any obstacles in those squares, add the force they apply on the agent.
        public Vector2 CalculateForceAtPoint(Vector2 agentPosition, bool isAttachable)
        {
            //Calculate the min and max grid positions of a Values.enemyGridScanRadius large box around the agent
            Vector2 force = new Vector2(0, 0);
            Vector2Int agentGridPosition = LevelManager.Instance.WorldGrid.GetGridPositionOfVector(agentPosition);
            Vector2Int agentGridScanMinimum = new Vector2Int (
                Math.Max(0, agentGridPosition.x - Constants.enemyGridScanRadius), 
                Math.Max(0, agentGridPosition.y - Constants.enemyGridScanRadius));
            Vector2Int agentGridScanMaximum = new Vector2Int (
                Math.Min(Values.Globals.GridSizeX - 1, agentGridPosition.x + Constants.enemyGridScanRadius), 
                Math.Min(Values.Globals.GridSizeY - 1, agentGridPosition.y + Constants.enemyGridScanRadius));

            //Check each position in the box for a marker for containing an obstacle
            for (int i = agentGridScanMinimum.x; i <= agentGridScanMaximum.x; i++)
            {
                for (int k = agentGridScanMinimum.y; k <= agentGridScanMaximum.y; k++)
                {
                    if (LevelManager.Instance.WorldGrid.GetGridSquareAtPosition(i, k).ObstacleInSquare)
                    {
                        Vector2 obstacleForce = GetForce(agentPosition, CalculateObstaclePositionChange(i, k));
                        force.x += obstacleForce.x;
                        force.y += obstacleForce.y;
                    }
                }
            }

            if (!isAttachable)
            {
                foreach (var attached in LevelManager.Instance.BotGameObject.attachedBlocks)
                {
                    Vector2 obstacleForce = GetForce(agentPosition, attached.transform.position);
                    force.x += obstacleForce.x;
                    force.y += obstacleForce.y;
                }
            }

            return force;
        }

        //Create a "reverse gravity" force for the agent from the obstacle, using a mass value and the distance between them
        private Vector2 GetForce(Vector2 agentPosition, Vector2 obstaclePosition)
        {
            float magnitude = Constants.obstacleMass / Vector2.SqrMagnitude(obstaclePosition - agentPosition);
            Vector2 direction = new Vector2(agentPosition.x - obstaclePosition.x, agentPosition.y - obstaclePosition.y);
            direction.Normalize();
            direction *= magnitude;
            return direction;
        }

        //Returns the position of the obstacle at this location in the grid, by getting the grid center position and
        //infering where it is in relation to that based on the timer and the obstacles movement speed
        //TODO: I don't remember why the -0.5f previously happened in this calculation (at the end, in the bracket with the global/constants). 
        //It doesn't look like it was correct. Do a deep dive on the math to make sure it should be there
        private Vector2 CalculateObstaclePositionChange(int x, int y)
        {
            return LevelManager.Instance.WorldGrid.GetCenterOfGridSquareInGridPosition(x, y) - m_obstaclePositionAdjuster * (Globals.AsteroidFallTimer / Constants.timeForAsteroidsToFall);
        }
    }
}
 
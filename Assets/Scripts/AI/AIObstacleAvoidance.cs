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
        private Bit[] m_bits;
        private Enemy[] m_enemies;

        //Temporary variables, simulating the movement speed of falling obstacles
        private float m_timer = Values.timeForAsteroidsToFall / 2;
        private Vector2 m_obstaclePositionAdjuster = new Vector2(0.0f, Values.gridCellSize);

        //Variables used for job scheduling system
        PositionUpdateJob m_positionUpdateJob;
        TransformAccessArray m_obstacleTransformAccessArray;

        void Start()
        {
            m_bits = new Bit[Values.numberBitsSpawn];
            m_enemies = new Enemy[Values.numberEnemiesSpawn];

            Transform[] transformArray = new Transform[m_bits.Length];

            //Temporary for testing - instantiate large numbers of test agents and obstacles. In the future, this w
            for (int i = 0; i < m_bits.Length; i++)
            {
                Bit newBit = GameObject.Instantiate(LevelManager.Instance.BitTestPrefab);
                m_bits[i] = newBit;
                Vector2 position = LevelManager.Instance.WorldGrid.GetRandomGridSquareWorldPosition();
                m_bits[i].transform.position = position;
                transformArray[i] = m_bits[i].transform;
                LevelManager.Instance.WorldGrid.SetObstacleInGridSquare(position, true);
            }

            for (int i = 0; i < m_enemies.Length; i++)
            {
                Enemy newEnemy = FactoryManager.Instance.GetFactory<EnemyFactory>().CreateObject<Enemy>(ENEMY_TYPE.Enemy1);
                m_enemies[i] = newEnemy;
                m_enemies[i].transform.position = LevelManager.Instance.WorldGrid.GetCenterOfGridSquareInGridPosition(Values.gridSizeX / 2, Values.gridSizeY / 2);
            }

            LevelManager.Instance.DemoText.text = "R to swap demo enemies, T to reset demo enemy position. WASD or arrow keys to move bot." +
                "\nEnemyType: " + m_enemies[0].m_enemyData.EnemyType +
                "\nMovementType: " + m_enemies[0].m_enemyData.MovementType +
                "\nAttackType: " + m_enemies[0].m_enemyData.AttackType +
                "\nMovementSpeed: " + m_enemies[0].m_enemyData.MovementSpeed +
                "\nAttackSpeed: " + m_enemies[0].m_enemyData.AttackSpeed;

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

        private int tempDemoingVariable = 0;

        void Update()
        {
            if(Input.GetKeyDown("r"))
            {
                tempDemoingVariable++;
                if (tempDemoingVariable == 15)
                {
                    tempDemoingVariable = 0;
                }
                Destroy(m_enemies[0].gameObject);
                Enemy newEnemy = FactoryManager.Instance.GetFactory<EnemyFactory>().CreateObject<Enemy>((ENEMY_TYPE)tempDemoingVariable);
                m_enemies[0] = newEnemy;
                m_enemies[0].transform.position = LevelManager.Instance.WorldGrid.GetCenterOfGridSquareInGridPosition(Values.gridSizeX / 2, Values.gridSizeY / 2);

                LevelManager.Instance.DemoText.text = "R to swap demo enemies, T to reset demo enemy position. WASD or arrow keys to move bot." +
                    "\nEnemyType: " + m_enemies[0].m_enemyData.EnemyType +
                    "\nMovementType: " + m_enemies[0].m_enemyData.MovementType +
                    "\nAttackType: " + m_enemies[0].m_enemyData.AttackType +
                    "\nMovementSpeed: " + m_enemies[0].m_enemyData.MovementSpeed +
                    "\nAttackSpeed: " + m_enemies[0].m_enemyData.AttackSpeed;
            }
            else if (Input.GetKeyDown("t"))
            {
                m_enemies[0].transform.position = LevelManager.Instance.WorldGrid.GetCenterOfGridSquareInGridPosition(Values.gridSizeX / 2, Values.gridSizeY / 2);
            }

            //Temporary code to simulate the speed of downward movement for obstacles and move the prefabs on screen downward
            m_timer += Time.deltaTime;
            if (m_timer >= Values.timeForAsteroidsToFall)
            {
                m_timer -= Values.timeForAsteroidsToFall;
                LevelManager.Instance.WorldGrid.MoveObstacleMarkersDownwardOnGrid();
            }

            Vector3 amountShiftDown = new Vector3(0, (Values.gridCellSize * Time.deltaTime) / Values.timeForAsteroidsToFall, 0);
            /*m_positionUpdateJob = new PositionUpdateJob()
            {
                distanceToMove = amountShiftDown,
            };

            m_positionUpdateJob.Schedule(m_obstacleTransformAccessArray);*/

            foreach (Bit bit in m_bits)
            {
                if (bit != null && bit.IsAttached != true)
                {
                    bit.transform.position -= amountShiftDown;
                    if (bit.transform.position.y < 0)
                    {
                        bit.transform.position += Vector3.up * Values.gridSizeY * Values.gridCellSize;
                    }
                }
            }
            //End temporary code

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

        //Returns the position of the obstacle at this location in the grid, by getting the grid center position and
        //infering where it is in relation to that based on the timer and the obstacles movement speed
        private Vector2 CalculateObstaclePositionChange(int x, int y)
        {
            return LevelManager.Instance.WorldGrid.GetCenterOfGridSquareInGridPosition(x, y) - m_obstaclePositionAdjuster * ((m_timer / Values.timeForAsteroidsToFall) - 0.5f);
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

        private void OnDestroy()
        {
            m_obstacleTransformAccessArray.Dispose();
        }
    }
}
 
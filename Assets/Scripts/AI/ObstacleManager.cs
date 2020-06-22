using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.Factories;
using StarSalvager.Constants;
using StarSalvager.AI;
using StarSalvager;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace StarSalvager
{
    public class ObstacleManager : MonoBehaviour
    {
        private Bit[] m_bits;

        //Temporary variables, simulating the movement speed of falling obstacles
        private float m_timer = Values.timeForAsteroidsToFall / 2;
        private Vector2 m_obstaclePositionAdjuster = new Vector2(0.0f, Values.gridCellSize);

        //Variables used for job scheduling system
        PositionUpdateJob m_positionUpdateJob;
        TransformAccessArray m_obstacleTransformAccessArray;

        struct PositionUpdateJob : IJobParallelForTransform
        {
            public Vector3 distanceToMove;

            public void Execute(int i, TransformAccess transform)
            {
                transform.position -= distanceToMove;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            m_bits = new Bit[Values.numberBitsSpawn];
            Transform[] transformArray = new Transform[m_bits.Length];

            for (int i = 0; i < m_bits.Length; i++)
            {
                Bit newBit = GameObject.Instantiate(LevelManager.Instance.BitTestPrefab);
                m_bits[i] = newBit;
                Vector2 position = LevelManager.Instance.WorldGrid.GetRandomGridSquareWorldPosition();
                m_bits[i].transform.position = position;
                transformArray[i] = m_bits[i].transform;
                LevelManager.Instance.WorldGrid.SetObstacleInGridSquare(position, true);
            }

            m_obstacleTransformAccessArray = new TransformAccessArray(transformArray);
        }

        // Update is called once per frame
        void Update()
        {
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
        }

        //Returns the position of the obstacle at this location in the grid, by getting the grid center position and
        //infering where it is in relation to that based on the timer and the obstacles movement speed
        public Vector2 CalculateObstaclePositionChange(int x, int y)
        {
            return LevelManager.Instance.WorldGrid.GetCenterOfGridSquareInGridPosition(x, y) - m_obstaclePositionAdjuster * ((m_timer / Values.timeForAsteroidsToFall) - 0.5f);
        }

        private void OnDestroy()
        {
            m_obstacleTransformAccessArray.Dispose();
        }
    }
}

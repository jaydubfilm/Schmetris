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
        private List<Bit> m_bits;

        private int m_numHorizontalMovements = 0;

        //Variables used for job scheduling system
        /*PositionUpdateJob m_positionUpdateJob;
        TransformAccessArray m_obstacleTransformAccessArray;

        struct PositionUpdateJob : IJobParallelForTransform
        {
            public Vector3 distanceToMove;

            public void Execute(int i, TransformAccess transform)
            {
                transform.position -= distanceToMove;
            }
        }*/

        // Start is called before the first frame update
        void Start()
        {
            m_bits = new List<Bit>();
            //Transform[] transformArray = new Transform[m_bits.Length];

            for (int i = 0; i < Values.numberBitsSpawn; i++)
            {
                Bit newBit = GameObject.Instantiate(LevelManager.Instance.BitTestPrefab);
                m_bits.Add(newBit);
                Vector2 position = LevelManager.Instance.WorldGrid.GetRandomGridSquareWorldPosition();
                newBit.transform.position = position;
                //transformArray[i] = m_bits[i].transform;
                LevelManager.Instance.WorldGrid.SetObstacleInGridSquare(position, true);
            }

            //m_obstacleTransformAccessArray = new TransformAccessArray(transformArray);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown("Y"))
            {
                m_numHorizontalMovements++;
            }
            else if (Input.GetKeyDown("U"))
            {
                m_numHorizontalMovements--;
            }
            
            Vector3 amountShift = Vector3.up * ((Values.gridCellSize * Time.deltaTime) / Values.timeForAsteroidsToFall);

            if (m_numHorizontalMovements != 0)
            {
                amountShift += Vector3.right * m_numHorizontalMovements * Values.gridCellSize;
                m_numHorizontalMovements = 0;
            }

            /*m_positionUpdateJob = new PositionUpdateJob()
            {
                distanceToMove = amountShiftDown,
            };

            m_positionUpdateJob.Schedule(m_obstacleTransformAccessArray);*/

            foreach (Bit bit in m_bits)
            {
                if (bit != null && bit.IsAttached != true)
                {
                    bit.transform.position -= amountShift;
                    if (bit.transform.position.y < 0)
                    {
                        bit.transform.position += Vector3.up * Values.gridSizeY * Values.gridCellSize;
                    }
                }
            }
            //End temporary code
        }

        private void OnDestroy()
        {
            //m_obstacleTransformAccessArray.Dispose();
        }
    }
}

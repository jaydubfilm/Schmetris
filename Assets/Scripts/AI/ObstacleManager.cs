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

        }
    }
}

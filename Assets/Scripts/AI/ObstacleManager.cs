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

        // Start is called before the first frame update
        void Start()
        {
            m_bits = new List<Bit>();

            for (int i = 0; i < Values.numberBitsSpawn; i++)
            {
                Bit newBit = GameObject.Instantiate(LevelManager.Instance.BitTestPrefab);
                m_bits.Add(newBit);
                Vector2 position = LevelManager.Instance.WorldGrid.GetRandomGridSquareWorldPosition();
                newBit.transform.position = position;
                //transformArray[i] = m_bits[i].transform;
                LevelManager.Instance.WorldGrid.SetObstacleInGridSquare(position, true);
            }
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

            for (var i = m_bits.Count - 1; i >= 0; i--)
            {
                var bit = m_bits[i];
                if (bit == null)
                {
                    m_bits.RemoveAt(i);
                    continue;
                }

                if (bit.IsAttached)
                {
                    m_bits.RemoveAt(i);
                    continue;
                }

                var pos = bit.transform.position;
                pos -= amountShift;

                if (pos.y < 0)
                    pos += Vector3.up * (Values.gridSizeY * Values.gridCellSize);

                bit.transform.position = pos;
            }
        }
    }
}

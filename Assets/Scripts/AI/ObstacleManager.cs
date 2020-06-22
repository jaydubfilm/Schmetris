﻿using System.Collections.Generic;
using UnityEngine;
using StarSalvager.Constants;

namespace StarSalvager
{
    public class ObstacleManager : MonoBehaviour
    {
        private List<Bit> m_bits;

        //Input Manager variables - -1.0f for left, 0 for nothing, 1.0f for right
        private float m_currentInput;

        public bool Moving => _moving;
        private bool _moving;

        private float m_distanceHorizontal = 0.0f;

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
            Vector3 amountShift = Vector3.up * ((Values.gridCellSize * Time.deltaTime) / Values.timeForAsteroidsToFall);

            if (m_distanceHorizontal != 0)
            {
                int gridPositionXPrevious = (int)Mathf.Ceil(m_distanceHorizontal + (Values.gridCellSize / 2) / Values.gridCellSize);

                if (m_distanceHorizontal > 0)
                {
                    float toMove = Mathf.Min(m_distanceHorizontal, Values.botHorizontalSpeed * Time.deltaTime);
                    amountShift += Vector3.right * toMove;
                    m_distanceHorizontal -= toMove;
                }
                else if (m_distanceHorizontal < 0)
                {
                    float toMove = Mathf.Min(Mathf.Abs(m_distanceHorizontal), Values.botHorizontalSpeed * Time.deltaTime);
                    amountShift += Vector3.left * toMove;
                    m_distanceHorizontal += toMove;
                }

                int gridPositionXCurrent = (int)Mathf.Ceil(m_distanceHorizontal + (Values.gridCellSize / 2) / Values.gridCellSize);
                if (gridPositionXPrevious > gridPositionXCurrent)
                {
                    LevelManager.Instance.WorldGrid.MoveObstacleMarkersLeftOnGrid(gridPositionXPrevious - gridPositionXCurrent);
                }
                else if (gridPositionXPrevious < gridPositionXCurrent)
                {
                    LevelManager.Instance.WorldGrid.MoveObstacleMarkersRightOnGrid(gridPositionXCurrent - gridPositionXPrevious);
                }
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
                Vector2 gridPosition = LevelManager.Instance.WorldGrid.GetGridPositionOfVector(bit.transform.position);
                pos -= amountShift;

                if (gridPosition.x < 0)
                    pos += Vector3.right * (Values.gridSizeX * Values.gridCellSize);
                else if (gridPosition.x >= Values.gridSizeX)
                    pos += Vector3.left * (Values.gridSizeX * Values.gridCellSize);

                if (gridPosition.y < 0)
                    pos += Vector3.up * (Values.gridSizeY * Values.gridCellSize);
                /*if (gridPosition.y < 0)
                {
                    m_bits.RemoveAt(i);
                    Destroy(bit.gameObject);
                    continue;
                }*/

                bit.transform.position = pos;
            }

            if (m_currentInput != 0.0f && Mathf.Abs(m_distanceHorizontal) <= 0.2f)
            {
                Move(m_currentInput);
            }
        }

        public void Move(float direction)
        {
            if (UnityEngine.Input.GetKey(KeyCode.LeftAlt))
            {
                m_currentInput = 0f;
                return;
            }

            m_currentInput = direction;

            m_distanceHorizontal += direction * Values.gridCellSize;

            _moving = true;
        }
    }
}

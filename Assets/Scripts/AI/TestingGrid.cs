using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class TestingGrid
    {
        private int m_width;
        private int m_height;
        private float m_gridCellSize;
        private int[,] m_gridArray;

        public TestingGrid(int width, int height, float gridCellSize)
        {
            m_width = width;
            m_height = height;
            m_gridCellSize = gridCellSize;

            m_gridArray = new int[m_width, m_height];

            for (int x = 0; x < m_gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < m_gridArray.GetLength(1); y++)
                {
                    //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //sphere.transform.position = 
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), new Color(255, 0, 0), 300f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), new Color(255, 0, 0), 300f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), new Color(255, 0, 0), 300f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), new Color(255, 0, 0), 300f);
        }

        private Vector2 GetWorldPosition (int x, int y)
        {
            return new Vector2(x, y) * m_gridCellSize;
        }

        public Vector2 GetArrayPositionOfVector(Vector2 worldLocation)
        {
            return new Vector2((int) (worldLocation.x / m_gridCellSize), (int) (worldLocation.y / m_gridCellSize));
        }
    }
}
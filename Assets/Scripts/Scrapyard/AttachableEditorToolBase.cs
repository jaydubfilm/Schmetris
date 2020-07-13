using StarSalvager.Cameras;
using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class AttachableEditorToolBase : MonoBehaviour
    {
        public Material material;
        protected List<ScrapyardBot> _scrapyardBots;

        [SerializeField]
        private CameraController m_cameraController;
        public CameraController CameraController => m_cameraController;

        public PART_TYPE? selectedPartType = null;
        public int selectedpartLevel = 0;

        public void DrawGL(Camera camera)
        {
            Vector2 m_anchorPoint = new Vector2(-Values.Constants.gridCellSize * 3.5f, -Values.Constants.gridCellSize * 3.5f);
            //Draw debug lines to show the area of the grid
            for (int x = 0; x < 7; x++)
            {
                for (int y = 0; y < 7; y++)
                {
                    Vector2 tempVector = new Vector2(x, y);

                    DrawWithGL(material, m_anchorPoint + tempVector * Values.Constants.gridCellSize, m_anchorPoint + new Vector2(x, y + 1) * Values.Constants.gridCellSize);
                    DrawWithGL(material, m_anchorPoint + tempVector * Values.Constants.gridCellSize, m_anchorPoint + new Vector2(x + 1, y) * Values.Constants.gridCellSize);
                }
            }
            DrawWithGL(material, m_anchorPoint + new Vector2(0, 7) * Values.Constants.gridCellSize, m_anchorPoint + new Vector2(7, 7) * Values.Constants.gridCellSize);
            DrawWithGL(material, m_anchorPoint + new Vector2(7, 0) * Values.Constants.gridCellSize, m_anchorPoint + new Vector2(7, 7) * Values.Constants.gridCellSize);
        }

        public void DrawWithGL(Material material, Vector2 startPoint, Vector2 endPoint)
        {
            GL.PushMatrix();
            material.SetPass(0);
            GL.Begin(GL.LINES);
            {
                GL.Color(Color.red);

                GL.Vertex(startPoint);
                GL.Vertex(endPoint);
            }
            GL.End();
            GL.PopMatrix(); // Pop changes.
        }

        //Get current mouse coordinate on the scrapyard grid.
        protected Vector2Int getMouseCoordinate()
        {
            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            if (worldMousePosition.x > 0)
            {
                worldMousePosition.x += Constants.gridCellSize / 2;
            }
            else if (worldMousePosition.x < 0)
            {
                worldMousePosition.x -= Constants.gridCellSize / 2;
            }
            if (worldMousePosition.y > 0)
            {
                worldMousePosition.y += Constants.gridCellSize / 2;
            }
            else if (worldMousePosition.y < 0)
            {
                worldMousePosition.y -= Constants.gridCellSize / 2;
            }

            Vector2Int mouseCoordinate = new Vector2Int((int)(worldMousePosition.x / Constants.gridCellSize), (int)(worldMousePosition.y / Constants.gridCellSize));
            return mouseCoordinate;
        }
    }
}
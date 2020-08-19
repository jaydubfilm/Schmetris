using StarSalvager.Cameras;
using StarSalvager.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class AttachableEditorToolBase : MonoBehaviour
    {
        private const int MAX_DISTANCE = 3;
        
        public Material material;
        protected List<ScrapyardBot> _scrapyardBots;

        [SerializeField]
        private CameraController m_cameraController;
        public CameraController CameraController => m_cameraController;

        public PART_TYPE? selectedPartType = null;

        [NonSerialized]
        public int SelectedPartLevel = 0;

        [NonSerialized]
        public bool selectedPartReturnToStorage = false;

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
        protected static bool TryGetMouseCoordinate(out Vector2Int mouseCoordinate)
        {
            mouseCoordinate = Vector2Int.zero;
            Vector2 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //if (worldMousePosition.x > 0)
            //{
            //    worldMousePosition.x += Constants.gridCellSize / 2;
            //}
            //else if (worldMousePosition.x < 0)
            //{
            //    worldMousePosition.x -= Constants.gridCellSize / 2;
            //}
            //if (worldMousePosition.y > 0)
            //{
            //    worldMousePosition.y += Constants.gridCellSize / 2;
            //}
            //else if (worldMousePosition.y < 0)
            //{
            //    worldMousePosition.y -= Constants.gridCellSize / 2;
            //}

            var tempMouseCoord = new Vector2Int(
                Mathf.RoundToInt(worldMousePosition.x / Constants.gridCellSize),
                Mathf.RoundToInt(worldMousePosition.y / Constants.gridCellSize));
            
            if (Mathf.Abs(tempMouseCoord.x) > MAX_DISTANCE || Mathf.Abs(tempMouseCoord.y) > MAX_DISTANCE)
                return false;

            mouseCoordinate = tempMouseCoord;
            
            return true;
        }
    }
}
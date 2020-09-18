using StarSalvager.Cameras;
using StarSalvager.Values;
using System;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager
{
    public class AttachableEditorToolBase : MonoBehaviour
    {
        private const int MAX_DISTANCE = 3;

        //====================================================================================================================//
        
        public Material material;
        public ScrapyardBot _scrapyardBot { get; protected set; }

        [SerializeField]
        private CameraController m_cameraController;
        public CameraController CameraController => m_cameraController;

        [NonSerialized]
        public BlockData? SelectedBrick;
        
        [NonSerialized]
        public Vector3? SelectedPartClickPosition;
        [NonSerialized]
        public Vector2Int? SelectedPartPreviousGridPosition = null;
        [NonSerialized]
        protected bool SelectedPartRemoveFromStorage = false;
        [NonSerialized]
        protected bool SelectedPartReturnToStorageIfNotPlaced = false;

        //====================================================================================================================//

        public void SelectPartFromStorage(BlockData? blockData, bool returnIfNotPlaced = false)
        {
            SelectedBrick = blockData;
            SelectedPartRemoveFromStorage = blockData.HasValue;
            SelectedPartReturnToStorageIfNotPlaced = returnIfNotPlaced;
        }

        public void DrawGL(Camera camera)
        {
            Vector2 m_anchorPoint = new Vector2(-Constants.gridCellSize * 3.5f, -Constants.gridCellSize * 3.5f);
            //Draw debug lines to show the area of the grid
            for (int x = 0; x < 7; x++)
            {
                for (int y = 0; y < 7; y++)
                {
                    Vector2 tempVector = new Vector2(x, y);

                    DrawWithGL(material, m_anchorPoint + tempVector * Constants.gridCellSize, m_anchorPoint + new Vector2(x, y + 1) * Constants.gridCellSize);
                    DrawWithGL(material, m_anchorPoint + tempVector * Constants.gridCellSize, m_anchorPoint + new Vector2(x + 1, y) * Constants.gridCellSize);
                }
            }
            DrawWithGL(material, m_anchorPoint + new Vector2(0, 7) * Constants.gridCellSize, m_anchorPoint + new Vector2(7, 7) * Constants.gridCellSize);
            DrawWithGL(material, m_anchorPoint + new Vector2(7, 0) * Constants.gridCellSize, m_anchorPoint + new Vector2(7, 7) * Constants.gridCellSize);
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

            if (Camera.main is null)
                return false;
            
            Vector2 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

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
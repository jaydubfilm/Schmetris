using StarSalvager.Values;
using StarSalvager.Factories;
using UnityEngine;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Cameras;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.UI;
using StarSalvager.Utilities.Inputs;
using StarSalvager.ScriptableObjects;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.JsonDataTypes;

namespace StarSalvager
{
    public class BotShapeEditor : MonoBehaviour, IReset
    {
        [SerializeField, Required]
        public EditorBotShapeGeneratorScriptableObject m_editorBotShapeGeneratorScripableObject;

        public Material material;
        private List<ScrapyardBot> _scrapyardBots;
        private List<Shape> _shapes;

        [SerializeField]
        private CameraController m_cameraController;
        public CameraController CameraController => m_cameraController;

        public PART_TYPE? selectedPartType = null;
        public int selectedpartLevel = 0;

        [SerializeField]
        private BotShapeEditorUI m_botShapeEditorUI;

        // Start is called before the first frame update
        void Start()
        {
            _scrapyardBots = new List<ScrapyardBot>();
            _shapes = new List<Shape>();
            InputManager.Instance.InitInput();
            Activate();
        }

        private void OnDestroy()
        {
            Camera.onPostRender -= DrawGL;
        }

        public void Activate()
        {
            Camera.onPostRender += DrawGL;
            GameTimer.SetPaused(true);
        }

        public void Reset()
        {
            Camera.onPostRender -= DrawGL;

            DeloadAllBots();
        }

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

        public void RotateBots(float direction)
        {
            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                scrapBot.Rotate(direction);
            }
        }

        //On left mouse button click, check if there is a bit/part at the mouse location. If there is not, purchase the selected part type and place it at this location.
        public void OnLeftMouseButtonDown()
        {
            print("TESTLEFT");
            
            if (selectedPartType == null)
            {
                return;
            }

            Vector2Int mouseCoordinate = getMouseCoordinate();

            if (Mathf.Abs(mouseCoordinate.x) > 3 || Mathf.Abs(mouseCoordinate.y) > 3)
                return;

            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                if (scrapBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate) != null)
                    continue;

                var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>((PART_TYPE)selectedPartType, 0);
                scrapBot.AttachNewBit(mouseCoordinate, attachable);
            }
        }

        //On right mouse button click, check for a bit/part at the clicked location. If one is there, sell it.
        public void OnRightMouseButtonDown()
        {
            Vector2Int mouseCoordinate = getMouseCoordinate();

            if (Mathf.Abs(mouseCoordinate.x) > 3 || Mathf.Abs(mouseCoordinate.y) > 3)
                return;

            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                scrapBot.TryRemoveAttachableAt(mouseCoordinate, false);
            }

            /*foreach (Shape shape in _shapes)
            {
                //shape.TryRemoveAttachableAt(mouseCoordinate, false);
            }*/
        }

        //Get current mouse coordinate on the scrapyard grid.
        private Vector2Int getMouseCoordinate()
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

        public void CreateBot()
        {
            DeloadAllBots();
            DeloadAllShapes();
            _scrapyardBots.Add(FactoryManager.Instance.GetFactory<BotFactory>().CreateScrapyardObject<ScrapyardBot>());
            _scrapyardBots[0].InitBot();
        }

        public void CreateShape()
        {
            DeloadAllBots();
            DeloadAllShapes();
            _shapes.Add(FactoryManager.Instance.GetFactory<ShapeFactory>().CreateObject<Shape>());
            _shapes[0].PushNewBit(FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateObject<Bit>(), Vector2Int.zero);
        }

        public void LoadBlockData()
        {
            DeloadAllBots();
            CreateBot();
            var blockData = m_editorBotShapeGeneratorScripableObject.GetEditorBotData(m_botShapeEditorUI.GetNameInputFieldValue()).BlockData;
            if (blockData != null)
                _scrapyardBots[0].InitBot(blockData.ImportBlockDatas(true));
        }

        private void DeloadAllBots()
        {
            for (int i = _scrapyardBots.Count() - 1; i >= 0; i--)
            {
                Recycling.Recycler.Recycle<ScrapyardBot>(_scrapyardBots[i].gameObject);
                _scrapyardBots.RemoveAt(i);
            }
        }

        private void DeloadAllShapes()
        {
            for (int i = _shapes.Count() - 1; i >= 0; i--)
            {
                Recycling.Recycler.Recycle<Shape>(_shapes[i].gameObject);
                _shapes.RemoveAt(i);
            }
        }

        //Save the current bot's data in blockdata to be loaded in the level manager.
        public void SaveBlockData()
        {
            foreach (ScrapyardBot scrapyardbot in _scrapyardBots)
            {
                EditorBotGeneratorData newData = new EditorBotGeneratorData(m_botShapeEditorUI.GetNameInputFieldValue(), scrapyardbot.attachedBlocks.GetBlockDatas());
                m_editorBotShapeGeneratorScripableObject.AddEditorBotData(newData);
            }
        }
    }
}
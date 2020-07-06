using StarSalvager.Values;
using StarSalvager.Factories;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Cameras;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities;

namespace StarSalvager
{
    public class Scrapyard : MonoBehaviour, IReset
    {
        public Material material;
        private ScrapyardBot[] _scrapyardBots;

        [SerializeField]
        private CameraController m_cameraController;
        public CameraController CameraController => m_cameraController;

        public PartRemoteData selectedData = null;

        // Start is called before the first frame update
        void Start()
        {
            if (_scrapyardBots == null || _scrapyardBots.Length == 0)
                _scrapyardBots = FindObjectsOfType<ScrapyardBot>();
        }

        // Update is called once per frame
        void Update()
        {
            //Place new attachable on bot
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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

                if (Mathf.Abs(mouseCoordinate.x) > 3 || Mathf.Abs(mouseCoordinate.y) > 3)
                    return;

                foreach (ScrapyardBot scrapBot in _scrapyardBots)
                {
                    if (scrapBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate) != null)
                        continue;

                    if (selectedData != null)
                    {
                        scrapBot.AttachNewBit(mouseCoordinate, FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>(selectedData.partType, 1));
                        continue;
                    }

                    switch (Random.Range(0, 2))
                    {
                        case 0:
                            scrapBot.AttachNewBit(mouseCoordinate, FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateScrapyardObject<IAttachable>((BIT_TYPE)Random.Range(1, 6)));
                            break;
                        case 1:
                            scrapBot.AttachNewBit(mouseCoordinate, FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>((PART_TYPE)Random.Range(0, 5), 1));
                            break;
                    }
                }
            }

            //Remove attachable from bot
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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

                if (Mathf.Abs(mouseCoordinate.x) > 3 || Mathf.Abs(mouseCoordinate.y) > 3)
                    return;

                foreach (ScrapyardBot scrapBot in _scrapyardBots)
                {
                    scrapBot.RemoveAttachableAt(mouseCoordinate);
                }
            }
        }

        private void OnDestroy()
        {
            Camera.onPostRender -= DrawGL;
        }

        public void Activate()
        {
            Camera.onPostRender += DrawGL;
        }

        public void Reset()
        {
            Camera.onPostRender -= DrawGL;
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

        private void ToGameplayButtonPressed()
        {
            StarSalvager.SceneLoader.SceneLoader.ActivateScene("AlexShulmanTestScene", "ScrapyardScene");
        }
    }
}
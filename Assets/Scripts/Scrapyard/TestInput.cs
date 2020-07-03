using StarSalvager.Values;
using StarSalvager.Factories;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StarSalvager
{
    public class TestInput : MonoBehaviour
    {
        public Material material;
        private ScrapyardBot[] _scrapyardBots;

        // Start is called before the first frame update
        void Start()
        {
            if (_scrapyardBots == null || _scrapyardBots.Length == 0)
                _scrapyardBots = FindObjectsOfType<ScrapyardBot>();
        }

        private void OnEnable()
        {
            Camera.onPostRender += DrawGL;
        }

        private void OnDisable()
        {
            Camera.onPostRender -= DrawGL;
        }

        public void DrawGL(Camera camera)
        {
            Vector2 m_anchorPoint = new Vector2(-Values.Constants.gridCellSize * 25.5f, -Values.Constants.gridCellSize * 25.5f);
            //Draw debug lines to show the area of the grid
            for (int x = 0; x < 50; x++)
            {
                for (int y = 0; y < 50; y++)
                {
                    Vector2 tempVector = new Vector2(x, y);

                    DrawWithGL(material, m_anchorPoint + tempVector * Values.Constants.gridCellSize, m_anchorPoint + new Vector2(x, y + 1) * Values.Constants.gridCellSize);
                    DrawWithGL(material, m_anchorPoint + tempVector * Values.Constants.gridCellSize, m_anchorPoint + new Vector2(x + 1, y) * Values.Constants.gridCellSize);
                }
            }
            DrawWithGL(material, m_anchorPoint + new Vector2(0, 50) * Values.Constants.gridCellSize, m_anchorPoint + new Vector2(50, 50) * Values.Constants.gridCellSize);
            DrawWithGL(material, m_anchorPoint + new Vector2(50, 0) * Values.Constants.gridCellSize, m_anchorPoint + new Vector2(50, 50) * Values.Constants.gridCellSize);
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

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                foreach (ScrapyardBot scrapBot in _scrapyardBots)
                {
                    scrapBot.AttachNewBit(Vector2Int.down, FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateScrapyardObject<IAttachable>(BIT_TYPE.BLUE));
                }
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                foreach (ScrapyardBot scrapBot in _scrapyardBots)
                {
                    scrapBot.AttachNewBit(Vector2Int.down, FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>(PART_TYPE.ARMOR, 1));
                }
            }

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

                Vector2Int botCoordinate = new Vector2Int((int)(worldMousePosition.x / Constants.gridCellSize), (int)(worldMousePosition.y / Constants.gridCellSize));
                foreach (ScrapyardBot scrapBot in _scrapyardBots)
                {
                    switch(Random.Range(0, 2))
                    {
                        case 0:
                            scrapBot.AttachNewBit(botCoordinate, FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateScrapyardObject<IAttachable>((BIT_TYPE)Random.Range(1, 6)));
                            break;
                        case 1:
                            scrapBot.AttachNewBit(botCoordinate, FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>((PART_TYPE)Random.Range(0, 5), 1));
                            break;
                    }
                }
            }

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
                print(mouseCoordinate);
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
    }
}
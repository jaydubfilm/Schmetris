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
using JetBrains.Annotations;

namespace StarSalvager
{
    public class Scrapyard : MonoBehaviour, IReset
    {
        public Material material;
        private List<ScrapyardBot> _scrapyardBots;

        [SerializeField]
        private CameraController m_cameraController;
        public CameraController CameraController => m_cameraController;

        public PART_TYPE? selectedPartType = null;
        public int selectedpartLevel = 0;

        [SerializeField]
        private ScrapyardUI m_scrapyardUI;

        // Start is called before the first frame update
        void Start()
        {
            _scrapyardBots = new List<ScrapyardBot>();
            InputManager.Instance.InitInput();
        }

        private void OnDestroy()
        {
            Camera.onPostRender -= DrawGL;
        }

        public void Activate()
        {
            Camera.onPostRender += DrawGL;

            _scrapyardBots.Add(FactoryManager.Instance.GetFactory<BotFactory>().CreateScrapyardObject<ScrapyardBot>());
            if (PlayerPersistentData.GetPlayerData().GetCurrentBlockData().Count == 0)
            {
                _scrapyardBots[0].InitBot();
            }
            else
            {
                _scrapyardBots[0].InitBot(PlayerPersistentData.GetPlayerData().GetCurrentBlockData().ImportBlockDatas(true));
            }
        }

        public void Reset()
        {
            Camera.onPostRender -= DrawGL;

            for (int i = _scrapyardBots.Count() - 1; i >= 0; i--)
            {
                Recycling.Recycler.Recycle<ScrapyardBot>(_scrapyardBots[i].gameObject);
                _scrapyardBots.RemoveAt(i);
            }
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

        public void SellBits()
        {
            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                Dictionary<BIT_TYPE, int> bits = FactoryManager.Instance.GetFactory<BitAttachableFactory>().GetTotalResources(scrapBot.attachedBlocks.OfType<ScrapyardBit>());
                PlayerPersistentData.GetPlayerData().AddResources(bits);
                scrapBot.RemoveAllBits();
            }
        }

        public void RotateBots(float direction)
        {
            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                scrapBot.Rotate(direction);
            }
        }

        //TODO: Simplify redundancies between left and right click

        public void LeftClick(float clicked)
        {
            if (clicked == 0)
            {
                return;
            }
            
            if (selectedPartType == null)
            {
                return;
            }

            if (!PlayerPersistentData.GetPlayerData().CanAffordPart((PART_TYPE)selectedPartType, selectedpartLevel))
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
                PlayerPersistentData.GetPlayerData().SubtractResources((PART_TYPE)selectedPartType, 0);
                scrapBot.AttachNewBit(mouseCoordinate, attachable);
                m_scrapyardUI.UpdateResources(PlayerPersistentData.GetPlayerData().GetResources());
            }
        }

        public void RightClick(float clicked)
        {
            if (clicked == 0)
            {
                return;
            }

            Vector2Int mouseCoordinate = getMouseCoordinate();

            if (Mathf.Abs(mouseCoordinate.x) > 3 || Mathf.Abs(mouseCoordinate.y) > 3)
                return;

            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                scrapBot.TryRemoveAttachableAt(mouseCoordinate, true);
                m_scrapyardUI.UpdateResources(PlayerPersistentData.GetPlayerData().GetResources());
            }
        }

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

        public void SaveBlockData()
        {
            foreach (ScrapyardBot scrapyardbot in _scrapyardBots)
            {
                PlayerPersistentData.GetPlayerData().SetCurrentBlockData(scrapyardbot.attachedBlocks.GetBlockDatas());
            }
        }

        private void ToGameplayButtonPressed()
        {
            StarSalvager.SceneLoader.SceneLoader.ActivateScene("AlexShulmanTestScene", "ScrapyardScene");
        }
    }
}
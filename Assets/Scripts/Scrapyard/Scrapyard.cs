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
    public class Scrapyard : AttachableEditorToolBase, IReset
    {
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
            GameTimer.SetPaused(true);
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

        //On left mouse button click, check if there is a bit/part at the mouse location. If there is not, purchase the selected part type and place it at this location.
        public void OnLeftMouseButtonDown()
        {
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

        //On right mouse button click, check for a bit/part at the clicked location. If one is there, sell it.
        public void OnRightMouseButtonDown()
        {
            Vector2Int mouseCoordinate = getMouseCoordinate();

            if (Mathf.Abs(mouseCoordinate.x) > 3 || Mathf.Abs(mouseCoordinate.y) > 3)
                return;

            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                scrapBot.TryRemoveAttachableAt(mouseCoordinate, true);
                m_scrapyardUI.UpdateResources(PlayerPersistentData.GetPlayerData().GetResources());
            }
        }

        //Save the current bot's data in blockdata to be loaded in the level manager.
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

        public void ProcessScrapyardUsageEndAnalytics()
        {
            Dictionary<string, object> scrapyardUsageEndAnalyticsDictionary = new Dictionary<string, object>();
            AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.ScrapyardUsageEnd, scrapyardUsageEndAnalyticsDictionary);
        }
    }
}
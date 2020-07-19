﻿using StarSalvager.Values;
using StarSalvager.Factories;
using UnityEngine;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.UI;
using UnityEngine.InputSystem;

using Input = StarSalvager.Utilities.Inputs.Input;

namespace StarSalvager
{
    public class Scrapyard : AttachableEditorToolBase, IReset, IInput
    {
        [SerializeField]
        private ScrapyardUI m_scrapyardUI;
        
        //============================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            _scrapyardBots = new List<ScrapyardBot>();
            //InputManager.Instance.InitInput();
            InitInput();
        }

        private void OnDestroy()
        {
            Camera.onPostRender -= DrawGL;

            DeInitInput();
        }
        
        //============================================================================================================//
        
        public void InitInput()
        {
            Input.Actions.Default.LeftClick.Enable();
            Input.Actions.Default.LeftClick.performed += OnLeftMouseButtonDown;
            
            Input.Actions.Default.RightClick.Enable();
            Input.Actions.Default.RightClick.performed += OnRightMouseButtonDown;
        }

        public void DeInitInput()
        {
            Input.Actions.Default.LeftClick.Disable();
            Input.Actions.Default.LeftClick.performed -= OnLeftMouseButtonDown;
            
            Input.Actions.Default.RightClick.Disable();
            Input.Actions.Default.RightClick.performed -= OnRightMouseButtonDown;
        }
        
        //============================================================================================================//

        public void Activate()
        {
            GameTimer.SetPaused(true);
            Camera.onPostRender += DrawGL;

            _scrapyardBots.Add(FactoryManager.Instance.GetFactory<BotFactory>().CreateScrapyardObject<ScrapyardBot>());
            if (PlayerPersistentData.PlayerData.GetCurrentBlockData().Count == 0)
            {
                _scrapyardBots[0].InitBot();
            }
            else
            {
                _scrapyardBots[0].InitBot(PlayerPersistentData.PlayerData.GetCurrentBlockData().ImportBlockDatas(true));
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
        
        //============================================================================================================//

        public void SellBits()
        {
            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                Dictionary<BIT_TYPE, int> bits = FactoryManager.Instance.GetFactory<BitAttachableFactory>().GetTotalResources(scrapBot.attachedBlocks.OfType<ScrapyardBit>());
                PlayerPersistentData.PlayerData.AddResources(bits);
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



        //============================================================================================================//

        //On left mouse button click, check if there is a bit/part at the mouse location. If there is not, purchase the selected part type and place it at this location.
        private void OnLeftMouseButtonDown(InputAction.CallbackContext ctx)
        {
            if (selectedPartType == null)
                return;

            if (ctx.ReadValue<float>() == 1f)
                return;

            if (!PlayerPersistentData.PlayerData.CanAffordPart((PART_TYPE)selectedPartType, selectedpartLevel))
            {
                m_scrapyardUI.DisplayInsufficientResources();
                return;
            }

            if (!TryGetMouseCoordinate(out Vector2Int mouseCoordinate))
                return;
            
            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                if (scrapBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate) != null)
                    continue;

                var playerData = PlayerPersistentData.PlayerData;

                var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>((PART_TYPE)selectedPartType, 0);
                playerData.SubtractResources((PART_TYPE)selectedPartType, 0);
                scrapBot.AttachNewBit(mouseCoordinate, attachable);
                
                m_scrapyardUI.UpdateResources(playerData.GetResources());
            }
        }

        //On right mouse button click, check for a bit/part at the clicked location. If one is there, sell it.
        private void OnRightMouseButtonDown(InputAction.CallbackContext ctx)
        {
            if (ctx.ReadValue<float>() == 0f)
                return;
            
            if (!TryGetMouseCoordinate(out Vector2Int mouseCoordinate))
                return;

            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                scrapBot.TryRemoveAttachableAt(mouseCoordinate, true);
                m_scrapyardUI.UpdateResources(PlayerPersistentData.PlayerData.GetResources());
            }
        }
        
        //============================================================================================================//

        public bool IsFullyConnected()
        {
            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                if (scrapBot.CheckHasDisconnects())
                {
                    return false;
                }
            }

            return true;
        }

        //Save the current bot's data in blockdata to be loaded in the level manager.
        public void SaveBlockData()
        {
            foreach (ScrapyardBot scrapyardbot in _scrapyardBots)
            {
                PlayerPersistentData.PlayerData.SetCurrentBlockData(scrapyardbot.attachedBlocks.GetBlockDatas());
            }
        }
        
        //============================================================================================================//


        private void ToGameplayButtonPressed()
        {
            StarSalvager.SceneLoader.SceneLoader.ActivateScene("AlexShulmanTestScene", "ScrapyardScene");
        }

        public void ProcessScrapyardUsageEndAnalytics()
        {
            Dictionary<string, object> scrapyardUsageEndAnalyticsDictionary = new Dictionary<string, object>();
            AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.ScrapyardUsageEnd, scrapyardUsageEndAnalyticsDictionary);
        }

        //============================================================================================================//

    }
}
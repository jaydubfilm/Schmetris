﻿using StarSalvager.Factories;
using UnityEngine;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Cameras;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.UI;
using UnityEngine.InputSystem;
using Input = StarSalvager.Utilities.Inputs.Input;
using Newtonsoft.Json;
using System.IO;
using StarSalvager.Values;

namespace StarSalvager
{
    public class BotShapeEditor : AttachableEditorToolBase, IReset, IInput
    {
        private List<Shape> _shapes;

        [SerializeField]
        private BotShapeEditorUI m_botShapeEditorUI;

        public bool EditingBot => _scrapyardBots.Count > 0;
        public bool EditingShape => _shapes.Count > 0;

        public EditorBotShapeGeneratorData EditorBotShapeData
        {
            get
            {
                if (m_editorBotShapeData == null)
                    m_editorBotShapeData = FactoryManager.Instance.ImportBotShapeRemoteData();

                return m_editorBotShapeData;
            }
        }
        private EditorBotShapeGeneratorData m_editorBotShapeData = null;

        public BIT_TYPE? SelectedBitType = null;

        //============================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            _scrapyardBots = new List<ScrapyardBot>();
            _shapes = new List<Shape>();
            Activate();

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
            Camera.onPostRender += DrawGL;
            //GameTimer.SetPaused(true);
        }

        public void Reset()
        {
            Camera.onPostRender -= DrawGL;

            DeloadAllBots();
        }
        public void RotateBots(float direction)
        {
            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                scrapBot.Rotate(direction);
            }

            foreach (Shape shape in _shapes)
            {
                //TODO: Rotate shape
            }
        }
        
        //============================================================================================================//

        //On left mouse button click, check if there is a bit/part at the mouse location. If there is not, purchase the selected part type and place it at this location.
        private void OnLeftMouseButtonDown(InputAction.CallbackContext ctx)
        {
            if (m_botShapeEditorUI.IsPopupActive)
                return;
            
            if (ctx.ReadValue<float>() == 1f)
                return;

            if (!TryGetMouseCoordinate(out Vector2Int mouseCoordinate))
                return;

            foreach (Shape shape in _shapes)
            {
                if (shape.AttachedBits.Any(b => b.Coordinate == mouseCoordinate))
                    continue;

                if (SelectedBitType is BIT_TYPE bitType)
                {
                    shape.PushNewBit(FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateObject<Bit>(bitType, SelectedPartLevel), mouseCoordinate);
                }
            }

            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                if (scrapBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate) != null)
                {
                    IAttachable attachable = scrapBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);
                    if (attachable != null && attachable is ScrapyardPart partAtCoordinates && partAtCoordinates.Type == PART_TYPE.CORE)
                    {
                        FactoryManager.Instance.GetFactory<PartAttachableFactory>().UpdatePartData(partAtCoordinates.Type, partAtCoordinates.level + 1, ref partAtCoordinates);
                    }
                    continue;
                }

                if (selectedPartType == null && SelectedBitType == null)
                {
                    return;
                }

                if (selectedPartType != null)
                {
                    var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>((PART_TYPE)selectedPartType, SelectedPartLevel);
                    scrapBot.AttachNewBit(mouseCoordinate, attachable);
                }
                else if (SelectedBitType is BIT_TYPE bitType)
                {
                    scrapBot.AttachNewBit(mouseCoordinate, FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateScrapyardObject<ScrapyardBit>(bitType, SelectedPartLevel));
                }
            }
        }

        //On right mouse button click, check for a bit/part at the clicked location. If one is there, sell it.
        private void OnRightMouseButtonDown(InputAction.CallbackContext ctx)
        {
            if (m_botShapeEditorUI.IsPopupActive)
                return;
            
            if (ctx.ReadValue<float>() == 0f)
                return;
            
            if (!TryGetMouseCoordinate(out Vector2Int mouseCoordinate))
                return;

            if (mouseCoordinate.x != 0 || mouseCoordinate.y != 0)
            {
                foreach (ScrapyardBot scrapBot in _scrapyardBots)
                {
                    IAttachable attachable = scrapBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);
                    if (attachable != null && attachable is ScrapyardPart partAtCoordinates && partAtCoordinates.Type == PART_TYPE.CORE && partAtCoordinates.level > 0)
                    {
                        FactoryManager.Instance.GetFactory<PartAttachableFactory>().UpdatePartData(partAtCoordinates.Type, partAtCoordinates.level - 1, ref partAtCoordinates);
                        continue;
                    }
                    
                    scrapBot.TryRemoveAttachableAt(mouseCoordinate, false);
                } 
            }
                

            foreach (Shape shape in _shapes)
            {
                shape.DestroyBit(mouseCoordinate);
            }
        }

        //============================================================================================================//

        
        public void CreateBot(bool initBot)
        {
            DeloadAllBots();
            DeloadAllShapes();
            _scrapyardBots.Add(FactoryManager.Instance.GetFactory<BotFactory>().CreateScrapyardObject<ScrapyardBot>());

            if (initBot)
                _scrapyardBots[0].InitBot();
        }

        public void CreateShape(List<Bit> bits)
        {
            DeloadAllBots();
            DeloadAllShapes();
            if (bits == null || bits.Count == 0)
            {
                _shapes.Add(FactoryManager.Instance.GetFactory<ShapeFactory>().CreateObject<Shape>());
                _shapes[0].PushNewBit(FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateObject<Bit>((BIT_TYPE)Random.Range(0, 6), 0), Vector2Int.zero);
            }
            else
            {
                _shapes.Add(FactoryManager.Instance.GetFactory<ShapeFactory>().CreateObject<Shape>(bits));
            }

            _shapes[0].transform.position = Vector3.zero;
        }

        public void LoadBlockData(string inputName)
        {
            DeloadAllBots();
            DeloadAllShapes();
            var botData = EditorBotShapeData.GetEditorBotData(inputName);
            if (botData != null && botData.BlockData != null)
            {
                CreateBot(false);
                _scrapyardBots[0].InitBot(botData.BlockData.ImportBlockDatas(true));
                m_botShapeEditorUI.SetPartsScrollActive(true);
                m_botShapeEditorUI.SetCategoriesScrollActive(false);
                return;
            }

            var shapeData = EditorBotShapeData.GetEditorShapeData(inputName);
            if (shapeData != null && shapeData.BlockData != null)
            {
                List<Bit> bits = shapeData.BlockData.ImportBlockDatas(false).FindAll(o => o is Bit).OfType<Bit>().ToList();
                CreateShape(bits);
                m_botShapeEditorUI.SetBitsScrollActive(true);
                m_botShapeEditorUI.SetCategoriesScrollActive(false);
                m_botShapeEditorUI.UpdateCategories(shapeData);
                return;
            }
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

        public bool CheckLegal()
        {
            foreach (ScrapyardBot scrapyardBot in _scrapyardBots)
            {
                foreach (var attached in scrapyardBot.attachedBlocks)
                {
                    if (!scrapyardBot.attachedBlocks.HasPathToCore(attached))
                    {
                        return false;
                    }
                }
            }

            foreach (Shape shape in _shapes)
            {
                foreach (var attached in shape.AttachedBits)
                {
                    if (!shape.AttachedBits.HasPathToCore(attached))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void RemoveFloating()
        {
            foreach (ScrapyardBot scrapyardBot in _scrapyardBots)
            {
                List<IAttachable> toRemove = new List<IAttachable>();
                foreach (var attached in scrapyardBot.attachedBlocks)
                {
                    if (!scrapyardBot.attachedBlocks.HasPathToCore(attached))
                    {
                        toRemove.Add(attached);
                    }
                }

                foreach (var remove in toRemove)
                {
                    scrapyardBot.TryRemoveAttachableAt(remove.Coordinate, false);
                }
            }

            foreach (Shape shape in _shapes)
            {
                List<IAttachable> toRemove = new List<IAttachable>();
                foreach (var attached in shape.AttachedBits)
                {
                    if (!shape.AttachedBits.HasPathToCore(attached))
                    {
                        toRemove.Add(attached);
                    }
                }

                foreach (var remove in toRemove)
                {
                    shape.DestroyBit(remove.Coordinate);
                }
            }
        }

        //Save the current bot's data in blockdata to be loaded in the level manager.
        public void SaveBlockData(string inputName)
        {
            foreach (ScrapyardBot scrapyardbot in _scrapyardBots)
            {
                EditorBotGeneratorData newData = new EditorBotGeneratorData(inputName, scrapyardbot.attachedBlocks.GetBlockDatas());
                EditorBotShapeData.AddEditorBotData(newData);
            }

            foreach (Shape shape in _shapes)
            {
                EditorShapeGeneratorData newData = new EditorShapeGeneratorData(inputName, shape.AttachedBits.GetBlockDatas(), m_botShapeEditorUI.GetCategories());
                EditorBotShapeData.AddEditorShapeData(newData);
            }

            //DeloadAllBots();
            //DeloadAllShapes();
        }

        public void PushBot()
        {
            foreach (ScrapyardBot scrapyardbot in _scrapyardBots)
            {
                PlayerPersistentData.PlayerData.SetCurrentBlockData(scrapyardbot.attachedBlocks.GetBlockDatas());
            }
        }

        public void AddCategory(string categoryName)
        {
            if (!EditorBotShapeData.m_categories.Contains(categoryName))
                EditorBotShapeData.m_categories.Add(categoryName);
        }

        public void OnApplicationQuit()
        {
            FactoryManager.Instance.ExportBotShapeRemoteData(m_editorBotShapeData);
        }
    }
}
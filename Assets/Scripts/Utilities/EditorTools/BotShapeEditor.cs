using System;
using StarSalvager.Factories;
using UnityEngine;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.UI;
using UnityEngine.InputSystem;
using Input = StarSalvager.Utilities.Inputs.Input;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Values;
using StarSalvager.Utilities.Saving;

namespace StarSalvager
{
    public class BotShapeEditor : AttachableEditorToolBase, IReset, IInput
    {
        private Shape _shape;

        public bool EditingBot => _scrapyardBot != null;
        public bool EditingShape => _shape != null;

        [SerializeField]
        private BotShapeEditorUI m_botShapeEditorUI;

        public EditorBotShapeGeneratorData EditorBotShapeData => _mEditorBotShapeData ?? (_mEditorBotShapeData = Files.ImportBotShapeRemoteData());
        private EditorBotShapeGeneratorData _mEditorBotShapeData;

        //public BIT_TYPE? SelectedBitType = null;

        //============================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
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

            DeloadBot();
        }
        public void RotateBots(float direction)
        {
            _scrapyardBot.Rotate(direction);
        }
        
        //============================================================================================================//

        //On left mouse button click, check if there is a bit/part at the mouse location. If there is not, purchase the selected part type and place it at this location.
        private void OnLeftMouseButtonDown(InputAction.CallbackContext ctx)
        {
            if (m_botShapeEditorUI.IsPopupActive)
                return;
            
            if (ctx.ReadValue<float>() == 1f)
                return;

            if (!IsMouseInEditorGrid(out Vector2Int mouseCoordinate))
                return;

            if (_shape != null && _shape.AttachedBits.All(b => b.Coordinate != mouseCoordinate))
            {
                if (SelectedBrick is BitData bitData)
                {
                    _shape.PushNewBit(FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateObject<Bit>(bitData), mouseCoordinate);

                }
            }

            if (_scrapyardBot == null)
                return;
            
            if (_scrapyardBot.AttachedBlocks.GetAttachableAtCoordinates(mouseCoordinate) != null)
            {
                IAttachable attachable = _scrapyardBot.AttachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);
                if (attachable != null && attachable is ScrapyardPart partAtCoordinates && partAtCoordinates.Type == PART_TYPE.CORE)
                {
                    FactoryManager.Instance.GetFactory<PartAttachableFactory>().UpdatePartData(partAtCoordinates.Type, 1, ref partAtCoordinates);
                }
            }
            else
            {
                if (SelectedBrick is null)
                    return;

                switch (SelectedBrick)
                {
                    case PartData partData:
                        var part = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>(partData);
                        _scrapyardBot.AttachNewBit(mouseCoordinate, part);
                        break;
                    case BitData bitData:
                        var bit = FactoryManager.Instance.GetFactory<BitAttachableFactory>()
                            .CreateScrapyardObject<ScrapyardBit>(bitData);
                        _scrapyardBot.AttachNewBit(mouseCoordinate, bit);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(SelectedBrick), SelectedBrick, null);
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
            
            if (!IsMouseInEditorGrid(out Vector2Int mouseCoordinate))
                return;

            if (mouseCoordinate.x != 0 || mouseCoordinate.y != 0)
            {
                if (_scrapyardBot != null)
                {
                    IAttachable attachable = _scrapyardBot.AttachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);
                    if (attachable != null && attachable is ScrapyardPart partAtCoordinates && partAtCoordinates.Type == PART_TYPE.CORE)
                    {
                        FactoryManager.Instance.GetFactory<PartAttachableFactory>().UpdatePartData(partAtCoordinates.Type, 0, ref partAtCoordinates);
                        return;
                    }

                    _scrapyardBot.TryRemoveAttachableAt(mouseCoordinate, false);
                } 
            }
                

            if (_shape != null)
            {
                _shape.DestroyBit(mouseCoordinate, false);
            }
        }

        //============================================================================================================//

        
        public void CreateBot(bool initBot)
        {
            DeloadBot();
            DeloadShape();
            _scrapyardBot = FactoryManager.Instance.GetFactory<BotFactory>().CreateScrapyardObject<ScrapyardBot>();

            if (initBot)
                _scrapyardBot.InitBot();
        }

        public void CreateShape(List<Bit> bits)
        {
            DeloadBot();
            DeloadShape();
            if (bits == null || bits.Count == 0)
            {
                _shape = FactoryManager.Instance.GetFactory<ShapeFactory>().CreateObject<Shape>();
            }
            else
            {
                _shape = FactoryManager.Instance.GetFactory<ShapeFactory>().CreateObject<Shape>(bits);
            }

            _shape.transform.position = Vector3.zero;
        }

        public void LoadBlockData(string inputName)
        {
            DeloadBot();
            DeloadShape();
            var botData = EditorBotShapeData.GetEditorBotData(inputName);
            if (botData != null && botData.BlockData != null)
            {
                CreateBot(false);
                _scrapyardBot.InitBot(botData.BlockData.ImportBlockDatas(true));
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

        private void DeloadBot()
        {
            if (_scrapyardBot == null) 
                return;
            
            Recycling.Recycler.Recycle<ScrapyardBot>(_scrapyardBot.gameObject);
            _scrapyardBot = null;
        }

        private void DeloadShape()
        {
            if (_shape == null) 
                return;
            
            Recycling.Recycler.Recycle<Shape>(_shape.gameObject);
            _shape = null;
        }

        public bool CheckLegal()
        {
            if (_scrapyardBot != null)
            {
                foreach (var attached in _scrapyardBot.AttachedBlocks)
                {
                    if (!_scrapyardBot.AttachedBlocks.HasPathToCore(attached))
                    {
                        return false;
                    }
                }
            }

            if (_shape != null)
            {
                foreach (var attached in _shape.AttachedBits)
                {
                    if (!_shape.AttachedBits.HasPathToCore(attached))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void RemoveFloating()
        {
            if (_scrapyardBot != null)
            {
                List<IAttachable> toRemove = new List<IAttachable>();
                foreach (var attached in _scrapyardBot.AttachedBlocks)
                {
                    if (!_scrapyardBot.AttachedBlocks.HasPathToCore(attached))
                    {
                        toRemove.Add(attached);
                    }
                }

                foreach (var remove in toRemove)
                {
                    _scrapyardBot.TryRemoveAttachableAt(remove.Coordinate, false);
                }
            }

            if (_shape != null)
            {
                List<IAttachable> toRemove = new List<IAttachable>();
                foreach (var attached in _shape.AttachedBits)
                {
                    if (!_shape.AttachedBits.HasPathToCore(attached))
                    {
                        toRemove.Add(attached);
                    }
                }

                foreach (var remove in toRemove)
                {
                    _shape.DestroyBit(remove.Coordinate);
                }
            }
        }

        //Save the current bot's data in blockdata to be loaded in the level manager.
        public void SaveBlockData(string inputName)
        {
            if (_scrapyardBot != null)
            {
                EditorBotGeneratorData newData = new EditorBotGeneratorData(inputName, _scrapyardBot.AttachedBlocks.GetBlockDatas());
                EditorBotShapeData.AddEditorBotData(newData);
            }

            if (_shape != null)
            {
                EditorShapeGeneratorData newData = new EditorShapeGeneratorData(inputName,
                    _shape.AttachedBits.GetBlockDatas(), 
                    m_botShapeEditorUI.GetCategories());
                
                EditorBotShapeData.AddEditorShapeData(newData);
            }
        }

        public void PushBot()
        {
            if (_scrapyardBot != null)
            {
                int saveSlotIndex = PlayerDataManager.GetIndexMostRecentSaveFile();

                if (saveSlotIndex >= 0)
                {
                    PlayerDataManager.SetCurrentSaveFile(saveSlotIndex);
                    PlayerDataManager.SetCurrentSaveSlotIndex(saveSlotIndex);
                }
                else
                {
                    saveSlotIndex = Files.GetNextAvailableSaveSlot();

                    if (saveSlotIndex >= 0)
                    {
                        PlayerDataManager.SetCurrentSaveSlotIndex(saveSlotIndex);
                        PlayerDataManager.ResetPlayerAccountData();
                    }
                    else
                    {
                        return;
                    }
                }

                PlayerDataManager.SetBlockData(_scrapyardBot.AttachedBlocks.GetBlockDatas());
            }
        }

        public void AddCategory(string categoryName)
        {
            if (!EditorBotShapeData.m_categories.Contains(categoryName))
                EditorBotShapeData.m_categories.Add(categoryName);
        }

        public void OnApplicationQuit()
        {
            Files.ExportBotShapeRemoteData(_mEditorBotShapeData);
        }
    }
}
using StarSalvager.Factories;
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
        private Shape _shape = null;

        public bool EditingBot => _scrapyardBot != null;
        public bool EditingShape => _shape != null;

        [SerializeField]
        private BotShapeEditorUI m_botShapeEditorUI;

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
            _scrapyardBot.Rotate(direction);

            //TODO Rotate shape
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

            if (_shape != null && !_shape.AttachedBits.Any(b => b.Coordinate == mouseCoordinate))
            {
                if (SelectedBitType is BIT_TYPE bitType)
                {
                    _shape.PushNewBit(FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateObject<Bit>(bitType, SelectedPartLevel), mouseCoordinate);
                }
            }

            if (_scrapyardBot != null)
            {
                if (_scrapyardBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate) != null)
                {
                    IAttachable attachable = _scrapyardBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);
                    if (attachable != null && attachable is ScrapyardPart partAtCoordinates && partAtCoordinates.Type == PART_TYPE.CORE)
                    {
                        FactoryManager.Instance.GetFactory<PartAttachableFactory>().UpdatePartData(partAtCoordinates.Type, partAtCoordinates.level + 1, ref partAtCoordinates);
                    }
                }
                else
                {
                    if (SelectedPartType == null && SelectedBitType == null)
                    {
                        return;
                    }

                    if (SelectedPartType != null)
                    {
                        var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>((PART_TYPE)SelectedPartType, SelectedPartLevel);
                        _scrapyardBot.AttachNewBit(mouseCoordinate, attachable);
                    }
                    else if (SelectedBitType is BIT_TYPE bitType)
                    {
                        _scrapyardBot.AttachNewBit(mouseCoordinate, FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateScrapyardObject<ScrapyardBit>(bitType, SelectedPartLevel));
                    }
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
                if (_scrapyardBot != null)
                {
                    IAttachable attachable = _scrapyardBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);
                    if (attachable != null && attachable is ScrapyardPart partAtCoordinates && partAtCoordinates.Type == PART_TYPE.CORE && partAtCoordinates.level > 0)
                    {
                        FactoryManager.Instance.GetFactory<PartAttachableFactory>().UpdatePartData(partAtCoordinates.Type, partAtCoordinates.level - 1, ref partAtCoordinates);
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
            DeloadAllBots();
            DeloadAllShapes();
            _scrapyardBot = FactoryManager.Instance.GetFactory<BotFactory>().CreateScrapyardObject<ScrapyardBot>();

            if (initBot)
                _scrapyardBot.InitBot();
        }

        public void CreateShape(List<Bit> bits)
        {
            DeloadAllBots();
            DeloadAllShapes();
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
            DeloadAllBots();
            DeloadAllShapes();
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

        private void DeloadAllBots()
        {
            Recycling.Recycler.Recycle<ScrapyardBot>(_scrapyardBot.gameObject);
            _scrapyardBot = null;
        }

        private void DeloadAllShapes()
        {
            Recycling.Recycler.Recycle<Shape>(_shape.gameObject);
            _shape = null;
        }

        public bool CheckLegal()
        {
            if (_scrapyardBot != null)
            {
                foreach (var attached in _scrapyardBot.attachedBlocks)
                {
                    if (!_scrapyardBot.attachedBlocks.HasPathToCore(attached))
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
                foreach (var attached in _scrapyardBot.attachedBlocks)
                {
                    if (!_scrapyardBot.attachedBlocks.HasPathToCore(attached))
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
                EditorBotGeneratorData newData = new EditorBotGeneratorData(inputName, _scrapyardBot.attachedBlocks.GetBlockDatas());
                EditorBotShapeData.AddEditorBotData(newData);
            }

            if (_shape != null)
            {
                EditorShapeGeneratorData newData = new EditorShapeGeneratorData(inputName, _shape.AttachedBits.GetBlockDatas(), m_botShapeEditorUI.GetCategories());
                EditorBotShapeData.AddEditorShapeData(newData);
            }
        }

        public void PushBot()
        {
            if (_scrapyardBot != null)
            {
                PlayerPersistentData.PlayerData.SetCurrentBlockData(_scrapyardBot.attachedBlocks.GetBlockDatas());
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
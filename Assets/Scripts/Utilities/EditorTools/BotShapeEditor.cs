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
using UnityEngine.InputSystem;
using Input = StarSalvager.Utilities.Inputs.Input;
using UnityEditor;
using Newtonsoft.Json;
using System.IO;

namespace StarSalvager
{
    public class BotShapeEditor : AttachableEditorToolBase, IReset, IInput
    {
        private List<Shape> _shapes;

        [SerializeField]
        private BotShapeEditorUI m_botShapeEditorUI;

        public EditorBotShapeGeneratorData EditorBotShapeData
        {
            get
            {
                if (m_editorBotShapeData == null)
                    m_editorBotShapeData = ImportRemoteData();

                return m_editorBotShapeData;
            }
        }
        private EditorBotShapeGeneratorData m_editorBotShapeData = null;

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
                
                shape.PushNewBit(FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateObject<Bit>(), mouseCoordinate);
            }

            if (selectedPartType == null)
            {
                return;
            }

            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                if (scrapBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate) != null)
                    continue;

                var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>((PART_TYPE)selectedPartType, 0);
                scrapBot.AttachNewBit(mouseCoordinate, attachable);
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

            if (mouseCoordinate.x == 0 && mouseCoordinate.y == 0)
            {
                foreach (ScrapyardBot scrapBot in _scrapyardBots)
                {
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
                _shapes[0].PushNewBit(FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateObject<Bit>(), Vector2Int.zero);
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
                m_botShapeEditorUI.SetPartsScrollActive(false);
                m_botShapeEditorUI.SetCategoriesScrollActive(true);
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

            DeloadAllBots();
            DeloadAllShapes();
        }

        public void AddCategory(string categoryName)
        {
            if (!EditorBotShapeData.m_categories.Contains(categoryName))
                EditorBotShapeData.m_categories.Add(categoryName);
        }

        public string ExportRemoteData(EditorBotShapeGeneratorData editorData)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(Application.dataPath + "/RemoteData/BotShapeEditorData.txt", export);

            return export;
        }

        public EditorBotShapeGeneratorData ImportRemoteData()
        {
            if (!File.Exists(Application.dataPath + "/RemoteData/BotShapeEditorData.txt"))
                return new EditorBotShapeGeneratorData();
            
            var loaded = JsonConvert.DeserializeObject<EditorBotShapeGeneratorData>(File.ReadAllText(Application.dataPath + "/RemoteData/BotShapeEditorData.txt"));

            return loaded;
        }

        public void OnApplicationQuit()
        {
            ExportRemoteData(m_editorBotShapeData);
            AssetDatabase.Refresh();
        }
    }
}
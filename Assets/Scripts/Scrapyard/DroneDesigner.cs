using StarSalvager.Values;
using StarSalvager.Factories;
using UnityEngine;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.UI;
using StarSalvager.Utilities.SceneManagement;
using UnityEngine.InputSystem;

using Recycling;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using Sirenix.OdinInspector;
using StarSalvager.UI.Scrapyard;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Utilities.UI;
using Input = StarSalvager.Utilities.Inputs.Input;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.Math;
using StarSalvager.Utilities.Saving;
using Object = System.Object;

namespace StarSalvager
{
    public class DroneDesigner : AttachableEditorToolBase, IReset, IInput, IHasHintElement
    {
        private DroneDesignUI DroneDesignUi
        {
            get
            {
                if (_droneDesignUi == null)
                    _droneDesignUi = FindObjectOfType<DroneDesignUI>();

                return _droneDesignUi;
            }
        }
        [SerializeField, Required]
        private DroneDesignUI _droneDesignUi;
        [SerializeField]
        private GameObject floatingPartWarningPrefab;
        [SerializeField]
        private GameObject availablePointMarkerPrefab;

        [NonSerialized]
        public bool IsUpgrading;

        private List<GameObject> _floatingPartWarnings;
        private List<GameObject> _availablePointMarkers;

        private Stack<ScrapyardEditData> _toUndoStack;
        private Stack<ScrapyardEditData> _toRedoStack;

        private ScrapyardLayout _currentLayout;

        public List<ScrapyardLayout> ScrapyardLayouts => _scrapyardLayouts;
        private List<ScrapyardLayout> _scrapyardLayouts;

        private bool _isStarted;
        private bool _isDragging;

        private SpriteRenderer _partDragImage;

        //============================================================================================================//

        #region Unity Functions

        // Start is called before the first frame update
        private void Start()
        {
            _floatingPartWarnings = new List<GameObject>();
            _availablePointMarkers = new List<GameObject>();
            _toUndoStack = new Stack<ScrapyardEditData>();
            _toRedoStack = new Stack<ScrapyardEditData>();
            _scrapyardLayouts = Files.ImportLayoutData();
            _currentLayout = null;
            IsUpgrading = false;
            
            InitInput();
            
            _isStarted = true;
        }

        private void Update()
        {
            if (_partDragImage == null || !_partDragImage.gameObject.activeSelf) 
                return;
            
            
            Vector3 screenToWorldPosition = Cameras.CameraController.Camera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            if (_isDragging || SelectedPartClickPosition != null && Vector3.Distance(SelectedPartClickPosition.Value, screenToWorldPosition) > 0.5f)
            {
                _isDragging = true;
                _partDragImage.transform.position = new Vector3(screenToWorldPosition.x, screenToWorldPosition.y, 0);
            }
        }

        private void OnDestroy()
        {
            DeInitInput();
        }

        #endregion //Unity Functions

        //============================================================================================================//

        #region Init

        public void InitInput()
        {
            //Input.Actions.Default.LeftClick.Enable();
            Input.Actions.MenuControls.LeftClick.performed += OnLeftMouseButton;

            //Input.Actions.Default.RightClick.Enable();
            Input.Actions.MenuControls.RightClick.performed += OnRightMouseButton;
        }

        public void DeInitInput()
        {
            //Input.Actions.Default.LeftClick.Disable();
            Input.Actions.MenuControls.LeftClick.performed -= OnLeftMouseButton;

            //Input.Actions.Default.RightClick.Disable();
            Input.Actions.MenuControls.RightClick.performed -= OnRightMouseButton;
        }

        #endregion

        //============================================================================================================//

        #region IReset Functions

        public void Activate()
        {
            GameManager.SetCurrentGameState(GameState.Scrapyard);
            
            GameTimer.SetPaused(true);

            SellBits();

            UpdateFloatingMarkers(false);
            
            /*DroneDesignUi.ShowRepairCost(GetRepairCost(), GetReplacementCost());*/
        }

        public void Reset()
        {
            if (SelectedBrick.HasValue && SelectedPartReturnToStorageIfNotPlaced)
            {
                BlockData blockData = SelectedBrick.Value;
                PlayerDataManager.AddPartToStorage(blockData);
            }

            SelectedBrick = null;
            //SelectedPartType = null;
            //SelectedPartLevel = 0;
            SelectedPartRemoveFromStorage = false;
            SelectedPartReturnToStorageIfNotPlaced = false;

            //Camera.onPostRender -= DrawGL;
            Globals.IsRecoveryBot = false;

            RecycleDrone();
        }

        #endregion //IReset Functions

        //============================================================================================================//

        #region User Input

        private void OnLeftMouseButton(InputAction.CallbackContext ctx)
        {
            if (ctx.control.IsPressed())
                OnLeftMouseButtonDown();
            else
                OnLeftMouseButtonUp();
        }

        private void OnLeftMouseButtonDown()
        {
            UpdateFloatingMarkers(SelectedBrick.HasValue);
            
            if (!TryGetMouseCoordinate(out Vector2Int mouseCoordinate))
                return;

            if (SelectedBrick.HasValue) 
                return;

            if (_scrapyardBot == null || mouseCoordinate == Vector2Int.zero) 
                return;
            
            IAttachable attachableAtCoordinates = _scrapyardBot.AttachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);

            if (attachableAtCoordinates == null ||
                !(attachableAtCoordinates is ScrapyardPart partAtCoordinates)) 
                return;
            var type = partAtCoordinates.Type;
                        
            Vector3 currentAttachablePosition = attachableAtCoordinates.transform.position;

            _scrapyardBot.TryRemoveAttachableAt(mouseCoordinate, false);

            SelectedBrick = partAtCoordinates.ToBlockData();
                        
            SelectedPartClickPosition = Cameras.CameraController.Camera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            SelectedPartPreviousGridPosition = mouseCoordinate;
            SelectedPartRemoveFromStorage = false;
            SelectedPartReturnToStorageIfNotPlaced = true;
            SaveBlockData();

            if (_partDragImage == null)
            {
                _partDragImage = new GameObject().AddComponent<SpriteRenderer>();
                _partDragImage.sortingOrder = 1;
            }
            _partDragImage.gameObject.SetActive(true);
            _partDragImage.sprite = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetProfileData(type).GetSprite();
            _partDragImage.transform.position = currentAttachablePosition;
        }

        private void OnLeftMouseButtonUp()
        {
            if (_partDragImage != null)
                _partDragImage.gameObject.SetActive(false);
            
            _isDragging = false;

            if (!SelectedBrick.HasValue || _scrapyardBot == null)
            {
                UpdateFloatingMarkers(false);
                return;
            }
            
            
            //Check if mouse coordinate is inside the editing grid
            if (!TryGetMouseCoordinate(out var mouseGridCoordinate))
            {
                Vector2 worldMousePosition = Cameras.CameraController.Camera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);

                //Move part back to previous location since drag position is inviable
                if(SelectedPartPreviousGridPosition != null)
                {
                    var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(SelectedBrick.Value);

                    //Check if part should be removed from storage
                    //TODO Should be checking if the player does in-fact have the part in their storage
                    if (SelectedPartRemoveFromStorage)
                    {
                        PlayerDataManager.RemovePartFromStorageAtIndex(SelectedIndex);
                    }

                    _scrapyardBot.AttachNewBit(SelectedPartPreviousGridPosition.Value, attachable);
                    DroneDesignUi.RefreshScrollViews();

                    SelectedBrick = null;
                    SelectedIndex = 0;
                    SelectedPartClickPosition = null;
                    SelectedPartPreviousGridPosition = null;
                    SelectedPartRemoveFromStorage = false;
                    SelectedPartReturnToStorageIfNotPlaced = false;
                        
                    SaveBlockData();
                }

                UpdateFloatingMarkers(false);
                return;
            }

            IAttachable attachableAtCoordinates = _scrapyardBot.AttachedBlocks.GetAttachableAtCoordinates(mouseGridCoordinate);
            //Check if there mouse coordinates are empty
            if (attachableAtCoordinates == null)
            {
                var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(SelectedBrick.Value);

                var blockData = SelectedBrick.Value;
                
                _scrapyardBot.AttachNewBit(mouseGridCoordinate, attachable);
                
                //Check if part should be removed from storage
                //TODO Should be checking if the player does in-fact have the part in their storage
                if (SelectedPartRemoveFromStorage)
                {
                    blockData.Coordinate = mouseGridCoordinate;

                    PlayerDataManager.RemovePartFromStorageAtIndex(SelectedIndex);

                    _toUndoStack.Push(new ScrapyardEditData
                    {
                        EventType = SCRAPYARD_ACTION.EQUIP,
                        BlockData = blockData
                    });
                    _toRedoStack.Clear();
                }
                else
                {
                    blockData.Coordinate = SelectedPartPreviousGridPosition.Value;
                    
                    _toUndoStack.Push(new ScrapyardEditData
                    {
                        EventType = SCRAPYARD_ACTION.RELOCATE,
                        Destination = mouseGridCoordinate,
                        BlockData = blockData
                    });
                    _toRedoStack.Clear();
                }

                
                DroneDesignUi.RefreshScrollViews();

                SelectedBrick = null;
                SelectedIndex = 0;
                SelectedPartClickPosition = null;
                SelectedPartPreviousGridPosition = null;
                SelectedPartRemoveFromStorage = false;
                SelectedPartReturnToStorageIfNotPlaced = false;
                SaveBlockData();
            }
            //If there is an attachable at location
            else if (SelectedPartPreviousGridPosition != null)
            {
                //Return object to previous location on bot
                var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(SelectedBrick.Value);

                //Check if part should be removed from storage
                //TODO Should be checking if the player does in-fact have the part in their storage
                if (SelectedPartRemoveFromStorage)
                {
                    PlayerDataManager.RemovePartFromStorageAtIndex(SelectedIndex);
                }

                _scrapyardBot.AttachNewBit(SelectedPartPreviousGridPosition.Value, attachable);

                DroneDesignUi.RefreshScrollViews();


                SelectedBrick = null;
                SelectedIndex = 0;
                SelectedPartPreviousGridPosition = null;
                SelectedPartRemoveFromStorage = false;
                SelectedPartReturnToStorageIfNotPlaced = false;
                SaveBlockData();
            }
            else if (SelectedPartPreviousGridPosition == null && attachableAtCoordinates != null)
            {
                SelectedBrick = null;
                return;
            }
            
            
            UpdateFloatingMarkers(false);
        }

        private void OnRightMouseButton(InputAction.CallbackContext ctx)
        {
            if (ctx.control.IsPressed())
                OnRightMouseButtonDown();
            else
                OnRightMouseButtonUp();
        }

        private void OnRightMouseButtonDown()
        {
            if (!TryGetMouseCoordinate(out Vector2Int mouseCoordinate))
                return;

            if (_scrapyardBot == null)
            {
                UpdateFloatingMarkers(false);
                return;
            }
            
            IAttachable attachableAtCoordinates = _scrapyardBot.AttachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);

            if (attachableAtCoordinates is ScrapyardPart scrapPart)
            {
                //Don't want to be able to remove the core
                if (scrapPart.Type == PART_TYPE.CORE)
                    return;
                
                var blockData = scrapPart.ToBlockData();
                blockData.Coordinate = mouseCoordinate;
                _scrapyardBot.TryRemoveAttachableAt(mouseCoordinate, false);

                
                PlayerDataManager.AddPartToStorage(scrapPart.ToBlockData());
                _toUndoStack.Push(new ScrapyardEditData
                {
                    EventType = SCRAPYARD_ACTION.UNEQUIP,
                    BlockData = blockData
                });
                _toRedoStack.Clear();

                SaveBlockData();
            }
            
            UpdateFloatingMarkers(false);
            /*DroneDesignUi.ShowRepairCost(GetRepairCost(), GetReplacementCost());*/
        }

        private void OnRightMouseButtonUp()
        {
        }

        #endregion //User Input

        //============================================================================================================//

        #region Undo/Redo Stack

        public void UndoStackPop()
        {
            if (_toUndoStack.Count == 0 || _scrapyardBot == null)
                return;

            ScrapyardEditData toUndo = _toUndoStack.Pop();

            var undoBlockData = toUndo.BlockData;
            var partType = (PART_TYPE) undoBlockData.Type;

            ScrapyardPart attachable;

            switch (toUndo.EventType)
            {
                case SCRAPYARD_ACTION.EQUIP:
                    PlayerDataManager.AddPartToStorage
                        (_scrapyardBot.AttachedBlocks.OfType<ScrapyardPart>().FirstOrDefault(a => a.Coordinate == undoBlockData.Coordinate).ToBlockData());
                    _scrapyardBot.TryRemoveAttachableAt(undoBlockData.Coordinate, false);
                    break;
                case SCRAPYARD_ACTION.UNEQUIP:
                    attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(partType);
                    PlayerDataManager.RemovePartFromStorageAtIndex(SelectedIndex);
                    _scrapyardBot.AttachNewBit(undoBlockData.Coordinate, attachable);
                    break;
                case SCRAPYARD_ACTION.RELOCATE:
                    attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(partType);
                    _scrapyardBot.TryRemoveAttachableAt(toUndo.Destination, false);
                    _scrapyardBot.AttachNewBit(undoBlockData.Coordinate, attachable);
                    break;
                case SCRAPYARD_ACTION.DISMANTLE_FROM_STORAGE:
                    PlayerDataManager.SubtractPartCosts(partType);
                    PlayerDataManager.AddPartToStorage(undoBlockData);
                    break;
                case SCRAPYARD_ACTION.DISMANTLE_FROM_BOT:
                    attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(partType);
                    PlayerDataManager.SubtractPartCosts(partType);
                    _scrapyardBot.AttachNewBit(undoBlockData.Coordinate, attachable);
                    break;
                case SCRAPYARD_ACTION.ROTATE:
                    RotateBots(-toUndo.Value, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(toUndo.EventType), toUndo.EventType, null);
            }

            DroneDesignUi.RefreshScrollViews();
            SaveBlockData();

            UpdateFloatingMarkers(false);
            _toRedoStack.Push(toUndo);
        }

        public void RedoStackPop()
        {
            if (_toRedoStack.Count == 0 || _scrapyardBot == null)
                return;

            ScrapyardEditData toRedo = _toRedoStack.Pop();
            
            
            var redoBlockData = toRedo.BlockData;
            var partType = (PART_TYPE) redoBlockData.Type;

            ScrapyardPart attachable;

            switch (toRedo.EventType)
            {
                case SCRAPYARD_ACTION.EQUIP:
                    attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(partType);
                    PlayerDataManager.RemovePartFromStorageAtIndex(SelectedIndex);
                    _scrapyardBot.AttachNewBit(redoBlockData.Coordinate, attachable);
                    break;
                case SCRAPYARD_ACTION.UNEQUIP:
                    PlayerDataManager.AddPartToStorage
                        (_scrapyardBot.AttachedBlocks.OfType<ScrapyardPart>().FirstOrDefault(a => a.Coordinate == redoBlockData.Coordinate).ToBlockData());
                    _scrapyardBot.TryRemoveAttachableAt(redoBlockData.Coordinate, false);
                    break;
                case SCRAPYARD_ACTION.RELOCATE:
                    attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(partType);
                    _scrapyardBot.TryRemoveAttachableAt(redoBlockData.Coordinate, false);
                    _scrapyardBot.AttachNewBit(toRedo.Destination, attachable);
                    break;
                case SCRAPYARD_ACTION.DISMANTLE_FROM_STORAGE:
                    PlayerDataManager.AddPartResources(partType, redoBlockData.Level, true);
                    PlayerDataManager.RemovePartFromStorageAtIndex(SelectedIndex);
                    break;
                case SCRAPYARD_ACTION.DISMANTLE_FROM_BOT:
                    PlayerDataManager.AddPartResources(partType, redoBlockData.Level, true);
                    _scrapyardBot.TryRemoveAttachableAt(redoBlockData.Coordinate, false);
                    break;
                case SCRAPYARD_ACTION.ROTATE:
                    RotateBots(toRedo.Value, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(toRedo.EventType), toRedo.EventType, null);
            }

            DroneDesignUi.RefreshScrollViews();
            SaveBlockData();

            UpdateFloatingMarkers(false);
            _toUndoStack.Push(toRedo);
        }

        public void ClearUndoRedoStacks()
        {
            if (!_isStarted)
                return;

            _toUndoStack.Clear();
            _toRedoStack.Clear();
        }

        #endregion //Stack

        //============================================================================================================//

        #region Save/Load Layouts

        public void SaveLayout(string layoutName)
        {
            ScrapyardLayout saveLayout = _scrapyardLayouts.FirstOrDefault(l => l.Name == layoutName);
            if (saveLayout != null)
            {
                saveLayout = new ScrapyardLayout(layoutName, _scrapyardBot.GetBlockDatas());
            }
            else
            {
                _scrapyardLayouts.Add(new ScrapyardLayout(layoutName, _scrapyardBot.GetBlockDatas()));
            }
            Files.ExportLayoutData(_scrapyardLayouts);
        }

        public void LoadLayout(string name)
        {
            var tempLayout = _scrapyardLayouts.First(l => l.Name == name);

            if (tempLayout == null)
                return;

            //Setup your list of available parts by adding storage and parts on bot together into a temp list
            List<BlockData> partComparer = new List<BlockData>();
            partComparer.AddRange(PlayerDataManager.GetCurrentPartsInStorage());
            foreach (var attachable in _scrapyardBot.AttachedBlocks)
            {
                if (attachable is ScrapyardPart part && part.Type != (int)PART_TYPE.CORE)
                {
                    partComparer.Add(part.ToBlockData());
                }
            }

            //Setup your list of available resources by putting player resources into a temp list
            Dictionary<BIT_TYPE, int> resourceComparer = new Dictionary<BIT_TYPE, int>();
            foreach (BIT_TYPE _bitType in Enum.GetValues(typeof(BIT_TYPE)))
            {
                if (_bitType == BIT_TYPE.WHITE || _bitType == BIT_TYPE.NONE)
                    continue;

                resourceComparer.Add(_bitType, PlayerDataManager.GetResource(_bitType).resource);
            }
            //Setup your list of available resources by putting player resources into a temp list
            Dictionary<COMPONENT_TYPE, int> componentComparer = new Dictionary<COMPONENT_TYPE, int>((IDictionary<COMPONENT_TYPE, int>) PlayerDataManager.GetComponents());

            //Setup your list of parts needing to be purchasing by comparing the list of parts in the layout to the list of available parts.
            List<BlockData> newLayoutComparer = new List<BlockData>();
            newLayoutComparer.AddRange(tempLayout.BlockData);

            for (int i = newLayoutComparer.Count - 1; i >= 0; i--)
            {
                if (partComparer.Any(b => b.Equals(newLayoutComparer[i])))
                {
                    //BlockData dataToRemove = newLayoutComparer[i];
                    partComparer.Remove(partComparer.FirstOrDefault(b => b.Equals(newLayoutComparer[i])));
                    newLayoutComparer.Remove(newLayoutComparer[i]);
                }
            }


            //Check if you have the resources available to afford the parts you need to purchase.
            foreach (var partData in newLayoutComparer)
            {
                if (partData.Type == (int)PART_TYPE.CORE)
                    continue;

                if (PlayerDataManager.CanAffordPart((PART_TYPE)partData.Type))
                {
                    PlayerDataManager.SubtractPartCosts((PART_TYPE)partData.Type);
                }
                else
                {
                    //CANT AFFORD LAYOUT
                    Debug.Log("CANT AFFORD LAYOUT");
                    return;
                }
            }

            //Swap to new layout
            _currentLayout = tempLayout;
            foreach (BIT_TYPE _bitType in Enum.GetValues(typeof(BIT_TYPE)))
            {
                if (_bitType == BIT_TYPE.WHITE || _bitType == BIT_TYPE.NONE)
                    continue;

                PlayerDataManager.GetResource(_bitType).SetResource(resourceComparer[_bitType]);
            }
            PlayerDataManager.SetCurrentPartsInStorage(partComparer);

            for (int i = _scrapyardBot.AttachedBlocks.Count - 1; i >= 0; i--)
            {
                if (_scrapyardBot.AttachedBlocks[i].Coordinate != Vector2Int.zero)
                {
                    _scrapyardBot.TryRemoveAttachableAt(_scrapyardBot.AttachedBlocks[i].Coordinate, false);
                }
            }

            foreach (var attachable in tempLayout.BlockData.ImportBlockDatas(true))
            {
                _scrapyardBot.AttachNewBit(attachable.Coordinate, attachable);
            }
            DroneDesignUi.UpdateBotResourceElements();
            DroneDesignUi.RefreshScrollViews();
            SaveBlockData();
        }

        #endregion //Layouts

        //============================================================================================================//

        #region Save Data

        //Save the current bot's data in blockdata to be loaded in the level manager.
        //Keep an eye on this - currently it will update the player block data each time there is a change
        public void SaveBlockData()
        {
            if (_scrapyardBot != null)
            {
                PlayerDataManager.SetBlockData(_scrapyardBot.AttachedBlocks.GetBlockDatas());
            }
        }



        #endregion

        //============================================================================================================//

        #region Other

        public void SetupDrone()
        {
            if (_scrapyardBot != null)
            {
                return;
            }

            _scrapyardBot = FactoryManager.Instance.GetFactory<BotFactory>().CreateScrapyardObject<ScrapyardBot>();

            var currentBlockData = PlayerDataManager.GetBlockDatas();
            //Checks to make sure there is a core on the bot
            if (currentBlockData.Count == 0 || !currentBlockData.Any(x => x.ClassType.Contains(nameof(Part)) && x.Type == (int)PART_TYPE.CORE))
            {
                _scrapyardBot.InitBot();
            }
            else
            {
                var importedData = currentBlockData.ImportBlockDatas(true);
                _scrapyardBot.InitBot(importedData);
            }
        }

        public void RecycleDrone()
        {
            if (_scrapyardBot == null)
            {
                return;
            }

            Recycling.Recycler.Recycle<ScrapyardBot>(_scrapyardBot.gameObject);
            _scrapyardBot = null;
        }

        public void ToggleDrones()
        {
            if (!IsFullyConnected())
            {
                Alert.ShowAlert("Alert!",
                    "A disconnected piece is active on your Bot! Please repair before continuing", "Fix",
                    () =>
                    {
                        /*ShowMenu(MENU.DESIGN);*/
                    });

                return;
            }

            SaveBlockData();

            if (_scrapyardBot != null)
            {
                Recycling.Recycler.Recycle<ScrapyardBot>(_scrapyardBot.gameObject);
                _scrapyardBot = null;
            }

            _scrapyardBot = FactoryManager.Instance.GetFactory<BotFactory>().CreateScrapyardObject<ScrapyardBot>();

            Globals.IsRecoveryBot = !Globals.IsRecoveryBot;
            List<BlockData> currentBlockData = PlayerDataManager.GetBlockDatas();

            //Checks to make sure there is a core on the bot
            if (currentBlockData.Count == 0 || !currentBlockData.Any(x => x.ClassType.Contains(nameof(Part)) && x.Type == (int)PART_TYPE.CORE))
            {
                _scrapyardBot.InitBot();
            }
            else
            {
                var importedData = currentBlockData.ImportBlockDatas(true);
                _scrapyardBot.InitBot(importedData);
            }

            UpdateFloatingMarkers(false);
        }

        //Sell Bits & Components
        //============================================================================================================//

        #region Sell Bits & Components

        private void SellBits()
        {
            var bitAttachableFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();

            //Obtain the block data from both the Recovery Drone & Drone
            //--------------------------------------------------------------------------------------------------------//
            
            Globals.IsRecoveryBot = true;
            //Get the Recovery Drone data & clean it
            var recoveryDroneBlockData = new List<BlockData>(PlayerDataManager.GetBlockDatas());
            PlayerDataManager.SetBlockData(recoveryDroneBlockData.Where(x => x.ClassType.Equals(nameof(Part)) || x.ClassType.Equals(nameof(ScrapyardPart))).ToList());
            
            Globals.IsRecoveryBot = false;
            
            //Get the active Drone Data & clean it
            var droneBlockData = new List<BlockData>(PlayerDataManager.GetBlockDatas());
            PlayerDataManager.SetBlockData(droneBlockData.Where(x => x.ClassType.Equals(nameof(Part)) || x.ClassType.Equals(nameof(ScrapyardPart))).ToList());
            
            //--------------------------------------------------------------------------------------------------------//

            //Only get all the things that aren't parts from the two bots
            List<BlockData> botBlockData = recoveryDroneBlockData
                .Where(x => !x.ClassType.Equals(nameof(Part)))
                .Concat(droneBlockData.Where(x => !x.ClassType.Equals(nameof(Part))))
                .ToList();

            //If we have nothing to process, don't bother moving forward
            if (botBlockData.Count == 0)
                return;
                
            var processedResources = new Dictionary<BIT_TYPE, int>();
            var wastedResources = new Dictionary<BIT_TYPE, int>();

            foreach (var blockData in botBlockData)
            {
                int amount;
                switch (blockData.ClassType)
                {
                    //------------------------------------------------------------------------------------------------//
                    case nameof(Component):
                        amount = 1;
                        var componentType = (COMPONENT_TYPE) blockData.Type;

                        if (blockData.Level > 0)
                            amount = blockData.Level * 3;

                        PlayerDataManager.AddComponent(componentType, amount, false);
                        break;
                    //------------------------------------------------------------------------------------------------//
                    case nameof(Bit):
                    case nameof(ScrapyardBit):
                        var bitType = (BIT_TYPE) blockData.Type;

                        amount = bitAttachableFactory.GetTotalResource(bitType, blockData.Level);

                        var addResourceAmount =  amount;
                        PlayerDataManager.GetResource(bitType)
                            .AddResourceReturnWasted(
                                addResourceAmount, 
                                out var wastedResource, 
                                false);

                        TryIncrementDict(bitType, amount, ref processedResources);
                        TryIncrementDict(bitType, wastedResource, ref wastedResources);


                        break;
                    //------------------------------------------------------------------------------------------------//
                    case nameof(Crate):
                        int numCrates = 1;
                        for (int i = 0; i < blockData.Level; i++)
                        {
                            numCrates *= 3;
                        }
                        for (int c = 0; c < numCrates; c++)
                        {
                            List<IRDSObject> loot = FactoryManager.Instance.GetFactory<CrateFactory>().GetCrateLoot();
                            for (int i = loot.Count - 1; i >= 0; i--)
                            {
                                switch (loot[i])
                                {
                                    case RDSValue<(BIT_TYPE, int)> rdsValueResourceRefined:
                                        PlayerDataManager.GetResource(rdsValueResourceRefined.rdsValue.Item1).AddResource(rdsValueResourceRefined.rdsValue.Item2);
                                        loot.RemoveAt(i);
                                        break;
                                    case RDSValue<Blueprint> rdsValueBlueprint:
                                        PlayerDataManager.UnlockBlueprint(rdsValueBlueprint.rdsValue);
                                        Toast.AddToast("Unlocked Blueprint!");
                                        loot.RemoveAt(i);
                                        break;
                                    case RDSValue<Vector2Int> rdsValueGears:
                                        {
                                            var gears = UnityEngine.Random.Range(rdsValueGears.rdsValue.x, rdsValueGears.rdsValue.y);
                                            PlayerDataManager.ChangeGears(gears);
                                            loot.RemoveAt(i);
                                            break;
                                        }
                                    case RDSValue<BlockData> rdsValueBlockData:
                                        {
                                            if (!GameManager.IsState(GameState.LEVEL_ACTIVE))
                                            {
                                                switch (rdsValueBlockData.rdsValue.ClassType)
                                                {
                                                    case nameof(Component):
                                                        PlayerDataManager.AddComponent((COMPONENT_TYPE)rdsValueBlockData.rdsValue.Type, 1);
                                                        loot.RemoveAt(i);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                        break;
                }
            }
            
            //Update all relevant parties
            PlayerDataManager.OnValuesChanged?.Invoke();
            DroneDesignUi.UpdateBotResourceElements();
            
            //Show the final alert to the player
            ShowAlertInfo(botBlockData, processedResources, wastedResources);
        }

        

        private static void ShowAlertInfo(IEnumerable<BlockData> botBlockDatas, Dictionary<BIT_TYPE, int> processedResources, Dictionary<BIT_TYPE, int> wastedResources)
        {
            if (processedResources.IsNullOrEmpty() || wastedResources.IsNullOrEmpty())
                return;
            
            var bits = botBlockDatas
                .Where(x => x.ClassType.Equals(nameof(Bit)) || x.ClassType.Equals(nameof(ScrapyardBit))).ToArray();
            
            BitAttachableFactory bitAttachableFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();
            
            
            string resourcesGained = string.Empty;
            foreach (var kvp in processedResources)
            {
                var bitType = kvp.Key;
                int numTotal = bits.Count(b => b.Type == (int)bitType);

                for (int i = 0; numTotal > 0; i++)
                {
                    int numAtLevel = bits.Count(b => (BIT_TYPE)b.Type == bitType && b.Level == i);
                    if (numAtLevel == 0)
                        continue;

                    var remoteData = bitAttachableFactory.GetBitRemoteData(bitType);

                    int resourceAmount = numAtLevel * remoteData.levels[i].resources;

                    var spriteIcon = TMP_SpriteMap.GetBitSprite(bitType, i);
                    var materialIcon = TMP_SpriteMap.MaterialIcons[bitType];
                    
                    resourcesGained += $"{numAtLevel} x {spriteIcon} = {resourceAmount} {materialIcon} ";
                    numTotal -= numAtLevel;
                }

                resourcesGained += "\n";
            }

            string resourcesWasted = string.Empty;
            foreach (var resource in wastedResources)
            {
                if(resource.Value <= 0)
                    continue;

                var materialIcon = TMP_SpriteMap.MaterialIcons[resource.Key];
                
                resourcesWasted += $"{resource.Value} {materialIcon} jettisoned due to lack of storage\n";
            }

            var body = $"{resourcesGained}{resourcesWasted}";

            if (string.IsNullOrEmpty(body))
                return;

            Alert.ShowAlert("Resources Refined", body, "Okay", null);
            Alert.SetLineHeight(90f);
        }
        
        private static void TryIncrementDict<TE>(TE type, int amount, ref Dictionary<TE, int> dictionary)
        {
            if (!dictionary.ContainsKey(type))
                dictionary.Add(type, amount);
            else
            {
                dictionary[type] += amount;
            }
        }

        #endregion //Sell Bits & Components

        //====================================================================================================================//
        
        public void RotateBots(float direction, bool pushToUndoStack = true)
        {
            if (_scrapyardBot != null)
            {
                _scrapyardBot.Rotate(direction);
                if (pushToUndoStack)
                {
                    _toUndoStack.Push(new ScrapyardEditData
                    {
                        EventType = SCRAPYARD_ACTION.ROTATE,
                        Value = direction
                    });
                }
            }
        }

        public bool HasPart(PART_TYPE partType)
        {
            return _scrapyardBot.AttachedBlocks.OfType<ScrapyardPart>().Any(p => p.Type == partType);
        }

        public bool HasParts(params PART_TYPE[] partTypes)
        {
            return _scrapyardBot.AttachedBlocks.OfType<ScrapyardPart>().Any(p => partTypes.Contains(p.Type));
        }

        public void UpdateFloatingMarkers(bool showAvailable)
        {
            foreach (var availablePoint in _availablePointMarkers)
            {
                Recycler.Recycle(ICONS.AVAILABLE, availablePoint);
            }
            _availablePointMarkers.Clear();

            if (showAvailable && _scrapyardBot != null)
            {
                foreach (var attached in _scrapyardBot.AttachedBlocks)
                {
                    if (!_scrapyardBot.AttachedBlocks.HasPathToCore(attached))
                        continue;

                    if (_scrapyardBot.AttachedBlocks.FindAll(a => a.Coordinate == attached.Coordinate + Vector2.left && _scrapyardBot.AttachedBlocks.HasPathToCore(a)).Count == 0)
                    {
                        if (!Recycler.TryGrab(ICONS.AVAILABLE, out GameObject availableMarker))
                            availableMarker = GameObject.Instantiate(availablePointMarkerPrefab);
                        availableMarker.transform.position = (Vector3)(attached.Coordinate + Vector2.left) * Constants.gridCellSize + Vector3.back;
                        _availablePointMarkers.Add(availableMarker);
                    }
                    if (_scrapyardBot.AttachedBlocks.FindAll(a => a.Coordinate == attached.Coordinate + Vector2.right && _scrapyardBot.AttachedBlocks.HasPathToCore(a)).Count == 0)
                    {
                        if (!Recycler.TryGrab(ICONS.AVAILABLE, out GameObject availableMarker))
                            availableMarker = GameObject.Instantiate(availablePointMarkerPrefab);
                        availableMarker.transform.position = (Vector3)(attached.Coordinate + Vector2.right) * Constants.gridCellSize + Vector3.back;
                        _availablePointMarkers.Add(availableMarker);
                    }
                    if (_scrapyardBot.AttachedBlocks.FindAll(a => a.Coordinate == attached.Coordinate + Vector2.up && _scrapyardBot.AttachedBlocks.HasPathToCore(a)).Count == 0)
                    {
                        if (!Recycler.TryGrab(ICONS.AVAILABLE, out GameObject availableMarker))
                            availableMarker = GameObject.Instantiate(availablePointMarkerPrefab);
                        availableMarker.transform.position = (Vector3)(attached.Coordinate + Vector2.up) * Constants.gridCellSize + Vector3.back;
                        _availablePointMarkers.Add(availableMarker);
                    }
                    if (_scrapyardBot.AttachedBlocks.FindAll(a => a.Coordinate == attached.Coordinate + Vector2.down && _scrapyardBot.AttachedBlocks.HasPathToCore(a)).Count == 0)
                    {
                        if (!Recycler.TryGrab(ICONS.AVAILABLE, out GameObject availableMarker))
                            availableMarker = GameObject.Instantiate(availablePointMarkerPrefab);
                        availableMarker.transform.position = (Vector3)(attached.Coordinate + Vector2.down) * Constants.gridCellSize + Vector3.back;
                        _availablePointMarkers.Add(availableMarker);
                    }
                }
            }

            foreach (var partWarning in _floatingPartWarnings)
            {
                Recycler.Recycle(ICONS.ALERT, partWarning);
            }
            _floatingPartWarnings.Clear();

            if (_scrapyardBot != null)
            {
                foreach (var attached in _scrapyardBot.AttachedBlocks)
                {
                    if (!_scrapyardBot.AttachedBlocks.HasPathToCore(attached))
                    {
                        if (!Recycler.TryGrab(ICONS.ALERT, out GameObject newWarning))
                            newWarning = GameObject.Instantiate(floatingPartWarningPrefab);
                        newWarning.transform.position = (Vector3)((Vector2)attached.Coordinate * Constants.gridCellSize) + Vector3.back;
                        _floatingPartWarnings.Add(newWarning);
                    }
                }
            }
        }

        public bool IsFullyConnected()
        {
            return _scrapyardBot == null || !_scrapyardBot.CheckHasDisconnects();
        }

        public void ProcessScrapyardUsageEndAnalytics()
        {
            Dictionary<string, object> scrapyardUsageEndAnalyticsDictionary = new Dictionary<string, object>();
            //AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.ScrapyardUsageEnd, scrapyardUsageEndAnalyticsDictionary);
        }

        #endregion //Other

        //============================================================================================================//
        public object[] GetHintElements(HINT hint)
        {
            switch (hint)
            {
                case HINT.NONE:
                    return default;
                case HINT.DAMAGE:
                    throw new Exception("Unhandled case for HINT.DAMAGE hover in DroneDesigner");
                    /*return new object[]
                    {
                        _repairHover.bounds
                    };*/
                default:
                    throw new ArgumentOutOfRangeException(nameof(hint), hint, null);
            }
        }
    }
}

﻿using StarSalvager.Values;
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
using StarSalvager.Missions;
using StarSalvager.Utilities.Math;

namespace StarSalvager
{
    public class DroneDesigner : AttachableEditorToolBase, IReset, IInput
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
        [SerializeField]
        private SpriteRenderer dismantleBinPrefab;

        [NonSerialized]
        public bool IsUpgrading;

        private List<GameObject> _floatingPartWarnings;
        private List<GameObject> _availablePointMarkers;
        private SpriteRenderer _dismantleBin;

        private Stack<ScrapyardEditData> _toUndoStack;
        private Stack<ScrapyardEditData> _toRedoStack;

        private ScrapyardLayout _currentLayout;

        public List<ScrapyardLayout> ScrapyardLayouts => _scrapyardLayouts;
        private List<ScrapyardLayout> _scrapyardLayouts;

        private bool _isStarted;
        private bool _isDragging;

        public bool IsEditingRecoveryDrone => _isEditingRecoveryDrone;
        private bool _isEditingRecoveryDrone;

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
            if (_isDragging || (SelectedPartClickPosition != null && Vector3.Distance(SelectedPartClickPosition.Value, screenToWorldPosition) > 0.5f))
            {
                _isDragging = true;
                _partDragImage.transform.position = new Vector3(screenToWorldPosition.x, screenToWorldPosition.y, 0);
            }
        }

        private void OnDestroy()
        {
            Camera.onPostRender -= DrawGL;

            DeInitInput();
        }

        #endregion //Unity Functions

        //============================================================================================================//

        #region Init

        public void InitInput()
        {
            Input.Actions.Default.LeftClick.Enable();
            Input.Actions.Default.LeftClick.performed += OnLeftMouseButton;

            Input.Actions.Default.RightClick.Enable();
            Input.Actions.Default.RightClick.performed += OnRightMouseButton;
        }

        public void DeInitInput()
        {
            Input.Actions.Default.LeftClick.Disable();
            Input.Actions.Default.LeftClick.performed -= OnLeftMouseButton;

            Input.Actions.Default.RightClick.Disable();
            Input.Actions.Default.RightClick.performed -= OnRightMouseButton;
        }

        #endregion

        //============================================================================================================//

        #region IReset Functions

        public void Activate()
        {
            GameTimer.SetPaused(true);
            Camera.onPostRender += DrawGL;

            _scrapyardBot = FactoryManager.Instance.GetFactory<BotFactory>().CreateScrapyardObject<ScrapyardBot>();

            var currentBlockData = PlayerPersistentData.PlayerData.GetCurrentBlockData();
            //Checks to make sure there is a core on the bot
            if (currentBlockData.Count == 0 || !currentBlockData.Any(x => x.ClassType.Contains(nameof(Part)) && x.Type == (int)PART_TYPE.CORE))
            {
                _scrapyardBot.InitBot(_isEditingRecoveryDrone);
            }
            else
            {
                var importedData = currentBlockData.ImportBlockDatas(true);
                _scrapyardBot.InitBot(importedData, _isEditingRecoveryDrone);
            }

            bool outOfWaterOnReturn = PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE] <= 0;
            SellBits();
            //TODO Need to decide if this should happen at arrival or at launch
            //TryFillBotResources();

            if (PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE] <= 0)
            {
                Alert.ShowAlert("Game Over", "Your crew has died of thirst - Game Over. thx!", "Main Menu", () =>
                {
                    PlayerPersistentData.ClearPlayerData();
                    PlayerPersistentData.PlayerMetadata.CurrentSaveFile = null;
                    SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.SCRAPYARD);
                });
            }
            else if (outOfWaterOnReturn)
            {
                Alert.ShowAlert("Water Restored", "You have resuscitated your thirsty crew.", "Phew!", null);
            }

            if (_dismantleBin == null)
            {
                _dismantleBin = Instantiate(dismantleBinPrefab);
                _dismantleBin.transform.position = new Vector2(10, 10);
                _dismantleBin.transform.parent = transform;
            }

            UpdateFloatingMarkers(false);
            
            DroneDesignUi.ShowRepairCost(GetRepairCost(), GetReplacementCost());
        }

        public void Reset()
        {
            if (SelectedBrick.HasValue && SelectedPartReturnToStorageIfNotPlaced)
            {
                BlockData blockData = SelectedBrick.Value;
                PlayerPersistentData.PlayerData.AddPartToStorage(blockData);
            }

            SelectedBrick = null;
            //SelectedPartType = null;
            //SelectedPartLevel = 0;
            SelectedPartRemoveFromStorage = false;
            SelectedPartReturnToStorageIfNotPlaced = false;

            Camera.onPostRender -= DrawGL;
            _isEditingRecoveryDrone = false;

            if (_scrapyardBot != null)
            {
                Recycling.Recycler.Recycle<ScrapyardBot>(_scrapyardBot.gameObject);
                _scrapyardBot = null;
            }
        }

        #endregion //IReset Functions

        //============================================================================================================//

        #region User Input

        private void OnLeftMouseButton(InputAction.CallbackContext ctx)
        {
            if (ctx.ReadValue<float>() == 1f)
                OnLeftMouseButtonDown();
            else
                OnLeftMouseButtonUp();
        }

        private void OnLeftMouseButtonDown()
        {
            if (!TryGetMouseCoordinate(out Vector2Int mouseCoordinate))
                return;

            if (!SelectedBrick.HasValue)
            {
                if (_scrapyardBot != null && mouseCoordinate != Vector2Int.zero)
                {
                    IAttachable attachableAtCoordinates = _scrapyardBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);

                    if (attachableAtCoordinates != null && attachableAtCoordinates is ScrapyardPart partAtCoordinates)
                    {
                        var type = partAtCoordinates.Type;
                        var level = partAtCoordinates.level;
                        
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
                        _partDragImage.sprite = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetProfileData(type).Sprites[level];
                        _partDragImage.transform.position = currentAttachablePosition;
                    }
                }
            }
            UpdateFloatingMarkers(true);
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
                if (_dismantleBin != null)
                {
                    Vector2 worldMousePosition = Cameras.CameraController.Camera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);

                    //Dismantle part
                    if (Vector2.Distance(worldMousePosition, _dismantleBin.transform.position) <= 3)
                    {
                        var blockData = SelectedBrick.Value;
                        
                        Toast.AddToast("Dismantle part", verticalLayout: Toast.Layout.Start, horizontalLayout: Toast.Layout.Middle);
                        PlayerPersistentData.PlayerData.AddResources(SelectedBrick.Value, true);

                        //Dismantle part from storage
                        if (SelectedPartRemoveFromStorage)
                        {
                            PlayerPersistentData.PlayerData.RemovePartFromStorage(blockData);

                            _toUndoStack.Push(new ScrapyardEditData
                            {
                                EventType = SCRAPYARD_ACTION.DISMANTLE_FROM_STORAGE,
                                BlockData = blockData
                            });
                            _toRedoStack.Clear();
                        }
                        //Dismantle part from bot
                        else
                        {
                            blockData.Coordinate = SelectedPartPreviousGridPosition.Value;
                            _toUndoStack.Push(new ScrapyardEditData
                            {
                                EventType = SCRAPYARD_ACTION.DISMANTLE_FROM_BOT,
                                BlockData = blockData
                            });
                            _toRedoStack.Clear();
                        }

                        SelectedBrick = null;
                        SelectedPartPreviousGridPosition = null;
                        SelectedPartRemoveFromStorage = false;
                        SelectedPartReturnToStorageIfNotPlaced = false;
                        SaveBlockData();
                    }
                    //Move part back to previous location since drag position is inviable
                    else if(SelectedPartPreviousGridPosition != null)
                    {
                        var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(SelectedBrick.Value);

                        //Check if part should be removed from storage
                        //TODO Should be checking if the player does in-fact have the part in their storage
                        if (SelectedPartRemoveFromStorage)
                        {
                            PlayerPersistentData.PlayerData.RemovePartFromStorage(attachable.ToBlockData());
                        }

                        _scrapyardBot.AttachNewBit(SelectedPartPreviousGridPosition.Value, attachable);
                        DroneDesignUi.RefreshScrollViews();

                        SelectedBrick = null;
                        SelectedPartClickPosition = null;
                        SelectedPartPreviousGridPosition = null;
                        SelectedPartRemoveFromStorage = false;
                        SelectedPartReturnToStorageIfNotPlaced = false;
                        
                        SaveBlockData();
                        
                        
                    }
                }
                UpdateFloatingMarkers(false);
                return;
            }
        
            IAttachable attachableAtCoordinates = _scrapyardBot.attachedBlocks.GetAttachableAtCoordinates(mouseGridCoordinate);
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
                    
                    PlayerPersistentData.PlayerData.RemovePartFromStorage(blockData);

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
                    PlayerPersistentData.PlayerData.RemovePartFromStorage(attachable.ToBlockData());
                }

                _scrapyardBot.AttachNewBit(SelectedPartPreviousGridPosition.Value, attachable);

                DroneDesignUi.RefreshScrollViews();


                SelectedBrick = null;
                SelectedPartPreviousGridPosition = null;
                SelectedPartRemoveFromStorage = false;
                SelectedPartReturnToStorageIfNotPlaced = false;
                SaveBlockData();
            }
            
            
            UpdateFloatingMarkers(false);
            DroneDesignUi.ShowRepairCost(GetRepairCost(), GetReplacementCost());
        }

        private void OnRightMouseButton(InputAction.CallbackContext ctx)
        {
            if (ctx.ReadValue<float>() == 1f)
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
            
            IAttachable attachableAtCoordinates = _scrapyardBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);

            if (attachableAtCoordinates != null && attachableAtCoordinates is ScrapyardPart scrapPart)
            {
                var blockData = scrapPart.ToBlockData();
                blockData.Coordinate = mouseCoordinate;
                _scrapyardBot.TryRemoveAttachableAt(mouseCoordinate, false);

                
                PlayerPersistentData.PlayerData.AddPartToStorage(scrapPart.ToBlockData());
                DroneDesignUi.AddToPartScrollView(scrapPart.ToBlockData());
                _toUndoStack.Push(new ScrapyardEditData
                {
                    EventType = SCRAPYARD_ACTION.UNEQUIP,
                    BlockData = blockData
                });
                _toRedoStack.Clear();

                SaveBlockData();
            }
            
            UpdateFloatingMarkers(false);
            DroneDesignUi.ShowRepairCost(GetRepairCost(), GetReplacementCost());
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
                    PlayerPersistentData.PlayerData.AddPartToStorage
                        (_scrapyardBot.attachedBlocks.OfType<ScrapyardPart>().FirstOrDefault(a => a.Coordinate == undoBlockData.Coordinate).ToBlockData());
                    _scrapyardBot.TryRemoveAttachableAt(undoBlockData.Coordinate, false);
                    break;
                case SCRAPYARD_ACTION.UNEQUIP:
                    attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(partType, undoBlockData.Level);
                    PlayerPersistentData.PlayerData.RemovePartFromStorage(attachable.ToBlockData());
                    _scrapyardBot.AttachNewBit(undoBlockData.Coordinate, attachable);
                    break;
                case SCRAPYARD_ACTION.RELOCATE:
                    attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(partType, undoBlockData.Level);
                    _scrapyardBot.TryRemoveAttachableAt(toUndo.Destination, false);
                    _scrapyardBot.AttachNewBit(undoBlockData.Coordinate, attachable);
                    break;
                case SCRAPYARD_ACTION.DISMANTLE_FROM_STORAGE:
                    PlayerPersistentData.PlayerData.SubtractPartCosts(partType, undoBlockData.Level, true);
                    PlayerPersistentData.PlayerData.AddPartToStorage(undoBlockData);
                    break;
                case SCRAPYARD_ACTION.DISMANTLE_FROM_BOT:
                    attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(partType, undoBlockData.Level);
                    PlayerPersistentData.PlayerData.SubtractPartCosts(partType, undoBlockData.Level, true);
                    _scrapyardBot.AttachNewBit(undoBlockData.Coordinate, attachable);
                    break;
                case SCRAPYARD_ACTION.ROTATE:
                    RotateBots(-toUndo.Value, false);
                    break;
                default:
                    //Debug.LogError("Unhandled undo/redo stack case");
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
                    attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(partType, redoBlockData.Level);
                    PlayerPersistentData.PlayerData.RemovePartFromStorage(attachable.ToBlockData());
                    _scrapyardBot.AttachNewBit(redoBlockData.Coordinate, attachable);
                    break;
                case SCRAPYARD_ACTION.UNEQUIP:
                    PlayerPersistentData.PlayerData.AddPartToStorage
                        (_scrapyardBot.attachedBlocks.OfType<ScrapyardPart>().FirstOrDefault(a => a.Coordinate == redoBlockData.Coordinate).ToBlockData());
                    _scrapyardBot.TryRemoveAttachableAt(redoBlockData.Coordinate, false);
                    break;
                case SCRAPYARD_ACTION.RELOCATE:
                    attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(partType, redoBlockData.Level);
                    _scrapyardBot.TryRemoveAttachableAt(redoBlockData.Coordinate, false);
                    _scrapyardBot.AttachNewBit(toRedo.Destination, attachable);
                    break;
                case SCRAPYARD_ACTION.DISMANTLE_FROM_STORAGE:
                    PlayerPersistentData.PlayerData.AddResources(partType, redoBlockData.Level, true);
                    PlayerPersistentData.PlayerData.RemovePartFromStorage(redoBlockData);
                    break;
                case SCRAPYARD_ACTION.DISMANTLE_FROM_BOT:
                    PlayerPersistentData.PlayerData.AddResources(partType, redoBlockData.Level, true);
                    _scrapyardBot.TryRemoveAttachableAt(redoBlockData.Coordinate, false);
                    break;
                case SCRAPYARD_ACTION.ROTATE:
                    RotateBots(toRedo.Value, false);
                    break;
                default:
                    //Debug.LogError("Unhandled undo/redo stack case");
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
            partComparer.AddRange(PlayerPersistentData.PlayerData.GetCurrentPartsInStorage());
            foreach (var attachable in _scrapyardBot.attachedBlocks)
            {
                if (attachable is ScrapyardPart part && part.Type != (int)PART_TYPE.CORE)
                {
                    partComparer.Add(part.ToBlockData());
                }
            }

            //Setup your list of available resources by putting player resources into a temp list
            Dictionary<BIT_TYPE, int> resourceComparer = new Dictionary<BIT_TYPE, int>(PlayerPersistentData.PlayerData.resources);
            //Setup your list of available resources by putting player resources into a temp list
            Dictionary<COMPONENT_TYPE, int> componentComparer = new Dictionary<COMPONENT_TYPE, int>((IDictionary<COMPONENT_TYPE, int>) PlayerPersistentData.PlayerData.components);

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

                if (CostCalculations.CanAffordPart(resourceComparer, componentComparer, partComparer, (PART_TYPE)partData.Type, partData.Level, true))
                {
                    CostCalculations.SubtractPartCosts(ref resourceComparer, ref componentComparer, partComparer, (PART_TYPE)partData.Type, partData.Level, true);
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
            PlayerPersistentData.PlayerData.SetResources(resourceComparer);
            PlayerPersistentData.PlayerData.SetCurrentPartsInStorage(partComparer);

            for (int i = _scrapyardBot.attachedBlocks.Count - 1; i >= 0; i--)
            {
                if (_scrapyardBot.attachedBlocks[i].Coordinate != Vector2Int.zero)
                {
                    _scrapyardBot.TryRemoveAttachableAt(_scrapyardBot.attachedBlocks[i].Coordinate, false);
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
                if (_isEditingRecoveryDrone)
                {
                    PlayerPersistentData.PlayerData.SetRecoveryDroneBlockData(_scrapyardBot.attachedBlocks.GetBlockDatas());
                }
                else
                {
                    PlayerPersistentData.PlayerData.SetCurrentBlockData(_scrapyardBot.attachedBlocks.GetBlockDatas());
                }
            }
        }



        #endregion

        //============================================================================================================//

        #region Other

        public void ToggleDrones()
        {
            SaveBlockData();

            if (_scrapyardBot != null)
            {
                Recycling.Recycler.Recycle<ScrapyardBot>(_scrapyardBot.gameObject);
                _scrapyardBot = null;
            }

            _scrapyardBot = FactoryManager.Instance.GetFactory<BotFactory>().CreateScrapyardObject<ScrapyardBot>();

            List<BlockData> currentBlockData;

            if (_isEditingRecoveryDrone)
            {
                _isEditingRecoveryDrone = false;
                currentBlockData = PlayerPersistentData.PlayerData.GetCurrentBlockData();
            }
            else
            {
                _isEditingRecoveryDrone = true;
                currentBlockData = PlayerPersistentData.PlayerData.GetRecoveryDroneBlockData();
            }

            //Checks to make sure there is a core on the bot
            if (currentBlockData.Count == 0 || !currentBlockData.Any(x => x.ClassType.Contains(nameof(Part)) && x.Type == (int)PART_TYPE.CORE))
            {
                _scrapyardBot.InitBot(_isEditingRecoveryDrone);
            }
            else
            {
                var importedData = currentBlockData.ImportBlockDatas(true);
                _scrapyardBot.InitBot(importedData, _isEditingRecoveryDrone);
            }

            UpdateFloatingMarkers(false);
        }

        //============================================================================================================//

        private void SellBits()
        {
            if (_scrapyardBot == null)
                return;

            List<BlockData> recoveryBotBlockData = PlayerPersistentData.PlayerData.recoveryDroneBlockData;

            List<ScrapyardBit> listBits = _scrapyardBot.attachedBlocks.OfType<ScrapyardBit>().ToList();
            List<Component> listComponents = _scrapyardBot.attachedBlocks.OfType<Component>().ToList();

            for (int i = recoveryBotBlockData.Count - 1; i >= 0; i--)
            {
                if (recoveryBotBlockData[i].ClassType == "Bit")
                {
                    listBits.Add(FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateScrapyardObject<ScrapyardBit>(recoveryBotBlockData[i]));
                    recoveryBotBlockData.RemoveAt(i);
                    continue;
                }
                
                if (recoveryBotBlockData[i].ClassType == "Component")
                {
                    listComponents.Add(FactoryManager.Instance.GetFactory<ComponentAttachableFactory>().CreateObject<Component>((COMPONENT_TYPE)recoveryBotBlockData[i].Type, recoveryBotBlockData[i].Level));
                    recoveryBotBlockData.RemoveAt(i);
                    continue;
                }
            }

            if (listComponents.Count > 0)
            {
                _scrapyardBot.RemoveAllComponents();

                //TODO Need to think about if I should be displaying the components processed or not
                foreach (var component in listComponents)
                {
                    var amount = 1;

                    if (component.level > 0)
                        amount = component.level * 3;

                    PlayerPersistentData.PlayerData.AddComponent(component.Type, amount);

                    MissionProgressEventData missionProgressEventData = new MissionProgressEventData
                    {
                        componentType = component.Type,
                        intAmount = amount
                    };
                    MissionManager.ProcessMissionData(typeof(ComponentCollectedMission), missionProgressEventData);
                }

                PlayerData.OnValuesChanged?.Invoke();
                SaveBlockData();
            }


            if (listBits.Count == 0)
                return;

            //var scrapyardBits = _scrapyardBot.attachedBlocks.OfType<ScrapyardBit>();


            var enumerable = listBits.ToArray();
            Dictionary<BIT_TYPE, int> bits = FactoryManager.Instance.GetFactory<BitAttachableFactory>()
                .GetTotalResources(enumerable);

            float refineryMultiplier = 1.0f;
            if (PlayerPersistentData.PlayerData.facilityRanks.ContainsKey(FACILITY_TYPE.REFINERY))
            {
                int refineryRank = PlayerPersistentData.PlayerData.facilityRanks[FACILITY_TYPE.REFINERY];
                float increaseAmount = FactoryManager.Instance.FacilityRemote.GetRemoteData(FACILITY_TYPE.REFINERY)
                    .levels[refineryRank].increaseAmount;
                refineryMultiplier = 1 + (increaseAmount / 100);
                Debug.Log("REFINERY MULTIPLIER: " + refineryMultiplier);
            }

            Dictionary<BIT_TYPE, int> wastedResources = PlayerPersistentData.PlayerData.AddResourcesReturnWasted(bits, refineryMultiplier);


            string resourcesGained = "";
            foreach (var resource in bits)
            {
                int numTotal = enumerable.Count(b => b.Type == resource.Key);

                for (int i = 0; numTotal > 0; i++)
                {
                    int numAtLevel = enumerable.Count(b => b.Type == resource.Key && b.level == i);
                    if (numAtLevel == 0)
                        continue;

                    BitRemoteData remoteData = FactoryManager.Instance.GetFactory<BitAttachableFactory>()
                        .GetBitRemoteData(resource.Key);
                    
                    int resourceAmount = (int) (numAtLevel * remoteData.levels[i].resources * refineryMultiplier);

                    var spriteXML = TMP_SpriteMap.GetBitSprite(resource.Key, i);
                    
                    resourcesGained +=
                        $"{numAtLevel} x {spriteXML} = {resourceAmount} {TMP_SpriteMap.MaterialIcons[resource.Key]} ";
                    numTotal -= numAtLevel;
                }

                resourcesGained += "\n";
            }

            foreach (var resource in wastedResources)
            {
                resourcesGained += $"{resource.Value} {TMP_SpriteMap.MaterialIcons[resource.Key]} jettisoned due to lack of storage\n";
            }

            Alert.ShowAlert("Resources Refined", resourcesGained, "Okay", null);
            Alert.SetLineHeight(90f);

            
            for (int i = listBits.Count - 1; i >= 0; i--)
            {
                if (_scrapyardBot.attachedBlocks.Contains(listBits[i]))
                {
                    continue;
                }

                Recycler.Recycle<ScrapyardBit>(listBits[i]);
                listBits.RemoveAt(i);
            }

            _scrapyardBot.RemoveAllBits();



            SaveBlockData();

            DroneDesignUi.UpdateBotResourceElements();
        }

        //Repair Calculations
        //====================================================================================================================//
        
        #region Repair Calculations

        public int GetTotalRepairCost()
        {
            return GetRepairCost() + GetReplacementCost();
        }
        public Vector2Int GetRepairCostPair()
        {
            return new Vector2Int(GetRepairCost(), GetReplacementCost());
        }
        
        private int GetRepairCost()
        {
            if (_scrapyardBot == null)
                return 0;
            
            var repairCost = _scrapyardBot.attachedBlocks
                .OfType<ScrapyardPart>()
                .Where(x => !x.Destroyed)
                .Where(x => x.CurrentHealth < x.StartingHealth)
                .Sum(x => x.StartingHealth - x.CurrentHealth);

            return Mathf.RoundToInt(repairCost);
        }
        
        private int GetReplacementCost()
        {
            if (_scrapyardBot == null)
                return 0;

            var replacementCost = _scrapyardBot.attachedBlocks
                .OfType<ScrapyardPart>()
                .Where(x => x.Destroyed)
                .Sum(x => x.StartingHealth);

            return Mathf.RoundToInt(replacementCost);
        }
        
        public void RepairParts()
        {
            if (_scrapyardBot == null) 
                return;

            var damagedPartList = _scrapyardBot.attachedBlocks.OfType<ScrapyardPart>()
                .Where(x => x.CurrentHealth < x.StartingHealth)
                .OrderBy(x => x.StartingHealth - x.CurrentHealth)
                .ToList();

            //var totalRepairCost = GetRepairCost();
            var availableResources = PlayerPersistentData.PlayerData.resources[BIT_TYPE.GREEN];

            if (availableResources <= 0f)
            {
                return;
            }

            //TODO Order list by least to most damage
            foreach (var damagedPart in damagedPartList)
            {
                if (!(damagedPart is IHealth partHealth))
                    continue;

                var cost = Mathf.RoundToInt(damagedPart.StartingHealth - damagedPart.CurrentHealth);
                
                //Require the full cost if repairinng from destruction
                if (damagedPart.Destroyed)
                {
                    //No more money
                    if (availableResources - cost < 0)
                        break;
                    
                    availableResources -= cost;
                
                    partHealth.SetupHealthValues(damagedPart.StartingHealth, damagedPart.StartingHealth);
                }
                //Allow partial payment for partial recovery on damaged parts
                else
                {
                    //No more money
                    if (availableResources - cost < 0)
                        cost = availableResources;

                    if (cost == 0f)
                        break;
                    
                    availableResources -= cost;
                
                    partHealth.ChangeHealth(cost);
                }
                
                damagedPart.SetSprite(FactoryManager.Instance.PartsProfileData.GetProfile(damagedPart.Type)
                    .GetSprite(damagedPart.level));
            }
            PlayerPersistentData.PlayerData.resources[BIT_TYPE.GREEN] = availableResources;
            
            SaveBlockData();

            DroneDesignUi.UpdateBotResourceElements();
            DroneDesignUi.ShowRepairCost(GetRepairCost(), GetReplacementCost());
        }

        #endregion //Repair Calculations

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
            return _scrapyardBot.attachedBlocks.OfType<ScrapyardPart>().Any(p => p.Type == partType);
        }

        public bool HasParts(params PART_TYPE[] partTypes)
        {
            return _scrapyardBot.attachedBlocks.OfType<ScrapyardPart>().Any(p => partTypes.Contains(p.Type));
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
                foreach (var attached in _scrapyardBot.attachedBlocks)
                {
                    if (!_scrapyardBot.attachedBlocks.HasPathToCore(attached))
                        continue;

                    if (_scrapyardBot.attachedBlocks.FindAll(a => a.Coordinate == attached.Coordinate + Vector2.left && _scrapyardBot.attachedBlocks.HasPathToCore(a)).Count == 0)
                    {
                        if (!Recycler.TryGrab(ICONS.AVAILABLE, out GameObject availableMarker))
                            availableMarker = GameObject.Instantiate(availablePointMarkerPrefab);
                        availableMarker.transform.position = (Vector3)(attached.Coordinate + Vector2.left) * Constants.gridCellSize + Vector3.back;
                        _availablePointMarkers.Add(availableMarker);
                    }
                    if (_scrapyardBot.attachedBlocks.FindAll(a => a.Coordinate == attached.Coordinate + Vector2.right && _scrapyardBot.attachedBlocks.HasPathToCore(a)).Count == 0)
                    {
                        if (!Recycler.TryGrab(ICONS.AVAILABLE, out GameObject availableMarker))
                            availableMarker = GameObject.Instantiate(availablePointMarkerPrefab);
                        availableMarker.transform.position = (Vector3)(attached.Coordinate + Vector2.right) * Constants.gridCellSize + Vector3.back;
                        _availablePointMarkers.Add(availableMarker);
                    }
                    if (_scrapyardBot.attachedBlocks.FindAll(a => a.Coordinate == attached.Coordinate + Vector2.up && _scrapyardBot.attachedBlocks.HasPathToCore(a)).Count == 0)
                    {
                        if (!Recycler.TryGrab(ICONS.AVAILABLE, out GameObject availableMarker))
                            availableMarker = GameObject.Instantiate(availablePointMarkerPrefab);
                        availableMarker.transform.position = (Vector3)(attached.Coordinate + Vector2.up) * Constants.gridCellSize + Vector3.back;
                        _availablePointMarkers.Add(availableMarker);
                    }
                    if (_scrapyardBot.attachedBlocks.FindAll(a => a.Coordinate == attached.Coordinate + Vector2.down && _scrapyardBot.attachedBlocks.HasPathToCore(a)).Count == 0)
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
                foreach (var attached in _scrapyardBot.attachedBlocks)
                {
                    if (!_scrapyardBot.attachedBlocks.HasPathToCore(attached))
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
    }
}

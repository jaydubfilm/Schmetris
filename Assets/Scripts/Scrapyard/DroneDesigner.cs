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

using Input = StarSalvager.Utilities.Inputs.Input;
using Recycling;
using System.IO;
using Newtonsoft.Json;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using StarSalvager.UI.Scrapyard;
using UnityEngine.Serialization;
using StarSalvager.Factories.Data;

namespace StarSalvager
{
    public class DroneDesigner : AttachableEditorToolBase, IReset, IInput
    {
        [FormerlySerializedAs("scrapyardUI")] [SerializeField]
        private DroneDesignUI droneDesignUi;
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
        private SpriteRenderer dismantleBin;

        private Stack<ScrapyardEditData> _toUndoStack;
        private Stack<ScrapyardEditData> _toRedoStack;

        private ScrapyardLayout _currentLayout;

        public List<ScrapyardLayout> ScrapyardLayouts => _scrapyardLayouts;
        private List<ScrapyardLayout> _scrapyardLayouts;

        private bool isStarted = false;

        //============================================================================================================//

        #region Unity Functions

        // Start is called before the first frame update
        private void Start()
        {
            _floatingPartWarnings = new List<GameObject>();
            _availablePointMarkers = new List<GameObject>();
            _toUndoStack = new Stack<ScrapyardEditData>();
            _toRedoStack = new Stack<ScrapyardEditData>();
            _scrapyardLayouts = ImportRemoteData();
            _currentLayout = null;
            IsUpgrading = false;
            InitInput();
            isStarted = true;
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
                _scrapyardBot.InitBot();
            }
            else
            {
                var importedData = currentBlockData.ImportBlockDatas(true);
                _scrapyardBot.InitBot(importedData);
            }
            SellBits();
            TryFillBotResources();

            if (PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE] == 0)
            {
                Alert.ShowAlert("Game Over", "You have run out of water. Your crew has died of thirst.", "Main Menu", () =>
                {
                    PlayerPersistentData.PlayerData.numLives = 3;
                    SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.ALEX_TEST_SCENE);
                });
            }

            if (dismantleBin == null)
            {
                dismantleBin = GameObject.Instantiate(dismantleBinPrefab);
                dismantleBin.transform.position = new Vector2(10, 10);
                dismantleBin.gameObject.SetActive(false);
            }

            UpdateFloatingMarkers(false);
        }

        public void Reset()
        {
            if (SelectedPartType != null && SelectedPartReturnToStorageIfNotPlaced == true)
            {
                BlockData blockData = new BlockData
                {
                    ClassType = "Part",
                    Type = (int)SelectedPartType,
                    Level = SelectedPartLevel
                };
                PlayerPersistentData.PlayerData.AddPartToStorage(blockData);
            }

            SelectedPartType = null;
            SelectedPartLevel = 0;
            SelectedPartRemoveFromStorage = false;
            SelectedPartReturnToStorageIfNotPlaced = false;

            Camera.onPostRender -= DrawGL;

            if (_scrapyardBot != null)
            {
                Recycling.Recycler.Recycle<ScrapyardBot>(_scrapyardBot.gameObject);
                _scrapyardBot = null;
            }
        }

        #endregion //IReset Functions

        static readonly BIT_TYPE[] types = {
            BIT_TYPE.RED,
            BIT_TYPE.GREY,
            BIT_TYPE.GREEN
        };
        private void TryFillBotResources()
        {
            foreach (var bitType in types)
            {
                var currentAmount = PlayerPersistentData.PlayerData.liquidResource[bitType];
                var currentCapacity = PlayerPersistentData.PlayerData.liquidCapacity[bitType];

                var fillRemaining = currentCapacity - currentAmount;

                //If its already full, then we're good to move on
                if (fillRemaining <= 0f)
                    continue;

                var availableResources = PlayerPersistentData.PlayerData.resources[bitType];

                //If we have no resources available to refill the liquid, move onto the next
                if(availableResources <= 0)
                    continue;

                var movingAmount = Mathf.RoundToInt(Mathf.Min(availableResources, fillRemaining));

                PlayerPersistentData.PlayerData.resources[bitType] -= movingAmount;
                PlayerPersistentData.PlayerData.AddLiquidResource(bitType, movingAmount);
            }
        }

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
            if (dismantleBin != null)
                dismantleBin.gameObject.SetActive(true);
            
            if (!TryGetMouseCoordinate(out Vector2Int mouseCoordinate))
                return;

            if (SelectedPartType.HasValue)
            {

            }
            else
            {
                if (_scrapyardBot != null && mouseCoordinate != Vector2Int.zero)
                {
                    IAttachable attachableAtCoordinates = _scrapyardBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);

                    if (attachableAtCoordinates != null && attachableAtCoordinates is ScrapyardPart partAtCoordinates)
                    {
                        _scrapyardBot.TryRemoveAttachableAt(mouseCoordinate, false);
                        SelectedPartType = partAtCoordinates.Type;
                        SelectedPartLevel = partAtCoordinates.level;
                        SelectedPartPreviousGridPosition = mouseCoordinate;
                        SelectedPartRemoveFromStorage = false;
                        SelectedPartReturnToStorageIfNotPlaced = true;
                        SaveBlockData();
                    }
                }
            }
            UpdateFloatingMarkers(true);
        }

        private void OnLeftMouseButtonUp()
        {
            if (dismantleBin != null)
                dismantleBin.gameObject.SetActive(false);

            if (!TryGetMouseCoordinate(out Vector2Int mouseCoordinate))
            {
                if (SelectedPartType != null && dismantleBin != null)
                {
                    Vector2 worldMousePosition = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                    if (Vector2.Distance(worldMousePosition, dismantleBin.transform.position) <= 3)
                    {
                        Toast.AddToast("Dismantle part", verticalLayout: Toast.Layout.Start, horizontalLayout: Toast.Layout.Middle);
                        PlayerPersistentData.PlayerData.AddResources(SelectedPartType.Value, SelectedPartLevel, true);

                        if (SelectedPartRemoveFromStorage)
                        {
                            BlockData blockData = new BlockData
                            {
                                Type = (int)SelectedPartType,
                                Level = SelectedPartLevel
                            };
                            
                            PlayerPersistentData.PlayerData.RemovePartFromStorage(blockData);
                        }

                        SelectedPartType = null;
                        SelectedPartLevel = 0;
                        SelectedPartPreviousGridPosition = null;
                        SelectedPartRemoveFromStorage = false;
                        SelectedPartReturnToStorageIfNotPlaced = false;
                        SaveBlockData();
                    }
                    else
                    {
                        if (SelectedPartPreviousGridPosition != null)
                        {
                            var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(SelectedPartType.Value, SelectedPartLevel);

                            //Check if part should be removed from storage
                            //TODO Should be checking if the player does in-fact have the part in their storage
                            if (SelectedPartRemoveFromStorage)
                            {
                                PlayerPersistentData.PlayerData.RemovePartFromStorage(attachable.ToBlockData());
                            }

                            droneDesignUi.RefreshScrollViews();
                            _scrapyardBot.AttachNewBit(SelectedPartPreviousGridPosition.Value, attachable);
                            _toUndoStack.Push(new ScrapyardEditData
                            {
                                EventType = SCRAPYARD_ACTION.EQUIP,
                                Coordinate = SelectedPartPreviousGridPosition.Value,
                                PartType = (PART_TYPE)SelectedPartType
                            });
                            _toRedoStack.Clear();

                            SelectedPartType = null;
                            SelectedPartLevel = 0;
                            SelectedPartPreviousGridPosition = null;
                            SelectedPartRemoveFromStorage = false;
                            SelectedPartReturnToStorageIfNotPlaced = false;
                            SaveBlockData();
                        }
                    }
                }
                UpdateFloatingMarkers(false);
                return;
            }
            
            if (SelectedPartType.HasValue)
            {
                if (_scrapyardBot != null)
                {
                    IAttachable attachableAtCoordinates = _scrapyardBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);

                    if (attachableAtCoordinates == null)
                    {
                        var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(SelectedPartType.Value, SelectedPartLevel);

                        //Check if part should be removed from storage
                        //TODO Should be checking if the player does in-fact have the part in their storage
                        if (SelectedPartRemoveFromStorage)
                        {
                            PlayerPersistentData.PlayerData.RemovePartFromStorage(attachable.ToBlockData());
                        }

                        droneDesignUi.RefreshScrollViews();
                        _scrapyardBot.AttachNewBit(mouseCoordinate, attachable);
                        _toUndoStack.Push(new ScrapyardEditData
                        {
                            EventType = SCRAPYARD_ACTION.EQUIP,
                            Coordinate = mouseCoordinate,
                            PartType = (PART_TYPE)SelectedPartType
                        });
                        _toRedoStack.Clear();

                        SelectedPartType = null;
                        SelectedPartLevel = 0;
                        SelectedPartPreviousGridPosition = null;
                        SelectedPartRemoveFromStorage = false;
                        SelectedPartReturnToStorageIfNotPlaced = false;
                        SaveBlockData();
                    }
                    else
                    {
                        if (SelectedPartPreviousGridPosition != null)
                        {
                            var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(SelectedPartType.Value, SelectedPartLevel);

                            //Check if part should be removed from storage
                            //TODO Should be checking if the player does in-fact have the part in their storage
                            if (SelectedPartRemoveFromStorage)
                            {
                                PlayerPersistentData.PlayerData.RemovePartFromStorage(attachable.ToBlockData());
                            }

                            droneDesignUi.RefreshScrollViews();
                            _scrapyardBot.AttachNewBit(SelectedPartPreviousGridPosition.Value, attachable);
                            _toUndoStack.Push(new ScrapyardEditData
                            {
                                EventType = SCRAPYARD_ACTION.EQUIP,
                                Coordinate = SelectedPartPreviousGridPosition.Value,
                                PartType = (PART_TYPE)SelectedPartType
                            });
                            _toRedoStack.Clear();

                            SelectedPartType = null;
                            SelectedPartLevel = 0;
                            SelectedPartPreviousGridPosition = null;
                            SelectedPartRemoveFromStorage = false;
                            SelectedPartReturnToStorageIfNotPlaced = false;
                            SaveBlockData();
                        }
                    }
                }
            }
            else
            {

            }
            UpdateFloatingMarkers(false);
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

            if (_scrapyardBot != null)
            {
                IAttachable attachableAtCoordinates = _scrapyardBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);

                if (attachableAtCoordinates != null && attachableAtCoordinates is ScrapyardPart scrapPart)
                {
                    PlayerPersistentData.PlayerData.AddPartToStorage(scrapPart.ToBlockData());
                    droneDesignUi.AddToPartScrollView(scrapPart.ToBlockData());
                    _toUndoStack.Push(new ScrapyardEditData
                    {
                        EventType = SCRAPYARD_ACTION.UNEQUIP,
                        Coordinate = mouseCoordinate,
                        PartType = scrapPart.Type,
                        Level = scrapPart.level
                    });
                    _toRedoStack.Clear();

                    _scrapyardBot.TryRemoveAttachableAt(mouseCoordinate, false);
                    SaveBlockData();
                }
            }
            UpdateFloatingMarkers(false);
        }

        private void OnRightMouseButtonUp()
        {

        }

        #endregion //User Input

        //============================================================================================================//

        #region Undo/Redo Stack

        public void UndoStackPop()
        {
            if (_toUndoStack.Count == 0)
                return;

            ScrapyardEditData toUndo = _toUndoStack.Pop();
            var playerData = PlayerPersistentData.PlayerData;

            switch (toUndo.EventType)
            {
                case SCRAPYARD_ACTION.EQUIP:
                    if (_scrapyardBot != null)
                    {
                        PlayerPersistentData.PlayerData.AddPartToStorage
                            (((ScrapyardPart)_scrapyardBot.attachedBlocks.FirstOrDefault(a => a.Coordinate == toUndo.Coordinate)).ToBlockData());

                        _scrapyardBot.TryRemoveAttachableAt(toUndo.Coordinate, false);
                        droneDesignUi.RefreshScrollViews();
                        SaveBlockData();
                    }
                    break;
                case SCRAPYARD_ACTION.UNEQUIP:
                    if (_scrapyardBot != null)
                    {
                        var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(toUndo.PartType, toUndo.Level);
                        PlayerPersistentData.PlayerData.RemovePartFromStorage(attachable.ToBlockData());
                        _scrapyardBot.AttachNewBit(toUndo.Coordinate, attachable);
                        droneDesignUi.RefreshScrollViews();
                        SaveBlockData();
                    }
                    break;

                case SCRAPYARD_ACTION.PURCHASE:
                    if (_scrapyardBot != null)
                    {
                        _scrapyardBot.TryRemoveAttachableAt(toUndo.Coordinate, true);
                        droneDesignUi.UpdateResourceElements();
                        SaveBlockData();
                    }
                    break;
                case SCRAPYARD_ACTION.UPGRADE:
                    if (_scrapyardBot != null)
                    {
                        IAttachable attachableAtCoordinates = _scrapyardBot.attachedBlocks.GetAttachableAtCoordinates(toUndo.Coordinate);
                        if (attachableAtCoordinates == null)
                            return;

                        if (attachableAtCoordinates is ScrapyardPart scrapyardPart)
                        {
                            if (!FactoryManager.Instance.GetFactory<PartAttachableFactory>().CheckLevelExists(toUndo.PartType, toUndo.Level - 1))
                                return;

                            playerData.AddResources(toUndo.PartType, toUndo.Level, false);
                            droneDesignUi.UpdateResourceElements();
                            FactoryManager.Instance.GetFactory<PartAttachableFactory>().UpdatePartData(scrapyardPart.Type, scrapyardPart.level - 1, ref scrapyardPart);
                            SaveBlockData();
                        }
                    }
                    break;
                case SCRAPYARD_ACTION.SALE:
                    if (_scrapyardBot != null)
                    {
                        IAttachable attachableAtCoordinates = _scrapyardBot.attachedBlocks.GetAttachableAtCoordinates(toUndo.Coordinate);
                        if (attachableAtCoordinates != null)
                            return;

                        var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>(toUndo.PartType, toUndo.Level);
                        playerData.SubtractResources(toUndo.PartType, toUndo.Level, true);
                        _scrapyardBot.AttachNewBit(toUndo.Coordinate, attachable);
                        droneDesignUi.UpdateResourceElements();
                        SaveBlockData();
                    }
                    break;
            }

            UpdateFloatingMarkers(false);
            _toRedoStack.Push(toUndo);
        }

        public void RedoStackPop()
        {
            if (_toRedoStack.Count == 0)
                return;

            ScrapyardEditData toRedo = _toRedoStack.Pop();
            var playerData = PlayerPersistentData.PlayerData;

            switch (toRedo.EventType)
            {
                case SCRAPYARD_ACTION.EQUIP:
                    if (_scrapyardBot != null)
                    {
                        var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(toRedo.PartType, toRedo.Level);
                        PlayerPersistentData.PlayerData.RemovePartFromStorage(attachable.ToBlockData());
                        _scrapyardBot.AttachNewBit(toRedo.Coordinate, attachable);
                        droneDesignUi.RefreshScrollViews();
                        SaveBlockData();
                    }
                    break;
                case SCRAPYARD_ACTION.UNEQUIP:
                    if (_scrapyardBot != null)
                    {
                        PlayerPersistentData.PlayerData.AddPartToStorage
                            (((ScrapyardPart)_scrapyardBot.attachedBlocks.FirstOrDefault(a => a.Coordinate == toRedo.Coordinate)).ToBlockData());

                        _scrapyardBot.TryRemoveAttachableAt(toRedo.Coordinate, false);
                        droneDesignUi.RefreshScrollViews();
                        SaveBlockData();
                    }
                    break;


                case SCRAPYARD_ACTION.PURCHASE:
                    if (_scrapyardBot != null)
                    {
                        IAttachable attachableAtCoordinates = _scrapyardBot.attachedBlocks.GetAttachableAtCoordinates(toRedo.Coordinate);
                        if (attachableAtCoordinates != null)
                            return;

                        if (!playerData.CanAffordPart(toRedo.PartType, 0, false))
                            return;

                        var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>(toRedo.PartType, 0);
                        playerData.SubtractResources(toRedo.PartType, 0, false);
                        _scrapyardBot.AttachNewBit(toRedo.Coordinate, attachable);
                        droneDesignUi.UpdateResourceElements();
                        SaveBlockData();
                    }
                    break;
                case SCRAPYARD_ACTION.UPGRADE:
                    if (_scrapyardBot != null)
                    {
                        IAttachable attachableAtCoordinates = _scrapyardBot.attachedBlocks.GetAttachableAtCoordinates(toRedo.Coordinate);
                        if (attachableAtCoordinates == null)
                            return;

                        if (attachableAtCoordinates is ScrapyardPart scrapyardPart)
                        {
                            if (!FactoryManager.Instance.GetFactory<PartAttachableFactory>().CheckLevelExists(toRedo.PartType, scrapyardPart.level + 1))
                                return;

                            playerData.SubtractResources(toRedo.PartType, scrapyardPart.level + 1, false);
                            droneDesignUi.UpdateResourceElements();
                            FactoryManager.Instance.GetFactory<PartAttachableFactory>().UpdatePartData(scrapyardPart.Type, scrapyardPart.level + 1, ref scrapyardPart);
                            SaveBlockData();
                        }
                    }
                    break;
                case SCRAPYARD_ACTION.SALE:
                    if (_scrapyardBot != null)
                    {
                        _scrapyardBot.TryRemoveAttachableAt(toRedo.Coordinate, true);
                        droneDesignUi.UpdateResourceElements();
                        SaveBlockData();
                    }
                    break;
            }

            UpdateFloatingMarkers(false);
            _toUndoStack.Push(toRedo);
        }

        public void ClearUndoRedoStacks()
        {
            if (!isStarted)
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
            ExportRemoteData(_scrapyardLayouts);
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
            Dictionary<COMPONENT_TYPE, int> componentComparer = new Dictionary<COMPONENT_TYPE, int>(PlayerPersistentData.PlayerData.components);

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
            droneDesignUi.UpdateResourceElements();
            droneDesignUi.RefreshScrollViews();
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
                PlayerPersistentData.PlayerData.SetCurrentBlockData(_scrapyardBot.attachedBlocks.GetBlockDatas());
            }
        }

        public string ExportRemoteData(List<ScrapyardLayout> editorData)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            System.IO.File.WriteAllText(Application.dataPath + "/RemoteData/ScrapyardLayoutData.txt", export);

            return export;
        }

        public List<ScrapyardLayout> ImportRemoteData()
        {
            if (!File.Exists(Application.dataPath + "/RemoteData/ScrapyardLayoutData.txt"))
                return new List<ScrapyardLayout>();

            var loaded = JsonConvert.DeserializeObject<List<ScrapyardLayout>>(File.ReadAllText(Application.dataPath + "/RemoteData/ScrapyardLayoutData.txt"));

            return loaded;
        }

        #endregion

        //============================================================================================================//

        #region Other

        // TMP Sprites section
        //============================================================================================================//

        private readonly Dictionary<BIT_TYPE, string> _textSprites = new Dictionary<BIT_TYPE, string>
        {
            { BIT_TYPE.GREEN,  "<sprite=\"MaterIalIcons_SS_ver1\" name=\"MaterIalIcons_SS_ver1_4\">" },
            { BIT_TYPE.GREY,   "<sprite=\"MaterIalIcons_SS_ver1\" name=\"MaterIalIcons_SS_ver1_3\">" },
            { BIT_TYPE.RED,    "<sprite=\"MaterIalIcons_SS_ver1\" name=\"MaterIalIcons_SS_ver1_2\">" },
            { BIT_TYPE.BLUE,   "<sprite=\"MaterIalIcons_SS_ver1\" name=\"MaterIalIcons_SS_ver1_1\">" },
            { BIT_TYPE.YELLOW, "<sprite=\"MaterIalIcons_SS_ver1\" name=\"MaterIalIcons_SS_ver1_0\">" },
        };

        private static string GetBitSprite(BIT_TYPE type, int level)
        {
            int typeBase;
            switch (type)
            {
                case BIT_TYPE.BLUE:
                    typeBase = 0;
                    break;
                case BIT_TYPE.GREEN:
                    typeBase = 3;
                    break;
                case BIT_TYPE.GREY:
                    typeBase = 4;
                    break;
                case BIT_TYPE.RED:
                    typeBase = 1;
                    break;
                case BIT_TYPE.YELLOW:
                    typeBase = 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            int levelOffset = level * 5;

            return $"<sprite=\"GamePieces_Atlas\" name=\"GamePieces_Atlas_{typeBase + levelOffset}\">";
        }

        //============================================================================================================//

        private void SellBits()
        {
            if (_scrapyardBot != null)
            {
                List<ScrapyardBit> listBits = _scrapyardBot.attachedBlocks.OfType<ScrapyardBit>().ToList();


                List<Component> listComponents = _scrapyardBot.attachedBlocks.OfType<Component>().ToList();
                if (listComponents.Count > 0)
                {
                    _scrapyardBot.RemoveAllComponents();

                    //TODO Need to think about if I should be displaying the components processed or not
                    foreach (var component in listComponents)
                    {
                        PlayerPersistentData.PlayerData.AddComponent(component.Type, 1);
                    }

                    PlayerData.OnValuesChanged?.Invoke();
                    SaveBlockData();
                }


                if (listBits.Count == 0)
                    return;

                var scrapyardBits = _scrapyardBot.attachedBlocks.OfType<ScrapyardBit>();

                Dictionary<BIT_TYPE, int> bits = FactoryManager.Instance.GetFactory<BitAttachableFactory>().GetTotalResources(scrapyardBits);

                PlayerPersistentData.PlayerData.AddResources(bits);


                string resourcesGained = "";
                foreach (var resource in bits)
                {
                    int numTotal = scrapyardBits.Count(b => b.Type == resource.Key);

                    for (int i = 0; numTotal > 0; i++)
                    {
                        int numAtLevel = scrapyardBits.Count(b => b.Type == resource.Key && b.level == i);
                        if (numAtLevel == 0)
                            continue;

                        BitRemoteData remoteData = FactoryManager.Instance.GetFactory<BitAttachableFactory>().GetBitRemoteData(resource.Key);
                        int resourceAmount = numAtLevel * remoteData.levels[i].resources;
                        resourcesGained += $"{numAtLevel} x {GetBitSprite(resource.Key, i)} = {resourceAmount} {_textSprites[resource.Key]} ";
                        numTotal -= numAtLevel;
                    }

                    resourcesGained += "\n";
                }
                Alert.ShowAlert("Resources Refined", resourcesGained, "Okay", null);
                Alert.SetLineHeight(90f);


                _scrapyardBot.RemoveAllBits();



                SaveBlockData();

                droneDesignUi.UpdateResourceElements();
            }
        }

        public void RotateBots(float direction)
        {
            if (_scrapyardBot != null)
            {
                _scrapyardBot.Rotate(direction);
            }
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
            if (_scrapyardBot != null && _scrapyardBot.CheckHasDisconnects())
            {
                return false;
            }

            return true;
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

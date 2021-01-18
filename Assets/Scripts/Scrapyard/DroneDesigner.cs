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
using StarSalvager.Utilities.FileIO;
using StarSalvager.Utilities.UI;
using Input = StarSalvager.Utilities.Inputs.Input;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.Saving;

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
        
        private StorageUI StorageUI
        {
            get
            {
                if (_storageUI == null)
                    _storageUI = FindObjectOfType<StorageUI>();

                return _storageUI;
            }
        }
        [SerializeField, Required]
        private StorageUI _storageUI;
        
        
        [SerializeField]
        private GameObject floatingPartWarningPrefab;
        [SerializeField]
        private GameObject availablePointMarkerPrefab;

        [NonSerialized]
        public bool IsUpgrading;

        private List<GameObject> _floatingPartWarnings;
        private List<GameObject> _availablePointMarkers;

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
            _scrapyardLayouts = Files.ImportLayoutData();
            _currentLayout = null;
            IsUpgrading = false;

            InitInput();

            _isStarted = true;
        }

        private void Update()
        {
            CheckForMousePartHover();
                
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

        //====================================================================================================================//
        private PatchUIElement _draggingPatch;
        public void BeginDragPatch(in PatchUIElement patchUIElement)
        {
            _draggingPatch = patchUIElement;
        }

        public void EndDragPatch()
        {
            //If we're not dragging anything, don't bother with the next steps
            if (_draggingPatch == null)
                return;

            //Check if the mouse is within the editor grid
            if (!IsMouseInEditorGrid(out var mouseGridCoordinate))
                return;

            //Check which part we're over, if any
            var attachableAtCoordinates = _scrapyardBot.AttachedBlocks.GetAttachableAtCoordinates(mouseGridCoordinate);

            if (attachableAtCoordinates == null)
            {
                _draggingPatch.ResetInScrollview();
                _draggingPatch = null;
                return;
            }

            //TODO Try to add the current Patch to that part

            //Check to make sure we're probing a part, and not something else
            if (!(attachableAtCoordinates is ScrapyardPart part))
                throw new ArgumentOutOfRangeException(nameof(attachableAtCoordinates), attachableAtCoordinates, $"Expected {nameof(ScrapyardPart)}");

            var patchData = _draggingPatch.data.PatchData;
            var patchRemoteData = FactoryManager.Instance.PatchRemoteData.GetRemoteData(patchData.Type);
            var partType = part.Type;

            //Check if the part is allowed to have this Patch
            if (!patchRemoteData.allowedParts.Contains(partType))
            {
                _draggingPatch.ResetInScrollview();
                _draggingPatch = null;
                return;
            }

            //Check to see if the part has any available slots for the new patch
            if (part.Patches.All(x => x.Type != (int) PATCH_TYPE.EMPTY))
            {
                _draggingPatch.ResetInScrollview();
                _draggingPatch = null;
                return;
            }

            //TODO Determine if we can have 2 of the same patch on one part
            
            //TODO Add Patch to part
            part.AddPatch(patchData);
            PlayerDataManager.RemovePatchFromStorageAtIndex(_draggingPatch.data.storageIndex);

            StorageUI.UpdateStorage();
            
            _draggingPatch.ResetInScrollview();
            _draggingPatch = null;
        }

        //============================================================================================================//

        #region IReset Functions

        public void Activate()
        {
            GameManager.SetCurrentGameState(GameState.Scrapyard);

            GameTimer.SetPaused(true);

            if(Globals.DownGradeBits)
                DowngradeAllBits(1);
            
            //SellBits();
            SetupDrone();

            UpdateFloatingMarkers(false);
        }

        public void Reset()
        {
            if (SelectedBrick != null && SelectedPartReturnToStorageIfNotPlaced)
            {
                PlayerDataManager.AddPartToStorage(SelectedBrick);
            }

            SelectedBrick = null;
            SelectedPartRemoveFromStorage = false;
            SelectedPartReturnToStorageIfNotPlaced = false;

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
            UpdateFloatingMarkers(SelectedBrick != null);

            if (!IsMouseInEditorGrid(out Vector2Int mouseCoordinate))
                return;

            if (SelectedBrick != null)
                return;

            if (_scrapyardBot == null || mouseCoordinate == Vector2Int.zero)
                return;

            IAttachable attachableAtCoordinates = _scrapyardBot.AttachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);

            if (attachableAtCoordinates == null ||
                !(attachableAtCoordinates is ScrapyardPart partAtCoordinates && partAtCoordinates.Type != PART_TYPE.EMPTY))
                return;

            var type = partAtCoordinates.Type;

            Vector3 currentAttachablePosition = attachableAtCoordinates.transform.position;

            _scrapyardBot.TryRemoveAttachableAt(mouseCoordinate);

            SelectedBrick = partAtCoordinates.ToBlockData();

            SelectedPartClickPosition = Cameras.CameraController.Camera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            SelectedPartPreviousGridPosition = mouseCoordinate;
            SelectedPartRemoveFromStorage = false;
            SelectedPartReturnToStorageIfNotPlaced = true;
            SaveBlockData();

            if (_partDragImage == null)
            {
                _partDragImage = new GameObject().AddComponent<SpriteRenderer>();
                _partDragImage.sortingLayerName = LayerHelper.ACTORS;
                _partDragImage.sortingOrder = 1;
                
            }
            _partDragImage.gameObject.SetActive(true);
            _partDragImage.sprite = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetProfileData(type).GetSprite();
            _partDragImage.transform.position = currentAttachablePosition;

            var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(new PartData
            {
                Type = (int)PART_TYPE.EMPTY,
                Coordinate = mouseCoordinate,
                Patches = new PatchData[0]
            });

            _scrapyardBot.AttachNewBit(mouseCoordinate, attachable);
        }

        private void OnLeftMouseButtonUp()
        {
            //--------------------------------------------------------------------------------------------------------//

            void ResetSelected()
            {
                SelectedBrick = null;
                SelectedIndex = 0;
                SelectedPartClickPosition = null;
                SelectedPartPreviousGridPosition = null;
                SelectedPartRemoveFromStorage = false;
                SelectedPartReturnToStorageIfNotPlaced = false;
            }
            
            //--------------------------------------------------------------------------------------------------------//
            
            if (_partDragImage != null)
                _partDragImage.gameObject.SetActive(false);

            _isDragging = false;

            if (SelectedBrick is null || _scrapyardBot == null)
            {
                UpdateFloatingMarkers(false);
                return;
            }


            //Check if mouse coordinate is inside the editing grid
            //--------------------------------------------------------------------------------------------------------//

            if (!IsMouseInEditorGrid(out var mouseGridCoordinate))
            {
                //Move part back to previous location since drag position is inviable
                if(SelectedPartPreviousGridPosition != null)
                {
                    if (!(SelectedBrick is PartData partData)) 
                        throw new ArgumentOutOfRangeException(nameof(SelectedBrick), SelectedBrick, $"Expected {nameof(PartData)}");

                    var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(partData);

                    //Check if part should be removed from storage
                    //TODO Should be checking if the player does in-fact have the part in their storage
                    if (SelectedPartRemoveFromStorage)
                    {
                        PlayerDataManager.RemovePartFromStorageAtIndex(SelectedIndex);
                    }

                    _scrapyardBot.TryRemoveAttachableAt(SelectedPartPreviousGridPosition.Value);
                    _scrapyardBot.AttachNewBit(SelectedPartPreviousGridPosition.Value, attachable);
                    
                    ResetSelected();
                    SaveBlockData();
                }

                UpdateFloatingMarkers(false);
                return;
            }
            
            //If mouse position was legal
            //--------------------------------------------------------------------------------------------------------//

            IAttachable attachableAtCoordinates = _scrapyardBot.AttachedBlocks.GetAttachableAtCoordinates(mouseGridCoordinate);

            if (attachableAtCoordinates is ScrapyardPart scrapyardPart && scrapyardPart.Type == PART_TYPE.EMPTY)
            {
                if (!(SelectedBrick is PartData partData))
                    throw new ArgumentOutOfRangeException(nameof(SelectedBrick), SelectedBrick, $"Expected {nameof(PartData)}");

                var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(partData);

                _scrapyardBot.TryRemoveAttachableAt(mouseGridCoordinate);
                _scrapyardBot.AttachNewBit(mouseGridCoordinate, attachable);

                //Check if part should be removed from storage
                //TODO Should be checking if the player does in-fact have the part in their storage
                if (SelectedPartRemoveFromStorage)
                {
                    SelectedBrick.Coordinate = mouseGridCoordinate;

                    PlayerDataManager.RemovePartFromStorageAtIndex(SelectedIndex);
                }
                else
                {
                    SelectedBrick.Coordinate = SelectedPartPreviousGridPosition.Value;
                }

                ResetSelected();
                SaveBlockData();
            }
            //If there is an attachable at location
            else if (SelectedPartPreviousGridPosition.HasValue)
            {
                if (!(SelectedBrick is PartData partData))
                    throw new ArgumentOutOfRangeException(nameof(SelectedBrick), SelectedBrick, $"Expected {nameof(PartData)}");

                //Return object to previous location on bot
                var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(partData);

                //Check if part should be removed from storage
                //TODO Should be checking if the player does in-fact have the part in their storage
                if (SelectedPartRemoveFromStorage)
                {
                    PlayerDataManager.RemovePartFromStorageAtIndex(SelectedIndex);
                }

                _scrapyardBot.TryRemoveAttachableAt(SelectedPartPreviousGridPosition.Value);
                _scrapyardBot.AttachNewBit(SelectedPartPreviousGridPosition.Value, attachable);

                ResetSelected();
                SaveBlockData();
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
            if (!IsMouseInEditorGrid(out Vector2Int mouseCoordinate))
                return;

            if (_scrapyardBot == null)
            {
                UpdateFloatingMarkers(false);
                return;
            }

            IAttachable attachableAtCoordinates = _scrapyardBot.AttachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);

            if (attachableAtCoordinates is ScrapyardPart scrapPart && scrapPart.Type != PART_TYPE.EMPTY)
            {
                //Don't want to be able to remove the core
                if (scrapPart.Type == PART_TYPE.CORE)
                    return;

                var blockData = scrapPart.ToBlockData();
                blockData.Coordinate = mouseCoordinate;
                _scrapyardBot.TryRemoveAttachableAt(mouseCoordinate);

                for (int i = 0; i < scrapPart.Patches.Length; i++)
                {
                    var patchData = scrapPart.Patches[i];
                    
                    if (patchData.Type == (int) PATCH_TYPE.EMPTY)
                        continue;
                    
                    PlayerDataManager.AddPatchToStorage(patchData);
                    scrapPart.Patches[i] = default;
                }
                
                PlayerDataManager.AddPartToStorage(scrapPart.ToBlockData());

                var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(new PartData
                {
                    Type = (int)PART_TYPE.EMPTY,
                    Coordinate = mouseCoordinate,
                    Patches = new PatchData[0]
                });

                _scrapyardBot.AttachNewBit(mouseCoordinate, attachable);

                SaveBlockData();
            }

            UpdateFloatingMarkers(false);
            /*DroneDesignUi.ShowRepairCost(GetRepairCost(), GetReplacementCost());*/
        }

        private void OnRightMouseButtonUp()
        {
        }

        private void CheckForMousePartHover()
        {
            var show = TryHoverPart(out var partData);

            DroneDesignUi.ShowPartDetails(show, partData);
        }

        private bool TryHoverPart(out ScrapyardPart scrapyardPart)
        {
            scrapyardPart = null;
            
            if(_draggingPatch || _isDragging || PlayerDataManager.GetCanChoosePart())
                return false;
            
            if (!IsMouseInEditorGrid(out Vector2Int mouseCoordinate))
                return false;

            if (_scrapyardBot == null || SelectedBrick != null)
                return false;

            var partAtCoords = _scrapyardBot.AttachedBlocks
                .OfType<ScrapyardPart>()
                .FirstOrDefault(x => x.Type != PART_TYPE.EMPTY && x.Coordinate == mouseCoordinate);

            if (partAtCoords is null)
                return false;

            scrapyardPart = partAtCoords;
            
            return true;
        }

        #endregion //User Input

        //============================================================================================================//

        #region Save/Load Layouts

        public void SaveLayout(string layoutName)
        {
            throw new NotImplementedException();
        }

        public void LoadLayout(string name)
        {
            throw new NotImplementedException();
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

            List<IBlockData> currentBlockData = PlayerDataManager.GetBlockDatas();

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

        /*private void SellBits()
        {
            var bitAttachableFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();

            var droneBlockData = new List<IBlockData>(PlayerDataManager.GetBlockDatas());
            PlayerDataManager.SetBlockData(droneBlockData.Where(x => x.ClassType.Equals(nameof(Part)) || x.ClassType.Equals(nameof(ScrapyardPart))).ToList());

            //--------------------------------------------------------------------------------------------------------//

            //Only get all the things that aren't parts from the bot
            List<IBlockData> botBlockData = droneBlockData.Where(x => !x.ClassType.Equals(nameof(Part))).ToList();

            //If we have nothing to process, don't bother moving forward
            if (botBlockData.Count == 0)
                return;

            var processedResources = new Dictionary<BIT_TYPE, int>();
            var wastedResources = new Dictionary<BIT_TYPE, int>();

            foreach (var blockData in botBlockData)
            {
                int amount;
                switch (blockData)
                {
                    //------------------------------------------------------------------------------------------------//
                    /*case nameof(Component):
                        amount = 1;
                        var componentType = (COMPONENT_TYPE) blockData.Type;

                        if (blockData.Level > 0)
                            amount = blockData.Level * 3;

                        PlayerDataManager.AddComponent(componentType, amount, false);
                        break;#1#
                    //------------------------------------------------------------------------------------------------//
                    case BitData bitData:
                        /*var bitType = (BIT_TYPE) bitData.Type;

                        amount = bitAttachableFactory.GetTotalResource(bitType, bitData.Level);

                        var addResourceAmount =  amount;
                        PlayerDataManager.GetResource(bitType)
                            .AddResourceReturnWasted(
                                addResourceAmount,
                                out var wastedResource,
                                false);

                        TryIncrementDict(bitType, amount, ref processedResources);
                        TryIncrementDict(bitType, wastedResource, ref wastedResources);#1#


                        break;
                    //------------------------------------------------------------------------------------------------//
                    case CrateData crateData:
                        int numCrates = 1;
                        for (int i = 0; i < crateData.Level; i++)
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
                                        //PlayerDataManager.GetResource(rdsValueResourceRefined.rdsValue.Item1).AddResource(rdsValueResourceRefined.rdsValue.Item2);
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
                                    case RDSValue<IBlockData> rdsValueBlockData:
                                        {
                                            if (!GameManager.IsState(GameState.LEVEL_ACTIVE))
                                            {
                                                switch (rdsValueBlockData.rdsValue.ClassType)
                                                {
                                                    case nameof(Component):
                                                        PlayerDataManager.AddComponent(1);
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
            //DroneDesignUi.UpdateBotResourceElements();

            //Show the final alert to the player
            ShowAlertInfo(botBlockData, processedResources, wastedResources);
        }*/



        private static void ShowAlertInfo(IEnumerable<IBlockData> botBlockDatas, Dictionary<BIT_TYPE, int> processedResources, Dictionary<BIT_TYPE, int> wastedResources)
        {
            if (processedResources.IsNullOrEmpty() || wastedResources.IsNullOrEmpty())
                return;

            var bits = botBlockDatas.OfType<BitData>().ToArray();

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

        private void DowngradeAllBits(int removeBelowLevel)
        {
            Debug.Log("DOWNGRADING DISABLED");
            return;
            var droneBlockData = new List<IBlockData>(PlayerDataManager.GetBlockDatas());
            
            var attachedBits = droneBlockData.OfType<BitData>().Where(x => x.Level < removeBelowLevel).ToArray();

            for (int i = 0; i < attachedBits.Length; i++)
            {
                var bitData = attachedBits[i];
                
                var orphanData = new List<OrphanMoveBlockData>();
                droneBlockData.CheckForOrphansFromProcessing(bitData, ref orphanData);

                droneBlockData.Remove(bitData);
                for (int ii = 0; ii < orphanData.Count; ii++)
                {
                    var data = orphanData[ii];
                    var index = droneBlockData.FindIndex(x => x.Coordinate == data.startingCoordinates);
                    
                    droneBlockData[index].Coordinate = data.intendedCoordinates;
                }
            }
            
            for (int i = 0; i < droneBlockData.Count; i++)
            {
                if(!(droneBlockData[i] is BitData bitData) || bitData.Level < removeBelowLevel)
                    continue;

                bitData.Level -= 1;
                droneBlockData[i] = bitData;
            }
           
            PlayerDataManager.SetBlockData(droneBlockData);
            Globals.DownGradeBits = false;
        }

        //====================================================================================================================//

        public void RotateBots(float direction)
        {
            if (_scrapyardBot != null)
            {
                _scrapyardBot.Rotate(direction);
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

            /*if (showAvailable && _scrapyardBot != null)
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
            }*/
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(hint), hint, null);
            }
        }
        
        [Button]
        public void ShowPart()
        {
            var corePartData = _scrapyardBot.AttachedBlocks.OfType<ScrapyardPart>()
                .FirstOrDefault(x => x.Type == PART_TYPE.GUN);

            if (!corePartData)
                return;
            
            DroneDesignUi.ShowPartDetails(true, corePartData);
        }
    }
}

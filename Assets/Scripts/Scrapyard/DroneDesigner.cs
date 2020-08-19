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

        [NonSerialized]
        public bool IsUpgrading;

        private List<GameObject> _floatingPartWarnings;
        private List<GameObject> _availablePointMarkers;

        private Stack<ScrapyardEditData> _toUndoStack;
        private Stack<ScrapyardEditData> _toRedoStack;

        private ScrapyardLayout _currentLayout;

        public List<ScrapyardLayout> ScrapyardLayouts => _scrapyardLayouts;
        private List<ScrapyardLayout> _scrapyardLayouts;

        private bool isStarted = false;

        /*[Sirenix.OdinInspector.Button("Clear Remote Data")]
        private void ClearRemoteData()
        {
            string persistentPlayerDataPath = Application.dataPath + "/RemoteData/PlayerPersistentData.player";
            string currentDataPath = Application.dataPath + "/RemoteData/MissionsCurrentData.mission";
            if (File.Exists(persistentPlayerDataPath))
            {
                File.Delete(persistentPlayerDataPath);
            }
            if (File.Exists(currentDataPath))
            {
                File.Delete(currentDataPath);
            }
        }*/

        //============================================================================================================//

        #region Unity Functions

        // Start is called before the first frame update
        private void Start()
        {
            _scrapyardBots = new List<ScrapyardBot>();
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

        #endregion

        //============================================================================================================//

        #region IReset Functions

        public void Activate()
        {
            GameTimer.SetPaused(true);
            Camera.onPostRender += DrawGL;

            _scrapyardBots.Add(FactoryManager.Instance.GetFactory<BotFactory>().CreateScrapyardObject<ScrapyardBot>());

            var currentBlockData = PlayerPersistentData.PlayerData.GetCurrentBlockData();
            //Checks to make sure there is a core on the bot
            if (currentBlockData.Count == 0 || !currentBlockData.Any(x => x.ClassType.Contains(nameof(Part)) && x.Type == (int)PART_TYPE.CORE))
            {
                _scrapyardBots[0].InitBot();
            }
            else
            {
                var importedData = currentBlockData.ImportBlockDatas(true);
                _scrapyardBots[0].InitBot(importedData);
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


            UpdateFloatingMarkers(false);
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

        //On left mouse button click, check if there is a bit/part at the mouse location. If there is not, purchase the selected part type and place it at this location.
        private void OnLeftMouseButtonDown(InputAction.CallbackContext ctx)
        {
            if (ctx.ReadValue<float>() == 1f)
            {
                UpdateFloatingMarkers(true);
                return;
            }
            UpdateFloatingMarkers(false);

            if (!TryGetMouseCoordinate(out Vector2Int mouseCoordinate))
                return;

            /*if (IsUpgrading)
            {
                foreach (ScrapyardBot scrapBot in _scrapyardBots)
                {
                    IAttachable attachableAtCoordinates = scrapBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);
                    var playerData = PlayerPersistentData.PlayerData;

                    if (attachableAtCoordinates != null)
                    {
                        if (attachableAtCoordinates is ScrapyardPart partAtCoordinates)
                        {
                            if (!FactoryManager.Instance.GetFactory<PartAttachableFactory>().CheckLevelExists(partAtCoordinates.Type, partAtCoordinates.level + 1))
                                return;

                            if (!PlayerPersistentData.PlayerData.CanAffordPart(partAtCoordinates.Type, partAtCoordinates.level + 1, false))
                            {
                                droneDesignUi.DisplayInsufficientResources();
                                return;
                            }

                            playerData.SubtractResources(partAtCoordinates.Type, partAtCoordinates.level + 1, false);
                            droneDesignUi.UpdateResources(playerData.GetResources());
                            _toUndoStack.Push(new ScrapyardEditData
                            {
                                EventType = SCRAPYARD_ACTION.UPGRADE,
                                Coordinate = mouseCoordinate,
                                PartType = partAtCoordinates.Type,
                                Level = partAtCoordinates.level + 1
                            });
                            _toRedoStack.Clear();
                            FactoryManager.Instance.GetFactory<PartAttachableFactory>().UpdatePartData(partAtCoordinates.Type, partAtCoordinates.level + 1, ref partAtCoordinates);
                            SaveBlockData();
                        }
                        return;
                    }
                }
                return;
            }*/

            if (!selectedPartType.HasValue)
                return;

            /*if (!PlayerPersistentData.PlayerData.CanAffordPart((PART_TYPE)selectedPartType, SelectedPartLevel, true))
            {
                droneDesignUi.DisplayInsufficientResources();
                return;
            }*/

            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                IAttachable attachableAtCoordinates = scrapBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);

                if (attachableAtCoordinates != null)
                {
                    continue;
                }

                var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(selectedPartType.Value, SelectedPartLevel);
                //TODO Should be checking if the player does in-fact have the part in their storage
                PlayerPersistentData.PlayerData.RemovePartFromStorage(attachable.ToBlockData());
                droneDesignUi.RefreshScrollViews();
                scrapBot.AttachNewBit(mouseCoordinate, attachable);
                _toUndoStack.Push(new ScrapyardEditData
                {
                    EventType = SCRAPYARD_ACTION.EQUIP,
                    Coordinate = mouseCoordinate,
                    PartType = (PART_TYPE)selectedPartType
                });
                _toRedoStack.Clear();

                selectedPartType = null;
                SelectedPartLevel = 0;
                SaveBlockData();
            }
            UpdateFloatingMarkers(false);
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
                IAttachable attachableAtCoordinates = scrapBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);

                if (attachableAtCoordinates == null)
                {
                    continue;
                }

                if (attachableAtCoordinates is ScrapyardPart scrapPart)
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
                }
                scrapBot.TryRemoveAttachableAt(mouseCoordinate, false);
                //droneDesignUi.UpdateResources(PlayerPersistentData.PlayerData.GetResources());
                SaveBlockData();
            }
            UpdateFloatingMarkers(false);
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
                    foreach (ScrapyardBot scrapBot in _scrapyardBots)
                    {
                        PlayerPersistentData.PlayerData.AddPartToStorage
                            (((ScrapyardPart)scrapBot.attachedBlocks.FirstOrDefault(a => a.Coordinate == toUndo.Coordinate)).ToBlockData());

                        scrapBot.TryRemoveAttachableAt(toUndo.Coordinate, false);
                        droneDesignUi.RefreshScrollViews();
                        SaveBlockData();
                    }
                    break;
                case SCRAPYARD_ACTION.UNEQUIP:
                    foreach (ScrapyardBot scrapBot in _scrapyardBots)
                    {
                        var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(toUndo.PartType, toUndo.Level);
                        PlayerPersistentData.PlayerData.RemovePartFromStorage(attachable.ToBlockData());
                        scrapBot.AttachNewBit(toUndo.Coordinate, attachable);
                        droneDesignUi.RefreshScrollViews();
                        SaveBlockData();
                    }
                    break;

                case SCRAPYARD_ACTION.PURCHASE:
                    foreach (ScrapyardBot scrapBot in _scrapyardBots)
                    {
                        scrapBot.TryRemoveAttachableAt(toUndo.Coordinate, true);
                        droneDesignUi.UpdateResourceElements();
                        SaveBlockData();
                    }
                    break;
                case SCRAPYARD_ACTION.UPGRADE:
                    foreach (ScrapyardBot scrapBot in _scrapyardBots)
                    {
                        IAttachable attachableAtCoordinates = scrapBot.attachedBlocks.GetAttachableAtCoordinates(toUndo.Coordinate);
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
                    foreach (ScrapyardBot scrapBot in _scrapyardBots)
                    {
                        IAttachable attachableAtCoordinates = scrapBot.attachedBlocks.GetAttachableAtCoordinates(toUndo.Coordinate);
                        if (attachableAtCoordinates != null)
                            return;

                        var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>(toUndo.PartType, toUndo.Level);
                        playerData.SubtractResources(toUndo.PartType, toUndo.Level, true);
                        scrapBot.AttachNewBit(toUndo.Coordinate, attachable);
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
                    foreach (ScrapyardBot scrapBot in _scrapyardBots)
                    {
                        var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(toRedo.PartType, toRedo.Level);
                        PlayerPersistentData.PlayerData.RemovePartFromStorage(attachable.ToBlockData());
                        scrapBot.AttachNewBit(toRedo.Coordinate, attachable);
                        droneDesignUi.RefreshScrollViews();
                        SaveBlockData();
                    }
                    break;
                case SCRAPYARD_ACTION.UNEQUIP:
                    foreach (ScrapyardBot scrapBot in _scrapyardBots)
                    {
                        PlayerPersistentData.PlayerData.AddPartToStorage
                            (((ScrapyardPart)scrapBot.attachedBlocks.FirstOrDefault(a => a.Coordinate == toRedo.Coordinate)).ToBlockData());

                        scrapBot.TryRemoveAttachableAt(toRedo.Coordinate, false);
                        droneDesignUi.RefreshScrollViews();
                        SaveBlockData();
                    }
                    break;


                case SCRAPYARD_ACTION.PURCHASE:
                    foreach (ScrapyardBot scrapBot in _scrapyardBots)
                    {
                        IAttachable attachableAtCoordinates = scrapBot.attachedBlocks.GetAttachableAtCoordinates(toRedo.Coordinate);
                        if (attachableAtCoordinates != null)
                            return;

                        if (!playerData.CanAffordPart(toRedo.PartType, 0, false))
                            return;

                        var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>(toRedo.PartType, 0);
                        playerData.SubtractResources(toRedo.PartType, 0, false);
                        scrapBot.AttachNewBit(toRedo.Coordinate, attachable);
                        droneDesignUi.UpdateResourceElements();
                        SaveBlockData();
                    }
                    break;
                case SCRAPYARD_ACTION.UPGRADE:
                    foreach (ScrapyardBot scrapBot in _scrapyardBots)
                    {
                        IAttachable attachableAtCoordinates = scrapBot.attachedBlocks.GetAttachableAtCoordinates(toRedo.Coordinate);
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
                    foreach (ScrapyardBot scrapBot in _scrapyardBots)
                    {
                        scrapBot.TryRemoveAttachableAt(toRedo.Coordinate, true);
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
                saveLayout = new ScrapyardLayout(layoutName, _scrapyardBots[0].GetBlockDatas());
            }
            else
            {
                _scrapyardLayouts.Add(new ScrapyardLayout(layoutName, _scrapyardBots[0].GetBlockDatas()));
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
            foreach (var attachable in _scrapyardBots[0].attachedBlocks)
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

            for (int i = _scrapyardBots[0].attachedBlocks.Count - 1; i >= 0; i--)
            {
                if (_scrapyardBots[0].attachedBlocks[i].Coordinate != Vector2Int.zero)
                {
                    _scrapyardBots[0].TryRemoveAttachableAt(_scrapyardBots[0].attachedBlocks[i].Coordinate, false);
                }
            }

            foreach (var attachable in tempLayout.BlockData.ImportBlockDatas(true))
            {
                _scrapyardBots[0].AttachNewBit(attachable.Coordinate, attachable);
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
            foreach (ScrapyardBot scrapyardbot in _scrapyardBots)
            {
                PlayerPersistentData.PlayerData.SetCurrentBlockData(scrapyardbot.attachedBlocks.GetBlockDatas());
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
            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                List<ScrapyardBit> listBits = scrapBot.attachedBlocks.OfType<ScrapyardBit>().ToList();


                List<Component> listComponents = scrapBot.attachedBlocks.OfType<Component>().ToList();
                if (listComponents.Count > 0)
                {
                    scrapBot.RemoveAllComponents();

                    //TODO Need to think about if I should be displaying the components processed or not
                    foreach (var component in listComponents)
                    {
                        PlayerPersistentData.PlayerData.AddComponent(component.Type, 1);
                    }

                    PlayerData.OnValuesChanged?.Invoke();
                    SaveBlockData();
                }


                if (listBits.Count == 0)
                    continue;

                var scrapyardBits = scrapBot.attachedBlocks.OfType<ScrapyardBit>();

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


                scrapBot.RemoveAllBits();



                SaveBlockData();

                droneDesignUi.UpdateResourceElements();
            }
        }

        public void RotateBots(float direction)
        {
            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                scrapBot.Rotate(direction);
            }
        }

        public void UpdateFloatingMarkers(bool showAvailable)
        {
            foreach (var availablePoint in _availablePointMarkers)
            {
                Recycler.Recycle(ICONS.AVAILABLE, availablePoint);
            }
            _availablePointMarkers.Clear();

            if (showAvailable)
            {
                foreach (ScrapyardBot scrapBot in _scrapyardBots)
                {
                    foreach (var attached in scrapBot.attachedBlocks)
                    {
                        if (!scrapBot.attachedBlocks.HasPathToCore(attached))
                            continue;

                        if (scrapBot.attachedBlocks.FindAll(a => a.Coordinate == attached.Coordinate + Vector2.left && scrapBot.attachedBlocks.HasPathToCore(a)).Count == 0)
                        {
                            if (!Recycler.TryGrab(ICONS.AVAILABLE, out GameObject availableMarker))
                                availableMarker = GameObject.Instantiate(availablePointMarkerPrefab);
                            availableMarker.transform.position = (Vector3)(attached.Coordinate + Vector2.left) * Constants.gridCellSize + Vector3.back;
                            _availablePointMarkers.Add(availableMarker);
                        }
                        if (scrapBot.attachedBlocks.FindAll(a => a.Coordinate == attached.Coordinate + Vector2.right && scrapBot.attachedBlocks.HasPathToCore(a)).Count == 0)
                        {
                            if (!Recycler.TryGrab(ICONS.AVAILABLE, out GameObject availableMarker))
                                availableMarker = GameObject.Instantiate(availablePointMarkerPrefab);
                            availableMarker.transform.position = (Vector3)(attached.Coordinate + Vector2.right) * Constants.gridCellSize + Vector3.back;
                            _availablePointMarkers.Add(availableMarker);
                        }
                        if (scrapBot.attachedBlocks.FindAll(a => a.Coordinate == attached.Coordinate + Vector2.up && scrapBot.attachedBlocks.HasPathToCore(a)).Count == 0)
                        {
                            if (!Recycler.TryGrab(ICONS.AVAILABLE, out GameObject availableMarker))
                                availableMarker = GameObject.Instantiate(availablePointMarkerPrefab);
                            availableMarker.transform.position = (Vector3)(attached.Coordinate + Vector2.up) * Constants.gridCellSize + Vector3.back;
                            _availablePointMarkers.Add(availableMarker);
                        }
                        if (scrapBot.attachedBlocks.FindAll(a => a.Coordinate == attached.Coordinate + Vector2.down && scrapBot.attachedBlocks.HasPathToCore(a)).Count == 0)
                        {
                            if (!Recycler.TryGrab(ICONS.AVAILABLE, out GameObject availableMarker))
                                availableMarker = GameObject.Instantiate(availablePointMarkerPrefab);
                            availableMarker.transform.position = (Vector3)(attached.Coordinate + Vector2.down) * Constants.gridCellSize + Vector3.back;
                            _availablePointMarkers.Add(availableMarker);
                        }
                    }
                }
            }

            foreach (var partWarning in _floatingPartWarnings)
            {
                Recycler.Recycle(ICONS.ALERT, partWarning);
            }
            _floatingPartWarnings.Clear();

            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                foreach (var attached in scrapBot.attachedBlocks)
                {
                    if (!scrapBot.attachedBlocks.HasPathToCore(attached))
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
            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                if (scrapBot.CheckHasDisconnects())
                {
                    return false;
                }
            }

            return true;
        }

        public void ProcessScrapyardUsageEndAnalytics()
        {
            Dictionary<string, object> scrapyardUsageEndAnalyticsDictionary = new Dictionary<string, object>();
            AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.ScrapyardUsageEnd, scrapyardUsageEndAnalyticsDictionary);
        }

        #endregion //Other

        //============================================================================================================//
    }
}

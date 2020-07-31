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

        [Sirenix.OdinInspector.Button("Clear Remote Data")]
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
        }

        //============================================================================================================//

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
            SellBits();
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

        //============================================================================================================//

        public void SellBits()
        {
            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                List<ScrapyardBit> listBits = scrapBot.attachedBlocks.OfType<ScrapyardBit>().ToList();
                if (listBits.Count == 0)
                    continue;
                
                Dictionary<BIT_TYPE, int> bits = FactoryManager.Instance.GetFactory<BitAttachableFactory>().GetTotalResources(scrapBot.attachedBlocks.OfType<ScrapyardBit>());
                PlayerPersistentData.PlayerData.AddResources(bits);
                string resourcesGained = "";
                foreach (var resource in bits)
                {
                    resourcesGained += resource.Key + ": " + resource.Value + "\n";
                }
                Alert.ShowAlert("Bits Sold", resourcesGained, "Okay", null);
                scrapBot.RemoveAllBits();
                SaveBlockData();
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
            if (ctx.ReadValue<float>() == 1f)
            {
                UpdateFloatingMarkers(true);
                return;
            }
            UpdateFloatingMarkers(false);

            if (!TryGetMouseCoordinate(out Vector2Int mouseCoordinate))
                return;

            if (IsUpgrading)
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
            }

            if (selectedPartType == null)
                return;

            if (!PlayerPersistentData.PlayerData.CanAffordPart((PART_TYPE)selectedPartType, SelectedPartLevel, true))
            {
                droneDesignUi.DisplayInsufficientResources();
                return;
            }

            foreach (ScrapyardBot scrapBot in _scrapyardBots)
            {
                IAttachable attachableAtCoordinates = scrapBot.attachedBlocks.GetAttachableAtCoordinates(mouseCoordinate);

                if (attachableAtCoordinates != null)
                {
                    continue;
                }

                var playerData = PlayerPersistentData.PlayerData;
                var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>((PART_TYPE)selectedPartType, 0);
                playerData.SubtractResources((PART_TYPE)selectedPartType, 0, false);
                scrapBot.AttachNewBit(mouseCoordinate, attachable);
                _toUndoStack.Push(new ScrapyardEditData
                {
                    EventType = SCRAPYARD_ACTION.PURCHASE,
                    Coordinate = mouseCoordinate,
                    PartType = (PART_TYPE)selectedPartType
                });
                _toRedoStack.Clear();

                droneDesignUi.UpdateResources(playerData.GetResources());
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

                if (attachableAtCoordinates is ScrapyardBit scrapBit)
                {
                    _toUndoStack.Push(new ScrapyardEditData
                    {
                        EventType = SCRAPYARD_ACTION.SALE,
                        Coordinate = mouseCoordinate,
                        BitType = scrapBit.Type,
                        Level = scrapBit.level
                    });
                    _toRedoStack.Clear();
                }
                else if (attachableAtCoordinates is ScrapyardPart scrapPart)
                {
                    _toUndoStack.Push(new ScrapyardEditData
                    {
                        EventType = SCRAPYARD_ACTION.SALE,
                        Coordinate = mouseCoordinate,
                        PartType = scrapPart.Type,
                        Level = scrapPart.level
                    });
                    _toRedoStack.Clear();
                }

                scrapBot.TryRemoveAttachableAt(mouseCoordinate, true);
                droneDesignUi.UpdateResources(PlayerPersistentData.PlayerData.GetResources());
                SaveBlockData();
            }
            UpdateFloatingMarkers(false);
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

        //============================================================================================================//

        public void UndoStackPop()
        {
            if (_toUndoStack.Count == 0)
                return;

            ScrapyardEditData toUndo = _toUndoStack.Pop();
            var playerData = PlayerPersistentData.PlayerData;

            switch (toUndo.EventType)
            {
                case SCRAPYARD_ACTION.PURCHASE:
                    foreach (ScrapyardBot scrapBot in _scrapyardBots)
                    {
                        scrapBot.TryRemoveAttachableAt(toUndo.Coordinate, true);
                        droneDesignUi.UpdateResources(playerData.GetResources());
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
                            droneDesignUi.UpdateResources(playerData.GetResources());
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

                        if (!playerData.CanAffordPart(toUndo.PartType, toUndo.Level, true))
                            return;

                        var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>(toUndo.PartType, toUndo.Level);
                        playerData.SubtractResources(toUndo.PartType, toUndo.Level, true);
                        scrapBot.AttachNewBit(toUndo.Coordinate, attachable);
                        droneDesignUi.UpdateResources(playerData.GetResources());
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
                        droneDesignUi.UpdateResources(playerData.GetResources());
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
                            droneDesignUi.UpdateResources(playerData.GetResources());
                            FactoryManager.Instance.GetFactory<PartAttachableFactory>().UpdatePartData(scrapyardPart.Type, scrapyardPart.level + 1, ref scrapyardPart);
                            SaveBlockData();
                        }
                    }
                    break;
                case SCRAPYARD_ACTION.SALE:
                    foreach (ScrapyardBot scrapBot in _scrapyardBots)
                    {
                        scrapBot.TryRemoveAttachableAt(toRedo.Coordinate, true);
                        droneDesignUi.UpdateResources(playerData.GetResources());
                        SaveBlockData();
                    }
                    break;
            }

            UpdateFloatingMarkers(false);
            _toUndoStack.Push(toRedo);
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
        //Keep an eye on this - currently it will update the player block data each time there is a change
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
           SceneLoader.ActivateScene("AlexShulmanTestScene", "ScrapyardScene");
        }

        public void ProcessScrapyardUsageEndAnalytics()
        {
            Dictionary<string, object> scrapyardUsageEndAnalyticsDictionary = new Dictionary<string, object>();
            AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.ScrapyardUsageEnd, scrapyardUsageEndAnalyticsDictionary);
        }

        //============================================================================================================//

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

            Dictionary<BIT_TYPE, int> resourceComparer = PlayerPersistentData.PlayerData.GetResources();
            foreach (var attachable in _scrapyardBots[0].attachedBlocks)
            {
                if (attachable is ScrapyardPart part && part.Coordinate != Vector2Int.zero)
                {
                    ResourceCalculations.AddResources(ref resourceComparer, part.Type, part.level, true);
                }
            }

            List<IAttachable> newLayoutAttachables = tempLayout.BlockData.ImportBlockDatas(true);

            foreach (var attachable in newLayoutAttachables)
            {
                if (attachable is ScrapyardPart part && part.Coordinate != Vector2Int.zero)
                {
                    if (ResourceCalculations.CanAffordPart(resourceComparer, part.Type, part.level, true))
                    {
                        ResourceCalculations.SubtractResources(ref resourceComparer, part.Type, part.level, true);
                    }
                    else
                    {
                        //CANT AFFORD LAYOUT
                    }
                }
            }

            _currentLayout = tempLayout;

            PlayerPersistentData.PlayerData.resources = resourceComparer;
            for (int i = _scrapyardBots[0].attachedBlocks.Count - 1; i >= 0; i--)
            {
                if (_scrapyardBots[0].attachedBlocks[i].Coordinate != Vector2Int.zero)
                {
                    _scrapyardBots[0].TryRemoveAttachableAt(_scrapyardBots[0].attachedBlocks[i].Coordinate, false);
                }
            }

            foreach (var attachable in newLayoutAttachables)
            {
                _scrapyardBots[0].AttachNewBit(attachable.Coordinate, attachable);
            }
            droneDesignUi.UpdateResources(PlayerPersistentData.PlayerData.resources);
        }

        public bool CheckAffordLayout(ScrapyardLayout layout)
        {
            PlayerPersistentData.PlayerData.GetResources();

            return true;
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
    }
}
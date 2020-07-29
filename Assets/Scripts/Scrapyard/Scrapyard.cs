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
using StarSalvager.Factories.Data;
using Recycling;
using System.IO;

namespace StarSalvager
{
    public class Scrapyard : AttachableEditorToolBase, IReset, IInput
    {
        [SerializeField]
        private ScrapyardUI m_scrapyardUI;

        public bool IsUpgrading;

        [SerializeField]
        private GameObject floatingPartWarningPrefab;
        [SerializeField]
        private GameObject availablePointMarkerPrefab;

        private List<GameObject> _floatingPartWarnings;
        private List<GameObject> _availablePointMarkers;

        private Stack<ScrapyardEditData> _toUndoStack;
        private Stack<ScrapyardEditData> _toRedoStack;

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
            IsUpgrading = false;
            //InputManager.Instance.InitInput();
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
            UpdateFloatingMarkers();
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
                Dictionary<BIT_TYPE, int> bits = FactoryManager.Instance.GetFactory<BitAttachableFactory>().GetTotalResources(scrapBot.attachedBlocks.OfType<ScrapyardBit>());
                PlayerPersistentData.PlayerData.AddResources(bits);
                scrapBot.RemoveAllBits();
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
                return;

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

                            if (!PlayerPersistentData.PlayerData.CanAffordPart(partAtCoordinates.Type, partAtCoordinates.level + 1))
                            {
                                m_scrapyardUI.DisplayInsufficientResources();
                                return;
                            }

                            playerData.SubtractResources(partAtCoordinates.Type, partAtCoordinates.level + 1);
                            m_scrapyardUI.UpdateResources(playerData.GetResources());
                            _toUndoStack.Push(new ScrapyardEditData
                            {
                                EventType = SCRAPYARD_ACTION.UPGRADE,
                                Coordinate = mouseCoordinate,
                                PartType = partAtCoordinates.Type
                            });
                            _toRedoStack.Clear();
                            FactoryManager.Instance.GetFactory<PartAttachableFactory>().UpdatePartData(partAtCoordinates.Type, partAtCoordinates.level + 1, ref partAtCoordinates);
                        }
                        return;
                    }
                }
                return;
            }

            if (selectedPartType == null)
                return;

            if (!PlayerPersistentData.PlayerData.CanAffordPart((PART_TYPE)selectedPartType, selectedpartLevel))
            {
                m_scrapyardUI.DisplayInsufficientResources();
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
                playerData.SubtractResources((PART_TYPE)selectedPartType, 0);
                scrapBot.AttachNewBit(mouseCoordinate, attachable);
                _toUndoStack.Push(new ScrapyardEditData
                {
                    EventType = SCRAPYARD_ACTION.PURCHASE,
                    Coordinate = mouseCoordinate,
                    PartType = (PART_TYPE)selectedPartType
                });
                _toRedoStack.Clear();


                m_scrapyardUI.UpdateResources(playerData.GetResources());
            }
            UpdateFloatingMarkers();
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
                m_scrapyardUI.UpdateResources(PlayerPersistentData.PlayerData.GetResources());
            }
            UpdateFloatingMarkers();
        }

        private void UpdateFloatingMarkers()
        {
            foreach (var availablePoint in _availablePointMarkers)
            {
                Recycler.Recycle(ICONS.AVAILABLE, availablePoint);
            }
            _availablePointMarkers.Clear();

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
                        availableMarker.transform.position = (attached.Coordinate + Vector2.left) * Constants.gridCellSize;
                        _availablePointMarkers.Add(availableMarker);
                    }
                    if (scrapBot.attachedBlocks.FindAll(a => a.Coordinate == attached.Coordinate + Vector2.right && scrapBot.attachedBlocks.HasPathToCore(a)).Count == 0)
                    {
                        if (!Recycler.TryGrab(ICONS.AVAILABLE, out GameObject availableMarker))
                            availableMarker = GameObject.Instantiate(availablePointMarkerPrefab);
                        availableMarker.transform.position = (attached.Coordinate + Vector2.right) * Constants.gridCellSize;
                        _availablePointMarkers.Add(availableMarker);
                    }
                    if (scrapBot.attachedBlocks.FindAll(a => a.Coordinate == attached.Coordinate + Vector2.up && scrapBot.attachedBlocks.HasPathToCore(a)).Count == 0)
                    {
                        if (!Recycler.TryGrab(ICONS.AVAILABLE, out GameObject availableMarker))
                            availableMarker = GameObject.Instantiate(availablePointMarkerPrefab);
                        availableMarker.transform.position = (attached.Coordinate + Vector2.up) * Constants.gridCellSize;
                        _availablePointMarkers.Add(availableMarker);
                    }
                    if (scrapBot.attachedBlocks.FindAll(a => a.Coordinate == attached.Coordinate + Vector2.down && scrapBot.attachedBlocks.HasPathToCore(a)).Count == 0)
                    {
                        if (!Recycler.TryGrab(ICONS.AVAILABLE, out GameObject availableMarker))
                            availableMarker = GameObject.Instantiate(availablePointMarkerPrefab);
                        availableMarker.transform.position = (attached.Coordinate + Vector2.down) * Constants.gridCellSize;
                        _availablePointMarkers.Add(availableMarker);
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
                        newWarning.transform.position = (Vector2)attached.Coordinate * Constants.gridCellSize;
                        _floatingPartWarnings.Add(newWarning);
                    }
                }
            }
        }

        //============================================================================================================//

        public void UndoStackPop()
        {
            ScrapyardEditData toUndo = _toUndoStack.Pop();

            switch(toUndo.EventType)
            {
                case SCRAPYARD_ACTION.PURCHASE:

                    break;
                case SCRAPYARD_ACTION.UPGRADE:

                    break;
                case SCRAPYARD_ACTION.SALE:

                    break;
            }

            _toRedoStack.Push(toUndo);
        }

        public void RedoStackPop()
        {
            ScrapyardEditData toRedo = _toRedoStack.Pop();


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

    }
}
using System;
using Sirenix.OdinInspector;
using StarSalvager.Utilities;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using System.Collections.Generic;
using StarSalvager.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using StarSalvager.Utilities.Saving;
using Recycling;
using StarSalvager.Audio;
using StarSalvager.UI.Hints;
using StarSalvager.UI.Wreckyard.PatchTrees;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Interfaces;
using StarSalvager.Utilities.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace StarSalvager.UI
{
    public class UniverseMap : MonoBehaviour, IReset, IHasHintElement, IStartedUsingController
    {
        //#3658df
        private static Color LINE_COLOR = new Color(0.2117f, 0.34509f, 0.874509f);
        private static Color LINE_COLOR_FADE = new Color(0.2117f, 0.34509f, 0.874509f, 0.5f);

        #region Properties

        [FormerlySerializedAs("m_universeSectorButtonPrefab")] [SerializeField, Required]
        private UniverseMapButton universeSectorButtonPrefab;

        [SerializeField, Required] private ScrollRect m_scrollRect;
        [SerializeField, Required] private RectTransform m_scrollRectArea;

        [SerializeField, Required] private Button backButton;
        [SerializeField, Required] private TMP_Text backButtonText;

        [SerializeField]
        private Vector2 offsetAmount;

        //====================================================================================================================//

        //[SerializeField]
        private UniverseMapButton[] _universeMapButtons;

        [FormerlySerializedAs("dottedLineImage")] [SerializeField]
        private Image dottedLineImagePrefab;

        private List<Image> _connectionImages;


        //====================================================================================================================//

        [SerializeField] private RectTransform botDisplayRectTransform;

        //private RectTransform _shipwreckButtonRectTransform;

        #endregion //Properties

        //Unity Functions
        //============================================================================================================//
        private void OnEnable()
        {
            InputManager.AddStartedControllerListener(this);
        }
        private void Start()
        {
            backButton.onClick.AddListener(Back);
        }
        
        private void OnDisable()
        {
            InputManager.RemoveControllerListener(this);
        }

        //====================================================================================================================//

        public object[] GetHintElements(HINT hint)
        {
            switch (hint)
            {
                case HINT.NONE:
                    return null;
                /*case HINT.HOME:
                    return new object[]
                    {
                        _shipwreckButtonRectTransform
                    };*/
                default:
                    throw new ArgumentOutOfRangeException(nameof(hint), hint, null);
            }
        }

        //IReset Functions
        //====================================================================================================================//

        public void Activate()
        {

            InitButtons();
            InitBackButton();
            DrawMap();

            PlayerDataManager.GetBotBlockDatas().CreateBotPreview(botDisplayRectTransform);

            //Wait until the map is generated to try and highlight
            StartedUsingController(InputManager.Instance.UsingController);
        }

        public void Reset()
        {
            if (_connectionImages.IsNullOrEmpty()) return;
            
            for (int i = _connectionImages.Count - 1; i >= 0; i--)
            {
                Destroy(_connectionImages[i].gameObject);
            }

            _connectionImages.Clear();
        }



        //UniverseMap Functions
        //====================================================================================================================//

        private void InitButtons()
        {
            //--------------------------------------------------------------------------------------------------------//

            void CreateButtonElement(in int index, in Vector2Int coordinate, in NodeType nodeType)
            {
                if (!Recycler.TryGrab(out UniverseMapButton button))
                {
                    button = Instantiate(universeSectorButtonPrefab, m_scrollRectArea);
                }
                else
                {
                    button.transform.SetParent(m_scrollRectArea, false);
                }

                _universeMapButtons[index] = button;

                _universeMapButtons[index].Reset();
                _universeMapButtons[index].Init(index, coordinate.x, nodeType, OnNodePressed);

                _universeMapButtons[index].gameObject.name = $"{nameof(UniverseMapButton)}_[{index}]";

                var sizeX = _universeMapButtons[index].transform.sizeDelta.x;

                var xCoord = coordinate.x;

                var anchoredPositionOffset = Vector2.right * (xCoord * sizeX * 2f);

                //Offset based on https://agamestudios.atlassian.net/browse/SS-187
                //--------------------------------------------------------------------------------------------------------//
                
                var sectorNodeCount = Rings.RingMaps[Globals.CurrentRingIndex].Nodes
                    .Count(x => x.Coordinate.x == xCoord);
                
                if(sectorNodeCount == 2)
                    anchoredPositionOffset += Vector2.up * (coordinate.y * sizeX);
                else
                    anchoredPositionOffset += Vector2.up * (coordinate.y * sizeX * 2f);

                //--------------------------------------------------------------------------------------------------------//
                
                anchoredPositionOffset += new Vector2(
                    Random.Range(-offsetAmount.x, offsetAmount.x),
                    Random.Range(-offsetAmount.y, offsetAmount.y));

                _universeMapButtons[index].transform.anchoredPosition += anchoredPositionOffset;

            }

            void CleanButtons()
            {
                if (!_universeMapButtons.IsNullOrEmpty())
                {
                    for (var i = _universeMapButtons.Length - 1; i >= 0; i--)
                    {
                        //FIXME Need to determine how best to reset positions
                        //Recycler.Recycle<UniverseMapButton>(_universeMapButtons[i]);
                        Destroy(_universeMapButtons[i].gameObject);
                    }
                }

                _universeMapButtons = new UniverseMapButton[0];
            }

            //--------------------------------------------------------------------------------------------------------//

            CleanButtons();

            //Generate the buttons
            //--------------------------------------------------------------------------------------------------------//
            var ring = Rings.RingMaps[Globals.CurrentRingIndex];
            var nodeCount = ring.Nodes.Length;

            var currentNodeIndex = ring.GetIndexFromCoordinate(PlayerDataManager.GetPlayerCoordinate());
            //If the player opens the map screen from the final node, we should then open the new map/ring
            if (currentNodeIndex == nodeCount - 1)
            {
                var ringIndex = Globals.CurrentRingIndex + 1;
                PlayerDataManager.SetCurrentRing(ringIndex);

                //Want to set the players map position back to the beginning
                PlayerDataManager.SetPlayerCoordinate(Vector2Int.zero);
                PlayerDataManager.SetPlayerTargetCoordinate(Vector2Int.zero);

                //Want to reset the players wave to first of this ring
                PlayerDataManager.SetCurrentWave(0);
                //Need to clear traversal history
                PlayerDataManager.ResetTraversedCoordinates();

                //Once all new values are set, re-attempt to do this function
                InitButtons();
                return;
            }


            _universeMapButtons = new UniverseMapButton[nodeCount];
            for (var i = 0; i < nodeCount; i++)
            {
                var nodeData = ring.Nodes[i];
                CreateButtonElement(i, nodeData.Coordinate, nodeData.NodeType);
            }

        }

        private void OnNodePressed(int nodeIndex, NodeType nodeType)
        {
            var currentRingMap = Rings.RingMaps[Globals.CurrentRingIndex];

            switch (nodeType)
            {
                case NodeType.Base:
                    //PlayerDataManager.SetCurrentWave(PlayerDataManager.GetCurrentWave() + 1);
                    PlayerDataManager.SetPlayerCoordinate(currentRingMap.Nodes[nodeIndex].Coordinate);

                    ScreenFade.Fade(DrawMap);
                    break;
                case NodeType.Level:
                    PlayerDataManager.SetPlayerTargetCoordinate(currentRingMap.Nodes[nodeIndex].Coordinate);

                    //Globals.CurrentRingIndex = 0;
                    Globals.CurrentWave = PlayerDataManager.GetCurrentWave();

                    ScreenFade.Fade(() =>
                    {
                        SceneLoader.ActivateScene(SceneLoader.LEVEL, SceneLoader.UNIVERSE_MAP);
                    });
                    break;
                case NodeType.Wreck:
                    PlayerDataManager.SetPlayerCoordinate(currentRingMap.Nodes[nodeIndex].Coordinate);

                    ScreenFade.Fade(() =>
                    {
                        SceneLoader.ActivateScene(SceneLoader.WRECKYARD, SceneLoader.UNIVERSE_MAP, MUSIC.SCRAPYARD);
                        AnalyticsManager.WreckStartEvent();
                        FindObjectOfType<PatchTreeUI>().InitWreck("Wreck", null);
                    });
                    break;
            }
        }

        //============================================================================================================//


        private void DrawMap()
        {
            var currentRingMap = Rings.RingMaps[Globals.CurrentRingIndex];

            var playerCoordinate = PlayerDataManager.GetPlayerCoordinate();
            var playerCoordinateIndex = GetPlayerCoordinateIndex(currentRingMap);

            CenterToItem(_universeMapButtons[playerCoordinateIndex].transform);

            //Setup Nodes
            //--------------------------------------------------------------------------------------------------------//

            for (var i = 0; i < _universeMapButtons.Length; i++)
            {
                var currentMapButton = _universeMapButtons[i];
                var isWreck = currentMapButton.NodeType == NodeType.Wreck;

                currentMapButton.SetBotImageActive(i == playerCoordinateIndex);

                if (i == playerCoordinateIndex && isWreck)
                {
                    currentMapButton.SetButtonInteractable(true);
                    continue;
                }

                currentMapButton.SetButtonInteractable(false);

            }

            //Try get list of path that should be marked as previously travelled
            //--------------------------------------------------------------------------------------------------------//

            List<Vector2Int> traversedConnections = new List<Vector2Int>();
            var traversedCoordinates = new List<Vector2Int>(PlayerDataManager.GetTraversedCoordinates());
            if (traversedCoordinates.Count > 1)
            {
                for (var i = 1; i < traversedCoordinates.Count; i++)
                {
                    var previousIndex = currentRingMap.GetIndexFromCoordinate(traversedCoordinates[i - 1]);
                    var currentIndex = currentRingMap.GetIndexFromCoordinate(traversedCoordinates[i]);
                    traversedConnections.Add(new Vector2Int(previousIndex, currentIndex));
                }
            }

            //Draw connections
            //--------------------------------------------------------------------------------------------------------//

            for (var i = 0; i < currentRingMap.Connections.Length; i++)
            {
                var connectionColor = LINE_COLOR;
                var connection = currentRingMap.Connections[i];

                var canTravelToNext = playerCoordinateIndex == connection.x;
                var drawDottedLine = !canTravelToNext;

                var startConnectionCoordinate = currentRingMap.GetCoordinateFromIndex(connection.x);
                var endConnectionCoordinate = currentRingMap.GetCoordinateFromIndex(connection.y);

                //If the player has taken this path, we want to showcase the history of the traversal
                if (traversedConnections.Contains(connection))
                {
                    connectionColor = LINE_COLOR_FADE;
                    drawDottedLine = false;
                }
                //Hide all the lines that weren't traversed behind the players coordinate
                else if (endConnectionCoordinate.x <= playerCoordinate.x ||
                         //Hide any lines emanating from nodes adjacent to the player that are impossible to traverse from
                         (startConnectionCoordinate.x == playerCoordinate.x &&
                          startConnectionCoordinate.y != playerCoordinate.y))
                {
                    connectionColor = Color.clear;
                }

                DrawConnection(connection.x, connection.y, drawDottedLine, connectionColor);

                //If another iteration set this node to active, we don't want to cancel that out
                if (_universeMapButtons[connection.y].IsButtonInteractable == false)
                    _universeMapButtons[connection.y].SetButtonInteractable(canTravelToNext);
            }

            /*if (playerNodeLocation + 1 < _universeMapButtons.Length)
                _universeMapButtons[playerNodeLocation + 1].SetButtonInteractable(true);*/

            //Check to see if the wreck is ahead of the player and can be interacted with
            //--------------------------------------------------------------------------------------------------------//

            var unlockedWreck = _universeMapButtons
                .FirstOrDefault(x =>
                    playerCoordinateIndex != 0 && x.IsButtonInteractable && x.NodeType == NodeType.Wreck);

            if (HintManager.CanShowHint(HINT.WRECK) && unlockedWreck != null)
            {
                HintManager.TryShowHint(HINT.WRECK, ScreenFade.DEFAULT_TIME, unlockedWreck.transform);
            }

            //--------------------------------------------------------------------------------------------------------//

        }


        private void DrawConnection(int connectionStart, int connectionEnd, bool dottedLine, Color color)
        {
            if (_connectionImages == null)
                _connectionImages = new List<Image>();
            
            //DrawConnection(connectionStart, connectionEnd, dottedLine, Color.white);
            Image connectionImage;
            if (dottedLine)
                connectionImage = UILineCreator.DrawConnection(m_scrollRectArea.transform,
                    _universeMapButtons[connectionStart].transform,
                    _universeMapButtons[connectionEnd].transform,
                    dottedLineImagePrefab,
                    color);
            else
            {
                connectionImage = UILineCreator.DrawConnection(m_scrollRectArea.transform,
                    _universeMapButtons[connectionStart].transform,
                    _universeMapButtons[connectionEnd].transform,
                    color);
            }
            
            _connectionImages.Add(connectionImage);
        }

        private static int GetPlayerCoordinateIndex()
        {
            var currentRingMap = Rings.RingMaps[Globals.CurrentRingIndex];
            return GetPlayerCoordinateIndex(currentRingMap);
        }
        private static int GetPlayerCoordinateIndex(in Ring ring)
        {
            var playerCoordinate = PlayerDataManager.GetPlayerCoordinate();
            return ring.GetIndexFromCoordinate(PlayerDataManager.GetPlayerCoordinate());
        }
        
        //Buttons Pressed Functions
        //====================================================================================================================//
        

        private void InitBackButton()
        {
            switch (SceneLoader.PreviousScene)
            {
                case SceneLoader.LEVEL:
                    backButtonText.text = "Menu";
                    break;
                case SceneLoader.MAIN_MENU:
                case SceneLoader.WRECKYARD:
                    backButtonText.text = "Back";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(SceneLoader.PreviousScene), SceneLoader.PreviousScene,
                        null);
            }
        }
        private void Back()
        {
            switch (SceneLoader.PreviousScene)
            {
                case SceneLoader.LEVEL:
                    ScreenFade.Fade(() =>
                    {
                        SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.UNIVERSE_MAP, MUSIC.MAIN_MENU, true);
                    });
                    break;

                case SceneLoader.MAIN_MENU:
                case SceneLoader.WRECKYARD:
                    ScreenFade.Fade(() =>
                    {
                        SceneLoader.LoadPreviousScene();
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(SceneLoader.PreviousScene), SceneLoader.PreviousScene,
                        null);
            }
        }

        //============================================================================================================//

        //TODO: ashulman, figure out if/why this works
        private void CenterToItem(RectTransform obj)
        {
            float normalizePositionX = ((m_scrollRectArea.rect.width / 2) + (obj.anchoredPosition.x * 2));
            float normalizePositionY = ((m_scrollRectArea.rect.height / 2) + (obj.anchoredPosition.y * 2));

            m_scrollRect.horizontalNormalizedPosition = normalizePositionX / m_scrollRectArea.rect.width;
            m_scrollRect.verticalNormalizedPosition = normalizePositionY / m_scrollRectArea.rect.height;
        }

        //====================================================================================================================//
        
        public void StartedUsingController(bool usingController)
        {
            if (_universeMapButtons.IsNullOrEmpty())
                return;
            
            if (usingController)
            {
                var playerCoordinateIndex = GetPlayerCoordinateIndex();
                var buttonObject = _universeMapButtons[playerCoordinateIndex].gameObject;

                EventSystem.current.SetSelectedGameObject(buttonObject);
                return;
            }
            
            EventSystem.current.SetSelectedGameObject(null);
        }
        
    }
}

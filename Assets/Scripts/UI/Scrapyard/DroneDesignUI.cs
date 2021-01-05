﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class DroneDesignUI : MonoBehaviour
    {
        public bool CanAffordRepair { get; private set; }

        [SerializeField, Required] 
        private TMP_Text flightDataText;
        
        [SerializeField, Required]
        private PointerEvents repairButtonPointerEvents;
        [SerializeField, Required]
        private GameObject recoveryDroneBannerObject;

        //====================================================================================================================//
        
        [SerializeField, Required, BoxGroup("Part UI")]
        private GameObject partsWindow;
        [SerializeField, Required, BoxGroup("Part UI")]
        private RemotePartProfileScriptableObject remotePartProfileScriptable;

        //============================================================================================================//

        [SerializeField, BoxGroup("Resource UI")]
        private ResourceUIElementScrollView resourceScrollView;
        
        [SerializeField, BoxGroup("Resource UI")]
        private ResourceUIElementScrollView liquidResourceContentView;

        [SerializeField, BoxGroup("Load List UI")]
        private LayoutElementScrollView layoutScrollView;

        //============================================================================================================//

        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button isUpgradingButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button rotateLeftButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button rotateRightButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button undoButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button redoButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button saveLayoutButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button loadLayoutButton;
        
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button repairButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private FadeUIImage repairButtonGlow;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button toggleBotsButton;
        private TMP_Text _repairButtonText;

        [SerializeField, Required, BoxGroup("Load Menu")]
        private GameObject loadMenu;
        [SerializeField, Required, BoxGroup("Load Menu")]
        private Button loadConfirm;
        [SerializeField, Required, BoxGroup("Load Menu")]
        private Button loadReturn;
        [SerializeField, Required, BoxGroup("Load Menu")]
        private Button loadReturn2;
        [SerializeField, Required, BoxGroup("Load Menu")]
        private TMP_Text loadName;

        [SerializeField, Required, BoxGroup("Save Menu")]
        private GameObject saveMenu;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private GameObject saveOverwritePortion;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private GameObject saveBasePortion;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private TMP_InputField saveNameInputField;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button saveConfirm;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button saveReturn;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button saveReturn2;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button saveOverwrite;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button saveAsNew;

        [SerializeField, Required, BoxGroup("Save Menu/Testing")]
        private Slider m_cameraZoomScaler;
        [SerializeField, Required, BoxGroup("Save Menu/Testing")]
        private SliderText _zoomSliderText;

        [SerializeField]
        private CameraController CameraController;

        [SerializeField, Required, BoxGroup("UI Visuals")]
        private Image screenBlackImage;

        public static Action CheckBlueprintNewAlertUpdate;

        //============================================================================================================//

        private DroneDesigner DroneDesigner
        {
            get
            {
                if (_droneDesigner == null)
                    _droneDesigner = FindObjectOfType<DroneDesigner>();

                return _droneDesigner;
            }
        }
        [SerializeField, Required]
        private DroneDesigner _droneDesigner;

        private ScrapyardLayout currentSelected;

        public bool IsPopupActive => loadMenu.activeSelf || saveMenu.activeSelf || Alert.Displayed;
        private bool _currentlyOverwriting;

        private bool _scrollViewsSetup;

        float cameraScaleOnEnter = 71;

        //Unity Functions
        //============================================================================================================//

        #region Unity Functions

        private void Start()
        {
            InitButtons();

            UpdateBotResourceElements();
            _scrollViewsSetup = true;

            _currentlyOverwriting = false;

            
        }

        private void OnEnable()
        {
            Camera.onPostRender += _droneDesigner.DrawGL;
            _droneDesigner.SetupDrone();

            m_cameraZoomScaler.value = cameraScaleOnEnter;

            if (_scrollViewsSetup)
                RefreshScrollViews();
            
            PlayerDataManager.OnValuesChanged += UpdateBotResourceElements;
            PlayerDataManager.OnCapacitiesChanged += UpdateBotResourceElements;

            //TODO May want to setup some sort of Init function to merge these two setups
            //launchButtonPointerEvents.PointerEntered += PreviewFillBothBotsResources;
            //repairButtonPointerEvents.PointerEntered += PreviewRepairCost;

            ScaleCamera(m_cameraZoomScaler.value);
            
            recoveryDroneBannerObject.SetActive(Globals.IsRecoveryBot);
        }

        private void OnDisable()
        {
            Camera.onPostRender -= _droneDesigner.DrawGL;
            _droneDesigner.RecycleDrone();

            cameraScaleOnEnter = m_cameraZoomScaler.value;

            DroneDesigner?.ClearUndoRedoStacks();

            PlayerDataManager.OnValuesChanged -= UpdateBotResourceElements;
            PlayerDataManager.OnCapacitiesChanged -= UpdateBotResourceElements;
            
            
            //launchButtonPointerEvents.PointerEntered -= PreviewFillBothBotsResources;
            //repairButtonPointerEvents.PointerEntered -= PreviewRepairCost;

            Globals.ScaleCamera(Globals.CameraScaleSize);
        }

        #endregion //Unity Functions

        //============================================================================================================//

        #region Init

        private void InitButtons()
        {

            _repairButtonText = repairButton.GetComponentInChildren<TMP_Text>();
            
            repairButton.onClick.AddListener(() =>
            {
                /*DroneDesigner.RepairParts();
                PreviewRepairCost(false);*/
            });

            //--------------------------------------------------------------------------------------------------------//

            rotateLeftButton.onClick.AddListener(() =>
            {
                DroneDesigner.RotateBots(-1.0f);
            });

            rotateRightButton.onClick.AddListener(() =>
            {
                DroneDesigner.RotateBots(1.0f);
            });

            undoButton.onClick.AddListener(() =>
            {
                DroneDesigner.UndoStackPop();
            });

            redoButton.onClick.AddListener(() =>
            {
                DroneDesigner.RedoStackPop();
            });

            //--------------------------------------------------------------------------------------------------------//

            isUpgradingButton.onClick.AddListener(() =>
            {
                DroneDesigner.IsUpgrading = !DroneDesigner.IsUpgrading;
            });

            //--------------------------------------------------------------------------------------------------------//

            saveLayoutButton.onClick.AddListener(() =>
            {
                if (DroneDesigner.IsFullyConnected())
                {
                    saveMenu.SetActive(true);
                    bool isOverwrite = _currentlyOverwriting;
                    saveOverwritePortion.SetActive(isOverwrite);
                    saveBasePortion.SetActive(!isOverwrite);
                }
                else
                {
                    Alert.ShowAlert("Alert!", "There are blocks currently floating or disconnected.", "Okay", () =>
                    {
                        screenBlackImage.gameObject.SetActive(false);
                    });
                }
                screenBlackImage.gameObject.SetActive(true);
            });

            loadLayoutButton.onClick.AddListener(() =>
            {
                loadMenu.SetActive(true);
                partsWindow.SetActive(false);
                currentSelected = null;
                UpdateLoadListUiScrollViews();
                screenBlackImage.gameObject.SetActive(true);
            });

            //--------------------------------------------------------------------------------------------------------//

            loadConfirm.onClick.AddListener(() =>
            {
                if (currentSelected == null)
                    return;

                DroneDesigner.LoadLayout(currentSelected.Name);
                loadMenu.SetActive(false);
                partsWindow.SetActive(true);
                _currentlyOverwriting = true;
                screenBlackImage.gameObject.SetActive(false);
            });

            loadReturn.onClick.AddListener(() =>
            {
                loadMenu.SetActive(false);
                partsWindow.SetActive(true);
                screenBlackImage.gameObject.SetActive(false);
            });

            loadReturn2.onClick.AddListener(() =>
            {
                loadMenu.SetActive(false);
                partsWindow.SetActive(true);
                screenBlackImage.gameObject.SetActive(false);
            });

            //--------------------------------------------------------------------------------------------------------//

            saveConfirm.onClick.AddListener(() =>
            {
                DroneDesigner.SaveLayout(saveNameInputField.text);
                saveMenu.SetActive(false);
                _currentlyOverwriting = true;
                screenBlackImage.gameObject.SetActive(false);
            });

            saveReturn.onClick.AddListener(() =>
            {
                saveMenu.SetActive(false);
                screenBlackImage.gameObject.SetActive(false);
            });

            saveReturn2.onClick.AddListener(() =>
            {
                saveMenu.SetActive(false);
                screenBlackImage.gameObject.SetActive(false);
            });

            saveOverwrite.onClick.AddListener(() =>
            {
                DroneDesigner.SaveLayout(currentSelected.Name);
                saveMenu.SetActive(false);
                _currentlyOverwriting = true;
                screenBlackImage.gameObject.SetActive(false);
            });

            saveAsNew.onClick.AddListener(() =>
            {
                saveOverwritePortion.SetActive(false);
                saveBasePortion.SetActive(true);
                _currentlyOverwriting = false;
            });

            //--------------------------------------------------------------------------------------------------------//

            toggleBotsButton.onClick.AddListener(() =>
            {
                
                DroneDesigner.ToggleDrones();
                UpdateBotResourceElements();
                
                recoveryDroneBannerObject.SetActive(Globals.IsRecoveryBot);

            });

            m_cameraZoomScaler.onValueChanged.AddListener(ScaleCamera);
        }

        private void ScaleCamera(float value)
        {
            Globals.ScaleCamera(m_cameraZoomScaler.maxValue + m_cameraZoomScaler.minValue - value);
            CameraController.CameraOffset(Vector3.zero, true);
        }

        #endregion //Init

        //============================================================================================================//

        #region Scroll Views

        public void RefreshScrollViews()
        {
            UpdateBotResourceElements();
        }

        public void UpdateBotResourceElements()
        {
            foreach (BIT_TYPE _bitType in Constants.BIT_ORDER)
            {
                if (_bitType == BIT_TYPE.WHITE)
                    continue;

                PlayerResource playerResource = PlayerDataManager.GetResource(_bitType);

                var data = new ResourceAmount
                {
                    //resourceType = CraftCost.TYPE.Bit,
                    type = _bitType,
                    amount = playerResource.resource,
                    capacity = playerResource.resourceCapacity
                };

                var element = resourceScrollView.AddElement(data, $"{_bitType}_UIElement");
                element.Init(data);
            }

            //liquidResourceContentView
            foreach (BIT_TYPE _bitType in Constants.BIT_ORDER)
            {
                if (_bitType == BIT_TYPE.WHITE /*|| _bitType == BIT_TYPE.BLUE*/)
                    continue;
                
                if (DroneDesigner._scrapyardBot == null)
                    continue;
                
                PlayerResource playerResource = PlayerDataManager.GetResource(_bitType);

                var data = new ResourceAmount
                {
                    amount = (int)playerResource.liquid,
                    capacity = playerResource.liquidCapacity,
                    type = _bitType,
                };

                /*if (_bitType == BIT_TYPE.YELLOW)
                    System.Console.WriteLine("");*/

                var element = liquidResourceContentView.AddElement(data, $"{_bitType}_UIElement");
                element.Init(data, true);
                
                element.gameObject.SetActive(DroneDesigner._scrapyardBot.UsedResourceTypes.Contains(_bitType));
            }

            UpdateFlightDataUI();
            
            //UpdateRepairButton();
        }

        private void UpdateLoadListUiScrollViews()
        {
            foreach (var layoutData in DroneDesigner.ScrapyardLayouts)
            {
                if (layoutScrollView.FindElement(layoutData))
                    continue;

                var element = layoutScrollView.AddElement(layoutData, $"{layoutData.Name}_UIElement");
                element.Init(layoutData, LayoutPressed);
            }
            layoutScrollView.SetElementsActive(true);
        }

        #endregion //Scroll Views

        //============================================================================================================//

        #region Flight Data UI

        private void UpdateFlightDataUI()
        {
            //--------------------------------------------------------------------------------------------------------//
            if (_droneDesigner?._scrapyardBot is null)
            {
                flightDataText.text = "Flight Data:\nPower Draw: 0.0 KW/s\nTotal Power: Infinite";
                return;
            }
            
            var partCapacity = _droneDesigner._scrapyardBot.PartCapacity;

            
            var powerDraw = _droneDesigner._scrapyardBot.PowerDraw;
            var availablePower =
                Mathf.Clamp(
                    PlayerDataManager.GetResource(BIT_TYPE.YELLOW).liquid +
                    PlayerDataManager.GetResource(BIT_TYPE.YELLOW).resource, 0,
                    PlayerDataManager.GetResource(BIT_TYPE.YELLOW).liquidCapacity);

            string powerTime = "infinite";
            if(powerDraw > 0)
                powerTime = TimeSpan.FromSeconds(availablePower / powerDraw).ToString(@"mm\:ss") + "s";

            flightDataText.text = $"Flight Data:\nParts: {partCapacity}\nPower Draw: {powerDraw:0.0} KW/s\nTotal Power: {powerTime}";

            //Temporarily remove flight data from screen
            flightDataText.gameObject.SetActive(false);
        }

        #endregion //Flight Data UI

        //Preview Costs
        //====================================================================================================================//

        #region Preview Costs

        private void PreviewFillBothBotsResources(bool showPreview)
        {
            Dictionary<ResourceUIElement, float> resourceScrollViewPreviewAmounts = new Dictionary<ResourceUIElement, float>();

            resourceScrollViewPreviewAmounts = PreviewFillBotResources(showPreview, false, resourceScrollViewPreviewAmounts);
            resourceScrollViewPreviewAmounts = PreviewFillBotResources(showPreview, true, resourceScrollViewPreviewAmounts);

            foreach (var keyValue in resourceScrollViewPreviewAmounts)
            {
                keyValue.Key.PreviewChange(-keyValue.Value);
            }
        }

        private Dictionary<ResourceUIElement, float> PreviewFillBotResources(bool showPreview, bool isRecoveryDrone, Dictionary<ResourceUIElement, float> resourceScrollViewPreviewAmounts)
        {
            BIT_TYPE[] types = {
                BIT_TYPE.RED,
                BIT_TYPE.GREY,
                BIT_TYPE.GREEN,
                BIT_TYPE.YELLOW
            };

            List<BlockData> botData = PlayerDataManager.GetBlockDatas();

            foreach (var bitType in types)
            {

                var botLiquidElement = liquidResourceContentView.FindElement(x => x.type == bitType);
                var storageResourceElement = resourceScrollView.FindElement(x => x.type == bitType);
                

                if (!showPreview)
                {
                    botLiquidElement.PreviewChange(0);
                    storageResourceElement.PreviewChange(0);
                    continue;
                }

                var fillRemaining = PlayerDataManager.GetResource(bitType).liquidCapacity - PlayerDataManager.GetResource(bitType).liquid;

                //If its already full, then we're good to move on
                if (fillRemaining <= 0f)
                    continue;

                var availableResources = PlayerDataManager.GetResource(bitType).resource;

                //If we have no resources available to refill the liquid, move onto the next
                if (availableResources <= 0)
                    continue;

                var movingAmount = Mathf.RoundToInt(Mathf.Min(availableResources, fillRemaining));

                if (isRecoveryDrone == Globals.IsRecoveryBot)
                {
                    botLiquidElement.PreviewChange(movingAmount);
                }

                if (resourceScrollViewPreviewAmounts.ContainsKey(storageResourceElement))
                {
                    resourceScrollViewPreviewAmounts[storageResourceElement] += movingAmount;
                }
                else
                {
                    resourceScrollViewPreviewAmounts.Add(storageResourceElement, movingAmount);
                }
            }

            return resourceScrollViewPreviewAmounts;
        }

        public void PreviewCraftCost(bool showPreview, Blueprint blueprint)
        {
            //Get all the elements here
            var resourceUIElements = new Dictionary<BIT_TYPE, ResourceUIElement>();
            foreach (BIT_TYPE type in Enum.GetValues(typeof(BIT_TYPE)))
            {
                if (type == BIT_TYPE.WHITE || type == BIT_TYPE.NONE)
                    continue;
                resourceUIElements.Add(type, resourceScrollView.FindElement(x => x.type == type));
            }

            if (!showPreview || !blueprint.CanAfford)
            {
                foreach (var resourceUIElement in resourceUIElements.Values)
                {
                    resourceUIElement?.PreviewChange(0f);
                }

                return;
            }
        }

        #endregion //Preview Costs

        //====================================================================================================================//
        
        #region Other

        public void DisplayInsufficientResources()
        {
            Alert.ShowAlert("Alert!",
                "You do not have enough resources to purchase this part!", "Okay", null);
        }

        private void LayoutPressed(ScrapyardLayout botData)
        {
            currentSelected = botData;
            loadName.text = "Load " + botData.Name;
        }

        #endregion //Other

        //============================================================================================================//
    }
}

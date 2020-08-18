using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class DroneDesignUI : MonoBehaviour
    {
        [SerializeField, Required, BoxGroup("Part UI")]
        private GameObject partsWindow;
        [SerializeField, Required, BoxGroup("Part UI")]
        private RemotePartProfileScriptableObject remotePartProfileScriptable;
        [SerializeField, BoxGroup("Part UI")]
        private PartUIElementScrollView partsScrollView;

        //============================================================================================================//

        [SerializeField, BoxGroup("Resource UI")]
        private ResourceUIElementScrollView resourceScrollView;
        
        [SerializeField, BoxGroup("Resource UI")]
        private ResourceUIElementScrollView liquidResourceContentView;

        [SerializeField, BoxGroup("Resource UI"), Required]
        private Button fillBotButton;
        
        [SerializeField, BoxGroup("Load List UI")]
        private LayoutElementScrollView layoutScrollView;

        //============================================================================================================//

        /*[SerializeField, BoxGroup("View")]
        private SliderText zoomSliderText;*/
        /*[SerializeField, BoxGroup("View"), Required]
        private Slider zoomSlider;*/

        /*[SerializeField, Required, BoxGroup("View")]
        private Button leftTurnButton;
        [SerializeField, Required, BoxGroup("View")]
        private Button rightTurnButton;*/

        //============================================================================================================//

        /*[SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button saveButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button loadButton;*/
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button isUpgradingButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button undoButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button redoButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button saveLayoutButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button loadLayoutButton;

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

        [SerializeField, Required, BoxGroup("UI Visuals")]
        private Image screenBlackImage;

        //============================================================================================================//

        [SerializeField]
        private CameraController m_cameraController;

        [FormerlySerializedAs("m_scrapyard")] [SerializeField]
        private DroneDesigner mDroneDesigner;

        private ScrapyardLayout currentSelected;

        public bool IsPopupActive => loadMenu.activeSelf || saveMenu.activeSelf || Alert.Displayed;
        private bool _currentlyOverwriting = false;

        private bool scrollViewsSetup = false;

        //============================================================================================================//

        #region Unity Functions

        private void Start()
        {
            /*zoomSliderText.Init();*/

            /*zoomSlider.onValueChanged.AddListener(SetCameraZoom);
            SetCameraZoom(zoomSlider.value);*/

            InitButtons();

            InitUiScrollView();
            UpdateResourceElements();
            scrollViewsSetup = true;

            _currentlyOverwriting = false;
        }

        void OnEnable()
        {
            if (scrollViewsSetup)
                RefreshScrollViews();

            PlayerData.OnValuesChanged += UpdateResourceElements;
        }

        void OnDisable()
        {
            mDroneDesigner.ClearUndoRedoStacks();
            
            PlayerData.OnValuesChanged -= UpdateResourceElements;
        }

        #endregion //Unity Functions

        //============================================================================================================//

        #region Init

        private void InitButtons()
        {
            //--------------------------------------------------------------------------------------------------------//

            /*leftTurnButton.onClick.AddListener(() =>
            {
                mDroneDesigner.RotateBots(-1.0f);
            });

            rightTurnButton.onClick.AddListener(() =>
            {
                mDroneDesigner.RotateBots(1.0f);
            });*/

            //--------------------------------------------------------------------------------------------------------//

            /*saveButton.onClick.AddListener(() =>
            {
                Debug.Log("Save Button Pressed");
            });

            loadButton.onClick.AddListener(() =>
            {
                Debug.Log("Load Button Pressed");
            });*/

            //--------------------------------------------------------------------------------------------------------//

            undoButton.onClick.AddListener(() =>
            {
                mDroneDesigner.UndoStackPop();
            });

            redoButton.onClick.AddListener(() =>
            {
                mDroneDesigner.RedoStackPop();
            });

            //--------------------------------------------------------------------------------------------------------//

            isUpgradingButton.onClick.AddListener(() =>
            {
                mDroneDesigner.IsUpgrading = !mDroneDesigner.IsUpgrading;
            });

            //--------------------------------------------------------------------------------------------------------//

            saveLayoutButton.onClick.AddListener(() =>
            {
                if (mDroneDesigner.IsFullyConnected())
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

                mDroneDesigner.LoadLayout(currentSelected.Name);
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
                mDroneDesigner.SaveLayout(saveNameInputField.text);
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
                mDroneDesigner.SaveLayout(currentSelected.Name);
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

            fillBotButton.onClick.AddListener(() =>
            {
                TryFillBotResources();
            });


            //--------------------------------------------------------------------------------------------------------//


        }

        #endregion //Init

        //============================================================================================================//

        #region Scroll Views

        public void InitUiScrollView()
        {
            foreach (var blockData in PlayerPersistentData.PlayerData.GetCurrentPartsInStorage())
            {
                var partRemoteData = remotePartProfileScriptable.GetRemoteData((PART_TYPE)blockData.Type);

                var element = partsScrollView.AddElement<BrickImageUIElement>(partRemoteData, $"{partRemoteData.partType}_UIElement", allowDuplicate: true);
                element.Init(partRemoteData, PartPressed, blockData.Level);
            }
        }

        /*public void InitResourceScrollViews()
        {
            var resources = PlayerPersistentData.PlayerData.resources;

            foreach (var resource in resources)
            {
                var data = new ResourceAmount
                {
                    //resourceType = CraftCost.TYPE.Bit,
                    type = resource.Key,
                    amount = resource.Value,
                    capacity = 2500
                };

                var element = resourceScrollView.AddElement<ResourceUIElement>(data, $"{resource.Key}_UIElement");
                element.Init(data);
            }

            //liquidResourceContentView
            var liquids = PlayerPersistentData.PlayerData.liquidResource;
            var liquidsCapacity = PlayerPersistentData.PlayerData.liquidCapacity;
            foreach (var liquid in liquids)
            {
                var bitType = liquid.Key;

                switch (bitType)
                {
                    case BIT_TYPE.BLACK:
                    case BIT_TYPE.BLUE:
                    case BIT_TYPE.YELLOW:
                    case BIT_TYPE.WHITE:
                        continue;
                }
                
                var data = new ResourceAmount
                {
                    amount = (int)liquid.Value,
                    capacity = liquidsCapacity[bitType],
                    type = bitType,
                };

                var element = resourceScrollView.AddElement<ResourceUIElement>(data, $"{liquid.Key}_UIElement");
                element.Init(data);
            }
        }*/

        public void AddToPartScrollView(BlockData blockData)
        {
            var partRemoteData = remotePartProfileScriptable.GetRemoteData((PART_TYPE)blockData.Type);

            var element = partsScrollView.AddElement<BrickImageUIElement>(partRemoteData, $"{partRemoteData.partType}_UIElement", allowDuplicate: true);
            element.Init(partRemoteData, PartPressed, blockData.Level);
        }

        public void RefreshScrollViews()
        {
            partsScrollView.ClearElements<BrickImageUIElement>();
            InitUiScrollView();
            UpdateResourceElements();
        }

        public void UpdateResourceElements()
        {
            var resources = PlayerPersistentData.PlayerData.resources;

            foreach (var resource in resources)
            {
                var data = new ResourceAmount
                {
                    //resourceType = CraftCost.TYPE.Bit,
                    type = resource.Key,
                    amount = resource.Value,
                    capacity = 1000
                };

                var element = resourceScrollView.AddElement<ResourceUIElement>(data, $"{resource.Key}_UIElement");
                element.Init(data);
            }

            //liquidResourceContentView
            var liquids = PlayerPersistentData.PlayerData.liquidResource;
            var liquidsCapacity = PlayerPersistentData.PlayerData.liquidCapacity;
            foreach (var liquid in liquids)
            {
                var bitType = liquid.Key;

                switch (bitType)
                {
                    case BIT_TYPE.BLACK:
                    case BIT_TYPE.BLUE:
                    case BIT_TYPE.YELLOW:
                    case BIT_TYPE.WHITE:
                        continue;
                }
                
                var data = new ResourceAmount
                {
                    amount = (int)liquid.Value,
                    capacity = liquidsCapacity[bitType],
                    type = bitType,
                };

                var element = liquidResourceContentView.AddElement<ResourceUIElement>(data, $"{liquid.Key}_UIElement");
                element.Init(data);
            }
        }

        private void UpdateLoadListUiScrollViews()
        {
            foreach (var layoutData in mDroneDesigner.ScrapyardLayouts)
            {
                if (layoutScrollView.FindElement<LayoutUIElement>(layoutData))
                    continue;

                var element = layoutScrollView.AddElement<LayoutUIElement>(layoutData, $"{layoutData.Name}_UIElement");
                element.Init(layoutData, LayoutPressed);
            }
            layoutScrollView.SetElementsActive(true);
        }

        #endregion //Scroll Views
        
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

        #region Other

        /*private void SetCameraZoom(float value)
        {
            m_cameraController.SetOrthographicSize(Values.Constants.gridCellSize * Values.Globals.ColumnsOnScreen * value, Vector3.zero);
            m_cameraController.CameraOffset(Vector3.zero, true);
        }*/

        public void DisplayInsufficientResources()
        {
            Alert.ShowAlert("Alert!",
                "You do not have enough resources to purchase this part!", "Okay", null);
        }

        private void PartPressed((Enum remoteDataType, int level) tuple)
        {
            if (tuple.remoteDataType is PART_TYPE partType)
            {
                PartPressed((partType, tuple.level));
            }
        }

        private void PartPressed((PART_TYPE partType, int level) tuple)
        {
            mDroneDesigner.selectedPartType = tuple.partType;
            mDroneDesigner.SelectedPartLevel = tuple.level;
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

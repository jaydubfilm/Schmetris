using System;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class DroneDesignUI : MonoBehaviour
    {
        private const int MAX_CAPACITY = 1500;
        
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

        [SerializeField, BoxGroup("Load List UI")]
        private LayoutElementScrollView layoutScrollView;

        //============================================================================================================//

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

        //[FormerlySerializedAs("m_scrapyard")] [SerializeField]
        private DroneDesigner DroneDesigner
        {
            get
            {
                if (_droneDesigner == null)
                    _droneDesigner = FindObjectOfType<DroneDesigner>();

                return _droneDesigner;
            }
        }
        private DroneDesigner _droneDesigner;

        private ScrapyardLayout currentSelected;

        public bool IsPopupActive => loadMenu.activeSelf || saveMenu.activeSelf || Alert.Displayed;
        private bool _currentlyOverwriting;

        private bool _scrollViewsSetup;

        //============================================================================================================//

        #region Unity Functions

        private void Start()
        {

            InitButtons();

            InitUiScrollView();
            UpdateResourceElements();
            _scrollViewsSetup = true;

            _currentlyOverwriting = false;
        }

        private void OnEnable()
        {
            if (_scrollViewsSetup)
                RefreshScrollViews();

            PlayerData.OnValuesChanged += UpdateResourceElements;
        }

        private void OnDisable()
        {
            DroneDesigner.ClearUndoRedoStacks();
            
            PlayerData.OnValuesChanged -= UpdateResourceElements;
        }

        #endregion //Unity Functions

        //============================================================================================================//

        #region Init

        private void InitButtons()
        {

            //--------------------------------------------------------------------------------------------------------//

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

        }

        #endregion //Init

        //============================================================================================================//

        #region Scroll Views

        public void InitUiScrollView()
        {
            foreach (var blockData in PlayerPersistentData.PlayerData.GetCurrentPartsInStorage())
            {
                var partRemoteData = remotePartProfileScriptable.GetRemoteData((PART_TYPE)blockData.Type);

                var element = partsScrollView.AddElement(partRemoteData, $"{partRemoteData.partType}_UIElement", allowDuplicate: true);
                element.Init(partRemoteData, BrickElementPressed, blockData.Level);
            }
        }

        public void AddToPartScrollView(BlockData blockData)
        {
            var partRemoteData = remotePartProfileScriptable.GetRemoteData((PART_TYPE)blockData.Type);

            var element = partsScrollView.AddElement(partRemoteData, $"{partRemoteData.partType}_UIElement", allowDuplicate: true);
            element.Init(partRemoteData, BrickElementPressed, blockData.Level);
        }

        public void RefreshScrollViews()
        {
            partsScrollView.ClearElements();
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
                    capacity = MAX_CAPACITY
                };

                var element = resourceScrollView.AddElement(data, $"{resource.Key}_UIElement");
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

                var element = liquidResourceContentView.AddElement(data, $"{liquid.Key}_UIElement");
                element.Init(data, true);
            }
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

        #region Other

        public void DisplayInsufficientResources()
        {
            Alert.ShowAlert("Alert!",
                "You do not have enough resources to purchase this part!", "Okay", null);
        }

        private void BrickElementPressed((Enum remoteDataType, int level) tuple)
        {
            var (remoteDataType, level) = tuple;

            string classType;
            int type;
            float health;
            
            switch (remoteDataType)
            {
                case PART_TYPE partType:
                    classType = nameof(Part);
                    type = (int) partType;
                    health = FactoryManager.Instance.PartsRemoteData.GetRemoteData(partType).levels[level].health;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(remoteDataType), remoteDataType, null);
            }
            
            var blockData = new BlockData
            {
                ClassType = classType,
                Type = type,
                Health = health
            };
            
            DroneDesigner.SelectPartFromStorage(blockData);
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

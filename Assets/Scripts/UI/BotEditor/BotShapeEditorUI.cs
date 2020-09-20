using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class BotShapeEditorUI : MonoBehaviour
    {
        [SerializeField, Required, BoxGroup("Part UI")]
        private GameObject PartsWindow;
        [SerializeField, Required, BoxGroup("Part UI")]
        private BitRemoteDataScriptableObject _remoteBitProfileScriptable;
        [SerializeField, Required, BoxGroup("Part UI")]
        private RemotePartProfileScriptableObject _remotePartProfileScriptable;
        [SerializeField, BoxGroup("Part UI")]
        private PartUIElementScrollView partsScrollView;
        [SerializeField, BoxGroup("Part UI")]
        private PartUIElementScrollView bitsScrollView;
        [SerializeField, Required, BoxGroup("Part UI")]
        private Button showBitsButton;
        [SerializeField, Required, BoxGroup("Part UI")]
        private Button showCategoriesButton;
        [SerializeField, Required, BoxGroup("Part UI")]
        private TMP_Text partsLabel;

        [SerializeField]
        private ScrollRect scrollRect;
        [SerializeField]
        private RectTransform bitRect;
        [SerializeField]
        private RectTransform categoryRect;

        //============================================================================================================//

        [SerializeField, Required, BoxGroup("Category UI")]
        private GameObject CategoryWindow;
        [SerializeField, BoxGroup("Category UI")]
        private CategoryElementScrollView categoriesScrollView;

        //============================================================================================================//

        [SerializeField, BoxGroup("Load List UI")]
        private BotShapeDataElementScrollView botLoadListScrollView;
        [SerializeField, BoxGroup("Load List UI")]
        private BotShapeDataElementScrollView shapeLoadListScrollView;
        [SerializeField, BoxGroup("Load List UI")]
        private Button ShowBotsButton;
        [SerializeField, BoxGroup("Load List UI")]
        private Button ShowShapesButton;

        //============================================================================================================//

        [SerializeField, BoxGroup("View")]
        private SliderText zoomSliderText;
        [SerializeField, BoxGroup("View"), Required]
        private Slider zoomSlider;

        [SerializeField, Required, BoxGroup("View")]
        private Button leftTurnButton;
        [SerializeField, Required, BoxGroup("View")]
        private Button rightTurnButton;

        //============================================================================================================//

        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button SaveButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button LoadButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button NewCategoryButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button PushBotButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button NewBotButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button NewShapeButton;

        [SerializeField, Required, BoxGroup("Load Menu")]
        private GameObject LoadMenu;
        [SerializeField, Required, BoxGroup("Load Menu")]
        private Button LoadConfirm;
        [SerializeField, Required, BoxGroup("Load Menu")]
        private Button LoadReturn;
        [SerializeField, Required, BoxGroup("Load Menu")]
        private Button LoadReturn2;
        [SerializeField, Required, BoxGroup("Load Menu")]
        private TMP_Text LoadName;

        [SerializeField, Required, BoxGroup("Save Menu")]
        private GameObject SaveMenu;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private GameObject SaveOverwritePortion;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private GameObject SaveBasePortion;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private TMP_InputField SaveNameInputField;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button SaveConfirm;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button SaveReturn;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button SaveReturn2;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button SaveOverwrite;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button SaveAsNew;

        //[SerializeField, Required, BoxGroup("Can't Save Menu")]
        //private GameObject CantSaveMenu;
        //[SerializeField, Required, BoxGroup("Can't Save Menu")]
        //private Button CantSaveRemove;
        //[SerializeField, Required, BoxGroup("Can't Save Menu")]
        //private Button CantSaveReturn;
        //[SerializeField, Required, BoxGroup("Can't Save Menu")]
        //private Button CantSaveReturn2;

        [SerializeField, Required, BoxGroup("New Category Menu")]
        private GameObject NewCategoryMenu;
        [SerializeField, Required, BoxGroup("New Category Menu")]
        private TMP_InputField NewCategoryNameInputField;
        [SerializeField, Required, BoxGroup("New Category Menu")]
        private Button NewCategoryConfirm;
        [SerializeField, Required, BoxGroup("New Category Menu")]
        private Button NewCategoryReturn;
        [SerializeField, Required, BoxGroup("New Category Menu")]
        private Button NewCategoryReturn2;

        //[SerializeField, Required, BoxGroup("Overwrite Menu")]
        //private GameObject OverwriteMenu;
        //[SerializeField, Required, BoxGroup("Overwrite Menu")]
        //private Button OverwriteConfirm;
        //[SerializeField, Required, BoxGroup("Overwrite Menu")]
        //private Button OverwriteReturn;

        [SerializeField, Required, BoxGroup("UI Visuals")]
        private Image ScreenBlackImage;

        //============================================================================================================//

        private CameraController m_cameraController;
        private BotShapeEditor m_botShapeEditor;

        private EditorGeneratorDataBase m_currentSelected;
        public bool IsPopupActive => LoadMenu.activeSelf || SaveMenu.activeSelf || Alert.Displayed || NewCategoryMenu.activeSelf;
        private bool m_currentlyOverwriting = false;


        private void Start()
        {
            m_cameraController = FindObjectOfType<CameraController>();
            m_botShapeEditor = FindObjectOfType<BotShapeEditor>();

            zoomSliderText.Init();

            zoomSlider.onValueChanged.AddListener(SetCameraZoom);
            SetCameraZoom(zoomSlider.value);

            InitUiScrollViews();

            InitButtons();
        }

        private void Update()
        {
            leftTurnButton.interactable = m_botShapeEditor.EditingShape;
            rightTurnButton.interactable = m_botShapeEditor.EditingBot;
        }

        //============================================================================================================//

        private void InitButtons()
        {
            LoadMenu.SetActive(false);
            SaveMenu.SetActive(false);
            showBitsButton.gameObject.SetActive(false);
            showCategoriesButton.gameObject.SetActive(false);

            //--------------------------------------------------------------------------------------------------------//

            showBitsButton.onClick.AddListener(() =>
            {
                SetBitsScrollActive(true);
            });

            showCategoriesButton.onClick.AddListener(() =>
            {
                SetCategoriesScrollActive(true);
            });

            //--------------------------------------------------------------------------------------------------------//

            leftTurnButton.onClick.AddListener(() =>
            {
                m_botShapeEditor.RotateBots(-1.0f);
            });

            rightTurnButton.onClick.AddListener(() =>
            {
                m_botShapeEditor.RotateBots(1.0f);
            });

            //--------------------------------------------------------------------------------------------------------//

            ShowBotsButton.onClick.AddListener(() =>
            {
                SetBotsScrollActive(true);
            });

            ShowShapesButton.onClick.AddListener(() =>
            {
                SetShapesScrollActive(true);
            });

            //--------------------------------------------------------------------------------------------------------//

            NewBotButton.onClick.AddListener(() =>
            {
                m_botShapeEditor.CreateBot(true);
                SetPartsScrollActive(true);
                SetCategoriesScrollActive(false);
                m_currentlyOverwriting = false;
            });

            NewShapeButton.onClick.AddListener(() =>
            {
                m_botShapeEditor.CreateShape(null);
                SetBitsScrollActive(true);
                SetCategoriesScrollActive(false);
                m_currentlyOverwriting = false;
            });

            SaveButton.onClick.AddListener(() =>
            {
                if (m_botShapeEditor.CheckLegal())
                {
                    SaveMenu.SetActive(true);
                    bool isOverwrite = m_currentlyOverwriting;
                    SaveOverwritePortion.SetActive(isOverwrite);
                    SaveBasePortion.SetActive(!isOverwrite);
                }
                else
                {
                    Alert.ShowAlert("Alert!",
                        "There are blocks currently floating or disconnected. Would you like to remove them?",
                        "Confirm", "Cancel", b =>
                        {
                            if (b)
                                m_botShapeEditor.RemoveFloating();

                            ScreenBlackImage.gameObject.SetActive(false);
                        });
                }
                ScreenBlackImage.gameObject.SetActive(true);
            });

            LoadButton.onClick.AddListener(() =>
            {
                LoadMenu.SetActive(true);
                PartsWindow.SetActive(false);
                m_currentSelected = null;
                UpdateLoadListUiScrollViews();
                ScreenBlackImage.gameObject.SetActive(true);
            });

            NewCategoryButton.onClick.AddListener(() =>
            {
                NewCategoryMenu.SetActive(true);
                ScreenBlackImage.gameObject.SetActive(true);
            });

            PushBotButton.onClick.AddListener(() =>
            {
                m_botShapeEditor.PushBot();
            });

            //--------------------------------------------------------------------------------------------------------//

            LoadConfirm.onClick.AddListener(() =>
            {
                if (m_currentSelected == null)
                    return;

                m_botShapeEditor.LoadBlockData(m_currentSelected.Name);
                LoadMenu.SetActive(false);
                PartsWindow.SetActive(true);
                m_currentlyOverwriting = true;
                ScreenBlackImage.gameObject.SetActive(false);
            });

            LoadReturn.onClick.AddListener(() =>
            {
                LoadMenu.SetActive(false);
                PartsWindow.SetActive(true);
                ScreenBlackImage.gameObject.SetActive(false);
            });

            LoadReturn2.onClick.AddListener(() =>
            {
                LoadMenu.SetActive(false);
                PartsWindow.SetActive(true);
                ScreenBlackImage.gameObject.SetActive(false);
            });

            //--------------------------------------------------------------------------------------------------------//

            SaveConfirm.onClick.AddListener(() =>
            {
                m_botShapeEditor.SaveBlockData(SaveNameInputField.text);
                SetCategoriesScrollActive(false);
                SaveMenu.SetActive(false);
                m_currentlyOverwriting = false;
                ScreenBlackImage.gameObject.SetActive(false);
            });

            SaveReturn.onClick.AddListener(() =>
            {
                SaveMenu.SetActive(false);
                ScreenBlackImage.gameObject.SetActive(false);
            });

            SaveReturn2.onClick.AddListener(() =>
            {
                SaveMenu.SetActive(false);
                ScreenBlackImage.gameObject.SetActive(false);
            });

            SaveOverwrite.onClick.AddListener(() =>
            {
                m_botShapeEditor.SaveBlockData(m_currentSelected.Name);
                SetCategoriesScrollActive(false);
                SaveMenu.SetActive(false);
                m_currentlyOverwriting = false;
                ScreenBlackImage.gameObject.SetActive(false);
            });

            SaveAsNew.onClick.AddListener(() =>
            {
                SaveOverwritePortion.SetActive(false);
                SaveBasePortion.SetActive(true);
                m_currentlyOverwriting = false;
            });

            //--------------------------------------------------------------------------------------------------------//

            NewCategoryConfirm.onClick.AddListener(() =>
            {
                m_botShapeEditor.AddCategory(NewCategoryNameInputField.text);
                NewCategoryMenu.SetActive(false);
                ScreenBlackImage.gameObject.SetActive(false);
                UpdateCategoriesScrollViews();
            });

            NewCategoryReturn.onClick.AddListener(() =>
            {
                NewCategoryMenu.SetActive(false);
                ScreenBlackImage.gameObject.SetActive(false);
            });

            NewCategoryReturn2.onClick.AddListener(() =>
            {
                NewCategoryMenu.SetActive(false);
                ScreenBlackImage.gameObject.SetActive(false);
            });

            //--------------------------------------------------------------------------------------------------------//


            /*m_loadNameInputField.onValueChanged.AddListener((content) =>
            {
                var isEmpty = string.IsNullOrEmpty(content);
                SaveButton.interactable = !isEmpty;
                LoadButton.interactable = !isEmpty;
            });

            m_loadNameInputField.text = null;*/

            //--------------------------------------------------------------------------------------------------------//
        }

        private void InitUiScrollViews()
        {
            //FIXME This needs to move to the Factory
            foreach (var partRemoteData in _remotePartProfileScriptable.partRemoteData)
            {
                for (int i = 0; i < partRemoteData.levels.Count; i++)
                {
                    if (partRemoteData.partType == PART_TYPE.CORE)
                        continue;

                    var element = partsScrollView.AddElement(partRemoteData, $"{partRemoteData.partType}_{i}_UIElement", true);
                    element.Init(partRemoteData, OnBrickElementPressed, i);
                }
            }

            //FIXME This needs to move to the Factory
            foreach (var bitRemoteData in _remoteBitProfileScriptable.BitRemoteData)
            {
                for (int i = 0; i < bitRemoteData.levels.Length; i++)
                {
                    var element = partsScrollView.AddElement(bitRemoteData, $"{bitRemoteData.bitType}_{i}_UIElement", true);
                    element.Init(bitRemoteData, OnBrickElementPressed, i);
                    var element2 = bitsScrollView.AddElement(bitRemoteData, $"{bitRemoteData.bitType}_{i}_UIElement", true);
                    element2.Init(bitRemoteData, OnBrickElementPressed, i);
                }
            }

            /*BitRemoteData remoteData = new BitRemoteData();
            remoteData.bitType = BIT_TYPE.BLACK;
            var test = bitsScrollView.AddElement<BrickImageUIElement>(remoteData, $"{remoteData.bitType}_0_UIElement", true);
            test.Init(remoteData, PartBitPressed, 0);*/

            UpdateCategoriesScrollViews();
            UpdateLoadListUiScrollViews();

            SetPartsScrollActive(false);
            SetBitsScrollActive(false);
            SetCategoriesScrollActive(false);
        }

        private void UpdateCategoriesScrollViews()
        {
            //FIXME This needs to move to the Factory
            foreach (var category in m_botShapeEditor.EditorBotShapeData.m_categories)
            {
                var element = categoriesScrollView.AddElement(category, category);
                element.Init(category, CategoryPressed);
            }
        }

        private void UpdateLoadListUiScrollViews()
        {
            foreach (var botGeneratorData in m_botShapeEditor.EditorBotShapeData.m_editorBotGeneratorData)
            {
                if (botLoadListScrollView.FindElement(botGeneratorData))
                    continue;

                var element = botLoadListScrollView.AddElement(botGeneratorData, $"{botGeneratorData.Name}_UIElement");
                element.Init(botGeneratorData, BotShapePressed);
            }

            foreach (var shapeGeneratorData in m_botShapeEditor.EditorBotShapeData.m_editorShapeGeneratorData)
            {
                if (shapeLoadListScrollView.FindElement(shapeGeneratorData))
                    continue;

                var element = shapeLoadListScrollView.AddElement(shapeGeneratorData, $"{shapeGeneratorData.Name}_UIElement");
                element.Init(shapeGeneratorData, BotShapePressed);
            }

            SetBotsScrollActive(true);
        }

        private void SetCameraZoom(float value)
        {
            m_cameraController.SetOrthographicSize(Values.Constants.gridCellSize * Values.Globals.ColumnsOnScreen * value, Vector3.zero);
            m_cameraController.CameraOffset(Vector3.zero, true);
        }

        //============================================================================================================//

        public void SetPartsScrollActive(bool active)
        {
            partsScrollView.SetElementsActive(active);
            partsLabel.gameObject.SetActive(active);
            showBitsButton.gameObject.SetActive(!active);
            showCategoriesButton.gameObject.SetActive(!active);
            if (active)
                bitsScrollView.SetElementsActive(false);
        }

        public void SetBitsScrollActive(bool active)
        {
            if (active)
            {
                scrollRect.content = bitRect;
            }

            bitsScrollView.SetElementsActive(active);
            if (active)
            {
                showBitsButton.gameObject.SetActive(true);
                showCategoriesButton.gameObject.SetActive(true);
                partsLabel.gameObject.SetActive(false);
                partsScrollView.SetElementsActive(false);
                categoriesScrollView.SetElementsActive(false);
            }
        }

        public void SetCategoriesScrollActive(bool active)
        {
            if (active)
            {
                scrollRect.content = categoryRect;
            }

            categoriesScrollView.SetElementsActive(active);
            if (active)
            {
                showBitsButton.gameObject.SetActive(true);
                showCategoriesButton.gameObject.SetActive(true);
                partsLabel.gameObject.SetActive(false);
                bitsScrollView.SetElementsActive(false);
            }
        }

        public void SetBotsScrollActive(bool active)
        {
            botLoadListScrollView.SetElementsActive(active);
            shapeLoadListScrollView.SetElementsActive(!active);
        }
        public void SetShapesScrollActive(bool active)
        {
            shapeLoadListScrollView.SetElementsActive(active);
            botLoadListScrollView.SetElementsActive(!active);
        }

        public void UpdateCategories(EditorShapeGeneratorData shapeData)
        {
            foreach (var element in categoriesScrollView.Elements)
            {
                if (element is CategoryToggleUIElement toggle)
                {
                    toggle.SetToggle(shapeData.Categories.Contains(element.data));
                }
            }
        }

        public List<string> GetCategories()
        {
            List<string> categories = new List<string>();
            foreach (var element in categoriesScrollView.Elements)
            {
                if (element is CategoryToggleUIElement toggle && toggle.GetToggleValue())
                {
                    categories.Add(toggle.data);
                }
            }
            return categories;
        }

        //============================================================================================================//

        private void OnBrickElementPressed((Enum remoteDataType, int level) tuple)
        {
            var (remoteDataType, level) = tuple;
            
            string classType;
            int type;
            float health;
            
            switch (remoteDataType)
            {
                case PART_TYPE partType:
                    type = (int) partType;
                    classType = nameof(Part);
                    health = FactoryManager.Instance.PartsRemoteData.GetRemoteData(partType).levels[level].health;
                    break;
                case BIT_TYPE bitType:
                    classType = nameof(Bit);
                    type = (int) bitType;
                    health = FactoryManager.Instance.BitsRemoteData.GetRemoteData(bitType).levels[level].health;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(remoteDataType), remoteDataType, null);
            }
            
            m_botShapeEditor.SelectedBrick = new BlockData
            {
                ClassType = classType,
                Type = type,
                Level = level,
                Health = health
            };
        }

        private void BotShapePressed(EditorGeneratorDataBase botData)
        {
            m_currentSelected = botData;
            m_botShapeEditor.LoadBlockData(m_currentSelected.Name);
            LoadName.text = "Load " + botData.Name;
        }

        private void CategoryPressed(string categoryName, bool active)
        {

        }
    }
}

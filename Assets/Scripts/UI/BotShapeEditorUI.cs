using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
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
        private RemotePartProfileScriptableObject _remotePartProfileScriptable;
        [SerializeField, BoxGroup("Part UI")]
        private PartUIElementScrollView partsScrollView;

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

        [SerializeField, Required, BoxGroup("Can't Save Menu")]
        private GameObject CantSaveMenu;
        [SerializeField, Required, BoxGroup("Can't Save Menu")]
        private Button CantSaveRemove;
        [SerializeField, Required, BoxGroup("Can't Save Menu")]
        private Button CantSaveReturn;
        [SerializeField, Required, BoxGroup("Can't Save Menu")]
        private Button CantSaveReturn2;

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

        [SerializeField, Required, BoxGroup("Overwrite Menu")]
        private GameObject OverwriteMenu;
        [SerializeField, Required, BoxGroup("Overwrite Menu")]
        private Button OverwriteConfirm;
        [SerializeField, Required, BoxGroup("Overwrite Menu")]
        private Button OverwriteReturn;

        [SerializeField, Required, BoxGroup("UI Visuals")]
        private Image ScreenBlackImage;

        //============================================================================================================//

        private CameraController m_cameraController;
        private BotShapeEditor m_botShapeEditor;

        private EditorGeneratorDataBase m_currentSelected;
        public bool IsPopupActive => LoadMenu.activeSelf || SaveMenu.activeSelf || CantSaveMenu.activeSelf || OverwriteMenu.activeSelf || NewCategoryMenu.activeSelf;
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
        
        //============================================================================================================//

        private void InitButtons()
        {
            LoadMenu.SetActive(false);
            SaveMenu.SetActive(false);
            
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
                SetPartsScrollActive(false);
                SetCategoriesScrollActive(true);
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
                    CantSaveMenu.SetActive(true);
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
                SetPartsScrollActive(false);
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
                SetPartsScrollActive(false);
                SetCategoriesScrollActive(false);
                SaveMenu.SetActive(false);
                m_currentlyOverwriting = false;
                ScreenBlackImage.gameObject.SetActive(false);
            });

            SaveAsNew.onClick.AddListener(() =>
            {
                SaveOverwritePortion.SetActive(false);
                SaveBasePortion.SetActive(true);
                SaveMenu.SetActive(false);
                m_currentlyOverwriting = false;
            });

            //--------------------------------------------------------------------------------------------------------//

            CantSaveRemove.onClick.AddListener(() =>
            {
                m_botShapeEditor.RemoveFloating();
                CantSaveMenu.SetActive(false);
                SaveMenu.SetActive(true);
            });

            CantSaveReturn.onClick.AddListener(() =>
            {
                CantSaveMenu.SetActive(false);
                ScreenBlackImage.gameObject.SetActive(false);
            });

            CantSaveReturn2.onClick.AddListener(() =>
            {
                CantSaveMenu.SetActive(false);
                ScreenBlackImage.gameObject.SetActive(false);
            });

            //--------------------------------------------------------------------------------------------------------//

            NewCategoryConfirm.onClick.AddListener(() =>
            {
                m_botShapeEditor.AddCategory(NewCategoryNameInputField.text);
                NewCategoryMenu.SetActive(false);
                ScreenBlackImage.gameObject.SetActive(false);
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

                var element = partsScrollView.AddElement<PartUIElement>(partRemoteData, $"{partRemoteData.partType}_UIElement");
                element.Init(partRemoteData, PartPressed);
            }

            //FIXME This needs to move to the Factory
            foreach (var category in m_botShapeEditor.EditorBotShapeData.m_categories)
            {

                var element = categoriesScrollView.AddElement<CategoryToggleUIElement>(category, category);
                element.Init(category, CategoryPressed);
            }

            UpdateLoadListUiScrollViews();

            SetPartsScrollActive(false);
            SetCategoriesScrollActive(false);
        }

        private void UpdateLoadListUiScrollViews()
        {
            foreach (var botGeneratorData in m_botShapeEditor.EditorBotShapeData.m_editorBotGeneratorData)
            {
                if (botLoadListScrollView.FindElement<BotLoadListUIElement>(botGeneratorData))
                    continue;

                var element = botLoadListScrollView.AddElement<BotLoadListUIElement>(botGeneratorData, $"{botGeneratorData.Name}_UIElement");
                element.Init(botGeneratorData, BotShapePressed);
            }

            foreach (var shapeGeneratorData in m_botShapeEditor.EditorBotShapeData.m_editorShapeGeneratorData)
            {
                if (shapeLoadListScrollView.FindElement<BotLoadListUIElement>(shapeGeneratorData))
                    continue;

                var element = shapeLoadListScrollView.AddElement<BotLoadListUIElement>(shapeGeneratorData, $"{shapeGeneratorData.Name}_UIElement");
                element.Init(shapeGeneratorData, BotShapePressed);
            }

            SetBotsScrollActive(true);
        }

        private void SetCameraZoom(float value)
        {
            m_cameraController.SetOrthographicSize(Values.Constants.gridCellSize * Values.Globals.ColumnsOnScreen * value, Vector3.zero, true);
        }
        
        //============================================================================================================//
        
        public void SetPartsScrollActive(bool active)
        {
            partsScrollView.SetElementsActive(active);
        }

        public void SetCategoriesScrollActive(bool active)
        {
            categoriesScrollView.SetElementsActive(active);
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

        private void PartPressed(PART_TYPE partType)
        {
            m_botShapeEditor.selectedPartType = partType;
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


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
        private RemotePartProfileScriptableObject _remotePartProfileScriptable;

        [SerializeField, BoxGroup("Part UI")]
        private PartUIElementScrollView partsScrollView;

        //============================================================================================================//

        [SerializeField, Required, BoxGroup("Load List UI")]
        private EditorBotShapeGeneratorScriptableObject _editorBotShapeGeneratorScriptable;

        [SerializeField, BoxGroup("Load List UI")]
        private BotDataElementScrollView loadListScrollView;

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
        private Button NewBotButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button NewShapeButton;

        [SerializeField, Required, BoxGroup("Load Menu")]
        private GameObject LoadMenu;
        [SerializeField, Required, BoxGroup("Load Menu")]
        private TMP_InputField LoadNameInputField;
        [SerializeField, Required, BoxGroup("Load Menu")]
        private Button LoadConfirm;
        [SerializeField, Required, BoxGroup("Load Menu")]
        private Button LoadReturn;

        [SerializeField, Required, BoxGroup("Save Menu")]
        private GameObject SaveMenu;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private TMP_InputField SaveNameInputField;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button SaveConfirm;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button SaveReturn;

        [SerializeField, Required, BoxGroup("Can't Save Menu")]
        private GameObject CantSaveMenu;
        [SerializeField, Required, BoxGroup("Can't Save Menu")]
        private Button CantSaveRemove;
        [SerializeField, Required, BoxGroup("Can't Save Menu")]
        private Button CantSaveReturn;

        [SerializeField, Required, BoxGroup("Overwrite Menu")]
        private GameObject OverwriteMenu;
        [SerializeField, Required, BoxGroup("Overwrite Menu")]
        private Button OverwriteConfirm;
        [SerializeField, Required, BoxGroup("Overwrite Menu")]
        private Button OverwriteReturn;

        //============================================================================================================//

        private CameraController m_cameraController;
        private BotShapeEditor m_botShapeEditor;

        public bool IsPopupActive => LoadMenu.activeSelf || SaveMenu.activeSelf;

        
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
            
            SaveButton.onClick.AddListener(() =>
            {
                if (CantSaveMenu.activeSelf || SaveMenu.activeSelf || LoadMenu.activeSelf || OverwriteMenu.activeSelf)
                    return; 

                if (m_botShapeEditor.CheckLegal())
                {
                    Debug.Log("Save Button Pressed");
                    SaveMenu.SetActive(true);
                }
                else
                {
                    CantSaveMenu.SetActive(true);
                }
            });
            
            LoadButton.onClick.AddListener(() =>
            {
                if (CantSaveMenu.activeSelf || SaveMenu.activeSelf || LoadMenu.activeSelf || OverwriteMenu.activeSelf)
                    return;

                Debug.Log("Load Button Pressed");
                LoadMenu.SetActive(true);
                UpdateLoadListUiScrollViews();
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
            });

            //--------------------------------------------------------------------------------------------------------//

            SaveConfirm.onClick.AddListener(() =>
            {
                Debug.Log("Save Button Pressed");
                m_botShapeEditor.SaveBlockData(SaveNameInputField.text);
                SetPartsScrollActive(false);
                SaveMenu.SetActive(false);
            });

            LoadConfirm.onClick.AddListener(() =>
            {
                Debug.Log("Load Button Pressed");
                m_botShapeEditor.LoadBlockData(LoadNameInputField.text);
                LoadMenu.SetActive(false);
            });

            //--------------------------------------------------------------------------------------------------------//

            SaveReturn.onClick.AddListener(() =>
            {
                SaveMenu.SetActive(false);
            });

            LoadReturn.onClick.AddListener(() =>
            {
                LoadMenu.SetActive(false);
            });

            //--------------------------------------------------------------------------------------------------------//

            NewBotButton.onClick.AddListener(() =>
            {
                m_botShapeEditor.CreateBot(true);
                SetPartsScrollActive(true);
            });

            NewShapeButton.onClick.AddListener(() =>
            {
                m_botShapeEditor.CreateShape(null);
                SetPartsScrollActive(false);
            });



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

            UpdateLoadListUiScrollViews();

            SetPartsScrollActive(false);
        }

        private void UpdateLoadListUiScrollViews()
        {
            foreach (var botGeneratorData in _editorBotShapeGeneratorScriptable.m_editorBotGeneratorData)
            {
                if (loadListScrollView.FindElement<BotLoadListUIElement>(botGeneratorData))
                    continue;
                
                print("addBot");
                var element = loadListScrollView.AddElement<BotLoadListUIElement>(botGeneratorData, $"{botGeneratorData.Name}_UIElement");
                element.Init(botGeneratorData, BotDataPressed);
            }
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
        
        
        //============================================================================================================//

        private void PartPressed(PART_TYPE partType)
        {
            Debug.Log($"Selected {partType}");
            m_botShapeEditor.selectedPartType = partType;
        }

        private void BotDataPressed(EditorBotGeneratorData botData)
        {

        }
    }
}


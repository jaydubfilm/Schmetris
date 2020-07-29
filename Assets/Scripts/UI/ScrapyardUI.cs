﻿using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class ScrapyardUI : MonoBehaviour, IDragHandler
    {
        [SerializeField]
        private Button MenuButton;

        [SerializeField, Required, BoxGroup("Part UI")]
        private RemotePartProfileScriptableObject _remotePartProfileScriptable;

        [SerializeField, BoxGroup("Part UI")]
        private PartUIElementScrollView partsScrollView;

        //============================================================================================================//

        [SerializeField, BoxGroup("Resource UI")]
        private ResourceUIElementScrollView resourceScrollView;

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
        private Button ReadyButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button SellBitsButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button IsUpgradingButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button UndoButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button RedoButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button SaveLayoutButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button LoadLayoutButton;

        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private GameObject saveMenuPortion;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private GameObject loadMenuPortion;

        //============================================================================================================//

        [SerializeField]
        private CameraController m_cameraController;

        [SerializeField]
        private Scrapyard m_scrapyard;


        private void Start()
        {
            zoomSliderText.Init();

            zoomSlider.onValueChanged.AddListener(SetCameraZoom);
            SetCameraZoom(zoomSlider.value);

            InitUiScrollViews();

            InitButtons();
        }

        //============================================================================================================//

        private void InitButtons()
        {
            //--------------------------------------------------------------------------------------------------------//

            MenuButton.onClick.AddListener(() =>
            {
                m_scrapyard.SaveBlockData();
                SceneLoader.ActivateScene("MainMenuScene", "ScrapyardScene");
            });

            leftTurnButton.onClick.AddListener(() =>
            {
                m_scrapyard.RotateBots(-1.0f);
            });

            rightTurnButton.onClick.AddListener(() =>
            {
                m_scrapyard.RotateBots(1.0f);
            });

            //--------------------------------------------------------------------------------------------------------//

            SaveButton.onClick.AddListener(() =>
            {
                saveMenuPortion.SetActive(true);
            });

            LoadButton.onClick.AddListener(() =>
            {
                loadMenuPortion.SetActive(true);
            });

            //--------------------------------------------------------------------------------------------------------//

            SaveButton.onClick.AddListener(() =>
            {
                Debug.Log("Save Button Pressed");
            });

            LoadButton.onClick.AddListener(() =>
            {
                Debug.Log("Load Button Pressed");
            });

            //--------------------------------------------------------------------------------------------------------//

            UndoButton.onClick.AddListener(() =>
            {
                m_scrapyard.UndoStackPop();
            });

            RedoButton.onClick.AddListener(() =>
            {
                m_scrapyard.RedoStackPop();
            });

            //--------------------------------------------------------------------------------------------------------//

            ReadyButton.onClick.AddListener(() =>
            {
                if (m_scrapyard.IsFullyConnected())
                {
                    m_scrapyard.SaveBlockData();
                    m_scrapyard.ProcessScrapyardUsageEndAnalytics();
                    if (Globals.SectorComplete)
                    {
                        Globals.SectorComplete = false;
                        SceneLoader.ActivateScene("UniverseMapScene", "ScrapyardScene");
                    }
                    else
                    {
                        SceneLoader.ActivateScene("AlexShulmanTestScene", "ScrapyardScene");
                    }
                }
                else
                {
                    Alert.ShowAlert("Alert!",
                        "A disconnected piece is active on your Bot! Please repair before continuing", "Okay", null);
                }
            });

            SellBitsButton.onClick.AddListener(() =>
            {
                m_scrapyard.SellBits();
                UpdateResources(PlayerPersistentData.PlayerData.GetResources());
            });

            IsUpgradingButton.onClick.AddListener(() =>
            {
                m_scrapyard.IsUpgrading = !m_scrapyard.IsUpgrading;
            });

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

            var resources = PlayerPersistentData.PlayerData.GetResources();

            foreach (var resource in resources)
            {
                var data = new ResourceAmount
                {
                    type = resource.Key,
                    amount = resource.Value
                };

                var element = resourceScrollView.AddElement<ResourceUIElement>(data, $"{resource.Key}_UIElement");
                element.Init(data);
            }
        }

        private void SetCameraZoom(float value)
        {
            m_cameraController.SetOrthographicSize(Values.Constants.gridCellSize * Values.Globals.ColumnsOnScreen * value, Vector3.zero, true);
        }

        //============================================================================================================//

        #region Unity Editor

        #if UNITY_EDITOR

        [Button("Test Resource Update"), DisableInEditorMode, BoxGroup("Resource UI")]
        private void TestUpdateResources()
        {
            var _resourcesTest = new Dictionary<BIT_TYPE, int>();
            for (var i = 0; i < 3; i++)
            {
                var type = (BIT_TYPE) Random.Range(1, 6);
                var amount = Random.Range(0, 1000);

                if (_resourcesTest.ContainsKey(type))
                {
                    _resourcesTest[type] += amount;
                    continue;
                }

                _resourcesTest.Add(type, amount);

            }

            UpdateResources(_resourcesTest);
        }

        #endif

        #endregion //Unity Editor

        public void UpdateResources(Dictionary<BIT_TYPE, int> resources)
        {
            UpdateResources(resources.ToResourceList());
        }

        public void UpdateResources(List<ResourceAmount> resources)
        {
            foreach (var resourceAmount in resources)
            {
                var element = resourceScrollView.FindElement<ResourceUIElement>(resourceAmount);

                if (element == null)
                    continue;

                element.Init(resourceAmount);
            }
        }

        public void DisplayInsufficientResources()
        {
            Alert.ShowAlert("Alert!",
                "You do not have enough resources to purchase this part!", "Okay", null);
        }

        //============================================================================================================//

        private void PartPressed(PART_TYPE partType)
        {
            Debug.Log($"Selected {partType}");
            m_scrapyard.selectedPartType = partType;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("Dragging");
        }
    }
}

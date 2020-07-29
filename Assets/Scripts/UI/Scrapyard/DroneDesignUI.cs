using System.Collections.Generic;
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
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class DroneDesignUI : MonoBehaviour, IDragHandler
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

        [FormerlySerializedAs("m_scrapyard")] [SerializeField]
        private DroneDesigner mDroneDesigner;


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
                mDroneDesigner.SaveBlockData();
                SceneLoader.ActivateScene("MainMenuScene", "ScrapyardScene");
            });

            leftTurnButton.onClick.AddListener(() =>
            {
                mDroneDesigner.RotateBots(-1.0f);
            });

            rightTurnButton.onClick.AddListener(() =>
            {
                mDroneDesigner.RotateBots(1.0f);
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
                mDroneDesigner.UndoStackPop();
            });

            RedoButton.onClick.AddListener(() =>
            {
                mDroneDesigner.RedoStackPop();
            });

            //--------------------------------------------------------------------------------------------------------//

            SellBitsButton.onClick.AddListener(() =>
            {
                mDroneDesigner.SellBits();
                UpdateResources(PlayerPersistentData.PlayerData.GetResources());
            });

            IsUpgradingButton.onClick.AddListener(() =>
            {
                mDroneDesigner.IsUpgrading = !mDroneDesigner.IsUpgrading;
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
            mDroneDesigner.selectedPartType = partType;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("Dragging");
        }
    }
}

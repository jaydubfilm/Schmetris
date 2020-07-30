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
    public class DroneDesignUI : MonoBehaviour, IDragHandler
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

        //============================================================================================================//

        [SerializeField, BoxGroup("Load List UI")]
        private LayoutElementScrollView layoutScrollView;

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
        private Button saveButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button loadButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button sellBitsButton;
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


        private void Start()
        {
            zoomSliderText.Init();

            zoomSlider.onValueChanged.AddListener(SetCameraZoom);
            SetCameraZoom(zoomSlider.value);

            InitUiScrollViews();

            InitButtons();

            _currentlyOverwriting = false;
        }

        //============================================================================================================//

        private void InitButtons()
        {
            //--------------------------------------------------------------------------------------------------------//

            leftTurnButton.onClick.AddListener(() =>
            {
                mDroneDesigner.RotateBots(-1.0f);
            });

            rightTurnButton.onClick.AddListener(() =>
            {
                mDroneDesigner.RotateBots(1.0f);
            });

            //--------------------------------------------------------------------------------------------------------//

            saveButton.onClick.AddListener(() =>
            {
                Debug.Log("Save Button Pressed");
            });

            loadButton.onClick.AddListener(() =>
            {
                Debug.Log("Load Button Pressed");
            });

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

            sellBitsButton.onClick.AddListener(() =>
            {
                mDroneDesigner.SellBits();
                UpdateResources(PlayerPersistentData.PlayerData.GetResources());
            });

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
                saveMenu.SetActive(false);
                _currentlyOverwriting = false;
            });

            //--------------------------------------------------------------------------------------------------------//

        }

        private void InitUiScrollViews()
        {
            //FIXME This needs to move to the Factory
            foreach (var partRemoteData in remotePartProfileScriptable.partRemoteData)
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

        public void SetPartsScrollActive(bool active)
        {
            partsScrollView.SetElementsActive(active);
        }

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

        private void LayoutPressed(ScrapyardLayout botData)
        {
            currentSelected = botData;
            loadName.text = "Load " + botData.Name;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("Dragging");
        }
    }
}

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
        private Button NewBotButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button NewShapeButton;

        [SerializeField, Required, BoxGroup("Input")]
        private TMP_InputField m_botNameInputField;

        //============================================================================================================//

        private CameraController m_cameraController;
        private BotShapeEditor m_botShapeEditor;

        
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
                Debug.Log("Save Button Pressed");
                m_botShapeEditor.SaveBlockData();
            });
            
            LoadButton.onClick.AddListener(() =>
            {
                Debug.Log("Load Button Pressed");
                m_botShapeEditor.LoadBlockData();
            });
            
            //--------------------------------------------------------------------------------------------------------//

            ReadyButton.onClick.AddListener(() =>
            {
                m_botShapeEditor.SaveBlockData();
                m_botShapeEditor.ProcessScrapyardUsageEndAnalytics();
                StarSalvager.SceneLoader.SceneLoader.ActivateScene("AlexShulmanTestScene", "ScrapyardScene");
            });

            NewBotButton.onClick.AddListener(() =>
            {
                m_botShapeEditor.CreateBot();
            });

            NewShapeButton.onClick.AddListener(() =>
            {
                FactoryManager.Instance.GetFactory<ShapeFactory>().CreateGameObject();
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

            var resources = PlayerPersistentData.GetPlayerData().GetResources();

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
        
        public string GetNameInputFieldValue()
        {
            return m_botNameInputField.text;
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
        
        
        //============================================================================================================//

        private void PartPressed(PART_TYPE partType)
        {
            Debug.Log($"Selected {partType}");
            m_botShapeEditor.selectedPartType = partType;
        }

    }
}


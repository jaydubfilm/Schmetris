using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Parts.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
 {
    public class DroneDesignUI : MonoBehaviour
    {
        public bool CanAffordRepair { get; private set; }

        [SerializeField, Required] private PointerEvents repairButtonPointerEvents;

        //====================================================================================================================//

        [SerializeField] private PurchasePatchUIElementScrollView purchasePatchUIElementScrollView;

        //============================================================================================================//

        [Serializable]
        public struct PatchUI
        {
            [HorizontalGroup("Row1"), LabelWidth(75)]
            public Image backgroundImage;

            [HorizontalGroup("Row1"), LabelWidth(50)]
            public TMP_Text text;
        }

        [Serializable]
        public struct GradeUI
        {
            [HorizontalGroup("Row1"), LabelWidth(75)]
            public Image bitImage;

            [HorizontalGroup("Row1"), LabelWidth(50)]
            public TMP_Text text;
        }

        //====================================================================================================================//
        

        [SerializeField, FoldoutGroup("Part Details Window")]
        private RectTransform partDetailsContainerRectTransform;

        [SerializeField, FoldoutGroup("Part Details Window")]
        private Image partImage;

        [SerializeField, FoldoutGroup("Part Details Window")]
        private TMP_Text partNameText;

        [SerializeField, FoldoutGroup("Part Details Window")]
        private TMP_Text otherPartDetailsText;

        [FormerlySerializedAs("PatchUis")] [SerializeField, FoldoutGroup("Part Details Window")]
        private PatchUI[] patchUis;

        [SerializeField, FoldoutGroup("Part Details Window")]
        private TMP_Text partDetailsText;

        [FormerlySerializedAs("GradeUis")] [SerializeField, FoldoutGroup("Part Details Window")]
        private GradeUI[] gradeUis;


        //====================================================================================================================//


        [SerializeField, Required, BoxGroup("Repairs Buttons")]
        private Button repairButton;
        [SerializeField, Required, BoxGroup("Repairs Buttons")]
        private TMP_Text repairButtonText;

        [SerializeField, Required, BoxGroup("Repairs Buttons")]
        private FadeUIImage repairButtonGlow;

        [SerializeField, Required, BoxGroup("Health UI")]
        private SliderText healthSliderText;
        

        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button launchButton;

        

        [SerializeField] private CameraController CameraController;


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

        [SerializeField, Required] private DroneDesigner _droneDesigner;

        private ScrapyardLayout currentSelected;

        public bool IsPopupActive => Alert.Displayed;
        private bool _currentlyOverwriting;

        private bool _scrollViewsSetup;

        float cameraScaleOnEnter = 71;

        //Unity Functions
        //============================================================================================================//

        #region Unity Functions

        private void Start()
        {
            InitButtons();
            InitHealthBar();

            _scrollViewsSetup = true;

            _currentlyOverwriting = false;


            InitPurchasePatches();

            ShowPartDetails(false, null);
        }

        private void OnEnable()
        {
            Camera.onPostRender += _droneDesigner.DrawGL;
            PlayerDataManager.OnValuesChanged += CheckCanRepair;

            UpdateHealthBar();
            CheckCanRepair();
        }

        private void OnDisable()
        {
            Camera.onPostRender -= _droneDesigner.DrawGL;
            PlayerDataManager.OnValuesChanged -= CheckCanRepair;
            _droneDesigner.RecycleDrone();

            Globals.ScaleCamera(Globals.CameraScaleSize);
        }

        #endregion //Unity Functions

        //============================================================================================================//

        #region Init

        private void InitButtons()
        {
            repairButton.onClick.AddListener(() =>
            {
                DroneDesigner.RepairDrone();
                healthSliderText.value = PlayerDataManager.GetBotHealth();

                CheckCanRepair();
            });

            launchButton.onClick.AddListener(() =>
            {
                SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.SCRAPYARD);
            });

            //--------------------------------------------------------------------------------------------------------//

        }

        private void InitHealthBar()
        {
            healthSliderText.Init(true);
            healthSliderText.SetBounds(0f, Globals.BotStartingHealth);
        }

        #endregion //Init

        //============================================================================================================//

        #region Scroll Views

        private void InitPurchasePatches()
        {
            var patchRemoteData = FactoryManager.Instance.PatchRemoteData;
            
            var patches = new[]
            {
                new Purchase_PatchData
                {
                    cost = patchRemoteData.GetRemoteData(PATCH_TYPE.RANGE).Levels[0].cost,
                    PatchData = new PatchData
                    {
                        Level = 0,
                        Type = (int) PATCH_TYPE.RANGE
                    }
                },
                new Purchase_PatchData
                {
                    cost = patchRemoteData.GetRemoteData(PATCH_TYPE.DAMAGE).Levels[0].cost,
                    PatchData = new PatchData
                    {
                        Level = 0,
                        Type = (int) PATCH_TYPE.DAMAGE
                    }
                },
                new Purchase_PatchData
                {
                    cost = patchRemoteData.GetRemoteData(PATCH_TYPE.FIRE_RATE).Levels[0].cost,
                    PatchData = new PatchData
                    {
                        Level = 0,
                        Type = (int) PATCH_TYPE.FIRE_RATE
                    }
                },
                new Purchase_PatchData
                {
                    cost = patchRemoteData.GetRemoteData(PATCH_TYPE.GRADE).Levels[0].cost,
                    PatchData = new PatchData
                    {
                        Level = 0,
                        Type = (int) PATCH_TYPE.GRADE
                    }
                }
            };

            foreach (var t in patches)
            {
                var element = purchasePatchUIElementScrollView.AddElement(t);
                element.Init(t);
            }
            
        }

        #endregion //Scroll Views

        //====================================================================================================================//

        #region Other

        public void DisplayInsufficientResources()
        {
            Alert.ShowAlert("Alert!",
                "You do not have enough resources to purchase this part!", "Okay", null);
        }

        private void UpdateHealthBar()
        {
            var health = PlayerDataManager.GetBotHealth();
            healthSliderText.value = health;
        }

        private void CheckCanRepair()
        {
            var currentHealth = PlayerDataManager.GetBotHealth();
            var startingHealth = Globals.BotStartingHealth;
            
            var canRepair = currentHealth < startingHealth;

            repairButton.gameObject.SetActive(canRepair);

            if (!canRepair)
                return;

            var cost = startingHealth - currentHealth;
            var components = PlayerDataManager.GetGears();

            var finalCost = components > 0 ? Mathf.Min(cost, components) : cost;

            repairButtonText.text = $"Repair {finalCost}";
            repairButton.interactable = !(finalCost > components);
        }

        #endregion //Other

        //============================================================================================================//

        public void ShowPartDetails(bool show, in ScrapyardPart scrapyardPart)
        {

        }

        //====================================================================================================================//
        
    }
}

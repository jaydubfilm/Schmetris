using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class DroneDesignUI : MonoBehaviour
    {
        public bool CanAffordRepair { get; private set; }

        [SerializeField, Required] 
        private TMP_Text flightDataText;
        
        [SerializeField, Required]
        private PointerEvents repairButtonPointerEvents;

        //====================================================================================================================//
        
        [SerializeField, BoxGroup("Resource UI")]
        private ResourceUIElementScrollView liquidResourceContentView;

        //============================================================================================================//

        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button repairButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private FadeUIImage repairButtonGlow;
        
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button launchButton;
        
        private TMP_Text _repairButtonText;

        [SerializeField]
        private CameraController CameraController;
        

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
        [SerializeField, Required]
        private DroneDesigner _droneDesigner;

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

            UpdateBotResourceElements();
            _scrollViewsSetup = true;

            _currentlyOverwriting = false;

            
        }

        private void OnEnable()
        {
            Camera.onPostRender += _droneDesigner.DrawGL;
            _droneDesigner.SetupDrone();

            if (_scrollViewsSetup)
                RefreshScrollViews();
            
            PlayerDataManager.OnValuesChanged += UpdateBotResourceElements;
            PlayerDataManager.OnCapacitiesChanged += UpdateBotResourceElements;

            //TODO May want to setup some sort of Init function to merge these two setups
            //launchButtonPointerEvents.PointerEntered += PreviewFillBothBotsResources;
            //repairButtonPointerEvents.PointerEntered += PreviewRepairCost;

        }

        private void OnDisable()
        {
            Camera.onPostRender -= _droneDesigner.DrawGL;
            _droneDesigner.RecycleDrone();

            DroneDesigner?.ClearUndoRedoStacks();

            PlayerDataManager.OnValuesChanged -= UpdateBotResourceElements;
            PlayerDataManager.OnCapacitiesChanged -= UpdateBotResourceElements;
            
            
            //launchButtonPointerEvents.PointerEntered -= PreviewFillBothBotsResources;
            //repairButtonPointerEvents.PointerEntered -= PreviewRepairCost;

            Globals.ScaleCamera(Globals.CameraScaleSize);
        }

        #endregion //Unity Functions

        //============================================================================================================//

        #region Init

        private void InitButtons()
        {
            _repairButtonText = repairButton.GetComponentInChildren<TMP_Text>();
            
            repairButton.onClick.AddListener(() =>
            {
                /*DroneDesigner.RepairParts();
                PreviewRepairCost(false);*/
            });
            
            launchButton.onClick.AddListener(() =>
            {
                SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.SCRAPYARD);
            });

            //--------------------------------------------------------------------------------------------------------//

        }

        #endregion //Init

        //============================================================================================================//

        #region Scroll Views

        public void RefreshScrollViews()
        {
            UpdateBotResourceElements();
        }

        public void UpdateBotResourceElements()
        {
            /*foreach (BIT_TYPE _bitType in Constants.BIT_ORDER)
            {
                if (_bitType == BIT_TYPE.WHITE)
                    continue;

                PlayerResource playerResource = PlayerDataManager.GetResource(_bitType);

                var data = new ResourceAmount
                {
                    //resourceType = CraftCost.TYPE.Bit,
                    type = _bitType,
                    amount = playerResource.resource,
                    capacity = playerResource.resourceCapacity
                };

                var element = resourceScrollView.AddElement(data, $"{_bitType}_UIElement");
                element.Init(data);
            }*/

            //liquidResourceContentView
            foreach (BIT_TYPE _bitType in Constants.BIT_ORDER)
            {
                if (_bitType == BIT_TYPE.WHITE /*|| _bitType == BIT_TYPE.BLUE*/)
                    continue;
                
                if (DroneDesigner._scrapyardBot == null)
                    continue;
                
                PlayerResource playerResource = PlayerDataManager.GetResource(_bitType);

                var data = new ResourceAmount
                {
                    amount = (int)playerResource.liquid,
                    capacity = playerResource.liquidCapacity,
                    type = _bitType,
                };

                /*if (_bitType == BIT_TYPE.YELLOW)
                    System.Console.WriteLine("");*/

                var element = liquidResourceContentView.AddElement(data, $"{_bitType}_UIElement");
                element.Init(data, true);
                
                element.gameObject.SetActive(DroneDesigner._scrapyardBot.UsedResourceTypes.Contains(_bitType));
            }

            UpdateFlightDataUI();
            
            //UpdateRepairButton();
        }

        #endregion //Scroll Views

        //============================================================================================================//

        #region Flight Data UI

        private void UpdateFlightDataUI()
        {
            //--------------------------------------------------------------------------------------------------------//
            if (_droneDesigner?._scrapyardBot is null)
            {
                flightDataText.text = "Flight Data:\nPower Draw: 0.0 KW/s\nTotal Power: Infinite";
                return;
            }
            
            var partCapacity = _droneDesigner._scrapyardBot.PartCapacity;

            
            var powerDraw = _droneDesigner._scrapyardBot.PowerDraw;
            var availablePower =
                Mathf.Clamp(
                    PlayerDataManager.GetResource(BIT_TYPE.YELLOW).liquid, 0,
                    PlayerDataManager.GetResource(BIT_TYPE.YELLOW).liquidCapacity);

            string powerTime = "infinite";
            if(powerDraw > 0)
                powerTime = TimeSpan.FromSeconds(availablePower / powerDraw).ToString(@"mm\:ss") + "s";

            flightDataText.text = $"Flight Data:\nParts: {partCapacity}\nPower Draw: {powerDraw:0.0} KW/s\nTotal Power: {powerTime}";

            //Temporarily remove flight data from screen
            flightDataText.gameObject.SetActive(false);
        }

        #endregion //Flight Data UI

        //====================================================================================================================//
        
        #region Other

        public void DisplayInsufficientResources()
        {
            Alert.ShowAlert("Alert!",
                "You do not have enough resources to purchase this part!", "Okay", null);
        }

        #endregion //Other

        //============================================================================================================//
    }
}

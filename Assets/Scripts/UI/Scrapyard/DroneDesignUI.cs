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
        private PointerEvents repairButtonPointerEvents;

        //====================================================================================================================//
        
        [SerializeField] 
        private PurchasePatchUIElementScrollView purchasePatchUIElementScrollView;

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

            _scrollViewsSetup = true;

            _currentlyOverwriting = false;


            InitPurchasePatches();
        }

        private void OnEnable()
        {
            Camera.onPostRender += _droneDesigner.DrawGL;
            _droneDesigner.SetupDrone();
        }

        private void OnDisable()
        {
            Camera.onPostRender -= _droneDesigner.DrawGL;
            _droneDesigner.RecycleDrone();

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

        private void InitPurchasePatches()
        {
            var patches = new[]
            {
                new Purchase_PatchData
                {
                    cost = 10,
                    PatchData = new PatchData
                    {
                        Level = 0,
                        Type = (int)PATCH_TYPE.RANGE
                    }
                },
                new Purchase_PatchData
                {
                    cost = 10,
                    PatchData = new PatchData
                    {
                        Level = 0,
                        Type = (int)PATCH_TYPE.DAMAGE
                    }
                },
                new Purchase_PatchData
                {
                    cost = 10,
                    PatchData = new PatchData
                    {
                        Level = 0,
                        Type = (int)PATCH_TYPE.FIRE_RATE
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

        #endregion //Other

        //============================================================================================================//
    }
}

﻿using System;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class ScrapyardUI : MonoBehaviour
    {
        //============================================================================================================//
        
        /*[SerializeField, Required]
        private GameObject droneDesignWindow;
        [SerializeField, Required]
        private GameObject craftingWindow;
        [SerializeField, Required]
        private GameObject storageWindow;*/
        [SerializeField, Required]
        private GameObject missionsWindow;
        
        [SerializeField, Required]
        private GameObject saveGameWindow;
        
        //============================================================================================================//

        /*[SerializeField, Required, FoldoutGroup("View Drone Window")]
        private GameObject viewDroneWindow;*/
        [SerializeField, Required, FoldoutGroup("View Drone Window")]
        private Button launchButton;

        //============================================================================================================//

        
        /*[SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button launchNavButton;
        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button droneDesignButton;
        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button craftingButton;
        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button storageButton;*/
        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button missionsButton;
        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button closeMissionsButton;
        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button menuButton;
        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button saveGameButton;

        //============================================================================================================//
        [SerializeField]
        private CameraController CameraController;
        
        private DroneDesigner _droneDesigner;
        
        //============================================================================================================//

        
        // Start is called before the first frame update
        private void Start()
        {
            _droneDesigner = FindObjectOfType<DroneDesigner>();
            
            InitButtons();
            
            //ShowMenu(MENU.LAUNCH);
            missionsWindow.SetActive(false);
            
            
        }

        private void OnEnable()
        {
            CameraController.CameraOffset(Vector3.zero, true);
        }

        //============================================================================================================//

        private void InitButtons()
        {
            //Launch Window Buttons
            //--------------------------------------------------------------------------------------------------------//

            launchButton.onClick.AddListener(Launch);
            
            //Navigation Buttons
            //--------------------------------------------------------------------------------------------------------//

            /*launchNavButton.onClick.AddListener(() => ShowMenu(MENU.LAUNCH));
            droneDesignButton.onClick.AddListener(() => ShowMenu(MENU.DESIGN));
            craftingButton.onClick.AddListener(() => ShowMenu(MENU.CRAFT));
            storageButton.onClick.AddListener(() => ShowMenu(MENU.STORAGE));*/
            missionsButton.onClick.AddListener(() =>
            {
                missionsWindow.SetActive(!missionsWindow.activeInHierarchy);
            });
            closeMissionsButton.onClick.AddListener(() =>
            {
                missionsWindow.SetActive(false);
            });
            menuButton.onClick.AddListener(() =>
            {
                SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.SCRAPYARD);
            });
            saveGameButton.onClick.AddListener(() =>
            {
                saveGameWindow.SetActive(true);
            });

            //launchNavButton.interactable = true;
            //droneDesignButton.interactable = true;
            //craftingButton.interactable = false;
            //storageButton.interactable = false;
            //missionsButton.interactable = false;
            //menuButton.interactable = true;

            //--------------------------------------------------------------------------------------------------------//

        }
        
        

        //Launch Window Functions
        //============================================================================================================//
        
        private void Launch()
        {
            if (!_droneDesigner.IsFullyConnected())
            {
                Alert.ShowAlert("Alert!",
                    "A disconnected piece is active on your Bot! Please repair before continuing", "Fix",
                    () =>
                    {
                        /*ShowMenu(MENU.DESIGN);*/
                    });
                
                return;
            }

            //TODO Need to decide if this should happen at arrival or at launch
            TryFillBotResources();
            
            _droneDesigner.ProcessScrapyardUsageEndAnalytics();
            
            if (Globals.SectorComplete)
            {
                Globals.SectorComplete = false;
            }
            
            SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.SCRAPYARD);
            
        }
        
        static readonly BIT_TYPE[] types = {
            BIT_TYPE.RED,
            BIT_TYPE.GREY,
            BIT_TYPE.GREEN
        };
        private void TryFillBotResources()
        {
            foreach (var bitType in types)
            {
                var currentAmount = PlayerPersistentData.PlayerData.liquidResource[bitType];
                var currentCapacity = PlayerPersistentData.PlayerData.liquidCapacity[bitType];

                var fillRemaining = currentCapacity - currentAmount;

                //If its already full, then we're good to move on
                if (fillRemaining <= 0f)
                    continue;

                var availableResources = PlayerPersistentData.PlayerData.resources[bitType];

                //If we have no resources available to refill the liquid, move onto the next
                if(availableResources <= 0)
                    continue;

                var movingAmount = Mathf.RoundToInt(Mathf.Min(availableResources, fillRemaining));

                PlayerPersistentData.PlayerData.resources[bitType] -= movingAmount;
                PlayerPersistentData.PlayerData.AddLiquidResource(bitType, movingAmount);
            }
        }
        
        //Menu Functions
        //============================================================================================================//

        /*private void ShowMenu(MENU menu)
        {
            
            viewDroneWindow.SetActive(false);
            droneDesignWindow.SetActive(false);
            craftingWindow.SetActive(false);
            storageWindow.SetActive(false);
            missionsWindow.SetActive(false);
            
            CameraController.CameraOffset(Vector3.zero, menu == MENU.DESIGN);
            //FIXME This should be happening within the DroneDesigner
            _droneDesigner.selectedPartType = null;
            
            switch (menu)
            {
                case MENU.LAUNCH:
                    viewDroneWindow.SetActive(true);
                    break;
                case MENU.DESIGN:
                    droneDesignWindow.SetActive(true);
                    break;
                case MENU.CRAFT:
                    craftingWindow.SetActive(true);
                    break;
                case MENU.STORAGE:
                    storageWindow.SetActive(true);
                    break;
                case MENU.MISSION:
                    missionsWindow.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(menu), menu, null);
            }
        }*/
        
        //============================================================================================================//
    } 
}


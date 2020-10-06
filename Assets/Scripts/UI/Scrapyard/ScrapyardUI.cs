using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class ScrapyardUI : MonoBehaviour
    {
        //============================================================================================================//

        [SerializeField, Required]
        private GameObject missionsWindow;
        
        [SerializeField, Required]
        private GameObject workbenchWindow;

        [SerializeField, Required]
        private GameObject logisticsWindow;
        
        [SerializeField, Required]
        private GameObject saveGameWindow;
        
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button workbenchButton;
        [FormerlySerializedAs("mapButton")] 
        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button launchButton;
        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button logisticsButton;
        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button missionsButton;
        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button menuButton;

        //====================================================================================================================//
        
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
            
                        
            workbenchWindow.SetActive(true);
            saveGameWindow.SetActive(false);
            logisticsWindow.SetActive(false);
            missionsWindow.SetActive(false);
        }

        private void OnEnable()
        {
            CameraController.CameraOffset(Vector3.zero, true);
            
            workbenchButton.onClick?.Invoke();
        }

        //============================================================================================================//

        private void InitButtons()
        {
            //Launch Window Buttons
            //--------------------------------------------------------------------------------------------------------//
            
            launchButton.onClick.AddListener(TryLaunch);
            
            //Navigation Buttons
            //--------------------------------------------------------------------------------------------------------//

            workbenchButton.onClick.AddListener(() =>
            {
                workbenchWindow.SetActive(true);
                saveGameWindow.SetActive(false);
                logisticsWindow.SetActive(false);
                missionsWindow.SetActive(false);
            });
            
            
            missionsButton.onClick.AddListener(() =>
            {
                workbenchWindow.SetActive(false);
                saveGameWindow.SetActive(false);
                logisticsWindow.SetActive(false);
                missionsWindow.SetActive(true);
            });

            menuButton.onClick.AddListener(() =>
            {
                PlayerPersistentData.SaveAutosaveFiles();
                SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.SCRAPYARD);
            });
            saveGameButton.onClick.AddListener(() =>
            {
                saveGameWindow.SetActive(true);
            });
            
            logisticsButton.onClick.AddListener(() =>
            {
                workbenchWindow.SetActive(false);
                saveGameWindow.SetActive(false);
                logisticsWindow.SetActive(true);
                missionsWindow.SetActive(false);
            });

            //--------------------------------------------------------------------------------------------------------//

        }

        //Launch Window Functions
        //============================================================================================================//
        
        private void TryLaunch()
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

            //Checks to see if we need to display a window
            if (PlayerPersistentData.PlayerData.partsInStorageBlockData.Count > 0)
            {
                Alert.ShowAlert("Warning!", 
                    "You have unused parts left in storage, are you sure you want to launch?",
                    "Launch!", 
                    "Back", 
                    state =>
                    {
                        if(state) Launch();
                        
                    }, 
                    "PartsStorage");
                
                return;
            }

            

            Launch();
        }

        private void Launch()
        {
            //TODO Need to decide if this should happen at arrival or at launch
            TryFillBotResources(true);
            TryFillBotResources(false);
            
            _droneDesigner.ProcessScrapyardUsageEndAnalytics();
            
            if (Globals.SectorComplete)
            {
                Globals.SectorComplete = false;
            }
            
            SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.SCRAPYARD);
        }
        
        
        private void TryFillBotResources(bool isRecoveryDrone)
        {
            BIT_TYPE[] types = {
                BIT_TYPE.RED,
                BIT_TYPE.GREY,
                BIT_TYPE.GREEN,
                BIT_TYPE.YELLOW
            };

            List<BlockData> botData;
            if (isRecoveryDrone)
            {
                botData = PlayerPersistentData.PlayerData.recoveryDroneBlockData;
            }
            else
            {
                botData = PlayerPersistentData.PlayerData.currentBlockData;
            }
            
            foreach (var bitType in types)
            {
                switch (bitType)
                {
                    case BIT_TYPE.GREEN:
                        //TODO Check for repair
                        if(!botData.Any(b => b.Type == (int)PART_TYPE.REPAIR))
                            continue;
                        break;
                    case BIT_TYPE.GREY:
                        //TODO Check for a gun
                        if (!botData.Any(b => b.Type == (int)PART_TYPE.GUN || b.Type == (int)PART_TYPE.TRIPLESHOT))
                            continue;
                        break;
                    case BIT_TYPE.YELLOW:
                        for (int i = 0; i < botData.Count; i++)
                        {
                            var partData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData((PART_TYPE)botData[i].Type).levels[botData[i].Level];
                            if (partData.powerDraw > 0)
                            {
                                continue;
                            }
                        }
                        break;
                    case BIT_TYPE.RED:
                        break;
                    default:
                        continue;
                }


                float currentAmount;
                if (isRecoveryDrone)
                {
                    currentAmount = PlayerPersistentData.PlayerData.recoveryDroneLiquidResource[bitType];
                }
                else
                {
                    currentAmount = PlayerPersistentData.PlayerData.liquidResource[bitType];
                }
                float currentCapacity;
                if (isRecoveryDrone)
                {
                    currentCapacity = PlayerPersistentData.PlayerData.recoveryDroneLiquidCapacity[bitType];
                }
                else
                {
                    currentCapacity = PlayerPersistentData.PlayerData.liquidCapacity[bitType];
                }

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
                PlayerPersistentData.PlayerData.AddLiquidResource(bitType, movingAmount, isRecoveryDrone);
            }
        }
        
        //============================================================================================================//

    } 
}


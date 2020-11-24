using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
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
        private GameObject shipInteriorWindow;
        [SerializeField, Required]
        private GameObject missionsWindow;
        
        [SerializeField, Required]
        private GameObject workbenchWindow;

        [SerializeField, Required]
        private GameObject logisticsWindow;
        
        /*[SerializeField, Required]
        private GameObject saveGameWindow;*/

        [SerializeField, Required, FoldoutGroup("Settings Menu")]
        private GameObject settingsWindow;
        [SerializeField, Required, FoldoutGroup("Settings Menu")]
        private Button resumeGameButton;
        /*[SerializeField, Required, FoldoutGroup("Settings Menu")]
        private Button saveGameButton;
        [SerializeField, Required, FoldoutGroup("Settings Menu")]
        private Button loadGameButton;*/
        [SerializeField, Required, FoldoutGroup("Settings Menu")]
        private Button settingsButton;
        [SerializeField, Required, FoldoutGroup("Settings Menu")]
        private Button quitGameButton;
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
        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button backButton;

        //====================================================================================================================//
        
        [SerializeField]
        private CameraController CameraController;
        
        private DroneDesigner _droneDesigner;

        private GameObject[] _windows;
        private enum Window
        {
            ShipInterior = 0,
            Workbench,
            Logistics,
            Missions,
            Settings,
        }
        
        //============================================================================================================//

        
        // Start is called before the first frame update
        private void Start()
        {
            _droneDesigner = FindObjectOfType<DroneDesigner>();
            
            _windows = new[]
            {
                shipInteriorWindow,
                workbenchWindow,
                logisticsWindow,
                missionsWindow,
                settingsWindow,
                //saveGameWindow
            };
            
            InitButtons();
            InitSettingsButtons();
        }

        private void OnEnable()
        {
            CameraController.CameraOffset(Vector3.zero, true);
            
            backButton.onClick?.Invoke();
            
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
                backButton.gameObject.SetActive(true);
                
                SetWindowActive(Window.Workbench);
            });
            
            missionsButton.onClick.AddListener(() =>
            {
                backButton.gameObject.SetActive(true);

                SetWindowActive(Window.Missions);
            });
            
            logisticsButton.onClick.AddListener(() =>
            {
                backButton.gameObject.SetActive(true);
                
                SetWindowActive(Window.Logistics);
            });
            
            menuButton.onClick.AddListener(() =>
            {
                _windows[(int)Window.Settings].SetActive(true);
            });

            
            backButton.onClick.AddListener(() =>
            {
                backButton.gameObject.SetActive(false);
                
                SetWindowActive(Window.ShipInterior);
            });

            //--------------------------------------------------------------------------------------------------------//

        }

        private void InitSettingsButtons()
        {
            resumeGameButton.onClick.AddListener(() =>
            {
                _windows[(int)Window.Settings].SetActive(false);
            });

            /*saveGameButton.onClick.AddListener(() =>
            {
                PlayerDataManager.SavePlayerAccountData();
            });
            
            loadGameButton.onClick.AddListener(() =>
            {
                throw new NotImplementedException();
            });*/
            settingsButton.onClick.AddListener(() =>
            {
                throw new NotImplementedException();
            });
            
            quitGameButton.onClick.AddListener(() =>
            {
                Alert.ShowAlert("Quitting",
                    "Are you sure you want to save & quit?",
                    "Desktop",
                    "Main Menu",
                    "Cancel",
                    quit =>
                    {
                        PlayerDataManager.SavePlayerAccountData();
                        //PlayerDataManager.ClearPlayerAccountData();

                        if (!quit)
                        {
                            ScreenFade.Fade(() =>
                            {
                                SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.SCRAPYARD);
                            });
                            
                            return;
                        }
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
                    },
                    null);
            });
        }

        //Launch Window Functions
        //============================================================================================================//
        
        private void TryLaunch()
        {
            if (PlayerDataManager.GetBlockDatas().CheckHasDisconnects())
            {
                Alert.ShowAlert("Alert!",
                    "A disconnected piece is active on your Bot! Please repair before continuing", "Fix",
                    () =>
                    {
                        backButton.gameObject.SetActive(true);

                        SetWindowActive(Window.Workbench);
                    });
                
                return;
            }

            //Checks to see if we need to display a window
            if (PlayerDataManager.GetCurrentPartsInStorage().Count > 0)
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
            Globals.IsRecoveryBot = true;
            TryFillBotResources();
            Globals.IsRecoveryBot = false;
            TryFillBotResources();
            
            _droneDesigner.ProcessScrapyardUsageEndAnalytics();
            
            if (Globals.SectorComplete)
            {
                Globals.SectorComplete = false;
            }
            
            
            
            ScreenFade.Fade(() =>
            {
                AudioController.FadeOutMusic();
                SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.SCRAPYARD);
            });
        }
        
        
        private void TryFillBotResources()
        {
            BIT_TYPE[] types = {
                BIT_TYPE.RED,
                BIT_TYPE.GREY,
                BIT_TYPE.GREEN,
                BIT_TYPE.YELLOW
            };

            List<BlockData> botData = PlayerDataManager.GetBlockDatas();
            
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


                float currentAmount = PlayerDataManager.GetResource(bitType).liquid;
                float currentCapacity = PlayerDataManager.GetResource(bitType).liquidCapacity;

                var fillRemaining = currentCapacity - currentAmount;

                //If its already full, then we're good to move on
                if (fillRemaining <= 0f)
                    continue;

                var availableResources = PlayerDataManager.GetResource(bitType).resource;

                //If we have no resources available to refill the liquid, move onto the next
                if(availableResources <= 0)
                    continue;

                var movingAmount = Mathf.RoundToInt(Mathf.Min(availableResources, fillRemaining));

                PlayerDataManager.GetResource(bitType).SubtractResource(movingAmount);
                PlayerDataManager.GetResource(bitType).AddLiquid(movingAmount);
            }
        }
        
        //============================================================================================================//
        private void SetWindowActive(Window window)
        {
            SetWindowActive((int)window);
        }

        private void SetWindowActive(int index)
        {
            for (var i = 0; i < _windows.Length; i++)
            {
                _windows[i].SetActive(i == index);
            }
        }

    } 
}


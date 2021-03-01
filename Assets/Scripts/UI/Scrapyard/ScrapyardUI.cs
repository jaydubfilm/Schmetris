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
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class ScrapyardUI : MonoBehaviour
    {
        //============================================================================================================//

        [SerializeField, Required]
        private GameObject workbenchWindow;

        //====================================================================================================================//

        [SerializeField, Required, FoldoutGroup("Menu Window")]
        private GameObject settingsWindow;
        [SerializeField, Required, FoldoutGroup("Menu Window")]
        private Button resumeGameButton;
        [SerializeField, Required, FoldoutGroup("Menu Window")]
        private Button settingsButton;
        [SerializeField, Required, FoldoutGroup("Menu Window")]
        private Button quitGameButton;

        //====================================================================================================================//

        [SerializeField, Required, FoldoutGroup("Settings Window")]
        private GameObject settingsWindowObject;
        [SerializeField, Required, FoldoutGroup("Settings Window")]
        private Button settingsBackButton;
        [SerializeField, Required, FoldoutGroup("Settings Window")]
        private Slider musicVolumeSlider;
        [SerializeField, Required, FoldoutGroup("Settings Window")]
        private Slider sfxVolumeSlider;
        [SerializeField, Required, FoldoutGroup("Settings Window")]
        private Toggle testingFeaturesToggle;

        //====================================================================================================================//

        [SerializeField, Required, FoldoutGroup("Part Choice Window")]
        private GameObject partChoiceWindow;

        //============================================================================================================//
        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button menuButton;
        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button backButton;

        [SerializeField, Required, FoldoutGroup("Gears Indicator")]
        private TMP_Text gearsNumber;

        //====================================================================================================================//

        [SerializeField]
        private CameraController CameraController;

        private DroneDesigner _droneDesigner;

        private PartChoiceUI _partChoice;

        private GameObject[] _windows;
        private enum Window
        {
            Workbench,
            Settings,
        }

        //============================================================================================================//



        // Start is called before the first frame update
        private void Start()
        {
            _droneDesigner = FindObjectOfType<DroneDesigner>();
            _partChoice = FindObjectOfType<PartChoiceUI>();

            _windows = new[]
            {
                workbenchWindow,
                settingsWindow
            };

            InitButtons();
            InitMenuButtons();
            InitSettings();

            SetWindowActive(Window.Workbench);
        }

        private void Update()
        {
            gearsNumber.text = $"{PlayerDataManager.GetGears()}";
        }

        private void OnEnable()
        {
            CameraController.CameraOffset(Vector3.zero, true);
            CameraController.SetOrthographicSize(31f, Vector3.down * 5f);

            backButton.onClick?.Invoke();

            partChoiceWindow.SetActive(PlayerDataManager.GetCanChoosePart());
            
            //--------------------------------------------------------------------------------------------------------//
            
            if (PlayerDataManager.GetCanChoosePart())
            {
                if (_partChoice == null)
                {
                    _partChoice = FindObjectOfType<PartChoiceUI>();
                }
                bool notYetStarted = PlayerDataManager.GetStarted();

                if (!notYetStarted)
                {
                    _partChoice.Init(PartAttachableFactory.PART_OPTION_TYPE.BasicWeapon);
                    PlayerDataManager.ClearAllPatches();
                }
                else
                {
                    _partChoice.Init(PartAttachableFactory.PART_OPTION_TYPE.Any);
                    
                    PlayerDataManager.SetPatches(Globals.CurrentRing.GenerateRingPatches());
                    _droneDesigner.DroneDesignUi.InitPurchasePatches();
                }
            }

        }

        //============================================================================================================//

        private void InitButtons()
        {

            menuButton.onClick.AddListener(() =>
            {
                _windows[(int)Window.Settings].SetActive(true);
            });


            backButton.onClick.AddListener(() =>
            {
            });

            //--------------------------------------------------------------------------------------------------------//

        }

        private void InitMenuButtons()
        {
            resumeGameButton.onClick.AddListener(() =>
            {
                _windows[(int)Window.Settings].SetActive(false);
            });

            settingsButton.onClick.AddListener(() =>
            {
                settingsWindowObject.SetActive(true);
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
                                
                                settingsWindowObject.SetActive(false);
                                SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.SCRAPYARD, MUSIC.MAIN_MENU);
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

        private void InitSettings()
        {
            musicVolumeSlider.onValueChanged.AddListener(AudioController.SetMusicVolume);

            sfxVolumeSlider.onValueChanged.AddListener(AudioController.SetSFXVolume);

            testingFeaturesToggle.onValueChanged.AddListener(toggle =>
            {
                Globals.TestingFeatures = toggle;
            });

            settingsBackButton.onClick.AddListener(() =>
            {
                settingsWindowObject.SetActive(false);
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
            _droneDesigner.ProcessScrapyardUsageEndAnalytics();

            ScreenFade.Fade(() =>
            {
                SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.SCRAPYARD);
            });
        }

        /*private void EscPressed()
        {
            switch (_currentWindow)
            {
                case Window.ShipInterior:
                    _windows[(int)Window.Settings].SetActive(true);
                    _currentWindow = Window.Settings;
                    break;
                case Window.Workbench:
                case Window.Settings:
                    _windows[(int)Window.Settings].SetActive(false);
                    _currentWindow = Window.ShipInterior;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }*/

        //============================================================================================================//

        /*private Window _currentWindow;*/

        private void SetWindowActive(Window window)
        {
            //_currentWindow = window;
            SetWindowActive((int)window);

            menuButton.gameObject.SetActive(true);
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

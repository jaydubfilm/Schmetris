﻿using System;
using UnityEngine;
using UnityEngine.UI;
using StarSalvager.Utilities.SceneManagement;
using UnityEngine.SceneManagement;
using StarSalvager.Values;
using StarSalvager.Cameras.Data;
using StarSalvager.Utilities;
using StarSalvager.Utilities.UI;
using Sirenix.OdinInspector;
using StarSalvager.Factories;

using CameraController = StarSalvager.Cameras.CameraController;
using System.Collections;
using StarSalvager.Audio;
using System.Collections.Generic;
using StarSalvager.Utilities.FileIO;
using TMPro;
using StarSalvager.Utilities.Saving;

namespace StarSalvager.UI
{
    [Obsolete("Use MainMenuv2 instead")]
    //FIXME Once the navigation style is decided, we can better solidify the data structure for the menus
    //FIXME All windows can be combined to reduce total images used
    public class MainMenu : MonoBehaviour
    {
        private enum MENU
        {
            MAIN,
            NEW,
            LOAD,
            OPTION
        }

        private enum MENUSTATE
        {
            MAINMENU,
            GAMEMENU
        }

        //============================================================================================================//

        [SerializeField] private CameraController m_cameraController;
        public CameraController CameraController => m_cameraController;

        [SerializeField]
        private GameObject menuCharactersRootObject;

        //============================================================================================================//

        #region Menu Windows

        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private TMP_Text headerText;

        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private GameObject mainMenuWindow;

        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private Button newGameButton;

        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private Button continueButton;

        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private TMP_Text continueButtonText;

        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private Button loadGameButton;

        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private Button optionsButton;

        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private Button quitButton;

        [SerializeField, Required, BoxGroup("Main Menu/Testing", Order = -1000)]
        private Button m_toggleOrientationButton;

        [SerializeField, Required, BoxGroup("Main Menu/Testing")]
        private Slider m_cameraZoomScaler;

        [SerializeField, Required, BoxGroup("Main Menu/Testing")]
        private SliderText _zoomSliderText;

        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("New Game Menu")]
        private GameObject newGameWindow;

        [SerializeField, Required, FoldoutGroup("New Game Menu")]
        private Button startGameButton;

        [SerializeField, Required, FoldoutGroup("New Game Menu")]
        private Button tutorialButton;

        [SerializeField, Required, FoldoutGroup("New Game Menu")]
        private Button ngBackButton;

        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Load Game Menu")]
        private GameObject loadGameWindow;
        /*[SerializeField, Required, FoldoutGroup("Load Game Menu")]
        private Button slot1Button;
        [SerializeField, Required, FoldoutGroup("Load Game Menu")]
        private Button slot2Button;
        [SerializeField, Required, FoldoutGroup("Load Game Menu")]
        private Button slot3Button;
        [SerializeField, Required, FoldoutGroup("Load Game Menu")]
        private Button lgBackButton;*/

        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Options Menu")]
        private GameObject optionsWindow;

        [SerializeField, Required, FoldoutGroup("Options Menu")]
        private Slider musicSlider;

        [SerializeField, Required, FoldoutGroup("Options Menu")]
        private Slider sfxSlider;

        [SerializeField, Required, FoldoutGroup("Options Menu")]
        private Button oBackButton;

        [SerializeField, Required, FoldoutGroup("Options Menu")]
        private Toggle testingFeaturesToggle;

        [SerializeField, Required] private GameObject introSceneCanvas;

        private MENUSTATE menuState = MENUSTATE.MAINMENU;

        #endregion //Menu Windows

        //============================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            StartCoroutine(Init());
        }

        private void OnEnable()
        {
            if (menuState == MENUSTATE.MAINMENU)
            {
                continueButtonText.text = "Continue";
                headerText.text = "Star Salvager\nMain Menu";
            }
            else if (menuState == MENUSTATE.GAMEMENU)
            {
                continueButtonText.text = "Resume";
                headerText.text = "Star Salvager\nGame Menu";
            }
        }

        private IEnumerator Init()
        {
            newGameButton.interactable = false;
            continueButton.interactable = false;
            loadGameButton.interactable = false;

            while (!SceneLoader.IsReady)
                yield return null;
            
            newGameButton.interactable = true;

            Dictionary<string, object> applicationOpenAnalyticsDictionary = new Dictionary<string, object>();
            //applicationOpenAnalyticsDictionary.Add("User ID", Globals.UserID);
            //applicationOpenAnalyticsDictionary.Add("Session ID", Globals.SessionID);
            //applicationOpenAnalyticsDictionary.Add("Playthrough ID", PlayerPersistentData.PlayerData.PlaythroughID);
            //applicationOpenAnalyticsDictionary.Add("Start Time", DateTime.Now.ToString());
            AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.ApplicationOpen,
                eventDataDictionary: applicationOpenAnalyticsDictionary);

            InitButtons();

            OpenMenu(MENU.MAIN);

            if (gameObject.scene == SceneManager.GetActiveScene())
                Globals.ScaleCamera(m_cameraZoomScaler.value);

            testingFeaturesToggle.isOn = Globals.TestingFeatures;
        }

        private void Update()
        {
            if (!SceneLoader.IsReady)
                return;

            continueButton.interactable = PlayerDataManager.GetIndexMostRecentSaveFile() >= 0;
            loadGameButton.interactable = PlayerDataManager.GetSaveFiles().Count > 0;

            m_toggleOrientationButton.gameObject.SetActive(Globals.TestingFeatures);
            m_cameraZoomScaler.gameObject.SetActive(Globals.TestingFeatures);
            _zoomSliderText.Text.gameObject.SetActive(Globals.TestingFeatures);
        }

        //============================================================================================================//

        private void InitButtons()
        {
            m_toggleOrientationButton.onClick.AddListener(() =>
            {
                Globals.Orientation = Globals.Orientation == ORIENTATION.HORIZONTAL
                    ? ORIENTATION.VERTICAL
                    : ORIENTATION.HORIZONTAL;
            });

            _zoomSliderText.Init();
            m_cameraZoomScaler.onValueChanged.AddListener(Globals.ScaleCamera);


            //Main Menu Buttons
            //--------------------------------------------------------------------------------------------------------//

            newGameButton.onClick.AddListener(() =>
            {
                if (menuState == MENUSTATE.MAINMENU)
                {
                    OpenMenu(MENU.NEW);
                }
                else
                {
                    Alert.ShowAlert("New Game",
                        "Starting a new game may override your autosave data. Are you sure you want to continue?",
                        "Yes", "No", (b) =>
                        {
                            if (b)
                            {
                                OpenMenu(MENU.NEW);
                            }
                        });
                }
            });

            continueButton.onClick.AddListener(() =>
            {
                int saveSlotIndex = PlayerDataManager.GetIndexMostRecentSaveFile();

                if (saveSlotIndex >= 0)
                {
                    print("LOADING FILE " + saveSlotIndex);

                    PlayerDataManager.SetCurrentSaveSlotIndex(saveSlotIndex);
                    FactoryManager.Instance.currentModularDataIndex = 0;
                    PlayerDataManager.SetRunStarted();

                    //menuState = MENUSTATE.GAMEMENU;
                    SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.MAIN_MENU);
                }
            });

            loadGameButton.onClick.AddListener(() => loadGameWindow.SetActive(true));

            optionsButton.onClick.AddListener(() => OpenMenu(MENU.OPTION));

            quitButton.onClick.AddListener(() =>
            {
                Alert.ShowAlert("Quit", "Are you sure you want to quit?", "Yes", "No", (b) =>
                {
                    if (b)
                    {
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
                    }
                });
            });

            //New Game Buttons
            //--------------------------------------------------------------------------------------------------------//

            startGameButton.onClick.AddListener(() =>
            {
                OpenMenu(MENU.MAIN);

                int saveSlotIndex = Files.GetNextAvailableSaveSlot();

                if (saveSlotIndex >= 0)
                {
                    PlayerDataManager.SetCurrentSaveSlotIndex(saveSlotIndex);
                    PlayerDataManager.ResetPlayerAccountData();
                    PlayerDataManager.SetRunStarted();

                    introSceneCanvas.SetActive(true);
                    mainMenuWindow.SetActive(false);
                    menuCharactersRootObject.SetActive(false);

                    //SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.MAIN_MENU);
                }
                /*else
                {
                    Toast.AddToast("No empty save slots! Load an existing game or delete a save file to proceed.",
                        time: 3.0f);
                }*/
            });

            tutorialButton.onClick.AddListener(() =>
            {
                Globals.UsingTutorial = true;
                Globals.CurrentSector = 4;
                Globals.CurrentWave = 0;

                SceneLoader.ActivateScene(SceneLoader.LEVEL, SceneLoader.MAIN_MENU);
            });

            ngBackButton.onClick.AddListener(() => OpenMenu(MENU.MAIN));

            //Load Game Buttons
            //--------------------------------------------------------------------------------------------------------//

            //FIXME This will likely need to be scalable
            /* slot1Button.onClick.AddListener(() =>
             {
                 OpenMenu(MENU.MAIN);
                 PlayerPersistentData.SetCurrentSaveFile(0);
                 MissionManager.SetCurrentSaveFile();
                 PlayerPersistentData.IsNewFile = false;
                 SceneLoader.ActivateScene("UniverseMapScene", "MainMenuScene");
             });
 
             slot2Button.onClick.AddListener(() =>
             {
                 OpenMenu(MENU.MAIN);
                 PlayerPersistentData.SetCurrentSaveFile(1);
                 MissionManager.SetCurrentSaveFile();
                 PlayerPersistentData.IsNewFile = false;
                 SceneLoader.ActivateScene("UniverseMapScene", "MainMenuScene");
             });
             //slot2Button.interactable = false;
             
             slot3Button.onClick.AddListener(() =>
             {
                 OpenMenu(MENU.MAIN);
                 PlayerPersistentData.SetCurrentSaveFile(2);
                 MissionManager.SetCurrentSaveFile();
                 PlayerPersistentData.IsNewFile = false;
                 SceneLoader.ActivateScene("UniverseMapScene", "MainMenuScene");
             });
             //slot3Button.interactable = false;
 
             lgBackButton.onClick.AddListener(() => OpenMenu(MENU.MAIN));*/

            //Options Buttons
            //--------------------------------------------------------------------------------------------------------//

            musicSlider.onValueChanged.AddListener(AudioController.SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(AudioController.SetSFXVolume);

            oBackButton.onClick.AddListener(() => OpenMenu(MENU.MAIN));

            testingFeaturesToggle.onValueChanged.AddListener(delegate
            {
                Globals.TestingFeatures = testingFeaturesToggle.isOn;
            });

            //--------------------------------------------------------------------------------------------------------//



        }

        //============================================================================================================//

        private void OpenMenu(MENU menu)
        {
            mainMenuWindow.SetActive(false);
            newGameWindow.SetActive(false);
            optionsWindow.SetActive(false);

            switch (menu)
            {
                case MENU.MAIN:
                    mainMenuWindow.SetActive(true);
                    break;
                case MENU.NEW:
                    newGameWindow.SetActive(true);
                    break;
                case MENU.OPTION:
                    optionsWindow.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(menu), menu, null);
            }
        }


        //============================================================================================================//

        /*private void ScaleCamera(float cameraZoomScalerValue)
        {
            Globals.ColumnsOnScreen = (int)cameraZoomScalerValue;
            if (Globals.ColumnsOnScreen % 2 == 0)
                Globals.ColumnsOnScreen += 1;
            
            CameraController.SetOrthographicSize(Constants.gridCellSize * Globals.ColumnsOnScreen, Vector3.zero);

            if (Globals.Orientation == ORIENTATION.VERTICAL)
            {
                Globals.GridSizeX = (int)(Globals.ColumnsOnScreen * Constants.GridWidthRelativeToScreen);
                Globals.GridSizeY = (int)((Camera.main.orthographicSize * Constants.GridHeightRelativeToScreen * 2) / Constants.gridCellSize);
            }
            else
            {
                Globals.GridSizeX = (int)(Globals.ColumnsOnScreen * Constants.GridWidthRelativeToScreen * (Screen.height / (float)Screen.width));
                Globals.GridSizeY = (int)((Camera.main.orthographicSize * Constants.GridHeightRelativeToScreen * 2 * (Screen.width / (float)Screen.height)) / Constants.gridCellSize);
            }
        }*/

        //============================================================================================================//

#if UNITY_EDITOR

        [Button("Clear Remote Data"), DisableInPlayMode]
        private void ClearRemoteData()
        {
            Files.ClearRemoteData();
        }

        [Button("Show Current Account Stats"), DisableInEditorMode]
        private void ShowAccountStats()
        {
            if (!Application.isPlaying) 
                return;
            
            if (PlayerDataManager.HasPlayerAccountData())
            {
                var newString = PlayerDataManager.GetAccountSummaryString();

                Alert.ShowAlert("Account Tracking Statistics", newString, "Ok", null);
            }
            else
            {
                Alert.ShowAlert("No Account Loaded", "No account loaded. Load an account and then click this again.", "Ok", null);
            }
        }

#endif
    }
}
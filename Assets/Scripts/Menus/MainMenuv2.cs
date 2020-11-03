﻿using System;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Factories;
using StarSalvager.Prototype;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class MainMenuv2 : MonoBehaviour
    {
        private enum WINDOW
        {
            NONE = -1,
            MAIN_MENU,
            ACCOUNT,
            ACCOUNT_MENU,
            RUN,
            SETTINGS,
        }

        private enum GAME_TYPE
        {
            NONE,
            CLASSIC,
            HARDCORE
        }
        
        private struct WindowData
        {
            public WINDOW Type;
            public GameObject WindowObject;
            public bool CloseOtherWindows;

            public void SetActive(bool active)
            {
                WindowObject.gameObject.SetActive(active);
            }
        }

        //Intro Scene Properties
        //====================================================================================================================//
        [SerializeField, Required] 
        private IntroScene IntroScene;

        [SerializeField, Required]
        private SpriteRenderer partSprite;
        
        //Main Menu Properties
        //====================================================================================================================//
        
        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private GameObject mainMenuWindowObject;
        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private Button playButton;
        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private Button settingsButton;
        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private Button quitButton;

        //Account Window Properties
        //====================================================================================================================//
        
        [SerializeField, Required, FoldoutGroup("Account Window")]
        private GameObject accountWindowObject;
        [FormerlySerializedAs("backButton")] 
        [SerializeField, Required, FoldoutGroup("Account Window")]
        private Button accountBackButton;
        //TODO This will likely need to be something beyond a normal Button
        [SerializeField, Required, FoldoutGroup("Account Window")]
        private Button[] accountButtons;
        [SerializeField, Required, FoldoutGroup("Account Window")]
        private Button[] deleteAccountButtons;

        //Account Menu Window Properties
        //====================================================================================================================//
        [SerializeField, Required, FoldoutGroup("Account Menu Window")]
        private GameObject accountMenuWindowObject;
        [SerializeField, Required, FoldoutGroup("Account Menu Window")]
        private Button changeAccountButton;
        [SerializeField, Required, FoldoutGroup("Account Menu Window")]
        private Button accountMenuSettingsButton;
        [SerializeField, Required, FoldoutGroup("Account Menu Window")]
        private Button accountMenuQuitButton;
        [SerializeField, Required, FoldoutGroup("Account Menu Window")]
        private Button newRunButton;
        [SerializeField, Required, FoldoutGroup("Account Menu Window")]
        private Button continueRunButton;
        [SerializeField, Required, FoldoutGroup("Account Menu Window")]
        private Button abandonRunButton;
        [SerializeField, Required, FoldoutGroup("Account Menu Window")]
        private Button tutorialButton;

        //Pick Run Window Properties
        //====================================================================================================================//
        
        [SerializeField, Required, FoldoutGroup("Pick Run Window")]
        private GameObject pickRunWindowObject;
        [SerializeField, Required, FoldoutGroup("Pick Run Window")]
        private Button classicRunButton;
        [SerializeField, Required, FoldoutGroup("Pick Run Window")]
        private Button hardcoreRunButton;
        [SerializeField, Required, FoldoutGroup("Pick Run Window")]
        private Button startRunButton;
        [SerializeField, Required, FoldoutGroup("Pick Run Window")]
        private Button runBackButton;
        [SerializeField, Required, FoldoutGroup("Pick Run Window")]
        private TMP_Text runDescriptionText;
        

        //Settings Window Properties
        //====================================================================================================================//
        
        [SerializeField, Required, FoldoutGroup("Settings Window")]
        private GameObject settingsWindowObject;
        [SerializeField, Required, FoldoutGroup("Settings Window")]
        private Slider musicVolumeSlider;
        [SerializeField, Required, FoldoutGroup("Settings Window")]
        private Slider sfxVolumeSlider;
        [SerializeField, Required, FoldoutGroup("Settings Window")]
        private Toggle testingFeaturesToggle;
        [SerializeField, Required, FoldoutGroup("Settings Window")]
        private Button settingsBackButton;


        //====================================================================================================================//

        private GAME_TYPE _selectedGameType;
        
        //====================================================================================================================//
        
        private WINDOW _currentWindow = WINDOW.NONE;
        private WINDOW _previousWindow = WINDOW.NONE;
        private WindowData[] _windowData;

        //Unity Functions
        //====================================================================================================================//

        private void OnEnable()
        {
            RefreshWindow(_currentWindow);
        }

        private void Start()
        {
            partSprite.sprite = FactoryManager.Instance.PartsProfileData.GetProfile(PART_TYPE.CORE).GetSprite(0);
            
            
            SetupWindows();
            SetupButtons();
        }

        //MainMenuV2 Functions
        //====================================================================================================================//

        private void RefreshWindow(WINDOW window)
        {
            switch (window)
            {
                case WINDOW.NONE:
                case WINDOW.MAIN_MENU:
                case WINDOW.SETTINGS:
                    break;
                case WINDOW.ACCOUNT:
                    SetupAccountWindow();
                    break;
                case WINDOW.ACCOUNT_MENU:
                    SetupAccountMenuWindow();
                    break;
                case WINDOW.RUN:
                    SetupRunMenuWindow();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(window), window, null);
            }
        }

        #region Setup Windows

        //Setup Account Window
        //------------------------------------------------------------------------------------------------------------//
        
        private void SetupAccountWindow()
        {
            for (var i = 0; i < accountButtons.Length; i++)
            {
                int index = i;
                accountButtons[i].onClick.RemoveAllListeners();
                accountButtons[i].onClick.AddListener(() =>
                {
                    PlayerDataManager.SetCurrentSaveSlotIndex(index);
                    SetupAccountMenuWindow();
                    OpenWindow(WINDOW.ACCOUNT_MENU);
                });

                var hasAccount = Files.TryGetPlayerSaveData(i, out var accountData);
                var buttonText = accountButtons[i].GetComponentInChildren<TMP_Text>();
                
                deleteAccountButtons[i].gameObject.SetActive(hasAccount);
                
                buttonText.text = !hasAccount
                    ? "Create new Account"
                    : $"Load Account {i + 1}\nTotal Runs: {accountData.TotalRuns}";
            }
        }
        
        //Setup Account Menu Window
        //------------------------------------------------------------------------------------------------------------//

        private void SetupAccountMenuWindow()
        {
            //TODO Get bool for current run
            bool hasRun = PlayerDataManager.GethasRunStarted();
            
            newRunButton.gameObject.SetActive(!hasRun);
            continueRunButton.gameObject.SetActive(hasRun);
            abandonRunButton.gameObject.SetActive(hasRun);
            
        }
        
        //Setup Run Window
        //------------------------------------------------------------------------------------------------------------//

        private void SetupRunMenuWindow()
        {
            hardcoreRunButton.interactable = false;
            
            startRunButton.interactable = false;

            ShowRunData(GAME_TYPE.NONE);
        }

        private void ShowRunData(GAME_TYPE gameType)
        {
            _selectedGameType = gameType;
            switch (gameType)
            {
                case GAME_TYPE.NONE:
                    runDescriptionText.text = string.Empty;
                    break;
                case GAME_TYPE.CLASSIC:
                    runDescriptionText.text = "Classic Game mode";
                    break;
                case GAME_TYPE.HARDCORE:
                    runDescriptionText.text = "Hardcore";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType), gameType, null);
            }
        }
        //------------------------------------------------------------------------------------------------------------//


        #endregion //Setup Windows

        //====================================================================================================================//
        
        #region Buttons

        private void SetupButtons()
        {
            SetupMainMenuButtons();
            SetupAccountButtons();
            SetupAccountMenuButtons();
            SetupRunMenuButtons();
            SetupSettingsButtons();
        }

        //Setup Main Menu Buttons
        //------------------------------------------------------------------------------------------------------------//
        
        private void SetupMainMenuButtons()
        {
            playButton.onClick.AddListener(() =>
            {
                OpenWindow(WINDOW.ACCOUNT);
            });
            settingsButton.onClick.AddListener(() =>
            {
                OpenWindow(WINDOW.SETTINGS);
            });
            quitButton.onClick.AddListener(Quit);
        }

        //Setup Account Buttons
        //------------------------------------------------------------------------------------------------------------//
        
        private void SetupAccountButtons()
        {
            accountBackButton.onClick.AddListener(CloseOpenWindow);

            //TODO These need to be further fleshed out
            foreach (var accountButton in accountButtons)
            {
                accountButton.onClick.AddListener(() =>
                {
                    //TODO Load account data
                    OpenWindow(WINDOW.ACCOUNT_MENU);
                });
            }

            for (var i = 0; i < deleteAccountButtons.Length; i++)
            {
                int index = i;
                deleteAccountButtons[i].onClick.AddListener(() =>
                {
                    Alert.ShowAlert($"Delete Account {index}",
                        "Are you sure you want to delete the account? Data will not be able to be recovered.",
                        "Delete",
                        "Cancel",
                        response =>
                        {
                            if (!response)
                                return;
                            
                            PlayerDataManager.DestroyAccountData();
                            PlayerDataManager.RemoveSaveFileData(index);
                            Files.DestroyPlayerSaveFile(index);
                            SetupAccountWindow();
                        });
                });
            }
        }

        //Setup Account Menu Buttons
        //------------------------------------------------------------------------------------------------------------//
        
        private void SetupAccountMenuButtons()
        {
            changeAccountButton.onClick.AddListener(() =>
            {
                OpenWindow(WINDOW.ACCOUNT);
            });
            
            accountMenuSettingsButton.onClick.AddListener(() =>
            {
                OpenWindow(WINDOW.SETTINGS);
            });
            accountMenuQuitButton.onClick.AddListener(Quit);
            newRunButton.onClick.AddListener(() =>
            {
                OpenWindow(WINDOW.RUN);
            });
            continueRunButton.onClick.AddListener(() =>
            {

                //TODO Need to load existing account run here
                PlayerDataManager.SetRunStarted();
                LeaveMenu(SceneLoader.SCRAPYARD);
            });
            abandonRunButton.onClick.AddListener(() =>
            {
                Alert.ShowAlert("Abandon Run",
                    "Are you sure you want to abandon your run, starting a new one?",
                    "Abandon",
                    "Cancel",
                    response =>
                    {
                        if (!response)
                            return;
                        
                        PlayerDataManager.ResetPlayerRunData();
                        PlayerDataManager.SavePlayerAccountData();
                        SetupAccountMenuWindow();
                    });
            });
            tutorialButton.onClick.AddListener(() =>
            {
                Globals.UsingTutorial = true;
                Globals.CurrentSector = 4;
                Globals.CurrentWave = 0;
                
                LeaveMenu(SceneLoader.LEVEL);
            });
        }

        //Setup Run Buttons
        //------------------------------------------------------------------------------------------------------------//
        
        private void SetupRunMenuButtons()
        {
            classicRunButton.onClick.AddListener(() =>
            {
                ShowRunData(GAME_TYPE.CLASSIC);
                startRunButton.interactable = true;
            });
            hardcoreRunButton.onClick.AddListener(() =>
            {
                /*ShowRunData(GAME_TYPE.HARDCORE);
                startRunButton.interactable = true;*/
                throw new NotImplementedException();
            });
            startRunButton.onClick.AddListener(() =>
            {
                switch (_selectedGameType)
                {
                    case GAME_TYPE.CLASSIC:
                        
                        PlayerDataManager.SetRunStarted();
                        OpenWindow(WINDOW.ACCOUNT_MENU);
                        IntroScene.gameObject.SetActive(true);
                        gameObject.SetActive(false);
                        break;
                    case GAME_TYPE.HARDCORE:
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_selectedGameType), _selectedGameType, null);
                }

            });

            runBackButton.onClick.AddListener(() =>
            {
                ShowRunData(GAME_TYPE.NONE);
                CloseOpenWindow();
            });

        }

        //Setup Settings Buttons
        //------------------------------------------------------------------------------------------------------------//
        
        private void SetupSettingsButtons()
        {
            musicVolumeSlider.onValueChanged.AddListener(AudioController.SetMusicVolume);
            sfxVolumeSlider.onValueChanged.AddListener(AudioController.SetSFXVolume);
            testingFeaturesToggle.onValueChanged.AddListener(toggle =>
            {
                Globals.DisableTestingFeatures = toggle;
            });
            
            settingsBackButton.onClick.AddListener(CloseOpenWindow);
        }

        #endregion //Buttons

        //====================================================================================================================//
        
        #region Windows

        private void SetupWindows()
        {
            _windowData = new[]
            {
                new WindowData
                {
                    Type = WINDOW.MAIN_MENU, WindowObject = mainMenuWindowObject, CloseOtherWindows = true
                }, //MAIN_MENU
                new WindowData
                    {Type = WINDOW.ACCOUNT, WindowObject = accountWindowObject, CloseOtherWindows = false }, //ACCOUNT
                new WindowData
                {
                    Type = WINDOW.ACCOUNT_MENU, WindowObject = accountMenuWindowObject, CloseOtherWindows = true
                }, //ACCOUNT_MENU
                new WindowData { Type = WINDOW.RUN, WindowObject = pickRunWindowObject, CloseOtherWindows = false }, //RUN
                new WindowData
                    {Type = WINDOW.SETTINGS, WindowObject = settingsWindowObject, CloseOtherWindows = false}, //SETTINGS
            };

            OpenWindow(WINDOW.MAIN_MENU);
        }

        private void OpenWindow(WINDOW openWindow)
        {
            
            RefreshWindow(openWindow);

            var windowData = _windowData[(int) openWindow];

            if (windowData.CloseOtherWindows)
            {
                foreach (var window in _windowData)
                {
                    window.SetActive(window.Type == openWindow);
                }
            }
            else
                windowData.SetActive(true);

            _previousWindow = _currentWindow;
            _currentWindow = openWindow;
        }

        private void CloseOpenWindow()
        {
            CloseWindow(_currentWindow);
        }

        private void CloseWindow(WINDOW openWindow)
        {
            var windowData = _windowData[(int) openWindow];
            
            if(windowData.CloseOtherWindows)
                throw new ArgumentException("Only windows that overlay can close");
            
            windowData.SetActive(false);
            OpenWindow(_previousWindow);
        }

        #endregion //Windows

        //====================================================================================================================//

        private void LeaveMenu(string targetScene)
        {
            SetupAccountMenuWindow();
            OpenWindow(WINDOW.ACCOUNT_MENU);
            SceneLoader.ActivateScene(targetScene, SceneLoader.MAIN_MENU);
        }

        private static void Quit()
        {
            Alert.ShowAlert("Quit Game",
                $"Are you sure you want to quit {Application.productName}?",
                "Quit",
                "Cancel",
                response =>
                {
                    if(!response)
                        return;
                    
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                });
        }

        //====================================================================================================================//
        
    }
}
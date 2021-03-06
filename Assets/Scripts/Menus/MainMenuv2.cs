﻿using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Factories;
using StarSalvager.PersistentUpgrades.Data;
using StarSalvager.Prototype;
using StarSalvager.UI.PersistentUpgrades;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Analytics.SessionTracking;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Interfaces;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class MainMenuv2 : MonoBehaviour, IReset, IBuildNavigationProfile
    {
        //Enums
        //====================================================================================================================//

        private enum WINDOW
        {
            NONE = -1,
            MAIN_MENU,
            ACCOUNT,
            ACCOUNT_MENU,
            RUN,
            SETTINGS,
            STARS
        }

        private enum GAME_TYPE
        {
            NONE,
            CLASSIC,
            HARDCORE
        }

        //Structs
        //====================================================================================================================//

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

        //Properties
        //====================================================================================================================//

        #region Properties

        //Intro Scene Properties
        //====================================================================================================================//
        [SerializeField, Required] private IntroScene IntroScene;

        /*[SerializeField, Required]
        private SpriteRenderer partSprite;*/

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

        [FormerlySerializedAs("backButton")] [SerializeField, Required, FoldoutGroup("Account Window")]
        private Button accountBackButton;

        //TODO This will likely need to be something beyond a normal Button
        [SerializeField, Required, FoldoutGroup("Account Window")]
        private Button[] accountButtons;

        [SerializeField, Required, FoldoutGroup("Account Window")]
        private RectTransform[] accountBotPreviewContainers;

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

        [SerializeField, Required, FoldoutGroup("Account Menu Window")]
        private Button starsButton;

        //Stars Window
        //====================================================================================================================//
        [SerializeField, Required, FoldoutGroup("Stars Window")]
        private GameObject starsMenuWindow;

        [SerializeField, Required, FoldoutGroup("Stars Window")]
        private Image starsButtonGlow;

        [SerializeField, Required, FoldoutGroup("Stars Window")]
        private Button starsBackButton;

        [SerializeField, Required, FoldoutGroup("Stars Window")]
        private PersistentUpgradesUI persistentUpgradesUI;

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
        private int _selectedAccountIndex = -1;

        //====================================================================================================================//

        private WINDOW _currentWindow = WINDOW.NONE;
        private WINDOW _previousWindow = WINDOW.NONE;
        private WindowData[] _windowData;


        private bool _showingHint;
        private void IsShowingHint(bool showingHint) => _showingHint = showingHint;

        #endregion //Properties

        //Unity Functions
        //====================================================================================================================//

        #region Unity Functions

        private void OnEnable()
        {
            InputManager.OnCancelPressed += OnCancelPressed;
            Hints.HintManager.OnShowingHintAction += IsShowingHint;
        }

        private void Start()
        {
            Globals.Init();
            SetupWindows();
            SetupButtons();

            OpenWindow(WINDOW.MAIN_MENU);
        }

        private void OnDisable()
        {
            InputManager.OnCancelPressed -= OnCancelPressed;
            Hints.HintManager.OnShowingHintAction -= IsShowingHint;
        }

        #endregion //Unity Functions

        //IReset Functions
        //====================================================================================================================//

        public void Activate()
        {
            Globals.ScaleCamera(Globals.CameraScaleSize);
            musicVolumeSlider.value = PlayerPrefs.GetFloat(AudioController.MUSIC_VOLUME, 1f);
            sfxVolumeSlider.value = PlayerPrefs.GetFloat(AudioController.SFX_VOLUME, 1f);

            RefreshWindow(_currentWindow);

            UISelectHandler.SetBuildTarget(this);

            if (!PlayerDataManager.ShouldShownSummary()) return;

            FindObjectOfType<PostGameUI>().ShowPostGameUI();

            UISelectHandler.RebuildNavigationProfile();
        }

        public void Reset()
        {
        }

        //MainMenuV2 Functions
        //====================================================================================================================//

        private void RefreshWindow(WINDOW window)
        {
            switch (window)
            {
                case WINDOW.NONE:
                    break;
                case WINDOW.MAIN_MENU:
                case WINDOW.SETTINGS:
                    break;
                case WINDOW.ACCOUNT:
                    if (CheckVersionConflict())
                    {
                        Alert.ShowAlert("Version Conflict",
                            "This version is newer then your save file version. Save files will need to be deleted.",
                            "Ok", () =>
                            {
                                Files.ClearRemoteData();
                                RefreshWindow(_currentWindow);
                                UISelectHandler.SetBuildTarget(this);
                            });
                    }
                    else
                    {
                        SetupAccountWindow();
                    }

                    break;
                case WINDOW.ACCOUNT_MENU:
                    SetupAccountMenuWindow();
                    break;
                case WINDOW.STARS:
                    SetupStarsWindow();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(window), window, null);
            }
        }

        #region Setup Windows

        //Setup Account Window
        //------------------------------------------------------------------------------------------------------------//

        private bool CheckVersionConflict()
        {
            for (var i = 0; i < accountButtons.Length; i++)
            {
                var hasAccount = Files.TryGetPlayerSaveData(i, out var accountData);

                if (hasAccount)
                {
                    if (accountData.Version != Constants.VERSION)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void SetupAccountWindow()
        {
            for (var i = 0; i < accountButtons.Length; i++)
            {
                int index = i;
                var interactable = i != _selectedAccountIndex;
                accountButtons[i].onClick.RemoveAllListeners();
                accountButtons[i].onClick.AddListener(() =>
                {
                    _selectedAccountIndex = index;
                    PlayerDataManager.SetCurrentSaveSlotIndex(index);
                    PlayerDataManager.ValidateData();
                    SetupAccountMenuWindow();
                    OpenWindow(WINDOW.ACCOUNT_MENU);
                    GameManager.SetCurrentGameState(GameState.AccountMenu);
                });

                var hasAccount = Files.TryGetPlayerSaveData(i, out var accountData);
                var buttonText = accountButtons[i].GetComponentInChildren<TMP_Text>();

                deleteAccountButtons[i].gameObject.SetActive(hasAccount);

                buttonText.text = !hasAccount
                    ? "Create new Account"
                    : $"{(interactable ? "" : "Current\n")}Load Account {i + 1}\nTotal Runs: {accountData.TotalRuns}";

                //If there's no account, pass null so the function knows to clean it
                List<IBlockData> blockDatas =
                    hasAccount && accountData.HasRun ? accountData.PlayerRunData.DroneBlockData : null;
                blockDatas.CreateBotPreview(accountBotPreviewContainers[i]);

                //Check to see if the currently opened account is this button, disable if yes
                accountButtons[i].interactable = interactable;
            }





        }

        //Setup Account Menu Window
        //------------------------------------------------------------------------------------------------------------//

        private void SetupAccountMenuWindow()
        {
            //--------------------------------------------------------------------------------------------------------//
            bool HasStarsToSpend()
            {
                bool CanPurchase(UpgradeData data)
                {
                    bool IsUnlocked()
                    {
                        var currentLevel = PlayerDataManager.GetCurrentUpgradeLevel(data.Type, data.BitType);

                        return data.Level <= currentLevel + 1;
                    }

                    bool HasPurchased()
                    {
                        var currentLevel = PlayerDataManager.GetCurrentUpgradeLevel(data.Type, data.BitType);

                        return data.Level <= currentLevel;
                    }

                    int GetCost() => FactoryManager.Instance.PersistentUpgrades.GetRemoteData(data.Type, data.BitType)
                        .Levels[data.Level].cost;

                    bool CanAfford(in int stars) => PlayerDataManager.GetStars() >= stars;

                    var cost = GetCost();
                    var hasPurchased = HasPurchased();
                    var isUnlocked = IsUnlocked();
                    var canAfford = CanAfford(cost);

                    return isUnlocked && !hasPurchased && canAfford;
                }



                var upgrades = FactoryManager.Instance.PersistentUpgrades.Upgrades;

                foreach (var upgradeRemoteData in upgrades)
                {
                    for (var i = 1; i < upgradeRemoteData.Levels.Count; i++)
                    {
                        var data = new UpgradeData(upgradeRemoteData.upgradeType, upgradeRemoteData.bitType, i);

                        if (CanPurchase(data))
                            return true;
                    }
                }

                return false;
            }

            //--------------------------------------------------------------------------------------------------------//

            //TODO Get bool for current run
            bool hasActiveRun = PlayerDataManager.HasActiveRun();

            newRunButton.gameObject.SetActive(!hasActiveRun);
            continueRunButton.gameObject.SetActive(hasActiveRun);
            abandonRunButton.gameObject.SetActive(hasActiveRun);

            starsButton.gameObject.SetActive(true);
            settingsButton.gameObject.SetActive(true);
            quitButton.gameObject.SetActive(true);
            changeAccountButton.gameObject.SetActive(true);



            starsButtonGlow.gameObject.SetActive(HasStarsToSpend() && !hasActiveRun);
            starsButton.interactable = !hasActiveRun;

        }

        //Setup Stars Window
        //------------------------------------------------------------------------------------------------------------//

        private void SetupStarsWindow()
        {
            persistentUpgradesUI.SetupPersistentUpgradesUI();
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
            SetupStarsButtons();
            SetupSettingsButtons();
        }

        //Setup Main Menu Buttons
        //------------------------------------------------------------------------------------------------------------//

        private void SetupMainMenuButtons()
        {
            playButton.onClick.AddListener(() => { OpenWindow(WINDOW.ACCOUNT); });
            settingsButton.onClick.AddListener(() => { OpenWindow(WINDOW.SETTINGS); });
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
                            {
                                UISelectHandler.SetBuildTarget(this);
                                return;
                            }

                            if (_selectedAccountIndex == index)
                            {
                                PlayerDataManager.DestroyAccountData();

                                UISelectHandler.SetBuildTarget(this);
                                OpenWindow(WINDOW.MAIN_MENU);
                                _selectedAccountIndex = -1;
                            }
                            else
                            {
                                UISelectHandler.SetBuildTarget(this);
                            }

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
            changeAccountButton.onClick.AddListener(() => { OpenWindow(WINDOW.ACCOUNT); });

            accountMenuSettingsButton.onClick.AddListener(() => { OpenWindow(WINDOW.SETTINGS); });
            accountMenuQuitButton.onClick.AddListener(Quit);
            newRunButton.onClick.AddListener(() =>
            {
                //OpenWindow(WINDOW.RUN);
                StartSelectedGameType(GAME_TYPE.CLASSIC);
            });
            continueRunButton.onClick.AddListener(() =>
            {
                AudioController.CrossFadeTrack(MUSIC.SCRAPYARD);
                //TODO Need to load existing account run here
                PlayerDataManager.SetRunStarted(true);
                LeaveMenu(SceneLoader.UNIVERSE_MAP, null);
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
                        {
                            UISelectHandler.SetBuildTarget(this);
                            return;
                        }

                        PlayerDataManager.CompleteCurrentRun();
                        PlayerDataManager.SavePlayerAccountData();
                        AnalyticsManager.AbandonRunEvent();
                        SetupAccountMenuWindow();
                        
                        if (!PlayerDataManager.ShouldShownSummary())
                        {
                            UISelectHandler.SetBuildTarget(this);
                            return;
                        }

                        FindObjectOfType<PostGameUI>().ShowPostGameUI();
                    });
            });
            tutorialButton.interactable = false;
            /*tutorialButton.onClick.AddListener(() =>
            {
                Globals.UsingTutorial = true;
                Globals.CurrentSector = FactoryManager.Instance.SectorRemoteData.Count - 1;
                Globals.CurrentWave = 0;
                
                LeaveMenu(SceneLoader.LEVEL);
            });*/

            starsButton.onClick.AddListener(() => { OpenWindow(WINDOW.STARS); });
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
            startRunButton.onClick.AddListener(() => { StartSelectedGameType(_selectedGameType); });

            runBackButton.onClick.AddListener(() =>
            {
                ShowRunData(GAME_TYPE.NONE);
                CloseOpenWindow();
            });

        }

        private void StartSelectedGameType(GAME_TYPE gameType)
        {
            switch (gameType)
            {
                case GAME_TYPE.CLASSIC:

                    AnalyticsManager.StartNewRunEvent();
                    PlayerDataManager.StartNewPlayerRun();
                    SessionDataProcessor.Instance.StartNewWreck(Vector2Int.zero);
                    /*var startingHealth = PART_TYPE.CORE.GetRemoteData().GetDataValue<float>(PartProperties.KEYS.Health);
                    
                    PlayerDataManager.SetGears((int)PlayerDataManager.GetCurrentUpgradeValue(UPGRADE_TYPE.STARTING_CURRENCY));
                    
                    PlayerDataManager.SetRunStarted(true);
                    PlayerDataManager.SetBotHealth(startingHealth);*/

                    //Set the ACCOUNT_MENU to open, so that when we return its ready to view
                    OpenWindow(WINDOW.ACCOUNT_MENU);
                    IntroScene.Init();
                    gameObject.SetActive(false);
                    break;
                case GAME_TYPE.HARDCORE:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType), gameType, null);
            }
        }


        //Setup Stars Buttons
        //------------------------------------------------------------------------------------------------------------//

        private void SetupStarsButtons()
        {
            starsBackButton.onClick.AddListener(CloseOpenWindow);
        }
        //Setup Settings Buttons
        //------------------------------------------------------------------------------------------------------------//

        private void SetupSettingsButtons()
        {
            musicVolumeSlider.onValueChanged.AddListener(AudioController.SetMusicVolume);
            sfxVolumeSlider.onValueChanged.AddListener(AudioController.SetSFXVolume);
            testingFeaturesToggle.onValueChanged.AddListener(toggle => { Globals.TestingFeatures = toggle; });

            settingsBackButton.onClick.AddListener(CloseOpenWindow);
        }

        #endregion //Buttons

        private void OnCancelPressed()
        {
            if (_showingHint)
                return;

            switch (_currentWindow)
            {
                case WINDOW.ACCOUNT:
                case WINDOW.RUN:
                case WINDOW.SETTINGS:
                case WINDOW.STARS:
                    CloseOpenWindow();
                    break;
                default:
                    return;
            }
        }

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
                    {Type = WINDOW.ACCOUNT, WindowObject = accountWindowObject, CloseOtherWindows = false}, //ACCOUNT
                new WindowData
                {
                    Type = WINDOW.ACCOUNT_MENU, WindowObject = accountMenuWindowObject, CloseOtherWindows = true
                }, //ACCOUNT_MENU
                new WindowData {Type = WINDOW.RUN, WindowObject = pickRunWindowObject, CloseOtherWindows = false}, //RUN
                new WindowData
                    {Type = WINDOW.SETTINGS, WindowObject = settingsWindowObject, CloseOtherWindows = false},
                new WindowData
                    {Type = WINDOW.STARS, WindowObject = starsMenuWindow, CloseOtherWindows = false}, //SETTINGS
            };


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

            UISelectHandler.RebuildNavigationProfile();
        }

        private void CloseOpenWindow()
        {
            if (_showingHint)
                return;

            CloseWindow(_currentWindow);
        }

        private void CloseWindow(WINDOW openWindow)
        {
            var windowData = _windowData[(int) openWindow];

            if (windowData.CloseOtherWindows)
                throw new ArgumentException("Only windows that overlay can close");

            windowData.SetActive(false);
            OpenWindow(_previousWindow);
        }

        #endregion //Windows

        //====================================================================================================================//

        private void LeaveMenu(string targetScene, Action onFadedCallback)
        {
            SetupAccountMenuWindow();
            OpenWindow(WINDOW.ACCOUNT_MENU);

            ScreenFade.Fade(() =>
            {
                SceneLoader.ActivateScene(targetScene, SceneLoader.MAIN_MENU);
                onFadedCallback?.Invoke();
            });
        }

        private void Quit()
        {
            Alert.ShowAlert("Quit Game",
                $"Are you sure you want to quit {Application.productName}?",
                "Quit",
                "Cancel",
                response =>
                {
                    if (!response)
                    {
                        UISelectHandler.SetBuildTarget(this);
                        return;
                    }

#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                });
        }

        //IBuildNavigationProfile Functions
        //====================================================================================================================//

        public NavigationProfile BuildNavigationProfile()
        {
            switch (_currentWindow)
            {
                case WINDOW.MAIN_MENU:
                    return new NavigationProfile(
                        playButton,
                        new Selectable[]
                        {
                            playButton,
                            settingsButton,
                            quitButton
                        }, null, null);
                case WINDOW.ACCOUNT:
                    var exceptions = new List<NavigationRestriction>
                    {
                        new NavigationRestriction
                        {
                            FromDirection =
                                NavigationRestriction.DIRECTION.RIGHT | NavigationRestriction.DIRECTION.LEFT,
                            Selectable = accountBackButton
                        }
                    };
                    foreach (var deleteAccountButton in deleteAccountButtons)
                    {
                        exceptions.Add(new NavigationRestriction
                        {
                            FromDirection = NavigationRestriction.DIRECTION.UP,
                            Selectable = deleteAccountButton
                        });
                    }

                    //Setup navigation, disallowing the use of back button as a left or right destination
                    return new NavigationProfile(
                        accountButtons[0],
                        accountWindowObject.GetComponentsInChildren<Selectable>(),
                        null, exceptions);
                case WINDOW.ACCOUNT_MENU:
                    return new NavigationProfile(
                        PlayerDataManager.HasActiveRun() ? continueRunButton : newRunButton,
                        new Selectable[]
                        {
                            newRunButton,
                            continueRunButton,
                            abandonRunButton,
                            starsButton,
                            accountMenuSettingsButton,
                            accountMenuQuitButton,
                            changeAccountButton,
                        }, new []
                        {
                            new NavigationOverride
                            {
                                FromSelectable = changeAccountButton,
                                DownTarget = PlayerDataManager.HasActiveRun() ? continueRunButton : newRunButton,
                            }
                        }, null);
                case WINDOW.RUN:
                    break;
                case WINDOW.SETTINGS:
                    return new NavigationProfile(
                        settingsBackButton,
                        new Selectable[]
                        {
                            musicVolumeSlider,
                            sfxVolumeSlider,
                            testingFeaturesToggle,
                            settingsBackButton
                        }, null, null);
                case WINDOW.STARS:
                    var upgrades = persistentUpgradesUI as IBuildNavigationProfile;
                    
                    return upgrades
                        .BuildNavigationProfile();
            }

            throw new ArgumentOutOfRangeException();
        }

        //====================================================================================================================//

    }
}

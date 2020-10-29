using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Utilities.Saving;
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
            public WINDOW type;
            public GameObject windowObject;
            public bool closeOtherWindows;

            public void SetActive(bool active)
            {
                windowObject.gameObject.SetActive(active);
            }
        }
        
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

        //Account Menu Window Properties
        //====================================================================================================================//
        [SerializeField, Required, FoldoutGroup("Account Menu Window")]
        private GameObject accountMenuWindowObject;
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
        
        private WINDOW _currentWindow = WINDOW.MAIN_MENU;
        private WINDOW _previousWindow = WINDOW.MAIN_MENU;
        private WindowData[] _windowData;

        //Unity Functions
        //====================================================================================================================//

        private void Start()
        {
            SetupWindows();
            SetupButtons();
        }

        //MainMenuV2 Functions
        //====================================================================================================================//

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
                
                var buttonText = accountButtons[i].GetComponentInChildren<TMP_Text>();
                if (!Files.TryGetPlayerSaveData(i, out var accountData))
                {
                    buttonText.text = "Create new Account";
                    continue;
                }

                buttonText.text = $"Load Account {i + 1}\nTotal Runs: {accountData.TotalRuns}";
            }
        }

        private void SetupAccountMenuWindow()
        {
            //TODO Get bool for current run
            bool hasRun = false;
            
            newRunButton.gameObject.SetActive(!hasRun);
            continueRunButton.gameObject.SetActive(hasRun);
            abandonRunButton.gameObject.SetActive(hasRun);
            
        }

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
                    runDescriptionText.text = "Classic";
                    break;
                case GAME_TYPE.HARDCORE:
                    runDescriptionText.text = "Hardcore";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType), gameType, null);
            }
        }

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
        
        private void SetupMainMenuButtons()
        {
            playButton.onClick.AddListener(() =>
            {
                SetupAccountWindow();
                OpenWindow(WINDOW.ACCOUNT);
            });
            settingsButton.onClick.AddListener(() =>
            {
                OpenWindow(WINDOW.SETTINGS);
            });
            quitButton.onClick.AddListener(Quit);
        }

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
        }

        private void SetupAccountMenuButtons()
        {
            accountMenuSettingsButton.onClick.AddListener(() =>
            {
                OpenWindow(WINDOW.SETTINGS);
            });
            accountMenuQuitButton.onClick.AddListener(Quit);
            newRunButton.onClick.AddListener(() =>
            {
                SetupRunMenuWindow();
                OpenWindow(WINDOW.RUN);
            });
            continueRunButton.onClick.AddListener(() =>
            {
                //TODO Need to load existing account run here
                throw new NotImplementedException();
            });
            abandonRunButton.onClick.AddListener(() =>
            {
                //TODO Need to confirm destruction of active run
                throw new NotImplementedException();
            });
        }

        private void SetupRunMenuButtons()
        {
            classicRunButton.onClick.AddListener(() =>
            {
                ShowRunData(GAME_TYPE.CLASSIC);
                startRunButton.interactable = true;
            });
            hardcoreRunButton.onClick.AddListener(() =>
            {
                ShowRunData(GAME_TYPE.HARDCORE);
                startRunButton.interactable = true;
            });
            startRunButton.onClick.AddListener(() =>
            {
                //TODO Use _selectedGameType to determine how to start the run
                throw new NotImplementedException();
            });
            runBackButton.onClick.AddListener(CloseOpenWindow);
            
        }
        
        private void SetupSettingsButtons()
        {
            musicVolumeSlider.onValueChanged.AddListener(volume => { });
            sfxVolumeSlider.onValueChanged.AddListener(volume => { });
            testingFeaturesToggle.onValueChanged.AddListener(toggle => { });
            
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
                    type = WINDOW.MAIN_MENU, windowObject = mainMenuWindowObject, closeOtherWindows = true
                }, //MAIN_MENU
                new WindowData
                    {type = WINDOW.ACCOUNT, windowObject = accountWindowObject, closeOtherWindows = false }, //ACCOUNT
                new WindowData
                {
                    type = WINDOW.ACCOUNT_MENU, windowObject = accountMenuWindowObject, closeOtherWindows = true
                }, //ACCOUNT_MENU
                new WindowData { type = WINDOW.RUN, windowObject = pickRunWindowObject, closeOtherWindows = false }, //RUN
                new WindowData
                    {type = WINDOW.SETTINGS, windowObject = settingsWindowObject, closeOtherWindows = false}, //SETTINGS
            };

            OpenWindow(_currentWindow);
        }

        private void OpenWindow(WINDOW openWindow)
        {
            
            var windowData = _windowData[(int) openWindow];

            if (windowData.closeOtherWindows)
            {
                foreach (var window in _windowData)
                {
                    window.SetActive(window.type == openWindow);
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
            
            if(windowData.closeOtherWindows)
                throw new ArgumentException("Only windows that overlay can close");
            
            windowData.SetActive(false);
            OpenWindow(_previousWindow);
        }

        #endregion //Windows

        //====================================================================================================================//

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
    }
}

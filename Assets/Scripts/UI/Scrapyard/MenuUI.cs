using System;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Wreckyard
{
    public class MenuUI : MonoBehaviour
    {
        public static Action<bool> OnMenuOpened;
        //Properties
        //====================================================================================================================//
        
        [SerializeField, Required]
        private Button menuButton;

        //====================================================================================================================//
        
        [SerializeField, Required, FoldoutGroup("Menu Window")]
        private GameObject menuWindow;
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

        //Unity Functions
        //====================================================================================================================//

        private void OnEnable()
        {
            InputManager.OnCancelPressed += OnCancelPressed;
            InputManager.OnPausePressed += OnPausePressed;
        }

        private void Start()
        {
            SetMenuActive(false);
            SetSettingsMenuActive(false);

            //Menu Buttons
            //--------------------------------------------------------------------------------------------------------//

            menuButton.onClick.AddListener(OpenMenu);

            resumeGameButton.onClick.AddListener(OnResumePressed);

            settingsButton.onClick.AddListener(() =>
            {
                SetSettingsMenuActive(true);
                UISelectHandler.SetupNavigation(settingsBackButton,
                    new Selectable[]
                    {
                        musicVolumeSlider,
                        sfxVolumeSlider,
                        testingFeaturesToggle,
                        settingsBackButton,
                    },
                    overrides: new[]
                    {
                        new NavigationOverride {FromSelectable = settingsBackButton, UpTarget = testingFeaturesToggle},
                        new NavigationOverride {FromSelectable = testingFeaturesToggle, DownTarget = settingsBackButton}
                    });
            });

            quitGameButton.onClick.AddListener(QuitPressed);

            //Settings Menu Buttons
            //--------------------------------------------------------------------------------------------------------//

            musicVolumeSlider.value = PlayerPrefs.GetFloat(AudioController.MUSIC_VOLUME, 1f);
            sfxVolumeSlider.value = PlayerPrefs.GetFloat(AudioController.SFX_VOLUME, 1f);

            musicVolumeSlider.onValueChanged.AddListener(AudioController.SetMusicVolume);
            sfxVolumeSlider.onValueChanged.AddListener(AudioController.SetSFXVolume);

            testingFeaturesToggle.onValueChanged.AddListener(toggle => { Globals.TestingFeatures = toggle; });

            settingsBackButton.onClick.AddListener(OnSettingsBackPressed);
        }

        private void OnDisable()
        {
            InputManager.OnCancelPressed -= OnCancelPressed;
            InputManager.OnPausePressed -= OnPausePressed;
        }

        //====================================================================================================================//

        public void OpenMenu()
        {
            SetMenuActive(true);
            
            UISelectHandler.SetupNavigation(resumeGameButton,
                new []
                {
                    resumeGameButton,
                    settingsButton,
                    quitGameButton
                });
        }
        
        //====================================================================================================================//
        
        private void SetMenuActive(in bool state)
        {
            menuWindow.SetActive(state);
            OnMenuOpened?.Invoke(state);
        }
        private void SetSettingsMenuActive(in bool state)
        {
            settingsWindowObject.SetActive(state);
        }

        //====================================================================================================================//

        private void OnResumePressed()
        {
            SetMenuActive(false);
            SetSettingsMenuActive(false);
        }

        private void OnSettingsBackPressed()
        {
            SetSettingsMenuActive(false);
            OpenMenu();
        }
        private void QuitPressed()
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
                            SetMenuActive(false);
                            SetSettingsMenuActive(false);
                            
                            //_windows[(int)Window.Settings].SetActive(false);
                            SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.WRECKYARD, MUSIC.MAIN_MENU);
                            AnalyticsManager.WreckEndEvent(AnalyticsManager.REASON.QUIT);
                            FindObjectOfType<MainMenuv2>().RefreshCurrentWindow();
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
        }

        //====================================================================================================================//

        private void OnCancelPressed()
        {
            if (settingsWindowObject.activeInHierarchy)
            {
                OnSettingsBackPressed();
                return;
            }

            OnPausePressed();
        }
        private void OnPausePressed()
        {
            if (!menuWindow.activeInHierarchy)
                return;
            
            OnResumePressed();
        }
    }
}

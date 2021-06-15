using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class MenuUI : MonoBehaviour
    {
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
        
        private void Start()
        {
            SetMenuActive(false);
            SetSettingsMenuActive(false);
            
            //Menu Buttons
            //--------------------------------------------------------------------------------------------------------//
            
            menuButton.onClick.AddListener(() =>
            {
                SetMenuActive(true);
                
            });
            
            resumeGameButton.onClick.AddListener(() =>
            {
                SetMenuActive(false);
                SetSettingsMenuActive(false);
            });

            settingsButton.onClick.AddListener(() =>
            {
                SetSettingsMenuActive(true);
            });

            quitGameButton.onClick.AddListener(QuitPressed);
            
            //Settings Menu Buttons
            //--------------------------------------------------------------------------------------------------------//
            
            musicVolumeSlider.value = PlayerPrefs.GetFloat(AudioController.MUSIC_VOLUME, 1f);
            sfxVolumeSlider.value = PlayerPrefs.GetFloat(AudioController.SFX_VOLUME, 1f);
            
            musicVolumeSlider.onValueChanged.AddListener(AudioController.SetMusicVolume);
            sfxVolumeSlider.onValueChanged.AddListener(AudioController.SetSFXVolume);

            testingFeaturesToggle.onValueChanged.AddListener(toggle =>
            {
                Globals.TestingFeatures = toggle;
            });

            settingsBackButton.onClick.AddListener(() =>
            {
                SetSettingsMenuActive(false);
            });
        }

        //====================================================================================================================//

        private void SetMenuActive(in bool state)
        {
            menuWindow.SetActive(state);
        }
        private void SetSettingsMenuActive(in bool state)
        {
            settingsWindowObject.SetActive(state);
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
                            SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.SCRAPYARD, MUSIC.MAIN_MENU);
                            AnalyticsManager.WreckEndEvent(AnalyticsManager.REASON.QUIT);
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
    }
}

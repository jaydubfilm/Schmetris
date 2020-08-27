﻿using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class LevelManagerUI : MonoBehaviour, IPausable
    {
        public bool isPaused => GameTimer.IsPaused;
        
        [SerializeField, Required]
        private TMP_Text deathText;
        [SerializeField, Required]
        private TMP_Text livesText;

        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("UISections")]
        private GameObject m_betweenWavesUI;
        [SerializeField, Required, FoldoutGroup("UISections")]
        private GameObject m_deathUI;

        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Between Waves")]
        private Button betweenWavesContinueButton;
        [SerializeField, Required, FoldoutGroup("Between Waves")]
        private Button betweenWavesScrapyardButton;

        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Death Window")]
        private Button deathWindowRetryButton;
        [SerializeField, Required, FoldoutGroup("Death Window")]
        private Button deathWindowScrapyrdButton;

        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Pause Menu")]
        private GameObject pauseWindow;
        [SerializeField, Required, FoldoutGroup("Pause Menu")]
        private Button pauseWindowScrapyardButton;
        [SerializeField, Required, FoldoutGroup("Pause Menu")]
        private Button pauseWindowMainMenuButton;
        [SerializeField, Required, FoldoutGroup("Pause Menu")]
        private Button resumeButton;
        [SerializeField, Required, FoldoutGroup("Pause Menu")]
        private TMP_Text pauseText;

        //============================================================================================================//

        private LevelManager m_levelManager;

        // Start is called before the first frame update
        private void Start()
        {
            RegisterPausable();
            m_levelManager = FindObjectOfType<LevelManager>();
            InitButtons();
        }

        private void Update()
        {
            //betweenWavesContinueButton.interactable = PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE] > 0;

            pauseWindowScrapyardButton.gameObject.SetActive(!FactoryManager.Instance.DisableTestingFeatures);
        }

        //============================================================================================================//

        private void InitButtons()
        {
            betweenWavesContinueButton.onClick.AddListener(() =>
            {
                GameTimer.SetPaused(false);
                ToggleBetweenWavesUIActive(false);

                m_levelManager.ContinueToNextWave();
            });

            betweenWavesScrapyardButton.onClick.AddListener(() =>
            {
                m_levelManager.IsWaveProgressing = true;
                m_levelManager.ProcessScrapyardUsageBeginAnalytics();
                ToggleBetweenWavesUIActive(false);
                LevelManager.Instance.EndWaveState = false;
                SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.ALEX_TEST_SCENE);
            });

            pauseWindowScrapyardButton.onClick.AddListener(() =>
            {
                m_levelManager.IsWaveProgressing = true;
                m_levelManager.SavePlayerData();
                ToggleBetweenWavesUIActive(false);
                m_levelManager.ProcessScrapyardUsageBeginAnalytics();
                SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.ALEX_TEST_SCENE);
            });

            pauseWindowMainMenuButton.onClick.AddListener(() =>
            {
                m_levelManager.IsWaveProgressing = true;
                SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.ALEX_TEST_SCENE);
            });

            deathWindowRetryButton.onClick.AddListener(() =>
            {
                m_levelManager.IsWaveProgressing = true;
                m_levelManager.RestartLevel();
            });

            deathWindowScrapyrdButton.onClick.AddListener(() =>
            {
                m_levelManager.IsWaveProgressing = true;
                GameTimer.SetPaused(false);
                SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.ALEX_TEST_SCENE);
            });
            
            resumeButton.onClick.AddListener(() =>
            {
                GameTimer.SetPaused(false);
                m_levelManager.IsWaveProgressing = true;
            });
            
            ToggleBetweenWavesUIActive(false);
            
            ToggleDeathUIActive(false, string.Empty);
        }

        //============================================================================================================//

        public void UpdateLivesText()
        {
            livesText.text = "Lives: " + PlayerPersistentData.PlayerData.numLives;
        }

        public void ToggleBetweenWavesUIActive(bool active)
        {
            m_betweenWavesUI.SetActive(active);
        }

        public void ToggleDeathUIActive(bool active, string description)
        {
            m_deathUI.SetActive(active);

            deathText.text = description;
        }

        //============================================================================================================//
        
        public void RegisterPausable()
        {
            GameTimer.AddPausable(this);
        }

        public void OnResume()
        {
            pauseWindow.SetActive(false);
            pauseText.gameObject.SetActive(true);
        }

        public void OnPause()
        {
            if (Console.Open)
                return;
            
            
            if (LevelManager.Instance.EndWaveState)
                return;
            
            pauseWindow.SetActive(true);
            pauseText.gameObject.SetActive(false);
        }
    }
}

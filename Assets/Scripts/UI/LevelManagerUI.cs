﻿using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Missions;
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
        [SerializeField, Required]
        private TMP_Text scrollingMissionsText;

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

        private float m_missionReminderTimer = 0.0f;
        private bool m_isMissionReminderScrolling = false;

        private LevelManager m_levelManager;
        private RectTransform m_canvasRect;

        // Start is called before the first frame update
        private void Start()
        {
            RegisterPausable();
            m_levelManager = FindObjectOfType<LevelManager>();
            InitButtons();

            m_canvasRect = GetComponent<RectTransform>();
            scrollingMissionsText.rectTransform.anchoredPosition = Vector3.right * ((m_canvasRect.rect.width / 2) + (scrollingMissionsText.rectTransform.rect.width / 2));
        }

        private void Update()
        {
            //betweenWavesContinueButton.interactable = PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE] > 0;

            pauseWindowScrapyardButton.gameObject.SetActive(!Globals.DisableTestingFeatures);

            if (!isPaused)
            {
                m_missionReminderTimer += Time.deltaTime;
                if (m_missionReminderTimer >= Globals.MissionReminderFrequency)
                {
                    m_missionReminderTimer -= Globals.MissionReminderFrequency;
                    PlayMissionReminder();
                }
            }

            if (scrollingMissionsText.rectTransform.anchoredPosition.x < (-1 * ((m_canvasRect.rect.width / 2) + (scrollingMissionsText.rectTransform.rect.width / 2))))
            {
                scrollingMissionsText.rectTransform.anchoredPosition = Vector3.right * ((m_canvasRect.rect.width / 2) + (scrollingMissionsText.rectTransform.rect.width / 2));
                m_isMissionReminderScrolling = false;
            }
            else if (m_isMissionReminderScrolling)
                scrollingMissionsText.rectTransform.anchoredPosition += Vector2.left * Time.deltaTime * 200;
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
                SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.LEVEL);
            });

            pauseWindowScrapyardButton.onClick.AddListener(() =>
            {
                m_levelManager.IsWaveProgressing = true;
                m_levelManager.SavePlayerData();
                ToggleBetweenWavesUIActive(false);
                m_levelManager.ProcessScrapyardUsageBeginAnalytics();
                SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.LEVEL);
            });

            pauseWindowMainMenuButton.onClick.AddListener(() =>
            {
                m_levelManager.IsWaveProgressing = true;
                SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.LEVEL);
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
                SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.LEVEL);
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

        private void PlayMissionReminder()
        {
            if (MissionManager.MissionsCurrentData.CurrentTrackedMissions.Count > 0)
            {
                string missionReminderText = MissionManager.MissionsCurrentData.CurrentTrackedMissions[Random.Range(0, MissionManager.MissionsCurrentData.CurrentTrackedMissions.Count)].m_missionDescription;
                scrollingMissionsText.text = missionReminderText;
                m_isMissionReminderScrolling = true;
            }
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

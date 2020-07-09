using Sirenix.OdinInspector;
using StarSalvager.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class LevelManagerUI : MonoBehaviour
    {
        [SerializeField, Required, BoxGroup("UISections")]
        private GameObject m_betweenWavesUI;
        [SerializeField, Required, BoxGroup("UISections")]
        private GameObject m_deathUI;

        [SerializeField, Required, BoxGroup("View")]
        private Button continueButton;
        [SerializeField, Required, BoxGroup("View")]
        private Button scrapyardButton;
        [SerializeField, Required, BoxGroup("View")]
        private Button toScrapyardButton;
        [SerializeField, Required, BoxGroup("View")]
        private Button toMainMenuButton;
        [SerializeField, Required, BoxGroup("View")]
        private Button retryButton;
        [SerializeField, Required, BoxGroup("View")]
        private Button mainMenuButton;

        [SerializeField, Required, BoxGroup("View")]
        private TMP_Text m_currentWaveText;

        private LevelManager m_levelManager;

        // Start is called before the first frame update
        void Start()
        {
            m_levelManager = FindObjectOfType<LevelManager>();
            InitButtons();
        }

        private void InitButtons()
        {
            continueButton.onClick.AddListener(() =>
            {
                GameTimer.SetPaused(false);
                ToggleBetweenWavesUIActive(false);
            });

            scrapyardButton.onClick.AddListener(() =>
            {
                GameTimer.SetPaused(false);
                m_levelManager.ProcessScrapyardUsageBeginAnalytics();
                StarSalvager.SceneLoader.SceneLoader.ActivateScene("ScrapyardScene", "AlexShulmanTestScene");
            });

            toScrapyardButton.onClick.AddListener(() =>
            {
                m_levelManager.SavePlayerData();
                GameTimer.SetPaused(false);
                m_levelManager.ProcessScrapyardUsageBeginAnalytics();
                StarSalvager.SceneLoader.SceneLoader.ActivateScene("ScrapyardScene", "AlexShulmanTestScene");
            });

            toMainMenuButton.onClick.AddListener(() =>
            {
                GameTimer.SetPaused(false);
                StarSalvager.SceneLoader.SceneLoader.ActivateScene("MainMenuScene", "AlexShulmanTestScene");
            });

            retryButton.onClick.AddListener(() =>
            {
                m_levelManager.RestartLevel();
            });

            mainMenuButton.onClick.AddListener(() =>
            {
                GameTimer.SetPaused(false);
                StarSalvager.SceneLoader.SceneLoader.ActivateScene("MainMenuScene", "AlexShulmanTestScene");
            });
            ToggleBetweenWavesUIActive(false);
            ToggleDeathUIActive(false);
        }

        public void SetCurrentWaveText(int currentWave)
        {
            m_currentWaveText.text = "Current Wave " + currentWave;
        }

        public void ToggleBetweenWavesUIActive(bool active)
        {
            m_betweenWavesUI.SetActive(active);
        }

        public void ToggleDeathUIActive(bool active)
        {
            m_deathUI.SetActive(active);
        }
    }
}
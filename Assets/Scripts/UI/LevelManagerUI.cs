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
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("UISections")]
        private GameObject m_betweenWavesUI;
        [SerializeField, Required, FoldoutGroup("UISections")]
        private GameObject m_deathUI;

        [SerializeField, Required]
        private TMP_Text deathText;

        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("View")]
        private Button continueButton;
        [SerializeField, Required, FoldoutGroup("View")]
        private Button scrapyardButton;
        [SerializeField, Required, FoldoutGroup("View")]
        private Button toScrapyardButton;
        [SerializeField, Required, FoldoutGroup("View")]
        private Button toMainMenuButton;
        [SerializeField, Required, FoldoutGroup("View")]
        private Button retryButton;
        [SerializeField, Required, FoldoutGroup("View")]
        private Button mainMenuButton;

        [SerializeField, Required, FoldoutGroup("View")]
        private TMP_Text m_currentWaveText;


        //============================================================================================================//

        private LevelManager m_levelManager;

        // Start is called before the first frame update
        private void Start()
        {
            m_levelManager = FindObjectOfType<LevelManager>();
            InitButtons();
        }

        //============================================================================================================//

        private void InitButtons()
        {
            continueButton.onClick.AddListener(() =>
            {
                GameTimer.SetPaused(false);
                ToggleBetweenWavesUIActive(false);
            });

            scrapyardButton.onClick.AddListener(() =>
            {
                m_levelManager.ProcessScrapyardUsageBeginAnalytics();
                ToggleBetweenWavesUIActive(false);
                StarSalvager.SceneLoader.SceneLoader.ActivateScene("ScrapyardScene", "AlexShulmanTestScene");
            });

            toScrapyardButton.onClick.AddListener(() =>
            {
                m_levelManager.SavePlayerData();
                ToggleBetweenWavesUIActive(false);
                m_levelManager.ProcessScrapyardUsageBeginAnalytics();
                StarSalvager.SceneLoader.SceneLoader.ActivateScene("ScrapyardScene", "AlexShulmanTestScene");
            });

            toMainMenuButton.onClick.AddListener(() =>
            {
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
            
            ToggleDeathUIActive(false, string.Empty);
        }

        //============================================================================================================//

        public void SetCurrentWaveText(string endString)
        {
            m_currentWaveText.text = "Sector " + (Values.Globals.CurrentSector + 1) + " Wave " + endString;
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
    }
}

using Sirenix.OdinInspector;
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
        
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("UISections")]
        private GameObject m_betweenWavesUI;
        [SerializeField, Required, FoldoutGroup("UISections")]
        private GameObject m_deathUI;

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

        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Pause Menu")]
        private GameObject pauseWindow;
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
                m_levelManager.SavePlayerData();
                m_levelManager.ProcessScrapyardUsageBeginAnalytics();
                ToggleBetweenWavesUIActive(false);
                SceneLoader.ActivateScene("ScrapyardScene", "AlexShulmanTestScene");
            });

            toScrapyardButton.onClick.AddListener(() =>
            {
                m_levelManager.SavePlayerData();
                ToggleBetweenWavesUIActive(false);
                m_levelManager.ProcessScrapyardUsageBeginAnalytics();
                SceneLoader.ActivateScene("ScrapyardScene", "AlexShulmanTestScene");
            });

            toMainMenuButton.onClick.AddListener(() =>
            {
                SceneLoader.ActivateScene("MainMenuScene", "AlexShulmanTestScene");
            });

            retryButton.onClick.AddListener(() =>
            {
                m_levelManager.RestartLevel();
            });

            mainMenuButton.onClick.AddListener(() =>
            {
                GameTimer.SetPaused(false);
                SceneLoader.ActivateScene("MainMenuScene", "AlexShulmanTestScene");
            });
            
            resumeButton.onClick.AddListener(() =>
            {
                GameTimer.SetPaused(false);
            });
            
            ToggleBetweenWavesUIActive(false);
            
            ToggleDeathUIActive(false, string.Empty);
        }

        //============================================================================================================//

        

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
            if (LevelManager.Instance.EndWaveState)
                return;
            
            pauseWindow.SetActive(true);
            pauseText.gameObject.SetActive(false);
        }
    }
}

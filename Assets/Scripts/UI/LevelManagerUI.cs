using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Missions;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Console = StarSalvager.Utilities.Console;
using Random = UnityEngine.Random;

namespace StarSalvager.UI
{
    public class LevelManagerUI : MonoBehaviour, IPausable
    {
        private const float SCROLL_SPEED = 0.05f;
        
        public bool isPaused => GameTimer.IsPaused;
        
        [SerializeField, Required]
        private TMP_Text deathText;
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
        /*[SerializeField, Required, FoldoutGroup("Pause Menu")]
        private TMP_Text pauseText;*/

        //============================================================================================================//
        //FIXME I'll want something a little better implemented based on feedback
        public static string OverrideText
        {
            get => _overrideText;
            set
            {
                //FIXME Once this is confirmed, it should be better established
                if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(_overrideText))
                {
                    var levelManagerUI = FindObjectOfType<LevelManagerUI>();

                    var canvasWidth = levelManagerUI.m_canvasRect.rect.width / 2;
                    var scrollingWidth = levelManagerUI.scrollingMissionsText.rectTransform.rect.width / 2;
                    
                    levelManagerUI.scrollingMissionsText.rectTransform.anchoredPosition = Vector3.right * (canvasWidth + scrollingWidth);
                    levelManagerUI.m_isMissionReminderScrolling = false;
                }

                _overrideText = value;
            } 
        }
        private static string _overrideText;

        private float m_missionReminderTimer = 0.0f;
        private bool m_isMissionReminderScrolling = false;

        private Vector3 _startScrollPosition, _endScrollPosition;
        private float _scrollPosition;

        private LevelManager m_levelManager;
        private RectTransform m_canvasRect;

        private Mission curMissionReminder = null;

        //====================================================================================================================//
        
        // Start is called before the first frame update
        private void Start()
        {
            RegisterPausable();
            m_levelManager = FindObjectOfType<LevelManager>();
            InitButtons();

            m_canvasRect = GetComponent<RectTransform>();
            InitScrollPositions();
            
        }

        private void OnEnable()
        {
            pauseWindowScrapyardButton.gameObject.SetActive(Globals.TestingFeatures);
            InitScrollPositions();
        }

        private void Update()
        {
            if (isPaused)
                return;
            
            if(m_isMissionReminderScrolling)
                MoveMissionReminder();
            else
                CheckShowMissionReminder();
        }

        //============================================================================================================//

        private void InitButtons()
        {
            betweenWavesContinueButton.onClick.AddListener(() =>
            {
                Globals.IsBetweenWavesInUniverseMap = true;

                m_levelManager.IsWaveProgressing = true;
                m_levelManager.ProcessScrapyardUsageBeginAnalytics();
                ToggleBetweenWavesUIActive(false);
                LevelManager.Instance.EndWaveState = false;
                
                
                ScreenFade.Fade(() =>
                {
                    SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.LEVEL);
                });
            });

            betweenWavesScrapyardButton.onClick.AddListener(() =>
            {
                m_levelManager.IsWaveProgressing = true;
                m_levelManager.ProcessScrapyardUsageBeginAnalytics();
                ToggleBetweenWavesUIActive(false);
                LevelManager.Instance.EndWaveState = false;
                ScreenFade.Fade(() =>
                {
                    SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.LEVEL, MUSIC.SCRAPYARD);
                });
                
            });

            pauseWindowScrapyardButton.onClick.AddListener(() =>
            {
                m_levelManager.IsWaveProgressing = true;
                m_levelManager.SavePlayerData();
                ToggleBetweenWavesUIActive(false);
                m_levelManager.ProcessScrapyardUsageBeginAnalytics();
                ScreenFade.Fade(() =>
                {
                    SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.LEVEL, MUSIC.SCRAPYARD);
                });
            });

            pauseWindowMainMenuButton.onClick.AddListener(() =>
            {
                Alert.ShowAlert("Are you sure?", "Giving up will abandon your current run. Are you sure you want to do that?", "Yes", "No", (b) =>
                {
                    if (b)
                    {
                        if (Globals.UsingTutorial)
                        {
                            Globals.UsingTutorial = false;
                            LevelManager.Instance.BotObject.PROTO_GodMode = false;
                        }

                        GameUI.Instance.ShowRecoveryBanner(false);
                        Globals.IsRecoveryBot = false;
                        m_levelManager.IsWaveProgressing = true;
                        PlayerDataManager.ResetPlayerRunData();
                        PlayerDataManager.SavePlayerAccountData();
                        
                        
                        ScreenFade.Fade(() =>
                        {
                            SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.LEVEL, MUSIC.MAIN_MENU);
                        });
                    }
                });
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

                ScreenFade.Fade(() =>
                {
                    SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.LEVEL, MUSIC.SCRAPYARD);
                });
            });
            
            resumeButton.onClick.AddListener(() =>
            {
                GameTimer.SetPaused(false);
                m_levelManager.IsWaveProgressing = true;
            });
            
            ToggleBetweenWavesUIActive(false);
            
            ToggleDeathUIActive(false, string.Empty);
        }

        private void InitScrollPositions()
        {
            if (!m_canvasRect)
                return;
            
            var canvasWidth = m_canvasRect.rect.width / 2;
            var scrollMissionWidth = scrollingMissionsText.rectTransform.rect.width / 2;
            
            _startScrollPosition = Vector3.right * (canvasWidth + scrollMissionWidth);
            _endScrollPosition = Vector3.left * (canvasWidth + scrollMissionWidth);
            
            SetPosition(0f);
        }

        //============================================================================================================//

        private void CheckShowMissionReminder()
        {
            if (m_missionReminderTimer > 0)
            {
                m_missionReminderTimer -= Time.deltaTime;
                return;
            }
            
            PlayMissionReminder();
        }
        
        private void MoveMissionReminder()
        {
            if (_scrollPosition >= 1f)
            {
                SetPosition(_scrollPosition = 0f);
                m_isMissionReminderScrolling = false;
                return;
            }

            if (curMissionReminder != null)
            {
                scrollingMissionsText.text = curMissionReminder.missionName + curMissionReminder.GetMissionProgressString();
            }
            
            _scrollPosition += Time.deltaTime * SCROLL_SPEED;
            SetPosition(_scrollPosition);
        }
        
        private void PlayMissionReminder()
        {
            if (Globals.UsingTutorial)
                return;
            
            if(MissionManager.MissionsCurrentData == null || MissionManager.MissionsCurrentData.CurrentTrackedMissions == null)
                return;
            
            if (MissionManager.MissionsCurrentData.CurrentTrackedMissions.Where(m => !m.MissionComplete()).ToList().Count <= 0 && string.IsNullOrEmpty(OverrideText)) 
                return;
            
            string missionReminderText;
            if (string.IsNullOrEmpty(OverrideText))
            {
                Mission curMission = MissionManager.MissionsCurrentData
                    .CurrentTrackedMissions.Where(m => !m.MissionComplete()).ToList()[
                        Random.Range(0, MissionManager.MissionsCurrentData.CurrentTrackedMissions.Where(m => !m.MissionComplete()).ToList().Count)];
                curMissionReminder = curMission;

                missionReminderText = curMission.missionName + curMission.GetMissionProgressString();
            }
            else
            {
                missionReminderText = OverrideText;
                curMissionReminder = null;
            }

            var multiplier = string.IsNullOrEmpty(OverrideText) ? 1f : 0.2f;
            m_missionReminderTimer = Globals.MissionReminderFrequency * multiplier;
            
            scrollingMissionsText.text = missionReminderText;
            m_isMissionReminderScrolling = true;
        }
        
        private void SetPosition(float normalizedT)
        {
            scrollingMissionsText.rectTransform.anchoredPosition =
                Vector3.Lerp(_startScrollPosition, _endScrollPosition, normalizedT);
        }
        
        
        //====================================================================================================================//

        public void ToggleBetweenWavesUIActive(bool active)
        {
            int curIndex = PlayerDataManager.GetLevelRingNodeTree().ConvertSectorWaveToNodeIndex(Globals.CurrentSector, Globals.CurrentWave);

            if (PlayerDataManager.GetLevelRingNodeTree().TryFindNode(curIndex) == null)
            {
                betweenWavesContinueButton.gameObject.SetActive(false);
            }
            else
            {
                List<LevelRingNode> childNodesAccessible = PlayerDataManager.GetLevelRingNodeTree().TryFindNode(curIndex).childNodes;
                betweenWavesContinueButton.gameObject.SetActive(childNodesAccessible.Count > 0);
            }

            m_betweenWavesUI.SetActive(active);
        }

        public void ToggleDeathUIActive(bool active, string description)
        {
            m_deathUI.SetActive(active);

            deathText.text = description;
        }

        public void ShowAlertWindow(string titleText, string summaryText, Action onConfirmedCallback, string buttonText = "Ok")
        {
            Alert.ShowAlert(titleText, summaryText, buttonText, onConfirmedCallback);
        }

        public void ShowSummaryWindow(string titleText, string summaryText, Action onConfirmedCallback, GameUI.WindowSpriteSet.TYPE type = GameUI.WindowSpriteSet.TYPE.DEFAULT)
        {
            GameUI.Instance.ShowWaveSummaryWindow(true, titleText, summaryText, onConfirmedCallback, moveTime: 0.5f, type: type);
        }
        
        public void ShowGameSummaryWindow(string titleText, string summaryText, Action onConfirmedCallback)
        {
            GameUI.Instance.ShowWaveSummaryWindow(true, titleText, summaryText, onConfirmedCallback,
                GameUI.WindowSpriteSet.TYPE.ORANGE, moveTime: 0.5f);
        }



        //============================================================================================================//
        
        public void RegisterPausable()
        {
            GameTimer.AddPausable(this);
        }

        public void OnResume()
        {
            pauseWindow.SetActive(false);
            //pauseText.gameObject.SetActive(true);
        }

        public void OnPause()
        {
            if (Console.Open)
                return;
            
            
            if (LevelManager.Instance.EndWaveState)
                return;
            
            pauseWindow.SetActive(true);
            //pauseText.gameObject.SetActive(false);
        }
    }
}

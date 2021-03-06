﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Console = StarSalvager.Utilities.Console;
using Input = UnityEngine.Input;
using Random = UnityEngine.Random;

namespace StarSalvager.UI
{
    public class LevelManagerUI : MonoBehaviour, IPausable, IBuildNavigationProfile
    {
        private const float SCROLL_SPEED = 0.05f;
        
        public bool isPaused => GameTimer.IsPaused;
        
        [SerializeField, Required]
        private TMP_Text deathText;
        [SerializeField, Required]
        private TMP_Text scrollingText;

        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("UISections")]
        private GameObject m_betweenWavesUI;
        [SerializeField, Required, FoldoutGroup("UISections")]
        private GameObject m_deathUI;

        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Between Waves")]
        private Button betweenWavesContinueButton;

        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Pause Menu")]
        private GameObject pauseWindow;
        [FormerlySerializedAs("pauseWindowMainMenuButton")] [SerializeField, Required, FoldoutGroup("Pause Menu")]
        private Button giveupButton;
        [SerializeField, Required, FoldoutGroup("Pause Menu")]
        private Button resumeButton;

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
                    var scrollingWidth = levelManagerUI.scrollingText.rectTransform.rect.width / 2;
                    
                    levelManagerUI.scrollingText.rectTransform.anchoredPosition = Vector3.right * (canvasWidth + scrollingWidth);
                    levelManagerUI.m_isWaveMessageReminderScrolling = false;
                }

                _overrideText = value;
            } 
        }
        private static string _overrideText;

        private float m_waveMessageReminderTimer = 0.0f;
        private bool m_isWaveMessageReminderScrolling = false;

        private Vector3 _startScrollPosition, _endScrollPosition;
        private float _scrollPosition;

        private LevelManager m_levelManager;
        private RectTransform m_canvasRect;

        //====================================================================================================================//
        
        // Start is called before the first frame update
        private void Start()
        {
            RegisterPausable();
            m_levelManager = FindObjectOfType<LevelManager>();
            InitButtons();

            m_canvasRect = GetComponent<RectTransform>();
            InitScrollPositions();

            /*StarSalvager.Utilities.Inputs.Input.Actions.MenuControls.Pause.performed += ctx =>
            {
                if (!ctx.control.IsPressed())
                    return;
                
                GameTimer.SetPaused(false);
                InputManager.SwitchCurrentActionMap("Default");
            };*/

        }

        private void OnEnable()
        {
            InitScrollPositions();
        }

        private void OnDisable()
        {
            SetPosition(_scrollPosition = 0f);
            m_waveMessageReminderTimer = 0;
            m_isWaveMessageReminderScrolling = false;
            scrollingText.text = "";
        }

        private void Update()
        {
            if (isPaused)
            {
                /*if(Input.GetKeyDown(KeyCode.Escape))
                    resumeButton.onClick.Invoke();*/
                
                return;
            }
            
            if(m_isWaveMessageReminderScrolling)
                MoveWaveMessageReminder();
            else
                CheckShowWaveMessageReminder();
        }

        //============================================================================================================//

        private void InitButtons()
        {
            betweenWavesContinueButton.onClick.AddListener(() =>
            {
                GameManager.SetCurrentGameState(GameState.UniverseMap);
                //m_levelManager.ProcessScrapyardUsageBeginAnalytics();
                ToggleBetweenWavesUIActive(false);
                
                ScreenFade.Fade(() =>
                {
                    SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.LEVEL);
                });
            });

            giveupButton.onClick.AddListener(() =>
            {
                Alert.ShowAlert("Are you sure?",
                    "Giving up will abandon your current run. Are you sure you want to do that?", "Yes", "No",
                    value =>
                    {
                        if (!value)
                        {
                            UISelectHandler.SetBuildTarget(this);
                            return;
                        }
                        
                        if (Globals.UsingTutorial)
                        {
                            Globals.UsingTutorial = false;
                            LevelManager.Instance.BotInLevel.IsInvulnerable = false;
                        }

                        PlayerDataManager.CompleteCurrentRun();
                        PlayerDataManager.SavePlayerAccountData();
                        
                        AnalyticsManager.WaveEndEvent(AnalyticsManager.REASON.LEAVE);


                        ScreenFade.Fade(() =>
                        {
                            GameManager.SetCurrentGameState(GameState.AccountMenu);
                            SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.LEVEL,
                                MUSIC.MAIN_MENU);
                        });
                    });
            });

            /*deathWindowRetryButton.onClick.AddListener(() =>
            {
                m_levelManager.RestartLevel();
            });

            deathWindowScrapyrdButton.onClick.AddListener(() =>
            {
                GameTimer.SetPaused(false);

                ScreenFade.Fade(() =>
                {
                    SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.LEVEL, MUSIC.SCRAPYARD);
                });
            });*/
            
            resumeButton.onClick.AddListener(() =>
            {
                GameTimer.SetPaused(false);
                InputManager.SwitchCurrentActionMap(ACTION_MAP.DEFAULT);
            });
            
            ToggleBetweenWavesUIActive(false);
            
            ToggleDeathUIActive(false, string.Empty);
        }

        private void InitScrollPositions()
        {
            if (!m_canvasRect)
                return;
            
            var canvasWidth = m_canvasRect.rect.width / 2;
            var scrollTextWidth = scrollingText.rectTransform.rect.width / 2;
            
            _startScrollPosition = Vector3.right * (canvasWidth + scrollTextWidth);
            _endScrollPosition = Vector3.left * (canvasWidth + scrollTextWidth);
            
            SetPosition(0f);
        }
        

        //============================================================================================================//

        private void CheckShowWaveMessageReminder()
        {
            if (m_waveMessageReminderTimer > 0)
            {
                m_waveMessageReminderTimer -= Time.deltaTime;
                return;
            }
            
            PlayWaveMessageReminder();
        }
        
        private void MoveWaveMessageReminder()
        {
            if (_scrollPosition >= 1f)
            {
                SetPosition(_scrollPosition = 0f);
                m_isWaveMessageReminderScrolling = false;
                return;
            }
            
            _scrollPosition += Time.deltaTime * SCROLL_SPEED;
            SetPosition(_scrollPosition);
        }
        
        private void PlayWaveMessageReminder()
        {
            if (Globals.UsingTutorial)
                return;

            if (string.IsNullOrEmpty(OverrideText))
                return;

            m_waveMessageReminderTimer = Globals.WaveMessageReminderFrequency;
            
            scrollingText.text = OverrideText;
            m_isWaveMessageReminderScrolling = true;
        }
        
        private void SetPosition(float normalizedT)
        {
            scrollingText.rectTransform.anchoredPosition =
                Vector3.Lerp(_startScrollPosition, _endScrollPosition, normalizedT);
        }
        
        
        //====================================================================================================================//

        public void ToggleBetweenWavesUIActive(bool active)
        {
            //int curIndex = PlayerDataManager.GetLevelRingNodeTree().ConvertSectorWaveToNodeIndex(Globals.CurrentWave);

            /*if (PlayerDataManager.GetLevelRingNodeTree().TryFindNode(curIndex) == null)
            {
                betweenWavesContinueButton.gameObject.SetActive(false);
            }
            else
            {
                List<LevelNode> childNodesAccessible = PlayerDataManager.GetLevelRingNodeTree().TryFindNode(curIndex).childNodes;
                betweenWavesContinueButton.gameObject.SetActive(childNodesAccessible.Count > 0);
            }*/

            Debug.Log("WARNING Checks for next waves need to be added here");
            
            betweenWavesContinueButton.gameObject.SetActive(true);

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

        public void ShowSummaryWindow(string titleText, 
            string summaryText,
            Action onConfirmedCallback,
            string buttonText = "Continue")
        {
            GameUI.Instance.ShowWaveSummaryWindow(true,
                false,
                titleText,
                summaryText, 
                onConfirmedCallback,
                buttonText: buttonText,
                moveTime: 0.5f);
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
            
            if (GameManager.IsState(GameState.LevelEndWave))
                return;
            
            pauseWindow.SetActive(true);
            UISelectHandler.SetBuildTarget(this);
        }

        //IBuildNavigationProfile Functions
        //====================================================================================================================//
        
        public NavigationProfile BuildNavigationProfile()
        {
            return new NavigationProfile(resumeButton,
                new[]
                {
                    resumeButton,
                    giveupButton
                }, null, null);
        }

        //====================================================================================================================//
        
    }
}

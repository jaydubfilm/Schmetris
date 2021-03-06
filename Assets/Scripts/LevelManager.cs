﻿using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Cameras;
using StarSalvager.Cameras.Data;
using StarSalvager.Factories;
using StarSalvager.ScriptableObjects;
using StarSalvager.UI;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Values;
using System.Collections.Generic;
using StarSalvager.Utilities.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Recycling;
using StarSalvager.Audio;
using StarSalvager.Tutorial;
using StarSalvager.Utilities.Analytics;
using Random = UnityEngine.Random;
using StarSalvager.Utilities.Saving;
using System;
using System.Linq;
using StarSalvager.Parts.Data;
using StarSalvager.Prototype;
using StarSalvager.Utilities.Analytics.SessionTracking;
using StarSalvager.Utilities.Helpers;
using Input = UnityEngine.Input;

namespace StarSalvager
{
    public class LevelManager : SceneSingleton<LevelManager>, IReset, IPausable
    {
        //Properties
        //====================================================================================================================//

        
        [SerializeField, Required, BoxGroup("Prototyping")]
        private OutroScene OutroScene;

        #region Properties
        
        private float yTopPosition => Constants.gridCellSize * Globals.GridSizeY;
        private bool BotIsInPosition => m_bots[0].transform.position.y >= yTopPosition;

        private List<Bot> m_bots = new List<Bot>();
        public Bot BotInLevel => m_bots.Count > 0 ? m_bots[0] : null;

        [SerializeField, Space(10f)] private CameraController m_cameraController;
        public CameraController CameraController => m_cameraController;

        //public SectorRemoteDataScriptableObject CurrentSector => FactoryManager.Instance.SectorRemoteData[Globals.CurrentSector];

        public WaveRemoteDataScriptableObject CurrentWaveData =>
            FactoryManager.Instance.RingRemoteDatas[Globals.CurrentRingIndex].GetRemoteData(Globals.CurrentWave);

        [SerializeField, Required] private StandardBufferZoneObstacleData m_standardBufferZoneObstacleData;
        public StandardBufferZoneObstacleData StandardBufferZoneObstacleData => m_standardBufferZoneObstacleData;

        /*[SerializeField, Required]
        private PlayerLevelRemoteDataScriptableObject m_playerlevelRemoteDataScriptableObject;

        public PlayerLevelRemoteDataScriptableObject PlayerlevelRemoteDataScriptableObject =>
            m_playerlevelRemoteDataScriptableObject;*/

        private float m_waveTimer;
        public float WaveTimer => m_waveTimer;

        private float m_levelTimer = 0;
        public float LevelTimer => m_levelTimer + m_waveTimer;

        private int m_currentStage;
        public int CurrentStage => m_currentStage;

        private LevelManagerUI m_levelManagerUI;

        public bool isPaused => GameTimer.IsPaused;

        public WorldGrid WorldGrid => m_worldGrid ?? (m_worldGrid = new WorldGrid());
        private WorldGrid m_worldGrid;

        //private bool m_runLostState = false;

        public AIObstacleAvoidance AIObstacleAvoidance
        {
            get
            {
                if (m_AIObstacleAvoidance == null)
                    m_AIObstacleAvoidance = gameObject.GetComponent<AIObstacleAvoidance>();

                return m_AIObstacleAvoidance;
            }
        }

        private AIObstacleAvoidance m_AIObstacleAvoidance;

        public EnemyManager EnemyManager
        {
            get
            {
                if (m_enemyManager == null)
                    m_enemyManager = gameObject.GetComponent<EnemyManager>();

                return m_enemyManager;
            }
        }

        private EnemyManager m_enemyManager;

        public ObstacleManager ObstacleManager
        {
            get
            {
                if (m_obstacleManager == null)
                    m_obstacleManager = gameObject.GetComponent<ObstacleManager>();

                return m_obstacleManager;
            }
        }

        private ObstacleManager m_obstacleManager;

        public TutorialManager TutorialManager
        {
            get
            {
                if (_tutorialManager == null)
                    _tutorialManager = m_levelManagerUI.GetComponentInChildren<TutorialManager>(true);

                return _tutorialManager;
            }
        }

        private TutorialManager _tutorialManager;

        public ProjectileManager ProjectileManager =>
            m_projectileManager ?? (m_projectileManager = new ProjectileManager());

        private ProjectileManager m_projectileManager;

        public WaveEndSummaryData WaveEndSummaryData => m_waveEndSummaryData;
        private WaveEndSummaryData m_waveEndSummaryData;

        public GameUI GameUi
        {
            get
            {
                if (!_gameUi)
                    _gameUi = m_levelManagerUI.GetComponentInChildren<GameUI>(true);

                return _gameUi;
            }
        }

        private GameUI _gameUi;

        public int NumWavesInRow;

        public bool m_botEnterScreen { get; private set; } = false;
        public bool m_botZoomOffScreen { get; private set; } = false;

        private bool _playerDataSaved;

        private float botMoveOffScreenSpeed = 1.0f;

        private bool m_endLevelOverride = false;
        
        [SerializeField] private AnimationCurve enterCurve;

        private GameObject _thrusterEffectObject;


        private const int WARNING_COUNT = 1;
        private int _audioCountDown = WARNING_COUNT;
        private float _afterWaveTimer;

        private float _enterTime => 1.3f;
        private float _t;
        private float _startY;

        #endregion //Properties

        //Unity Functions
        //====================================================================================================================//

        #region Unity Functions

        private void Start()
        {
            RegisterPausable();

            _afterWaveTimer = Globals.TimeAfterWaveEndFlyOut;

            m_bots = new List<Bot>();

            m_levelManagerUI = FindObjectOfType<LevelManagerUI>();
            _gameUi = m_levelManagerUI.GetComponentInChildren<GameUI>(true);

            Bot.OnBotDied += OnBotDied;
        }

        private void Update()
        {
            if (isPaused)
                return;

            if (!GameManager.IsState(GameState.LEVEL))
                return;

            CheckBotPositions();

            if (Globals.UsingTutorial)
                return;

            if (GameManager.IsState(GameState.LevelActiveEndSequence))
            {
                TryBeginWaveEndSequence();
            }
            else if (GameManager.IsState(GameState.LEVEL_ACTIVE))
            {
                ProgressStage();
            }
            else if (BotIsInPosition)
            {
                ProcessEndOfWave();
            }
            else
            {
                SetBotExitScreen(true);
            }
        }

        private void LateUpdate()
        {
            if (GameManager.IsState(GameState.LevelEndWave))
                return;

            UpdateUIClock();
            CheckPlayWarningSound();
        }

        #endregion //Unity Functions

        //Level Init Functions
        //====================================================================================================================//

        #region Level Init Functions

        private void InitLevel()
        {
            AudioController.CrossFadeTrack(MUSIC.FRINGE);

            //--------------------------------------------------------------------------------------------------------//

            m_worldGrid = null;
            m_waveEndSummaryData = new WaveEndSummaryData(Globals.CurrentRingIndex, Globals.CurrentWave);
            
            GameManager.SetCurrentGameState(GameState.LevelActive);


            //Setup Bot
            //--------------------------------------------------------------------------------------------------------//

            //var startingHealth = PART_TYPE.CORE.GetRemoteData().GetDataValue<float>(PartProperties.KEYS.Health);

            m_bots.Add(FactoryManager.Instance.GetFactory<BotFactory>().CreateObject<Bot>());
            BotInLevel.transform.position = new Vector2(0, Constants.gridCellSize * 5);

            var botDataToLoad = PlayerDataManager.GetBotBlockDatas();

            if (botDataToLoad.Count == 0 || Globals.UsingTutorial)
            {
                BotInLevel.InitBot();
                //BotInLevel.SetupHealthValues(startingHealth, startingHealth);
            }
            else
            {
                BotInLevel.InitBot(botDataToLoad.ImportBlockDatas(false));
                //BotInLevel.SetupHealthValues(startingHealth, PlayerDataManager.GetBotHealth());
            }

            BotInLevel.transform.parent = null;
            SceneManager.MoveGameObjectToScene(BotInLevel.gameObject, gameObject.scene);
            PlayerDataManager.RefillHealth();


            //Post Bot Setup
            //--------------------------------------------------------------------------------------------------------//

            InputManager.Instance.InitInput();
            InputManager.Instance.LockRotation = true;

            //FIXME
            //SessionDataProcessor.Instance.StartNewWave(Globals.CurrentRingIndex, Globals.CurrentWave, BotInLevel.GetBlockDatas());

            CameraController.SetOrthographicSize(Constants.gridCellSize * Globals.ColumnsOnScreen,
                BotInLevel.transform.position);
            if (Globals.Orientation == ORIENTATION.VERTICAL)
            {
                Globals.GridSizeY =
                    (int) ((CameraController.Camera.orthographicSize * Globals.GridHeightRelativeToScreen * 2) /
                           Constants.gridCellSize);
            }
            else
            {
                Globals.GridSizeY =
                    (int) ((CameraController.Camera.orthographicSize * Globals.GridHeightRelativeToScreen * 2 *
                            (Screen.width / (float) Screen.height)) / Constants.gridCellSize);
            }


            WorldGrid.SetupGrid();
            ProjectileManager.Activate();

            m_levelManagerUI.ToggleDeathUIActive(false, string.Empty);
            GameTimer.SetPaused(false);

            if (Globals.UsingTutorial)
            {
                TutorialManager.SetupTutorial();
                return;
            }

            AnalyticsManager.WaveStartEvent(m_waveEndSummaryData.Sector, m_waveEndSummaryData.Wave);

            Random.InitState(CurrentWaveData.WaveSeed);
            Debug.Log("SET SEED " + CurrentWaveData.WaveSeed);

            SetBotBelowScreen();
            SetBotExitScreen(false);
            SetBotEnterScreen(true);

            //CheckPlayerWater();
        }

        #endregion //Level Init Functions

        //LevelManager Update Functions
        //====================================================================================================================//

        //FIXME Does this need to be happening every frame? This function is for the zoom in/out of level, it likely does not need to run every frame
        private void CheckBotPositions()
        {
            if (GameManager.IsState(GameState.LevelBotDead))
            {
                return;
            }

            foreach (var bot in m_bots)
            {
                var pos = bot.transform.position;

                if (!m_botEnterScreen)
                {
                    bot.transform.localScale = Vector2.one;
                    continue;
                }

                if (_t / _enterTime >= 1f)
                {
                    SetBotEnterScreen(false);
                    BotInLevel.IsInvulnerable = Globals.UsingTutorial;

                    //See if we need to show any hints to the player once the bot is on screen
                    BotInLevel.DisplayHints();

                    _t = 0f;
                    _startY = 0f;

                    continue;
                }

                var t = enterCurve.Evaluate(_t / _enterTime);

                var newY = Mathf.Lerp(_startY, 5 * Constants.gridCellSize, t);
                pos.y = newY;

                bot.transform.position = pos;
                float scale = Mathf.Lerp(Globals.BotEnterScreenMaxSize, 1,
                    t /*bot.transform.position.y / (5 * Constants.gridCellSize)*/);
                bot.transform.localScale = Vector3.one * scale;

                _t += Time.deltaTime;

                m_cameraController.SetTrackedOffset(y: (5 * Constants.gridCellSize) - newY);
            }

            if (m_botZoomOffScreen)
            {
                MoveBotOffScreen();
            }
        }

        //Stage Functions
        //====================================================================================================================//

        private void ProgressStage()
        {
            if (GameManager.IsState(GameState.LEVEL_ACTIVE))
            {
                m_waveTimer += Time.deltaTime;
            }

            int currentStage = m_currentStage;
            if (CurrentWaveData.TrySetCurrentStage(m_waveTimer, out m_currentStage))
            {
                if (currentStage != m_currentStage)
                {
                    ObstacleManager.SetupStage(m_currentStage);
                }

                return;
            }

            if (m_currentStage == currentStage + 1)
                TransitionToEndWaveState();
        }

        public void SetStage(int stage)
        {
            throw new NotImplementedException();
            /*if (stage >= CurrentWaveData.StageRemoteData.Count)
            {
                Debug.LogError("Tried to set stage that does not exist in this wave");
                return;
            }

            float waveTimer = 0;

            for (int i = 0; i < stage; i++)
            {
                waveTimer += CurrentWaveData.StageRemoteData[i].StageDuration;
            }

            m_waveTimer = waveTimer;

            int currentStage = m_currentStage;
            if (CurrentWaveData.TrySetCurrentStage(m_waveTimer, out m_currentStage))
            {
                if (currentStage != m_currentStage)
                {
                    ObstacleManager.SetupStage(m_currentStage);
                }

                return;
            }

            if (m_currentStage == currentStage + 1)
                TransitionToEndWaveState();*/
        }

        //====================================================================================================================//
        public void ForceSetTimeRemaining(float timeLeft)
        {
            m_waveTimer = CurrentWaveData.GetWaveDuration() - timeLeft;
        }

        //Wave End State
        //============================================================================================================//

        public void CompleteWave()
        {
            m_endLevelOverride = true;
            TransitionToEndWaveState();
        }

        //This handles cleanup for when you've entered the end wave state above. This and the above function likely should be combined in some way, I don't recall why it was originally set up like this and its on the list to fix.
        private void TryBeginWaveEndSequence()
        {
            //Checks to see if there are any collectible bits in view of the player before wrapping
            if (!m_endLevelOverride && _afterWaveTimer >= 0)
            {
                _afterWaveTimer -= Time.deltaTime;
                CheckPlayWarningSound();
                return;
            }
            
            if (ObstacleManager.AnyAttachableBitOnScreen && !m_endLevelOverride) return;


            //AudioController.PlaySound(SOUND.END_WAVE);

            GameManager.SetCurrentGameState(GameState.LevelEndWave);
            //EnemyManager.SetEnemiesFallEndLevel();

            SavePlayerData();

            //ObstacleManager.IncreaseSpeedAllOffGridMoving(3.0f);
            NumWavesInRow++;

            LevelManagerUI.OverrideText = string.Empty;
            m_levelTimer += m_waveTimer;
            m_waveTimer = 0;

            BotInLevel.SetSortingLayer(LayerHelper.OVERLAY, 10000);


            Random.InitState(CurrentWaveData.WaveSeed);
            Debug.Log("SET SEED " + CurrentWaveData.WaveSeed);

            //FIXME
            Debug.Log("WARNING The progress for bot must be saved here");
            /*int curNodeIndex = PlayerDataManager.GetLevelRingNodeTree().ConvertSectorWaveToNodeIndex(Globals.CurrentSector, Globals.CurrentWave);
            if (!PlayerDataManager.GetPlayerPreviouslyCompletedNodes().Contains(curNodeIndex))
            {
                PlayerDataManager.AddCompletedNode(curNodeIndex);
            }
            PlayerDataManager.SetCurrentNode(curNodeIndex);*/

            PlayerDataManager.SetCurrentWave(PlayerDataManager.GetCurrentWave() + 1);

            for (int i = 0; i < m_bots.Count; i++)
            {
                m_bots[i].SetColliderActive(false);
            }
        }

        //This triggers when the timer hits 0 and the level is moving into completed state. This is not when the level pauses and the summaryy screen shows
        private void TransitionToEndWaveState()
        {
            if (GameManager.IsState(GameState.LevelBotDead))
            {
                return;
            }

            //Prevent the bot from burning anymore resources at the end of a wave
            if (BotInLevel.CanUseResources)
            {
                GameUi.SetCurrentWaveText("Complete");
                
                Toast.AddToast("Wave Complete!");
                
                BotInLevel.CanUseResources = false;
            }


            GameManager.SetCurrentGameState(GameState.LevelActiveEndSequence);
            EnemyManager.SetEnemiesFallEndLevel();
        }

        //This triggers when the wave ends, not when the timer hits 0
        private void ProcessEndOfWave()
        {
            if (GameManager.IsState(GameState.LevelBotDead))
            {
                return;
            }

            BotInLevel.IsInvulnerable = false;

            var botBlockData = BotInLevel.GetBlockDatas();
            SessionDataProcessor.Instance.SetEndingLayout(botBlockData);
            SessionDataProcessor.Instance.EndActiveWave();

            GameUi.SetLevelProgressSlider(1f);
            

            PlayerDataManager.ChangeXP(CurrentWaveData.WaveXP);
           

            
            //SavePlayerData();
            GameTimer.SetPaused(true);

            //PlayerDataManager.GetResource(BIT_TYPE.RED).AddAmmo(10);
            AudioController.CrossFadeTrack(MUSIC.NONE);

            m_levelManagerUI.ShowSummaryWindow(
                WaveEndSummaryData.WaveEndTitle,
                m_waveEndSummaryData.GetWaveEndSummaryDataString(),
                () =>
                {
                    GameManager.SetCurrentGameState(GameState.UniverseMap);
                    //ProcessScrapyardUsageBeginAnalytics();
                    PlayerDataManager.SetCanChoosePart(true);

                    ScreenFade.Fade(() => { SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.LEVEL); });
                },
                "Continue");

            //Dictionary<string, object> levelCompleteAnalyticsDictionary = WaveEndSummaryData.GetWaveEndSummaryAnalytics();
            
            //string levelCompleteString = $"{WaveEndSummaryData.CompletedSector}.{WaveEndSummaryData.CompletedWave}";
            
            AnalyticsManager.WaveEndEvent(AnalyticsManager.REASON.WIN);
            PlayerDataManager.SetPlayerCoordinate(PlayerDataManager.GetPlayerTargetCoordinate());
            /*AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelComplete,
                eventDataDictionary: levelCompleteAnalyticsDictionary, eventDataParameter: levelCompleteString);*/

            ObstacleManager.MoveToNewWave();
            EnemyManager.MoveToNewWave();
            EnemyManager.SetEnemiesInert(false);
            EnemyManager.RecycleAllEnemies();
            CurrentWaveData.TrySetCurrentStage(m_waveTimer, out m_currentStage);

            ProjectileManager.CleanProjectiles();
            Globals.ResetFallSpeed();
        }

        //Bot offscreen Movement
        //====================================================================================================================//
        //FIXME This will need to be cleaned up
        private void MoveBotOffScreen()
        {
            const float offset = Constants.gridCellSize * 5;

            var yPos = Constants.gridCellSize * Globals.GridSizeY;
            if (botMoveOffScreenSpeed < 20)
            {
                botMoveOffScreenSpeed += Time.deltaTime * botMoveOffScreenSpeed * 2;
            }

            foreach (var bot in m_bots)
            {
                bot.transform.position += Vector3.up * (botMoveOffScreenSpeed * Time.deltaTime);
                float scale = Mathf.Lerp(1.0f, Globals.BotExitScreenMaxSize,
                    (bot.transform.position.y - offset) / (yPos - offset));
                bot.transform.localScale = new Vector2(scale, scale);

                m_cameraController.SetTrackedOffset(y: offset + -bot.transform.position.y);
            }
        }

        public void SetBotBelowScreen()
        {
            //Debug.LogError("ERROR: This needs to be fixed to support new movement system");
            for (int i = 0; i < m_bots.Count; i++)
            {
                m_bots[i].transform.position = Vector3.down * 5;
            }

            _startY = -5f;

        }

        public void SetBotEnterScreen(bool value)
        {
            m_botEnterScreen = value;

            if (value)
            {
                AudioController.PlaySound(SOUND.BOT_ARRIVES);
                CreateThrustEffect(BotInLevel);
                BotInLevel.IsInvulnerable = true;
            }
            else if (_thrusterEffectObject)
            {
                InputManager.Instance.LockRotation = false;
                Destroy(_thrusterEffectObject);
            }
        }

        public void SetBotExitScreen(bool value)
        {
            if (GameManager.IsState(GameState.LevelBotDead))
            {
                return;
            }

            m_botZoomOffScreen = value;

            if (!value)
            {
                botMoveOffScreenSpeed = 1.0f;
            }

            if (value && !_thrusterEffectObject)
            {
                AudioController.PlaySound(SOUND.BOT_DEPARTS);
                CreateThrustEffect(BotInLevel);
            }
            else if (!value && _thrusterEffectObject)
            {
                Destroy(_thrusterEffectObject);
            }
        }

        //====================================================================================================================//

        //FIXME Does this need to be in the LevelManager?
        public void DropLoot(List<IRDSObject> loot, Vector3 position, bool isFromEnemyLoot)
        {
            ObstacleManager.SpawnObstacleExplosion(position, loot, isFromEnemyLoot);
        }

        public void RestartLevel()
        {
            m_levelManagerUI.ToggleDeathUIActive(false, string.Empty);
            GameUi.SetCurrentWaveText(0, Globals.CurrentWave + 1);
            GameTimer.SetPaused(false);

            ScreenFade.Fade(() => { SceneLoader.ActivateScene(SceneLoader.LEVEL, SceneLoader.LEVEL); });

        }

        private void OnBotDied(Bot _, string deathMethod)
        {
            //LiquidResourcesCachedOnDeath = new Dictionary<BIT_TYPE, float>();
            //PlayerDataManager.AddCoreDeath();

            /*foreach (BIT_TYPE _bitType in Enum.GetValues(typeof(BIT_TYPE)))
            {
                if (_bitType == BIT_TYPE.BUMPER || _bitType == BIT_TYPE.NONE)
                    continue;

                LiquidResourcesCachedOnDeath.Add(_bitType, PlayerDataManager.GetResource(_bitType).Ammo);
            }*/

            InputManager.Instance.CancelMove();
            InputManager.Instance.LockRotation = true;

            SavePlayerData();
            GameManager.SetCurrentGameState(GameState.LevelBotDead);

            Dictionary<int, float> tempDictionary = new Dictionary<int, float>();
            foreach (var bitType in Constants.BIT_ORDER)
            {
                tempDictionary.Add((int) bitType, PlayerDataManager.GetResource(bitType).Ammo);
            }

            /*Dictionary<string, object> levelLostAnalyticsDictionary = new Dictionary<string, object>
            {
                {AnalyticsManager.DeathCause, deathMethod},
                {AnalyticsManager.LevelTime, m_levelTimer + m_waveTimer},
            };
            string levelLostString = $"Wave {Globals.CurrentWave + 1}";
            AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelLost,
                levelLostAnalyticsDictionary,
                levelLostString);*/

            AnalyticsManager.WaveEndEvent(AnalyticsManager.REASON.DEATH);

            SessionDataProcessor.Instance.PlayerKilled();
            SessionDataProcessor.Instance.EndActiveWave();

            //Alert.ShowDancers(true);
            AudioController.CrossFadeTrack(MUSIC.NONE);

            //m_runLostState = true;
            PlayerDataManager.CompleteCurrentRun();
            PlayerDataManager.SavePlayerAccountData();
            
            Globals.CurrentWave = 0;

            InputManager.SwitchCurrentActionMap(ACTION_MAP.MENU);
            OutroScene.gameObject.SetActive(true);
            GameUI.Instance.FadeBackground(true);
        }

        //====================================================================================================================//
        private void SavePlayerData()
        {
            //For some reason the function was being called MANY TIMES. Added this bool to prevent.
            if (_playerDataSaved)
                return;
            
            if (Globals.UsingTutorial)
                return;

            foreach (Bot bot in m_bots)
            {
                bot.ResetRotationToIdentity();
                
                //PlayerDataManager.SetBotHealth(bot.CurrentHealth);
                PlayerDataManager.SetDroneBlockData(bot.GetBlockDatas());
                PlayerDataManager.DowngradeAllBits(1, false);
            }
            
            PlayerDataManager.SavePlayerAccountData();

            _playerDataSaved = true;
        }

        private void CleanLevel()
        {
            for (int i = m_bots.Count - 1; i >= 0; i--)
            {
                if (m_bots == null)
                    continue;

                Recycler.Recycle<Bot>(m_bots[i]);
                m_bots.RemoveAt(i);
            }

            ObstacleManager.WorldElementsRoot.transform.position = Vector3.zero;

            m_waveEndSummaryData = null;
            m_waveTimer = 0;

            _audioCountDown = WARNING_COUNT;
            _afterWaveTimer = Globals.TimeAfterWaveEndFlyOut;

            SetBotEnterScreen(false);
            SetBotExitScreen(false);

            CurrentWaveData.TrySetCurrentStage(m_waveTimer, out m_currentStage);
            ProjectileManager.Reset();
            m_endLevelOverride = false;

            _playerDataSaved = false;
        }

        //UI Functions
        //====================================================================================================================//

        private void UpdateUIClock()
        {
            //Displays the time in timespan & the fill value
            var duration = CurrentWaveData.GetWaveDuration();
            var timeLeft = duration - m_waveTimer;

            GameUi.SetLevelProgressSlider(1f - timeLeft / duration);
        }

        //Effects Functions
        //====================================================================================================================//

        #region Effects

        private void CreateThrustEffect(in Bot bot)
        {
            if (_thrusterEffectObject != null)
                return;

            if (bot.Rotating)
            {
                bot.ForceCompleteRotation();
            }

            //Find the lowest world space positioned block on bot
            var lowestPosition = bot.AttachedBlocks
                .Select(x => (Vector2)x.transform.position)
                .OrderBy(pos => pos.y)
                .First();

            var localPosition = lowestPosition + DIRECTION.DOWN.ToVector2() / 2f;

            var effect = FactoryManager.Instance.GetFactory<EffectFactory>().CreateEffect(EffectFactory.EFFECT.THRUST);
            var effectTransform = effect.transform;
            effectTransform.SetParent(bot.transform);
            effectTransform.localPosition = bot.transform.InverseTransformPoint(localPosition);

            _thrusterEffectObject = effect;
        }

        private void CheckPlayWarningSound()
        {
            if (Globals.UsingTutorial)
                return;

            var timeLeft = _afterWaveTimer;

            if (_audioCountDown < 1 || timeLeft >= 5f)
                //if (_audioCountDown < 1 || timeLeft >= _audioCountDown)
                return;

            _audioCountDown--;
            AudioController.PlaySound(SOUND.END_WAVE_COUNT);
        }

        #endregion //Effects

        //IReset Functions
        //====================================================================================================================//

        #region IReset Functions

        public void Activate()
        {
            TutorialManager.gameObject.SetActive(Globals.UsingTutorial);

            InitLevel();
        }

        public void Reset()
        {
            CleanLevel();
        }

        #endregion //IReset Functions

        //IPausable Functions
        //============================================================================================================//

        #region IPausable

        public void RegisterPausable()
        {
            GameTimer.AddPausable(this);
        }

        public void OnResume()
        {
            GameUi.SetCurrentWaveText(Globals.CurrentRingIndex + 1, Globals.CurrentWave + 1);
        }

        public void OnPause()
        {

        }

        #endregion //IPausable

        //Unity Editor
        //====================================================================================================================//

        #region Unity Editor

#if UNITY_EDITOR

        [SerializeField] private bool drawGrid = true;

        private void OnDrawGizmos()
        {
            if (!drawGrid)
                return;

            Gizmos.color = Color.red;
            WorldGrid?.OnDrawGizmos();
        }

#endif

        #endregion //Unity Editor

        //====================================================================================================================//

    }
}

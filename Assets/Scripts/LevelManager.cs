using Sirenix.OdinInspector;
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
using System.Linq;
using StarSalvager.Utilities.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarSalvager.Missions;
using StarSalvager.Utilities.JsonDataTypes;
using Recycling;
using StarSalvager.Audio;
using StarSalvager.Tutorial;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Particles;
using Random = UnityEngine.Random;
using StarSalvager.Utilities.Saving;
using System;
using StarSalvager.Prototype;

namespace StarSalvager
{
    public class LevelManager : SceneSingleton<LevelManager>, IReset, IPausable
    {
        //Properties
        //====================================================================================================================//

        [SerializeField, Required, BoxGroup("Prototyping")]
        private OutroScene OutroScene;

        #region Properties

        private List<Bot> m_bots = new List<Bot>();
        public Bot BotObject => m_bots.Count > 0 ? m_bots[0] : null;

        [SerializeField, Space(10f)]
        private CameraController m_cameraController;
        public CameraController CameraController => m_cameraController;

        public SectorRemoteDataScriptableObject CurrentSector => FactoryManager.Instance.SectorRemoteData[Globals.CurrentSector];

        public WaveRemoteDataScriptableObject CurrentWaveData => CurrentSector.GetIndexConvertedRemoteData(Globals.CurrentSector, Globals.CurrentWave);

        [SerializeField, Required]
        private StandardBufferZoneObstacleData m_standardBufferZoneObstacleData;
        public StandardBufferZoneObstacleData StandardBufferZoneObstacleData => m_standardBufferZoneObstacleData;

        [SerializeField, Required]
        private PlayerLevelRemoteDataScriptableObject m_playerlevelRemoteDataScriptableObject;
        public PlayerLevelRemoteDataScriptableObject PlayerlevelRemoteDataScriptableObject => m_playerlevelRemoteDataScriptableObject;

        private float m_waveTimer;
        public float WaveTimer => m_waveTimer;

        public bool IsWaveProgressing = true;

        private float m_levelTimer = 0;
        public float LevelTimer => m_levelTimer + m_waveTimer;

        private float m_checkFlightLengthMissionTimer = 0;

        private int m_currentStage;
        public int CurrentStage => m_currentStage;

        public bool EndWaveState = false;
        public bool EndSectorState = false;

        private LevelManagerUI m_levelManagerUI;

        public bool isPaused => GameTimer.IsPaused;

        public WorldGrid WorldGrid => m_worldGrid ?? (m_worldGrid = new WorldGrid());
        private WorldGrid m_worldGrid;

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

        public ProjectileManager ProjectileManager => m_projectileManager ?? (m_projectileManager = new ProjectileManager());
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

        public Dictionary<BIT_TYPE, float> LiquidResourcesCachedOnDeath = new Dictionary<BIT_TYPE, float>();
        public int WaterAtBeginningOfWave;
        public int NumWavesInRow;
        public Dictionary<ENEMY_TYPE, int> EnemiesKilledInWave = new Dictionary<ENEMY_TYPE, int>();
        public List<string> MissionsCompletedDuringThisFlight = new List<string>();
        public bool BotDead = false;

        public bool m_botEnterScreen { get; private set; } = false;
        public bool m_botZoomOffScreen { get; private set; } = false;

        private float botMoveOffScreenSpeed = 1.0f;

        #endregion //Properties

        //Unity Functions
        //====================================================================================================================//
        
        private void Start()
        {
            RegisterPausable();
            
            m_bots = new List<Bot>();
            
            m_levelManagerUI = FindObjectOfType<LevelManagerUI>();
            _gameUi = m_levelManagerUI.GetComponentInChildren<GameUI>(true);

            Bot.OnBotDied += OnBotDied;
        }

        private void Update()
        {
            /*if (UnityEngine.Input.GetKeyDown(KeyCode.Y))
            {
                WorldGrid.DrawDebugMarkedGridPoints();
                Debug.Break();
            }*/

            if (isPaused)
                return;

            CheckBotPositions();

            if (Globals.UsingTutorial)
            {
                return;
            }

            if (!EndWaveState)
            {
                ProgressStage();
            }
            else if (BotIsInPosition())
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
            if (EndWaveState) 
                return;
            
            UpdateUIClock();
        }

        //LevelManager Update Functions
        //====================================================================================================================//

        private float _enterTime = 1.3f;
        private float _t;
        private float _startY;
        //FIXME Does this need to be happening every frame?
        private void CheckBotPositions()
        {
            if (BotDead || (BotObject != null && BotObject.Destroyed))
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
                    BotObject.PROTO_GodMode = Globals.UsingTutorial;
                    
                    _t = 0f;
                    _startY = 0f;
                    
                    continue;
                }

                var t = enterCurve.Evaluate(_t / _enterTime);
                
                var newY = Mathf.Lerp(_startY, 5 * Constants.gridCellSize,  t);
                pos.y = newY;
                
                bot.transform.position = pos;
                float scale = Mathf.Lerp(Globals.BotEnterScreenMaxSize, 1,  t/*bot.transform.position.y / (5 * Constants.gridCellSize)*/);
                bot.transform.localScale = Vector3.one * scale;

                _t += Time.deltaTime;
                
                m_cameraController.SetTrackedOffset(y: (5 * Constants.gridCellSize) - newY);
            }

            if (m_botZoomOffScreen)
            {
                MoveBotOffScreen();
            }
        }

        //====================================================================================================================//
        
        private void ProgressStage()
        {
            if (IsWaveProgressing)
            {
                m_waveTimer += Time.deltaTime;
                m_checkFlightLengthMissionTimer += Time.deltaTime;
                if (m_checkFlightLengthMissionTimer >= 1.0f)
                {
                    m_checkFlightLengthMissionTimer -= 1;
                    MissionProgressEventData missionProgressEventData = new MissionProgressEventData
                    {
                        floatAmount = LevelTimer
                    };
                    MissionManager.ProcessMissionData(typeof(FlightLengthMission), missionProgressEventData);
                }
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
            if (stage >= CurrentWaveData.StageRemoteData.Count)
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
                TransitionToEndWaveState();
        }

        private void ProcessEndOfWave()
        {
            if (BotDead || (BotObject != null && BotObject.Destroyed))
            {
                return;
            }

            var botBlockData = BotObject.GetBlockDatas();
            SessionDataProcessor.Instance.SetEndingLayout(botBlockData);
            SessionDataProcessor.Instance.EndActiveWave();

            GameUi.SetProgressValue(1f);
            //GameUi.SetTimeString(0);
            SavePlayerData();
            GameTimer.SetPaused(true);

            PlayerDataManager.GetResource(BIT_TYPE.RED).AddLiquid(10);

            if (Globals.IsRecoveryBot)
            {
                m_levelManagerUI.ShowSummaryScreen("Bot Recovered",
                    "You have recovered your wrecked bot. Return to base!", () =>
                    {
                        GameUi.ShowRecoveryBanner(false);
                        GameTimer.SetPaused(false);
                        EndWaveState = false;
                        EndSectorState = false;
                        ProcessLevelCompleteAnalytics();
                        ProcessScrapyardUsageBeginAnalytics();

                        ScreenFade.Fade(() =>
                        {
                            SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.LEVEL);
                        });
                    });
            }
            else if (EndSectorState)
            {
                m_levelManagerUI.ShowSummaryScreen("Sector Completed",
                    "You beat the last wave of the sector. Return to base!", () =>
                    {
                        GameTimer.SetPaused(false);
                        EndWaveState = false;
                        EndSectorState = false;
                        ProcessLevelCompleteAnalytics();
                        ProcessScrapyardUsageBeginAnalytics();

                        ScreenFade.Fade(() =>
                        {
                            SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.LEVEL);
                        });
                    });
            }
            else
            {
                
                
                m_levelManagerUI.ShowWaveSummaryWindow(
                    WaveEndSummaryData.WaveEndTitle,
                    m_waveEndSummaryData.GetWaveEndSummaryDataString(),
                    () => 
                {
                    
                    Globals.IsBetweenWavesInUniverseMap = true;
                    IsWaveProgressing = true;
                    ProcessScrapyardUsageBeginAnalytics();
                    EndWaveState = false;
                    
                    ScreenFade.Fade(() =>
                    {
                        AudioController.FadeInMusic();
                        SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.LEVEL);
                    });
                });
                
                /*//Turn wave end summary data into string, post in alert, and clear wave end summary data
                m_levelManagerUI.ShowSummaryScreen(WaveEndSummaryData.WaveEndTitle,
                    m_waveEndSummaryData.GetWaveEndSummaryDataString(),
                    () => 
                    {
                        Globals.IsBetweenWavesInUniverseMap = true;
                        IsWaveProgressing = true;
                        ProcessScrapyardUsageBeginAnalytics();
                        EndWaveState = false;
                        SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.LEVEL);
                    },
                    "Continue");*/
            }



            Dictionary<string, object> levelCompleteAnalyticsDictionary =
                WaveEndSummaryData.GetWaveEndSummaryAnalytics();
            string levelCompleteString = $"{WaveEndSummaryData.CompletedSector}.{WaveEndSummaryData.CompletedWave}";
            AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelComplete,
                eventDataDictionary: levelCompleteAnalyticsDictionary, eventDataParameter: levelCompleteString);

            m_waveEndSummaryData = new WaveEndSummaryData();
            ObstacleManager.MoveToNewWave();
            EnemyManager.MoveToNewWave();
            EnemyManager.SetEnemiesInert(false);
            EnemyManager.RecycleAllEnemies();
            CurrentWaveData.TrySetCurrentStage(m_waveTimer, out m_currentStage);

            EnemiesKilledInWave.Clear();
            MissionManager.ProcessWaveComplete();

            /*if (PlayerDataManager.GetResource(BIT_TYPE.BLUE).resource <= 0)
            {
                m_levelManagerUI.ShowSummaryScreen("Out of water",
                    "Your scrapyard is out of water. You must return now.", () =>
                    {
                        IsWaveProgressing = true;
                        EndWaveState = false;
                        SavePlayerData();
                        m_levelManagerUI.ToggleBetweenWavesUIActive(false);
                        ProcessScrapyardUsageBeginAnalytics();
                        SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.LEVEL);
                    });
            }*/
            
            ProjectileManager.CleanProjectiles();

            MissionManager.ProcessMissionData(typeof(SectorsCompletedMission),
                new MissionProgressEventData());

            //ProjectileManager.UpdateForces();

            Globals.IsRecoveryBot = false;
        }

        //FIXME This will need to be cleaned up
        private void MoveBotOffScreen()
        {
            const float offset = Constants.gridCellSize * 5;
            
            var yPos = Constants.gridCellSize * Globals.GridSizeY;
            if (botMoveOffScreenSpeed < 20)
            {
                if (Globals.IsRecoveryBot && !ObstacleManager.RecoveredBotTowing)
                {
                    botMoveOffScreenSpeed += Time.deltaTime * botMoveOffScreenSpeed * 0.25f;
                }
                else
                {
                    botMoveOffScreenSpeed += Time.deltaTime * botMoveOffScreenSpeed * 2;
                }
            }

            foreach (var bot in m_bots)
            {
                bot.transform.position += Vector3.up * (botMoveOffScreenSpeed * Time.deltaTime);
                float scale = Mathf.Lerp(1.0f, Globals.BotExitScreenMaxSize,
                    (bot.transform.position.y - offset) / (yPos - offset));
                bot.transform.localScale = new Vector2(scale, scale);


                const float distanceTrail = 6.0f;
                if (ObstacleManager.RecoveredBotFalling != null && bot.transform.position.y >=
                    ObstacleManager.RecoveredBotFalling.transform.position.y + distanceTrail)
                {
                    if (!ObstacleManager.RecoveredBotTowing)
                    {
                        CreateTowEffect();
                        AudioController.PlaySound(SOUND.RECOVERY_TOW);
                    }

                    ObstacleManager.RecoveredBotTowing = true;
                    ObstacleManager.RecoveredBotFalling.transform.position =
                        bot.transform.position + (Vector3.down * distanceTrail);

                    UpdateTowLineRenderer(bot.transform.position,
                        ObstacleManager.RecoveredBotFalling.transform.position);
                }
                
                m_cameraController.SetTrackedOffset(y: offset + -bot.transform.position.y);
            }
        }

        //====================================================================================================================//
        
        private LineRenderer _towLineRenderer;
        private void CreateTowEffect()
        {
            _towLineRenderer = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreateEffect(EffectFactory.EFFECT.LINE).GetComponent<LineRenderer>();

            _towLineRenderer.widthMultiplier = 0.2f;
            _towLineRenderer.startColor = _towLineRenderer.endColor = Color.gray;
        }

        private void UpdateTowLineRenderer(Vector3 botPosition, Vector3 recoveryDronePosition)
        {
            if (!_towLineRenderer)
                return;
            
            _towLineRenderer.SetPositions(new []
            {
                botPosition,
                recoveryDronePosition
            });
        }

        //====================================================================================================================//

        private void UpdateUIClock()
        {
            //Displays the time in timespan & the fill value
            var duration = CurrentWaveData.GetWaveDuration();
            var timeLeft = duration - m_waveTimer;
            
            GameUi.SetProgressValue(1f - timeLeft / duration);
            //GameUi.SetTimeString((int) timeLeft);
        }

        private bool BotIsInPosition()
        {
            var yPos = Constants.gridCellSize * Globals.GridSizeY;

            return m_bots[0].transform.position.y >= yPos && (ObstacleManager.RecoveredBotFalling == null || ObstacleManager.RecoveredBotFalling.transform.position.y > yPos);
        }

        //LevelManager Functions
        //====================================================================================================================//

        private void InitLevel()
        {
            AudioController.PlayTESTWaveMusic(Globals.CurrentWave, true);
            
            //--------------------------------------------------------------------------------------------------------//
            
            MissionsCompletedDuringThisFlight.Clear();
            
            BotDead = false;
            m_worldGrid = null;
            m_waveEndSummaryData = new WaveEndSummaryData();
            NumWavesInRow = 0;
            
            //Setup Bot
            //--------------------------------------------------------------------------------------------------------//
            
            m_bots.Add(FactoryManager.Instance.GetFactory<BotFactory>().CreateObject<Bot>());
            BotObject.transform.position = new Vector2(0, Constants.gridCellSize * 5);

            var botDataToLoad = PlayerDataManager.GetBlockDatas();

            if (botDataToLoad.Count == 0 || Globals.UsingTutorial)
            {
                BotObject.InitBot();
            }
            else
            {
                BotObject.InitBot(botDataToLoad.ImportBlockDatas(false));
            }
            
            BotObject.transform.parent = null;
            SceneManager.MoveGameObjectToScene(BotObject.gameObject, gameObject.scene);
            
            
            //Post Bot Setup
            //--------------------------------------------------------------------------------------------------------//

            InputManager.Instance.InitInput();
            InputManager.Instance.LockRotation = true;

            WaterAtBeginningOfWave = PlayerDataManager.GetResource(BIT_TYPE.BLUE).resource;

            SessionDataProcessor.Instance.StartNewWave(Globals.CurrentSector, Globals.CurrentWave, BotObject.GetBlockDatas());

            
            CameraController.SetOrthographicSize(Constants.gridCellSize * Globals.ColumnsOnScreen, BotObject.transform.position);
            if (Globals.Orientation == ORIENTATION.VERTICAL)
            {
                Globals.GridSizeY = (int)((CameraController.Camera.orthographicSize * Globals.GridHeightRelativeToScreen * 2) / Constants.gridCellSize);
            }
            else
            {
                Globals.GridSizeY = (int)((CameraController.Camera.orthographicSize * Globals.GridHeightRelativeToScreen * 2 * (Screen.width / (float)Screen.height)) / Constants.gridCellSize);
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

            /*if (PlayerPersistentData.PlayerData.firstFlight)
            {
                PlayerPersistentData.PlayerData.firstFlight = false;
                Toast.AddToast(
                    "<b>Move: AD or Left/Right\nRotate: WS or Up/Down</b>",
                    time: 10.0f,
                    verticalLayout: Toast.Layout.End,
                    horizontalLayout: Toast.Layout.Middle);
            }*/

            SetupLevelAnalytics();

            Random.InitState(CurrentWaveData.WaveSeed);
            Debug.Log("SET SEED " + CurrentWaveData.WaveSeed);

            SetBotBelowScreen();
            SetBotExitScreen(false);
            SetBotEnterScreen(true);

            //CheckPlayerWater();
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

            if (!Globals.IsRecoveryBot)
            {
                LiquidResourcesCachedOnDeath.Clear();
            }

            ObstacleManager.WorldElementsRoot.transform.position = Vector3.zero;

            m_waveEndSummaryData = null;
            BotDead = false;
            m_waveTimer = 0;

            if (!Globals.IsBetweenWavesInUniverseMap)
            {
                m_levelTimer = 0;
            }

            SetBotEnterScreen(false);
            SetBotExitScreen(false);

            CurrentWaveData.TrySetCurrentStage(m_waveTimer, out m_currentStage);
            ProjectileManager.Reset();
            MissionsCompletedDuringThisFlight.Clear();
            
            if(_towLineRenderer)
                Destroy(_towLineRenderer.gameObject);
        }

        public void ResetLevelTimer()
        {
            m_levelTimer = 0;
        }

        private void SetupLevelAnalytics()
        {
            Dictionary<int, float> tempResourceDictionary = new Dictionary<int, float>();
            foreach (BIT_TYPE _bitType in Enum.GetValues(typeof(BIT_TYPE)))
            {
                if (_bitType == BIT_TYPE.WHITE || _bitType == BIT_TYPE.NONE)
                    continue;

                tempResourceDictionary.Add((int)_bitType, PlayerDataManager.GetResource(_bitType).resource);
            }

            Dictionary<int, int> tempComponentDictionary = new Dictionary<int, int>();
            foreach (var component in PlayerDataManager.GetComponents())
            {
                tempComponentDictionary.Add((int)component.Key, component.Value);
            }

            Dictionary<string, object> levelStartAnalyticsDictionary = new Dictionary<string, object>
            {

            };
            string levelStartString = Globals.CurrentSector + "." + Globals.CurrentWave;
            AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelStart, eventDataDictionary: levelStartAnalyticsDictionary, eventDataParameter: levelStartString);
        }

        private void CheckPlayerWater()
        {
            var amount = PlayerDataManager.GetResource(BIT_TYPE.BLUE).resource;
            var required = Instance.CurrentWaveData.GetWaveDuration() * Constants.waterDrainRate;

            if (amount >= required)
                return;
            
            GameTimer.SetPaused(true);
            m_levelManagerUI.ShowSummaryScreen("Almost out of water",
                "You are nearly out of water at base. You will have to return home at the end of this wave with extra water.",
                () => { GameTimer.SetPaused(false); }
            );
        }
        
        //============================================================================================================//
        
        private void TransitionToEndWaveState()
        {
            if (BotDead || (BotObject != null && BotObject.Destroyed))
            {
                return;
            }
            
            SavePlayerData();

            //Unlock loot for completing wave
            ObstacleManager.IncreaseSpeedAllOffGridMoving(3.0f);
            NumWavesInRow++;

            MissionProgressEventData missionProgressEventData = new MissionProgressEventData
            {
                sectorNumber = Globals.CurrentSector + 1,
                waveNumber = Globals.CurrentWave + 1,
                intAmount = NumWavesInRow,
                floatAmount = LevelTimer
            };
            MissionManager.ProcessMissionData(typeof(LevelProgressMission), missionProgressEventData);
            MissionManager.ProcessMissionData(typeof(ChainWavesMission), missionProgressEventData);
            MissionManager.ProcessMissionData(typeof(FlightLengthMission), missionProgressEventData);

            WaveEndSummaryData.CompletedSector = Globals.CurrentSector;
            WaveEndSummaryData.CompletedWave = Globals.CurrentWave;
            WaveEndSummaryData.WaveEndTitle = $"Sector {Globals.CurrentSector + 1}.{Globals.CurrentWave + 1} Complete";

            int progressionSector = Globals.CurrentSector;
            string endWaveMessage;

            PlayerDataManager.ReduceLevelResourceModifier(Globals.CurrentSector, Globals.CurrentWave);

            endWaveMessage = "Wave Complete!";

            Toast.AddToast(endWaveMessage);

            EndWaveState = true;
            LevelManagerUI.OverrideText = string.Empty;
            m_levelTimer += m_waveTimer;
            m_waveTimer = 0;
            GameUi.SetCurrentWaveText("Complete");
            GameUi.ShowAbortWindow(false);
            EnemyManager.SetEnemiesInert(true);
            
            BotObject.SetSortingLayer(Actor2DBase.OVERLAY_LAYER, 10000);

            Random.InitState(CurrentWaveData.WaveSeed);
            Debug.Log("SET SEED " + CurrentWaveData.WaveSeed);

            if (!Globals.OnlyGetWaveLootOnce || !PlayerDataManager.CheckIfCompleted(progressionSector, Globals.CurrentWave))
            {
                UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
                /*CurrentWaveData.ConfigureLootTable();
                List<IRDSObject> newWaveLoot = CurrentWaveData.rdsTable.rdsResult.ToList();
                DropLoot(newWaveLoot, -ObstacleManager.WorldElementsRoot.transform.position + Vector3.up * (10 * Constants.gridCellSize), false);*/

                SectorLootTableScriptableObject sectorLootTable = FactoryManager.Instance.SectorRemoteData[Globals.CurrentSector].sectorRemoteDataLootTablesScriptable.GetLootTableAtIndex(PlayerDataManager.NumTimesGottenLootTableInSector[Globals.CurrentSector]);
                if (sectorLootTable != null)
                {
                    List<LevelRingNode> childNodesAccessible = PlayerDataManager.GetLevelRingNodeTree().TryFindNode(PlayerDataManager.GetLevelRingNodeTree().ConvertSectorWaveToNodeIndex(Globals.CurrentSector, Globals.CurrentWave)).childNodes;
                    if (childNodesAccessible.Count == 0 || UnityEngine.Random.Range(0.0f, 1.0f) <= 0.33f)
                    {
                        sectorLootTable.ConfigureLootTable();
                        List<IRDSObject> newWaveLoot = sectorLootTable.rdsTable.rdsResult.ToList();
                        DropLoot(newWaveLoot, -ObstacleManager.WorldElementsRoot.transform.position + Vector3.up * (10 * Constants.gridCellSize), false);
                        PlayerDataManager.NumTimesGottenLootTableInSector[Globals.CurrentSector]++;
                    }
                }
            }

            int curNodeIndex = PlayerDataManager.GetLevelRingNodeTree().ConvertSectorWaveToNodeIndex(Globals.CurrentSector, Globals.CurrentWave);
            if (!PlayerDataManager.GetPlayerPreviouslyCompletedNodes().Contains(curNodeIndex))
            {
                PlayerDataManager.AddCompletedNode(curNodeIndex);
            }

            if (Globals.IsRecoveryBot)
            {
                ScrapyardBot scrapyardBot = FactoryManager.Instance.GetFactory<BotFactory>().CreateScrapyardObject<ScrapyardBot>();
                Globals.IsRecoveryBot = false;
                var currentBlockData = PlayerDataManager.GetBlockDatas();
                Globals.IsRecoveryBot = true;
                //Checks to make sure there is a core on the bot
                if (currentBlockData.Count == 0 || !currentBlockData.Any(x => x.ClassType.Contains(nameof(Part)) && x.Type == (int)PART_TYPE.CORE))
                {
                    scrapyardBot.InitBot();
                }
                else
                {
                    Globals.IsRecoveryBot = false;
                    var importedData = currentBlockData.ImportBlockDatas(true);
                    scrapyardBot.InitBot(importedData);
                    Globals.IsRecoveryBot = true;
                }
                if (!Globals.RecoveryOfDroneLocksHorizontalMovement)
                {
                    scrapyardBot.transform.parent = m_obstacleManager.WorldElementsRoot;
                }
                scrapyardBot.transform.position = m_bots[0].transform.position + (Vector3.up * (Globals.GridSizeY * Constants.gridCellSize));
                ObstacleManager.RecoveredBotFalling = scrapyardBot.gameObject;
            }

            for (int i = 0; i < m_bots.Count; i++)
            {
                m_bots[i].SetColliderActive(false);
            }
        }


        //====================================================================================================================//

        private GameObject _effect;

        private void CreateThrustEffect(Bot bot)
        {
            if (_effect != null)
                return;

            if (bot.Rotating)
            {
                bot.ForceCompleteRotation();
            }
            
            var lowestCoordinate =
                bot.attachedBlocks.GetAttachableInDirection(Vector2Int.zero, DIRECTION.DOWN).Coordinate;

            var localPosition = bot.transform.position + (Vector3)(lowestCoordinate + DIRECTION.DOWN.ToVector2() / 2f);
            
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>().CreateEffect(EffectFactory.EFFECT.THRUST);
            var effectTransform = effect.transform;
            effectTransform.SetParent(bot.transform);
            effectTransform.localPosition = bot.transform.InverseTransformPoint(localPosition);

            _effect = effect;
        }
        
        
        //====================================================================================================================//

        [SerializeField]
        private AnimationCurve enterCurve;
        

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
                CreateThrustEffect(BotObject);
                BotObject.PROTO_GodMode = true;
            }
            else if (_effect)
            {
                InputManager.Instance.LockRotation = false;
                Destroy(_effect);
            }
        }

        public void SetBotExitScreen(bool value)
        {
            if (BotDead || (BotObject != null && BotObject.Destroyed))
            {
                return;
            }

            m_botZoomOffScreen = value;
            
            if (!value)
            {
                botMoveOffScreenSpeed = 1.0f;
            }

            if (value && !_effect)
            {
                AudioController.FadeOutMusic();
                AudioController.PlaySound(SOUND.BOT_DEPARTS);
                CreateThrustEffect(BotObject);
            }
            else if (!value && _effect)
            {
                Destroy(_effect);
            }
        }

        //FIXME Does this need to be in the LevelManager?
        public void DropLoot(List<IRDSObject> loot, Vector3 position, bool isFromEnemyLoot)
        {
            for (int i = loot.Count - 1; i >= 0; i--)
            {
                switch (loot[i])
                {
                    case RDSValue<(BIT_TYPE, int)> rdsValueResourceRefined:
                        PlayerDataManager.GetResource(rdsValueResourceRefined.rdsValue.Item1).AddResource(rdsValueResourceRefined.rdsValue.Item2);
                        loot.RemoveAt(i);
                        break;
                    case RDSValue<Blueprint> rdsValueBlueprint:
                        PlayerDataManager.UnlockBlueprint(rdsValueBlueprint.rdsValue);
                        Toast.AddToast("Unlocked Blueprint!");
                        loot.RemoveAt(i);
                        break;
                    case RDSValue<FacilityBlueprint> rdsValueFacilityBlueprint:
                        PlayerDataManager.UnlockFacilityBlueprintLevel(rdsValueFacilityBlueprint.rdsValue);
                        Toast.AddToast("Unlocked Facility Blueprint!");
                        loot.RemoveAt(i);
                        break;
                    case RDSValue<Vector2Int> rdsValueGears:
                    {
                        var gears = Random.Range(rdsValueGears.rdsValue.x, rdsValueGears.rdsValue.y);
                        PlayerDataManager.ChangeGears(gears);
                        loot.RemoveAt(i);
                        
                        FloatingText.Create($"+{gears}", position, Color.white);
                        
                        break;
                    }
                    case RDSValue<BlockData> rdsValueBlockData:
                    {
                        if (EndWaveState)
                        {
                            switch (rdsValueBlockData.rdsValue.ClassType)
                            {
                                case nameof(Component):
                                    PlayerDataManager.AddComponent((COMPONENT_TYPE)rdsValueBlockData.rdsValue.Type, 1);
                                    loot.RemoveAt(i);
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    }
                }
            }

            ObstacleManager.SpawnObstacleExplosion(position, loot, isFromEnemyLoot);
        }

        public void SavePlayerData()
        {
            if (Globals.UsingTutorial)
                return;
            
            foreach (Bot bot in m_bots)
            {
                var blockData = bot.GetBlockDatas();
                if (!blockData.Any(x => x.ClassType.Contains(nameof(Part)) && x.Type == (int)PART_TYPE.CORE))
                    blockData = new List<BlockData>();

                PlayerDataManager.SetBlockData(blockData);
            }
        }

        public void RestartLevel()
        {
            m_levelManagerUI.ToggleDeathUIActive(false, string.Empty);
            GameUi.SetCurrentWaveText(Globals.CurrentSector + 1, Globals.CurrentWave + 1);
            GameTimer.SetPaused(false);

            ScreenFade.Fade(() =>
            {
                Globals.IsRecoveryBot = true;
                GameUi.ShowRecoveryBanner(true);
                SceneLoader.ActivateScene(SceneLoader.LEVEL, SceneLoader.LEVEL);
            });
            
        }

        private void OnBotDied(Bot _, string deathMethod)
        {
            LiquidResourcesCachedOnDeath = new Dictionary<BIT_TYPE, float>();
            PlayerDataManager.AddCoreDeath();

            foreach (BIT_TYPE _bitType in Enum.GetValues(typeof(BIT_TYPE)))
            {
                if (_bitType == BIT_TYPE.WHITE || _bitType == BIT_TYPE.NONE)
                    continue;

                LiquidResourcesCachedOnDeath.Add(_bitType, PlayerDataManager.GetResource(_bitType).liquid);
            }

            InputManager.Instance.CancelMove();
            InputManager.Instance.LockRotation = true;

            if (!Globals.IsRecoveryBot)
            {
                foreach (Bot bot in m_bots)
                {
                    IAttachable attachable = bot.attachedBlocks.First(a => a.Coordinate == Vector2.zero);
                    if (attachable is Part core)
                    {

                        core.SetupHealthValues(core.StartingHealth, core.StartingHealth / 2);
                    }
                }
            }

            SavePlayerData();
            BotDead = true;

            Dictionary<int, float> tempDictionary = new Dictionary<int, float>();
            foreach (BIT_TYPE _bitType in Enum.GetValues(typeof(BIT_TYPE)))
            {
                if (_bitType == BIT_TYPE.WHITE || _bitType == BIT_TYPE.NONE)
                    continue;

                tempDictionary.Add((int)_bitType, PlayerDataManager.GetResource(_bitType).liquid);
            }

            Dictionary<string, object> levelLostAnalyticsDictionary = new Dictionary<string, object>
            {
                {AnalyticsManager.DeathCause, deathMethod},
                {AnalyticsManager.LevelTime, m_levelTimer + m_waveTimer},
            };
            string levelLostString = Globals.CurrentSector + "." + Globals.CurrentWave;
            AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelLost,
                eventDataDictionary: levelLostAnalyticsDictionary, eventDataParameter: levelLostString);

            SessionDataProcessor.Instance.PlayerKilled();
            SessionDataProcessor.Instance.EndActiveWave();

            if (!Globals.IsRecoveryBot)
            {
                IsWaveProgressing = false;

                Alert.ShowAlert("Bot wrecked",
                    "Your bot has been wrecked. Deploy your recovery bot to rescue it.",
                    "Deploy",
                    () =>
                    {
                        IsWaveProgressing = true;
                        RestartLevel();
                    });

                //m_levelManagerUI.ToggleDeathUIActive(true, deathMethod);
            }
            else
            {
                //Alert.ShowDancers(true);
                //AudioController.PlayMusic(MUSIC.GAME_OVER, true);

                IsWaveProgressing = false;
                //GameTimer.SetPaused(false);

                OutroScene.gameObject.SetActive(true);
            }
        }

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
            GameUi.SetCurrentWaveText(Globals.CurrentSector + 1, Globals.CurrentWave + 1);
        }

        public void OnPause()
        {

        }

        #endregion //IPausable

        //Analytics Functions
        //============================================================================================================//

        #region Analytics Functions

        public void ProcessScrapyardUsageBeginAnalytics()
        {
            Dictionary<string, object> scrapyardUsageBeginAnalyticsDictionary = new Dictionary<string, object>
            {
                {"Sector Number", Globals.CurrentSector}
            };
            //AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.ScrapyardUsageBegin, scrapyardUsageBeginAnalyticsDictionary);
        }

        private void ProcessLevelCompleteAnalytics()
        {
            Dictionary<string, object> levelCompleteAnalyticsDictionary = new Dictionary<string, object>
            {
                {"Level Time", m_levelTimer + m_waveTimer}
            };
            //AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelComplete, levelCompleteAnalyticsDictionary, Values.Globals.CurrentSector);
        }

        #endregion //Analytics Functions

        //====================================================================================================================//

        public void ForceSetTimeRemaining(float timeLeft)
        {
            m_waveTimer = CurrentWaveData.GetWaveDuration() - timeLeft;
        }

        //Unity Editor
        //====================================================================================================================//

        #region Unity Editor

#if UNITY_EDITOR

        [SerializeField]
        private bool drawGrid = true;

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

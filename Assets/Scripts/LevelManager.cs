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
using System.Linq;
using StarSalvager.Utilities.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarSalvager.Missions;
using StarSalvager.Utilities.JsonDataTypes;
using Newtonsoft.Json;

namespace StarSalvager
{
    public class LevelManager : SceneSingleton<LevelManager>, IReset, IPausable
    {
        public bool generateRandomSeed;
        [DisableIf("$generateRandomSeed")] public int seed = 1234567890;

        private List<Bot> m_bots;
        public Bot BotGameObject => m_bots[0];

        [SerializeField]
        private CameraController m_cameraController;
        public CameraController CameraController => m_cameraController;

        public SectorRemoteDataScriptableObject CurrentSector => FactoryManager.Instance.SectorRemoteData[Values.Globals.CurrentSector];

        public WaveRemoteDataScriptableObject CurrentWaveData => CurrentSector.GetRemoteData(Globals.CurrentWave);

        private float m_waveTimer;
        public float WaveTimer => m_waveTimer;

        public bool IsWaveProgressing = true;

        private float m_levelTimer = 0;

        private int m_currentStage;
        public int CurrentStage => m_currentStage;

        public bool EndWaveState = false;

        private LevelManagerUI m_levelManagerUI;

        public bool isPaused => GameTimer.IsPaused;

        public WorldGrid WorldGrid
        {
            get
            {
                if (m_worldGrid == null)
                    m_worldGrid = new WorldGrid();

                return m_worldGrid;
            }
        }
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

        public ProjectileManager ProjectileManager
        {
            get
            {
                if (m_projectileManager == null)
                    m_projectileManager = new ProjectileManager();

                return m_projectileManager;
            }
        }
        private ProjectileManager m_projectileManager;

        private GameUI GameUi
        {
            get
            {
                if (!_gameUi)
                    _gameUi = FindObjectOfType<GameUI>();

                return _gameUi;
            }
        }
        private GameUI _gameUi;

        public Dictionary<BIT_TYPE, float> LiquidResourcesAttBeginningOfWave = new Dictionary<BIT_TYPE, float>();
        public Dictionary<ENEMY_TYPE, int> EnemiesKilledInWave = new Dictionary<ENEMY_TYPE, int>();
        public List<string> MissionsCompletedDuringThisFlight = new List<string>();

        private void Start()
        {
            m_bots = new List<Bot>();

            if (generateRandomSeed)
            {
                seed = Random.Range(int.MinValue, int.MaxValue);
                Debug.Log($"Generated Seed {seed}");
            }

            RegisterPausable();
            m_levelManagerUI = FindObjectOfType<LevelManagerUI>();
            //m_levelManagerUI.SetCurrentWaveText((m_currentWave + 1).ToString() + "/" + CurrentSector.GetNumberOfWaves());

            GameUi.SetCurrentWaveText(Globals.CurrentSector + 1, Globals.CurrentWave + 1);

            Bot.OnBotDied += (deadBot, deathMethod) =>
            {
                Dictionary<int, float> tempDictionary = new Dictionary<int, float>();
                foreach (var resource in PlayerPersistentData.PlayerData.liquidResource)
                {
                    tempDictionary.Add((int)resource.Key, resource.Value);
                }

                Dictionary<string, object> botDiedAnalyticsDictionary = new Dictionary<string, object>();
                botDiedAnalyticsDictionary.Add("Death Cause", deathMethod);
                botDiedAnalyticsDictionary.Add("CurrentSector", Globals.CurrentSector);
                botDiedAnalyticsDictionary.Add("CurrentWave", Globals.CurrentWave);
                botDiedAnalyticsDictionary.Add("CurrentStage", m_currentStage);
                botDiedAnalyticsDictionary.Add("Level Time", m_levelTimer + m_waveTimer);
                botDiedAnalyticsDictionary.Add("Liquid Resource Current", JsonConvert.SerializeObject(tempDictionary, Formatting.None));
                botDiedAnalyticsDictionary.Add("Enemies Killed", JsonConvert.SerializeObject(EnemiesKilledInWave, Formatting.None));
                botDiedAnalyticsDictionary.Add("Missions Completed", JsonConvert.SerializeObject(MissionsCompletedDuringThisFlight, Formatting.None));
                AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.BotDied, eventDataDictionary: botDiedAnalyticsDictionary);

                PlayerPersistentData.PlayerData.numLives--;
                if (PlayerPersistentData.PlayerData.numLives > 0)
                {
                    foreach (var resource in LiquidResourcesAttBeginningOfWave)
                    {
                        PlayerPersistentData.PlayerData.SetLiquidResource(resource.Key, resource.Value);
                    }
                    IsWaveProgressing = false;
                    m_levelManagerUI.UpdateLivesText();
                    m_levelManagerUI.ToggleDeathUIActive(true, deathMethod);
                }
                else
                {
                    Alert.ShowAlert("GAME OVER", "Ran out of lives. Click to return to main menu.", "Ok", () =>
                    {
                        Globals.CurrentWave = 0;
                        GameTimer.SetPaused(false);
                        PlayerPersistentData.PlayerData.numLives = 3;
                        SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.ALEX_TEST_SCENE);
                    });
                }
                //Debug.LogError("Bot Died. Press 'R' to restart");
            };

            Random.InitState(seed);
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Y))
            {
                WorldGrid.DrawDebugMarkedGridPoints();
                Debug.Break();
            }

            if (isPaused)
                return;

            if (!EndWaveState)
            {
                if (IsWaveProgressing)
                    m_waveTimer += Time.deltaTime;

                m_currentStage = CurrentWaveData.GetCurrentStage(m_waveTimer);

                //Displays the time in timespan & the fill value
                var duration = CurrentWaveData.GetWaveDuration();
                var timeLeft = duration - m_waveTimer;
                GameUi.SetClockValue( timeLeft / duration);
                GameUi.SetTimeString((int)timeLeft);

                if (m_currentStage == -1)
                    TransitionToNewWave();
            }
            else if (ObstacleManager.HasNoActiveObstacles)
            {
                GameUi.SetClockValue(0f);
                GameUi.SetTimeString("0:00");
                SavePlayerData();
                GameTimer.SetPaused(true);
                m_levelManagerUI.ToggleBetweenWavesUIActive(true);
                ObstacleManager.MoveToNewWave();
                EnemyManager.MoveToNewWave();
                EnemyManager.SetEnemiesInert(false);
                EnemyManager.RecycleAllEnemies();
                m_currentStage = CurrentWaveData.GetCurrentStage(m_waveTimer);

                Dictionary<int, float> tempDictionary = new Dictionary<int, float>();
                foreach (var resource in PlayerPersistentData.PlayerData.liquidResource)
                {
                    tempDictionary.Add((int)resource.Key, resource.Value);
                }

                Dictionary<string, object> waveEndAnalyticsDictionary = new Dictionary<string, object>();
                waveEndAnalyticsDictionary.Add("Bot Layout", JsonConvert.SerializeObject(BotGameObject.GetBlockDatas(), Formatting.None));
                waveEndAnalyticsDictionary.Add("Liquid Resource Current", JsonConvert.SerializeObject(tempDictionary, Formatting.None));
                waveEndAnalyticsDictionary.Add("Enemies Killed", JsonConvert.SerializeObject(EnemiesKilledInWave, Formatting.None));
                AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.WaveEnd, eventDataDictionary: waveEndAnalyticsDictionary);

                EnemiesKilledInWave.Clear();
            }

            ProjectileManager.UpdateForces();
        }

        public void Activate()
        {
            m_worldGrid = null;
            m_bots.Add(FactoryManager.Instance.GetFactory<BotFactory>().CreateObject<Bot>());
            
            MissionsCompletedDuringThisFlight.Clear();
            LiquidResourcesAttBeginningOfWave.Clear();
            foreach (var resource in PlayerPersistentData.PlayerData.liquidResource)
            {
                LiquidResourcesAttBeginningOfWave.Add(resource.Key, resource.Value);
            }
            BotGameObject.transform.position = new Vector2(0, Constants.gridCellSize * 5);
            if (PlayerPersistentData.PlayerData.GetCurrentBlockData().Count == 0)
            {
                BotGameObject.InitBot();
            }
            else
            {
                print("Load from data");
                BotGameObject.InitBot(PlayerPersistentData.PlayerData.GetCurrentBlockData().ImportBlockDatas(false));
            }
            BotGameObject.transform.parent = null;
            SceneManager.MoveGameObjectToScene(BotGameObject.gameObject, gameObject.scene);

            InputManager.Instance.InitInput();
            CameraController.SetOrthographicSize(Constants.gridCellSize * Values.Globals.ColumnsOnScreen, BotGameObject.transform.position);
            if (Globals.Orientation == ORIENTATION.VERTICAL)
            {
                Values.Globals.GridSizeX = (int)(Values.Globals.ColumnsOnScreen * Values.Constants.GridWidthRelativeToScreen);
                Values.Globals.GridSizeY = (int)((Camera.main.orthographicSize * Values.Constants.GridHeightRelativeToScreen * 2) / Values.Constants.gridCellSize);
            }
            else
            {
                Values.Globals.GridSizeX = (int)(Values.Globals.ColumnsOnScreen * Values.Constants.GridWidthRelativeToScreen * (Screen.height / (float)Screen.width));
                Values.Globals.GridSizeY = (int)((Camera.main.orthographicSize * Values.Constants.GridHeightRelativeToScreen * 2 * (Screen.width / (float)Screen.height)) / Values.Constants.gridCellSize);
            }
            WorldGrid.SetupGrid();
            ProjectileManager.Activate();

            m_levelManagerUI.UpdateLivesText();
            m_levelManagerUI.ToggleDeathUIActive(false, string.Empty);
            GameTimer.SetPaused(false);

            if (PlayerPersistentData.PlayerData.firstFlight)
            {
                PlayerPersistentData.PlayerData.firstFlight = false;
                Toast.AddToast("Controls: AD or Left/Right arrows for left/right movement, WS or Up/Down arrows to rotate. Escape to pause.", time: 6.0f, verticalLayout: Toast.Layout.End, horizontalLayout: Toast.Layout.Middle);
            }

            Dictionary<int, float> tempDictionary = new Dictionary<int, float>();
            foreach (var resource in PlayerPersistentData.PlayerData.resources)
            {
                tempDictionary.Add((int)resource.Key, resource.Value);
            }

            Dictionary<string, object> flightBeginAnalyticsDictionary = new Dictionary<string, object>();
            flightBeginAnalyticsDictionary.Add("Stored Resource Current", JsonConvert.SerializeObject(tempDictionary, Formatting.None));
            flightBeginAnalyticsDictionary.Add("Stored Parts and Components", JsonConvert.SerializeObject(PlayerPersistentData.PlayerData.partsInStorageBlockData, Formatting.None));
            flightBeginAnalyticsDictionary.Add("Stored Parts and Components", JsonConvert.SerializeObject(PlayerPersistentData.PlayerData.components, Formatting.None));
            flightBeginAnalyticsDictionary.Add("Bot Layout", JsonConvert.SerializeObject(BotGameObject.GetBlockDatas(), Formatting.None));
            AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.FlightBegin, eventDataDictionary: flightBeginAnalyticsDictionary);
        }

        public void Reset()
        {
            for (int i = m_bots.Count - 1; i >= 0; i--)
            {
                if (m_bots == null)
                    continue;

                Recycling.Recycler.Recycle<Bot>(m_bots[i].gameObject);
                m_bots.RemoveAt(i);
            }
            m_waveTimer = 0;
            m_currentStage = CurrentWaveData.GetCurrentStage(m_waveTimer);
            ProjectileManager.Reset();
            MissionsCompletedDuringThisFlight.Clear();
        }

        private void TransitionToNewWave()
        {
            SavePlayerData();

            if (Globals.CurrentWave < CurrentSector.WaveRemoteData.Count - 1)
            {
                Toast.AddToast("Wave Complete!", time: 1.0f, verticalLayout: Toast.Layout.Middle, horizontalLayout: Toast.Layout.Middle);
                PlayerPersistentData.PlayerData.AddSectorProgression(Globals.CurrentSector, Globals.CurrentWave + 1);
                MissionManager.ProcessLevelProgressMissionData(Globals.CurrentSector + 1, Globals.CurrentWave + 1);
                MissionManager.ProcessChainWavesMissionData(Globals.CurrentWave + 1);
                MissionManager.ProcessFlightLengthMissionData(m_levelTimer);
                EndWaveState = true;
                Globals.CurrentWave++;
                m_levelTimer += m_waveTimer;
                m_waveTimer = 0;
                GameUi.SetCurrentWaveText("Complete");
                EnemyManager.SetEnemiesInert(true);
            }
            else
            {
                PlayerPersistentData.PlayerData.AddSectorProgression(Globals.CurrentSector + 1, 0);
                MissionManager.ProcessLevelProgressMissionData(Globals.CurrentSector + 1, Globals.CurrentWave + 1);
                MissionManager.ProcessChainWavesMissionData(Globals.CurrentWave + 1);
                MissionManager.ProcessSectorCompletedMissionData(Globals.CurrentSector + 1);
                MissionManager.ProcessFlightLengthMissionData(m_levelTimer);
                ProcessLevelCompleteAnalytics();
                ProcessScrapyardUsageBeginAnalytics();
                Globals.CurrentWave = 0;
                Globals.SectorComplete = true;
                SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.ALEX_TEST_SCENE);
            }
        }

        public void SavePlayerData()
        {
            foreach (Bot bot in m_bots)
            {
                //FIXME Need to avoid saving in the event that the bot was destroyed

                var blockData = bot.GetBlockDatas();
                if (!blockData.Any(x => x.ClassType.Contains(nameof(Part)) && x.Type == (int) PART_TYPE.CORE))
                    blockData = new List<BlockData>();
                
                PlayerPersistentData.PlayerData.SetCurrentBlockData(blockData);
            }
        }

        public void RestartLevel()
        {
            //Globals.CurrentWave = 0;
            m_levelManagerUI.ToggleDeathUIActive(false, string.Empty);
            //m_levelManagerUI.SetCurrentWaveText((m_currentWave + 1).ToString() + "/" + CurrentSector.GetNumberOfWaves());
            GameUi.SetCurrentWaveText(Globals.CurrentSector + 1, Globals.CurrentWave + 1);
            GameTimer.SetPaused(false);
            //AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelStart, eventDataParameter: Values.Globals.CurrentSector);
            SceneLoader.ActivateScene(SceneLoader.ALEX_TEST_SCENE, SceneLoader.ALEX_TEST_SCENE);
        }

        //============================================================================================================//

        public void RegisterPausable()
        {
            GameTimer.AddPausable(this);
        }

        public void OnResume()
        {
            //m_levelManagerUI.SetCurrentWaveText((m_currentWave + 1).ToString() + "/" + CurrentSector.GetNumberOfWaves());
            GameUi.SetCurrentWaveText(Globals.CurrentSector + 1, Globals.CurrentWave + 1);
        }

        public void OnPause()
        {

        }

        //============================================================================================================//

        public void ProcessScrapyardUsageBeginAnalytics()
        {
            Dictionary<string, object> scrapyardUsageBeginAnalyticsDictionary = new Dictionary<string, object>();
            scrapyardUsageBeginAnalyticsDictionary.Add("Sector Number", Values.Globals.CurrentSector);
            //AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.ScrapyardUsageBegin, scrapyardUsageBeginAnalyticsDictionary);
        }

        private void ProcessLevelCompleteAnalytics()
        {
            Dictionary<string, object> levelCompleteAnalyticsDictionary = new Dictionary<string, object>();
            levelCompleteAnalyticsDictionary.Add("Level Time", m_levelTimer + m_waveTimer);
            //AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelComplete, levelCompleteAnalyticsDictionary, Values.Globals.CurrentSector);
        }
    }
}

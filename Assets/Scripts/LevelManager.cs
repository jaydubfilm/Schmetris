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
using UnityEngine;
using UnityEngine.SceneManagement;

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

        [SerializeField]
        private List<SectorRemoteDataScriptableObject> m_sectorRemoteData;
        public SectorRemoteDataScriptableObject CurrentSector => m_sectorRemoteData[Values.Globals.CurrentSector];

        public WaveRemoteDataScriptableObject CurrentWaveData => CurrentSector.GetRemoteData(CurrentWave);

        private float m_waveTimer;
        public float WaveTimer => m_waveTimer;

        private float m_levelTimer = 0;

        private int m_currentStage;
        public int CurrentStage => m_currentStage;

        private int m_currentWave = 0;
        public int CurrentWave => m_currentWave;

        private bool m_endWaveState = false;
        public bool EndWaveState => m_endWaveState;

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

        private void Start()
        {
            m_bots = new List<Bot>();

            if (generateRandomSeed)
            {
                seed = Random.Range(int.MinValue, int.MaxValue);
                Debug.Log($"Generated Seed {seed}");
            }

            GameTimer.AddPausable(this);
            m_levelManagerUI = FindObjectOfType<LevelManagerUI>();
            m_levelManagerUI.SetCurrentWaveText((m_currentWave + 1).ToString() + "/" + CurrentSector.GetNumberOfWaves());

            Random.InitState(seed);
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Y))
            {
                MissionManager.ProcessEnemyKilledMissionData(ENEMY_TYPE.Enemy1, 1);
                //WorldGrid.DrawDebugMarkedGridPoints();
                //Debug.Break();
            }
            
            if (isPaused)
                return;

            if (!EndWaveState)
            {
                m_waveTimer += Time.deltaTime;
                m_currentStage = CurrentWaveData.GetCurrentStage(m_waveTimer);
                if (m_currentStage == -1)
                    TransitionToNewWave();
            }
            else if (ObstacleManager.HasNoActiveObstacles)
            {
                GameTimer.SetPaused(true);
                m_endWaveState = false;
                m_levelManagerUI.ToggleBetweenWavesUIActive(true);
                ObstacleManager.MoveToNewWave();
                EnemyManager.MoveToNewWave();
                m_currentStage = CurrentWaveData.GetCurrentStage(m_waveTimer);
            }
            
            ProjectileManager.UpdateForces();
        }

        public void Activate()
        {
            m_worldGrid = null;
            m_bots.Add(FactoryManager.Instance.GetFactory<BotFactory>().CreateObject<Bot>());
            BotGameObject.transform.position = new Vector2(0, Constants.gridCellSize * 5);
            if (PlayerPersistentData.PlayerData.GetCurrentBlockData().Count == 0)
            {
                BotGameObject.InitBot();
            }
            else
            {
                BotGameObject.InitBot(PlayerPersistentData.PlayerData.GetCurrentBlockData().ImportBlockDatas(false));
            }
            Bot.OnBotDied += (deadBot, deathMethod) =>
            {
                GameTimer.SetPaused(true);
                AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.BotDied);
                Dictionary<string, object> levelLostAnalyticsDictionary = new Dictionary<string, object>();
                levelLostAnalyticsDictionary.Add("CurrentSector", Values.Globals.CurrentSector);
                levelLostAnalyticsDictionary.Add("CurrentWave", m_currentWave);
                levelLostAnalyticsDictionary.Add("CurrentStage", m_currentStage);
                levelLostAnalyticsDictionary.Add("Level Time", m_levelTimer + m_waveTimer);
                AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelLost, eventDataDictionary: levelLostAnalyticsDictionary);
                m_levelManagerUI.ToggleDeathUIActive(true, deathMethod);
                //Debug.LogError("Bot Died. Press 'R' to restart");
            };
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

            GameTimer.SetPaused(false);
            m_levelManagerUI.ToggleDeathUIActive(false, string.Empty);
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
        }

        private void TransitionToNewWave()
        {
            SavePlayerData();

            if (m_currentWave >= 2 && Values.Globals.CurrentSector == Values.Globals.MaxSector)
            {
                Values.Globals.MaxSector++;
            }

            if (m_currentWave < CurrentSector.WaveRemoteData.Count - 1)
            {
                m_endWaveState = true;
                m_currentWave++;
                m_levelTimer += m_waveTimer;
                m_waveTimer = 0;
                m_levelManagerUI.SetCurrentWaveText(m_currentWave.ToString() + " Complete");
            }
            else
            {
                ProcessLevelCompleteAnalytics();
                ProcessScrapyardUsageBeginAnalytics();
                m_currentWave = 0;
                StarSalvager.SceneLoader.SceneLoader.ActivateScene("ScrapyardScene", "AlexShulmanTestScene");
            }
        }

        public void SavePlayerData()
        {
            foreach (Bot bot in m_bots)
            {
                PlayerPersistentData.PlayerData.SetCurrentBlockData(bot.GetBlockDatas());
            }
        }

        public void RestartLevel()
        {
            m_currentWave = 0;
            m_levelManagerUI.ToggleDeathUIActive(false, string.Empty);
            m_levelManagerUI.SetCurrentWaveText((m_currentWave + 1).ToString() + "/" + CurrentSector.GetNumberOfWaves());
            GameTimer.SetPaused(false);
            AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelStart, eventDataParameter: Values.Globals.CurrentSector);
            SceneLoader.SceneLoader.ActivateScene("AlexShulmanTestScene", "AlexShulmanTestScene");
        }

        //============================================================================================================//

        public void OnResume()
        {
            m_levelManagerUI.SetCurrentWaveText((m_currentWave + 1).ToString() + "/" + CurrentSector.GetNumberOfWaves());
        }

        public void OnPause()
        {

        }

        //============================================================================================================//

        public void ProcessScrapyardUsageBeginAnalytics()
        {
            Dictionary<string, object> scrapyardUsageBeginAnalyticsDictionary = new Dictionary<string, object>();
            scrapyardUsageBeginAnalyticsDictionary.Add("Sector Number", Values.Globals.CurrentSector);
            AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.ScrapyardUsageBegin, scrapyardUsageBeginAnalyticsDictionary);
        }

        private void ProcessLevelCompleteAnalytics()
        {
            Dictionary<string, object> levelCompleteAnalyticsDictionary = new Dictionary<string, object>();
            levelCompleteAnalyticsDictionary.Add("Level Time", m_levelTimer + m_waveTimer);
            AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelComplete, levelCompleteAnalyticsDictionary, Values.Globals.CurrentSector);
        }
    }
}

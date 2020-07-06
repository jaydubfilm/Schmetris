using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.Utilities;
using StarSalvager.Values;
using StarSalvager.AI;
using UnityEngine.UI;
using StarSalvager.ScriptableObjects;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Utilities.Inputs;
using UnityEngine.SceneManagement;
using StarSalvager.Cameras.Data;

namespace StarSalvager
{
    public class LevelManager : SceneSingleton<LevelManager>, IReset
    {
        public bool generateRandomSeed;
        [DisableIf("$generateRandomSeed")] public int seed = 1234567890;

        private List<Bot> m_bots;
        public Bot BotGameObject => m_bots[0];

        [SerializeField]
        private CameraController m_cameraController;
        public CameraController CameraController => m_cameraController;

        [SerializeField]
        private List<WaveRemoteDataScriptableObject> m_waveRemoteData;
        public WaveRemoteDataScriptableObject CurrentWaveData => m_waveRemoteData[CurrentWave];

        [SerializeField]
        private Button m_scrapyardButton;
        [SerializeField]
        private Button m_menuButton;

        private float m_waveTimer;
        public float WaveTimer => m_waveTimer;

        private int m_currentStage;
        public int CurrentStage => m_currentStage;

        private int m_currentWave = 0;
        public int CurrentWave => m_currentWave;

        private bool m_started = false;

        [SerializeField]
        private Canvas m_pauseCanvas;

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

            m_scrapyardButton.onClick.AddListener(ScrapyardButtonPressed);
            m_menuButton.onClick.AddListener(MenuButtonPressed);

            Random.InitState(seed);
        }

        private void Update()
        {
            m_waveTimer += Time.deltaTime;
            m_currentStage = CurrentWaveData.GetCurrentStage(m_waveTimer);
            if (m_currentStage == -1)
                TransitionToNewWave();
            
            ProjectileManager.UpdateForces();
        }

        public void Activate()
        {
            m_bots.Add(FactoryManager.Instance.GetFactory<BotFactory>().CreateObject<Bot>());
            BotGameObject.transform.position = new Vector2(0, 0);
            BotGameObject.InitBot();
            Bot.OnBotDied += deadBot =>
            {
                Debug.LogError("Bot Died. Press 'R' to restart");
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
        }

        public void Reset()
        {
            m_worldGrid = null;
            for (int i = m_bots.Count - 1; i >= 0; i--)
            {
                Recycling.Recycler.Recycle<Bot>(m_bots[i].gameObject);
                m_bots.RemoveAt(i);
            }
            m_waveTimer = 0;
            m_currentStage = CurrentWaveData.GetCurrentStage(m_waveTimer);
            ProjectileManager.Reset();
        }

        private void TransitionToNewWave()
        {
            print("NEWWAVE");
            if (m_currentWave < m_waveRemoteData.Count - 1)
            {
                //offer trip to scrapyard
                print("TRANSITION TO NEXT WAVE");

                Time.timeScale = 0;

                m_currentWave++;
                m_waveTimer = 0;
                m_currentStage = CurrentWaveData.GetCurrentStage(m_waveTimer);
            }
            else
            {
                Time.timeScale = 0;
                
                print("GO TO SCRAPYARD");
                //Go to scrapyard
            }
        }

        private void ScrapyardButtonPressed()
        {
            StarSalvager.SceneLoader.SceneLoader.ActivateScene("ScrapyardScene", "AlexShulmanTestScene");
        }

        private void MenuButtonPressed()
        {
            StarSalvager.SceneLoader.SceneLoader.ActivateScene("MainMenuScene", "AlexShulmanTestScene");
        }
    }
}

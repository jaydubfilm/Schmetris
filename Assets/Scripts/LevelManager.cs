﻿using System.Collections;
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

namespace StarSalvager
{
    public class LevelManager : SceneSingleton<LevelManager>
    {
        public bool generateRandomSeed;
        [DisableIf("$generateRandomSeed")] public int seed = 1234567890;

        private List<Bot> m_bots;
        public Bot BotGameObject => m_bots[0];

        [SerializeField]
        private CameraController m_cameraController;
        public CameraController CameraController => m_cameraController;

        [SerializeField]
        private WaveRemoteDataScriptableObject m_waveRemoteData;
        public WaveRemoteDataScriptableObject WaveRemoteData => m_waveRemoteData;

        [SerializeField]
        private Button m_scrapyardButton;
        [SerializeField]
        private Button m_menuButton;

        private float m_waveTimer;
        public float WaveTimer => m_waveTimer;

        private int m_currentStage;
        public int CurrentStage => m_currentStage;

        private bool m_started = false;

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
            if (generateRandomSeed)
            {
                seed = Random.Range(int.MinValue, int.MaxValue);
                Debug.Log($"Generated Seed {seed}");
            }

            m_scrapyardButton.onClick.AddListener(ScrapyardButtonPressed);
            m_menuButton.onClick.AddListener(MenuButtonPressed);

            Random.InitState(seed);

            m_bots = new List<Bot>();
            m_bots.Add (FactoryManager.Instance.GetFactory<BotFactory>().CreateObject<Bot>());
            BotGameObject.transform.position = new Vector2(0, 0);
            BotGameObject.InitBot();
            Bot.OnBotDied += deadBot =>
            {
                Debug.LogError("Bot Died. Press 'R' to restart");
            };
            SceneManager.MoveGameObjectToScene(BotGameObject.gameObject, gameObject.scene);

            InputManager.Instance.InitInput();
            CameraController.SetOrthographicSize(Constants.gridCellSize * Values.Globals.ColumnsOnScreen, BotGameObject.transform.position);
            Values.Globals.GridSizeX = (int)(Values.Globals.ColumnsOnScreen * Constants.GridWidthRelativeToScreen);
            Values.Globals.GridSizeY = (int)((Camera.main.orthographicSize * Constants.GridHeightRelativeToScreen * 2) / Constants.gridCellSize);
            WorldGrid.SetupGrid();
            m_started = true;
        }
        
        //TODO: Review whether this is the proper way to handle things that should happen on scene activation
        private void OnEnable()
        {
            if (m_started)
            {
                InputManager.Instance.InitInput();
                CameraController.SetOrthographicSize(Constants.gridCellSize * Values.Globals.ColumnsOnScreen, BotGameObject.transform.position);
                Values.Globals.GridSizeX = (int)(Values.Globals.ColumnsOnScreen * Constants.GridWidthRelativeToScreen);
                Values.Globals.GridSizeY = (int)((Camera.main.orthographicSize * Constants.GridHeightRelativeToScreen * 2) / Constants.gridCellSize);
                WorldGrid.SetupGrid();
            }
        }

        private void Update()
        {
            m_waveTimer += Time.deltaTime;
            m_currentStage = m_waveRemoteData.GetCurrentStage(m_waveTimer);
            
            ProjectileManager.UpdateForces();
        }

        private void OnDisable()
        {
            //m_worldGrid = null;
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

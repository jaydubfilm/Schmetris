using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.Utilities;
using StarSalvager.Constants;
using StarSalvager.AI;
using UnityEngine.UI;

namespace StarSalvager
{
    public class LevelManager : Singleton<LevelManager>
    {
        public static LevelManager Instance => m_instance;
        private static LevelManager m_instance;

        [SerializeField]
        private Bot m_botGameObject;
        public Bot BotGameObject => m_botGameObject;

        [SerializeField]
        private Bit m_bitTestPrefab;
        public Bit BitTestPrefab => m_bitTestPrefab;
        
        [SerializeField]
        private Text m_demoText;
        public Text DemoText => m_demoText;

        public new WorldGrid WorldGrid
        {
            get
            {
                if (m_worldGrid == null)
                    m_worldGrid = new WorldGrid();

                return m_worldGrid;
            }
        }
        private WorldGrid m_worldGrid;

        public new AIObstacleAvoidance AIObstacleAvoidance
        {
            get
            {
                if (m_AIObstacleAvoidance == null)
                    m_AIObstacleAvoidance = gameObject.GetComponent<AIObstacleAvoidance>();

                return m_AIObstacleAvoidance;
            }
        }
        private AIObstacleAvoidance m_AIObstacleAvoidance;

        private new EnemyManager EnemyManager
        {
            get
            {
                if (m_enemyManager == null)
                    m_enemyManager = gameObject.GetComponent<EnemyManager>();

                return m_enemyManager;
            }
        }
        private EnemyManager m_enemyManager;

        public new ObstacleManager ObstacleManager
        {
            get
            {
                if (m_obstacleManager == null)
                    m_obstacleManager = gameObject.GetComponent<ObstacleManager>();

                return m_obstacleManager;
            }
        }
        private ObstacleManager m_obstacleManager;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.Log($"An instance of LevelManager already exists.");
            }

            m_instance = this;
        }
    }
}

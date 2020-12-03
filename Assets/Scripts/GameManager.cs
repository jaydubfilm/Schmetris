using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;
using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    //FIXME We'll likely want to make use of enum flags here
    public enum GameState
    {
        MainMenu,
        AccountMenu,
        Scrapyard,
        UniverseMapBeforeFlight,
        UniverseMapBetweenWaves,
        LevelActive,
        LevelActiveEndSequence,
        LevelEndWave,
        LevelBotDead
    }
    
    //Don't need to set this, Singleton already triggers [DefaultExecutionOrder]
    //[DefaultExecutionOrder(-10000)]
    public class GameManager : Singleton<GameManager>
    {
        //FIXME The game state can likely be stored as static, since we don't gain anything by accessing it through the instance
        private GameState m_currentGameState = GameState.MainMenu;
        
        [SerializeField, Required]
        private GameSettingsScriptableObject m_gameSettings;


#if UNITY_EDITOR
        public void OnValidate()
        {
            m_gameSettings.SetupGameSettings();
        }
#endif

        public void Start()
        {
            m_gameSettings.SetupGameSettings();
        }

        public GameState GetCurrentGameState()
        {
            return m_currentGameState;
        }

        public void SetCurrentGameState(GameState newGameState)
        {
            m_currentGameState = newGameState;
        }

        public bool IsLevel()
        {
            return m_currentGameState == GameState.LevelActive || m_currentGameState == GameState.LevelActiveEndSequence || m_currentGameState == GameState.LevelEndWave || m_currentGameState == GameState.LevelBotDead;
        }

        public bool IsLevelActive()
        {
            return m_currentGameState == GameState.LevelActive || m_currentGameState == GameState.LevelActiveEndSequence;
        }

        public bool IsLevelActiveEndSequence()
        {
            return m_currentGameState == GameState.LevelActiveEndSequence;
        }

        public bool IsLevelEndWave()
        {
            return m_currentGameState == GameState.LevelEndWave;
        }

        public bool IsLevelBotDead()
        {
            return m_currentGameState == GameState.LevelBotDead;
        }

        public bool IsUniverseMapBetweenWaves()
        {
            return m_currentGameState == GameState.UniverseMapBetweenWaves;
        }
    }
}
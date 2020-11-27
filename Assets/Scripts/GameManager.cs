using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;
using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public enum GameState
    {
        MainMenu,
        AccountMenu,
        Scrapyard,
        UniverseMapBeforeFlight,
        UniverseMapDuringFlight,
        LevelAlive,
        LevelLost
    }
    
    [DefaultExecutionOrder(-10000)]
    public class GameManager : Singleton<GameManager>
    {
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

        public bool IsInLevel()
        {
            return m_currentGameState == GameState.LevelAlive || m_currentGameState == GameState.LevelLost;
        }

        public bool IsBetweenWavesUniverseMap()
        {
            return m_currentGameState == GameState.UniverseMapDuringFlight;
        }
    }
}
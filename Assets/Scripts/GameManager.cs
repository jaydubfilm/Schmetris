using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;
using StarSalvager.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    //FIXME We'll likely want to make use of enum flags here
    [Flags]
    public enum GameState
    {
        MainMenu = 0,
        AccountMenu = 1 << 0,
        Scrapyard = 1 << 1,
        UniverseMapBeforeFlight = 1 << 2,
        UniverseMapBetweenWaves = 1 << 3,
        LevelActive = 1 << 4,
        LevelActiveEndSequence = 1 << 5,
        LevelEndWave = 1 << 6,
        LevelBotDead = 1 << 7,

        LEVEL_ACTIVE = LevelActive | LevelActiveEndSequence,
        LEVEL = LEVEL_ACTIVE | LevelEndWave | LevelBotDead,
    }
    
    //Don't need to set this, Singleton already triggers [DefaultExecutionOrder]
    //[DefaultExecutionOrder(-10000)]
    public class GameManager : Singleton<GameManager>
    {
        //FIXME The game state can likely be stored as static, since we don't gain anything by accessing it through the instance
        private static GameState m_currentGameState = GameState.MainMenu;
        
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

        public static bool IsState(GameState gameState)
        {
            return m_currentGameState.HasFlag(gameState);
        }

        /*public void SetCurrentGameState(GameState newGameState)
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
        }*/
    }
}
using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;
using StarSalvager.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Analytics.SessionTracking;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Saving;
using UnityEngine;

namespace StarSalvager
{
    [Flags]
    public enum GameState
    {
        MainMenu = 1 << 0,
        AccountMenu = 1 << 1,
        Wreckyard = 1 << 2,
        UniverseMap = 1 << 3,
        LevelActive = 1 << 4,
        LevelActiveEndSequence = 1 << 5, // After the timer is over, before the level properly ends
        LevelEndWave = 1 << 6,
        LevelBotDead = 1 << 7,

        LEVEL_ACTIVE = LevelActive | LevelActiveEndSequence,
        LEVEL = LEVEL_ACTIVE | LevelEndWave | LevelBotDead,

    }
    
    //Don't need to set this, Singleton already triggers [DefaultExecutionOrder]
    //[DefaultExecutionOrder(-10000)]
    public class GameManager : Singleton<GameManager>
    {
        //Properties
        //====================================================================================================================//
        
        public static GameState CurrentGameState => m_currentGameState;
        //FIXME The game state can likely be stored as static, since we don't gain anything by accessing it through the instance
        private static GameState m_currentGameState = GameState.MainMenu;
        
        [SerializeField, Required]
        private GameSettingsScriptableObject m_gameSettings;

        //Unity Functions
        //====================================================================================================================//
        
#if UNITY_EDITOR
        public GameSettingsScriptableObject GameSettings => m_gameSettings;
        public void OnValidate()
        {
            m_gameSettings.SetupGameSettings();
        }
#endif

        public void Start()
        {
            m_gameSettings.SetupGameSettings();
            AnalyticsManager.ApplicationStartEvent();
        }

        private void OnApplicationQuit()
        {
            SessionDataProcessor.ExportSessionData();

            Debug.Log($"{nameof(InputManager)} called {nameof(OnApplicationQuit)}");

            var timePlayed = GameTimer.GetTimePlayed;
            
            if (!IsState(GameState.MainMenu))
            {
                PlayerDataManager.SavePlayerAccountData();
            }

            if (IsState(GameState.LEVEL))
            {
                AnalyticsManager.WaveEndEvent(AnalyticsManager.REASON.QUIT);
            }
            else if (IsState(GameState.Wreckyard))
            {
                AnalyticsManager.WreckEndEvent(AnalyticsManager.REASON.QUIT);
            }
            
            AnalyticsManager.ApplicationEndEvent();
        }

        //Game Manager Functions
        //====================================================================================================================//
        

        public static bool IsState(GameState gameState)
        {
            switch(gameState)
            {
                case GameState.LEVEL:
                    return GameState.LEVEL.HasFlag(m_currentGameState);
                case GameState.LEVEL_ACTIVE:
                    return GameState.LEVEL_ACTIVE.HasFlag(m_currentGameState);
                default:
                    return m_currentGameState.HasFlag(gameState);
            }
        }
        
        public static bool ContainsState(in GameState gameState)
        {
            //Solution from: https://stackoverflow.com/a/1340003
           return (m_currentGameState & gameState) != 0;
        }

        public static void SetCurrentGameState(GameState newGameState)
        {
            m_currentGameState = newGameState;
        }
    }
}
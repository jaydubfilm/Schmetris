﻿using System;
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
using Newtonsoft.Json;
using StarSalvager.Audio;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Particles;
using Random = UnityEngine.Random;
using UnityEngine.Analytics;

namespace StarSalvager
{
    public class LevelManager : SceneSingleton<LevelManager>, IReset, IPausable
    {
        private List<Bot> m_bots;
        public Bot BotObject => m_bots[0];

        [SerializeField]
        private CameraController m_cameraController;
        public CameraController CameraController => m_cameraController;

        public SectorRemoteDataScriptableObject CurrentSector => FactoryManager.Instance.SectorRemoteData[Values.Globals.CurrentSector];

        public WaveRemoteDataScriptableObject CurrentWaveData => CurrentSector.GetRemoteData(Globals.CurrentWave);

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

        public ProjectileManager ProjectileManager => m_projectileManager ?? (m_projectileManager = new ProjectileManager());
        private ProjectileManager m_projectileManager;

        public WaveEndSummaryData WaveEndSummaryData => m_waveEndSummaryData;
        private WaveEndSummaryData m_waveEndSummaryData;

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

        public Dictionary<BIT_TYPE, float> LiquidResourcesCachedOnDeath = new Dictionary<BIT_TYPE, float>();
        public int WaterAtBeginningOfWave;
        public int NumWavesInRow;
        public Dictionary<ENEMY_TYPE, int> EnemiesKilledInWave = new Dictionary<ENEMY_TYPE, int>();
        public List<string> MissionsCompletedDuringThisFlight = new List<string>();
        public bool RecoverFromDeath = false;
        public bool BotDead = false;

        //====================================================================================================================//
        
        private void Start()
        {
            m_bots = new List<Bot>();

            RegisterPausable();
            m_levelManagerUI = FindObjectOfType<LevelManagerUI>();

            GameUi.SetCurrentWaveText(Globals.CurrentSector + 1, Globals.CurrentWave + 1);

            Bot.OnBotDied += (deadBot, deathMethod) =>
            {
                LiquidResourcesCachedOnDeath = new Dictionary<BIT_TYPE, float>((IDictionary<BIT_TYPE, float>)PlayerPersistentData.PlayerData.liquidResource);

                InputManager.Instance.CancelMove();
                SavePlayerData();
                BotDead = true;

                Dictionary<int, float> tempDictionary = new Dictionary<int, float>();
                foreach (var resource in PlayerPersistentData.PlayerData.liquidResource)
                {
                    tempDictionary.Add((int)resource.Key, resource.Value);
                }

                Dictionary<string, object> levelLostAnalyticsDictionary = new Dictionary<string, object>
                {
                    {AnalyticsManager.DeathCause, deathMethod},
                    {AnalyticsManager.LevelTime, m_levelTimer + m_waveTimer},
                };
                string levelLostString = Globals.CurrentSector + "." + Globals.CurrentWave;
                AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelLost, eventDataDictionary: levelLostAnalyticsDictionary, eventDataParameter: levelLostString);
                
                SessionDataProcessor.Instance.PlayerKilled();
                SessionDataProcessor.Instance.EndActiveWave();

                if (!RecoverFromDeath)
                {
                    IsWaveProgressing = false;
                    RecoverFromDeath = true;

                    Alert.ShowAlert("Bot wrecked",
                        "Your bot has been wrecked. Deploy your recovery bot to rescue it.", 
                        "Deploy", 
                        () =>
                        {
                            RecoverFromDeath = true;
                            IsWaveProgressing = true;
                            GameUi.ShowRecoveryBanner(true);
                            RestartLevel();
                        });

                    //m_levelManagerUI.ToggleDeathUIActive(true, deathMethod);
                }
                else
                {
                    m_levelManagerUI.ShowSummaryScreen("GAME OVER", 
                        "You failed to recover your bot. Click to return to main menu.",
                        () =>
                        {
                            Globals.CurrentWave = 0;
                            GameTimer.SetPaused(false);
                            PlayerPersistentData.SaveAutosaveFiles();
                            SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.LEVEL);
                        });
                }
                //Debug.LogError("Bot Died. Press 'R' to restart");
            };
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

                int currentStage = m_currentStage;
                if (!CurrentWaveData.TrySetCurrentStage(m_waveTimer, out m_currentStage))
                {
                    if (m_currentStage == currentStage + 1)
                        TransitionToEndWaveState();
                }

                if (!EndWaveState)
                {
                    //Displays the time in timespan & the fill value
                    var duration = CurrentWaveData.GetWaveDuration();
                    var timeLeft = duration - m_waveTimer;
                    GameUi.SetClockValue(timeLeft / duration);
                    GameUi.SetTimeString((int) timeLeft);
                }
            }
            else if (ObstacleManager.HasNoActiveObstacles)
            {
                var botBlockData = BotObject.GetBlockDatas();
                SessionDataProcessor.Instance.SetEndingLayout(botBlockData);
                SessionDataProcessor.Instance.EndActiveWave();


                GameUi.SetClockValue(0f);
                GameUi.SetTimeString(0);
                SavePlayerData();
                GameTimer.SetPaused(true);

                if (RecoverFromDeath)
                {
                    /*var values = Enum.GetValues(typeof(BIT_TYPE));
                    foreach (BIT_TYPE bitType in values)
                    {
                        if (bitType == BIT_TYPE.WHITE)
                        {
                            continue;
                        }

                        //PlayerPersistentData.PlayerData.AddLiquidResource(bitType, PlayerPersistentData.PlayerData.liquidResource[bitType]);
                        //PlayerPersistentData.PlayerData.SetLiquidResource(bitType, 0);
                        //PlayerPersistentData.PlayerData.SetLiquidResource(bitType, LiquidResourcesCachedOnDeath[bitType]);
                    }*/
                    m_levelManagerUI.ShowSummaryScreen("Bot Recovered",
                        "You have recovered your wrecked bot. Return to base!", () =>
                        {
                            GameUi.ShowRecoveryBanner(false);
                            GameTimer.SetPaused(false);
                            EndWaveState = false;
                            EndSectorState = false;
                            ProcessLevelCompleteAnalytics();
                            ProcessScrapyardUsageBeginAnalytics();
                            SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.LEVEL);
                        });
                }

                if (EndSectorState)
                {
                    m_levelManagerUI.ShowSummaryScreen("Sector Completed",
                        "You beat the last wave of the sector. Return to base!", () =>
                        {
                            GameTimer.SetPaused(false);
                            EndWaveState = false;
                            EndSectorState = false;
                            PlayerPersistentData.PlayerData.AddSectorProgression(Globals.CurrentSector + 1, 0);
                            MissionManager.ProcessMissionData(typeof(SectorsCompletedMission), new MissionProgressEventData());
                            ProcessLevelCompleteAnalytics();
                            ProcessScrapyardUsageBeginAnalytics();
                            SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.LEVEL);
                        });
                }
                else
                {
                    //Turn wave end summary data into string, post in alert, and clear wave end summary data
                    m_levelManagerUI.ShowSummaryScreen(WaveEndSummaryData.WaveEndTitle,
                        m_waveEndSummaryData.GetWaveEndSummaryDataString(),
                        () =>
                        {
                            m_levelManagerUI.ToggleBetweenWavesUIActive(true);
                        },
                        "Continue");
                }

                

                Dictionary<string, object> levelCompleteAnalyticsDictionary = WaveEndSummaryData.GetWaveEndSummaryAnalytics();
                string levelCompleteString = WaveEndSummaryData.CompletedSector + "." + WaveEndSummaryData.CompletedWave;
                AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelComplete,
                    eventDataDictionary: levelCompleteAnalyticsDictionary, eventDataParameter: levelCompleteString);

                m_waveEndSummaryData = new WaveEndSummaryData();
                ObstacleManager.MoveToNewWave();
                EnemyManager.MoveToNewWave();
                EnemyManager.SetEnemiesInert(false);
                EnemyManager.RecycleAllEnemies();
                CurrentWaveData.TrySetCurrentStage(m_waveTimer, out m_currentStage);

                Dictionary<int, float> tempDictionary = new Dictionary<int, float>();
                foreach (var resource in PlayerPersistentData.PlayerData.liquidResource)
                {
                    tempDictionary.Add((int)resource.Key, resource.Value);
                }

                EnemiesKilledInWave.Clear();

                if (PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE] <= 0)
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
                }

                ProjectileManager.UpdateForces();
                RecoverFromDeath = false;
            }
        }

        //====================================================================================================================//

        public void Activate()
        {
            BotDead = false;
            m_worldGrid = null;
            m_bots.Add(FactoryManager.Instance.GetFactory<BotFactory>().CreateObject<Bot>());
            m_waveEndSummaryData = new WaveEndSummaryData();
            NumWavesInRow = 0;

            BotObject.transform.position = new Vector2(0, Constants.gridCellSize * 5);

            List<BlockData> botDataToLoad;
            if (RecoverFromDeath)
            {
                botDataToLoad = PlayerPersistentData.PlayerData.GetRecoveryDroneBlockData();
            }
            else
            {
                botDataToLoad = PlayerPersistentData.PlayerData.GetCurrentBlockData();
            }

            if (botDataToLoad.Count == 0)
            {
                BotObject.InitBot(RecoverFromDeath);
            }
            else
            {
                BotObject.InitBot(botDataToLoad.ImportBlockDatas(false), RecoverFromDeath);
            }

            if (RecoverFromDeath)
            {
                /*var values = Enum.GetValues(typeof(BIT_TYPE));
                foreach (BIT_TYPE bitType in values)
                {
                    if (bitType == BIT_TYPE.WHITE)
                    {
                        continue;
                    }
                    
                    PlayerPersistentData.PlayerData.SetLiquidResource
                        (bitType, Mathf.Min(PlayerPersistentData.PlayerData.recoveryDroneLiquidResource[bitType], 
                        PlayerPersistentData.PlayerData.liquidCapacity[bitType]));

                    PlayerPersistentData.PlayerData.SubtractRecoveryDroneLiquidResource(bitType, PlayerPersistentData.PlayerData.liquidResource[bitType]);
                }*/

                PlayerPersistentData.PlayerData.SetResources(BIT_TYPE.BLUE, WaterAtBeginningOfWave);
            }

            BotObject.transform.parent = null;
            SceneManager.MoveGameObjectToScene(BotObject.gameObject, gameObject.scene);

            SessionDataProcessor.Instance.StartNewWave(Globals.CurrentSector, Globals.CurrentWave, BotObject.GetBlockDatas());
            AudioController.PlayTESTWaveMusic(Globals.CurrentWave, true);

            MissionsCompletedDuringThisFlight.Clear();

            WaterAtBeginningOfWave = PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE];

            //FIXME We shouldn't be using Camera.main
            InputManager.Instance.InitInput();
            CameraController.SetOrthographicSize(Constants.gridCellSize * Globals.ColumnsOnScreen, BotObject.transform.position);
            //Globals.GridSizeX = CurrentSector.GridSizeX;
            if (Globals.Orientation == ORIENTATION.VERTICAL)
            {
                Globals.GridSizeY = (int)((CameraController.Camera.orthographicSize * Globals.GridHeightRelativeToScreen * 2) / Values.Constants.gridCellSize);
            }
            else
            {
                Globals.GridSizeY = (int)((CameraController.Camera.orthographicSize * Globals.GridHeightRelativeToScreen * 2 * (Screen.width / (float)Screen.height)) / Values.Constants.gridCellSize);
            }
            WorldGrid.SetupGrid();
            ProjectileManager.Activate();

            m_levelManagerUI.ToggleDeathUIActive(false, string.Empty);
            GameTimer.SetPaused(false);

            if (PlayerPersistentData.PlayerData.firstFlight)
            {
                PlayerPersistentData.PlayerData.firstFlight = false;
                Toast.AddToast(
                    "<b>Move: AD or Left/Right\nRotate: WS or Up/Down</b>",
                    time: 10.0f,
                    verticalLayout: Toast.Layout.End,
                    horizontalLayout: Toast.Layout.Middle);
            }

            Dictionary<int, float> tempResourceDictionary = new Dictionary<int, float>();
            foreach (var resource in PlayerPersistentData.PlayerData.resources)
            {
                tempResourceDictionary.Add((int)resource.Key, resource.Value);
            }

            Dictionary<int, int> tempComponentDictionary = new Dictionary<int, int>();
            foreach (var component in PlayerPersistentData.PlayerData.components)
            {
                tempComponentDictionary.Add((int)component.Key, component.Value);
            }

            Dictionary<string, object> levelStartAnalyticsDictionary = new Dictionary<string, object>
            {

            };
            string levelStartString = Globals.CurrentSector + "." + Globals.CurrentWave;
            AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelStart, eventDataDictionary: levelStartAnalyticsDictionary, eventDataParameter: levelStartString);

            Random.InitState(CurrentWaveData.WaveSeed);
            Debug.Log("SET SEED " + CurrentWaveData.WaveSeed);

            if (PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE] <
                Instance.CurrentWaveData.GetWaveDuration() * Constants.waterDrainRate)
            {
                GameTimer.SetPaused(true);
                m_levelManagerUI.ShowSummaryScreen("Almost out of water",
                    "You are nearly out of water at base. You will have to return home at the end of this wave with extra water.",
                    () => { GameTimer.SetPaused(false); }
                );
            }
        }

        public void Reset()
        {
            for (int i = m_bots.Count - 1; i >= 0; i--)
            {
                if (m_bots == null)
                    continue;

                Recycling.Recycler.Recycle<Bot>(m_bots[i]);
                m_bots.RemoveAt(i);
            }

            if (!RecoverFromDeath)
            {
                LiquidResourcesCachedOnDeath.Clear();
            }

            ObstacleManager.WorldElementsRoot.transform.position = Vector3.zero;

            m_waveTimer = 0;
            m_levelTimer = 0;
            CurrentWaveData.TrySetCurrentStage(m_waveTimer, out m_currentStage);
            ProjectileManager.Reset();
            MissionsCompletedDuringThisFlight.Clear();
            m_waveEndSummaryData = null;
            BotDead = false;
        }

        //====================================================================================================================//

        public void BeginNextWave()
        {
            IsWaveProgressing = true;
            EndWaveState = false;

            //LiquidResourcesCachedOnDeath = new Dictionary<BIT_TYPE, float>((IDictionary<BIT_TYPE, float>)PlayerPersistentData.PlayerData.liquidResource);

            SessionDataProcessor.Instance.StartNewWave(Globals.CurrentSector, Globals.CurrentWave, BotObject.GetBlockDatas());
            AudioController.PlayTESTWaveMusic(Globals.CurrentWave);

            if (PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE] <
                Instance.CurrentWaveData.GetWaveDuration() * Constants.waterDrainRate)
            {
                GameTimer.SetPaused(true);
                m_levelManagerUI.ShowSummaryScreen("Almost out of water",
                    "You are nearly out of water at base. You will have to return home at the end of this wave with extra water.",
                    () => { GameTimer.SetPaused(false); });
            }
        }

        
        private void TransitionToEndWaveState()
        {
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
            WaveEndSummaryData.WaveEndTitle = $"Sector {Globals.CurrentSector + 1} Wave {Globals.CurrentWave + 1} Complete";//"Wave " + (Globals.CurrentWave + 1) + " Sector " +  + " Complete";

            int progressionSector = Globals.CurrentSector;
            string endWaveMessage;

            PlayerPersistentData.PlayerData.ReduceLevelResourceModifier(Globals.CurrentSector, Globals.CurrentWave);
            
            if (Globals.CurrentWave < CurrentSector.WaveRemoteData.Count - 1)
            {
                Globals.CurrentWave++;
                endWaveMessage = "Wave Complete!";
            }
            else
            {
                Globals.CurrentWave = 0;
                progressionSector++;
                EndSectorState = true;
                endWaveMessage = "Sector Complete!";
            }

            Toast.AddToast(endWaveMessage, time: 1.0f, verticalLayout: Toast.Layout.Middle, horizontalLayout: Toast.Layout.Middle);
            if (!Globals.OnlyGetWaveLootOnce || !PlayerPersistentData.PlayerData.CheckIfQualifies(progressionSector, Globals.CurrentWave))
            {
                CurrentWaveData.ConfigureLootTable();
                List<IRDSObject> newWaveLoot = CurrentWaveData.rdsTable.rdsResult.ToList();
                DropLoot(newWaveLoot, -ObstacleManager.WorldElementsRoot.transform.position + (Vector3.up * 10 * Constants.gridCellSize), false);
            }
            PlayerPersistentData.PlayerData.AddSectorProgression(progressionSector, Globals.CurrentWave);
            EndWaveState = true;
            LevelManagerUI.OverrideText = string.Empty;
            m_levelTimer += m_waveTimer;
            m_waveTimer = 0;
            GameUi.SetCurrentWaveText("Complete");
            GameUi.ShowAbortWindow(false);
            EnemyManager.SetEnemiesInert(true);

            Random.InitState(CurrentWaveData.WaveSeed);
            Debug.Log("SET SEED " + CurrentWaveData.WaveSeed);
        }

        public void DropLoot(List<IRDSObject> loot, Vector3 position, bool isFromEnemyLoot)
        {
            for (int i = loot.Count - 1; i >= 0; i--)
            {
                switch (loot[i])
                {
                    case RDSValue<Blueprint> rdsValueBlueprint:
                        PlayerPersistentData.PlayerData.UnlockBlueprint(rdsValueBlueprint.rdsValue);
                        Toast.AddToast("Unlocked Blueprint!");
                        loot.RemoveAt(i);
                        break;
                    case RDSValue<FacilityBlueprint> rdsValueFacilityBlueprint:
                        PlayerPersistentData.PlayerData.UnlockFacilityBlueprintLevel(rdsValueFacilityBlueprint.rdsValue);
                        Toast.AddToast("Unlocked Facility Blueprint!");
                        loot.RemoveAt(i);
                        break;
                    case RDSValue<Vector2Int> rdsValueGears:
                    {
                        var gears = Random.Range(rdsValueGears.rdsValue.x, rdsValueGears.rdsValue.y);
                        PlayerPersistentData.PlayerData.ChangeGears(gears);
                        loot.RemoveAt(i);
                        
                        FloatingText.Create($"+{gears}", position, Color.white);
                        
                        break;
                    }
                }
            }

            ObstacleManager.SpawnObstacleExplosion(position, loot, isFromEnemyLoot);
        }

        public void SavePlayerData()
        {
            foreach (Bot bot in m_bots)
            {
                var blockData = bot.GetBlockDatas();
                if (!blockData.Any(x => x.ClassType.Contains(nameof(Part)) && x.Type == (int)PART_TYPE.CORE))
                    blockData = new List<BlockData>();

                if (RecoverFromDeath)
                {
                    PlayerPersistentData.PlayerData.SetRecoveryDroneBlockData(blockData);
                }
                else
                {
                    PlayerPersistentData.PlayerData.SetCurrentBlockData(blockData);
                }
            }
        }

        public void RestartLevel()
        {
            m_levelManagerUI.ToggleDeathUIActive(false, string.Empty);
            GameUi.SetCurrentWaveText(Globals.CurrentSector + 1, Globals.CurrentWave + 1);
            GameTimer.SetPaused(false);
            SceneLoader.ActivateScene(SceneLoader.LEVEL, SceneLoader.LEVEL);
        }

        //============================================================================================================//

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

        //============================================================================================================//

        public void ProcessScrapyardUsageBeginAnalytics()
        {
            Dictionary<string, object> scrapyardUsageBeginAnalyticsDictionary = new Dictionary<string, object>
            {
                {"Sector Number", Values.Globals.CurrentSector}
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

        //====================================================================================================================//
        
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

    }
}

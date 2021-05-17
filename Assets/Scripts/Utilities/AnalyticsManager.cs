using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Saving;
using UnityEngine;
using UnityEngine.Analytics;

namespace StarSalvager.Utilities
{
    //TODO: Investigate whether specific achievements should be recorded as custom Analytic events, or whether data tracking for achievements is handled by whatever achievement system is used.

    public static class AnalyticsManager
    {
        
        //Enum
        //====================================================================================================================//
        public enum REASON
        {
            WIN,
            DEATH,
            QUIT,
            LEAVE
        }
        
        /*public enum AnalyticsEventType
        {
            ApplicationOpen,
            ApplicationQuit,
            LevelStart,
            //LevelComplete,
            //LevelLost,


            //GameOver,
            FirstInteraction,
            TutorialStart,
            TutorialStep,
            TutorialComplete,
            TutorialSkip,
            GameStart,
            ScrapyardUsageBegin,
            ScrapyardUsageEnd,
            BotDied,
            FlightBegin,
            WaveEnd
        }*/

        //Properties
        //====================================================================================================================//
        
        //EVENTS
        //====================================================================================================================//
        private const string APPLICATION_START_EVENT = "application_start";
        private const string APPLICATION_END_EVENT = "application_end";
        
        private const string NEW_RUN_EVENT = "run_new";
        private const string ABANDON_RUN_EVENT = "run_abandon";
        
        private const string WAVE_START_EVENT = "wave_start";
        private const string WAVE_END_EVENT = "wave_end";
        
        private const string PICKED_PART_EVENT = "picked_part";
        private const string PURCHASE_PATCH_EVENT = "purchased_patch";

        private const string WRECK_START_EVENT = "wreck_start";
        private const string WRECK_END_EVENT = "wreck_end";
        
        //Event Parameters
        //====================================================================================================================//
        //ID
        private const string TIMESTAMP = "timestamp";
        private const string USER_ID = "user_id";
        private const string PLAYTHROUGH_ID = "playthrough_id";
        private const string SESSION_ID = "session_id";

        private const string RUN_COUNT = "run_count";
        private const string TIME_PLAYED = "time_played";
        private const string EVENT_REASON = "reason";
        private const string XP_EARNED = "earned_xp";
        private const string COMBOS_EARNED = "earned_combos";
        
        //EVENT
        private const string CURRENT_SECTOR = "current_sector";
        private const string CURRENT_WAVE = "current_wave";
        private const string CURRENT_STAGE = "current_stage";
        private const string CURRENT_WRECK = "current_wreck";
        
        //BITS
        private const string BITS_APPEARED_TOTAL = "bits_appeared_total";
        private const string BITS_COLLECTED_TOTAL = "bits_collected_total";
        
        //CURRENCY
        private const string CURRENT_XP = "current_xp";
        private const string CURRENT_GEARS = "current_gears";
        private const string CURRENT_SILVER = "current_silver";
        
        private const string SPENT_GEARS = "spent_gears";
        private const string SPENT_SILVER = "spent_silver";
        
        //ENEMIES
        private const string ENEMIES_APPEARED = "enemies_appeared";
        private const string ENEMIES_KILLED = "enemies_killed";


        //====================================================================================================================//
        
        /*public static string TotalPlaytime = "Total Playtime";
        public static string DeathCause = "Death Cause";
        public static string GearsGained = "Gears Gained";
        public static string LevelsGained = "Levels Gained";
        public static string EnemiesKilled = "Enemies Killed";
        public static string EnemiesKilledPercentage = "Enemies Killed Percentage";
        public static string BonusShapesMatched = "Bonus Shapes Matched";
        public static string BonusShapesMatchedPercentage = "Bonus Shapes Matched Percentage";
        public static string BlueprintsUnlocked = "Blueprints Unlocked";
        public static string LevelTime = "Level Time";*/



        private static int m_recentAnalyticEvents = 0;
        private const int m_recentAnalyticEventsCap = 100;

        //NEW ANALYTICS REPORTS
        //====================================================================================================================//
        //APPLICATION_START
        //APPLICATION_END
        //WAVE_START
        //WAVE_END
        //WRECK_START
        //WRECK_END

        public static void ApplicationStartEvent()
        {
            TryTriggerEvent(APPLICATION_START_EVENT);
        }
        public static void ApplicationEndEvent()
        {
            var eventData = new Dictionary<string, object>
            {
                {TIME_PLAYED, Mathf.RoundToInt((float)GameTimer.GetTimePlayed.TotalSeconds)}
            };
            
            TryTriggerEvent(APPLICATION_END_EVENT, eventData);
        }
        
        public static void StartNewRunEvent()
        {
            var eventData = new Dictionary<string, object>
            {
                {RUN_COUNT, PlayerDataManager.GetRunCount()}
            };
            
            TryTriggerEvent(NEW_RUN_EVENT, eventData);
        }
        
        public static void AbandonRunEvent()
        {
            var combosThisRun = PlayerDataManager.GetCombosMadeThisRun().Sum(x => x.Value);
            var enemiesKilledThisRun = PlayerDataManager.GetEnemiesKilledThisRun().Sum(x => x.Value);
            var eventData = new Dictionary<string, object>
            {
                {CURRENT_SECTOR, PlayerDataManager.GetSector()},
                {CURRENT_WAVE, PlayerDataManager.GetWave()},
                
                {XP_EARNED, PlayerDataManager.GetXPThisRun()},
                {COMBOS_EARNED, combosThisRun},
                
                {CURRENT_SILVER, PlayerDataManager.GetSilver()},
                {CURRENT_GEARS, PlayerDataManager.GetGears()},
                
                {ENEMIES_KILLED, enemiesKilledThisRun},
            };
            
            TryTriggerEvent(ABANDON_RUN_EVENT, eventData);
        }
        
        public static void WaveStartEvent(in int sector, in int wave)
        {
            var eventData = new Dictionary<string, object>
            {
                {CURRENT_SECTOR, sector},
                {CURRENT_WAVE, wave}
            };
            
            TryTriggerEvent(WAVE_START_EVENT, eventData);
            //analyticsDictionary.AddRange(GetAmmoInfo());
        }
        public static void WaveEndEvent(in REASON reason)
        {
            var waveEndSummaryData = LevelManager.Instance.WaveEndSummaryData;

            if (waveEndSummaryData == null)
                throw new ArgumentException();
            
            var eventData = new Dictionary<string, object>
            {
                {EVENT_REASON, reason.ToString()},

                {CURRENT_SECTOR, waveEndSummaryData.Sector},
                {CURRENT_WAVE, waveEndSummaryData.Wave},
                
                {XP_EARNED, waveEndSummaryData.XPGained},
                {COMBOS_EARNED, waveEndSummaryData.CombosMade},
                
                {ENEMIES_KILLED, waveEndSummaryData.NumEnemiesKilled},
                {ENEMIES_APPEARED, waveEndSummaryData.NumTotalEnemiesSpawned},

            };

            TryTriggerEvent(WAVE_END_EVENT, eventData);
        }

        public static void PickedPartEvent(in Dictionary<PART_TYPE, bool> selectedParts)
        {
            if (selectedParts.IsNullOrEmpty())
                throw new ArgumentException();
            
            var eventData = new Dictionary<string, object>
            {
                {CURRENT_XP, PlayerDataManager.GetXP()}
            };

            foreach (var selectedPart in selectedParts)
            {
                eventData.Add(selectedPart.Key.ToString(), selectedPart.Value ? 1 : 0);
            }

            TryTriggerEvent(PICKED_PART_EVENT, eventData);
        }

        public static void PurchasedPatchEvent(in PART_TYPE partType, in PatchData patchData)
        {
            var eventData = new Dictionary<string, object>
            {
                {nameof(PART_TYPE), partType.ToString()},
                {nameof(PATCH_TYPE), ((PATCH_TYPE)patchData.Type).ToString()},
                {"patch_level", patchData.Level}
            };
            
            TryTriggerEvent(PURCHASE_PATCH_EVENT, eventData);
        }
        
        public static void WreckStartEvent()
        {
            var eventData = new Dictionary<string, object>
            {
                {CURRENT_SECTOR, PlayerDataManager.GetSector()},
                
                {CURRENT_XP, PlayerDataManager.GetXP()},
                {CURRENT_GEARS, PlayerDataManager.GetGears()},
                {CURRENT_SILVER, PlayerDataManager.GetSilver()},
            };
            
            TryTriggerEvent(WRECK_START_EVENT, eventData);
        }
        public static void WreckEndEvent(in REASON reason)
        {
            Dictionary<string, object> eventData;

            switch (reason)
            {
                case REASON.QUIT:
                    eventData = new Dictionary<string, object>
                    {
                        {EVENT_REASON, reason.ToString()}
                    };
                    break;
                case REASON.LEAVE:
                    var patchRemoteData = FactoryManager.Instance.PatchRemoteData;
                    var spentSilver = 0;
                    var spentGears = 0;

                    var purchasedPatches = PlayerDataManager.PurchasedPatches;

                    if (!purchasedPatches.IsNullOrEmpty())
                    {
                        foreach (var patchData in purchasedPatches)
                        {
                            var remoteData = patchRemoteData.GetRemoteData(patchData.Type);
                        
                            spentSilver += remoteData.Levels[patchData.Level].silver;
                            spentGears += Mathf.RoundToInt(remoteData.Levels[patchData.Level].gears *
                                                           PlayerDataManager.GetCurrentUpgradeValue(UPGRADE_TYPE.PATCH_COST));
                        }
                    }

                    eventData = new Dictionary<string, object>
                    {
                        {EVENT_REASON, reason.ToString()},
                        {SPENT_GEARS, spentGears},
                        {SPENT_SILVER, spentSilver},
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
            }
            
            TryTriggerEvent(WRECK_END_EVENT, eventData);
        }

        //====================================================================================================================//
        
        private static bool TryTriggerEvent(in string eventName, in IDictionary<string, object> eventData)
        {
            if (eventData.IsNullOrEmpty())
                return false;
            if (ProcessRecentAnalyticEventCapMet())
                return false;
            if (ProcessDictionarySizeRestrictionsExceeded(eventData))
                return false;
            
#if UNITY_EDITOR
            
            Debug.Log($"[DEBUG LOCKED] Sending event {eventName} with data:\n{JsonConvert.SerializeObject(eventData)}");
            return true;
            
#else
            
            var result = AnalyticsEvent.Custom(eventName, eventData);

            switch (result)
            {
                case AnalyticsResult.Ok:
                    return true;
                case AnalyticsResult.NotInitialized:
                case AnalyticsResult.AnalyticsDisabled:
                    Debug.LogError($"WARNING {result} when calling {eventName}");
                    break;
                case AnalyticsResult.TooManyItems:
                case AnalyticsResult.SizeLimitReached:
                case AnalyticsResult.TooManyRequests:
                case AnalyticsResult.InvalidData:
                case AnalyticsResult.UnsupportedPlatform:
                    Debug.LogError($"ERROR {result} when calling {eventName}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
#endif
        }
        
        private static bool TryTriggerEvent(in string eventName)
        {
            if (ProcessRecentAnalyticEventCapMet())
                return false;

#if UNITY_EDITOR
            
            Debug.Log($"[DEBUG LOCKED] Sending event {eventName}");
            return true;
            
#else
            
            var result = AnalyticsEvent.Custom(eventName);

            switch (result)
            {
                case AnalyticsResult.Ok:
                    return true;
                case AnalyticsResult.NotInitialized:
                case AnalyticsResult.AnalyticsDisabled:
                    Debug.LogError($"WARNING {result} when calling {eventName}");
                    break;
                case AnalyticsResult.TooManyItems:
                case AnalyticsResult.SizeLimitReached:
                case AnalyticsResult.TooManyRequests:
                case AnalyticsResult.InvalidData:
                case AnalyticsResult.UnsupportedPlatform:
                    Debug.LogError($"ERROR {result} when calling {eventName}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
#endif
        }
        
        //====================================================================================================================//
        


        /*private static Dictionary<string, object> GetAmmoInfo()
        {
            const string AMMO = "ammo";

            string GetAmmoName(in BIT_TYPE bitType) =>$"{AMMO}_{bitType}";
            
            var analyticsDictionary = new Dictionary<string, object>();
            var playerResources = PlayerDataManager.GetResources();
            foreach (var playerResource in playerResources)
            {
                var name = GetAmmoName(playerResource.BitType);
                analyticsDictionary.Add(name, Mathf.RoundToInt(playerResource.Ammo));
            }

            return analyticsDictionary;
        }
        
        private static Dictionary<string, object> GetComboInfo()
        {
            const string COMBO = "combo";

            string GetComboName(in BIT_TYPE bitType) => $"{COMBO}_{bitType}";
            
            var analyticsDictionary = new Dictionary<string, object>();
            
            var combosThisRun = PlayerDataManager.GetCombosMadeThisRun();

            foreach (var combo in combosThisRun)
            {
                analyticsDictionary.Add(GetComboName(combo.Key.BitType), combo.Value);
            }

            return analyticsDictionary;
        }
        private static Dictionary<string, object> GetEnemiesInfo()
        {

        }*/
        
        

        //Functions
        //====================================================================================================================//
        
        /*public static bool ReportAnalyticsEvent(AnalyticsEventType eventType, Dictionary<string, object> eventDataDictionary = null, object eventDataParameter = null)
        {
            //Check to confirm if too many analytic events have been sent recently or the dictionary input is too large. If either is true, unity analytics won't accept the message, so we avoid sending it at all
            if ((eventDataDictionary != null && ProcessDictionarySizeRestrictionsExceeded(eventDataDictionary, eventDataParameter)) || 
                ProcessRecentAnalyticEventCapMet())
            {
                return false;
            }
            
            //Call the corresponding function for the eventType
            AnalyticsResult result;
            switch(eventType)
            {
                /*case AnalyticsEventType.GameOver:
                    result = ReportGameOver(eventDataParameter.ToString(), eventDataDictionary);
                    break;#1#
                case AnalyticsEventType.FirstInteraction:
                    result = ReportFirstInteraction(eventDataDictionary);
                    break;
                case AnalyticsEventType.TutorialStart:
                    result = ReportTutorialStart(eventDataDictionary);
                    break;
                case AnalyticsEventType.TutorialStep:
                    result = ReportTutorialStep((int)eventDataParameter, eventDataDictionary);
                    break;
                case AnalyticsEventType.TutorialComplete:
                    result = ReportTutorialComplete(eventDataDictionary);
                    break;
                case AnalyticsEventType.TutorialSkip:
                    result = ReportTutorialSkip(eventDataDictionary);
                    break;


                case AnalyticsEventType.GameStart:
                    result = ReportGameStart(eventDataDictionary);
                    break;
                case AnalyticsEventType.LevelStart:
                    result = ReportLevelStart(eventDataParameter.ToString(), eventDataDictionary);
                    break;
                /*case AnalyticsEventType.LevelComplete:
                    result = ReportLevelComplete(eventDataParameter.ToString(), eventDataDictionary);
                    break;
                case AnalyticsEventType.LevelLost:
                    result = ReportLevelLost(eventDataDictionary);
                    break;#1#
                case AnalyticsEventType.ScrapyardUsageBegin:
                    result = ReportScraypardUsageBegin(eventDataDictionary);
                    break;
                case AnalyticsEventType.ScrapyardUsageEnd:
                    result = ReportScraypardUsageEnd(eventDataDictionary);
                    break;

                case AnalyticsEventType.ApplicationOpen:
                    result = ReportApplicationOpen(eventDataDictionary);
                    break;
                case AnalyticsEventType.ApplicationQuit:
                    result = ReportApplicationQuit(eventDataDictionary);
                    break;
                case AnalyticsEventType.BotDied:
                    result = ReportBotDied(eventDataDictionary);
                    break;
                case AnalyticsEventType.FlightBegin:
                    result = ReportFlightBegin(eventDataDictionary);
                    break;
                case AnalyticsEventType.WaveEnd:
                    result = ReportWaveEnd(eventDataDictionary);
                    break;
                default:
                    Debug.Log("AnalyticsEventType not implemented in switch case");
                    return false;
            }

            //Temporary testing line to print the result of the analyticevent
            Debug.Log(eventType.ToString() + " with result of " + result);

            return (result == AnalyticsResult.Ok);
        }*/

        //TODO: Add checking to clear recent analytic events from the cap at the turn of the hour, if that turns out to be how it works
        //TODO: Ensure that the game tracks analytic event cap even through the game being restarted, in some persistent storage
        private static bool ProcessRecentAnalyticEventCapMet()
        {
            //Check if recent analytic events have met cap. If not, increment the cap, and allow the message to be sent
            return m_recentAnalyticEvents >= m_recentAnalyticEventsCap;
        }

        private static bool ProcessDictionarySizeRestrictionsExceeded(in IDictionary<string, object> eventData, in object eventDataParameter = null)
        {
            //Check if the dictionary length exceeds the cap. The cap for unity analytics is 10, in code we will cap to 9 since that 10 includes mandatory variables of standard events
            if (eventData.Count > 10 || eventData.Count > 9 && eventDataParameter != null)
            {
                Debug.LogError("Dictionary length too long to return as analytic event");
                return true;
            }

            //Confirm that the sum of all key/value pairs don't exceed 500 characters. 
            int dictionaryCharacterLength = 0;
            foreach (KeyValuePair<string, object> entry in eventData)
            {
                //TODO: convert the entry.value.ToString().Length into something that accounts for the size of each variable as per https://docs.unity3d.com/Manual/UnityAnalyticsEventLimits.html
                dictionaryCharacterLength += entry.Key.Length + entry.Value.ToString().Length;
            }

            if (dictionaryCharacterLength > 500)
            {
                Debug.Log("Sum of key/value pairs in dictionary exceeded 500 characters, which is too large to return as analytic event parameter, at length" + dictionaryCharacterLength);
                return true;
            }

            return false;
        }

        //Report Events
        //====================================================================================================================//

        /*#region Report Events

        private static AnalyticsResult ReportGameOver(string levelName, Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.GameOver(levelName, eventData);
        }

        private static AnalyticsResult ReportLevelComplete(string levelName, Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.LevelComplete(levelName, eventData);
        }

        private static AnalyticsResult ReportFirstInteraction(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.FirstInteraction(eventData: eventData);
        }

        private static AnalyticsResult ReportTutorialStart(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.TutorialStart(eventData: eventData);
        }

        private static AnalyticsResult ReportTutorialStep(int tutorialStepIndex, Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.TutorialStep(tutorialStepIndex, eventData: eventData);
        }

        private static AnalyticsResult ReportTutorialComplete(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.TutorialComplete(eventData: eventData);
        }

        private static AnalyticsResult ReportTutorialSkip(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.TutorialSkip(eventData: eventData);
        }

        //============================================================================================================//

        private static AnalyticsResult ReportGameStart(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.GameStart(eventData);
        }

        private static AnalyticsResult ReportLevelStart(string levelName, Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.LevelStart(levelName, eventData);
        }

        private static AnalyticsResult ReportLevelLost(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.Custom("level_lost", eventData);
        }

        private static AnalyticsResult ReportScraypardUsageBegin(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.Custom("scrapyard_usage_begin", eventData);
        }

        private static AnalyticsResult ReportScraypardUsageEnd(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.Custom("scrapyard_usage_end", eventData);
        }

        //============================================================================================================//

        private static AnalyticsResult ReportApplicationOpen(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.Custom("application_open", eventData);
        }

        private static AnalyticsResult ReportApplicationQuit(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.Custom("application_quit", eventData);
        }

        private static AnalyticsResult ReportBotDied(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.Custom("bot_died", eventData);
        }

        private static AnalyticsResult ReportFlightBegin(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.Custom("flight_begin", eventData);
        }

        private static AnalyticsResult ReportWaveEnd(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.Custom("wave_end", eventData);
        }

        #endregion //Report Events*/

        //====================================================================================================================//
        
    }
}
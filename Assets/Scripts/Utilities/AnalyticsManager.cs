using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

namespace StarSalvager.Utilities
{
    //TODO: Investigate whether specific achievements should be recorded as custom Analytic events, or whether data tracking for achievements is handled by whatever achievement system is used.
    
    public static class AnalyticsManager
    {
        public enum AnalyticsEventType
        {
            GameStart,
            GameOver,
            LevelStart,
            LevelComplete,
            FirstInteraction,
            TutorialStart,
            TutorialStep,
            TutorialComplete,
            TutorialSkip,
            MissionUnlock,
            MissionComplete,

            BotDied,
            ScrapyardUsageBegin,
            ScrapyardUsageEnd
        }

        private static int m_recentAnalyticEvents = 0;
        private const int m_recentAnalyticEventsCap = 100;

        public static bool ReportAnalyticsEvent(AnalyticsEventType eventType, Dictionary<string, object> eventDataDictionary = null, object eventDataParameter = null)
        {
            //Check to confirm if too many analytic events have been sent recently or the dictionary input is too large. If either is true, unity analytics won't accept the message, so we avoid sending it at all
            if ((eventDataDictionary != null && ProcessDictionarySizeRestrictionsExceeded(eventDataDictionary)) || 
                ProcessRecentAnalyticEventCapMet())
            {
                return false;
            }
            
            //Call the corresponding function for the eventType
            AnalyticsResult result;
            switch(eventType)
            {
                case AnalyticsEventType.GameOver:
                    result = ReportGameOver(eventDataParameter.ToString(), eventDataDictionary);
                    break;
                case AnalyticsEventType.LevelComplete:
                    result = ReportLevelComplete(eventDataParameter.ToString(), eventDataDictionary);
                    break;
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
                case AnalyticsEventType.MissionUnlock:
                    result = ReportMissionUnlock(eventDataDictionary);
                    break;
                case AnalyticsEventType.MissionComplete:
                    result = ReportMissionComplete(eventDataDictionary);
                    break;


                case AnalyticsEventType.GameStart:
                    result = ReportGameStart(eventDataDictionary);
                    break;
                case AnalyticsEventType.BotDied:
                    result = ReportBotDied(eventDataDictionary);
                    break;
                case AnalyticsEventType.LevelStart:
                    result = ReportLevelStart(eventDataParameter.ToString(), eventDataDictionary);
                    break;
                case AnalyticsEventType.ScrapyardUsageBegin:
                    result = ReportScraypardUsageBegin(eventDataDictionary);
                    break;
                case AnalyticsEventType.ScrapyardUsageEnd:
                    result = ReportScraypardUsageEnd(eventDataDictionary);
                    break;
                default:
                    Debug.Log("AnalyticsEventType not implemented in switch case");
                    return false;
                    break;
            }

            //Temporary testing line to print the result of the analyticevent
            Debug.Log(eventType.ToString() + " with result of " + result);

            return (result == AnalyticsResult.Ok);
        }

        //TODO: Add checking to clear recent analytic events from the cap at the turn of the hour, if that turns out to be how it works
        //TODO: Ensure that the game tracks analytic event cap even through the game being restarted, in some persistent storage
        public static bool ProcessRecentAnalyticEventCapMet()
        {
            //Check if recent analytic events have met cap. If not, increment the cap, and allow the message to be sent
            if (m_recentAnalyticEvents < m_recentAnalyticEventsCap)
            {
                //m_recentAnalyticEvents++;
                return false;
            }

            return true;
        }

        public static bool ProcessDictionarySizeRestrictionsExceeded(Dictionary<string, object> eventData)
        {
            //Check if the dictionary length exceeds the cap. The cap for unity analytics is 10, in code we will cap to 9 since that 10 includes mandatory variables of standard events
            if (eventData.Count > 9)
            {
                Debug.Log("Dictionary length too long to return as analytic event");
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

        private static AnalyticsResult ReportMissionUnlock(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.Custom("mission_unlock", eventData);
        }

        private static AnalyticsResult ReportMissionComplete(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.Custom("mission_complete", eventData);
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

        private static AnalyticsResult ReportBotDied(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.Custom("bot_died", eventData);
        }

        private static AnalyticsResult ReportScraypardUsageBegin(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.Custom("scrapyard_usage_begin", eventData);
        }

        private static AnalyticsResult ReportScraypardUsageEnd(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.Custom("scrapyard_usage_end", eventData);
        }
    }
}
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
            TutorialStart,
            TutorialStep,
            TutorialComplete,
            TutorialSkip,
            MissionUnlock,
            MissionComplete,
            None
        }

        private static int m_recentAnalyticEvents = 0;
        private const int m_recentAnalyticEventsCap = 100;

        public static bool ReportAnalyticsEvent(AnalyticsEventType eventType, Dictionary<string, object> eventDataDictionary = null, object eventDataParameter = null)
        {
            if (eventDataDictionary != null && 
                (ProcessDictionarySizeRestrictionsExceeded(eventDataDictionary) || 
                ProcessRecentAnalyticEventCapMet()))
            {
                return false;
            }

            if (ProcessRecentAnalyticEventCapMet())
            {
                return false;
            }
            
            AnalyticsResult? result = null;
            
            bool analyticsResultSuccessful = false;
            switch(eventType)
            {
                case AnalyticsEventType.GameStart:
                    result = ReportGameStart(eventDataDictionary);
                    break;
                case AnalyticsEventType.GameOver:
                    result = ReportGameOver(eventDataParameter.ToString(), eventDataDictionary);
                    break;
                case AnalyticsEventType.LevelStart:
                    result = ReportLevelStart(eventDataParameter.ToString(), eventDataDictionary);
                    break;
                case AnalyticsEventType.LevelComplete:
                    result = ReportLevelComplete(eventDataParameter.ToString(), eventDataDictionary);
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
                case AnalyticsEventType.None:
                    analyticsResultSuccessful = false;
                    break;
            }

            if (result != null)
            {
                Debug.Log(eventType.ToString() + " with result of " + result);
            }

            return (result != null && result == AnalyticsResult.Ok);
        }

        //TODO: Add checking to clear recent analytic events from the cap at the turn of the hour, if that turns out to be how it works
        //TODO: Ensure that the game tracks analytic event cap even through the game being restarted, in some persistent storage
        public static bool ProcessRecentAnalyticEventCapMet()
        {
            
            if (m_recentAnalyticEvents < m_recentAnalyticEventsCap)
            {
                return false;
            }

            //m_recentAnalyticEvents++;
            return true;
        }

        public static bool ProcessDictionarySizeRestrictionsExceeded(Dictionary<string, object> eventData)
        {
            if (eventData.Count > 9)
            {
                Debug.Log("Dictionary length too long to return as analytic event");
                return true;
            }

            int dictionaryCharacterLength = 0;
            foreach (KeyValuePair<string, object> entry in eventData)
            {
                int entryCharacterLength = entry.Key.Length + entry.Value.ToString().Length;
                if (entryCharacterLength > 100)
                {
                    Debug.Log("Dictionary entry " + entry.Key + " has exceeded 100 characters, which is too large to return as analytic event parameter, at length" + entryCharacterLength);
                    return true;
                }

                dictionaryCharacterLength += entryCharacterLength;
            }

            if (dictionaryCharacterLength > 500)
            {
                Debug.Log("Dictionary has exceeded 500 characters, which is too large to return as analytic event parameter, at length" + dictionaryCharacterLength);
                return true;
            }

            return false;
        }

        private static AnalyticsResult ReportGameStart(Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.GameStart(eventData);
        }

        private static AnalyticsResult ReportGameOver(string levelName, Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.GameOver(levelName, eventData);
        }

        private static AnalyticsResult ReportLevelStart(string levelName, Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.LevelStart(levelName, eventData);
        }

        private static AnalyticsResult ReportLevelComplete(string levelName, Dictionary<string, object> eventData = null)
        {
            return AnalyticsEvent.LevelComplete(levelName, eventData);
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
    }
}
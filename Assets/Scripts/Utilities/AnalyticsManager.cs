using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

namespace StarSalvager.Utilities
{
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

        public static bool ReportAnalyticsEvent(AnalyticsEventType eventType, object eventDataParameter = null, Dictionary<string, object> eventDataDictionary = null)
        {
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
                    analyticsResultSuccessful = false;
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
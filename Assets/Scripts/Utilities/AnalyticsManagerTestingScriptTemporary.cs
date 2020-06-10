using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.Utilities;

namespace StarSalvager
{
    public class AnalyticsManagerTestingScriptTemporary : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.GameStart))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }


            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.GameOver, 1.4f))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }


            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelStart, 1.4f))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }


            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelComplete, 1.4f))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }


            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.TutorialStart))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }


            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.TutorialStep, 1))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }


            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.TutorialComplete))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }

            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.TutorialSkip))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }

            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.MissionUnlock))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }

            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.MissionComplete))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }
        }
    }
}
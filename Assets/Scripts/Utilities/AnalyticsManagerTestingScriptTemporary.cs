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
            /*Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("1qpriugbj12qpriugbj12qpriugbj12qpriugbjriug", 9850709841);
            dictionary.Add("2qpriugbj12qpriugbj12qpriugbj12qpriugbjriug", 9850709841.0f);
            dictionary.Add("3qpriugbj12qpriugbj12qpriugbj12qpriugbjriug", "string value");
            print (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.GameStart, dictionary));*/



            /*for (int i = 0; i < 10; i++)
            {
                if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.GameStart))
                {
                    print("Event sent successfully");
                }
                else
                {
                    print("Event failed");
                }
            }

            /*if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.GameOver, eventDataParameter: 1.4f))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }


            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelStart, eventDataParameter: 1.4f))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }


            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.LevelComplete, eventDataParameter: 1.4f))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }*/

            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.FirstInteraction))
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


            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.TutorialStep, eventDataParameter: 1))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }


            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.TutorialStep, eventDataParameter: 2))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }


            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.TutorialStep, eventDataParameter: 3))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }


            /*if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.TutorialComplete))
            {
                print("Event sent successfully");
            }
            else
            {
                print("Event failed");
            }

            /*if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.TutorialSkip))
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
            }*/
        }
    }
}
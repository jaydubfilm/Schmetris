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
            dictionary.Add("carroeij12", 1);
            dictionary.Add("carroeij11", 1);
            dictionary.Add("carroeij0", 1);
            dictionary.Add("carroeij9", 1);
            dictionary.Add("carroeij8", 1);
            dictionary.Add("carroeij7", 1);
            dictionary.Add("carroeij6", 1);
            dictionary.Add("carroeij5", 1);
            dictionary.Add("carroeij4", 1);
            dictionary.Add("carroeij3", 1);
            dictionary.Add("carroeij2", 1);
            dictionary.Add("carroeij1", 1);

            print (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.GameStart, dictionary));*/



            for (int i = 0; i < 100; i++)
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

            if (AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.GameOver, eventDataParameter: 1.4f))
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
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.UI;
using UnityEngine;

namespace StarSalvager.Utilities
{
    public class ErrorCatcher : MonoBehaviour
    {
        public struct ErrorInfo
        {
            public LogType Type;
            public string Condition;
            public string StackTrace;

            public override string ToString()
            {
                return $"{Type}: {Condition}\n{StackTrace}\n";
            }
        }

        public static List<ErrorInfo> LoggedErrors => _loggedErrors;
        private static List<ErrorInfo> _loggedErrors;
        
        //============================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            SetupExceptionHandling();
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleException;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleException;
        }

        //============================================================================================================//

        static bool isExceptionHandlingSetup;
        private static void SetupExceptionHandling()
        {
            if (isExceptionHandlingSetup) 
                return;
            
            isExceptionHandlingSetup = true;
            Application.logMessageReceived += HandleException;
            _loggedErrors = new List<ErrorInfo>();
        }

        private static void HandleException(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                /*case LogType.Error:
                    break;
                case LogType.Assert:
                    break;
                case LogType.Warning:
                    break;
                case LogType.Log:
                    break;*/
                case LogType.Log:
                case LogType.Assert:
                case LogType.Error:
                    break;
                case LogType.Exception:
                    //FIXME May want to show something else depending on the debug condition
                    Alert.ShowAlert("Error Occured", $"<b>{condition}</b>\n{stackTrace}", "Okay", null);
                    break;
                default:
                    return;
            }
            
            _loggedErrors.Add(new ErrorInfo
            {
                Type = type,
                Condition = condition,
                StackTrace = stackTrace
            });
            
            /*if (type == LogType.Exception)
            {
                MessageBox.Show(condition + "\n" + stackTrace);
            }*/
        }
        
        //============================================================================================================//

    }
}


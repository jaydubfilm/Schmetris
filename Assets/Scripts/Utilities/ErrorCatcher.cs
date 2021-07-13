using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.UI;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Trello;
using UnityEngine;
using Input = UnityEngine.Input;

namespace StarSalvager.Utilities
{
    public class ErrorCatcher : Singleton<ErrorCatcher>
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
        
        private static bool _isExceptionHandlingSetup;

        private static bool _pauseRecording;

        
        //============================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            SetupExceptionHandling();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                throw new Exception("Testing Exception");
            }
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;
            
            _isExceptionHandlingSetup = false;
            Application.logMessageReceived -= HandleException;
        }

        private void OnDisable()
        {
            if (Instance != this)
                return;
            
            _isExceptionHandlingSetup = false;
            Application.logMessageReceived -= HandleException;
        }

        //============================================================================================================//

        private static void SetupExceptionHandling()
        {
            if (_isExceptionHandlingSetup) 
                return;
            
            _isExceptionHandlingSetup = true;
            Application.logMessageReceived += HandleException;
            _loggedErrors = new List<ErrorInfo>();
        }

        private static void HandleException(string condition, string stackTrace, LogType type)
        {
            if (_pauseRecording)
                return;
            
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
                    _pauseRecording = true;
                    TrelloSender.Instance.TakeEarlyScreenShot();
                    TrelloSender.Instance.SaveStateInfo(
                        GameManager.CurrentGameState,
                        InputManager.CurrentInputDeviceName, 
                        InputManager.CurrentActionMap);
                    //FIXME May want to show something else depending on the debug condition
                    Alert.ShowAlert("Error Occured", $"<b>{condition}</b>\n{stackTrace}", "Okay", "Send Bug", 
                        answer =>
                        {
                            if (answer) return;
                            
                            TrelloSender.Instance.OpenBugSubmissionWindow(() =>
                            {
                                _pauseRecording = false;
                            }, false);
                        });
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


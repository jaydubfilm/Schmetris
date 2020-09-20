using System;
using Sirenix.OdinInspector;
using StarSalvager.UI;
using UnityEngine;

namespace StarSalvager.Utilities
{
    public class ErrorCatcher : MonoBehaviour
    {
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

#if UNITY_EDITOR

        [Button("Throw Test Exception"), DisableInEditorMode]
        private void Test()
        {
            throw new Exception("Text Exception");
        }
        
        #endif

        //============================================================================================================//

        static bool isExceptionHandlingSetup;
        private static void SetupExceptionHandling()
        {
            if (isExceptionHandlingSetup) 
                return;
            
            isExceptionHandlingSetup = true;
            Application.logMessageReceived += HandleException;
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
                case LogType.Exception:
                    Alert.ShowAlert("Error Occured", $"<b>{condition}</b>\n{stackTrace}", "Okay", null);
                    break;
            }
            /*if (type == LogType.Exception)
            {
                MessageBox.Show(condition + "\n" + stackTrace);
            }*/
        }
        
        //============================================================================================================//

    }
}


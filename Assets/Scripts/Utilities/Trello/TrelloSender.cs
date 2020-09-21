using System;
using System.Collections;
using System.Collections.Generic;
using DG.TrelloAPI;
using DG.Util;
using StarSalvager.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.Utilities.Trello
{
    public class TrelloSender : Singleton<TrelloSender>
    {
        private const string BOARD = "Salvager";
        private const string CATEGORY = "BUGS";
        
        private readonly string KEY = Base64.Decode("M2VjMGYzMDkxNTVmNmM5MTc2ZDA0NmU4NDFiZDM2ZjQ=");
        private readonly string TOKEN = Base64.Decode("YTA1NjNkYTJhYjM3ODE4OTU5NmMyNDY4OTRhNTkxMzFlNTEzNWZlYTcxMWViZDI0MTgyN2Q5YTVjNzMwOTg4OA==");

        //====================================================================================================================//

        [SerializeField]
        private GameObject bugWindowObject;
        [SerializeField]
        private GameObject processingSendObject;
        [SerializeField]
        private KeyCode openWindowKey = KeyCode.F12;

        [SerializeField]
        private TMP_InputField summaryText;
        [SerializeField]
        private TMP_InputField descriptionText;
        [SerializeField]
        private Button submitButton;
        [SerializeField]
        private Button cancelButton;
        [SerializeField]
        private RawImage rawImage;

        private Texture2D _screenshot;
        //====================================================================================================================//

        // Trello API obj
        private DG.TrelloAPI.Trello trello;

        //====================================================================================================================//

        // Platform dependent Log Path
        private string logPath
        {
            get
            {

#if UNITY_STANDALONE_WIN
                var absolutePath = "%USERPROFILE%/AppData/LocalLow/" + Application.companyName + "/" + Application.productName + "/output_log.txt";
                var filePath = System.Environment.ExpandEnvironmentVariables(absolutePath);
                return filePath;
                //Old windows log path
                //return System.Diagnostics.Process.GetCurrentProcess().ProcessName + "_Data/output_log.txt";

#elif UNITY_STANDALONE_LINUX
                return "~/.config/unity3d/" + Application.companyName + "/" + Application.productName + "/Player.log";

#elif UNITY_STANDALONE_OSX
                return "~/Library/Logs/Unity/Player.log";
#else
                return "";
#endif
            }
        }
        // Platform dependent Log Path copy
        private string logPathCopy
        {
            get
            {
#if UNITY_STANDALONE_WIN
                var absolutePath = "%USERPROFILE%/AppData/LocalLow/" + Application.companyName + "/" + Application.productName + "/output_logCopy.txt";
                var filePath = System.Environment.ExpandEnvironmentVariables(absolutePath);
                return filePath;
                //Old windows log path
                //return System.Diagnostics.Process.GetCurrentProcess().ProcessName + "_Data/output_log2.txt";

#elif UNITY_STANDALONE_LINUX
                return "~/.config/unity3d/" + Application.companyName + "/" + Application.productName + "/PlayerCopy.log";

#elif UNITY_STANDALONE_OSX
               return "~/Library/Logs/Unity/PlayerCopy.log";
#else
                return "";
#endif
            }
        }

        //====================================================================================================================//
        
        private IEnumerator Start()
        {
            submitButton.onClick.AddListener(SendReport);
            cancelButton.onClick.AddListener(()=>
            {
                bugWindowObject.SetActive(false);
                GameTimer.SetPaused(false);
            });
            bugWindowObject.SetActive(false);
            
            //Checks if we are already connected
            if (trello != null && trello.IsConnected())
            {
                Debug.Log("Connection with Trello server succesful");
                yield break;
            } 

            // Creates our trello Obj with our key and token
            trello = new DG.TrelloAPI.Trello(KEY, TOKEN);
            
            // gets the boards of the current user
            yield return trello.PopulateBoardsRoutine(); 
            trello.SetCurrentBoard(BOARD);
            
            // gets the lists on the current board
            yield return trello.PopulateListsRoutine();

            if (!trello.IsListCached(CATEGORY))
            {
                var optionList = trello.NewList(CATEGORY);
                yield return trello.UploadListRoutine(optionList);
            }

            // caches the new lists created (if any)
            yield return trello.PopulateListsRoutine();

            
            
        }

        private void Update()
        {
            if (Input.GetKeyDown(openWindowKey))
            {
                OpenBugSubmissionWindow();
            }
        }

        //====================================================================================================================//

        private void OpenBugSubmissionWindow()
        {
            if (bugWindowObject.activeInHierarchy) 
                return;

            
            StartCoroutine(TakeScreenshotRoutine(() =>
            {
                rawImage.texture = _screenshot;
                
                summaryText.text = string.Empty;
                descriptionText.text = string.Empty;
                
                bugWindowObject.SetActive(true);
                
                GameTimer.SetPaused(true);

            }));


        }

        private void SendReport()
        {
            var title = $"{summaryText.text} - {Application.platform}";
            var description = string.Join("\n", new[]
            {
                descriptionText.text,
                "\n",
                "SYSTEM DETAILS:",
                "====================",
                $"Platform: {Application.platform}",
                $"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}"
            });

            
            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(description))
                return;
            
            TrelloCard card = trello.NewCard(title, description, CATEGORY);
            StartCoroutine(SendReportCoroutine(card, _screenshot, 
                () =>
                {
                    processingSendObject.SetActive(true);
                },
                () =>
                {
                    GameTimer.SetPaused(false);
                    processingSendObject.SetActive(false);
                    bugWindowObject.SetActive(false);
                    
                    Toast.AddToast("Bug Submitted", horizontalLayout: Toast.Layout.End);
                }));
        }
        
        //====================================================================================================================//

        /*private IEnumerator SendReportCoroutine(TrelloCard card, List<Texture2D> screenshots)
        {
            // Shows the "in progress" text
            inProgressUIObject.SetActive(true);

            // We upload the card with an async custom coroutine that will return the card ID
            // Once it has been uploaded.
            CustomCoroutine cC = new CustomCoroutine(this, trello.UploadCardRoutine(card));
            yield return cC.coroutine;

            // The uploaded card ID
            string cardID = (string)cC.result;

            int i = 0;
            foreach (Texture2D screenshot in screenshots)
            {
                i++;
                // We can now attach the screenshot to the card given its ID.
                yield return trello.SetUpAttachmentInCardRoutine(cardID, $"ScreenShot{i}.png", screenshot);
            }

#if UNITY_STANDALONE
            // We make sure the log exists before trying to retrieve it.
            if (System.IO.File.Exists(logPath))
            {
                // We make a copy of the log since the original is being used by Unity.
                System.IO.File.Copy(logPath, logPathCopy, true);

                // We attach the Unity log file to the card.
                yield return trello.SetUpAttachmentInCardFromFileRoutine(cardID, "output_log.txt", logPathCopy);
            }
#endif

            // Wait for one extra second to let the player read that his issue is being processed
            yield return new WaitForSeconds(1);

            // Since we are done we can deactivate the in progress canvas
            inProgressUIObject.SetActive(false);

            // Now we show the success text to let the user know the action has been completed
            StartCoroutine(SetActiveForSecondsRoutine(successUIObject, 2));
        }*/
        
        private IEnumerator SendReportCoroutine(TrelloCard card, Texture2D screenshot, Action PreCallCallback, Action OnSuccessCallback)
        {
            PreCallCallback?.Invoke();

            // We upload the card with an async custom coroutine that will return the card ID
            // Once it has been uploaded.
            CustomCoroutine cC = new CustomCoroutine(this, trello.UploadCardRoutine(card));
            yield return cC.coroutine;

            // The uploaded card ID
            string cardID = (string)cC.result;

            yield return trello.SetUpAttachmentInCardRoutine(cardID, "ScreenShot.png", screenshot);

#if UNITY_STANDALONE
            // We make sure the log exists before trying to retrieve it.
            if (System.IO.File.Exists(logPath))
            {
                // We make a copy of the log since the original is being used by Unity.
                System.IO.File.Copy(logPath, logPathCopy, true);

                // We attach the Unity log file to the card.
                yield return trello.SetUpAttachmentInCardFromFileRoutine(cardID, "output_log.txt", logPathCopy);
            }
#endif

            // Wait for one extra second to let the player read that his issue is being processed
            yield return new WaitForSeconds(1);
            
            OnSuccessCallback?.Invoke();
        }

        // Sets gameObject active or inactive for timeInSeconds
        private IEnumerator SetActiveForSecondsRoutine(GameObject gameObject, float timeInSeconds, bool setActive = true)
        {
            gameObject.SetActive(setActive);
            yield return new WaitForSeconds(timeInSeconds);
            gameObject.SetActive(!setActive);
        }

        //====================================================================================================================//
        
        /*private Coroutine SendReportCoroutine(string title, string description, string listName, List<Texture2D> screenshots)
        {
            // if both the title and description are empty show warning message to avoid spam
            if (title == "" && description == "")
            {
                StartCoroutine(SetActiveForSecondsRoutine(fillInFormMessageUIObject, 2));
                return null;
            }

            TrelloCard card = trello.NewCard(title, description, listName);
            return StartCoroutine(SendReportCoroutine(card, screenshots));
        }*/

        /*private Coroutine SendReportCoroutine(string title, string description, string listName, Texture2D screenshot)
        {
            List<Texture2D> screenshots = new List<Texture2D> { screenshot };
            return SendReportCoroutine(title, description, listName, screenshots);
        }*/

        //====================================================================================================================//
        
        public IEnumerator TakeScreenshotRoutine(Action onfinishedCallback)
        {
            yield return new WaitForEndOfFrame();
            
            _screenshot = TakeScreenshot();
            
            onfinishedCallback?.Invoke();
        }
        
        private static Texture2D TakeScreenshot()
        {
            // Create a texture the size of the screen, RGBA32 format
            int width = Screen.width;
            int height = Screen.height;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true, true);
            
            // Read screen contents into the texture
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0, true);
            tex.Apply(false);
            
            return tex;
        }

        //====================================================================================================================//
        
    }

}
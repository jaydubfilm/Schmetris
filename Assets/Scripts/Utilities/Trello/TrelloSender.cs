using System;
using System.Collections;
using System.Collections.Generic;
using DG.TrelloAPI;
using DG.Util;
using Sirenix.OdinInspector;
using StarSalvager.UI;
using StarSalvager.Utilities;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Jira;
using StarSalvager.Utilities.Math;
using StarSalvager.Utilities.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Input = UnityEngine.Input;
using StarSalvager.Utilities.UI;

namespace StarSalvager.Utilities.Trello
{
    public class TrelloSender : Singleton<TrelloSender>
    {
        private const string BOARD = "Salvager";
        private const string CATEGORY = "BUGS";
        private readonly string[] LABELS = { "Bug" };
        
        private readonly string KEY = Base64.Decode("M2VjMGYzMDkxNTVmNmM5MTc2ZDA0NmU4NDFiZDM2ZjQ=");
        private readonly string TOKEN = Base64.Decode("YTA1NjNkYTJhYjM3ODE4OTU5NmMyNDY4OTRhNTkxMzFlNTEzNWZlYTcxMWViZDI0MTgyN2Q5YTVjNzMwOTg4OA==");

        //====================================================================================================================//

        private Action _callBack;
        
        [SerializeField]
        private GameObject bugWindowObject;
        [SerializeField]
        private GameObject processingSendObject;
        [SerializeField]
        private KeyCode openWindowKey = KeyCode.F12;

        [SerializeField, Required]
        private TMP_InputField summaryText;
        [SerializeField, Required]
        private TMP_InputField descriptionText;
        [SerializeField, Required]
        private TMP_InputField nameText;
        [SerializeField, Required]
        private Button submitButton;
        [SerializeField, Required]
        private Button cancelButton;
        [SerializeField, Required]
        private RawImage rawImage;

        private Texture2D _screenshot;
        //====================================================================================================================//

        // Trello API obj
        private DG.TrelloAPI.Trello trello;

        //====================================================================================================================//

        //====================================================================================================================//
        
        private IEnumerator Start()
        {
            bool earlyExit = false;
            submitButton.onClick.AddListener(SendReport);
            cancelButton.onClick.AddListener(()=>
            {
                ResetInput();
                UISelectHandler.SendNavigationEvents = true;
                bugWindowObject.SetActive(false);
                GameTimer.SetPaused(false);
                InputManager.SetToExpectedActionMap();
                _callBack?.Invoke();
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
            yield return trello.PopulateBoardsRoutine(() =>
            {
                earlyExit = true;
            });

            if (earlyExit)
                yield break;
            
            trello.SetCurrentBoard(BOARD);
            yield return trello.PopulateLabelsRoutine(); 
            
            
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
                OpenBugSubmissionWindow(null);
            }
        }

        //====================================================================================================================//

        //private ACTION_MAP _previousInputMap;

        public void OpenBugSubmissionWindow(Action callback, in bool takeScreenShot = true)
        {
            //--------------------------------------------------------------------------------------------------------//
            
            void Ready()
            {
                rawImage.texture = _screenshot;
                
                summaryText.text = string.Empty;
                descriptionText.text = string.Empty;

                Files.CreateLogFile();
                
                bugWindowObject.SetActive(true);
                
                GameTimer.SetPaused(true); 
            }

            //--------------------------------------------------------------------------------------------------------//
            
            if (bugWindowObject.activeInHierarchy) 
                return;

            UISelectHandler.SendNavigationEvents = false;

            _callBack = callback;
            
            //_previousInputMap = InputManager.CurrentActionMap;
            InputManager.SwitchCurrentActionMap(ACTION_MAP.MENU);

            if (takeScreenShot)
            {
                StartCoroutine(TakeScreenshotRoutine(Ready));
                return;
            }

            Ready();

        }

        public void TakeEarlyScreenShot()
        {
            StartCoroutine(TakeScreenshotRoutine(null));
        }

        private GameState _gameState;
        private string _deviceName;
        private ACTION_MAP _actionMap;
        public void SaveStateInfo(in GameState gameState, in string deviceName, in ACTION_MAP actionMap)
        {
            _gameState = gameState;
            _deviceName = deviceName;
            _actionMap = actionMap;
        }

        private void SendReport()
        {
            var platform = Application.isEditor ? "Editor" : Application.platform.ToString();
            
            var title = $"{summaryText.text} - {platform}";
            var description = string.Join("\n", new[]
            {
                $"Submitted by: {nameText.text}\n",
                descriptionText.text,
                "\n",
                "SYSTEM DETAILS:",
                "====================",
                $"Platform: {platform}",
                $"Version: {Application.version}",
                $"Game State: {_gameState.ToString()}",
                $"Input Device: {_deviceName}",
                $"Input Action Map: {_actionMap}",
                $"Scene: {SceneLoader.CurrentScene}"
            });

            
            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(description))
                return;
            
            
            
            TrelloCard card = trello.NewCard(title, description, CATEGORY, LABELS);
            StartCoroutine(JiraSender.SendReportCoroutine(card, _screenshot, 
                () =>
                {
                    processingSendObject.SetActive(true);
                },
                () =>
                {
                    GameTimer.SetPaused(false);
                    processingSendObject.SetActive(false);
                    bugWindowObject.SetActive(false);
                    
                    Toast.AddToast("Bug Submitted");

                    ResetInput();
                    
                    _callBack?.Invoke();
                }));
            /*StartCoroutine(SendReportCoroutine(card, _screenshot, 
                () =>
                {
                    processingSendObject.SetActive(true);
                },
                () =>
                {
                    GameTimer.SetPaused(false);
                    processingSendObject.SetActive(false);
                    bugWindowObject.SetActive(false);
                    
                    Toast.AddToast("Bug Submitted");

                    ResetInput();
                }));*/
        }

        private void ResetInput()
        {
            /*InputManager.SwitchCurrentActionMap(_previousInputMap);
            _previousInputMap = ACTION_MAP.NULL;*/
            InputManager.SetToExpectedActionMap();
            Alert.Instance.SetActive(false);
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

            // We make sure the log exists before trying to retrieve it.
            if (System.IO.File.Exists(Files.LOG_DIRECTORY))
            {

                // We attach the Unity log file to the card.
                yield return trello.SetUpAttachmentInCardFromFileRoutine(cardID, "error_log.txt", Files.LOG_DIRECTORY);
            }

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
        
        private IEnumerator TakeScreenshotRoutine(Action onfinishedCallback)
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
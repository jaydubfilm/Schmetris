using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DG.TrelloAPI;
using DG.Util;
using MiniJSON;
using Newtonsoft.Json;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Utilities.Math;
using UnityEngine;
using UnityEngine.Networking;

namespace StarSalvager.Utilities.Jira
{
    public class JiraSender : MonoBehaviour
    {
        //Properties
        //====================================================================================================================//
        
        private static readonly string BASE_URL = "agamestudios.atlassian.net";
        private static readonly int PROJECT_ID = 10000;

        private static readonly string USER_ID = Base64.Decode("NWY2M2E0MTBiZGIwNzgwMDcwMWY4NmQ3");
        private static readonly string EMAIL = Base64.Decode("YnVnc0BhZ2FtZXN0dWRpb3MuY2E=");
        private static readonly string API_TOKEN = Base64.Decode("YU9mc1BNN0U1a3hsR284TXBJYzlEMDFF");

        private static string GetAttachmentURL(in string issueID) =>
            $"https://{BASE_URL}/rest/api/3/issue/{issueID}/attachments";

        //Static Coroutines
        //====================================================================================================================//
        
        public static IEnumerator SendReportCoroutine(TrelloCard card, Texture2D screenshot, Action PreCallCallback, Action OnSuccessCallback)
        {
            PreCallCallback?.Invoke();

            // We upload the card with an async custom coroutine that will return the card ID
            // Once it has been uploaded.
            CustomCoroutine cC = new CustomCoroutine(GameManager.Instance, UploadNewIssue(card.name, card.desc));
            yield return cC.coroutine;

            // The uploaded card ID
            string cardID = (string)cC.result;

            yield return SetUpAttachmentInCardRoutine(cardID, "ScreenShot.png", screenshot);

            // We make sure the log exists before trying to retrieve it.
            if (System.IO.File.Exists(Files.LOG_DIRECTORY))
            {

                // We attach the Unity log file to the card.
                yield return SetUpAttachmentInCardFromFileRoutine(cardID, "error_log.txt", Files.LOG_DIRECTORY);
            }

            // Wait for one extra second to let the player read that his issue is being processed
            yield return new WaitForSeconds(1);
            
            OnSuccessCallback?.Invoke();
        }

        private static IEnumerator UploadNewIssue(string title, string body)
        {
            string NEW_ISSUE_URL = $"https://{BASE_URL}/rest/api/3/issue";

            var jsonBody = GetJsonData(title, body);
            
            var request = new UnityWebRequest (NEW_ISSUE_URL, "POST");
            request.SetRequestHeader("AUTHORIZATION", Authenticate(EMAIL, API_TOKEN));
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();

            if (!string.IsNullOrEmpty(request.error))
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                Debug.Log("All OK");
                Debug.Log("Status Code: " + request.responseCode);
            }

            //CheckWwwStatus("Could not upload new card to Trello", request.error);

            var dict = Json.Deserialize(request.downloadHandler.text) as Dictionary<string, object>;

            yield return (string)dict["id"];
        }

        #region File Upload

        private static IEnumerator SetUpAttachmentInCardRoutine(string cardId, string attachmentName, Texture2D image)
        {
            // Encode texture into PNG
            byte[] bytes = image.EncodeToPNG();

            WWWForm form = new WWWForm();
            form.AddBinaryData("file", bytes, attachmentName, "image/png");

            yield return GameManager.Instance.StartCoroutine(SendFileCoroutine(cardId, form));
        }

        private static IEnumerator SetUpAttachmentInCardFromFileRoutine(string cardId, string attachmentName, string path)
        {
            Debug.Assert(File.Exists(path), "The path to the log file specified is not correct");

            byte[] bytes = File.ReadAllBytes(path);

            // Create a Web Form
            WWWForm form = new WWWForm();
            form.AddBinaryData("file", bytes, attachmentName, "text/plain");

            yield return GameManager.Instance.StartCoroutine(SendFileCoroutine(cardId, form));
        }

        private static IEnumerator SendFileCoroutine(string issueID, WWWForm form)
        {
            using (UnityWebRequest request = UnityWebRequest.Post(GetAttachmentURL(issueID), form))
            {
                request.SetRequestHeader("X-Atlassian-Token", "nocheck");
                request.SetRequestHeader("AUTHORIZATION", Authenticate(EMAIL, API_TOKEN));
                
                yield return request.SendWebRequest();

                if (!string.IsNullOrEmpty(request.error))
                {
                    Debug.LogError("Error: " + request.error+"\n"+request.downloadHandler.text);
                }
                else
                {
                    Debug.Log("All OK");
                    Debug.Log("Status Code: " + request.responseCode);
                }
            }
        }

        #endregion //File Upload

        //Static Functions
        //====================================================================================================================//
        
        private static string Authenticate(string username, string password)
        {
            var auth = username + ":" + password;
            auth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
            auth = "Basic " + auth;
            return auth;
        }

        private static string GetJsonData(in string title, in string body)
        {
            var @object = new
            {
                fields = new
                {
                    summary = title,
                    issuetype = new
                    {
                        id = 10005
                    },
                    project = new
                    {
                        id = PROJECT_ID
                    },
                    description = new
                    {
                        type = "doc",
                        version = 1,
                        content = new []
                        {
                            new
                            {
                                type = "paragraph",
                                content = new []
                                {
                                    new
                                    {
                                        text = body,
                                        type = "text"
                                    }
                                }
                            }
                        }
                    },
                    reporter = new
                    {
                        id = USER_ID
                    },
                    priority = new
                    {
                        id = "3"
                    }
                }
            };

            return JsonConvert.SerializeObject(@object);
        }

    }
}

﻿using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using Newtonsoft.Json;
using StarSalvager.Utilities.Analytics.SessionTracking;
using StarSalvager.Utilities.Analytics.SessionTracking.Data;
using StarSalvager.Utilities.Converters;
using StarSalvager.Utilities.JSON.Converters;
using StarSalvager.Utilities.Math;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.Utilities.FileIO
{
    public static class Files
    {
        private const Formatting JSON_FORMAT = Formatting.Indented;

        private const string PLAYER_PATTERN = "*.player";

        private const string BUILDDATA_PATH = "BuildData";
        private const string REMOTE_PATH = "RemoteData";
        private const string ADDTOBUILD_PATH = "AddToBuild";

        private const string BOTSHAPEEDITOR_FILE = "BotShapeEditorData.txt";

        #if UNITY_EDITOR
        private static readonly string PARENT_DIRECTORY = new DirectoryInfo(Application.dataPath).Parent.FullName;
        #elif UNITY_STANDALONE_WIN
        private static readonly string PARENT_DIRECTORY = Application.dataPath;
        #elif UNITY_STANDALONE_OSX
        private static readonly string PARENT_DIRECTORY = Application.persistentDataPath;
        #endif

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        private static readonly string REMOTE_DIRECTORY = Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName, REMOTE_PATH);
#elif UNITY_STANDALONE_OSX
        private static readonly string REMOTE_DIRECTORY = Path.Combine(new DirectoryInfo(Application.persistentDataPath).FullName, REMOTE_PATH);
#endif

        public static string LOG_DIRECTORY
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                var directory = Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName, "RemoteData",
                    "Sessions");
#elif UNITY_STANDALONE_OSX
            var directory = Path.Combine(new DirectoryInfo(Application.persistentDataPath).FullName, "RemoteData",
                "Sessions");
#endif

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);


                return Path.Combine(directory, "error.log");
            }
        }

        //Player Data Directory
        //====================================================================================================================//

        private const string GAME_META_FILE = "GameMetaData.player";

        private static readonly string
            GAME_META_PATH = Path.Combine(REMOTE_DIRECTORY, GAME_META_FILE);

        private static readonly List<string> PlayerAccountSavePaths = new List<string>
        {
            Path.Combine(REMOTE_DIRECTORY, "PlayerRunAccountData0.player"),
            Path.Combine(REMOTE_DIRECTORY, "PlayerRunAccountData1.player"),
            Path.Combine(REMOTE_DIRECTORY, "PlayerRunAccountData2.player"),
            Path.Combine(REMOTE_DIRECTORY, "PlayerRunAccountData3.player")
        };

        public static string GetPlayerAccountSavePath(int saveSlot)
        {
            return PlayerAccountSavePaths[saveSlot];
        }

        //====================================================================================================================//

        public const string SCRAPYARD_LAYOUT_FILE = "ScrapyardLayoutData.txt";

        private static readonly JsonConverter[] CONVERTERS = {
            new IBlockDataArrayConverter(),
            new ComboRecordDataConverter(),
            new EnumBoolDictionaryConverter<PART_TYPE>(),
            new PatchDictionaryConverter()
        };


        //Bot Shape Editor Remote Data
        //====================================================================================================================//

        #region Bot Shape Editor

        public static string ExportBotShapeRemoteData(EditorBotShapeGeneratorData editorData)
        {
            if (editorData == null)
                return string.Empty;

#if UNITY_EDITOR
            var path = Path.Combine(REMOTE_DIRECTORY, ADDTOBUILD_PATH, BOTSHAPEEDITOR_FILE);
#elif UNITY_STANDALONE_WIN
            var path = Path.Combine(PARENT_DIRECTORY, BUILDDATA_PATH, BOTSHAPEEDITOR_FILE);
#elif UNITY_STANDALONE_OSX
            var path = Path.Combine(Application.dataPath, BUILDDATA_PATH, BOTSHAPEEDITOR_FILE);
#endif
            var jsonToExport = JsonConvert.SerializeObject(editorData, JSON_FORMAT);


            File.WriteAllText(path, jsonToExport);


            return jsonToExport;
        }

        public static EditorBotShapeGeneratorData ImportBotShapeRemoteData()
        {


#if UNITY_EDITOR
            var path = Path.Combine(REMOTE_DIRECTORY, ADDTOBUILD_PATH, BOTSHAPEEDITOR_FILE);
#elif UNITY_STANDALONE_WIN
            var path = Path.Combine(PARENT_DIRECTORY, BUILDDATA_PATH, BOTSHAPEEDITOR_FILE);
#elif UNITY_STANDALONE_OSX
            var path = Path.Combine(Application.dataPath, BUILDDATA_PATH, BOTSHAPEEDITOR_FILE);
#endif

            if (!File.Exists(path))
            {
                return new EditorBotShapeGeneratorData();
            }

            return ImportJsonData<EditorBotShapeGeneratorData>(path, new IBlockDataArrayConverter());
        }

        #endregion //Bot Shape Editor

        //Player Data
        //====================================================================================================================//

        #region Player Data

        public static bool DoesSaveExist(int index)
        {
            if (!Directory.Exists(REMOTE_DIRECTORY))
                return false;

            return File.Exists(PlayerAccountSavePaths[index]);
        }

        public static bool TryGetPlayerSaveData(int index, out PlayerSaveAccountData accountData)
        {
            accountData = null;
            var result = DoesSaveExist(index);

            if (!result)
                return false;

            accountData = TryImportPlayerSaveAccountData(index);

            return true;
        }

        public static int GetNextAvailableSaveSlot()
        {
            for (int i = 0; i < PlayerAccountSavePaths.Count; i++)
            {
                if (!Directory.Exists(REMOTE_DIRECTORY))
                    Directory.CreateDirectory(REMOTE_DIRECTORY);

                if (!File.Exists(PlayerAccountSavePaths[i]))
                    return i;
            }

            Debug.Log("No available save slots, using slot 0");
            return 0;
        }

        public static GameMetadata ImportGameMetaData()
        {
            if (!Directory.Exists(GAME_META_PATH))
                Directory.CreateDirectory(REMOTE_DIRECTORY);

            if (!File.Exists(GAME_META_PATH))
            {
                GameMetadata data = new GameMetadata();
                return data;
            }

            return ImportJsonData<GameMetadata>(GAME_META_PATH);
        }

        public static string ExportGameMetaData(GameMetadata editorData)
        {
            var export = JsonConvert.SerializeObject(editorData, JSON_FORMAT);
            File.WriteAllText(GAME_META_PATH, export);

            return export;
        }

        public static PlayerSaveAccountData TryImportPlayerSaveAccountData(int saveSlotIndex)
        {
            if (!Directory.Exists(PlayerAccountSavePaths[saveSlotIndex]))
                Directory.CreateDirectory(REMOTE_DIRECTORY);

            if (!File.Exists(PlayerAccountSavePaths[saveSlotIndex]))
            {
                return null;
            }

            var loaded = ImportJsonData<PlayerSaveAccountData>(
                PlayerAccountSavePaths[saveSlotIndex],
                CONVERTERS);

            return loaded;
        }

        public static string ExportPlayerSaveAccountData(PlayerSaveAccountData playerSaveAccountData, int saveSlotIndex)
        {
            var export = ExportJsonData(playerSaveAccountData,
                PlayerAccountSavePaths[saveSlotIndex],
                CONVERTERS);

            return export;
        }

        public static void DestroyPlayerSaveFile(int index)
        {
            if (!Directory.Exists(REMOTE_DIRECTORY))
                return;

            if(!File.Exists(PlayerAccountSavePaths[index]))
                return;

            File.Delete(PlayerAccountSavePaths[index]);
        }

        #endregion //Player Data

        //Session Summary Data
        //====================================================================================================================//

        public static void TestSessionData()
        {
            const string fileName = "Test";

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            var directory = Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName, "RemoteData",
                "Sessions");
#elif UNITY_STANDALONE_OSX
            var directory = Path.Combine(new DirectoryInfo(Application.persistentDataPath).FullName, "RemoteData",
                "Sessions");
#endif

            if (!Directory.Exists(directory)) 
                Directory.CreateDirectory(directory);


            var path = Path.Combine(directory, $"{fileName}");

            File.WriteAllText(path, string.Empty);
        }
        
        //TODO Move this to the Files location
        public static void ExportSessionData(string playerID, SessionData sessionData)
        {
            if (sessionData.waves.Count == 0) return;

            var fileName = Base64.Encode($"{playerID}_{sessionData.date:yyyyMMddHHmm}");

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            var directory = Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName, "RemoteData",
                "Sessions");
#elif UNITY_STANDALONE_OSX
            var directory = Path.Combine(new DirectoryInfo(Application.persistentDataPath).FullName, "RemoteData",
                "Sessions");
#endif

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);


            var path = Path.Combine(directory, $"{fileName}.session");

            var json = ExportJsonData(sessionData,
                path,
                CONVERTERS);

#if !UNITY_EDITOR
            //Sends file to master to review data
            SendSessionData(path, playerID);
#endif
        }

        public static Dictionary<string, List<SessionData>> ImportSessionData(in string directoryPath)
        {
            const string SEARCH_PATTERN = "*.session";
            
            var directory = new DirectoryInfo(directoryPath);

            var files = directory.GetFiles(SEARCH_PATTERN);
            
            var playerSessions = new Dictionary<string, List<SessionData>>();
            foreach (var file in files)
            {
                var sessionData = ImportJsonData<SessionData>(file.FullName, CONVERTERS);

                //Don't want to add any sessions that are no longer supported
                if (sessionData.Version != SessionDataProcessor.VERSION)
                    continue;
                
                if(!playerSessions.ContainsKey(sessionData.PlayerID))
                    playerSessions.Add(sessionData.PlayerID, new List<SessionData>());
                
                playerSessions[sessionData.PlayerID].Add(sessionData);
            }

            return playerSessions;
        }

        private static void SendSessionData(string filePath, string playerID)
        {
            var from = Base64.Decode("YW5hbHl0aWNzQGFnYW1lc3R1ZGlvcy5jYQ==");
            var to = Base64.Decode("");

            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("mail.agamestudios.ca");
            mail.From = new MailAddress(from);
            mail.To.Add(from);
            mail.Subject = "New Player Session File";
            mail.Body = $"New session for {playerID}";

            var attachment = new Attachment(filePath);
            mail.Attachments.Add(attachment);

            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential(from, Base64.Decode("TTMxbWIxMzRSIQ=="));
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);
        }

        //Clearing Remote Data
        //====================================================================================================================//

        public static void ClearRemoteData()
        {
            var directory = new DirectoryInfo(REMOTE_DIRECTORY);

            //FIXME This should be using persistent file names
            var files = new List<FileInfo>();
            files.AddRange(directory.GetFiles(PLAYER_PATTERN));


            foreach (var file in files)
            {
                if(file == null)
                    continue;

                File.Delete(file.FullName);
            }

            if (Application.isPlaying)
            {
                PlayerDataManager.ClearPlayerAccountData();
                PlayerDataManager.ResetGameMetaData();
            }

        }

        //Create Log File
        //====================================================================================================================//
        public static FileInfo CreateLogFile()
        {
            return CreateLogFile(ErrorCatcher.LoggedErrors);
        }
        public static FileInfo CreateLogFile(List<ErrorCatcher.ErrorInfo> loggedErrors)
        {
            //We reverse them to ensure that the newest will be shown at the top
            loggedErrors.Reverse();
            var data = string.Join("\n", loggedErrors);

            var path = LOG_DIRECTORY;

            File.WriteAllText(path, data);

            return new FileInfo(path);
        }

        //====================================================================================================================//

        public static bool TryDeleteFile(string path)
        {
            if (Directory.Exists(path))
            {
                File.Delete(path);
                return true;
            }

            return false;
        }

        private static T ImportJsonData<T>(string path)
        {
            var jsonData = File.ReadAllText(path);

            var settings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
            };

            return JsonConvert.DeserializeObject<T>(jsonData, settings);
        }
        private static T ImportJsonData<T>(string path, params JsonConverter[] converters)
        {
            var jsonData = File.ReadAllText(path);

            var settings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                Converters = converters
            };

            return JsonConvert.DeserializeObject<T>(jsonData, settings);
        }

        private static string ExportJsonData(in object data, in string path, params JsonConverter[] converters)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                Converters = converters
            };

            var export = JsonConvert.SerializeObject(data, settings);

            File.WriteAllText(path, export);

            //return JsonConvert.DeserializeObject<T>(jsonData, settings);

            return export;
        }
    }

}

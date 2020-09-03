using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using Newtonsoft.Json;
using StarSalvager.Factories;
using StarSalvager.Missions;
using StarSalvager.Utilities.Analytics.Data;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.Utilities.FileIO
{
    public static class Files
    {
        private const string PLAYER_PATTERN = "*.player";
        private const string MISSION_PATTERN = "*.mission";
        
        private const string BUILDDATA_PATH = "BuildData";
        private const string REMOTE_PATH = "RemoteData"; 
        private const string ADDTOBUILD_PATH = "AddToBuild"; 
        
        private const string BOTSHAPEEDITOR_FILE = "BotShapeEditorData.txt";
        
        #if UNITY_EDITOR
        private static readonly string PARENT_DIRECTORY = new DirectoryInfo(Application.dataPath).Parent.FullName;
        #else
        private static readonly string PARENT_DIRECTORY = Application.dataPath;
        #endif

        private static readonly string REMOTE_DIRECTORY = Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName, REMOTE_PATH);

        //Player Data Directory
        //====================================================================================================================//
        
        private const string AUTOSAVE_FILE = "Autosave.player";
        private const string PLAYER_PERSISTENT_FILE = "PlayerPersistentMetadata.player";
        
        
        public static readonly string AUTOSAVE_PATH = Path.Combine(REMOTE_DIRECTORY, AUTOSAVE_FILE);

        private static readonly string
            PERSISTENT_META_PATH = Path.Combine(REMOTE_DIRECTORY, PLAYER_PERSISTENT_FILE);

        private static readonly List<string> PersistentDataPaths = new List<string>
        {
            Path.Combine(REMOTE_DIRECTORY, "PlayerPersistentDataSaveFile0.player"),
            Path.Combine(REMOTE_DIRECTORY, "PlayerPersistentDataSaveFile1.player"),
            Path.Combine(REMOTE_DIRECTORY, "PlayerPersistentDataSaveFile2.player"),
            Path.Combine(REMOTE_DIRECTORY, "PlayerPersistentDataSaveFile3.player"),
            Path.Combine(REMOTE_DIRECTORY, "PlayerPersistentDataSaveFile4.player"),
            Path.Combine(REMOTE_DIRECTORY, "PlayerPersistentDataSaveFile5.player")
        };

        //====================================================================================================================//
        
        private const string MISSION_MASTER_FILE = "MissionsMasterData.mission";
        public const string SCRAPYARD_LAYOUT_FILE = "ScrapyardLayoutData.txt";

        
        //Bot Shape Editor Remote Data
        //====================================================================================================================//

        #region Bot Shape Editor

        public static string ExportBotShapeRemoteData(EditorBotShapeGeneratorData editorData)
        {
            if (editorData == null)
                return string.Empty;

#if UNITY_EDITOR
            var path = Path.Combine(PARENT_DIRECTORY, REMOTE_PATH,
                ADDTOBUILD_PATH, BOTSHAPEEDITOR_FILE);
#else
            var path = Path.Combine(PARENT_DIRECTORY, BUILDDATA_PATH, BOTSHAPEEDITOR_FILE);

#endif
            var jsonToExport = JsonConvert.SerializeObject(editorData, Formatting.None);

            
            File.WriteAllText(path, jsonToExport);

            
            return jsonToExport;
        }

        public static EditorBotShapeGeneratorData ImportBotShapeRemoteData()
        {
            
            
#if UNITY_EDITOR
            var path = Path.Combine(REMOTE_DIRECTORY, ADDTOBUILD_PATH, BOTSHAPEEDITOR_FILE);
#else
            var path = Path.Combine(PARENT_DIRECTORY, BUILDDATA_PATH, BOTSHAPEEDITOR_FILE);
#endif
            
            if (!File.Exists(path))
            {
                return new EditorBotShapeGeneratorData();
            }

            return ImportJsonData<EditorBotShapeGeneratorData>(path);
        }

        #endregion //Bot Shape Editor

        //Player Data
        //====================================================================================================================//

        #region Player Data

        public static string GetNextAvailableSaveSlot()
        {
            foreach (var path in PersistentDataPaths)
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(REMOTE_DIRECTORY);

                if (!File.Exists(path))
                    return path;
            }

            return AUTOSAVE_PATH;
        }
        
        public static PlayerMetadata ImportPlayerPersistentMetadata()
        {
            if (!Directory.Exists(PERSISTENT_META_PATH))
                Directory.CreateDirectory(REMOTE_DIRECTORY);

            if (!File.Exists(PERSISTENT_META_PATH))
            {
                PlayerMetadata data = new PlayerMetadata();
                return data;
            }

            return ImportJsonData<PlayerMetadata>(PERSISTENT_META_PATH);
        }
        
        public static string ExportPlayerPersistentMetadata(PlayerMetadata editorData)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            File.WriteAllText(PERSISTENT_META_PATH, export);

            return export;
        }

        public static PlayerData ImportPlayerPersistentData(string saveSlot)
        {
            if (!Directory.Exists(saveSlot))
                Directory.CreateDirectory(REMOTE_DIRECTORY);

            if (!File.Exists(saveSlot))
            {
                PlayerData data = new PlayerData();
                if (!Globals.DisableTestingFeatures)
                {
                    for (int i = 0; i < FactoryManager.Instance.SectorRemoteData.Count; i++)
                    {
                        data.AddSectorProgression(i, 0);
                    }
                }
                else
                {
                    data.AddSectorProgression(0, 0);
                }
                data.PlaythroughID = Guid.NewGuid().ToString();
                
                return data;
            }

            var loaded = JsonConvert.DeserializeObject<PlayerData>(File.ReadAllText(saveSlot));

            return loaded;
        }
        
        public static string ExportPlayerPersistentData(PlayerData editorData, string saveSlot)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            File.WriteAllText(saveSlot, export);

            return export;
            
        }
            #endregion //Player Data

        //Mission Data
        //====================================================================================================================//

        #region Mission Data

        public static string ExportMissionsMasterRemoteData(MissionsMasterData editorData)
        {
            editorData.SaveMissionData();

            if (!Directory.Exists(REMOTE_DIRECTORY))
                Directory.CreateDirectory(REMOTE_DIRECTORY);

            
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            File.WriteAllText(Path.Combine(REMOTE_DIRECTORY, MISSION_MASTER_FILE), export);

            return export;
        }

        public static MissionsMasterData ImportMissionsMasterRemoteData()
        {
            if (!Directory.Exists(REMOTE_DIRECTORY))
                Directory.CreateDirectory(REMOTE_DIRECTORY);

            var path = Path.Combine(REMOTE_DIRECTORY, MISSION_MASTER_FILE);

            if (!File.Exists(path))
            {
                MissionsMasterData masterData = new MissionsMasterData();
                foreach (var mission in FactoryManager.Instance.MissionRemoteData.GenerateMissionData())
                {
                    masterData.m_missionsMasterData.Add(mission.ToMissionData());
                }
                return masterData;
            }

            var loaded = JsonConvert.DeserializeObject<MissionsMasterData>(File.ReadAllText(path));

            return loaded;
        }

        #endregion //Mission Data

        //Drone Design Data
        //====================================================================================================================//

        #region Drone Data

        public static string ExportLayoutData(List<ScrapyardLayout> editorData)
        {
            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
            
            File.WriteAllText(Path.Combine(REMOTE_DIRECTORY, SCRAPYARD_LAYOUT_FILE), export);

            return export;
        }

        public static List<ScrapyardLayout> ImportLayoutData()
        {
            var path = Path.Combine(REMOTE_DIRECTORY, SCRAPYARD_LAYOUT_FILE);
            
            return !File.Exists(path) ? new List<ScrapyardLayout>() : ImportJsonData<List<ScrapyardLayout>>(path);
        }

        #endregion //Drone Data

        //Session Summary Data
        //====================================================================================================================//
        
        //TODO Move this to the Files location
        public static void ExportSessionData(string playerID, SessionData sessionData)
        {
            if (sessionData.waves.Count == 0)
                return;

            var fileName = Base64.Encode($"{playerID}_{sessionData.date:yyyyMMddHHmm}");

            var directory = Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName, "RemoteData",
                "Sessions");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            

            var path = Path.Combine(directory, $"{fileName}.session");

            var json = JsonConvert.SerializeObject(sessionData, Formatting.Indented);
            
            File.WriteAllText(path, json);


#if !UNITY_EDITOR
            //Sends file to master to review data
            SendSessionData(path, playerID);
#endif
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
            files.AddRange(directory.GetFiles(MISSION_PATTERN));


            foreach (var file in files)
            {
                if(file == null)
                    continue;
                
                File.Delete(file.FullName);
            }

            if (Application.isPlaying)
            {
                PlayerPersistentData.ClearPlayerData();
            }

        }

        //====================================================================================================================//

        public static void DeleteFile(string path)
        {
            File.Delete(path);
        }
        
        private static T ImportJsonData<T>(string path)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }
        
    }

}
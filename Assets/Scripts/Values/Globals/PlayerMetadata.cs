using StarSalvager.Utilities.Saving;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StarSalvager.Values
{
    public class PlayerMetadata
    {
        private string path = Application.dataPath + "/RemoteData/";
        private int maxSaveSlots = 6;

        //TODO Get all the save files here
        public List<SaveFileData> SaveFiles = new List<SaveFileData>();

        public PlayerMetadata()
        {
            /*SaveFiles.Clear();
            foreach (var fileName in Directory.GetFiles(path))
            {
                if (fileName.Contains("PlayerPersistentDataSaveFile") && !fileName.Contains(".meta"))
                {
                    SaveFiles.Add(new SaveFileData
                    {
                        Name = fileName,
                        Date = System.IO.File.GetLastWriteTime(fileName),
                        FilePath = fileName,
                        MissionFilePath = fileName.Replace("PlayerPersistentDataSaveFile", "MissionsCurrentDataSaveFile")
                    });
                }
            }*/
        }

        public string GetPathMostRecentFile()
        {
            string saveFileMostRecent = string.Empty;
            DateTime dateTimeMostRecent = new DateTime(2000, 1, 1);
            foreach (var saveFile in SaveFiles)
            {
                if (saveFileMostRecent == string.Empty)
                {
                    saveFileMostRecent = saveFile.FilePath;
                    dateTimeMostRecent = saveFile.Date;
                }
                else if (DateTime.Compare(dateTimeMostRecent, saveFile.Date) < 0)
                {
                    saveFileMostRecent = saveFile.FilePath;
                    dateTimeMostRecent = saveFile.Date;
                }
            }
            
            return saveFileMostRecent;
        }

        public string GetPathMostRecentMissionFile()
        {
            string missionFileMostRecent = string.Empty;
            DateTime dateTimeMostRecent = new DateTime(2000, 1, 1);
            foreach (var saveFile in SaveFiles)
            {
                if (missionFileMostRecent == string.Empty)
                {
                    missionFileMostRecent = saveFile.MissionFilePath;
                    dateTimeMostRecent = saveFile.Date;
                }
                else if (DateTime.Compare(dateTimeMostRecent, saveFile.Date) < 0)
                {
                    missionFileMostRecent = saveFile.FilePath;
                    dateTimeMostRecent = saveFile.Date;
                }
            }

            return missionFileMostRecent;
        }
    }
}
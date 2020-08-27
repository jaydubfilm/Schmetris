using StarSalvager.Utilities.Saving;
using System;
using System.Collections.Generic;

namespace StarSalvager.Values
{
    public class PlayerMetadata
    {
        //TODO Get all the save files here
        public readonly List<SaveFileData> SaveFiles = new List<SaveFileData>();

        public SaveFileData? CurrentSaveFile;


        //====================================================================================================================//
        
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

        //====================================================================================================================//
        
    }
}
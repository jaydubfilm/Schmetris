using System;
using System.Collections.Generic;
using System.Linq;

namespace StarSalvager.Utilities.Saving
{
    public class GameMetadata
    {
        //TODO Get all the save files here
        public List<SaveFileData> SaveFiles = new List<SaveFileData>();

        public SaveFileData? CurrentSaveFile;


        //====================================================================================================================//
        
        public int GetIndexMostRecentSaveFile()
        {
            int saveSlotIndexMostRecent = -1;
            DateTime dateTimeMostRecent = new DateTime(2000, 1, 1);
            foreach (var saveFile in SaveFiles)
            {
                if (DateTime.Compare(dateTimeMostRecent, saveFile.Date) < 0)
                {
                    saveSlotIndexMostRecent = saveFile.SaveSlotIndex;
                    dateTimeMostRecent = saveFile.Date;
                }
            }
            
            return saveSlotIndexMostRecent;
        }

        public void SetCurrentSaveFile(int saveSlotIndex)
        {
            CurrentSaveFile = SaveFiles.FirstOrDefault(s => s.SaveSlotIndex == saveSlotIndex);
        }

        //====================================================================================================================//
        
    }
}
using StarSalvager.Factories;
using StarSalvager.Missions;
using StarSalvager.Utilities.Saving;
using System;
using StarSalvager.Utilities.FileIO;

namespace StarSalvager.Values
{
    public static class PlayerPersistentData
    {
        private static string CurrentSaveFile = string.Empty;

        public static PlayerData PlayerData = new PlayerData();

        public static readonly PlayerMetadata PlayerMetadata = Files.ImportPlayerPersistentMetadata();

        //====================================================================================================================//

        public static void SetCurrentSaveFile(string saveFile)
        {
            PlayerData = Files.ImportPlayerPersistentData(saveFile);
            CurrentSaveFile = saveFile;
            MissionManager.LoadMissionData();
        }

        //====================================================================================================================//

        public static void ResetPlayerData()
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
            foreach (var facilityData in Globals.FacilityInitialData)
            {
                data.UnlockFacilityLevel((FACILITY_TYPE)facilityData.type, facilityData.level, false);
            }

            foreach (var facilityData in Globals.FacilityInitialBlueprintData)
            {
                data.UnlockFacilityBlueprintLevel((FACILITY_TYPE)facilityData.type, facilityData.level);
            }

            PlayerData = data;
            MissionManager.LoadMissionData();

            foreach (var blueprintData in Globals.BlueprintInitialData)
            {
                Blueprint blueprint = new Blueprint
                {
                    name = (PART_TYPE)blueprintData.type + " " + blueprintData.level,
                    partType = (PART_TYPE)blueprintData.type,
                    level = blueprintData.level
                };
                PlayerPersistentData.PlayerData.UnlockBlueprint(blueprint);
            }
        }

        //====================================================================================================================//

        public static void ClearPlayerData()
        {
            PlayerData = null;
        }

        //====================================================================================================================//

        public static void CustomOnApplicationQuit()
        {
            if (CurrentSaveFile != string.Empty)
            {
                Files.ExportPlayerPersistentData(PlayerData, CurrentSaveFile);
            }

            if (PlayerMetadata.CurrentSaveFile == null)
            {
                SaveFileData newSaveFile = new SaveFileData
                {
                    Name = DateTime.Now.ToString(),
                    Date = DateTime.Now,
                    FilePath = CurrentSaveFile
                };

                PlayerMetadata.SaveFiles.Add(newSaveFile);
            }

            Files.ExportPlayerPersistentMetadata(PlayerMetadata);
        }

        //====================================================================================================================//
        
    }
}
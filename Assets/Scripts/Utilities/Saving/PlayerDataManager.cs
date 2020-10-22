using StarSalvager.Missions;
using System;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Values;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using StarSalvager.UI.Scrapyard;
using System.Security.Policy;
using StarSalvager.Utilities.Math;

namespace StarSalvager.Utilities.Saving
{
    public static class PlayerDataManager
    {
        public static int CurrentSaveSlotIndex = 0;

        public static Action OnValuesChanged;
        public static Action OnCapacitiesChanged;

        private static PlayerSaveAccountData PlayerAccountData = new PlayerSaveAccountData();
        private static PlayerSaveRunData PlayerRunData => PlayerAccountData.PlayerRunData;

        private static GameMetadata GameMetaData = Files.ImportGameMetaData();


        //====================================================================================================================//

        public static bool HasPlayerRunData()
        {
            return PlayerAccountData != null && PlayerRunData != null;
        }

        public static void ExportPlayerAccountData(int saveSlotIndex)
        {
            Files.ExportPlayerSaveAccountData(PlayerDataManager.PlayerAccountData, saveSlotIndex);
        }

        //Run Data Functions
        //====================================================================================================================//

        public static List<PlayerResource> GetResources()
        {
            return PlayerRunData.GetResources();
        }

        public static PlayerResource GetResource(BIT_TYPE bitType)
        {
            return PlayerRunData.GetResource(bitType);
        }

        public static IReadOnlyDictionary<COMPONENT_TYPE, int> GetComponents()
        {
            return PlayerRunData.Components;
        }

        public static Dictionary<COMPONENT_TYPE, int> GetComponentsClone()
        {
            return new Dictionary<COMPONENT_TYPE, int>(PlayerRunData.Components);
        }

        public static List<BlockData> GetBlockDatas(bool isRecoveryDrone)
        {
            if (isRecoveryDrone)
            {
                return PlayerRunData.recoveryDroneBlockData;
            }
            else
            {
                return PlayerRunData.mainDroneBlockData;
            }
        }

        public static IReadOnlyList<string> GetDontShowAgainKeys()
        {
            return PlayerRunData.DontShowAgainKeys;
        }

        public static MissionsCurrentData GetMissionsCurrentData()
        {
            return PlayerAccountData.missionsCurrentData;
        }

        public static void SetComponents(COMPONENT_TYPE type, int value)
        {
            PlayerRunData.SetComponents(type, value);

            OnValuesChanged?.Invoke();
        }

        public static void SetComponents(Dictionary<COMPONENT_TYPE, int> componentDictionary)
        {
            PlayerRunData.SetComponents(componentDictionary);

            OnValuesChanged?.Invoke();
        }

        public static void SetBlockDatas(List<BlockData> blockData, bool isRecoveryDrone)
        {
            if (isRecoveryDrone)
            {
                PlayerRunData.SetRecoveryDroneBlockData(blockData);
            }
            else
            {
                PlayerRunData.SetShipBlockData(blockData);
            }
        }

        public static void SetMissionsCurrentData(MissionsCurrentData missionData)
        {
            PlayerAccountData.missionsCurrentData = missionData;
        }

        //============================================================================================================//

        public static void AddPartResources(BlockData blockData, bool isRecursive)
        {
            if (!blockData.ClassType.Equals(nameof(Part)))
                return;

            AddPartResources((PART_TYPE)blockData.Type, blockData.Level, isRecursive);
        }

        public static void AddPartResources(PART_TYPE partType, int level, bool isRecursive)
        {
            CostCalculations.AddPartResources(partType, level, isRecursive);

            OnValuesChanged?.Invoke();
        }

        public static void SubtractPartResources(PART_TYPE partType, int level, bool isRecursive)
        {
            CostCalculations.SubtractPartResources(partType, level, isRecursive);

            OnValuesChanged?.Invoke();
        }

        public static void SubtractResources(IEnumerable<CraftCost> cost)
        {
            CostCalculations.SubtractResources(cost);

            OnValuesChanged?.Invoke();
        }


        public static void SubtractComponents(IEnumerable<CraftCost> cost)
        {
            PlayerRunData.SubtractComponents(cost);

            OnValuesChanged?.Invoke();
        }

        public static void SubtractPartCosts(PART_TYPE partType, int level, bool isRecursive, float costModifier = 1.0f)
        {
            PlayerRunData.SubtractPartCosts(partType, level, isRecursive, costModifier);

            OnValuesChanged?.Invoke();
        }

        public static void AddComponent(COMPONENT_TYPE type, int amount)
        {
            PlayerRunData.AddComponent(type, amount);

            OnValuesChanged?.Invoke();
        }

        public static void SubtractComponent(COMPONENT_TYPE type, int amount)
        {
            PlayerRunData.SubtractComponent(type, amount);

            OnValuesChanged?.Invoke();
        }

        public static void AddDontShowAgainKey(string key)
        {
            PlayerRunData.AddDontShowAgainKey(key);
        }

        //============================================================================================================//

        public static bool CanAffordFacilityBlueprint(TEST_FacilityBlueprint facilityBlueprint)
        {
            return PlayerRunData.CanAffordFacilityBlueprint(facilityBlueprint);
        }

        public static bool CanAffordBits(IEnumerable<CraftCost> levelCost)
        {
            return PlayerRunData.CanAffordBits(levelCost);
        }

        public static bool CanAffordComponents(IEnumerable<CraftCost> levelCost)
        {
            return PlayerRunData.CanAffordComponents(levelCost);
        }

        public static bool CanAffordPart(PART_TYPE partType, int level, bool isRecursive)
        {
            return PlayerRunData.CanAffordPart(partType, level, isRecursive);
        }

        //====================================================================================================================//

        public static void IncreaseRationCapacity(int amount)
        {
            PlayerAccountData.PlayerRunData.RationCapacity += amount;

            OnValuesChanged?.Invoke();
        }

        //====================================================================================================================//

        public static LevelRingNodeTree GetLevelRingNodeTree()
        {
            return PlayerRunData.LevelRingNodeTree;
        }

        public static void AddCompletedNode(int node)
        {
            PlayerRunData.PlayerPreviouslyCompletedNodes.Add(node);
        }

        public static IReadOnlyList<int> GetPlayerPreviouslyCompletedNodes()
        {
            return PlayerRunData.PlayerPreviouslyCompletedNodes;
        }

        public static IReadOnlyList<int> GetShortcutNodes()
        {
            return PlayerRunData.ShortcutNodes;
        }

        //====================================================================================================================//

        public static IReadOnlyList<BlockData> GetCurrentPartsInStorage()
        {
            return PlayerRunData.GetCurrentPartsInStorage();
        }

        public static void SetCurrentPartsInStorage(List<BlockData> blockData)
        {
            PlayerRunData.SetCurrentPartsInStorage(blockData);
        }

        public static void AddPartToStorage(BlockData blockData)
        {
            PlayerRunData.AddPartToStorage(blockData);

            OnValuesChanged?.Invoke();
        }

        public static void RemovePartFromStorage(BlockData blockData)
        {
            PlayerRunData.RemovePartFromStorage(blockData);
            
            OnValuesChanged?.Invoke();
        }

        //====================================================================================================================//

        public static float GetLevelResourceModifier(int sector, int wave)
        {
            return PlayerRunData.GetLevelResourceModifier(sector, wave);
        }

        public static void ReduceLevelResourceModifier(int sector, int wave)
        {
            PlayerRunData.ReduceLevelResourceModifier(sector, wave);
        }

        public static bool CheckIfCompleted(int sector, int waveAt)
        {
            return PlayerRunData.CheckIfCompleted(sector, waveAt);
        }

        //====================================================================================================================//


        //Account Data Functions
        //====================================================================================================================//

        public static int GetGears()
        {
            return PlayerAccountData.Gears;
        }

        public static void ChangeGears(int amount)
        {
            PlayerAccountData.ChangeGears(amount);

            OnValuesChanged?.Invoke();
        }

        public static bool CheckHasFacility(FACILITY_TYPE type, int level = 0)
        {
            return PlayerAccountData.CheckHasFacility(type, level);
        }

        public static void UnlockBlueprint(Blueprint blueprint)
        {
            PlayerAccountData.UnlockBlueprint(blueprint);

            OnValuesChanged?.Invoke();
        }

        public static void UnlockBlueprint(PART_TYPE partType, int level)
        {
            PlayerAccountData.UnlockBlueprint(partType, level);

            OnValuesChanged?.Invoke();
        }

        public static void UnlockAllBlueprints()
        {
            PlayerAccountData.UnlockAllBlueprints();

            OnValuesChanged?.Invoke();
        }

        public static IReadOnlyList<Blueprint> GetUnlockedBlueprints()
        {
            return PlayerAccountData.unlockedBlueprints;
        }

        public static IReadOnlyDictionary<FACILITY_TYPE, int> GetFacilityRanks()
        {
            return PlayerAccountData.facilityRanks;
        }

        public static IReadOnlyDictionary<FACILITY_TYPE, int> GetFacilityBlueprintRanks()
        {
            return PlayerAccountData.facilityBlueprintRanks;
        }

        public static void UnlockFacilityLevel(FACILITY_TYPE facilityType, int level, bool triggerMissionCheck = true)
        {
            PlayerAccountData.UnlockFacilityLevel(facilityType, level, triggerMissionCheck);

            OnValuesChanged?.Invoke();
        }

        public static void UnlockFacilityBlueprintLevel(FacilityBlueprint facilityBlueprint)
        {
            PlayerAccountData.UnlockFacilityBlueprintLevel(facilityBlueprint);

            OnValuesChanged?.Invoke();
        }

        public static void UnlockFacilityBlueprintLevel(FACILITY_TYPE facilityType, int level, bool triggerMissionCheck = true)
        {
            PlayerAccountData.UnlockFacilityLevel(facilityType, level, triggerMissionCheck);

            OnValuesChanged?.Invoke();
        }

        //====================================================================================================================//

        public static void SetCurrentSaveSlotIndex(int saveSlotIndex)
        {
            PlayerAccountData = Files.ImportPlayerSaveAccountData(saveSlotIndex);
            CurrentSaveSlotIndex = saveSlotIndex;
            MissionManager.LoadMissionData();
        }

        public static void ResetPlayerAccountData()
        {
            PlayerSaveAccountData playerAccountData = new PlayerSaveAccountData();
            foreach (var blueprintData in Globals.BlueprintInitialData)
            {
                Blueprint blueprint = new Blueprint
                {
                    name = (PART_TYPE)blueprintData.type + " " + blueprintData.level,
                    partType = (PART_TYPE)blueprintData.type,
                    level = blueprintData.level
                };
                playerAccountData.UnlockBlueprint(blueprint);
            }

            foreach (var facilityData in Globals.FacilityInitialData)
            {
                UnlockFacilityLevel((FACILITY_TYPE)facilityData.type, facilityData.level, false);
            }

            foreach (var facilityData in Globals.FacilityInitialBlueprintData)
            {
                playerAccountData.UnlockFacilityBlueprintLevel((FACILITY_TYPE)facilityData.type, facilityData.level);
            }

            playerAccountData.ResetPlayerRunData();
            PlayerAccountData = playerAccountData;
            PlayerRunData.PlaythroughID = Guid.NewGuid().ToString();

            MissionManager.LoadMissionData();
        }

        public static void ClearPlayerAccountData()
        {
            PlayerAccountData = null;
        }

        public static void ResetPlayerRunData()
        {
            PlayerAccountData.ResetPlayerRunData();
        }

        public static void SavePlayerAccountData()
        {
            if (PlayerAccountData == null)
            {
                return;
            }
            
            Files.ExportPlayerSaveAccountData(PlayerAccountData, CurrentSaveSlotIndex);

            GameMetaData.SaveFiles.RemoveAll(s => s.SaveSlotIndex == CurrentSaveSlotIndex);
            SaveFileData autoSaveFile = new SaveFileData
            {
                Name = DateTime.Now.ToString(),
                Date = DateTime.Now,
                SaveSlotIndex = CurrentSaveSlotIndex
            };
            GameMetaData.SaveFiles.Add(autoSaveFile);
            GameMetaData.CurrentSaveFile = autoSaveFile;

            Files.ExportGameMetaData(GameMetaData);
        }

        //====================================================================================================================//


        //Meta Data Functions
        //====================================================================================================================//

        public static int GetIndexMostRecentSaveFile()
        {
            return GameMetaData.GetIndexMostRecentSaveFile();
        }

        public static void AddSaveFileData(SaveFileData data)
        {
            GameMetaData.SaveFiles.Add(data);
        }

        public static void RemoveSaveFileData(SaveFileData data)
        {
            GameMetaData.SaveFiles.Remove(data);
        }

        public static void RemoveSaveFileData(int index)
        {
            GameMetaData.SaveFiles.RemoveAt(index);
        }

        public static void ClearSaveFileData(SaveFileData data)
        {
            GameMetaData.SaveFiles.Remove(data);
        }

        public static void SetCurrentSaveFile(int index)
        {
            GameMetaData.SetCurrentSaveFile(index);
        }

        public static void ClearCurrentSaveFile()
        {
            GameMetaData.CurrentSaveFile = null;
        }

        public static IReadOnlyList<SaveFileData> GetSaveFiles()
        {
            return GameMetaData.SaveFiles;
        }

        //====================================================================================================================//

        public static void CustomOnApplicationQuit()
        {
            SavePlayerAccountData();
        }

        //====================================================================================================================//
        
    }
}
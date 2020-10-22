using StarSalvager.Missions;
using System;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Values;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using System.CodeDom;
using StarSalvager.UI.Scrapyard;
using System.Security.Policy;

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

        public static IReadOnlyDictionary<BIT_TYPE, int> GetResources()
        {
            return PlayerRunData.readOnlyBits;
        }

        public static IReadOnlyDictionary<COMPONENT_TYPE, int> GetComponents()
        {
            return PlayerRunData.readOnlyComponents;
        }


        public static IReadOnlyDictionary<BIT_TYPE, float> GetLiquidResources(bool isRecoveryDrone)
        {
            if (isRecoveryDrone)
            {
                return PlayerRunData.recoveryDroneLiquidResource;
            }
            else
            {
                return PlayerRunData.liquidResource;
            }
        }

        public static Dictionary<BIT_TYPE, int> GetResourcesClone()
        {
            return new Dictionary<BIT_TYPE, int> (PlayerRunData.readOnlyBits);
        }

        public static Dictionary<COMPONENT_TYPE, int> GetComponentsClone()
        {
            return new Dictionary<COMPONENT_TYPE, int>(PlayerRunData.readOnlyComponents);
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
            return PlayerRunData.missionsCurrentData;
        }

        public static void SetResources(Dictionary<BIT_TYPE, int> values)
        {
            PlayerRunData.SetResources(values);

            OnValuesChanged?.Invoke();
        }

        public static void SetResources(BIT_TYPE type, int value)
        {
            PlayerRunData.SetResources(type, value);

            OnValuesChanged?.Invoke();
        }

        public static void SetComponents(COMPONENT_TYPE type, int value)
        {
            PlayerRunData.SetComponents(type, value);

            OnValuesChanged?.Invoke();
        }

        public static void SetLiquidResource(BIT_TYPE type, float value, bool isRecoveryDrone)
        {
            PlayerRunData.SetLiquidResource(type, value, isRecoveryDrone);

            OnValuesChanged?.Invoke();
        }

        public static void SetLiquidResources(Dictionary<BIT_TYPE, float> liquidValues, bool isRecoveryDrone)
        {
            PlayerRunData.SetLiquidResources(liquidValues, isRecoveryDrone);

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
            PlayerRunData.missionsCurrentData = missionData;
        }

        //============================================================================================================//

        public static void AddResources(Dictionary<BIT_TYPE, int> toAdd, float multiplier)
        {
            PlayerRunData.AddResources(toAdd, multiplier);

            OnValuesChanged?.Invoke();
        }

        public static Dictionary<BIT_TYPE, int> AddResourcesReturnWasted(Dictionary<BIT_TYPE, int> toAdd, float multiplier)
        {
            Dictionary<BIT_TYPE, int> wastedResources = PlayerRunData.AddResourcesReturnWasted(toAdd, multiplier);
            
            OnValuesChanged?.Invoke();

            return wastedResources;
        }

        public static void AddResource(BIT_TYPE type, int amount)
        {
            PlayerRunData.AddResource(type, amount);

            OnValuesChanged?.Invoke();
        }

        public static void AddPartResources(PART_TYPE partType, int level, bool isRecursive)
        {
            PlayerRunData.AddPartResources(partType, level, isRecursive);

            OnValuesChanged?.Invoke();
        }

        public static void AddPartResources(BlockData blockData, bool isRecursive)
        {
            if (!blockData.ClassType.Equals(nameof(Part)))
                return;

            AddPartResources((PART_TYPE)blockData.Type, blockData.Level, isRecursive);

            OnValuesChanged?.Invoke();
        }

        public static void AddLiquidResource(BIT_TYPE type, float amount, bool isRecoveryDrone)
        {
            PlayerRunData.AddLiquidResource(type, amount, isRecoveryDrone);

            OnValuesChanged?.Invoke();
        }

        public static void SubtractResources(BIT_TYPE bitType, int amount)
        {
            PlayerRunData.SubtractResources(bitType, amount);
        }

        public static void SubtractResources(IEnumerable<CraftCost> cost)
        {
            PlayerRunData.SubtractResources(cost);

            OnValuesChanged?.Invoke();
        }

        public static void SubtractLiquidResource(BIT_TYPE type, float amount, bool isRecoveryDrone)
        {
            PlayerRunData.SubtractLiquidResource(type, amount, isRecoveryDrone);

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

        public static IReadOnlyDictionary<BIT_TYPE, int> GetResourceCapacities()
        {
            return PlayerRunData.ResourceCapacities;
        }

        public static void SetCapacity(BIT_TYPE type, int amount, bool isRecoveryDrone)
        {
            PlayerRunData.SetCapacity(type, amount, isRecoveryDrone);

            OnCapacitiesChanged?.Invoke();
        }

        public static void SetCapacities(Dictionary<BIT_TYPE, int> capacities, bool isRecoveryDrone)
        {
            PlayerRunData.SetCapacities(capacities, isRecoveryDrone);

            OnCapacitiesChanged?.Invoke();
        }

        public static IReadOnlyDictionary<BIT_TYPE, int> GetLiquidCapacities(bool isRecoveryDrone)
        {
            if (isRecoveryDrone)
            {
                return PlayerRunData.recoveryDroneLiquidCapacity;
            }
            else
            {
                return PlayerRunData.liquidCapacity;
            }
        }

        public static void ClearLiquidCapacity(bool isRecoveryDrone)
        {
            PlayerRunData.ClearLiquidCapacity(isRecoveryDrone);

            OnCapacitiesChanged?.Invoke();
        }

        public static (float current, float capacity) GetCurrentAndCapacity(BIT_TYPE type, bool isRecoveryDrone)
        {
            return PlayerRunData.GetCurrentAndCapacity(type, isRecoveryDrone);
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

        public static void IncreaseResourceCapacity(BIT_TYPE bitType, int amount)
        {
            PlayerRunData.IncreaseResourceCapacity(bitType, amount);

            OnValuesChanged?.Invoke();
        }

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

        public static void ResetPlayerRunData()
        {
            PlayerAccountData.ResetPlayerRunData();
        }

        public static void SavePlayerAccountData()
        {
            if (PlayerRunData == null)
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
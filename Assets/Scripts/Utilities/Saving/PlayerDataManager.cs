using StarSalvager.Missions;
using System;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Values;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using StarSalvager.UI.Scrapyard;
using StarSalvager.Utilities.Math;
using System.Linq;
using UnityEngine;
using StarSalvager.Factories;
using UnityEditor;
using StarSalvager.Factories.Data;

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

        public static bool HasPlayerAccountData()
        {
            return PlayerAccountData != null;
        }

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

        public static List<BlockData> GetBlockDatas()
        {
            if (Globals.IsRecoveryBot)
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

        public static void SetBlockDatas(List<BlockData> blockData)
        {
            if (Globals.IsRecoveryBot)
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
            var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            AddCraftCostResources(costs);

            if (!isRecursive)
                return;

            if (level > 0)
                AddPartResources(partType, level - 1, isRecursive);
        }

        public static void AddCraftCostResources(List<CraftCost> costs)
        {
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Bit)
                    continue;

                PlayerDataManager.GetResource((BIT_TYPE)resource.type).AddResource(resource.amount, false);
            }
        }

        public static void SubtractPartResources(PART_TYPE partType, int level, bool isRecursive, float costModifier = 1.0f)
        {
            var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            SubtractCraftCostResources(costs);

            if (!isRecursive)
                return;

            if (level > 0)
                SubtractPartResources(partType, level - 1, isRecursive, costModifier);
        }

        public static void SubtractCraftCostResources(List<CraftCost> costs, float costModifier = 1.0f)
        {
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Bit)
                    continue;

                PlayerDataManager.GetResource((BIT_TYPE)resource.type).SubtractResource((int)(resource.amount * costModifier), false);
            }
        }


        public static void SubtractComponents(IEnumerable<CraftCost> cost)
        {
            PlayerDataManager.SubtractCraftCostComponents(cost);

            OnValuesChanged?.Invoke();
        }

        public static void AddComponent(COMPONENT_TYPE type, int amount)
        {
            PlayerRunData.AddComponent(type, amount);

            OnValuesChanged?.Invoke();
        }

        public static void SubtractPartCosts(PART_TYPE partType, int level, bool isRecursive, float costModifier = 1.0f)
        {
            SubtractPartResources(partType, level, isRecursive, costModifier);
            SubtractPartComponents(partType, level, isRecursive);
            SubtractPartPremades(partType, level, isRecursive);

            OnValuesChanged?.Invoke();
        }

        public static void SubtractPartComponents(PART_TYPE partType, int level, bool isRecursive)
        {
            var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            SubtractCraftCostComponents(costs);

            if (!isRecursive)
                return;

            if (level > 0)
                SubtractPartComponents(partType, level - 1, isRecursive);
        }

        public static void SubtractCraftCostComponents(IEnumerable<CraftCost> costs)
        {
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Component)
                    continue;

                PlayerDataManager.SubtractComponent((COMPONENT_TYPE)resource.type, resource.amount);
            }
        }

        public static void SubtractComponent(COMPONENT_TYPE type, int amount)
        {
            PlayerRunData.SubtractComponent(type, amount);

            OnValuesChanged?.Invoke();
        }

        public static void SubtractPartPremades(PART_TYPE partType, int level, bool isRecursive)
        {
            var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            SubtractCraftCostPremades(costs);

            if (!isRecursive)
                return;

            if (level > 0)
                SubtractPartPremades(partType, level - 1, isRecursive);
        }

        public static void SubtractCraftCostPremades(IEnumerable<CraftCost> costs)
        {
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Part)
                    continue;

                PlayerDataManager.SubtractPremade((PART_TYPE)resource.type, resource.partPrerequisiteLevel, resource.amount);
            }
        }

        public static void SubtractPremade(PART_TYPE partType, int level, int amount)
        {
            List<BlockData> storedMatches = PlayerRunData.partsInStorageBlockData.FindAll(p => p.Type == (int)partType && p.Level == level);

            if (storedMatches.Count < amount)
            {
                Debug.LogError("Tried to subtract premade parts that don't exist");
            }

            for (int i = 0; i < amount; i++)
            {
                PlayerRunData.RemovePartFromStorage(storedMatches[0]);
            }

            OnValuesChanged?.Invoke();
        }

        //FIXME This should be stored via Account, not Run
        public static void AddDontShowAgainKey(string key)
        {
            PlayerRunData.AddDontShowAgainKey(key);
        }

        //============================================================================================================//

        public static bool CanAffordPart(PART_TYPE partType, int level, float resourceCostModifier = 1.0f)
        {
            bool hasResources;
            bool hasComponents;
            bool hasParts;

            var resourceCosts = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            hasResources = CanAffordCraftCostResources(resourceCosts);

            var componentCosts = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            hasComponents = CanAffordCraftCostComponents(componentCosts);

            var premadeCosts = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            hasParts = CanAffordCraftCostPremades(premadeCosts);

            return hasResources && hasComponents && hasParts;
        }

        public static bool CanAffordFacilityBlueprint(TEST_FacilityBlueprint facilityBlueprint)
        {
            return PlayerAccountData.GetAvailablePatchPoints() >= facilityBlueprint.patchCost;
        }

        public static bool CanAffordCraftCostResources(List<CraftCost> costs)
        {
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Bit)
                    continue;

                if (PlayerDataManager.GetResource((BIT_TYPE)resource.type).resource < resource.amount)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CanAffordCraftCostComponents(List<CraftCost> costs)
        {
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Component)
                    continue;

                if (PlayerDataManager.GetComponents()[(COMPONENT_TYPE)resource.type] < resource.amount)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CanAffordCraftCostPremades(IEnumerable<CraftCost> costs)
        {
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Part)
                    continue;

                if (resource.type == (int)PART_TYPE.CORE)
                    continue;

                var partCount = PlayerRunData.partsInStorageBlockData.Count(p => p.Type == resource.type && p.Level == resource.partPrerequisiteLevel);

                if (partCount < resource.amount)
                    return false;
            }

            return true;
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

        public static int GetGearsThisRun()
        {
            return PlayerAccountData.Gears - PlayerAccountData.GearsAtRunBeginning;
        }

        public static (int, int) GetPatchPointProgress()
        {
            return PlayerAccountData.GetPatchPointProgress();
        }

        public static int GetTotalPatchPoints()
        {
            return PlayerAccountData.GetTotalPatchPoints();
        }

        public static int GetAvailablePatchPoints()
        {
            return PlayerAccountData.GetAvailablePatchPoints();
        }

        public static void SpendPatchPoints(int amount)
        {
            PlayerAccountData.SpendPatchPoints(amount);
        }

        public static void ChangeGears(int amount)
        {
            PlayerAccountData.ChangeGears(amount);

            OnValuesChanged?.Invoke();
        }

        public static int GetCoreDeaths()
        {
            return PlayerAccountData.CoreDeaths;
        }

        public static int GetCoreDeathsThisRun()
        {
            return PlayerAccountData.CoreDeaths - PlayerAccountData.CoreDeathsAtRunBeginning;
        }

        public static void AddCoreDeath()
        {
            PlayerAccountData.CoreDeaths++;
        }

        public static void RecordBitConnection(BIT_TYPE bit)
        {
            PlayerAccountData.RecordBitConnection(bit);
        }

        public static Dictionary<BIT_TYPE, int> GetBitConnections()
        {
            return PlayerAccountData.BitConnections;
        }

        public static int GetBitConnections(BIT_TYPE bit)
        {
            if (!PlayerAccountData.BitConnections.ContainsKey(bit))
            {
                Debug.LogError("Can't find key in Bitconnections");
            }
            
            return PlayerAccountData.BitConnections[bit];
        }

        public static int GetBitConnectionsThisRun(BIT_TYPE bit)
        {
            if (!PlayerAccountData.BitConnections.ContainsKey(bit) || !PlayerAccountData.BitConnectionsAtRunBeginning.ContainsKey(bit))
            {
                Debug.LogError("Can't find key in Bitconnections or Bitconnectionsatrunbeginning");
            }

            return PlayerAccountData.BitConnections[bit] - PlayerAccountData.BitConnectionsAtRunBeginning[bit];
        }

        public static void RecordEnemyKilled(string enemyId)
        {
            PlayerAccountData.RecordEnemyKilled(enemyId);
        }

        public static Dictionary<string, int> GetEnemiesKilled()
        {
            return PlayerAccountData.EnemiesKilled;
        }

        public static int GetEnemiesKilled(string enemyId)
        {
            if (!PlayerAccountData.EnemiesKilled.ContainsKey(enemyId))
            {
                return 0;
            }

            return PlayerAccountData.EnemiesKilled[enemyId];
        }

        public static int GetEnemiesKilledhisRun(string enemyId)
        {
            int enemiesKilledTotal = GetEnemiesKilled(enemyId);
            int enemiesKilledAtRunBeginning;

            if (!PlayerAccountData.EnemiesKilledAtRunBeginning.ContainsKey(enemyId))
            {
                enemiesKilledAtRunBeginning = 0;
            }
            else
            {
                enemiesKilledAtRunBeginning = PlayerAccountData.EnemiesKilledAtRunBeginning[enemyId];
            }


            return enemiesKilledTotal - enemiesKilledAtRunBeginning;
        }

        public static float GetRepairsDone()
        {
            return PlayerAccountData.RepairsDone;
        }

        public static float GetRepairsDoneThisRun()
        {
            return PlayerAccountData.RepairsDone - PlayerAccountData.RepairsDoneAtRunBeginning;
        }

        public static void AddRepairsDone(float amount)
        {
            PlayerAccountData.RepairsDone += amount;
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
            CurrentSaveSlotIndex = saveSlotIndex;
            PlayerAccountData = Files.ImportPlayerSaveAccountData(saveSlotIndex);
            MissionManager.LoadMissionData();
        }

        public static void ResetPlayerAccountData()
        {
            PlayerSaveAccountData playerAccountData = new PlayerSaveAccountData();
            playerAccountData.ResetPlayerRunData();
            PlayerAccountData = playerAccountData;
            PlayerRunData.PlaythroughID = Guid.NewGuid().ToString();

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
                playerAccountData.UnlockFacilityLevel((FACILITY_TYPE)facilityData.type, facilityData.level, false);
            }

            List<FacilityRemoteData> remoteData = FactoryManager.Instance.FacilityRemote.GetRemoteDatas();
            foreach (var facilityData in remoteData)
            {
                playerAccountData.UnlockFacilityBlueprintLevel((FACILITY_TYPE)facilityData.type, facilityData.levels.Count - 1);
            }

            MissionManager.LoadMissionData();
        }

        public static void ResetGameMetaData()
        {
            GameMetaData = new GameMetadata();
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

﻿using System;
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
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.UI;
using Random = UnityEngine.Random;

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

        public static Version GetVersion()
        {
            return PlayerAccountData.Version;
        }

        public static bool GetHasRunStarted()
        {
            return HasPlayerRunData() && PlayerRunData.runStarted;
        }

        public static void SetRunStarted()
        {
            PlayerRunData.runStarted = true;
        }

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
            Files.ExportPlayerSaveAccountData(PlayerAccountData, saveSlotIndex);
        }

        //Run Data Functions
        //====================================================================================================================//

        public static int GetCurrentNode()
        {
            return PlayerRunData.CurrentNode;
        }

        public static void SetCurrentNode(int node)
        {
            PlayerRunData.CurrentNode = node;
        }

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

        public static List<IBlockData> GetBlockDatas()
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

        public static void SetBlockData(List<IBlockData> blockData)
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

        //============================================================================================================//

        public static void AddPartResources(IBlockData blockData, bool isRecursive)
        {
            if (!blockData.ClassType.Equals(nameof(Part)))
                return;

            AddPartResources((PART_TYPE)blockData.Type, 0, isRecursive);
        }

        public static void AddPartResources(PART_TYPE partType, int level, bool isRecursive)
        {
            /*var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            AddCraftCostResources(costs);

            if (!isRecursive)
                return;

            if (level > 0)
                AddPartResources(partType, level - 1, isRecursive);*/
        }

        public static void AddCraftCostResources(List<CraftCost> costs)
        {
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Bit)
                    continue;

                GetResource((BIT_TYPE)resource.type).AddResource(resource.amount, false);
            }
        }

        public static void SubtractPartResources(PART_TYPE partType, float costModifier = 1.0f)
        {
            /*var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            SubtractCraftCostResources(costs);

            if (!isRecursive)
                return;

            if (level > 0)
                SubtractPartResources(partType, level - 1, isRecursive, costModifier);*/
        }

        public static void SubtractCraftCostResources(List<CraftCost> costs, float costModifier = 1.0f)
        {
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Bit)
                    continue;

                GetResource((BIT_TYPE)resource.type).SubtractResource((int)(resource.amount * costModifier), false);
            }
        }


        public static void SubtractComponents(IEnumerable<CraftCost> cost)
        {
            SubtractCraftCostComponents(cost);

            OnValuesChanged?.Invoke();
        }

        public static void AddComponent(COMPONENT_TYPE type, int amount, bool updateValuesChanged = true)
        {
            PlayerRunData.AddComponent(type, amount);

            if (updateValuesChanged)
                OnValuesChanged?.Invoke();
        }

        public static void SubtractPartCosts(PART_TYPE partType, float costModifier = 1.0f)
        {
            SubtractPartResources(partType, costModifier);
            SubtractPartComponents(partType);
            SubtractPartPremades(partType);

            OnValuesChanged?.Invoke();
        }

        public static void SubtractPartComponents(PART_TYPE partType)
        {
            /*var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            SubtractCraftCostComponents(costs);

            if (!isRecursive)
                return;

            if (level > 0)
                SubtractPartComponents(partType, level - 1, isRecursive);*/
        }

        public static void SubtractCraftCostComponents(IEnumerable<CraftCost> costs)
        {
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Component)
                    continue;

                SubtractComponent((COMPONENT_TYPE)resource.type, resource.amount);
            }
        }

        public static void SubtractComponent(COMPONENT_TYPE type, int amount)
        {
            PlayerRunData.SubtractComponent(type, amount);

            OnValuesChanged?.Invoke();
        }

        public static void SubtractPartPremades(PART_TYPE partType)
        {
            /*var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            SubtractCraftCostPremades(costs);

            if (!isRecursive)
                return;

            if (level > 0)
                SubtractPartPremades(partType, level - 1, isRecursive);*/
        }

        public static void SubtractCraftCostPremades(IEnumerable<CraftCost> costs)
        {
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Part)
                    continue;

                SubtractPremade((PART_TYPE)resource.type, resource.partPrerequisiteLevel, resource.amount);
            }
        }

        public static void SubtractPremade(PART_TYPE partType, int level, int amount)
        {
            List<IBlockData> storedMatches = PlayerRunData.partsInStorageBlockData.FindAll(p => p.Type == (int)partType);

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

        public static bool CanAffordPart(PART_TYPE partType)
        {
            /*bool hasResources;
            bool hasComponents;
            bool hasParts;

            var resourceCosts = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            hasResources = CanAffordCraftCostResources(resourceCosts);

            var componentCosts = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            hasComponents = CanAffordCraftCostComponents(componentCosts);

            var premadeCosts = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            hasParts = CanAffordCraftCostPremades(premadeCosts);

            return hasResources && hasComponents && hasParts;*/

            return true;
        }

        public static bool CanAffordCraftCostResources(List<CraftCost> costs)
        {
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Bit)
                    continue;

                if (GetResource((BIT_TYPE)resource.type).resource < resource.amount)
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

                if (GetComponents()[(COMPONENT_TYPE)resource.type] < resource.amount)
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

                var partCount = PlayerRunData.partsInStorageBlockData.Count(p => p.Type == resource.type );

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

        public static LevelNodeTree GetLevelRingNodeTree()
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

        public static IReadOnlyList<int> GetWreckNodes()
        {
            return PlayerRunData.WreckNodes;
        }

        //====================================================================================================================//

        public static IReadOnlyList<IBlockData> GetCurrentPartsInStorage()
        {
            return PlayerRunData.GetCurrentPartsInStorage();
        }

        public static void SetCurrentPartsInStorage(List<IBlockData> blockData)
        {
            PlayerRunData.SetCurrentPartsInStorage(blockData);
        }

        public static void AddPartToStorage(IBlockData blockData)
        {
            PlayerRunData.AddPartToStorage(blockData);

            OnValuesChanged?.Invoke();
        }

        public static void RemovePartFromStorage(IBlockData blockData)
        {
            PlayerRunData.RemovePartFromStorage(blockData);

            OnValuesChanged?.Invoke();
        }

        public static void RemovePartFromStorageAtIndex(int index)
        {
            PlayerRunData.RemovePartFromStorageAtIndex(index);

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

        public static void AddGearsToGetPatchPoints(int numPatchPointsToGet)
        {
            PlayerAccountData.AddGearsToGetPatchPoints(numPatchPointsToGet);

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

        public static void UnlockBlueprint(Blueprint blueprint)
        {
            PlayerAccountData.UnlockBlueprint(blueprint);

            OnValuesChanged?.Invoke();
        }

        public static void UnlockBlueprint(PART_TYPE partType)
        {
            PlayerAccountData.UnlockBlueprint(partType);

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

        //============================================================================================================//

        public static bool CheckHasBlueprintAlert(Blueprint blueprint)
        {
            return PlayerAccountData.PlayerNewAlertData.CheckHasBlueprintAlert(blueprint);
        }

        public static bool CheckHasAnyBlueprintAlerts()
        {
            return PlayerAccountData.PlayerNewAlertData.CheckHasAnyBlueprintAlerts();
        }

        public static void AddNewBlueprintAlert(Blueprint blueprint)
        {
            PlayerAccountData.PlayerNewAlertData.AddNewBlueprintAlert(blueprint);
        }

        public static void ClearNewBlueprintAlert(Blueprint blueprint)
        {
            PlayerAccountData.PlayerNewAlertData.ClearNewBlueprintAlert(blueprint);
        }

        public static void ClearAllBlueprintAlerts()
        {
            PlayerAccountData.PlayerNewAlertData.ClearAllBlueprintAlerts();
        }

        //====================================================================================================================//

        public static void SetCurrentSaveSlotIndex(int saveSlotIndex)
        {
            CurrentSaveSlotIndex = saveSlotIndex;
            PlayerSaveAccountData tryImportPlayerAccountData = Files.TryImportPlayerSaveAccountData(saveSlotIndex);
            if (tryImportPlayerAccountData == null)
            {
                ResetPlayerAccountData();
            }
            else
            {
                PlayerAccountData = tryImportPlayerAccountData;
            }
            SavePlayerAccountData();
        }

        public static void ResetPlayerAccountData()
        {
            PlayerSaveAccountData playerAccountData = new PlayerSaveAccountData();
            PlayerAccountData = playerAccountData;
            playerAccountData.ResetPlayerRunData();
            PlayerRunData.PlaythroughID = Guid.NewGuid().ToString();

            foreach (var blueprintData in Globals.BlueprintInitialData)
            {
                Blueprint blueprint = new Blueprint
                {
                    name = $"{((PART_TYPE)blueprintData.type)}",
                    partType = (PART_TYPE)blueprintData.type
                };
                playerAccountData.UnlockBlueprint(blueprint);
            }

            SavePlayerAccountData();
        }

        public static void ResetGameMetaData()
        {
            GameMetaData = new GameMetadata();
        }

        public static void ClearPlayerAccountData()
        {
            PlayerAccountData = new PlayerSaveAccountData();
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

            if (PlayerAccountData.PlayerRunData.PlaythroughID == "")
            {
                Debug.LogError("Saving empty player run data");
                Debug.Break();
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


        //Hints
        //====================================================================================================================//

        public static void SetHint(HINT hint, bool value)
        {
            PlayerAccountData.SetHintDisplay(hint, value);
        }

        public static bool GetHint(HINT hint)
        {
            return PlayerAccountData.HintDisplay[hint];
        }

        public static IReadOnlyDictionary<HINT, bool> GetHints()
        {
            return PlayerAccountData.HintDisplay;
        }
        

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
            GameMetaData.SaveFiles.RemoveAll(s => s.SaveSlotIndex == index);
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
            if (!GameManager.IsState(GameState.MainMenu))
            {
                SavePlayerAccountData();
            }
        }

        //====================================================================================================================//


        public static string GetAccountSummaryString()
        {
            string summaryText = string.Empty;
            summaryText += $"Total Gears: {GetGears()}, this run: {GetGearsThisRun()}\n";
            summaryText += $"Total Core Deaths: {GetCoreDeaths()}, this run: {GetCoreDeathsThisRun()}\n";
            summaryText += $"Total Repairs Done: {GetRepairsDone()}, this run: {GetRepairsDoneThisRun()}\n";


            if (GetBitConnections().Count > 0)
            {
                summaryText += ("<b>Bits Connected:</b>\n");

                foreach (var keyValuePair in GetBitConnections())
                {
                    summaryText += $"\t{keyValuePair.Key}: {keyValuePair.Value}, this run: {GetBitConnectionsThisRun(keyValuePair.Key)}\n";
                }
            }

            if (GetEnemiesKilled().Count > 0)
            {
                summaryText += ("<b>Enemies Killed:</b>\n");

                foreach (var keyValuePair in GetEnemiesKilled())
                {
                    summaryText += $"\t{keyValuePair.Key}: {keyValuePair.Value}, this run: {GetEnemiesKilledhisRun(keyValuePair.Key)}\n";
                }
            }

            return summaryText;
        }
        
        public static string GetRunSummaryString()
        {
            string summaryText = string.Empty;
            summaryText += $"{GetAsTitle("Total Gears:")} {GetGearsThisRun()}\n";
            summaryText += $"{GetAsTitle("Total Core Deaths:")}  {GetCoreDeathsThisRun()}\n";
            summaryText += $"{GetAsTitle("Total Repairs Done:")}  {GetRepairsDoneThisRun()}\n";


            if (GetBitConnections().Count > 0)
            {
                summaryText += ($"{GetAsTitle("Bits Connected")}\n");

                foreach (var keyValuePair in GetBitConnections())
                {
                    summaryText += $"\t{TMP_SpriteMap.GetBitSprite(keyValuePair.Key, 0)} = {GetBitConnectionsThisRun(keyValuePair.Key)}\n";
                }
            }

            if (GetEnemiesKilled().Count > 0)
            {
                var enemyProfileData = FactoryManager.Instance.EnemyProfile;
                
                summaryText += ($"{GetAsTitle("Enemies Killed")}\n");

                foreach (var keyValuePair in GetEnemiesKilled())
                {
                    var spriteName = enemyProfileData.GetEnemyProfileData(keyValuePair.Key).Sprite?.name;
                
                    summaryText += $"\t{TMP_SpriteMap.GetEnemySprite(spriteName)} = {GetEnemiesKilledhisRun(keyValuePair.Key)}\n";
                }
            }

            return summaryText;
        }

        public static void DestroyAccountData()
        {
            PlayerAccountData = null;
        }

        private static string GetAsTitle(in string value)
        {
            return $"<b><color=white>{value}</color></b>";
        }
    }
}

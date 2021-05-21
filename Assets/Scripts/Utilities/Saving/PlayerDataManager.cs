using System;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Values;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StarSalvager.Factories.Data;
using UnityEngine;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Puzzle.Data;
using StarSalvager.Utilities.Puzzle.Structs;

namespace StarSalvager.Utilities.Saving
{
    public static class PlayerDataManager
    {
        public static Action<PlayerLevelRemoteData.UnlockData> OnItemUnlocked;
        public static Action OnValuesChanged;
        public static Action OnCapacitiesChanged;

        //Properties
        //====================================================================================================================//

        #region Properties

        public static Version Version => PlayerAccountData.Version;

        public static int CurrentSaveSlotIndex;

        private static PlayerSaveAccountData PlayerAccountData = new PlayerSaveAccountData();
        private static PlayerSaveRunData PlayerRunData => PlayerAccountData.PlayerRunData;

        private static GameMetadata GameMetaData = Files.ImportGameMetaData();

        #endregion //Properties

        //Summary Strings
        //====================================================================================================================//
        public static string GetAccountSummaryString() => PlayerAccountData.GetSummaryString();

        public static string GetRunSummaryString() => PlayerRunData.GetSummaryString();
        
        //Upgrades
        //====================================================================================================================//

        #region Upgrades

        public static float GetCurrentUpgradeValue(in UPGRADE_TYPE upgradeType, in BIT_TYPE bitType = BIT_TYPE.NONE) =>
            PlayerAccountData?.GetCurrentUpgradeValue(upgradeType, bitType) ?? default;

        public static void SetUpgradeLevel(in UPGRADE_TYPE upgradeType, in int newLevel,
            in BIT_TYPE bitType = BIT_TYPE.NONE) =>
            PlayerAccountData.SetUpgradeLevel(upgradeType, bitType, newLevel);

        public static int GetCurrentUpgradeLevel(in UPGRADE_TYPE upgradeType, in BIT_TYPE bitType = BIT_TYPE.NONE) =>
            PlayerAccountData.GetCurrentUpgradeLevel(upgradeType, bitType);

        #endregion //Upgrades
        
        //XP Functions
        //====================================================================================================================//

        #region XP

        public static int GetXP() => PlayerAccountData.XP;

        public static int GetXPThisRun() => PlayerAccountData.GetXPThisRun();

        public static void ChangeXP(in int amount) => PlayerAccountData.AddXP(amount);

        #endregion //XP

        //Stars
        //====================================================================================================================//

        #region Stars

        public static int GetStarsThisRun() => PlayerAccountData.GetStarsThisRun();
        public static int GetStars() => PlayerAccountData.Stars;
        public static void SetStars(in int value) => PlayerAccountData.SetStars(value);
        public static void AddStars(in int amount = 1) => PlayerAccountData.AddStars(amount);
        public static bool TrySubtractStars(in int amount = 1) => PlayerAccountData.TrySubtractStars(amount);

        #endregion //Stars

        //Unlocks
        //====================================================================================================================//

        #region Unlocks

        public static bool IsPartUnlocked(in PART_TYPE partType) => PlayerAccountData.IsPartUnlocked(partType);
        public static void UnlockPart(in PART_TYPE partType) => PlayerAccountData.UnlockPart(partType);
        
        public static bool IsPatchUnlocked(in PatchData patchData) => PlayerAccountData.IsPatchUnlocked(patchData);
        public static void UnlockPatch(in PatchData patchData) => PlayerAccountData.UnlockPatch(patchData);

        #endregion //Unlocks
        
        //Run Status
        //====================================================================================================================//

        #region Run Status

        public static int GetRunCount() => PlayerAccountData.TotalRuns; 

        public static bool ShouldShownSummary()
        {
            if (!HasRunData)
                return false;
            
            if (!HasRunStarted())
                return false;
            if (!PlayerRunData.hasCompleted)
                return false;
            if (PlayerRunData.hasShownSummary)
                return false;
            return true;
        }

        public static bool HasRunStarted()
        {
            return PlayerAccountData.PlayerRunData.hasStarted;
        }
        public static void SetRunStarted(bool started)
        {
            PlayerRunData.hasStarted = started;
        }
        
        public static bool HasActiveRun()
        {
            return HasRunData && PlayerRunData.hasStarted && !PlayerRunData.hasCompleted;
        }
        
        public static bool HasRunData => !(PlayerAccountData == null || PlayerRunData == null);

        #endregion //Run Status

        //Bot Data
        //====================================================================================================================//

        #region Bot Data

        public static List<Vector2Int> GetBotLayout()
        {
            return PlayerSaveAccountData.BotLayout.Keys.ToList();
        }

        public static BIT_TYPE GetCategoryAtCoordinate(Vector2Int coordinate)
        {
            return PlayerAccountData.GetCategoryAtCoordinate(coordinate);
        }

        public static Vector2Int GetCoordinateForCategory(BIT_TYPE bitType)
        {
            return PlayerAccountData.GetCoordinateForCategory(bitType);
        }

        public static List<IBlockData> GetBlockDatas() => HasRunData ? PlayerRunData.DroneBlockData : default;

        public static void SetBlockData(IEnumerable<IBlockData> blockData)
        {
            PlayerRunData.SetDroneBlockData(blockData);
        }
        
        public static void DowngradeAllBits(int removeBelowLevel, bool downgradeBits)
        {
            //--------------------------------------------------------------------------------------------------------//

            void RemoveBit(ref List<IBlockData> blockDatas, in Vector2Int coordinate)
            {
                var tempCoordinate = coordinate;
                //Need to make sure that we only remove Bits and NOT parts!
                var index = blockDatas.FindIndex(x => x is BitData bitData && bitData.Coordinate == tempCoordinate);

                if (index < 0)
                {
#if UNITY_EDITOR
                    throw new ArgumentOutOfRangeException(nameof(index), index,
                        $"Trying to remove bit at [{coordinate}] which was not in List:\n{JsonConvert.SerializeObject(blockDatas)}");
#else
                    return;
#endif
                }


                blockDatas.RemoveAt(index);
            }

            //--------------------------------------------------------------------------------------------------------//
            
            var droneBlockData = new List<IBlockData>(GetBlockDatas());
            var originalBackup = new List<IBlockData>(droneBlockData);

            var bitsToRemove = droneBlockData
                .OfType<BitData>()
                .Where(x => x.Level < removeBelowLevel && x.Type != (int)BIT_TYPE.WHITE)
                .OrderBy(x => x.Coordinate.magnitude)
                .ToList();

            for (int i = bitsToRemove.Count - 1; i >= 0; i--)
            {
                var bitData = bitsToRemove[i];
                
                var orphanData = new List<OrphanMoveBlockData>();
                droneBlockData.CheckForOrphansFromProcessing(bitData, ref orphanData);

                //droneBlockData.Remove(bitData);
                RemoveBit(ref droneBlockData, bitData.Coordinate);
                bitsToRemove.RemoveAt(i);
                for (int ii = 0; ii < orphanData.Count; ii++)
                {
                    var data = orphanData[ii];
                    var index = droneBlockData.FindIndex(x => x.Coordinate == data.startingCoordinates);
                    
                    droneBlockData[index].Coordinate = data.intendedCoordinates;
                }
            }

            //Review all the bits (After having moved) to ensure there is no one floating
            if (droneBlockData.OfType<BitData>().Any(bitData => !droneBlockData.HasPathToCore(bitData)))
            {
                throw new Exception($"No Path to Core found\nOriginal: {JsonConvert.SerializeObject(originalBackup)}\nSolved: {JsonConvert.SerializeObject(droneBlockData)}");
            }

            if (downgradeBits)
            {
                for (int i = 0; i < droneBlockData.Count; i++)
                {
                    if(!(droneBlockData[i] is BitData bitData) || bitData.Level < removeBelowLevel)
                        continue;

                    bitData.Level -= 1;
                    droneBlockData[i] = bitData;
                }
            }

           
            SetBlockData(droneBlockData);
        }

        #endregion //Bot Data
        
        //Bot Health
        //====================================================================================================================//

        #region Bot Health

        public static float GetBotHealth() => HasRunData ? PlayerRunData.currentBotHealth : 0;

        public static void SetBotHealth(in float health)
        {
            PlayerRunData.currentBotHealth = health;
        }

        #endregion //Bot Health
        
        //Storage Parts
        //====================================================================================================================//

        #region Storage Parts

        public static IReadOnlyList<IBlockData> GetCurrentPartsInStorage()
        {
            return PlayerRunData.GetCurrentPartsInStorage();
        }

        public static void SetCurrentPartsInStorage(IEnumerable<IBlockData> blockData)
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

        #endregion //Storage Parts
        
        //Part Selection
        //====================================================================================================================//

        #region Part Selection

        public static bool CanChoosePart => HasRunData ? PlayerRunData.canChoosePart : false;

        public static void SetCanChoosePart(bool canChoosePart)
        {
            PlayerRunData.canChoosePart = canChoosePart;
        }

        #endregion //Part Selection
        
        //Patches
        //====================================================================================================================//

        #region Patches
        public static IReadOnlyList<PatchData> PurchasedPatches => PlayerRunData.GetPurchasedPatches();
        public static IReadOnlyList<PatchData> CurrentPatchOptions => PlayerRunData.CurrentPatchOptions;
        public static void SetCurrentPatchOptions(in IEnumerable<PatchData> patches) => PlayerRunData.SetCurrentPatchOptions(patches);
        public static void ClearAllPatches()=> PlayerRunData.ClearAllPatches();
        public static void RemovePatchAtIndex(in int index) => PlayerRunData.RemovePatchAtIndex(index);

        #endregion //Patches

        //Player Resources
        //====================================================================================================================//

        #region Player Resources

        public static List<PlayerResource> GetResources()
        {
            return PlayerRunData.GetResources();
        }

        public static PlayerResource GetResource(BIT_TYPE bitType)
        {
            return PlayerRunData.GetResource(bitType);
        }

        #endregion //Player Resources
        
        //Gears
        //============================================================================================================//

        #region Gears

        public static int GetGears() =>HasRunData ? PlayerRunData.Gears : 0;
        public static void SetGears(int value)
        {
            PlayerRunData.SetGears(value);

            OnValuesChanged?.Invoke();
        }
        
        public static void AddGears(int amount, bool updateValuesChanged = true)
        {
            PlayerRunData.AddGears(amount);

            if (updateValuesChanged)
                OnValuesChanged?.Invoke();
        }

        public static void SubtractGears(int amount)
        {
            PlayerRunData.SubtractGears(amount);

            OnValuesChanged?.Invoke();
        }

        #endregion //Gears

        //Silver
        //====================================================================================================================//

        #region Silver

        public static int GetSilver() => PlayerRunData.Silver;
        public static void SetSilver(int value)
        {
            PlayerRunData.SetSilver(value);

            OnValuesChanged?.Invoke();
        }
        
        public static void AddSilver(int amount, bool updateValuesChanged = true)
        {
            PlayerRunData.AddSilver(amount);

            if (updateValuesChanged)
                OnValuesChanged?.Invoke();
        }

        public static void SubtractSilver(int amount)
        {
            PlayerRunData.SubtractSilver(amount);
            OnValuesChanged?.Invoke();
        }

        #endregion //Silver

        //Map Nodes
        //====================================================================================================================//

        #region Map Nodes
        
        public static bool CheckIfCompleted(in int waveAt)
        {
            return PlayerRunData.CheckIfCompleted(waveAt);
        }

        public static int GetCurrentNode()
        {
            return PlayerRunData.currentNode;
        }

        public static void SetCurrentNode(int node)
        {
            PlayerRunData.currentNode = node;
        }

        
        
        public static void AddCompletedNode(int node)
        {
            PlayerRunData.playerPreviouslyCompletedNodes.Add(node);
        }

        public static IReadOnlyList<int> GetPlayerPreviouslyCompletedNodes()
        {
            return PlayerRunData.playerPreviouslyCompletedNodes;
        }

        public static IReadOnlyList<int> GetWreckNodes()
        {
            return PlayerRunData.wreckNodes;
        }

        #endregion //Map Nodes

        //Progress Data
        //====================================================================================================================//
        public static int GetSector() => PlayerRunData.currentSector;
        public static int GetWave() => PlayerRunData.currentWave;
        
        public static void SetSectorWave(in int sector, in int wave)
        {
            PlayerRunData.SetSectorWave(sector, wave);
        }

        //Player Run Data Recording
        //====================================================================================================================//

        #region Player Run Data Tracking

        

        public static void RecordCombo(in ComboRecordData comboRecordData) => PlayerAccountData.RecordCombo(comboRecordData);
        public static IReadOnlyDictionary<ComboRecordData, int> GetCombosMade() => PlayerAccountData.CombosMade;

        public static IReadOnlyDictionary<ComboRecordData, int> GetCombosMadeThisRun()
        {
            var outDict = new Dictionary<ComboRecordData, int>(PlayerAccountData.CombosMade);
            foreach (var kvp in PlayerAccountData.CombosMade)
            {
                if (!PlayerRunData.CombosMadeAtBeginning.TryGetValue(kvp.Key, out var startValue))
                {
                    outDict[kvp.Key] = kvp.Value;
                    continue;
                }
                
                outDict[kvp.Key] = kvp.Value - startValue;
            }

            return outDict;
        
        }

        public static void RecordBitConnection(in BIT_TYPE bit) => PlayerAccountData.RecordBitConnection(bit);

        public static IReadOnlyDictionary<BIT_TYPE, int> GetBitConnections() => PlayerAccountData.BitConnections;

        public static int GetBitConnections(in BIT_TYPE bit) => PlayerAccountData.BitConnections[bit];

        public static int GetBitConnectionsThisRun(in BIT_TYPE bit) => 
            PlayerAccountData.BitConnections[bit] - PlayerRunData.BitConnectionsAtRunBeginning[bit];

        public static IReadOnlyDictionary<BIT_TYPE, int> GetBitConnectionsThisRun()
        {
            var outDict = new Dictionary<BIT_TYPE, int>(PlayerAccountData.BitConnections);
            foreach (var kvp in PlayerAccountData.BitConnections)
            {
                if (!PlayerRunData.BitConnectionsAtRunBeginning.TryGetValue(kvp.Key, out var startValue))
                {
                    outDict[kvp.Key] = kvp.Value;
                    continue;
                }
                
                outDict[kvp.Key] = kvp.Value - startValue;
            }

            return outDict;
        }

        public static void RecordEnemyKilled(in string enemyId) => PlayerAccountData.RecordEnemyKilled(enemyId);

        public static IReadOnlyDictionary<string, int> GetEnemiesKilled() => PlayerAccountData.EnemiesKilled;

        public static int GetEnemiesKilled(in string enemyId)
        {
            var id = enemyId;
            return GetEnemiesKilled()
                .FirstOrDefault(x => x.Key.Equals(id))
                .Value;
        }

        public static IReadOnlyDictionary<string, int> GetEnemiesKilledThisRun()
        {
            var outDict = new Dictionary<string, int>(PlayerAccountData.EnemiesKilled);
            foreach (var kvp in PlayerAccountData.EnemiesKilled)
            {
                if (!PlayerRunData.EnemiesKilledAtRunBeginning.TryGetValue(kvp.Key, out var startValue))
                {
                    outDict[kvp.Key] = kvp.Value;
                    continue;
                }
                
                outDict[kvp.Key] = kvp.Value - startValue;
            }

            return outDict;
        }
        public static int GetEnemiesKilledThisRun(in string enemyId)
        {
            var id = enemyId;
            var atBeginning = PlayerRunData.EnemiesKilledAtRunBeginning
                .FirstOrDefault(x => x.Key.Equals(id))
                .Value;
            
            return GetEnemiesKilled()
                .FirstOrDefault(x => x.Key.Equals(id))
                .Value - atBeginning;
        }

        public static float GetRepairsDone() => PlayerAccountData.RepairsDone;

        public static float GetRepairsDoneThisRun() =>
            PlayerAccountData.RepairsDone - PlayerRunData.RepairsDoneAtRunBeginning;

        public static void AddRepairsDone(in float amount) => PlayerAccountData.RepairsDone += amount;

        #endregion //Player Run Data Tracking

        //Hints
        //====================================================================================================================//

        #region Hints

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

        #endregion //Hints

        //Data Validation
        //====================================================================================================================//
        
        #region Data Validation

        public static void ValidateData() => PlayerAccountData.ValidateData();
        
        #endregion //Data Validation

        //Misc Functions
        //====================================================================================================================//

        #region Misc

        public static void AddDontShowAgainKey(string key)
        {
            PlayerRunData.AddDontShowAgainKey(key);
        }
        public static void CustomOnApplicationQuit()
        {
            if (!GameManager.IsState(GameState.MainMenu))
            {
                SavePlayerAccountData();
            }
        }
        
        public static IReadOnlyList<string> GetDontShowAgainKeys()
        {
            return PlayerRunData.DontShowAgainKeys;
        }

        #endregion //Misc

        //Run Data I/O
        //====================================================================================================================//

        #region Run Data I/O

        public static void CompleteCurrentRun() =>PlayerAccountData.CompleteCurrentRun();

        public static void StartNewPlayerRun() => PlayerAccountData.StartNewRun();

        #endregion //Run Data I/O

        //Account Data I/O
        //====================================================================================================================//

        #region Account Data I/O
        
        public static bool HasPlayerAccountData()
        {
            return PlayerAccountData != null;
        }

        

        public static void ClearPlayerAccountData()
        {
            PlayerAccountData = new PlayerSaveAccountData();
            
        }
        public static void DestroyAccountData()
        {
            PlayerAccountData = null;
        }
        public static void ResetPlayerAccountData()
        {
            PlayerAccountData = new PlayerSaveAccountData();

            SavePlayerAccountData();
        }
        public static void SavePlayerAccountData()
        {
            if (PlayerAccountData == null)
            {
                return;
            }

            if (PlayerAccountData.PlayerRunData?.PlaythroughID == "")
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

        #endregion //Account Data I/O
        
        //Save Data I/O
        //====================================================================================================================//

        #region Save Data I/O
        
        public static void ExportPlayerAccountData(int saveSlotIndex)
        {
            Files.ExportPlayerSaveAccountData(PlayerAccountData, saveSlotIndex);
        }

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

        
        public static void ResetGameMetaData()
        {
            GameMetaData = new GameMetadata();
        }
        
        
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
        
        

        #endregion //Save Data I/O

        //====================================================================================================================//
        
    }
}

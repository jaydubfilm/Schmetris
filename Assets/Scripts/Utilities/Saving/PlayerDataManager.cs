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
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.Extensions;
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

        public static List<List<Vector2Int>> botLayoutOptions = new List<List<Vector2Int>>()
        {
            new List<Vector2Int>()
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(-1, 0),
                new Vector2Int(0, -1)
            },
            new List<Vector2Int>()
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(1, -1),
                new Vector2Int(-1, -1)
            },
            new List<Vector2Int>()
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(-2, 0),
                new Vector2Int(2, 1),
                new Vector2Int(2, -1),
                new Vector2Int(-2, 1),
                new Vector2Int(-2, -1)
            },
            new List<Vector2Int>()
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(-2, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
                new Vector2Int(-1, 1),
                new Vector2Int(0, 2)
            },
            new List<Vector2Int>()
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(-2, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2),
                new Vector2Int(0, -1),
                new Vector2Int(0, -2)
            },
            new List<Vector2Int>()
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
                new Vector2Int(-1, 0),
                new Vector2Int(0, -1),
                new Vector2Int(-1, -1)
            },
            new List<Vector2Int>()
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(2, 1),
                new Vector2Int(2, 2),
                new Vector2Int(-1, 0),
                new Vector2Int(-2, 0),
                new Vector2Int(-2, -1),
                new Vector2Int(-2, -2)
            },
            new List<Vector2Int>()
            {
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2),
                new Vector2Int(1, 2),
                new Vector2Int(2, 2),
                new Vector2Int(-1, 2),
                new Vector2Int(-2, 2),
                new Vector2Int(2, 1),
                new Vector2Int(2, 0),
                new Vector2Int(-2, 1),
                new Vector2Int(-2, 0)
            },
            new List<Vector2Int>()
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
                new Vector2Int(-1, 1),
                new Vector2Int(0, -1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, -1)
            },
            new List<Vector2Int>()
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(-2, 0),
                new Vector2Int(1, 1),
                new Vector2Int(0, 1),
                new Vector2Int(-1, 1),
                new Vector2Int(0, 2),
                new Vector2Int(1, -1),
                new Vector2Int(0, -1),
                new Vector2Int(-1, -1),
                new Vector2Int(0, -2)
            }
        };

        //====================================================================================================================//

        public static Version GetVersion()
        {
            return PlayerAccountData.Version;
        }

        public static bool GetCanChoosePart()
        {
            return PlayerRunData.CanChoosePart;
        }

        public static void SetCanChoosePart(bool canChoosePart)
        {
            PlayerRunData.CanChoosePart = canChoosePart;
        }

        public static bool GetHasRunStarted()
        {
            return HasPlayerRunData() && PlayerRunData.runStarted;
        }

        public static void SetRunStarted()
        {
            //PlayerRunData.currentBotHealth = Globals.BotStartingHealth;
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

        public static List<Vector2Int> GetBotLayout()
        {
            return PlayerAccountData._botLayout;
        }

        public static void SetBotLayout(int index)
        {
            PlayerAccountData._botLayout = botLayoutOptions[index];
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

        public static int GetComponents()
        {
            return PlayerRunData.Components;
        }

        /*public static IReadOnlyDictionary<COMPONENT_TYPE, int> GetComponents()
        {
            return PlayerRunData.Components;
        }*/

        /*public static Dictionary<COMPONENT_TYPE, int> GetComponentsClone()
        {
            return new Dictionary<COMPONENT_TYPE, int>(PlayerRunData.Components);
        }*/

        public static List<IBlockData> GetBlockDatas()
        {
            return PlayerRunData.mainDroneBlockData;
        }

        public static IReadOnlyList<string> GetDontShowAgainKeys()
        {
            return PlayerRunData.DontShowAgainKeys;
        }

        public static void SetComponents(int value)
        {
            PlayerRunData.SetComponents(value);

            OnValuesChanged?.Invoke();
        }

        public static void SetBlockData(List<IBlockData> blockData)
        {
            PlayerRunData.SetShipBlockData(blockData);
        }
        
        public static void DowngradeAllBits(int removeBelowLevel, bool downgradeBits)
        {
            void RemoveBit(ref List<IBlockData> blockDatas, in Vector2Int coordinate)
            {
                var data = coordinate;
                var index = blockDatas.FindIndex(x => x.Coordinate == data);

                blockDatas.RemoveAt(index);
            }

            var droneBlockData = new List<IBlockData>(GetBlockDatas());
            var originalBackup = new List<IBlockData>(droneBlockData);

            
            var bitsToRemove = droneBlockData
                .OfType<BitData>()
                .Where(x => x.Level < removeBelowLevel)
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
                throw new Exception($"No Path to Core found\nOriginal: {Newtonsoft.Json.JsonConvert.SerializeObject(originalBackup)}\nSolved: {Newtonsoft.Json.JsonConvert.SerializeObject(droneBlockData)}");
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
            Globals.StripBits = false;
        }

        //============================================================================================================//

        public static void AddComponent(int amount, bool updateValuesChanged = true)
        {
            PlayerRunData.AddComponent(amount);

            if (updateValuesChanged)
                OnValuesChanged?.Invoke();
        }

        public static void SubtractComponent(int amount)
        {
            PlayerRunData.SubtractComponent(amount);

            OnValuesChanged?.Invoke();
        }

        //FIXME This should be stored via Account, not Run
        public static void AddDontShowAgainKey(string key)
        {
            PlayerRunData.AddDontShowAgainKey(key);
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

        //Bot Health
        //====================================================================================================================//

        public static float GetBotHealth()
        {
            return PlayerRunData.currentBotHealth;
        }

        public static void SetBotHealth(in float health)
        {
            PlayerRunData.currentBotHealth = health;
        }
        
        //Parts
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

        //Patches
        //====================================================================================================================//

        public static IReadOnlyList<PatchData> GetCurrentPatchesInStorage()
        {
            return PlayerRunData.GetCurrentPatchesInStorage();
        }
        
        public static void AddPatchToStorage(PatchData patchData)
        {
            PlayerRunData.AddPatchToStorage(patchData);

            OnValuesChanged?.Invoke();
        }

        public static void RemovePatchFromStorage(PatchData patchData)
        {
            PlayerRunData.RemovePatchFromStorage(patchData);

            OnValuesChanged?.Invoke();
        }

        public static void RemovePatchFromStorageAtIndex(int index)
        {
            PlayerRunData.RemovePatchFromStorageAtIndex(index);

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

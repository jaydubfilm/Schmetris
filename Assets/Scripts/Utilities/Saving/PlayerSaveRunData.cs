using Newtonsoft.Json;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JSON.Converters;
using StarSalvager.Utilities.Puzzle.Structs;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.Utilities.Saving
{
    [Serializable]
    public class PlayerSaveRunData
    {
        //Properties
        //====================================================================================================================//
        
        #region Properties

        [JsonProperty]
        public string PlaythroughID { get; private set; }
        
        
        public bool hasCompleted;
        public bool hasStarted;
        
        //Starting values
        //====================================================================================================================//

        #region Starting Values

        public readonly int XPAtRunBeginning;
        [JsonConverter(typeof(DecimalConverter))]
        public readonly float RepairsDoneAtRunBeginning;

        public readonly IReadOnlyDictionary<BIT_TYPE, int> BitConnectionsAtRunBeginning;
        public readonly IReadOnlyDictionary<string, int> EnemiesKilledAtRunBeginning;
        public readonly IReadOnlyDictionary<ComboRecordData, int> CombosMadeAtBeginning;

        #endregion //Starting Values

        //====================================================================================================================//
                
        public bool canChoosePart;
        
        public int currentNode;

        [JsonProperty] private List<PlayerResource> _playerResources;

        //public int RationCapacity = 500;

        [JsonIgnore]
        public int Gears => _gears;

        [JsonProperty] private int _gears;
        
        [JsonIgnore]
        public int Silver => _silver;

        [JsonProperty] private int _silver;

        [JsonConverter(typeof(DecimalConverter))]
        public float currentBotHealth;
        public List<IBlockData> DroneBlockData;
        public List<IBlockData> PartsInStorageBlockData;



        [JsonIgnore]
        public IReadOnlyList<string> DontShowAgainKeys => _dontShowAgainKeys;
        [JsonProperty]
        private List<string> _dontShowAgainKeys;

        public List<int> wreckNodes;
        public List<int> playerPreviouslyCompletedNodes;

        [JsonIgnore]
        public IReadOnlyList<PatchData> PatchDatas => _patchDatas;
        [JsonProperty]

        private List<PatchData> _patchDatas;

        #endregion //Properties



        //Constructor
        //====================================================================================================================//

        public PlayerSaveRunData(
            in int startingGears,
            in float botStartHealth,
            in int xpAtRunBeginning, 
            in float repairsDoneAtRunBeginning,
            in Dictionary<BIT_TYPE, int> bitConnectionsAtRunBeginning,
            in Dictionary<ComboRecordData, int> combosMadeAtBeginning,
            in Dictionary<string, int> enemiesKilledAtRunBeginning)
        {
            PlaythroughID = Guid.NewGuid().ToString();
            canChoosePart = true;

            _gears = startingGears;
            currentBotHealth = botStartHealth;
            
            XPAtRunBeginning = xpAtRunBeginning;
            RepairsDoneAtRunBeginning = repairsDoneAtRunBeginning;
            
            //Have to create copies of the data to not let original change this ref
            //Need to include the null check for files that might be old versions
            BitConnectionsAtRunBeginning = !combosMadeAtBeginning.IsNullOrEmpty()
                ? new Dictionary<BIT_TYPE, int>(bitConnectionsAtRunBeginning)
                : new Dictionary<BIT_TYPE, int>();
            
            CombosMadeAtBeginning = !combosMadeAtBeginning.IsNullOrEmpty()
                ? new Dictionary<ComboRecordData, int>(combosMadeAtBeginning)
                : new Dictionary<ComboRecordData, int>();
            
            EnemiesKilledAtRunBeginning = !enemiesKilledAtRunBeginning.IsNullOrEmpty()
                ? new Dictionary<string, int>(enemiesKilledAtRunBeginning)
                : new Dictionary<string, int>();
            
            DroneBlockData = new List<IBlockData>();
            PartsInStorageBlockData = new List<IBlockData>();
            _patchDatas = new List<PatchData>();
            
            _dontShowAgainKeys = new List<string>();

            wreckNodes = new List<int>();
            playerPreviouslyCompletedNodes = new List<int>
            {
                0
            };

            var capacity = (int) PlayerDataManager.GetCurrentUpgradeValue(UPGRADE_TYPE.AMMO_CAPACITY);
            _playerResources = new List<PlayerResource>();
            foreach (var bitType in Constants.BIT_ORDER)
            {
                _playerResources.Add(new PlayerResource(bitType, Globals.StartingAmmo, capacity));
            }
        }

        //Bot data
        //====================================================================================================================//

        #region Block Data

        public List<IBlockData> GetCurrentBlockData()
        {
            return DroneBlockData;
        }

        public void SetDroneBlockData(IEnumerable<IBlockData> blockData)
        {
            DroneBlockData.Clear();
            DroneBlockData.AddRange(blockData);
        }

        #endregion //Block Data
        
        //Player Resources
        //============================================================================================================//

        #region Player Resources

        public List<PlayerResource> GetResources()
        {
            return _playerResources;
        }

        public PlayerResource GetResource(in BIT_TYPE bitType)
        {
            try
            {
                var type = (int) bitType - 1;
                return _playerResources[type];
            }
            catch (ArgumentOutOfRangeException e)
            {
                var type = (int) bitType - 1;
                Debug.LogError($"Failed trying to find PlayerResource[{type}] ({bitType} : {(int)bitType}) ");
                throw;
            }
            
        }

        #endregion //Player Resources
        
        //Part Storage
        //====================================================================================================================//

        #region Part Storage

        public List<IBlockData> GetCurrentPartsInStorage()
        {
            return PartsInStorageBlockData;
        }

        public void SetCurrentPartsInStorage(IEnumerable<IBlockData> blockData)
        {
            PartsInStorageBlockData = new List<IBlockData>(blockData);
        }

        public void AddPartToStorage(IBlockData blockData)
        {
            PartsInStorageBlockData.Add(blockData);
            PlayerDataManager.OnValuesChanged?.Invoke();
        }

        public void RemovePartFromStorage(IBlockData blockData)
        {
            PartsInStorageBlockData.Remove(PartsInStorageBlockData
                .FirstOrDefault(b => b.Type == blockData.Type));
        }

        public void RemovePartFromStorageAtIndex(int index)
        {
            if (PartsInStorageBlockData.Count > index)
            {
                PartsInStorageBlockData.RemoveAt(index);
            }
        }

        #endregion //Part Storage

        //Patches
        //====================================================================================================================//

        #region Patches

        public void SetPatches(in IEnumerable<PatchData> patches)
        {
            _patchDatas = new List<PatchData>(patches);
        }

        public void ClearAllPatches()
        {
            _patchDatas.Clear();
        }

        public void RemovePatchAtIndex(in int index)
        {
            _patchDatas.RemoveAt(index);
        }

        #endregion //Patches
        
        //Gears
        //============================================================================================================//

        #region Gears

        public void SetGears(int value)
        {
            _gears = value;
        }

        public void AddGears(int amount)
        {
            _gears += Mathf.Abs(amount);
        }

        public void SubtractGears(int amount)
        {
            _gears -= Mathf.Abs(amount);
        }

        #endregion //Gears

        //Silver
        //====================================================================================================================//

        #region Silver

        public void SetSilver(int value)
        {
            _silver = value;
        }

        public void AddSilver(int amount)
        {
            _silver += Mathf.Abs(amount);
        }

        public void SubtractSilver(int amount)
        {
            _silver -= Mathf.Abs(amount);
        }

        #endregion //Silver

        //Misc Functions
        //====================================================================================================================//

        public void AddDontShowAgainKey(string key)
        {
            _dontShowAgainKeys.Add(key);
        }
        
        public bool CheckIfCompleted(in int waveIndex)
        {
            throw new NotImplementedException();
            /*Debug.LogError("Checks not yet setup");
            return false;*/
        }

        public void SaveData()
        {
            throw new NotImplementedException();
            //LevelRingConnectionsJson = LevelRingNodeTree.ConvertNodeTreeIntoConnections();
        }

        //Summary String
        //====================================================================================================================//

        #region Summary String

        public string GetSummaryString()
        {
            string GetAsTitle(in string value)
            {
                return $"<b><color=white>{value}</color></b>";
            }

            var summaryText = string.Empty;
            summaryText += $"{GetAsTitle("Total XP:")} {PlayerDataManager.GetXPThisRun()}\n";
            summaryText += $"{GetAsTitle("Total Repairs:")} {PlayerDataManager.GetRepairsDoneThisRun()}\n";


            var bitConnections = PlayerDataManager.GetBitConnectionsThisRun();
            if (bitConnections.Count > 0)
            {
                summaryText += $"{GetAsTitle("Bits Connected:")}\n";

                foreach (var keyValuePair in bitConnections)
                {
                    summaryText += $"\t{keyValuePair.Key}: {keyValuePair.Value}\n";
                }
            }

            var enemiesKilled = PlayerDataManager.GetEnemiesKilledThisRun();
            if (enemiesKilled.Count > 0)
            {
                summaryText += $"{GetAsTitle("Enemies Killed:")}\n";

                foreach (var keyValuePair in enemiesKilled)
                {
                    summaryText += $"\t{keyValuePair.Key}: {keyValuePair.Value}\n";
                }
            }

            return summaryText;
        }

        #endregion //Summary String

        //====================================================================================================================//
        
    }
}

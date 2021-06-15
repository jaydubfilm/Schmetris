using Newtonsoft.Json;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JSON.Converters;
using StarSalvager.Utilities.Puzzle.Structs;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.Serialization;

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
        public bool hasShownSummary;

        //Starting values
        //====================================================================================================================//

        #region Starting Values

        public readonly int StarsAtRunBeginning;
        public readonly int XPAtRunBeginning;
        [JsonConverter(typeof(DecimalConverter))]
        public readonly float RepairsDoneAtRunBeginning;

        public readonly IReadOnlyDictionary<BIT_TYPE, int> BitConnectionsAtRunBeginning;
        public readonly IReadOnlyDictionary<string, int> EnemiesKilledAtRunBeginning;
        [JsonProperty, JsonConverter(typeof(ComboRecordDataConverter))]
        public Dictionary<ComboRecordData, int> CombosMadeAtBeginning;

        #endregion //Starting Values

        //====================================================================================================================//

        public bool canChoosePart;

        public int currentRing;
        public int currentWave;
        
        /*public int currentSector;
        public int currentWave;*/

        [JsonConverter(typeof(Vector2IntConverter))]
        public Vector2Int currentMapCoordinate;
        [JsonConverter(typeof(Vector2IntConverter))]
        public Vector2Int targetMapCoordinate;

        [JsonConverter(typeof(IEnumberableVector2IntConverter))]
        public List<Vector2Int> traversedMapCoordinates;

        [JsonProperty] private List<PlayerResource> _playerResources;

        //public int RationCapacity = 500;

        [JsonIgnore]
        public int GearsEarned => _gearsEarned;
        [JsonProperty]
        private int _gearsEarned;
        [JsonIgnore]
        public int SilverEarned => _silverEarned;
        [JsonProperty]
        private int _silverEarned;

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
        public IReadOnlyList<PartData> CurrentPatchOptions => _currentPatchOptions;
        [JsonProperty]
        private List<PartData> _currentPatchOptions;
        [JsonProperty]
        private List<PartData> _wreckPatchOptions;

        #endregion //Properties



        //Constructor
        //====================================================================================================================//

        public PlayerSaveRunData(
            in int startingGears,
            in float botStartHealth,
            in int starsAtRunBeginning,
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

            StarsAtRunBeginning = starsAtRunBeginning;
            XPAtRunBeginning = xpAtRunBeginning;
            RepairsDoneAtRunBeginning = repairsDoneAtRunBeginning;

            //Create traverse list with default position being the starting wreck/base
            traversedMapCoordinates = new List<Vector2Int>
            {
                Vector2Int.zero
            };

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

            PartsInStorageBlockData = new List<IBlockData>();
            _currentPatchOptions = new List<PartData>();

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

            //Default Drone Data
            //--------------------------------------------------------------------------------------------------------//
            
            var botLayout = PlayerDataManager.GetBotLayout();
            var defaultDrone = new List<IBlockData>();
            for (var i = 0; i < botLayout.Count; i++)
            {
                PartData partData = new PartData
                {
                    Type = (int) (botLayout[i] == Vector2Int.zero ? PART_TYPE.CORE : PART_TYPE.EMPTY),
                    Coordinate = botLayout[i],
                    Patches = new List<PatchData>()
                };

                defaultDrone.Add(partData);
            }
            
            DroneBlockData = new List<IBlockData>(defaultDrone);

            //--------------------------------------------------------------------------------------------------------//
            
        }

        //Bot data
        //====================================================================================================================//

        #region Block Data

        public IReadOnlyList<IBlockData> GetCurrentBlockData() => DroneBlockData;

        public void SetDroneBlockData(in IEnumerable<IBlockData> blockData)
        {
            DroneBlockData = new List<IBlockData>(blockData);
        }
        
        public void SetDroneBlockDataAtCoordinate(in Vector2Int coordinate, in IBlockData blockData)
        {
            var temp = coordinate;
            var index = DroneBlockData.FindIndex(x => x.Coordinate == temp);

            if (index < 0) throw new ArgumentException();

            DroneBlockData[index] = blockData;
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

        public void SetCurrentPatchOptions(in IEnumerable<PartData> partPatches)
        {
            _currentPatchOptions = new List<PartData>(partPatches);
            _wreckPatchOptions = new List<PartData>(partPatches);
        }

        public void ClearAllPatches()
        {
            _currentPatchOptions.Clear();
        }

        public void RemovePatchAtIndex(in int index)
        {
            _currentPatchOptions.RemoveAt(index);
        }

        public List<PartData> GetPurchasedPatches()
        {
            return _wreckPatchOptions.IsNullOrEmpty()
                ? null
                : new List<PartData>(_wreckPatchOptions.Where(x => !_currentPatchOptions.Contains(x)));
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
            var abs = Mathf.Abs(amount);
            _gears += abs;

            _gearsEarned += abs;
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
            var abs = Mathf.Abs(amount);
            _silver += abs;
            _silverEarned += abs;
        }

        public void SubtractSilver(int amount)
        {
            _silver -= Mathf.Abs(amount);
        }

        #endregion //Silver

        //Run Progress
        //====================================================================================================================//

        #region Run Progress

        public void TryAddTraversedCoordinate(in Vector2Int coordinate)
        {
            if (traversedMapCoordinates.Contains(coordinate))
                return;
            
            traversedMapCoordinates.Add(coordinate);
        }

        public void ResetTraversedCoordinates()
        {
            traversedMapCoordinates = new List<Vector2Int>
            {
                Vector2Int.zero
            };
        }

        /*public void SetTargetCoordinate(in Vector2Int coordinate)
        {
            targetCoordinate = coordinate;
        }
        public void SetCurrentCoordinate(in Vector2Int coordinate)
        {
            currentCoordinate = coordinate;
        }*/

        #endregion //Run Progress

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
                    var enemyName = FactoryManager.Instance.EnemyRemoteData.GetEnemyRemoteData(keyValuePair.Key).Name;
                    
                    summaryText += $"\t{enemyName}: {keyValuePair.Value}\n";
                }
            }

            return summaryText;
        }

        #endregion //Summary String

        //====================================================================================================================//

    }
}

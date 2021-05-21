using Newtonsoft.Json;
using StarSalvager.Audio;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.Saving;
using System;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Parts.Data;
using StarSalvager.PersistentUpgrades.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JSON.Converters;
using StarSalvager.Utilities.Puzzle.Structs;
using UnityEngine;

namespace StarSalvager.Values
{
    public class PlayerSaveAccountData
    {
        //Static Bot Layout
        //====================================================================================================================//
        
        [JsonIgnore] 
        public static readonly Dictionary<Vector2Int, BIT_TYPE> BotLayout =
            new Dictionary<Vector2Int, BIT_TYPE>()
            {
                [new Vector2Int(0, 0)] = BIT_TYPE.GREEN,
                [new Vector2Int(1, 0)] = BIT_TYPE.RED,
                [new Vector2Int(0, 1)] = BIT_TYPE.YELLOW,
                [new Vector2Int(-1, 0)] = BIT_TYPE.BLUE,
                [new Vector2Int(0, -1)] = BIT_TYPE.GREY
            };
        
        //Properties
        //====================================================================================================================//

        #region Properties

        [JsonIgnore]
        public bool HasRun => PlayerRunData != null && !PlayerRunData.hasCompleted;

        public PlayerSaveRunData PlayerRunData;

        public Version Version = Constants.VERSION;

        //public bool HasStarted = false;
        [JsonProperty]
        public int TotalRuns { get; private set; }


        [JsonProperty]
        public int Stars { get; private set; }

        [JsonProperty]
        public int XP { get; private set; }

        [JsonConverter(typeof(DecimalConverter))]
        public float RepairsDone;

        [JsonProperty] public Dictionary<BIT_TYPE, int> BitConnections;

        [JsonProperty] public Dictionary<string, int> EnemiesKilled;

        [JsonProperty, JsonConverter(typeof(ComboRecordDataConverter))]
        public Dictionary<ComboRecordData, int> CombosMade;

        [JsonIgnore] public IReadOnlyDictionary<HINT, bool> HintDisplay => _hintDisplay;
        [JsonProperty] private Dictionary<HINT, bool> _hintDisplay;

        [JsonIgnore] public UpgradeData[] Upgrades => _upgrades;

        [JsonProperty, JsonConverter(typeof(IEnumberableUpgradeDataConverter))]
        private UpgradeData[] _upgrades;

        [JsonProperty, JsonConverter(typeof(EnumBoolDictionaryConverter<PART_TYPE>))]
        private Dictionary<PART_TYPE, bool> _partsUnlocks;
        [JsonProperty, JsonConverter(typeof(PatchDictionaryConverter))]
        private Dictionary<PatchData, bool> _patchUnlocks;

        #endregion //Properties

        //Constructor
        //====================================================================================================================//

        #region Constructor

        public PlayerSaveAccountData()
        {
            CombosMade = new Dictionary<ComboRecordData, int>();
            EnemiesKilled = new Dictionary<string, int>();
            BitConnections = new Dictionary<BIT_TYPE, int>
            {
                {BIT_TYPE.RED, 0},
                {BIT_TYPE.BLUE, 0},
                {BIT_TYPE.YELLOW, 0},
                {BIT_TYPE.GREEN, 0},
                {BIT_TYPE.GREY, 0},
                {BIT_TYPE.WHITE, 0},
            };
            _hintDisplay = new Dictionary<HINT, bool>
            {
                [HINT.GUN] = false,
                //[HINT.FUEL] = false,
                //[HINT.HOME] = false,
                //[HINT.BONUS] = false,
                [HINT.MAGNET] = false,

                [HINT.GEARS] = false,
                //[HINT.PATCH_POINT] = false,
                //[HINT.CRAFT_PART] = false,

                [HINT.PARASITE] = false,
                [HINT.DAMAGE] = false,
                [HINT.WHITE] = false,
                [HINT.SILVER] = false,
                
                [HINT.HEALTH] = false,
                [HINT.WRECK] = false,
            };
            _upgrades = new[]
            {
                new UpgradeData(UPGRADE_TYPE.GEAR_DROP, 0),
                new UpgradeData(UPGRADE_TYPE.PATCH_COST, 0),
                new UpgradeData(UPGRADE_TYPE.AMMO_CAPACITY, 0),
                new UpgradeData(UPGRADE_TYPE.STARTING_CURRENCY, 0),

                new UpgradeData(UPGRADE_TYPE.CATEGORY_EFFICIENCY, BIT_TYPE.RED, 0),
                new UpgradeData(UPGRADE_TYPE.CATEGORY_EFFICIENCY, BIT_TYPE.BLUE, 0),
                new UpgradeData(UPGRADE_TYPE.CATEGORY_EFFICIENCY, BIT_TYPE.GREY, 0),
                new UpgradeData(UPGRADE_TYPE.CATEGORY_EFFICIENCY, BIT_TYPE.GREEN, 0),
                new UpgradeData(UPGRADE_TYPE.CATEGORY_EFFICIENCY, BIT_TYPE.YELLOW, 0)
            };

            //Setup parts for New Account
            //--------------------------------------------------------------------------------------------------------//
            
            _partsUnlocks = new Dictionary<PART_TYPE, bool>();
            var partsAtStart = FactoryManager.Instance.PlayerLevelsRemoteData.PartsUnlockedAtStart;
            var implementedParts = FactoryManager.Instance.PartsRemoteData.partRemoteData
                .Where(x => x.isImplemented)
                .Select(x => x.partType);
            
            foreach (var partType in implementedParts)
            {
                if (partType == PART_TYPE.EMPTY || partType == PART_TYPE.CORE)
                {
                    _partsUnlocks.Add(partType, true);
                    continue;
                }
                
                _partsUnlocks.Add(partType, partsAtStart.Contains(partType));
            }

            //Setup Patches for new Account
            //--------------------------------------------------------------------------------------------------------//

            _patchUnlocks = new Dictionary<PatchData, bool>();
            var patchesAtStart = FactoryManager.Instance.PlayerLevelsRemoteData.PatchesUnlockedAtStart;
            var implementedPatches = FactoryManager.Instance.PatchRemoteData.GetImplementedPatchData();

            foreach (var patchData in implementedPatches)
            {
                var patchType = (PATCH_TYPE) patchData.Type;
                if (patchType == PATCH_TYPE.EMPTY)
                {
                    _patchUnlocks.Add(patchData, true);
                    continue;
                }
                
                _patchUnlocks.Add(patchData, patchesAtStart.Contains(patchData));
            }

            //--------------------------------------------------------------------------------------------------------//

        }

        #endregion //Constructor

        //Layout Coordinates
        //====================================================================================================================//

        #region Layout Coordinates

        public BIT_TYPE GetCategoryAtCoordinate(in Vector2Int coordinate)
        {
            if (!BotLayout.TryGetValue(coordinate, out var bitType))
                throw new ArgumentException($"No coordinate found at {coordinate}");

            return bitType;
        }

        public Vector2Int GetCoordinateForCategory(in BIT_TYPE bitType)
        {
            var temp = bitType;

            return BotLayout
                .FirstOrDefault(x => x.Value == temp)
                .Key;
        }

        #endregion //Layout Coordinates

        //Player XP
        //====================================================================================================================//

        #region Player XP

        public int GetXPThisRun()
        {
            if (PlayerRunData == null) return 0;
            
            return XP - PlayerRunData.XPAtRunBeginning;
        }

        public void AddXP(in int amount)
        {
            var startXP = XP;
             var changedXP = startXP + amount;

            var startLevel = GetCurrentLevel(startXP);
            var newLevel = GetCurrentLevel(changedXP);
            
            XP += amount;
            
            //FIXME Think that this can move to some sort of callback
            if (GameManager.IsState(GameState.LEVEL))
            {
                LevelManager.Instance.WaveEndSummaryData.AddXPGained(amount);
            }

            if (startLevel == newLevel)
                return;

            var startCount = startLevel + 1; 
            for (var i = startCount; i <= newLevel; i++)
            {
                var unlocks = FactoryManager.Instance.PlayerLevelsRemoteData.GetUnlocksForLevel(i);
                foreach (var unlockData in unlocks)
                {
                    switch (unlockData.Unlock)
                    {
                        case PlayerLevelRemoteData.UNLOCK_TYPE.PART:
                            UnlockPart(unlockData.PartType);
                            break;
                        case PlayerLevelRemoteData.UNLOCK_TYPE.PATCH:
                            UnlockPatch(new PatchData
                            {
                                Type = (int)unlockData.PatchType,
                                Level = unlockData.Level
                            });
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    PlayerDataManager.OnItemUnlocked?.Invoke(unlockData);
                }
            }

            //var difference = newTotalLevels - totalLevels;

            //Do something to signify gaining a level
        }

        //XP Info Considerations: https://www.youtube.com/watch?v=MCPruAKSG0g
        //Alt option: https://gamedev.stackexchange.com/a/20946
        //Based on: https://gamedev.stackexchange.com/a/13639
        public static int GetCurrentLevel(in int xp) => PlayerLevelsRemoteDataScriptableObject.GetCurrentLevel(xp);

        public static int GetExperienceReqForLevel(in int level) => PlayerLevelsRemoteDataScriptableObject.GetXPForLevel(level);

        #endregion //Player XP

        //Stars
        //====================================================================================================================//

        #region Stars

        public int GetStarsThisRun()
        {
            if (PlayerRunData == null) return 0;
            
            return Stars - PlayerRunData.StarsAtRunBeginning;
        }
        public void SetStars(in int value)
        {
            Stars = value;
            PlayerDataManager.OnValuesChanged?.Invoke();
        }

        public void AddStars(in int amount)
        {
            Stars += amount;
            PlayerDataManager.OnValuesChanged?.Invoke();
        }

        public bool TrySubtractStars(in int amount)
        {
            if (Stars <= 0)
                return false;

            Stars -= amount;

            PlayerDataManager.OnValuesChanged?.Invoke();

            return true;
        }

        #endregion //Stars

        //Part Unlocks
        //====================================================================================================================//

        public bool IsPartUnlocked(in PART_TYPE partType)
        {
            if (!_partsUnlocks.TryGetValue(partType, out var unlocked))
                throw new ArgumentOutOfRangeException($"{partType} not unlock option");

            return unlocked;
        }

        public void UnlockPart(in PART_TYPE partType)
        {
            if (!_partsUnlocks.ContainsKey(partType))
                throw new ArgumentOutOfRangeException($"{partType} not unlock option");

            _partsUnlocks[partType] = true;
        }
        
        //Patch Unlocks
        //====================================================================================================================//

        public bool IsPatchUnlocked(in PatchData patchData)
        {
            if (!_patchUnlocks.TryGetValue(patchData, out var unlocked))
                throw new ArgumentOutOfRangeException($"{patchData} not unlock option");

            return unlocked;
        }

        public void UnlockPatch(in PatchData patchData)
        {
            if (!_patchUnlocks.ContainsKey(patchData))
                throw new ArgumentOutOfRangeException($"{patchData} not unlock option");

            _patchUnlocks[patchData] = true;
        }
        
        //Upgrades
        //====================================================================================================================//

        #region Upgrades

        public float GetCurrentUpgradeValue(in UPGRADE_TYPE upgradeType, in BIT_TYPE bitType)
        {
            var upg = upgradeType;
            var bit = bitType;
            var data = _upgrades.First(x => x.Type == upg && x.BitType == bit);

            return FactoryManager.Instance.PersistentUpgrades.GetUpgradeValue(upgradeType, bitType, data.Level);
        }

        public int GetCurrentUpgradeLevel(in UPGRADE_TYPE upgradeType, in BIT_TYPE bitType)
        {
            var upg = upgradeType;
            var bit = bitType;

            return _upgrades.First(x => x.Type == upg && x.BitType == bit).Level;
        }
        /*public void IncreaseUpgradeLevel(in UPGRADE_TYPE upgradeType, in BIT_TYPE bitType)
        {
            var upg = upgradeType;
            var bit = bitType;
            
            var index = _upgrades.ToList().FindIndex(x => x.Type == upg && x.BitType == bit);

            if (index < 0)
                throw new ArgumentException($"No upgrade found fitting {upgradeType} & {bitType}");

            var upgradeData = _upgrades[index];
            upgradeData.Level++;

            _upgrades[index] = upgradeData;
        }*/


        public void SetUpgradeLevel(in UPGRADE_TYPE upgradeType, in BIT_TYPE bitType, in int newLevel)
        {
            var upg = upgradeType;
            var bit = bitType;

            var index = _upgrades.ToList().FindIndex(x => x.Type == upg && x.BitType == bit);

            if (index < 0) throw new ArgumentException($"No upgrade found fitting {upgradeType} & {bitType}");

            var remoteData = FactoryManager.Instance.PersistentUpgrades.GetRemoteData(upgradeType, bitType);

            if (newLevel >= remoteData.Levels.Count)
                throw new ArgumentOutOfRangeException(
                    $"Cannot set {upgradeType}{(bitType == BIT_TYPE.NONE ? string.Empty : $" - {bitType}")} to level[{newLevel}]. Max is {remoteData.Levels.Count - 1}");

            var upgradeData = _upgrades[index];
            upgradeData.Level = newLevel;

            _upgrades[index] = upgradeData;
        }

        #endregion //Upgrades

        //Hints
        //====================================================================================================================//

        #region Hints

        public void SetHintDisplay(HINT hint, bool state)
        {
            _hintDisplay[hint] = state;
        }

        #endregion //Hints
        
        //Recording Data
        //====================================================================================================================//

        #region Recording Data

        public void RecordCombo(in ComboRecordData comboRecordData)
        {
            //FIXME I want this to be cleaner
            if (GameManager.IsState(GameState.LEVEL))
            {
                LevelManager.Instance.WaveEndSummaryData.AddCombo();
            }
            
            if (CombosMade.ContainsKey(comboRecordData))
            {
                CombosMade[comboRecordData]++;
                return;
            }
            
            CombosMade.Add(comboRecordData, 1);
        }
        
        public void RecordBitConnection(BIT_TYPE bit)
        {
            if (BitConnections.ContainsKey(bit))
            {
                BitConnections[bit]++;
            }
            else
            {
                Debug.LogError($"Bit Connection stat tracking, can't find bit type {bit}");
            }
        }

        public void RecordEnemyKilled(string enemyId)
        {
            if (!EnemiesKilled.ContainsKey(enemyId))
            {
                EnemiesKilled.Add(enemyId, 0);
            }

            EnemiesKilled[enemyId]++;
        }

        #endregion //Recording Data

        //Player Run Data
        //====================================================================================================================//

        #region Player Run Data

        public void CompleteCurrentRun()
        {
            PlayerRunData.hasCompleted = true;
            PlayerDataManager.SavePlayerAccountData();
        }

        public void StartNewRun()
        {
            var startingGears = (int)PlayerDataManager
                .GetCurrentUpgradeValue(UPGRADE_TYPE.STARTING_CURRENCY);
            var startingHealth =PART_TYPE.CORE
                .GetRemoteData()
                .GetDataValue<float>(PartProperties.KEYS.Health);

            var newPlayerRunData = new PlayerSaveRunData(
                startingGears,
                startingHealth,
                Stars,
                XP,
                RepairsDone,
                BitConnections,
                CombosMade,
                EnemiesKilled);

            TotalRuns++;

            PlayerRunData = newPlayerRunData;
            PlayerDataManager.SavePlayerAccountData();
        }

        [Obsolete]
        public void SaveData()
        {
            //PlayerRunData.SaveData();
        }

        #endregion //Player Run Data

        //Data Validation
        //====================================================================================================================//

        #region Data Validation

        public void ValidateData()
        {
            ValidateUnlocks();
            //TODO Validate equipped & stored parts
            
            //Ensure that if anything changed, we save it
            PlayerDataManager.SavePlayerAccountData();
        }

        private void ValidateUnlocks()
        {
            var currentLevel = PlayerLevelsRemoteDataScriptableObject.GetCurrentLevel(XP);
            var unlocksUpTo = FactoryManager.Instance.PlayerLevelsRemoteData.GetUnlocksUpToLevel(currentLevel).ToList();
            
            //Parts
            //--------------------------------------------------------------------------------------------------------//

            #region Parts

            var implementedParts = FactoryManager.Instance.PartsRemoteData.partRemoteData
                .Where(x => x.isImplemented)
                .Select(x => x.partType)
                .ToList();
            //Create a list of all parts that should be unlocked by default, and up to this level
            var partsUnlocked = new List<PART_TYPE>(FactoryManager.Instance.PlayerLevelsRemoteData.PartsUnlockedAtStart);
            partsUnlocked.AddRange(unlocksUpTo
                .Where(x => x.Unlock == PlayerLevelRemoteData.UNLOCK_TYPE.PART)
                .Select(x => x.PartType));

            foreach (PART_TYPE partType in Enum.GetValues(typeof(PART_TYPE)))
            {
                //If the partType is neither implemented or added, we can move on
                if (!implementedParts.Contains(partType) && !_partsUnlocks.ContainsKey(partType))
                    continue;
                
                //If the part no longer is implemented, but is tracked, we must remove it from the list
                if (!implementedParts.Contains(partType) && _partsUnlocks.ContainsKey(partType))
                {
                    _partsUnlocks.Remove(partType);
                    continue;
                }
                
                //If the part is implemented, but not yet tracked, ensure that its added and set to the correct value
                if (implementedParts.Contains(partType) && !_partsUnlocks.ContainsKey(partType))
                {
                    _partsUnlocks.Add(partType, partsUnlocked.Contains(partType));
                    continue;
                }

                //Ensure that if the part is implemented, and is already tracked, that the value is set correctly
                if (implementedParts.Contains(partType) && _partsUnlocks.ContainsKey(partType))
                    _partsUnlocks[partType] = partsUnlocked.Contains(partType);
            }

            #endregion //Parts

            //Patches
            //--------------------------------------------------------------------------------------------------------//

            #region Patches

            var implementedPatches = FactoryManager.Instance.PatchRemoteData.GetImplementedPatchData();

            var patchesUnlocked =
                new List<PatchData>(FactoryManager.Instance.PlayerLevelsRemoteData.PatchesUnlockedAtStart);
            patchesUnlocked.AddRange(unlocksUpTo
                .Where(x => x.Unlock == PlayerLevelRemoteData.UNLOCK_TYPE.PATCH)
                .Select(x => new PatchData
                {
                    Type = (int)x.PatchType,
                    Level = x.Level - 1
                }));

            foreach (var patchData in implementedPatches)
            {
                //FIXME There is no way of knowing which items to remove from the list without destroying the list
                
                //If the part is implemented, but not yet tracked, ensure that its added and set to the correct value
                if (!_patchUnlocks.ContainsKey(patchData))
                {
                    _patchUnlocks.Add(patchData, patchesUnlocked.Contains(patchData));
                    continue;
                }

                //Ensure that if the patch is implemented, and is already tracked, that the value is set correctly
                if (_patchUnlocks.ContainsKey(patchData))
                    _patchUnlocks[patchData] = patchesUnlocked.Contains(patchData);
            }

            #endregion //Patches

            //--------------------------------------------------------------------------------------------------------//
            
        }

        #endregion //Data Validation

        //Account Summary String
        //====================================================================================================================//

        #region Account Summary String

        public string GetSummaryString()
        {
            string GetAsTitle(in string value)
            {
                return $"<b><color=white>{value}</color></b>";
            }
            
            var summaryText = string.Empty;
            summaryText += $"{GetAsTitle("Total XP:")} {XP}\n";
            summaryText += $"{GetAsTitle("Total Repairs Done:")} {RepairsDone}\n";


            var bitConnections = BitConnections;
            if (bitConnections.Count > 0)
            {
                summaryText += $"{GetAsTitle("Bits Connected:")}\n";

                foreach (var keyValuePair in bitConnections)
                {
                    summaryText += $"\t{keyValuePair.Key}: {keyValuePair.Value}\n";
                }
            }

            if (EnemiesKilled.Count > 0)
            {
                summaryText += ($"{GetAsTitle("Enemies Killed:")}\n");

                foreach (var keyValuePair in EnemiesKilled)
                {
                    var enemyName = FactoryManager.Instance.EnemyRemoteData.GetEnemyName(keyValuePair.Key);
                    summaryText += $"\t{enemyName}: {keyValuePair.Value}\n";
                }
            }

            return summaryText;
        }

        #endregion //Account Summary String

        //====================================================================================================================//
        
    }
}
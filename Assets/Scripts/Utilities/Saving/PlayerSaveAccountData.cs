using Newtonsoft.Json;
using StarSalvager.Audio;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.Saving;
using System;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories;
using StarSalvager.Parts.Data;
using StarSalvager.PersistentUpgrades.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JSON.Converters;
using StarSalvager.Utilities.Puzzle.Structs;
using UnityEngine;

namespace StarSalvager.Values
{
    public class PlayerSaveAccountData
    {
        //Properties
        //====================================================================================================================//

        #region Properties

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

        [JsonProperty]
        public Dictionary<BIT_TYPE, int> BitConnections = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 0},
            {BIT_TYPE.BLUE, 0},
            {BIT_TYPE.YELLOW, 0},
            {BIT_TYPE.GREEN, 0},
            {BIT_TYPE.GREY, 0},
        };

        [JsonProperty]
        public Dictionary<string, int> EnemiesKilled = new Dictionary<string, int>();
        
        [JsonProperty,JsonConverter(typeof(ComboRecordDataConverter))]
        public Dictionary<ComboRecordData, int> CombosMade = new Dictionary<ComboRecordData, int>();

        [JsonIgnore] public IReadOnlyDictionary<HINT, bool> HintDisplay => _hintDisplay;

        [JsonProperty] private Dictionary<HINT, bool> _hintDisplay = new Dictionary<HINT, bool>
        {
            [HINT.GUN] = false,
            //[HINT.FUEL] = false,
            //[HINT.HOME] = false,
            [HINT.BONUS] = false,
            [HINT.MAGNET] = false,

            //[HINT.GEARS] = false,
            //[HINT.PATCH_POINT] = false,
            //[HINT.CRAFT_PART] = false,

            [HINT.PARASITE] = false,
            [HINT.DAMAGE] = false,

        };

        [JsonIgnore] public UpgradeData[] Upgrades => _upgrades;

        [JsonProperty, JsonConverter(typeof(IEnumberableUpgradeDataConverter))]
        private UpgradeData[] _upgrades = new[]
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

        [JsonIgnore] 
        public static readonly Dictionary<Vector2Int, BIT_TYPE> BotLayout =
            new Dictionary<Vector2Int, BIT_TYPE>()
            {
                [new Vector2Int(0, 0)] = BIT_TYPE.GREEN,
                [new Vector2Int(1, 0)] = BIT_TYPE.RED,
                [new Vector2Int(0, 1)] = BIT_TYPE.BLUE,
                [new Vector2Int(-1, 0)] = BIT_TYPE.GREY,
                [new Vector2Int(0, -1)] = BIT_TYPE.YELLOW
            };

        #endregion //Properties

        //Layout Coordinates
        //====================================================================================================================//
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

        //Player XP
        //====================================================================================================================//
        public int GetXPThisRun()
        {
            if (PlayerRunData == null) return 0;
            
           return XP - PlayerRunData.XPAtRunBeginning;
        }
        public void SetXP(in int value)
        {
            XP = value;
        }
        public void AddXP(in int amount)
        {
            //var startXP = XP;
           // var changedXP = startXP + amount;

            //var startLevel = GetCurrentLevel(startXP);
            //var newLevel = GetCurrentLevel(changedXP);
            
            XP += amount;
            
            //FIXME Think that this can move to some sort of callback
            if (GameManager.IsState(GameState.LEVEL))
            {
                LevelManager.Instance.WaveEndSummaryData.AddXPGained(amount);
            }

            //if (startLevel == newLevel)
            //    return;

            //var difference = newTotalLevels - totalLevels;

            //Do something to signify gaining a level
        }

        //XP Info Considerations: https://www.youtube.com/watch?v=MCPruAKSG0g
        //Alt option: https://gamedev.stackexchange.com/a/20946
        //Based on: https://gamedev.stackexchange.com/a/13639
        public static int GetCurrentLevel(in int xp)
        {
            //level = constant * sqrt(XP)
            //level = (sqrt(100(2experience+25))+50)/100
            var constant = Globals.LevelXPConstant;

            return Mathf.RoundToInt(constant * Mathf.Sqrt(xp));
        }

        public static int GetExperienceReqForLevel(in int level)
        {
            //XP = (level / constant)^2
            //experience =(level^2+level)/2*100-(level*100)
            
            var constant = Globals.LevelXPConstant;

            return Mathf.RoundToInt(Mathf.Pow(level / constant, 2));
        }

        //Stars
        //====================================================================================================================//

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

        //Upgrades
        //====================================================================================================================//
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

        //Hints
        //====================================================================================================================//

        public void SetHintDisplay(HINT hint, bool state)
        {
            _hintDisplay[hint] = state;
        }
        //Recording Data
        //====================================================================================================================//

        public void RecordCombo(in ComboRecordData comboRecordData)
        {
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

        //Player Run Data
        //====================================================================================================================//

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
                XP,
                RepairsDone,
                BitConnections,
                CombosMade,
                EnemiesKilled);

            TotalRuns++;

            PlayerRunData = newPlayerRunData;
            PlayerDataManager.SavePlayerAccountData();
        }

        public void SaveData()
        {
            //PlayerRunData.SaveData();
        }

        //====================================================================================================================//

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
                    summaryText += $"\t{keyValuePair.Key}: {keyValuePair.Value}\n";
                }
            }

            return summaryText;
        }

        //====================================================================================================================//
        
    }
}
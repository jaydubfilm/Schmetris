using Newtonsoft.Json;
using StarSalvager.Audio;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.Saving;
using System;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories;
using StarSalvager.PersistentUpgrades.Data;
using StarSalvager.Utilities.JSON.Converters;
using UnityEngine;

namespace StarSalvager.Values
{
    public class PlayerSaveAccountData
    {
        //Properties
        //====================================================================================================================//

        #region Properties

        public PlayerSaveRunData PlayerRunData = new PlayerSaveRunData();

        //public PlayerNewAlertData PlayerNewAlertData = new PlayerNewAlertData();

        public Version Version = Constants.VERSION;

        public bool HasStarted = false;

        public int Stars { get; private set; }
        public int XP;

        public int CoreDeaths;
        public float RepairsDone;

        public Dictionary<BIT_TYPE, int> BitConnections = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 0},
            {BIT_TYPE.BLUE, 0},
            {BIT_TYPE.YELLOW, 0},
            {BIT_TYPE.GREEN, 0},
            {BIT_TYPE.GREY, 0},
        };

        public Dictionary<string, int> EnemiesKilled = new Dictionary<string, int>();

        public int XPAtRunBeginning;
        public int CoreDeathsAtRunBeginning;
        public float RepairsDoneAtRunBeginning;
        public int TotalRuns;

        public Dictionary<BIT_TYPE, int> BitConnectionsAtRunBeginning = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 0},
            {BIT_TYPE.BLUE, 0},
            {BIT_TYPE.YELLOW, 0},
            {BIT_TYPE.GREEN, 0},
            {BIT_TYPE.GREY, 0},
        };

        public Dictionary<string, int> EnemiesKilledAtRunBeginning = new Dictionary<string, int>();

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

        [JsonIgnore] public static readonly Dictionary<Vector2Int, BIT_TYPE> BotLayout =
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

        public void ChangeXP(int amount)
        {
            int totalLevels = GetTotalLevels();
            XP += amount;

            if (GameManager.IsState(GameState.LEVEL))
            {
                LevelManager.Instance.WaveEndSummaryData.AddXPGained(amount);
            }

            int newTotalLevels = GetTotalLevels();

            if (newTotalLevels <= totalLevels)
                return;

            var difference = newTotalLevels - totalLevels;

            //Do something to signify gaining a level
        }

        /*public (int, int) GetLevelProgress()
        {
            int levelBaseExperience = Globals.LevelBaseExperience;
            int levelExperienceIncrement = Globals.LevelExperienceIncrement;

            int totalLevels = 0;
            int experienceAmount = XP;

            while (levelBaseExperience + (levelExperienceIncrement * totalLevels) <= experienceAmount)
            {
                experienceAmount -= levelBaseExperience + (levelExperienceIncrement * totalLevels);
                totalLevels++;
            }

            return (experienceAmount, levelBaseExperience + (levelExperienceIncrement * totalLevels));
        }*/

        public int GetTotalLevels()
        {
            int levelBaseExperience = Globals.LevelBaseExperience;
            int levelExperienceIncrement = Globals.LevelExperienceIncrement;

            int totalLevels = 0;
            int experienceAmount = XP;

            while (levelBaseExperience + (levelExperienceIncrement * totalLevels) <= experienceAmount)
            {
                experienceAmount -= levelBaseExperience + (levelExperienceIncrement * totalLevels);
                totalLevels++;
            }

            return totalLevels;
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
        public void ResetPlayerRunData()
        {
            PlayerSaveRunData data = new PlayerSaveRunData()
            {
                PlaythroughID = Guid.NewGuid().ToString(),
                runStarted = false,
            };

            //data.SetupMap(LevelRingConnectionsJson, WreckNodes);

            XPAtRunBeginning = XP;
            CoreDeathsAtRunBeginning = CoreDeaths;
            BitConnectionsAtRunBeginning.Clear();
            foreach (var keyValue in BitConnections)
            {
                BitConnectionsAtRunBeginning.Add(keyValue.Key, keyValue.Value);
            }

            EnemiesKilledAtRunBeginning.Clear();
            foreach (var keyValue in EnemiesKilled)
            {
                EnemiesKilledAtRunBeginning.Add(keyValue.Key, keyValue.Value);
            }

            TotalRuns++;

            PlayerRunData = data;
            PlayerDataManager.SetCanChoosePart(true);
            PlayerDataManager.SavePlayerAccountData();
        }

        public void SaveData()
        {
            PlayerRunData.SaveData();
        }

        //====================================================================================================================//

    }
}
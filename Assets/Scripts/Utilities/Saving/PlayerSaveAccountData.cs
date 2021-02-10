﻿using Newtonsoft.Json;
using StarSalvager.Audio;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.Saving;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Values
{
    public class PlayerSaveAccountData
    {
        public PlayerSaveRunData PlayerRunData = new PlayerSaveRunData();

        public PlayerNewAlertData PlayerNewAlertData = new PlayerNewAlertData();

        public Version Version = Constants.VERSION;

        public int Experience;

        public int CoreDeaths;
        public float RepairsDone;
        public Dictionary<BIT_TYPE, int> BitConnections = new Dictionary<BIT_TYPE, int>
        {
            { BIT_TYPE.RED, 0},
            { BIT_TYPE.BLUE, 0},
            { BIT_TYPE.YELLOW, 0},
            { BIT_TYPE.GREEN, 0},
            { BIT_TYPE.GREY, 0},
        };
        public Dictionary<string, int> EnemiesKilled = new Dictionary<string, int>();

        public int ExperienceAtRunBeginning;
        public int CoreDeathsAtRunBeginning;
        public float RepairsDoneAtRunBeginning;
        public int TotalRuns;
        public Dictionary<BIT_TYPE, int> BitConnectionsAtRunBeginning = new Dictionary<BIT_TYPE, int>
        {
            { BIT_TYPE.RED, 0},
            { BIT_TYPE.BLUE, 0},
            { BIT_TYPE.YELLOW, 0},
            { BIT_TYPE.GREEN, 0},
            { BIT_TYPE.GREY, 0},
        };
        public Dictionary<string, int> EnemiesKilledAtRunBeginning = new Dictionary<string, int>();

        [JsonIgnore]
        public IReadOnlyDictionary<HINT, bool> HintDisplay => _hintDisplay;
        [JsonProperty]
        private Dictionary<HINT, bool> _hintDisplay = new Dictionary<HINT, bool>
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

        //This system likely should be reworked. These vector2ints define the connections between nodes on the universe map, where the 1st value is the node that can be reached from the second node.
        //Example: (2,0) represents you being able to go from node 0 to node 2
        private List<Vector2Int> LevelRingConnectionsJson = new List<Vector2Int>
        {
            new Vector2Int(1, 0),
            new Vector2Int(2, 1),
            new Vector2Int(3, 2),
            new Vector2Int(6, 3),
            new Vector2Int(7, 6),
            new Vector2Int(8, 7),
            new Vector2Int(11, 8),
            new Vector2Int(12, 11),
            new Vector2Int(13, 12),
            new Vector2Int(16, 13),
            new Vector2Int(17, 16),
            new Vector2Int(18, 17),
            new Vector2Int(21, 18),
            new Vector2Int(26, 21),

            new Vector2Int(4, 0),
            new Vector2Int(9, 4),
            new Vector2Int(14, 9),
            new Vector2Int(19, 14),
            new Vector2Int(24, 19),
            new Vector2Int(23, 24),
            new Vector2Int(22, 23),

            new Vector2Int(5, 0),
            new Vector2Int(10, 5),
            new Vector2Int(15, 10),
            new Vector2Int(20, 15),
            new Vector2Int(25, 20),
        };

        //These are the nodes that have wrecks. Should also be reworked whenever the proper universe map setup is done
        [JsonIgnore]
        public List<int> WreckNodes = new List<int>()
        {
            1,
            6,
            11,
            16,
            21,
        };


        public List<Vector2Int> _botLayout = new List<Vector2Int>()
        {

        };

        //====================================================================================================================//

        public void ResetPlayerRunData()
        {
            PlayerSaveRunData data = new PlayerSaveRunData()
            {
                PlaythroughID = Guid.NewGuid().ToString(),
                runStarted = false,
            };

            data.SetupMap(LevelRingConnectionsJson, WreckNodes);

            ExperienceAtRunBeginning = Experience;
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

        public void ChangeExperience(int amount)
        {
            int totalLevels = GetTotalLevels();
            Experience += amount;

            if (GameManager.IsState(GameState.LEVEL))
            {
                LevelManager.Instance.WaveEndSummaryData.AddGearsGained(amount);
            }

            int newTotalLevels = GetTotalLevels();

            if (newTotalLevels <= totalLevels) 
                return;
            
            var difference = newTotalLevels - totalLevels;

            //Do something to signify gaining a level
        }

        public (int, int) GetLevelProgress()
        {
            int levelBaseExperience = Globals.LevelBaseExperience;
            int levelExperienceIncrement = Globals.LevelExperienceIncrement;

            int totalLevels = 0;
            int experienceAmount = Experience;

            while (levelBaseExperience + (levelExperienceIncrement * totalLevels) <= experienceAmount)
            {
                experienceAmount -= levelBaseExperience + (levelExperienceIncrement * totalLevels);
                totalLevels++;
            }

            return (experienceAmount, levelBaseExperience + (levelExperienceIncrement * totalLevels));
        }

        public int GetTotalLevels()
        {
            int levelBaseExperience = Globals.LevelBaseExperience;
            int levelExperienceIncrement = Globals.LevelExperienceIncrement;

            int totalLevels = 0;
            int experienceAmount = Experience;

            while (levelBaseExperience + (levelExperienceIncrement * totalLevels) <= experienceAmount)
            {
                experienceAmount -= levelBaseExperience + (levelExperienceIncrement * totalLevels);
                totalLevels++;
            }

            return totalLevels;
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

        //====================================================================================================================//

        public void SetHintDisplay(HINT hint, bool state)
        {
            _hintDisplay[hint] = state;
        }

        //====================================================================================================================//

        public void SaveData()
        {
            PlayerRunData.SaveData();
        }
    }
}
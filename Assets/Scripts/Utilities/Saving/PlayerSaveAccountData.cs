using Newtonsoft.Json;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Utilities.Saving;
using System;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Audio;
using StarSalvager.UI.Hints;
using UnityEngine;

namespace StarSalvager.Values
{
    public class PlayerSaveAccountData
    {
        public PlayerSaveRunData PlayerRunData = new PlayerSaveRunData();

        public PlayerNewAlertData PlayerNewAlertData = new PlayerNewAlertData();

        public Version Version = Constants.VERSION;

        //TEMP
        public Dictionary<int, int> numTimesBeatNewWaveInSector = new Dictionary<int, int>()
        {
            { 0, 0 },
            { 1, 0 },
            { 2, 0 },
            { 3, 0 },
            { 4, 0 }
        };

        public int Gears;
        public int PatchPointsSpent;

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

        public int GearsAtRunBeginning;
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

        public List<Blueprint> unlockedBlueprints = new List<Blueprint>();

        [JsonIgnore]
        public IReadOnlyDictionary<HINT, bool> HintDisplay => _hintDisplay;
        [JsonProperty]
        private Dictionary<HINT, bool> _hintDisplay = new Dictionary<HINT, bool>
        {
            [HINT.GUN] = false,
            [HINT.FUEL] = false,
            [HINT.HOME] = false,
            [HINT.BONUS] = false,
            [HINT.MAGNET] = false,
            
            [HINT.GEARS] = false,
            [HINT.PATCH_POINT] = false,
            [HINT.CRAFT_PART] = false,
            
            [HINT.PARASITE] = false,
            [HINT.DAMAGE] = false,
            [HINT.COMPONENT] = false,
            
        };

        private List<List<Vector2Int>> LevelRingConnectionsJson = new List<List<Vector2Int>>
        {
            new List<Vector2Int>
            {
                new Vector2Int(2, 0),
                new Vector2Int(1, 2),
                new Vector2Int(4, 0),
                new Vector2Int(3, 4),
                new Vector2Int(5, 4),
                new Vector2Int(6, 2),
                new Vector2Int(7, 2),
                new Vector2Int(8, 3),
                new Vector2Int(10, 4),
                new Vector2Int(9, 10),
                new Vector2Int(11, 6),
                new Vector2Int(12, 11),
                new Vector2Int(13, 8),
                new Vector2Int(15, 9),
                new Vector2Int(14, 15),
                new Vector2Int(14, 15),
                new Vector2Int(16, 11),
                new Vector2Int(17, 13),
                new Vector2Int(18, 17),
                new Vector2Int(19, 14),
                new Vector2Int(20, 19),
                new Vector2Int(21, 16),
                new Vector2Int(22, 16),
                new Vector2Int(23, 17),
                new Vector2Int(24, 18),
                new Vector2Int(25, 20),
                new Vector2Int(26, 22),
                new Vector2Int(26, 24),
            },
            new List<Vector2Int>
            {
                new Vector2Int(2, 0),
                new Vector2Int(1, 2),
                new Vector2Int(3, 2),
                new Vector2Int(7, 2),
                new Vector2Int(5, 0),
                new Vector2Int(4, 5),
                new Vector2Int(6, 1),
                new Vector2Int(8, 3),
                new Vector2Int(10, 4),
                new Vector2Int(11, 6),
                new Vector2Int(13, 8),
                new Vector2Int(12, 13),
                new Vector2Int(15, 10),
                new Vector2Int(14, 15),
                new Vector2Int(16, 12),
                new Vector2Int(17, 12),
                new Vector2Int(18, 14),
                new Vector2Int(19, 15),
                new Vector2Int(20, 19),
                new Vector2Int(21, 16),
                new Vector2Int(26, 21),
                new Vector2Int(23, 17),
                new Vector2Int(22, 23),
                new Vector2Int(24, 19),
                new Vector2Int(26, 24),
                new Vector2Int(25, 20),
            },
            new List<Vector2Int>
            {
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(6, 1),
                new Vector2Int(3, 2),
                new Vector2Int(7, 2),
                new Vector2Int(4, 3),
                new Vector2Int(8, 3),
                new Vector2Int(5, 4),
                new Vector2Int(10, 4),
                new Vector2Int(12, 6),
                new Vector2Int(11, 12),
                new Vector2Int(13, 8),
                new Vector2Int(14, 8),
                new Vector2Int(15, 10),
                new Vector2Int(16, 11),
                new Vector2Int(17, 11),
                new Vector2Int(18, 14),
                new Vector2Int(19, 14),
                new Vector2Int(22, 16),
                new Vector2Int(21, 22),
                new Vector2Int(23, 17),
                new Vector2Int(20, 19),
                new Vector2Int(24, 19),
                new Vector2Int(25, 19),
                new Vector2Int(26, 22),
                new Vector2Int(26, 25),
            },
            new List<Vector2Int>
            {
                new Vector2Int(3, 0),
                new Vector2Int(5, 0),
                new Vector2Int(2, 3),
                new Vector2Int(1, 2),
                new Vector2Int(7, 1),
                new Vector2Int(6, 7),
                new Vector2Int(8, 3),
                new Vector2Int(4, 5),
                new Vector2Int(9, 5),
                new Vector2Int(10, 5),
                new Vector2Int(11, 7),
                new Vector2Int(12, 7),
                new Vector2Int(13, 8),
                new Vector2Int(14, 9),
                new Vector2Int(15, 10),
                new Vector2Int(16, 11),
                new Vector2Int(17, 11),
                new Vector2Int(18, 14),
                new Vector2Int(19, 15),
                new Vector2Int(22, 17),
                new Vector2Int(21, 22),
                new Vector2Int(26, 21),
                new Vector2Int(23, 17),
                new Vector2Int(24, 18),
                new Vector2Int(20, 19),
                new Vector2Int(25, 19),
                new Vector2Int(26, 25),
            },
            new List<Vector2Int>
            {
                new Vector2Int(2, 0),
                new Vector2Int(3, 0),
                new Vector2Int(1, 2),
                new Vector2Int(6, 1),
                new Vector2Int(7, 3),
                new Vector2Int(8, 3),
                new Vector2Int(4, 3),
                new Vector2Int(5, 4),
                new Vector2Int(9, 4),
                new Vector2Int(10, 5),
                new Vector2Int(12, 6),
                new Vector2Int(14, 8),
                new Vector2Int(15, 10),
                new Vector2Int(11, 12),
                new Vector2Int(13, 12),
                new Vector2Int(16, 11),
                new Vector2Int(17, 11),
                new Vector2Int(19, 15),
                new Vector2Int(20, 15),
                new Vector2Int(18, 17),
                new Vector2Int(21, 17),
                new Vector2Int(22, 17),
                new Vector2Int(25, 20),
                new Vector2Int(23, 22),
                new Vector2Int(24, 23),
                new Vector2Int(26, 24),
                new Vector2Int(26, 25),
            }
        };

        [JsonIgnore]
        public List<List<int>> ShortcutNodes = new List<List<int>>()
        {
            new List<int>
            { 
                4,
                6,
                8,
                15,
                16,
                17,
                19,
                24,
            },
            new List<int>
            {
                3,
                4,
                6,
                13,
                15,
                16,
                17,
                24,
            },
            new List<int>
            {
                3,
                6,
                10,
                13,
                14,
                16,
                17,
                25,
            },
            new List<int>
            {
                1,
                3,
                9,
                10,
                11,
                18,
                19,
                22,
            },
            new List<int>
            {
                1,
                4,
                8,
                10,
                12,
                17,
                23,
                25,
            },
        };

        //====================================================================================================================//

        public void ResetPlayerRunData()
        {
            int randomIndex = UnityEngine.Random.Range(0, 5);

            PlayerSaveRunData data = new PlayerSaveRunData()
            {
                PlaythroughID = Guid.NewGuid().ToString(),
                runStarted = false
            };
            data.SetupMap(LevelRingConnectionsJson[randomIndex], ShortcutNodes[randomIndex]);

            GearsAtRunBeginning = Gears;
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
            PlayerDataManager.SavePlayerAccountData();
        }

        public void ChangeGears(int amount)
        {
            int totalPatchPoints = GetTotalPatchPoints();
            Gears += amount;

            if (GameManager.IsState(GameState.LEVEL))
            {
                LevelManager.Instance.WaveEndSummaryData.AddGearsGained(amount);
            }

            int newTotalPatchPoints = GetTotalPatchPoints();

            if (newTotalPatchPoints <= totalPatchPoints) 
                return;
            
            var difference = newTotalPatchPoints - totalPatchPoints;
            Toast.AddToast($"Unlocked {(difference > 1 ? $"{difference} Patch Points!" : "New Patch Point!")}");
                
            AudioController.PlaySound(SOUND.UNLOCK_PATCH_POINT);
            
            LevelManager.Instance?.GameUi?.CreatePatchPointEffect(difference);
        }

        public void AddGearsToGetPatchPoints(int numPatchPointsToGet)
        {
            for (int i = 0; i < numPatchPointsToGet; i++)
            {
                (int, int) progress = GetPatchPointProgress();
                ChangeGears(progress.Item2 - progress.Item1);
            }
        }

        public (int, int) GetPatchPointProgress()
        {
            int patchPointBaseCost = Globals.PatchPointBaseCost;
            int patchPointCostIncrement = Globals.PatchPointIncrementCost;

            int totalPatchPoints = 0;
            int gearsAmount = Gears;

            while (patchPointBaseCost + (patchPointCostIncrement * totalPatchPoints) <= gearsAmount)
            {
                gearsAmount -= patchPointBaseCost + (patchPointCostIncrement * totalPatchPoints);
                totalPatchPoints++;
            }

            return (gearsAmount, patchPointBaseCost + (patchPointCostIncrement * totalPatchPoints));
        }

        public int GetTotalPatchPoints()
        {
            int patchPointBaseCost = Globals.PatchPointBaseCost;
            int patchPointCostIncrement = Globals.PatchPointIncrementCost;

            int totalPatchPoints = 0;
            int gearsAmount = Gears;

            while (patchPointBaseCost + (patchPointCostIncrement * totalPatchPoints) <= gearsAmount)
            {
                gearsAmount -= patchPointBaseCost + (patchPointCostIncrement * totalPatchPoints);
                totalPatchPoints++;
            }

            return totalPatchPoints;
        }

        public int GetAvailablePatchPoints()
        {
            return GetTotalPatchPoints() - PatchPointsSpent;
        }

        public void SpendPatchPoints(int amount)
        {
            PatchPointsSpent += amount;
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

        public void UnlockBlueprint(Blueprint blueprint)
        {
            if (unlockedBlueprints.All(b => b.name != blueprint.name))
            {
                unlockedBlueprints.Add(blueprint);

                //FIXME This may benefit from the use of a callback instead of a direct call
                if (LevelManager.Instance != null && LevelManager.Instance.WaveEndSummaryData != null)
                {
                    LevelManager.Instance.WaveEndSummaryData.AddUnlockedBlueprint(blueprint.DisplayString);
                }

                PlayerDataManager.AddNewBlueprintAlert(blueprint);
            }
        }

        public void UnlockBlueprint(PART_TYPE partType)
        {
            Blueprint blueprint = new Blueprint
            {
                name = $"{partType}",
                partType = partType
            };
            UnlockBlueprint(blueprint);
        }

        public void UnlockAllBlueprints()
        {
            foreach (var partRemoteData in FactoryManager.Instance.PartsRemoteData.partRemoteData)
            {
                //TODO Add these back in when we're ready!
                switch (partRemoteData.partType)
                {
                    //Still want to be able to upgrade the core, just don't want to buy new ones?
                    case PART_TYPE.CORE:
                    case PART_TYPE.SPIKES:
                    case PART_TYPE.LASER:
                    case PART_TYPE.GRENADE:
                    case PART_TYPE.CATAPULT:
                    case PART_TYPE.LIGHTNING:
                    case PART_TYPE.BOOSTRANGE:
                    case PART_TYPE.BOOSTRATE:
                    case PART_TYPE.BOOSTDAMAGE:
                    case PART_TYPE.BOOSTDEFENSE:
                    case PART_TYPE.STACKER:
                    case PART_TYPE.CLOAK:
                    case PART_TYPE.SONAR:
                    case PART_TYPE.DECOY:
                    case PART_TYPE.RETRACTOR:
                    case PART_TYPE.HOOVER:
                    case PART_TYPE.FREEZE:
                        continue;
                }

                Blueprint blueprint = new Blueprint
                {
                    name = $"{partRemoteData.partType}",
                    partType = partRemoteData.partType
                };
                UnlockBlueprint(blueprint);
            }
        }

        //====================================================================================================================//

        public void SaveData()
        {
            PlayerRunData.SaveData();
        }
    }
}
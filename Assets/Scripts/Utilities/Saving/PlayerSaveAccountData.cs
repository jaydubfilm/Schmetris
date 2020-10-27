using Newtonsoft.Json;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Missions;
using StarSalvager.Utilities.Saving;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarSalvager.Values
{
    public class PlayerSaveAccountData
    {
        public PlayerSaveRunData PlayerRunData = new PlayerSaveRunData();

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

        public MissionsCurrentData missionsCurrentData = null;

        [JsonIgnore]
        public IReadOnlyDictionary<FACILITY_TYPE, int> facilityRanks => _facilityRanks;
        [JsonProperty]
        private Dictionary<FACILITY_TYPE, int> _facilityRanks = new Dictionary<FACILITY_TYPE, int>();

        [JsonIgnore]
        public IReadOnlyDictionary<FACILITY_TYPE, int> facilityBlueprintRanks => _facilityBlueprintRanks;
        [JsonProperty]
        private Dictionary<FACILITY_TYPE, int> _facilityBlueprintRanks = new Dictionary<FACILITY_TYPE, int>();

        //====================================================================================================================//

        public void ResetPlayerRunData()
        {
            PlayerSaveRunData data = new PlayerSaveRunData();
            data.PlaythroughID = Guid.NewGuid().ToString();

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
            //MissionManager.LoadMissionData();
        }

        public void ChangeGears(int amount)
        {
            int totalPatchPoints = GetTotalPatchPoints();
            Gears += amount;

            int newTotalPatchPoints = GetTotalPatchPoints();

            if (newTotalPatchPoints > totalPatchPoints)
            {
                for (int i = totalPatchPoints; i < newTotalPatchPoints; i++)
                {
                    Toast.AddToast("Unlocked New Patch Point!");
                }
            }
        }

        public (int, int) GetPatchPointProgress()
        {
            int patchPointBaseCost = 100;
            int patchPointCostIncrement = 10;

            int totalPatchPoints = 0;
            int gearsAmount = Gears;

            while (patchPointBaseCost + (patchPointCostIncrement * totalPatchPoints) <= gearsAmount)
            {
                gearsAmount -= patchPointCostIncrement + (patchPointCostIncrement * totalPatchPoints);
                totalPatchPoints++;
            }

            return (gearsAmount, patchPointBaseCost + (patchPointCostIncrement * totalPatchPoints));
        }

        public int GetTotalPatchPoints()
        {
            int patchPointBaseCost = 100;
            int patchPointCostIncrement = 10;

            int totalPatchPoints = 0;
            int gearsAmount = Gears;

            while (patchPointBaseCost + (patchPointCostIncrement * totalPatchPoints) <= gearsAmount)
            {
                gearsAmount -= patchPointCostIncrement + (patchPointCostIncrement * totalPatchPoints);
                totalPatchPoints++;
            }

            return totalPatchPoints;
        }

        public int GetAvailablePatchPoints()
        {
            return GetTotalPatchPoints() - PatchPointsSpent;
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

        public bool CheckHasFacility(FACILITY_TYPE type, int level = 0)
        {
            if (_facilityRanks.TryGetValue(type, out var rank))
                return rank >= level;

            return false;
        }

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
            }
        }

        public void UnlockBlueprint(PART_TYPE partType, int level)
        {
            Blueprint blueprint = new Blueprint
            {
                name = partType + " " + level,
                partType = partType,
                level = level
            };
            UnlockBlueprint(blueprint);
        }

        public void UnlockAllBlueprints()
        {
            foreach (var partRemoteData in FactoryManager.Instance.PartsRemoteData.partRemoteData)
            {
                for (int i = 0; i < partRemoteData.levels.Count; i++)
                {
                    //TODO Add these back in when we're ready!
                    switch (partRemoteData.partType)
                    {
                        //Still want to be able to upgrade the core, just don't want to buy new ones?
                        case PART_TYPE.CORE when i == 0:
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
                        name = partRemoteData.partType + " " + i,
                        partType = partRemoteData.partType,
                        level = i
                    };
                    UnlockBlueprint(blueprint);
                }
            }
        }

        public void UnlockFacilityLevel(FACILITY_TYPE type, int level, bool triggerMissionCheck = true)
        {
            FacilityRemoteData remoteData = FactoryManager.Instance.FacilityRemote.GetRemoteData(type);
            if (_facilityRanks.ContainsKey(type) && _facilityRanks[type] < level)
            {
                _facilityRanks[type] = level;
            }
            else if (!_facilityRanks.ContainsKey(type))
            {
                _facilityRanks.Add(type, level);
            }

            if (triggerMissionCheck)
            {
                MissionProgressEventData missionProgressEventData = new MissionProgressEventData
                {
                    facilityType = type,
                    level = level
                };

                MissionManager.ProcessMissionData(typeof(FacilityUpgradeMission), missionProgressEventData);
            }

            int increaseAmount = remoteData.levels[level].increaseAmount;
            switch (type)
            {
                case FACILITY_TYPE.FREEZER:
                    PlayerDataManager.IncreaseRationCapacity(increaseAmount);
                    break;
                case FACILITY_TYPE.STORAGEELECTRICITY:
                    PlayerDataManager.GetResource(BIT_TYPE.YELLOW).AddResourceCapacity(increaseAmount);
                    break;
                case FACILITY_TYPE.STORAGEFUEL:
                    PlayerDataManager.GetResource(BIT_TYPE.RED).AddResourceCapacity(increaseAmount);
                    break;
                case FACILITY_TYPE.STORAGEPLASMA:
                    PlayerDataManager.GetResource(BIT_TYPE.GREEN).AddResourceCapacity(increaseAmount);
                    break;
                case FACILITY_TYPE.STORAGESCRAP:
                    PlayerDataManager.GetResource(BIT_TYPE.GREY).AddResourceCapacity(increaseAmount);
                    break;
                case FACILITY_TYPE.STORAGEWATER:
                    PlayerDataManager.GetResource(BIT_TYPE.BLUE).AddResourceCapacity(increaseAmount);
                    break;
            }

            //Debug.Log(_rationCapacity);
            PlayerDataManager.OnCapacitiesChanged?.Invoke();
            PlayerDataManager.OnValuesChanged?.Invoke();
        }

        public void UnlockFacilityBlueprintLevel(FacilityBlueprint facilityBlueprint)
        {
            UnlockFacilityBlueprintLevel(facilityBlueprint.facilityType, facilityBlueprint.level);
        }

        public void UnlockFacilityBlueprintLevel(FACILITY_TYPE facilityType, int level)
        {
            FacilityRemoteData remoteData = FactoryManager.Instance.FacilityRemote.GetRemoteData(facilityType);
            string blueprintUnlockString = $"{remoteData.displayName} lvl {level + 1}";

            if (_facilityBlueprintRanks.ContainsKey(facilityType))
            {
                if (_facilityBlueprintRanks[facilityType] < level)
                {
                    _facilityBlueprintRanks[facilityType] = level;

                    //FIXME This may benefit from the use of a callback instead of a direct call
                    if (LevelManager.Instance.WaveEndSummaryData != null)
                    {
                        LevelManager.Instance.WaveEndSummaryData.AddUnlockedBlueprint(blueprintUnlockString);
                    }
                }
            }
            else
            {
                _facilityBlueprintRanks.Add(facilityType, level);
                //FIXME This may benefit from the use of a callback instead of a direct call
                if (LevelManager.Instance.WaveEndSummaryData != null)
                {
                    LevelManager.Instance.WaveEndSummaryData.AddUnlockedBlueprint(blueprintUnlockString);
                }
            }
        }

        //====================================================================================================================//

        public void SaveData()
        {
            PlayerRunData.SaveData();
        }
    }
}
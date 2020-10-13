﻿using System;
using StarSalvager.UI.Scrapyard;
using StarSalvager.Utilities;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using StarSalvager.Missions;
using UnityEngine.SceneManagement;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Factories;
using StarSalvager.ScriptableObjects;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Math;

namespace StarSalvager.Values
{
    public class PlayerData
    {
        [JsonIgnore]
        public static Action OnValuesChanged;
        [JsonIgnore]
        public static Action OnCapacitiesChanged;

        //============================================================================================================//

        //TODO: Add an add/subtract function for ResourceAmount, and make this IReadOnlyDictionary<>
        [JsonIgnore]
        public Dictionary<BIT_TYPE, int> resources => _resources;

        [JsonProperty]
        private Dictionary<BIT_TYPE, int> _resources = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 100},
            {BIT_TYPE.BLUE, 75},
            {BIT_TYPE.YELLOW, 0},
            {BIT_TYPE.GREEN, 0},
            {BIT_TYPE.GREY, 0},
        };

        [JsonIgnore]
        public IReadOnlyDictionary<BIT_TYPE, int> ResourceCapacities => _resourceCapacity;
        [JsonProperty]
        private Dictionary<BIT_TYPE, int> _resourceCapacity = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 300},
            {BIT_TYPE.BLUE, 300},
            {BIT_TYPE.YELLOW, 300},
            {BIT_TYPE.GREEN, 300},
            {BIT_TYPE.GREY, 300},
        };

        [JsonProperty]
        private int _rationCapacity = 500;

        [JsonIgnore]
        public IReadOnlyDictionary<COMPONENT_TYPE, int> components => _components;
        [JsonProperty]
        private Dictionary<COMPONENT_TYPE, int> _components = new Dictionary<COMPONENT_TYPE, int>
        {
            {COMPONENT_TYPE.FUSOR, 0},
            {COMPONENT_TYPE.CHIP, 0},
            {COMPONENT_TYPE.NUT, 0},
            {COMPONENT_TYPE.BOLT, 0},
            {COMPONENT_TYPE.COIL, 0}
        };

        [JsonIgnore]
        public IReadOnlyDictionary<BIT_TYPE, float> liquidResource => _liquidResource;
        [JsonProperty]
        //FIXME This needs to use some sort of capacity value
        private Dictionary<BIT_TYPE, float> _liquidResource = new Dictionary<BIT_TYPE, float>
        {
            {BIT_TYPE.RED, 30},
            {BIT_TYPE.BLUE, 0},
            {BIT_TYPE.YELLOW, 0},
            {BIT_TYPE.GREEN, 0},
            {BIT_TYPE.GREY, 0},
        };

        [JsonIgnore]
        public IReadOnlyDictionary<BIT_TYPE, float> recoveryDroneLiquidResource => _recoveryDroneLiquidResource;
        [JsonProperty]
        //FIXME This needs to use some sort of capacity value
        private Dictionary<BIT_TYPE, float> _recoveryDroneLiquidResource = new Dictionary<BIT_TYPE, float>
        {
            {BIT_TYPE.RED, 30},
            {BIT_TYPE.BLUE, 0},
            {BIT_TYPE.YELLOW, 0},
            {BIT_TYPE.GREEN, 0},
            {BIT_TYPE.GREY, 0},
        };

        //FIXME I think that this should not be so persistent (Shouldn't need to be saved data)
        [JsonIgnore]
        public IReadOnlyDictionary<BIT_TYPE, int> liquidCapacity => _liquidCapacity;
        [JsonProperty]
        private Dictionary<BIT_TYPE, int> _liquidCapacity = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 0},
            {BIT_TYPE.BLUE, 0},
            {BIT_TYPE.YELLOW, 0},
            {BIT_TYPE.GREEN, 0},
            {BIT_TYPE.GREY, 0},
        };

        //FIXME I think that this should not be so persistent (Shouldn't need to be saved data)
        [JsonIgnore]
        public IReadOnlyDictionary<BIT_TYPE, int> recoveryDroneLiquidCapacity => _recoveryDroneLiquidCapacity;
        [JsonProperty]
        private Dictionary<BIT_TYPE, int> _recoveryDroneLiquidCapacity = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 0},
            {BIT_TYPE.BLUE, 0},
            {BIT_TYPE.YELLOW, 0},
            {BIT_TYPE.GREEN, 0},
            {BIT_TYPE.GREY, 0},
        };

        public List<BlockData> currentBlockData = new List<BlockData>();
        public List<BlockData> recoveryDroneBlockData = new List<BlockData>();
        public List<BlockData> partsInStorageBlockData = new List<BlockData>();

        public List<Blueprint> unlockedBlueprints = new List<Blueprint>();

        public Dictionary<int, int> maxSectorProgression = new Dictionary<int, int>();

        public List<SectorWaveModifier> levelResourceModifier = new List<SectorWaveModifier>();

        public MissionsCurrentData missionsCurrentData = null;

        public int currentModularSectorIndex = 0;

        public bool firstFlight = true;

        public int Level;
        public int Gears;

        [JsonIgnore]
        public IReadOnlyDictionary<FACILITY_TYPE, int> facilityRanks => _facilityRanks;
        [JsonProperty]
        private Dictionary<FACILITY_TYPE, int> _facilityRanks = new Dictionary<FACILITY_TYPE, int>();

        [JsonIgnore]
        public IReadOnlyDictionary<FACILITY_TYPE, int> facilityBlueprintRanks => _facilityBlueprintRanks;
        [JsonProperty]
        private Dictionary<FACILITY_TYPE, int> _facilityBlueprintRanks = new Dictionary<FACILITY_TYPE, int>();

        public string PlaythroughID = string.Empty;

        [JsonIgnore]
        public IReadOnlyList<string> DontShowAgainKeys => _dontShowAgainKeys;
        [JsonProperty] 
        private List<string> _dontShowAgainKeys = new List<string>();

        [JsonIgnore]
        public LevelRingNodeTree LevelRingNodeTree = new LevelRingNodeTree();
        [JsonProperty]
        private List<Vector2Int> LevelRingConnectionsJson = new List<Vector2Int>
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
        };

        //============================================================================================================//

        public PlayerData()
        {
            LevelRingNodeTree.ReadInNodeConnectionData(LevelRingConnectionsJson);
        }

        public void ChangeGears(int amount)
        {
            Gears += amount;
            if (LevelManager.Instance.WaveEndSummaryData != null)
            {
                LevelManager.Instance.WaveEndSummaryData.NumGearsGained += amount;
            }

            int gearsToLevelUp = LevelManager.Instance.PlayerlevelRemoteDataScriptableObject.GetRemoteData(Level).GearsToLevelUp;
            if (Gears >= gearsToLevelUp)
            {
                Gears -= gearsToLevelUp;
                DropLevelLoot();
                if (LevelManager.Instance.WaveEndSummaryData != null)
                {
                    LevelManager.Instance.WaveEndSummaryData.NumLevelsGained++;
                }
                Level++;

                MissionProgressEventData missionProgressEventData = new MissionProgressEventData
                {
                    level = Level
                };
                MissionManager.ProcessMissionData(typeof(PlayerLevelMission), missionProgressEventData);
            }
            
            OnValuesChanged?.Invoke();
        }

        private void DropLevelLoot()
        {
            LevelManager.Instance.PlayerlevelRemoteDataScriptableObject.GetRemoteData(Level).ConfigureLootTable();
            List<IRDSObject> levelUpLoot = LevelManager.Instance.PlayerlevelRemoteDataScriptableObject.GetRemoteData(Level).rdsTable.rdsResult.ToList();
            for (int i = levelUpLoot.Count - 1; i >= 0; i--)
            {
                if (levelUpLoot[i] is RDSValue<Blueprint> rdsValueBlueprint)
                {
                    UnlockBlueprint(rdsValueBlueprint.rdsValue);
                    Toast.AddToast("Unlocked Blueprint!");
                    levelUpLoot.RemoveAt(i);
                    continue;
                }
                if (levelUpLoot[i] is RDSValue<FacilityBlueprint> rdsValueFacilityBlueprint)
                {
                    UnlockFacilityBlueprintLevel(rdsValueFacilityBlueprint.rdsValue);
                    Toast.AddToast("Unlocked Facility Blueprint!");
                    levelUpLoot.RemoveAt(i);
                    continue;
                }
                else if (levelUpLoot[i] is RDSValue<Vector2Int> rdsValueGears)
                {
                    ChangeGears(UnityEngine.Random.Range(rdsValueGears.rdsValue.x, rdsValueGears.rdsValue.y));
                    levelUpLoot.RemoveAt(i);
                    continue;
                }
                else if (levelUpLoot[i] is RDSValue<Bit> rdsValueBit)
                {
                    AddResource(rdsValueBit.rdsValue.Type, FactoryManager.Instance.BitsRemoteData.GetRemoteData(rdsValueBit.rdsValue.Type).levels[0].resources);
                }
                else if (levelUpLoot[i] is RDSValue<(BIT_TYPE, int)> rdsValueResourceRefined)
                {
                    AddResource(rdsValueResourceRefined.rdsValue.Item1, rdsValueResourceRefined.rdsValue.Item2);
                }
                else if (levelUpLoot[i] is RDSValue<Component> rdsValueComponent)
                {
                    AddComponent(rdsValueComponent.rdsValue.Type, 1);
                }
            }
        }

        //============================================================================================================//

        public void SetResources(Dictionary<BIT_TYPE, int> values)
        {
            _resources = values;
        }

        public void SetResources(BIT_TYPE type, int value)
        {
            _resources[type] = Mathf.Min(value, ResourceCapacities[type]);
        }

        //============================================================================================================//

        public void SetLiquidResource(BIT_TYPE type, float value, bool isRecoveryDrone)
        {
            if (isRecoveryDrone)
            {
                _recoveryDroneLiquidResource[type] = Mathf.Clamp(value, 0f, _recoveryDroneLiquidCapacity[type]);
            }
            else
            {
                _liquidResource[type] = Mathf.Clamp(value, 0f, _liquidCapacity[type]);
            }

            OnValuesChanged?.Invoke();
        }

        public void SetLiquidResource(Dictionary<BIT_TYPE, float> liquidValues, bool isRecoveryDrone)
        {
            foreach (var value in liquidValues)
            {
                if (isRecoveryDrone)
                {
                    _recoveryDroneLiquidResource[value.Key] = Mathf.Clamp(value.Value, 0f, _recoveryDroneLiquidCapacity[value.Key]);
                }
                else
                {
                    _liquidResource[value.Key] = Mathf.Clamp(value.Value, 0f, _liquidCapacity[value.Key]);
                }
            }

            OnValuesChanged?.Invoke();
        }

        //============================================================================================================//

        public void SetComponents(COMPONENT_TYPE type, int value)
        {
            _components[type] = value;

            OnValuesChanged?.Invoke();
        }

        public void SetComponents(Dictionary<COMPONENT_TYPE, int> liquidValues)
        {
            foreach (var value in liquidValues)
            {
                _components[value.Key] = value.Value;
            }

            OnValuesChanged?.Invoke();
        }

        //============================================================================================================//

        public void SetCapacity(BIT_TYPE type, int amount, bool isRecoveryDrone)
        {
            if (isRecoveryDrone)
            {
                _recoveryDroneLiquidCapacity[type] = amount;
            }
            else
            {
                _liquidCapacity[type] = amount;
            }
            OnCapacitiesChanged?.Invoke();
        }

        public void SetCapacities(Dictionary<BIT_TYPE, int> capacities, bool isRecoveryDrone)
        {
            foreach (var capacity in capacities)
            {
                if (isRecoveryDrone)
                {
                    _recoveryDroneLiquidCapacity[capacity.Key] = capacity.Value;
                }
                else
                {
                    _liquidCapacity[capacity.Key] = capacity.Value;
                }
            }

            OnCapacitiesChanged?.Invoke();
        }

        public void ClearLiquidCapacity(bool isRecoveryDrone)
        {
            if (isRecoveryDrone)
            {
                _recoveryDroneLiquidCapacity = new Dictionary<BIT_TYPE, int>
                {
                    {BIT_TYPE.RED, 0},
                    {BIT_TYPE.BLUE, 0},
                    {BIT_TYPE.YELLOW, 0},
                    {BIT_TYPE.GREEN, 0},
                    {BIT_TYPE.GREY, 0},
                };
            }
            else
            {
                _liquidCapacity = new Dictionary<BIT_TYPE, int>
                {
                    {BIT_TYPE.RED, 0},
                    {BIT_TYPE.BLUE, 0},
                    {BIT_TYPE.YELLOW, 0},
                    {BIT_TYPE.GREEN, 0},
                    {BIT_TYPE.GREY, 0},
                };
            }
        }

        //============================================================================================================//

        public void AddResources(Dictionary<BIT_TYPE, int> toAdd, float multiplier)
        {
            CostCalculations.AddResources(ref _resources, toAdd, multiplier);

            foreach (var bitType in toAdd.Select(keyValuePair => keyValuePair.Key))
            {
                _resources[bitType] = Mathf.Min(_resources[bitType], ResourceCapacities[bitType]);
            }
            
            OnValuesChanged?.Invoke();
        }

        public void AddResource(BIT_TYPE type, int amount)
        {
            _resources[type] = Mathf.Min(_resources[type] + amount, ResourceCapacities[type]);
            OnValuesChanged?.Invoke();
        }

        public void AddResources(PART_TYPE partType, int level, bool isRecursive)
        {
            CostCalculations.AddResources(ref _resources, partType, level, isRecursive);
            OnValuesChanged?.Invoke();
        }
        public void AddResources(BlockData blockData, bool isRecursive)
        {
            if (!blockData.ClassType.Equals(nameof(Part)))
                return;
            
            AddResources((PART_TYPE) blockData.Type, blockData.Level, isRecursive);
        }

        public void SubtractResources(BIT_TYPE type, int amount)
        {
            Dictionary<BIT_TYPE, int> resources = new Dictionary<BIT_TYPE, int>();
            resources.Add(type, amount);
            SubtractResources(resources);
        }

        public void SubtractResources(Dictionary<BIT_TYPE, int> toSubtract)
        {
            CostCalculations.SubtractResources(ref _resources, toSubtract);
            OnValuesChanged?.Invoke();
        }

        public void SubtractResources(PART_TYPE partType, int level, bool isRecursive)
        {
            CostCalculations.SubtractResources(ref _resources, partType, level, isRecursive);
            OnValuesChanged?.Invoke();
        }

        public void SubtractResources(IEnumerable<CraftCost> cost)
        {
            CostCalculations.SubtractResources(ref _resources, cost);
            OnValuesChanged?.Invoke();
        }

        public void SubtractComponents(IEnumerable<CraftCost> cost)
        {
            CostCalculations.SubtractComponents(ref _components, cost);
            OnValuesChanged?.Invoke();
        }

        public void SubtractPartCosts(PART_TYPE partType, int level, bool isRecursive, float costModifier = 1.0f)
        {
            CostCalculations.SubtractPartCosts(ref _resources, ref _components, partsInStorageBlockData, partType, level, isRecursive, costModifier);
            OnValuesChanged?.Invoke();
        }

        //============================================================================================================//

        public void AddLiquidResource(BIT_TYPE type, float amount, bool isRecoveryDrone)
        {
            MissionProgressEventData missionProgressEventData = new MissionProgressEventData
            {
                bitType = type,
                floatAmount = amount
            };
            MissionManager.ProcessMissionData(typeof(LiquidResourceConvertedMission), missionProgressEventData);
            if (isRecoveryDrone)
            {
                _recoveryDroneLiquidResource[type] = Mathf.Clamp(recoveryDroneLiquidResource[type] + Mathf.Abs(amount), 0, recoveryDroneLiquidCapacity[type]);
            }
            else
            {
                _liquidResource[type] = Mathf.Clamp(liquidResource[type] + Mathf.Abs(amount), 0, liquidCapacity[type]);
            }
            OnValuesChanged?.Invoke();
        }

        public void SubtractLiquidResource(BIT_TYPE type, float amount, bool isRecoveryDrone)
        {
            if (isRecoveryDrone)
            {
                _recoveryDroneLiquidResource[type] = Mathf.Clamp(recoveryDroneLiquidResource[type] - Mathf.Abs(amount), 0, recoveryDroneLiquidCapacity[type]);
            }
            else
            {
                _liquidResource[type] = Mathf.Clamp(liquidResource[type] - Mathf.Abs(amount), 0, liquidCapacity[type]);
            }
            OnValuesChanged?.Invoke();
        }

        //============================================================================================================//

        public void AddComponent(COMPONENT_TYPE type, int amount)
        {
            _components[type] += Mathf.Abs(amount);
            OnValuesChanged?.Invoke();
        }

        public void SubtractComponent(COMPONENT_TYPE type, int amount)
        {
            _components[type] -= Mathf.Abs(amount);
            OnValuesChanged?.Invoke();
        }

        //============================================================================================================//

        public bool CanAffordFacilityBlueprint(TEST_FacilityBlueprint facilityBlueprint)
        {
            return CanAffordBits(facilityBlueprint.cost) && CanAffordComponents(facilityBlueprint.cost);
        }

        public bool CanAffordBits(BIT_TYPE type, int amount)
        {
            return CostCalculations.CanAffordResource(resources, type, amount);
        }
        public bool CanAffordBits(IEnumerable<CraftCost> levelCost)
        {
            Dictionary<BIT_TYPE, int> tempDictionary = new Dictionary<BIT_TYPE, int>(resources);
            return CostCalculations.CanAffordResources(tempDictionary, levelCost);
        }

        public bool CanAffordComponents(IEnumerable<CraftCost> levelCost)
        {
            foreach (var craftCost in levelCost)
            {
                if ((craftCost.resourceType == CraftCost.TYPE.Component && craftCost.type == (int)COMPONENT_TYPE.CHIP && !facilityRanks.ContainsKey(FACILITY_TYPE.WORKBENCHCHIP)) ||
                    (craftCost.resourceType == CraftCost.TYPE.Component && craftCost.type == (int)COMPONENT_TYPE.COIL && !facilityRanks.ContainsKey(FACILITY_TYPE.WORKBENCHCOIL)) ||
                    (craftCost.resourceType == CraftCost.TYPE.Component && craftCost.type == (int)COMPONENT_TYPE.FUSOR && !facilityRanks.ContainsKey(FACILITY_TYPE.WORKBENCHFUSOR)))
                {
                    Debug.Log("MISSING FACILITY");
                    return false;
                }
            }

            Dictionary<COMPONENT_TYPE, int> tempDictionary = new Dictionary<COMPONENT_TYPE, int>(_components);

            return CostCalculations.CanAffordComponents(tempDictionary, levelCost);
        }

        public bool CanAffordPart(PART_TYPE partType, int level, bool isRecursive)
        {
            Dictionary<BIT_TYPE, int> tempResourceDictionary = new Dictionary<BIT_TYPE, int>(resources);
            Dictionary<COMPONENT_TYPE, int> tempComponentDictionary = new Dictionary<COMPONENT_TYPE, int>(_components);
            List<BlockData> tempPartsInStorage = new List<BlockData>(partsInStorageBlockData);
            return CostCalculations.CanAffordPart(tempResourceDictionary, tempComponentDictionary, tempPartsInStorage, partType, level, isRecursive);
        }

        //============================================================================================================//

        public void AddSectorProgression(int sector, int waveAt)
        {
            if (maxSectorProgression.ContainsKey(sector))
                maxSectorProgression[sector] = Mathf.Max(maxSectorProgression[sector], waveAt);
            else
                maxSectorProgression.Add(sector, waveAt);
        }

        public bool CheckIfQualifies(int sector, int waveAt)
        {
            if (maxSectorProgression.ContainsKey(sector) && maxSectorProgression[sector] >= waveAt)
                return true;

            return false;
        }
        public bool CheckIfCompleted(int sector, int waveAt)
        {
            return maxSectorProgression.ContainsKey(sector) && maxSectorProgression[sector] > waveAt;
        }
        public float GetLevelResourceModifier(int sector, int wave)
        {
            int index = levelResourceModifier.FindIndex(s => s.Sector == sector && s.Wave == wave);

            if (index == -1)
            {
                levelResourceModifier.Add(new SectorWaveModifier
                {
                    Sector = sector,
                    Wave = wave,
                    Modifier = 1.0f
                });
                index = levelResourceModifier.FindIndex(s => s.Sector == sector && s.Wave == wave);
            }

            return levelResourceModifier[index].Modifier;
        }

        public void ReduceLevelResourceModifier(int sector, int wave)
        {
            int index = levelResourceModifier.FindIndex(s => s.Sector == sector && s.Wave == wave);
            float previousModifier;

            if (index >= 0)
            {
                previousModifier = levelResourceModifier[index].Modifier;
                levelResourceModifier.RemoveAt(index);
            }
            else
            {
                previousModifier = 1.0f;
            }

            levelResourceModifier.Add(new SectorWaveModifier
            {
                Sector = sector,
                Wave = wave,
                Modifier = previousModifier * Globals.LevelResourceDropReductionAmount
            });
        }

        //============================================================================================================//

        //DontShowAgain Tracking Functions
        //====================================================================================================================//

        public void AddDontShowAgainKey(string key)
        {
            _dontShowAgainKeys.Add(key);
        }

        //====================================================================================================================//
        
        public List<BlockData> GetCurrentBlockData()
        {
            return currentBlockData;
        }

        public void SetCurrentBlockData(List<BlockData> blockData)
        {
            currentBlockData.Clear();
            currentBlockData.AddRange(blockData);
        }

        public List<BlockData> GetRecoveryDroneBlockData()
        {
            return recoveryDroneBlockData;
        }

        public void SetRecoveryDroneBlockData(List<BlockData> blockData)
        {
            recoveryDroneBlockData.Clear();
            recoveryDroneBlockData.AddRange(blockData);
        }

        public List<BlockData> GetCurrentPartsInStorage()
        {
            return partsInStorageBlockData;
        }

        public void SetCurrentPartsInStorage(List<BlockData> blockData)
        {
            partsInStorageBlockData.Clear();
            partsInStorageBlockData.AddRange(blockData);
        }

        public void AddPartToStorage(BlockData blockData)
        {
            partsInStorageBlockData.Add(blockData);
            OnValuesChanged?.Invoke();
        }

        public void RemovePartFromStorage(BlockData blockData)
        {
            partsInStorageBlockData.Remove(partsInStorageBlockData.FirstOrDefault(b => b.Level == blockData.Level && b.Type == blockData.Type));
            OnValuesChanged?.Invoke();
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
            OnValuesChanged?.Invoke();
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
                        case PART_TYPE.BOOST:
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
            OnValuesChanged?.Invoke();
        }
        
        public bool CheckHasFacility(FACILITY_TYPE type, int level = 0)
        {
            if (_facilityRanks.TryGetValue(type, out var rank))
                return rank >= level;

            return false;
        }

        public void UnlockFacilityLevel(FACILITY_TYPE type, int level, bool triggerMissionCheck = true)
        {
            FacilityRemoteData remoteData = FactoryManager.Instance.FacilityRemote.GetRemoteData(type);
            if (_facilityRanks.ContainsKey(type) && _facilityRanks[type] < level)
            {
                _facilityRanks[type] = level;
            }
            else
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
                    _rationCapacity += increaseAmount;
                    break;
                case FACILITY_TYPE.STORAGEELECTRICITY:
                    _resourceCapacity[BIT_TYPE.YELLOW] += increaseAmount;
                    break;
                case FACILITY_TYPE.STORAGEFUEL:
                    _resourceCapacity[BIT_TYPE.RED] += increaseAmount;
                    break;
                case FACILITY_TYPE.STORAGEPLASMA:
                    _resourceCapacity[BIT_TYPE.GREEN] += increaseAmount;
                    break;
                case FACILITY_TYPE.STORAGESCRAP:
                    _resourceCapacity[BIT_TYPE.GREY] += increaseAmount;
                    break;
                case FACILITY_TYPE.STORAGEWATER:
                    _resourceCapacity[BIT_TYPE.BLUE] += increaseAmount;
                    break;
            }

            //Debug.Log(_rationCapacity);
            OnCapacitiesChanged?.Invoke();
            OnValuesChanged?.Invoke();
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
            OnValuesChanged?.Invoke();
        }

        public void SaveData()
        {
            LevelRingConnectionsJson = LevelRingNodeTree.ConvertNodeTreeIntoConnections();
        }
    }
}

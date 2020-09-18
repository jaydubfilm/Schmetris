using System;
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
            {BIT_TYPE.RED, 0},
            {BIT_TYPE.BLUE, 100},
            {BIT_TYPE.YELLOW, 100},
            {BIT_TYPE.GREEN, 0},
            {BIT_TYPE.GREY, 0},
        };

        [JsonIgnore]
        public Dictionary<BIT_TYPE, int> resourceCapacities => _resourceCapacity;
        [JsonProperty]
        private Dictionary<BIT_TYPE, int> _resourceCapacity = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 5000},
            {BIT_TYPE.BLUE, 5000},
            {BIT_TYPE.YELLOW, 5000},
            {BIT_TYPE.GREEN, 5000},
            {BIT_TYPE.GREY, 5000},
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

        public List<BlockData> currentBlockData = new List<BlockData>();
        public List<BlockData> partsInStorageBlockData = new List<BlockData>();

        public List<Blueprint> unlockedBlueprints = new List<Blueprint>();

        public Dictionary<int, int> maxSectorProgression = new Dictionary<int, int>();

        public MissionsCurrentData missionsCurrentData = null;

        public int currentModularSectorIndex = 0;

        public int numLives = 3;
        public bool firstFlight = true;

        public int Level;
        public int Gears;

        [JsonIgnore]
        public IReadOnlyDictionary<FACILITY_TYPE, int> facilityRanks => _facilityRanks;
        [JsonProperty]
        private Dictionary<FACILITY_TYPE, int> _facilityRanks = new Dictionary<FACILITY_TYPE, int>
        {
            {FACILITY_TYPE.REFINERY, 0}
        };

        [JsonIgnore]
        public IReadOnlyDictionary<FACILITY_TYPE, int> facilityBlueprintRanks => _facilityBlueprintRanks;
        [JsonProperty]
        private Dictionary<FACILITY_TYPE, int> _facilityBlueprintRanks = new Dictionary<FACILITY_TYPE, int>
        {
            {FACILITY_TYPE.FREEZER, 0},
            {FACILITY_TYPE.STORAGEELECTRICITY, 0},
            {FACILITY_TYPE.STORAGEFUEL, 0},
            {FACILITY_TYPE.STORAGEPLASMA, 0},
            {FACILITY_TYPE.STORAGESCRAP, 0},
            {FACILITY_TYPE.STORAGEWATER, 0},
            {FACILITY_TYPE.WORKBENCHCHIP, 0},
            {FACILITY_TYPE.WORKBENCHCOIL, 0},
            {FACILITY_TYPE.WORKBENCHFUSOR, 0},
            {FACILITY_TYPE.REFINERY, 1}
        };

        public string PlaythroughID = string.Empty;

        //============================================================================================================//

        public void ChangeGears(int amount)
        {
            Gears += amount;
            if (LevelManager.Instance.WaveEndSummaryData != null)
            {
                LevelManager.Instance.WaveEndSummaryData.numGearsGained += amount;
            }

            int gearsToLevelUp = LevelManager.Instance.PlayerlevelRemoteDataScriptableObject.GetRemoteData(Level).GearsToLevelUp;
            if (Gears >= gearsToLevelUp)
            {
                Gears -= gearsToLevelUp;
                DropLevelLoot();
                if (LevelManager.Instance.WaveEndSummaryData != null)
                {
                    LevelManager.Instance.WaveEndSummaryData.numLevelsGained++;
                }
                Level++;
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
                    PlayerPersistentData.PlayerData.UnlockBlueprint(rdsValueBlueprint.rdsValue);
                    Toast.AddToast("Unlocked Blueprint!");
                    levelUpLoot.RemoveAt(i);
                }
                if (levelUpLoot[i] is RDSValue<FacilityBlueprint> rdsValueFacilityBlueprint)
                {
                    PlayerPersistentData.PlayerData.UnlockFacilityBlueprintLevel(rdsValueFacilityBlueprint.rdsValue);
                    Toast.AddToast("Unlocked Facility Blueprint!");
                    levelUpLoot.RemoveAt(i);
                }
                else if (levelUpLoot[i] is RDSValue<Vector2Int> rdsValueGears)
                {
                    PlayerPersistentData.PlayerData.ChangeGears(UnityEngine.Random.Range(rdsValueGears.rdsValue.x, rdsValueGears.rdsValue.y));
                    levelUpLoot.RemoveAt(i);
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
            _resources[type] = Mathf.Min(value, _resourceCapacity[type]);
        }

        //============================================================================================================//

        public void SetLiquidResource(BIT_TYPE type, float value)
        {
            _liquidResource[type] = Mathf.Clamp(value, 0f, _liquidCapacity[type]);
            //_liquidResource[type] = value;

            OnValuesChanged?.Invoke();
        }

        public void SetLiquidResource(Dictionary<BIT_TYPE, float> liquidValues)
        {
            foreach (var value in liquidValues)
            {
              _liquidResource[value.Key] = Mathf.Clamp(value.Value, 0f, _liquidCapacity[value.Key]);
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

        public void SetCapacity(BIT_TYPE type, int amount)
        {
            _liquidCapacity[type] = amount;
            OnCapacitiesChanged?.Invoke();
        }
        public void SetCapacities(Dictionary<BIT_TYPE, int> capacities)
        {
            foreach (var capacity in capacities)
            {
                _liquidCapacity[capacity.Key] = capacity.Value;
            }

            OnCapacitiesChanged?.Invoke();
        }

        public void ClearLiquidCapacity()
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

        //============================================================================================================//

        public void AddResources(Dictionary<BIT_TYPE, int> toAdd, float multiplier)
        {
            CostCalculations.AddResources(ref _resources, toAdd, multiplier);
            OnValuesChanged?.Invoke();
        }

        public void AddResource(BIT_TYPE type, int amount)
        {
            _resources[type] = Mathf.Min(_resources[type] + amount, _resourceCapacity[type]);
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

        public void AddLiquidResource(BIT_TYPE type, float amount)
        {
            MissionManager.ProcessLiquidResourceConvertedMission(type, amount);
            _liquidResource[type] = Mathf.Clamp(liquidResource[type] + Mathf.Abs(amount), 0, liquidCapacity[type]);
            OnValuesChanged?.Invoke();
        }

        public void SubtractLiquidResource(BIT_TYPE type, float amount)
        {
            _liquidResource[type] = Mathf.Clamp(liquidResource[type] - Mathf.Abs(amount), 0, liquidCapacity[type]);
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

        //============================================================================================================//

        public List<BlockData> GetCurrentBlockData()
        {
            return currentBlockData;
        }

        public void SetCurrentBlockData(List<BlockData> blockData)
        {
            currentBlockData.Clear();
            currentBlockData.AddRange(blockData);
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
            if (!unlockedBlueprints.Any(b => b.name == blueprint.name))
            {
                unlockedBlueprints.Add(blueprint);

                if (LevelManager.Instance.WaveEndSummaryData != null)
                {
                    LevelManager.Instance.WaveEndSummaryData.numBlueprintsUnlocked++;
                    LevelManager.Instance.WaveEndSummaryData.blueprintsUnlockedStrings.Add(blueprint.name);
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

        public void UnlockFacilityLevel(FACILITY_TYPE type, int level)
        {
            if (_facilityRanks.ContainsKey(type) && _facilityRanks[type] < level)
            {
                _facilityRanks[type] = level;
            }
            else
            {
                _facilityRanks.Add(type, level);
            }

            int increaseAmount = FactoryManager.Instance.FacilityRemote.GetRemoteData(type).levels[level].increaseAmount;
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

            Debug.Log(_rationCapacity);

            OnValuesChanged?.Invoke();
        }

        public void UnlockFacilityBlueprintLevel(FacilityBlueprint facilityBlueprint)
        {
            if (_facilityBlueprintRanks.ContainsKey(facilityBlueprint.facilityType) && _facilityBlueprintRanks[facilityBlueprint.facilityType] < facilityBlueprint.level)
            {
                _facilityBlueprintRanks[facilityBlueprint.facilityType] = facilityBlueprint.level;
            }
            else
            {
                _facilityBlueprintRanks.Add(facilityBlueprint.facilityType, facilityBlueprint.level);
            }
            OnValuesChanged?.Invoke();
        }
    }
}

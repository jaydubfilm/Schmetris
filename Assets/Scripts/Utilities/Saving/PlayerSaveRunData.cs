using System;
using StarSalvager.UI.Scrapyard;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using StarSalvager.Missions;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Math;
using StarSalvager.Values;

namespace StarSalvager.Utilities.Saving
{
    [Serializable]
    public class PlayerSaveRunData
    {
        //============================================================================================================//

        //TODO: Add an add/subtract function for ResourceAmount, and make this IReadOnlyDictionary<>
        [JsonIgnore]
        public Dictionary<BIT_TYPE, int> readOnlyBits => _resources;

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
        public Dictionary<BIT_TYPE, int> ResourceCapacities => _resourceCapacity;
        [JsonProperty]
        private Dictionary<BIT_TYPE, int> _resourceCapacity = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 300},
            {BIT_TYPE.BLUE, 300},
            {BIT_TYPE.YELLOW, 300},
            {BIT_TYPE.GREEN, 300},
            {BIT_TYPE.GREY, 300},
        };

        public int RationCapacity = 500;

        [JsonIgnore]
        public Dictionary<COMPONENT_TYPE, int> readOnlyComponents => _components;
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

        public List<BlockData> mainDroneBlockData = new List<BlockData>();
        public List<BlockData> recoveryDroneBlockData = new List<BlockData>();
        public List<BlockData> partsInStorageBlockData = new List<BlockData>();

        public List<SectorWaveModifier> levelResourceModifier = new List<SectorWaveModifier>();

        public MissionsCurrentData missionsCurrentData = null;

        public int currentModularSectorIndex = 0;

        public bool firstFlight = true;

        public string PlaythroughID = string.Empty;

        [JsonIgnore]
        public IReadOnlyList<string> DontShowAgainKeys => _dontShowAgainKeys;
        [JsonProperty] 
        private List<string> _dontShowAgainKeys = new List<string>();

        [JsonIgnore]
        public LevelRingNodeTree LevelRingNodeTree = new LevelRingNodeTree();
        [JsonIgnore]
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

        [JsonIgnore]
        public List<int> ShortcutNodes = new List<int>()
        {
            4,
            6,
            8,
            15,
            16,
            17,
            19,
            24,
        };

        public List<int> PlayerPreviouslyCompletedNodes = new List<int>()
        {
            0
        };

        //============================================================================================================//

        public PlayerSaveRunData()
        {
            LevelRingNodeTree.ReadInNodeConnectionData(LevelRingConnectionsJson);
        }

        //============================================================================================================//

        public void IncreaseResourceCapacity(BIT_TYPE bitType, int amount)
        {
            if (!_resourceCapacity.ContainsKey(bitType))
            {
                Debug.LogError("Resource Capacities missing Bit Type " + bitType);
            }

            _resourceCapacity[bitType] += amount;
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

        public (float current, float capacity) GetCurrentAndCapacity(BIT_TYPE type, bool isRecoveryDrone)
        {
            var current = isRecoveryDrone ? recoveryDroneLiquidResource[type] : liquidResource[type];
            var capacity = isRecoveryDrone ? recoveryDroneLiquidCapacity[type] : liquidCapacity[type];

            return (current, capacity);
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
        }

        public void SetLiquidResources(Dictionary<BIT_TYPE, float> liquidValues, bool isRecoveryDrone)
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
        }

        //============================================================================================================//

        public void SetComponents(COMPONENT_TYPE type, int value)
        {
            _components[type] = value;
        }

        public void SetComponents(Dictionary<COMPONENT_TYPE, int> liquidValues)
        {
            foreach (var value in liquidValues)
            {
                _components[value.Key] = value.Value;
            }
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
            PlayerDataManager.OnCapacitiesChanged?.Invoke();
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

            PlayerDataManager.OnCapacitiesChanged?.Invoke();
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

        public void SubtractResources(BIT_TYPE bitType, int amount)
        {
            _resources[bitType] = Mathf.Max(_resources[bitType] - amount, 0);
        }

        public void AddResources(Dictionary<BIT_TYPE, int> toAdd, float multiplier)
        {
            CostCalculations.AddResources(ref _resources, toAdd, multiplier);

            foreach (var bitType in toAdd.Select(keyValuePair => keyValuePair.Key))
            {
                _resources[bitType] = Mathf.Min(_resources[bitType], ResourceCapacities[bitType]);
            }
        }

        public Dictionary<BIT_TYPE, int> AddResourcesReturnWasted(Dictionary<BIT_TYPE, int> toAdd, float multiplier)
        {
            CostCalculations.AddResources(ref _resources, toAdd, multiplier);

            Dictionary<BIT_TYPE, int> wastedResources = new Dictionary<BIT_TYPE, int>();

            foreach (var bitType in toAdd.Select(keyValuePair => keyValuePair.Key))
            {
                if (ResourceCapacities[bitType] < _resources[bitType])
                {
                    wastedResources.Add(bitType, _resources[bitType] - ResourceCapacities[bitType]);
                }
                
                _resources[bitType] = Mathf.Min(_resources[bitType], ResourceCapacities[bitType]);
            }

            return wastedResources;
        }

        public void AddResource(BIT_TYPE type, int amount)
        {
            _resources[type] = Mathf.Min(_resources[type] + amount, ResourceCapacities[type]);
        }

        public void AddPartResources(PART_TYPE partType, int level, bool isRecursive)
        {
            CostCalculations.AddResources(ref _resources, partType, level, isRecursive);
        }

        public void AddResources(BlockData blockData, bool isRecursive)
        {
            if (!blockData.ClassType.Equals(nameof(Part)))
                return;
            
            AddPartResources((PART_TYPE) blockData.Type, blockData.Level, isRecursive);
        }

        public void SubtractResources(IEnumerable<CraftCost> cost)
        {
            CostCalculations.SubtractResources(ref _resources, cost);
        }

        public void SubtractComponents(IEnumerable<CraftCost> cost)
        {
            CostCalculations.SubtractComponents(ref _components, cost);
        }

        public void SubtractPartCosts(PART_TYPE partType, int level, bool isRecursive, float costModifier = 1.0f)
        {
            CostCalculations.SubtractPartCosts(ref _resources, ref _components, partsInStorageBlockData, partType, level, isRecursive, costModifier);
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
        }

        //============================================================================================================//

        public void AddComponent(COMPONENT_TYPE type, int amount)
        {
            _components[type] += Mathf.Abs(amount);
        }

        public void SubtractComponent(COMPONENT_TYPE type, int amount)
        {
            _components[type] -= Mathf.Abs(amount);
        }

        //============================================================================================================//

        public bool CanAffordFacilityBlueprint(TEST_FacilityBlueprint facilityBlueprint)
        {
            return CanAffordBits(facilityBlueprint.cost) && CanAffordComponents(facilityBlueprint.cost);
        }

        public bool CanAffordBits(IEnumerable<CraftCost> levelCost)
        {
            Dictionary<BIT_TYPE, int> tempDictionary = new Dictionary<BIT_TYPE, int>(_resources);
            return CostCalculations.CanAffordResources(tempDictionary, levelCost);
        }

        public bool CanAffordComponents(IEnumerable<CraftCost> levelCost)
        {
            foreach (var craftCost in levelCost)
            {
                if ((craftCost.resourceType == CraftCost.TYPE.Component && craftCost.type == (int)COMPONENT_TYPE.CHIP && !PlayerDataManager.GetFacilityRanks().ContainsKey(FACILITY_TYPE.WORKBENCHCHIP)) ||
                    (craftCost.resourceType == CraftCost.TYPE.Component && craftCost.type == (int)COMPONENT_TYPE.COIL && !PlayerDataManager.GetFacilityRanks().ContainsKey(FACILITY_TYPE.WORKBENCHCOIL)) ||
                    (craftCost.resourceType == CraftCost.TYPE.Component && craftCost.type == (int)COMPONENT_TYPE.FUSOR && !PlayerDataManager.GetFacilityRanks().ContainsKey(FACILITY_TYPE.WORKBENCHFUSOR)))
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
            Dictionary<BIT_TYPE, int> tempResourceDictionary = new Dictionary<BIT_TYPE, int>(_resources);
            Dictionary<COMPONENT_TYPE, int> tempComponentDictionary = new Dictionary<COMPONENT_TYPE, int>(_components);
            List<BlockData> tempPartsInStorage = new List<BlockData>(partsInStorageBlockData);
            return CostCalculations.CanAffordPart(tempResourceDictionary, tempComponentDictionary, tempPartsInStorage, partType, level, isRecursive);
        }

        //============================================================================================================//

        public bool CheckIfCompleted(int sector, int waveAt)
        {
            for (int i = 0; i < PlayerPreviouslyCompletedNodes.Count; i++)
            {
                (int, int) curSectorWaveTuple = LevelRingNodeTree.ConvertNodeIndexIntoSectorWave(PlayerPreviouslyCompletedNodes[i]);

                if (curSectorWaveTuple.Item1 == sector && curSectorWaveTuple.Item2 == waveAt)
                {
                    return true;
                }
            }

            return false;
        }

        //============================================================================================================//

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
            return mainDroneBlockData;
        }

        public void SetShipBlockData(List<BlockData> blockData)
        {
            mainDroneBlockData.Clear();
            mainDroneBlockData.AddRange(blockData);
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
            PlayerDataManager.OnValuesChanged?.Invoke();
        }

        public void RemovePartFromStorage(BlockData blockData)
        {
            partsInStorageBlockData.Remove(partsInStorageBlockData.FirstOrDefault(b => b.Level == blockData.Level && b.Type == blockData.Type));
        }

        public void SaveData()
        {
            LevelRingConnectionsJson = LevelRingNodeTree.ConvertNodeTreeIntoConnections();
        }
    }
}

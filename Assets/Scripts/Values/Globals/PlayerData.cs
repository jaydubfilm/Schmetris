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

namespace StarSalvager.Values
{
    public class PlayerData
    {
        [JsonIgnore]
        public static Action OnValuesChanged;
        [JsonIgnore]
        public static Action OnCapacitiesChanged;

        //============================================================================================================//

        //TODO: Add an add/subtract function for ResourceAmount
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

        [JsonProperty]
        private Dictionary<BIT_TYPE, int> _resourceCapacity = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 5000},
            {BIT_TYPE.BLUE, 300},
            {BIT_TYPE.YELLOW, 300},
            {BIT_TYPE.GREEN, 5000},
            {BIT_TYPE.GREY, 5000},
        };

        [JsonIgnore]
        public ReadOnlyDictionary<COMPONENT_TYPE, int> components => new ReadOnlyDictionary<COMPONENT_TYPE, int>(_components);
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
        public ReadOnlyDictionary<BIT_TYPE, float> liquidResource => new ReadOnlyDictionary<BIT_TYPE, float>(_liquidResource);
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
        public ReadOnlyDictionary<BIT_TYPE, int> liquidCapacity => new ReadOnlyDictionary<BIT_TYPE, int>(_liquidCapacity);
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

        public string PlaythroughID = string.Empty;

        //============================================================================================================//

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

        public void AddResources(Dictionary<BIT_TYPE, int> toAdd)
        {
            CostCalculations.AddResources(ref _resources, toAdd);
            OnValuesChanged?.Invoke();
        }

        public void AddResources(PART_TYPE partType, int level, bool isRecursive)
        {
            CostCalculations.AddResources(ref _resources, partType, level, isRecursive);
            OnValuesChanged?.Invoke();
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


        public bool CanAffordCost(BIT_TYPE type, int amount)
        {
            return CostCalculations.CanAffordResource(resources, type, amount);
        }
        public bool CanAffordCost(IEnumerable<CraftCost> levelCost)
        {
            Dictionary<BIT_TYPE, int> tempDictionary = new Dictionary<BIT_TYPE, int>(resources);
            return CostCalculations.CanAffordResources(tempDictionary, levelCost);
        }

        public bool CanAffordPart(PART_TYPE partType, int level, bool isRecursive)
        {
            Dictionary<BIT_TYPE, int> tempResourceDictionary = new Dictionary<BIT_TYPE, int>(resources);
            Dictionary<COMPONENT_TYPE, int> tempComponentDictionary = new Dictionary<COMPONENT_TYPE, int>(components);
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
            }
        }
    }
}

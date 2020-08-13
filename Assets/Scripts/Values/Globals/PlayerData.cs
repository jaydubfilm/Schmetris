using System;
using StarSalvager.UI.Scrapyard;
using StarSalvager.Utilities;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace StarSalvager.Values
{
    public class PlayerData
    {
        [JsonIgnore]
        public Action OnValuesChanged;
        
        //TODO: Add an add/subtract function for ResourceAmount
        [JsonIgnore]
        public Dictionary<BIT_TYPE, int> resources => _resources;

        [JsonProperty]
        private Dictionary<BIT_TYPE, int> _resources = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 1250},
            {BIT_TYPE.BLUE, 1250},
            {BIT_TYPE.YELLOW, 1250},
            {BIT_TYPE.GREEN, 1250},
            {BIT_TYPE.GREY, 1250},
        };

        [JsonIgnore]
        public Dictionary<COMPONENT_TYPE, int> components => _components;
        [JsonProperty]
        private Dictionary<COMPONENT_TYPE, int> _components = new Dictionary<COMPONENT_TYPE, int>
        {
            {COMPONENT_TYPE.CALLIT, 3},
            {COMPONENT_TYPE.DOHICKEY, 3},
            {COMPONENT_TYPE.GADGET, 3},
            {COMPONENT_TYPE.GIZMO, 3},
            {COMPONENT_TYPE.THINGY, 3}
        };
        
        [JsonIgnore]
        public Dictionary<BIT_TYPE, float> liquidResource => _liquidResource;
        [JsonProperty]
        //FIXME This needs to use some sort of capacity value
        private Dictionary<BIT_TYPE, float> _liquidResource = new Dictionary<BIT_TYPE, float>
        {
            {BIT_TYPE.RED, 250},
            {BIT_TYPE.BLUE, 0},
            {BIT_TYPE.YELLOW, 0},
            {BIT_TYPE.GREEN, 0},
            {BIT_TYPE.GREY, 0},
        };

        public List<BlockData> currentBlockData = new List<BlockData>();
        public List<BlockData> partsInStorageBlockData = new List<BlockData>();

        public List<TEST_Blueprint> unlockedBlueprints = new List<TEST_Blueprint>();

        public Dictionary<int, int> maxSectorProgression = new Dictionary<int, int>();

        //============================================================================================================//

        public void SetResources(Dictionary<BIT_TYPE, int> values)
        {
            _resources = values;
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
            liquidResource[type] += Mathf.Abs(amount);
            OnValuesChanged?.Invoke();
        }
        
        public void SubtractLiquidResource(BIT_TYPE type, float amount)
        {
            liquidResource[type] -= Mathf.Abs(amount);
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

        public void UnlockBlueprint(TEST_Blueprint blueprint)
        {   
            if (!unlockedBlueprints.Any(b => b.name == blueprint.name))
            {
                unlockedBlueprints.Add(blueprint);
            }
        }
    }
}
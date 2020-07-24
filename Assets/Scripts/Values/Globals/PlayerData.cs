using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarSalvager.Values
{
    public class PlayerData
    {
        //TODO: Add an add/subtract function for ResourceAmount
        
        private Dictionary<BIT_TYPE, int> resources = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 1250},
            {BIT_TYPE.BLUE, 1250},
            {BIT_TYPE.YELLOW, 1250},
            {BIT_TYPE.GREEN, 1250},
            {BIT_TYPE.GREY, 1250},
        };

        List<BlockData> currentBlockData = new List<BlockData>();

        public Dictionary<int, int> maxSectorProgression = new Dictionary<int, int>();

        //============================================================================================================//

        public Dictionary<BIT_TYPE, int> GetResources()
        {
            return resources;
        }

        public void AddResources(Dictionary<BIT_TYPE, int> toAdd)
        {
            List<BIT_TYPE> keys = resources.Keys.ToList();
            foreach (BIT_TYPE key in keys)
            {
                if (toAdd.ContainsKey(key))
                {
                    resources[key] += toAdd[key];
                }
            }
        }

        public void AddResources(PART_TYPE partType, int level)
        {
            var remoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).costs[level];
            foreach (ResourceAmount resource in remoteData.levelCosts)
            {
                resources[resource.type] += resource.amount;
            }
        }

        public void SubtractResources(Dictionary<BIT_TYPE, int> toSubtract)
        {
            List<BIT_TYPE> keys = resources.Keys.ToList();
            foreach (BIT_TYPE key in keys)
            {
                if (toSubtract.ContainsKey(key))
                {
                    resources[key] -= toSubtract[key];
                }
            }
        }

        public void SubtractResources(PART_TYPE partType, int level)
        {
            var remoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).costs[level];
            foreach (ResourceAmount resource in remoteData.levelCosts)
            {
                resources[resource.type] -= resource.amount;
            }
        }

        public void SubtractResources(LevelCost cost)
        {
            foreach (ResourceAmount resource in cost.levelCosts)
            {
                resources[resource.type] -= resource.amount;
            }
        }

        public bool CanAfford(LevelCost levelCost)
        {
            foreach (ResourceAmount resource in levelCost.levelCosts)
            {
                if (resources[resource.type] < resource.amount)
                    return false;
            }
            return true;
        }

        public bool CanAffordPart(PART_TYPE partType, int level)
        {
            var remoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).costs[level];
            return CanAfford(remoteData);
        }

        public bool CanAffordUpgrade(PART_TYPE partType, int currentLevel, int levelTo)
        {
            return CanAfford(GetCostDifference(partType, currentLevel, levelTo));
        }

        public LevelCost GetCostDifference(PART_TYPE partType, int currentLevel, int levelTo)
        {
            var currentCost = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).costs[currentLevel];
            var levelToCost = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).costs[levelTo];

            foreach (ResourceAmount resource in levelToCost.levelCosts)
            {
                if (currentCost.levelCosts.Count(r => r.type == resource.type) != 0)
                {
                    resources[resource.type] -= currentCost.levelCosts.FirstOrDefault(r => r.type == resource.type).amount;
                }
            }

            return levelToCost;
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
    }
}
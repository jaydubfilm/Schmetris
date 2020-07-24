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
            {BIT_TYPE.RED, 250},
            {BIT_TYPE.BLUE, 250},
            {BIT_TYPE.YELLOW, 250},
            {BIT_TYPE.GREEN, 250},
            {BIT_TYPE.GREY, 250},
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

        public bool CanAffordPart(PART_TYPE partType, int level)
        {
            var remoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).costs[level];
            foreach (ResourceAmount resource in remoteData.levelCosts)
            {
                if (resources[resource.type] < resource.amount)
                    return false;
            }
            return true;
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
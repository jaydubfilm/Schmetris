using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarSalvager.Utilities
{
    public static class ResourceCalculations
    {
        public static void AddResources(ref Dictionary<BIT_TYPE, int> resources, Dictionary<BIT_TYPE, int> toAdd)
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

        public static void AddResources(ref Dictionary<BIT_TYPE, int> resources, PART_TYPE partType, int level, bool isRecursive)
        {
            var remoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).costs[level];
            foreach (ResourceAmount resource in remoteData.levelCosts)
            {
                resources[resource.type] += resource.amount;
            }

            if (!isRecursive)
                return;

            if (level > 0)
                AddResources(ref resources, partType, level - 1, isRecursive);
        }

        public static void SubtractResources(ref Dictionary<BIT_TYPE, int> resources, Dictionary<BIT_TYPE, int> toSubtract)
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

        public static void SubtractResources(ref Dictionary<BIT_TYPE, int> resources, PART_TYPE partType, int level, bool isRecursive)
        {
            var remoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).costs[level];
            foreach (ResourceAmount resource in remoteData.levelCosts)
            {
                resources[resource.type] -= resource.amount;
            }

            if (!isRecursive)
                return;

            if (level > 0)
                SubtractResources(ref resources, partType, level - 1, isRecursive);
        }

        public static void SubtractResources(ref Dictionary<BIT_TYPE, int> resources, LevelCost cost)
        {
            foreach (ResourceAmount resource in cost.levelCosts)
            {
                resources[resource.type] -= resource.amount;
            }
        }

        //============================================================================================================//

        public static bool CanAfford(Dictionary<BIT_TYPE, int> resources, LevelCost levelCost)
        {
            foreach (ResourceAmount resource in levelCost.levelCosts)
            {
                if (resources[resource.type] < resource.amount)
                    return false;
            }
            return true;
        }

        public static bool CanAffordPart(Dictionary<BIT_TYPE, int> resources, PART_TYPE partType, int level, bool isRecursive)
        {
            var remoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).costs[level];
            bool canAfford = CanAfford(resources, remoteData);

            if (!isRecursive || !canAfford || level == 0)
                return canAfford;
            else
            {
                SubtractResources(ref resources, remoteData);
                return CanAffordPart(resources, partType, level - 1, isRecursive);
            }
        }
    }
}
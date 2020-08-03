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
            //var remoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).costs[level];
            var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Bit)
                    continue;
                
                resources[(BIT_TYPE)resource.type] += resource.amount;
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
            //var remoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).costs[level];
            var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Bit)
                    continue;
                
                resources[(BIT_TYPE)resource.type] -= resource.amount;
            }

            if (!isRecursive)
                return;

            if (level > 0)
                SubtractResources(ref resources, partType, level - 1, isRecursive);
        }

        public static void SubtractResources(ref Dictionary<BIT_TYPE, int> resources, IEnumerable<CraftCost> levelCosts)
        {
            foreach (CraftCost resource in levelCosts)
            {
                if (resource.resourceType != CraftCost.TYPE.Bit)
                    continue;
                
                resources[(BIT_TYPE)resource.type] -= resource.amount;
            }
        }

        //============================================================================================================//

        public static bool CanAfford(Dictionary<BIT_TYPE, int> resources, IEnumerable<CraftCost> levelCosts)
        {
            foreach (CraftCost resource in levelCosts)
            {
                if (resource.resourceType != CraftCost.TYPE.Bit)
                    continue;
                
                if (resources[(BIT_TYPE)resource.type] < resource.amount)
                    return false;
            }
            return true;
        }

        public static bool CanAffordPart(Dictionary<BIT_TYPE, int> resources, PART_TYPE partType, int level, bool isRecursive)
        {
            //var remoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).costs[level];
            var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            bool canAfford = CanAfford(resources, costs);

            if (!isRecursive || !canAfford || level == 0)
                return canAfford;
            else
            {
                SubtractResources(ref resources, costs);
                return CanAffordPart(resources, partType, level - 1, isRecursive);
            }
        }
    }
}
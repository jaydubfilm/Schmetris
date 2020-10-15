using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarSalvager.Utilities.Math
{
    public static class CostCalculations
    {
        public static void AddResources(ref Dictionary<BIT_TYPE, int> resources, Dictionary<BIT_TYPE, int> toAdd, float multiplier)
        {
            List<BIT_TYPE> keys = resources.Keys.ToList();
            foreach (BIT_TYPE key in keys)
            {
                if (!resources.ContainsKey(key))
                {
                    resources.Add(key, 0);
                }

                if (toAdd.ContainsKey(key))
                {
                    resources[key] += (int)(toAdd[key] * multiplier);
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

                if (!resources.ContainsKey((BIT_TYPE)resource.type))
                {
                    resources.Add((BIT_TYPE)resource.type, 0); 
                }

                resources[(BIT_TYPE)resource.type] += resource.amount;
            }

            if (!isRecursive)
                return;

            if (level > 0)
                AddResources(ref resources, partType, level - 1, isRecursive);
        }

        public static void SubtractResources(ref Dictionary<BIT_TYPE, int> resources, Dictionary<BIT_TYPE, int> toSubtract, float costModifier = 1.0f)
        {
            List<BIT_TYPE> keys = resources.Keys.ToList();
            foreach (BIT_TYPE key in keys)
            {
                if (toSubtract.ContainsKey(key))
                {
                    resources[key] -= (int)(toSubtract[key] * costModifier);
                }
            }
        }

        public static void SubtractResources(ref Dictionary<BIT_TYPE, int> resources, PART_TYPE partType, int level, bool isRecursive, float costModifier = 1.0f)
        {
            //var remoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).costs[level];
            var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Bit)
                    continue;
                
                resources[(BIT_TYPE)resource.type] -= (int)(resource.amount * costModifier);
            }

            if (!isRecursive)
                return;

            if (level > 0)
                SubtractResources(ref resources, partType, level - 1, isRecursive, costModifier);
        }

        public static void SubtractResources(ref Dictionary<BIT_TYPE, int> resources, IEnumerable<CraftCost> levelCosts, float costModifier = 1.0f)
        {
            foreach (CraftCost resource in levelCosts)
            {
                if (resource.resourceType != CraftCost.TYPE.Bit)
                    continue;
                
                resources[(BIT_TYPE)resource.type] -= (int)(resource.amount * costModifier);
            }
        }

        public static void SubtractComponents(ref Dictionary<COMPONENT_TYPE, int> components, PART_TYPE partType, int level, bool isRecursive)
        {
            var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Component)
                    continue;

                components[(COMPONENT_TYPE)resource.type] -= resource.amount;
            }

            if (!isRecursive)
                return;

            if (level > 0)
                SubtractComponents(ref components, partType, level - 1, isRecursive);
        }

        public static void SubtractComponents(ref Dictionary<COMPONENT_TYPE, int> components, IEnumerable<CraftCost> costs)
        {
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Component)
                    continue;

                COMPONENT_TYPE key = (COMPONENT_TYPE)resource.type;
                if (components.ContainsKey(key))
                {
                    components[key] -= resource.amount;
                }
                else
                {
                    Debug.LogError("Trying to subtract from nonexisting resource");
                }
            }
        }

        public static void SubtractPremades(ref List<BlockData> partData, PART_TYPE partType, int level, bool isRecursive)
        {
            var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Part)
                    continue;

                List<BlockData> storedMatches = partData.FindAll(p => p.Type == resource.type && p.Level == resource.partPrerequisiteLevel);

                if (storedMatches.Count >= resource.amount)
                {
                    for (int i = 0; i < resource.amount; i++)
                    {
                        partData.Remove(storedMatches[0]);
                    }
                }
                else
                {
                    Debug.LogError("Trying to subtract from nonexisting resource");
                }
            }

            if (!isRecursive)
                return;

            if (level > 0)
                SubtractPremades(ref partData, partType, level - 1, isRecursive);
        }

        public static void SubtractPremades(ref List<BlockData> partData, IEnumerable<CraftCost> costs)
        {
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Part)
                    continue;

                List<BlockData> storedMatches = partData.FindAll(p => p.Type == resource.type && p.Level == resource.partPrerequisiteLevel);

                if (storedMatches.Count >= resource.amount)
                {
                    for (int i = 0; i < resource.amount; i++)
                    {
                        partData.Remove(storedMatches[0]);
                    }
                }
                else
                {
                    Debug.LogError("Trying to subtract from nonexisting resource");
                }
            }
        }

        public static void SubtractPartCosts(ref Dictionary<BIT_TYPE, int> resources, ref Dictionary<COMPONENT_TYPE, int> components, List<BlockData> storedParts,
            PART_TYPE partType, int level, bool isRecursive, float costModifier = 1.0f)
        {
            SubtractResources(ref resources, partType, level, isRecursive, costModifier);
            SubtractComponents(ref components, partType, level, isRecursive);
            SubtractPremades(ref storedParts, partType, level, isRecursive);
        }

        //============================================================================================================//

        public static bool CanAffordPart(Dictionary<BIT_TYPE, int> resources, Dictionary<COMPONENT_TYPE, int> components, List<BlockData> storedParts,
    PART_TYPE partType, int level, bool isRecursive, float resourceCostModifier = 1.0f)
        {
            List<BlockData> tempStoredParts = new List<BlockData>(storedParts);

            var hasResources = CanAffordPartResources(resources, partType, level, isRecursive, resourceCostModifier);
            var hasComponents = CanAffordPartComponents(components, partType, level, isRecursive);
            var hasParts = HasPartPremades(tempStoredParts, partType, level, isRecursive);

            return hasResources && hasComponents && hasParts;
        }

        public static bool CanAffordResources(Dictionary<BIT_TYPE, int> resources, IEnumerable<CraftCost> costs)
        {
            foreach (CraftCost resource in costs)
            {
                if ((BIT_TYPE)resource.type == BIT_TYPE.WHITE)
                {
                    Debug.LogError("Found a white bit cost in a resource check");
                }
                
                if (resource.resourceType != CraftCost.TYPE.Bit)
                    continue;

                if (!resources.ContainsKey((BIT_TYPE)resource.type))
                    return false;
                
                if (resources[(BIT_TYPE)resource.type] < resource.amount)
                    return false;
            }
            return true;
        }
        public static bool CanAffordResource(Dictionary<BIT_TYPE, int> resources, BIT_TYPE type, int amount)
        {
            return resources[type] >= amount;
        }

        public static bool CanAffordComponents(Dictionary<COMPONENT_TYPE, int> components, IEnumerable<CraftCost> costs)
        {
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Component)
                    continue;

                if (!components.ContainsKey((COMPONENT_TYPE)resource.type))
                    return false;

                if (components[(COMPONENT_TYPE)resource.type] < resource.amount)
                    return false;
            }
            return true;
        }

        public static bool HasPremades(List<BlockData> partData, IEnumerable<CraftCost> costs)
        {
            foreach (CraftCost resource in costs)
            {
                if (resource.resourceType != CraftCost.TYPE.Part)
                    continue;

                if (resource.type == (int)PART_TYPE.CORE)
                    continue;

                var partCount = partData.Count(p => p.Type == resource.type && p.Level == resource.partPrerequisiteLevel);
                if (partCount < resource.amount)
                    return false;
            }
            return true;
        }

        public static bool CanAffordPartResources(Dictionary<BIT_TYPE, int> resources, PART_TYPE partType, int level, bool isRecursive, float costModifier = 1.0f)
        {
            var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            bool canAfford = CanAffordResources(resources, costs);

            if (!isRecursive || !canAfford || level == 0)
                return canAfford;
            else
            {
                SubtractResources(ref resources, costs, costModifier);
                return CanAffordPartResources(resources, partType, level - 1, isRecursive, costModifier);
            }
        }

        public static bool CanAffordPartComponents(Dictionary<COMPONENT_TYPE, int> components, PART_TYPE partType, int level, bool isRecursive)
        {
            var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            bool canAfford = CanAffordComponents(components, costs);

            if (!isRecursive || !canAfford || level == 0)
                return canAfford;
            else
            {
                SubtractComponents(ref components, costs);
                return CanAffordPartComponents(components, partType, level - 1, isRecursive);
            }
        }

        public static bool HasPartPremades(List<BlockData> partData, PART_TYPE partType, int level, bool isRecursive)
        {
            var costs = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(partType).levels[level].cost;
            bool canAfford = HasPremades(partData, costs);

            if (!isRecursive || !canAfford || level == 0)
                return canAfford;
            else
            {
                SubtractPremades(ref partData, costs);
                return HasPartPremades(partData, partType, level - 1, isRecursive);
            }
        }
    }
}
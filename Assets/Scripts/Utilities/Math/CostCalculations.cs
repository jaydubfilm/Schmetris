using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarSalvager.Utilities.Math
{
    public static class CostCalculations
    {
        //============================================================================================================//

        /*public static bool CanAffordPart(Dictionary<BIT_TYPE, int> resources, Dictionary<COMPONENT_TYPE, int> components, List<BlockData> storedParts,
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
        }*/
    }
}
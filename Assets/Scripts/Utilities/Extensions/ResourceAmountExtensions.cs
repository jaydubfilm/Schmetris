using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class ResourceAmountExtensions
    {

        public static Dictionary<BIT_TYPE, int> ToDictionary(this IEnumerable<ResourceAmount> resources)
        {
            var dict = new Dictionary<BIT_TYPE, int>();
            foreach (var resourceAmount in resources)
            {
                if (dict.ContainsKey(resourceAmount.type))
                {
                    dict[resourceAmount.type] += resourceAmount.amount;
                    continue;
                }
                
                dict.Add(resourceAmount.type, resourceAmount.amount);
            }

            return dict;
        }
        
        public static List<ResourceAmount> ToResourceList(this Dictionary<BIT_TYPE, int> resources)
        {
            return resources.Select(resource => new ResourceAmount {type = resource.Key, amount = resource.Value})
                .ToList();
        }
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories;
using StarSalvager.Parts.Data;

namespace StarSalvager.Utilities.Extensions
{
    public static class PatchIEnumerableExtensions
    {
        /*public static int GetPatchUpgradersSum(this IEnumerable<PatchData> patches)
        {
            return patches.Where(x => x.Type == (int) PATCH_TYPE.GRADE).Sum(x => x.Level + 1);
        }*/
        
        public static float GetPatchMultiplier(this IEnumerable<PatchData> patches, in PATCH_TYPE patchType)
        {
            //Find out
            var pType = (int) patchType;
            var patchesOfType = patches.Where(x => x.Type == pType).ToList().AsReadOnly();
            
            if (patchesOfType.Count == 0)
                return 1;

            var patchRemoteData = FactoryManager.Instance.PatchRemoteData;
            
            var total = 0f;
            foreach (var patchData in patchesOfType)
            {
                var data = patchRemoteData.GetRemoteData(patchType).GetMultiplier(patchData.Level);
                    //.GetDataValue<float>(patchData.Level, PartProperties.KEYS.Multiplier);

                total += data;
            }

            switch (patchType)
            {
                case PATCH_TYPE.POWER:
                case PATCH_TYPE.RANGE:
                    return 1 + total;
                case PATCH_TYPE.REINFORCED:
                case PATCH_TYPE.EFFICIENCY:
                case PATCH_TYPE.FIRE_RATE:
                    return  1 - total;
                
                case PATCH_TYPE.AOE:
                case PATCH_TYPE.BURN:
                    throw new NotImplementedException($"{patchType} not yet implemented");
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(patchType), patchType, null);
            }
        }
        public static IReadOnlyDictionary<PATCH_TYPE, float> GetPatchMultipliers(this IEnumerable<PatchData> patches, params PATCH_TYPE[] patchTypes)
        {
            var outData = new Dictionary<PATCH_TYPE, float>();
            
            var patchDatas = patches as PatchData[] ?? patches.ToArray();
            foreach (var patchType in patchTypes)
            {
                if(outData.ContainsKey(patchType))
                    continue;

                var mult = patchDatas.GetPatchMultiplier(patchType);
                outData.Add(patchType, mult);
            }

            return outData;
        }
    }
}

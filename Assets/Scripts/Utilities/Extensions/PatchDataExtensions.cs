using StarSalvager.Factories;
using StarSalvager.Utilities.Saving;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class PatchDataExtensions
    {
        public static (int gears, int silver) GetPatchCost(this PatchData patchData)
        {
            var patchRemoteData = FactoryManager.Instance.PatchRemoteData.GetRemoteData(patchData.Type);
            var patchLevelData = patchRemoteData.Levels[patchData.Level];
            
            var costReductionMultiplier = PlayerDataManager.GetCurrentUpgradeValue(UPGRADE_TYPE.PATCH_COST);
            var gears = Mathf.CeilToInt(patchLevelData.gears * costReductionMultiplier);
            var silver = Mathf.CeilToInt(patchLevelData.silver * costReductionMultiplier);
            
            return (gears, silver);
        }
    }
}

using StarSalvager.Factories;

namespace StarSalvager.Utilities.Extensions
{
    public static class PatchDataExtensions
    {
        public static (int gears, int silver) GetPatchCost(this PatchData patchData)
        {
            var patchRemoteData = FactoryManager.Instance.PatchRemoteData.GetRemoteData(patchData.Type);
            var patchLevelData = patchRemoteData.Levels[patchData.Level];
            
            return (patchLevelData.gears, patchLevelData.silver);
        }
    }
}

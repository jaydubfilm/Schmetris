using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Parts.Data;
using StarSalvager.Utilities.JsonDataTypes;

namespace StarSalvager.Utilities.Extensions
{
    public static class PartDataExtensions
    {
        //GetPartDetails
        //====================================================================================================================//
        
        public static string GetPartDetails(this PartData partData)
        {
            var partRemoteData = FactoryManager.Instance.PartsRemoteData.GetRemoteData((PART_TYPE) partData.Type);
            return partData.GetPartDetails(partRemoteData);
        }
        
        public static string GetPartDetails(this PartData partData, in PartRemoteData partRemoteData)
        {
            var multipliers = partData.Patches.GetPatchMultipliers(
                PATCH_TYPE.POWER,
                PATCH_TYPE.RANGE,
                PATCH_TYPE.FIRE_RATE,
                PATCH_TYPE.EFFICIENCY);

            var partRemote = FactoryManager.Instance.PartsRemoteData.GetRemoteData(partRemoteData.partType);

            var modifiers = new[]
            {
                partRemote.TryGetValue(PartProperties.KEYS.Damage, out float damage),
                partRemote.TryGetValue(PartProperties.KEYS.Cooldown, out float cooldown),
                partRemote.TryGetValue(PartProperties.KEYS.Radius, out int range),
                partRemote.TryGetValue(PartProperties.KEYS.Projectile, out string projectileID),
                partRemote.TryGetValue(PartProperties.KEYS.Speed, out float speed),
                partRemote.TryGetValue(PartProperties.KEYS.Heal, out float heal)
            };

            var outList = new Dictionary<string, float>();



            if (modifiers[0])
                outList.Add("Damage", damage * multipliers[PATCH_TYPE.POWER]);

            if (partRemote.ammoUseCost > 0)
                outList.Add("Ammo", partRemote.ammoUseCost * multipliers[PATCH_TYPE.EFFICIENCY]);

            if (modifiers[1])
                outList.Add("Cooldown", cooldown * multipliers[PATCH_TYPE.FIRE_RATE]);

            if (modifiers[2])
            {
                outList.Add("Range", range * multipliers[PATCH_TYPE.RANGE]);
            }
            else if (modifiers[3])
            {
                var projectileRange = FactoryManager.Instance.ProjectileProfile
                    .GetProjectileProfileData(projectileID).ProjectileRange;

                outList.Add("Range", projectileRange * multipliers[PATCH_TYPE.RANGE]);
            }

            if (modifiers[4])
                outList.Add("Speed", speed);

            if (modifiers[5])
                outList.Add("Heal", heal);

            return string.Join("\n", outList.Select(x => $"{x.Key}: {x.Value}"));
        }

        //GetPatchNames
        //====================================================================================================================//
        
        public static string GetPatchNames(this PartData partData)
        {
            var patchRemoteData = FactoryManager.Instance.PatchRemoteData;
            var patches = partData.Patches;

            return string.Join("\n",
                patches.Where(x => x.Type != (int) PATCH_TYPE.EMPTY)
                    .Select(x => $"{patchRemoteData.GetRemoteData(x.Type).name} {x.Level + 1}"));
            
        }

        //====================================================================================================================//
        
    }

}
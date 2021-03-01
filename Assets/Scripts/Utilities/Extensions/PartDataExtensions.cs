﻿using System.Collections.Generic;
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

            return string.Join("\n", outList.Select(x => $"{x.Key}: {x.Value:#0.##}"));
        }

        private readonly struct PartDetail
        {
            private readonly string _name;
            private readonly object _value;
            private readonly object _previewValue;

            public PartDetail(string name, object value, object previewValue = null)
            {
                _name = name;
                _value = value;
                _previewValue = previewValue;
            }

            public override string ToString()
            {
                return $"{_name}: {(_previewValue is null ? $"{_value:#0.##}": $"<b>{_value:#0.##} -> <color=green>{_previewValue:#0.##}")}</color></b>";
            }
        }
        public static string GetPartDetailsPatchPreview(this PartData partData, in PatchData patchToPreview)
        {
            var partRemoteData = FactoryManager.Instance.PartsRemoteData.GetRemoteData((PART_TYPE) partData.Type);
            return partData.GetPartDetailsPatchPreview(partRemoteData, patchToPreview);
        }
        
        public static string GetPartDetailsPatchPreview(this PartData partData, in PartRemoteData partRemoteData, in PatchData patchToPreview)
        {

            //--------------------------------------------------------------------------------------------------------//
            
            PartDetail GetPartDetailInfo(in string title, 
                in float value, 
                in PATCH_TYPE patchType, 
                in PATCH_TYPE previewPatchType, 
                IReadOnlyDictionary<PATCH_TYPE, float> patchMultipliers,
                IReadOnlyDictionary<PATCH_TYPE, float> previewPatchMultipliers)
            {
                object previewValue = null;

                if (previewPatchType == patchType)
                    previewValue = value * previewPatchMultipliers[patchType];
                
                return new PartDetail(title,
                    value * patchMultipliers[patchType],
                    previewValue);
            }

            //--------------------------------------------------------------------------------------------------------//
            
            var previewType = (PATCH_TYPE)patchToPreview.Type;
            var previewPatches = new List<PatchData>(partData.Patches)
            {
                patchToPreview
            };

            var multipliers = partData.Patches.GetPatchMultipliers(
                PATCH_TYPE.POWER,
                PATCH_TYPE.RANGE,
                PATCH_TYPE.FIRE_RATE,
                PATCH_TYPE.EFFICIENCY);
            //FIXME There has to be a better way of implementing this
            var previewMultipliers = previewPatches.GetPatchMultipliers(
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

            //--------------------------------------------------------------------------------------------------------//
            
            var outList = new List<PartDetail>();

            if (modifiers[0])
                outList.Add(GetPartDetailInfo("Damage", damage, PATCH_TYPE.POWER, previewType, multipliers, previewMultipliers));

            if (partRemote.ammoUseCost > 0)
                outList.Add(GetPartDetailInfo("Ammo", partRemote.ammoUseCost, PATCH_TYPE.EFFICIENCY, previewType, multipliers, previewMultipliers));

            if (modifiers[1])
                outList.Add(GetPartDetailInfo("Cooldown", cooldown, PATCH_TYPE.FIRE_RATE, previewType, multipliers, previewMultipliers));

            if (modifiers[2])
            {
                outList.Add(GetPartDetailInfo("Range", range, PATCH_TYPE.RANGE, previewType, multipliers, previewMultipliers));
            }
            else if (modifiers[3])
            {
                var projectileRange = FactoryManager.Instance.ProjectileProfile
                    .GetProjectileProfileData(projectileID).ProjectileRange;

                outList.Add(GetPartDetailInfo("Range", projectileRange, PATCH_TYPE.RANGE, previewType, multipliers, previewMultipliers));
            }

            if (modifiers[4])
                outList.Add(new PartDetail("Speed", speed));


            if (modifiers[5])
                outList.Add(new PartDetail("Heal", heal));

            return string.Join("\n", outList.Select(x => x.ToString()));
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
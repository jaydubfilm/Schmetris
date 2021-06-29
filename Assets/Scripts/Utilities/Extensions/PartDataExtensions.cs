using System;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Parts.Data;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;

namespace StarSalvager.Utilities.Extensions
{
    public static class PartDataExtensions
    {
        //GetPartDetails
        //====================================================================================================================//
        
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
        
        public static string GetPartDetails(this PartData partData)
        {
            var partRemoteData = ((PART_TYPE) partData.Type).GetRemoteData();
            return partData.GetPartDetails(partRemoteData);
        }
        
        public static string GetPartDetails(this PartData partData, in PartRemoteData partRemoteData)
        {
            var multipliers = partData.Patches.GetPatchMultipliers(
                PATCH_TYPE.POWER,
                PATCH_TYPE.RANGE,
                PATCH_TYPE.FIRE_RATE,
                PATCH_TYPE.EFFICIENCY);

            var partProperties = new []
            {
                PartProperties.KEYS.Damage,
                PartProperties.KEYS.Cooldown,
                PartProperties.KEYS.Radius,
                PartProperties.KEYS.Projectile,
                PartProperties.KEYS.Speed,
                PartProperties.KEYS.Heal,
                PartProperties.KEYS.Health,
                PartProperties.KEYS.Capacity,
                PartProperties.KEYS.Magnet,
                PartProperties.KEYS.Reduction
            };
            
            var outList = new List<PartDetail>();

            //If the part uses ammo we'll check that first
            if (partRemoteData.ammoUseCost > 0 /*&& partData.Type != (int)PART_TYPE.CORE*/)
                outList.Add(new PartDetail("Ammo", 
                    partRemoteData.ammoUseCost * multipliers[PATCH_TYPE.EFFICIENCY] * PlayerDataManager.GetCurrentUpgradeValue(UPGRADE_TYPE.CATEGORY_EFFICIENCY, partRemoteData.category)));
            
            foreach (var property in partProperties)
            {
                if (!partRemoteData.TryGetValue(property, out var value))
                    continue;

                var propertyName = PartProperties.GetPropertyName(property);
                object total;

                switch (property)
                {
                    case PartProperties.KEYS.Damage when value is float f:
                        total = f * multipliers[PATCH_TYPE.POWER];
                        break;
                    case PartProperties.KEYS.Reduction when value is float f:
                        total = $"{f * multipliers[PATCH_TYPE.POWER]:P0}";
                        propertyName = "Damage Reduction";
                        break;
                    case PartProperties.KEYS.Cooldown when value is float f:
                        total = f * multipliers[PATCH_TYPE.FIRE_RATE];
                        break;
                    case PartProperties.KEYS.Radius when value is int r:
                        total = r * multipliers[PATCH_TYPE.RANGE];
                        break;
                    case PartProperties.KEYS.Projectile when value is string s:
                        propertyName = "Range";
                        var projectileRange = FactoryManager.Instance.ProjectileProfile
                            .GetProjectileProfileData(s).ProjectileRange;
                        total = projectileRange * multipliers[PATCH_TYPE.RANGE];
                        break;
                    
                    case PartProperties.KEYS.Health when partData.Type == (int)PART_TYPE.CORE:
                        propertyName = "Hull Strength";
                        total = value;
                        break;
                    case PartProperties.KEYS.Health when value is float f:
                        propertyName = "Hull Strength";
                        total = f * multipliers[PATCH_TYPE.POWER];
                        break;
                    case PartProperties.KEYS.Magnet:
                        propertyName = "Magnetism";
                        total = value;
                        break;
                    case PartProperties.KEYS.Speed:
                    case PartProperties.KEYS.Heal:
                    case PartProperties.KEYS.Health:
                    case PartProperties.KEYS.Capacity:
                        total = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(property), property, null);
                }

                var partDetail = new PartDetail(propertyName, total, null);
                outList.Add(partDetail);
            }

            return string.Join("\n", outList.Select(x => x.ToString()));
        }


        public static string GetPartDetailsPatchPreview(this PartData partData, in PatchData patchToPreview)
        {
            var partRemoteData = ((PART_TYPE) partData.Type).GetRemoteData();
            return partData.GetPartDetailsPatchPreview(partRemoteData, patchToPreview);
        }
        
        public static string GetPartDetailsPatchPreview(this PartData partData, in PartRemoteData partRemoteData, in PatchData patchToPreview)
        {

            //--------------------------------------------------------------------------------------------------------//
            
            object GetPartDetailInfo(in float value, 
                in PATCH_TYPE patchType, 
                in PATCH_TYPE previewPatchType, 
                IReadOnlyDictionary<PATCH_TYPE, float> previewPatchMultipliers)
            {
                object previewValue = default;

                if (previewPatchType == patchType)
                    previewValue = value * previewPatchMultipliers[patchType];

                return previewValue;
            }

            //--------------------------------------------------------------------------------------------------------//
            
            var typeToPreview = (PATCH_TYPE)patchToPreview.Type;
            var previewPatches = new List<PatchData>(partData.Patches)
            {
                patchToPreview
            };


            //--------------------------------------------------------------------------------------------------------//
            
            var multipliers = partData.Patches.GetPatchMultipliers(
                PATCH_TYPE.POWER,
                PATCH_TYPE.RANGE,
                PATCH_TYPE.FIRE_RATE,
                PATCH_TYPE.EFFICIENCY);
            
            var previewMultipliers = previewPatches.GetPatchMultipliers(
                PATCH_TYPE.POWER,
                PATCH_TYPE.RANGE,
                PATCH_TYPE.FIRE_RATE,
                PATCH_TYPE.EFFICIENCY);

            //var partRemote = partRemoteData.partType.GetRemoteData();

            var partProperties = new []
            {
                PartProperties.KEYS.Damage,
                PartProperties.KEYS.Cooldown,
                PartProperties.KEYS.Radius,
                PartProperties.KEYS.Projectile,
                PartProperties.KEYS.Speed,
                PartProperties.KEYS.Heal,
                PartProperties.KEYS.Health,
                PartProperties.KEYS.Capacity,
                PartProperties.KEYS.Magnet,
                PartProperties.KEYS.Reduction
            };
            
            var outList = new List<PartDetail>();

            //If the part uses ammo we'll check that first
            if (partRemoteData.ammoUseCost > 0 /*&& partData.Type != (int) PART_TYPE.CORE*/)
            {
                var preview = GetPartDetailInfo(partRemoteData.ammoUseCost,
                    PATCH_TYPE.EFFICIENCY,
                    typeToPreview,
                    previewMultipliers);
                
                if (preview is float value)
                    preview = value * PlayerDataManager.GetCurrentUpgradeValue(UPGRADE_TYPE.CATEGORY_EFFICIENCY, partRemoteData.category);

                var total = partRemoteData.ammoUseCost * multipliers[PATCH_TYPE.EFFICIENCY] * PlayerDataManager.GetCurrentUpgradeValue(UPGRADE_TYPE.CATEGORY_EFFICIENCY, partRemoteData.category);
                var partDetail = new PartDetail("Ammo", total, preview);
                outList.Add(partDetail);
                
            }//outList.Add(new PartDetail("Ammo", partRemote.ammoUseCost * multipliers[PATCH_TYPE.EFFICIENCY]));
            
            foreach (var property in partProperties)
            {
                if (!partRemoteData.TryGetValue(property, out var value))
                    continue;

                var propertyName = PartProperties.GetPropertyName(property);
                object total;
                object preview = default;

                switch (property)
                {
                    case PartProperties.KEYS.Damage when value is float f:
                        total = f * multipliers[PATCH_TYPE.POWER];
                        preview = GetPartDetailInfo(f, PATCH_TYPE.POWER, typeToPreview, previewMultipliers);
                        break;
                    case PartProperties.KEYS.Reduction when value is float f:
                        propertyName = "Damage Reduction";
                        total = $"{f * multipliers[PATCH_TYPE.POWER] :P0}";
                        preview = GetPartDetailInfo(f, PATCH_TYPE.POWER, typeToPreview, previewMultipliers);
                        if(preview != null) preview = $"{preview:P0}";
                        break;
                    case PartProperties.KEYS.Cooldown when value is float f:
                        total = f * multipliers[PATCH_TYPE.FIRE_RATE];
                        preview = GetPartDetailInfo(f, PATCH_TYPE.FIRE_RATE, typeToPreview, previewMultipliers);
                        break;
                    case PartProperties.KEYS.Radius when value is int r:
                        total = r * multipliers[PATCH_TYPE.RANGE];
                        preview = GetPartDetailInfo(r, PATCH_TYPE.RANGE, typeToPreview, previewMultipliers);
                        break;
                    case PartProperties.KEYS.Projectile when value is string s:
                        propertyName = "Range";
                        var projectileRange = FactoryManager.Instance.ProjectileProfile
                            .GetProjectileProfileData(s).ProjectileRange;
                        total = projectileRange * multipliers[PATCH_TYPE.RANGE];
                        preview = GetPartDetailInfo(projectileRange, PATCH_TYPE.RANGE, typeToPreview, previewMultipliers);
                        break;
                    case PartProperties.KEYS.Health when partData.Type == (int)PART_TYPE.CORE:
                        propertyName = "Hull Strength";
                        total = value;
                        break;
                    case PartProperties.KEYS.Health when value is float f:
                        propertyName = "Hull Strength";
                        total = f * multipliers[PATCH_TYPE.POWER];
                        preview = GetPartDetailInfo(f, PATCH_TYPE.POWER, typeToPreview, previewMultipliers);
                        break;
                    case PartProperties.KEYS.Magnet:
                        propertyName = "Magnetism";
                        total = value;
                        break;
                    case PartProperties.KEYS.Speed:
                    case PartProperties.KEYS.Heal:
                    case PartProperties.KEYS.Health:
                    case PartProperties.KEYS.Capacity:
                        total = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(property), property, null);
                }

                var partDetail = new PartDetail(propertyName, total, preview);
                outList.Add(partDetail);
            }

            return string.Join("\n", outList.Select(x => x.ToString()));

            /*var multipliers = partData.Patches.GetPatchMultipliers(
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

            return string.Join("\n", outList.Select(x => x.ToString()));*/
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
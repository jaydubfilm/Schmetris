using System;
using StarSalvager.Factories;
using StarSalvager.PersistentUpgrades.Data;
using StarSalvager.Utilities.Helpers;

namespace StarSalvager.Utilities.Extensions
{
    public static class UpgradeDataExtensions
    {
        public static string GetUpgradeTitleText(this UpgradeData upgradeData)
        {
            var remoteData = FactoryManager.Instance.PersistentUpgrades
                .GetRemoteData(upgradeData.Type, upgradeData.BitType);

            switch (upgradeData.Type)
            {
                case UPGRADE_TYPE.PATCH_COST:
                case UPGRADE_TYPE.AMMO_CAPACITY:
                case UPGRADE_TYPE.GEAR_DROP:
                case UPGRADE_TYPE.STARTING_CURRENCY:
                case UPGRADE_TYPE.CATEGORY_EFFICIENCY:
                    return $"{remoteData.name} - Level {upgradeData.Level}";
                //return $"{upgradeData.BitType.GetRemoteData().name} {remoteData.name} - Level {upgradeData.Level}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public static string GetUpgradeDetailText(this UpgradeData upgradeData)
        {
            var remoteData = FactoryManager.Instance.PersistentUpgrades
                    .GetRemoteData(upgradeData.Type, upgradeData.BitType);

            string displayValue;
            var defaultValue = remoteData.Levels[0].value;
            var currentValue = remoteData.Levels[upgradeData.Level].value;

            switch (upgradeData.Type)
            {
                case UPGRADE_TYPE.PATCH_COST:
                    displayValue = $"Decrease patch cost by {defaultValue - currentValue:P0}";
                    break;
                case UPGRADE_TYPE.AMMO_CAPACITY:
                    displayValue = $"Increase ammo capacity to {currentValue}";
                    break;
                case UPGRADE_TYPE.GEAR_DROP:
                    displayValue = $"Increase Gear Drops by {currentValue- defaultValue:P0}";
                    break;
                case UPGRADE_TYPE.STARTING_CURRENCY:
                    displayValue = $"Start with {currentValue}{TMP_SpriteHelper.GEAR_ICON}";
                    break;
                case UPGRADE_TYPE.CATEGORY_EFFICIENCY:
                    displayValue = $"Decrease ammo cost by {defaultValue - currentValue:P0} for {upgradeData.BitType.GetRemoteData().name} parts";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return $"{displayValue}";
        }
    }
}

using System.Collections.Generic;
using StarSalvager.Factories;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.UI;

namespace StarSalvager.Utilities.Extensions
{
    public static class RDSLootDataListExtentions
    {
        public static string AsDisplayString(this List<RDSLootData> lootDatas)
        {
            if (lootDatas.IsNullOrEmpty())
                return string.Empty;
            
            RemotePartProfileScriptableObject partRemoteData = null;
            FacilityRemoteDataScriptableObject facilityRemoteData = null;
            ComponentRemoteDataScriptableObject componentRemoteData = null;
            
            var _outString = new List<string>
            {
                "<b>Rewards</b>"
            };

            foreach (var lootData in lootDatas)
            {
                switch (lootData.rdsData)
                {
                    case RDSLootData.TYPE.Bit:
                        var bitIcon = TMP_SpriteMap.GetBitSprite((BIT_TYPE) lootData.type, lootData.level);
                        _outString.Add($"{lootData.amount} {bitIcon}");
                        break;
                    case RDSLootData.TYPE.ResourcesRefined:
                        var refinedIcon = TMP_SpriteMap.MaterialIcons[(BIT_TYPE) lootData.type];
                        _outString.Add($"{lootData.amount} {refinedIcon}");
                        break;
                    case RDSLootData.TYPE.Component:
                        if (componentRemoteData)
                            componentRemoteData = FactoryManager.Instance.componentRemoteData;
                        
                        var componentType = (COMPONENT_TYPE)lootData.type;
                        var componentData = componentRemoteData.GetRemoteData(componentType);
                        
                        _outString.Add($"Blueprint: {componentData.name}");
                        break;
                    case RDSLootData.TYPE.Blueprint:
                        if (partRemoteData == null)
                            partRemoteData = FactoryManager.Instance.PartsRemoteData;
                        
                        var partType = (PART_TYPE)lootData.type;
                        var partData = partRemoteData.GetRemoteData(partType);
                        
                        _outString.Add($"Blueprint: {partData.name} lvl{lootData.level + 1}");
                        break;
                    case RDSLootData.TYPE.FacilityBlueprint:
                        //_outString.Add($"Facility Blueprint: {lootData.blue}");
                        
                        if (facilityRemoteData == null)
                            facilityRemoteData = FactoryManager.Instance.FacilityRemote;
                        
                        var facilityType = (FACILITY_TYPE)lootData.type;
                        var facilityData = facilityRemoteData.GetRemoteData(facilityType);
                        
                        _outString.Add($"Blueprint: {facilityData.displayName} lvl{lootData.level + 1}");
                        break;
                    case RDSLootData.TYPE.Gears:
                        _outString.Add($"Gears: {lootData.GearValue}");
                        break;
                    default:
                        continue;
                }
            }

            return string.Join("\n\t", _outString);
        }
    }
}
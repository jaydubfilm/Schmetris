using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.UI.Scrapyard;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using System.Linq;
using UnityEngine;

namespace StarSalvager
{
    public class CraftingBench : MonoBehaviour
    {
        [SerializeField, Required]
        private CraftingBenchUI storageUI;

        [SerializeField, Required]
        private DroneDesigner mDroneDesigner;

        public void CraftBlueprint(Blueprint blueprint)
        {
            if (!Globals.TestingFeatures && (!PlayerDataManager.CanAffordPart(blueprint.partType, blueprint.level)
                || blueprint.partType == PART_TYPE.CORE && mDroneDesigner._scrapyardBot.AttachedBlocks.GetBlockDatas().All(p => p.Type != (int) PART_TYPE.CORE)))
            {
                if (!Toast.Instance.showingToast)
                    Toast.AddToast("Not enough resources to craft");
                return;
            }

            IBlockData blockData = new PartData
            {
                Type = (int)blueprint.partType,
            };

            if (!Globals.TestingFeatures)
            {
                PlayerDataManager.SubtractPartCosts(blueprint.partType, blueprint.level, false);
            }

            if (blueprint.partType == PART_TYPE.CORE)
            {
                ScrapyardPart core = mDroneDesigner._scrapyardBot.AttachedBlocks.First(p => p.Coordinate == Vector2Int.zero) as ScrapyardPart;
                FactoryManager.Instance.GetFactory<PartAttachableFactory>().UpdatePartData(blueprint.partType, blueprint.level,
                    ref core);
            }
            else
                PlayerDataManager.AddPartToStorage(blockData);
        }
    }
}

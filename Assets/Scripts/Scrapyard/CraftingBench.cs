using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Missions;
using StarSalvager.UI.Scrapyard;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
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

        public void CraftBlueprint(TEST_Blueprint blueprint)
        {
            if (!PlayerPersistentData.PlayerData.CanAffordPart(blueprint.partType, blueprint.level, false)
                || blueprint.partType == PART_TYPE.CORE && !mDroneDesigner._scrapyardBot.attachedBlocks.GetBlockDatas().Any(p => p.Type == (int)PART_TYPE.CORE && p.Level == blueprint.level - 1))
            {
                if (!Toast.Instance.showingToast)
                    Toast.AddToast("Not enough resources to craft", time: 1.0f, verticalLayout: Toast.Layout.Start, horizontalLayout: Toast.Layout.Middle);
                return;
            }
            
            if (PlayerPersistentData.PlayerData.resources[BIT_TYPE.YELLOW] == 0)
            {
                Toast.AddToast("Not enough power to craft", time: 1.0f, verticalLayout: Toast.Layout.Start, horizontalLayout: Toast.Layout.Middle);
                return;
            }

            BlockData blockData = new BlockData
            {
                ClassType = nameof(Part),
                Type = (int)blueprint.partType,
                Level = blueprint.level
            };

            PlayerPersistentData.PlayerData.SubtractPartCosts(blueprint.partType, blueprint.level, false);
            MissionManager.ProcessCraftPartMissionData(blueprint.partType, blueprint.level);

            if (blueprint.partType == PART_TYPE.CORE)
            {
                ScrapyardPart core = mDroneDesigner._scrapyardBot.attachedBlocks.First(p => p.Coordinate == Vector2Int.zero) as ScrapyardPart;
                FactoryManager.Instance.GetFactory<PartAttachableFactory>().UpdatePartData(blueprint.partType, blueprint.level,
                    ref core);
            }
            else
                PlayerPersistentData.PlayerData.AddPartToStorage(blockData);
        }
    }
}

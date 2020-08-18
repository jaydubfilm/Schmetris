using Sirenix.OdinInspector;
using StarSalvager.Missions;
using StarSalvager.UI.Scrapyard;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager
{
    public class CraftingBench : MonoBehaviour
    {
        [SerializeField, Required]
        private CraftingBenchUI storageUI;

        public void CraftBlueprint(TEST_Blueprint blueprint)
        {
            if (!PlayerPersistentData.PlayerData.CanAffordPart(blueprint.partType, blueprint.level, false))
                return;

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
            PlayerPersistentData.PlayerData.AddPartToStorage(blockData);
            MissionManager.ProcessCraftPartMissionData(blueprint.partType, blueprint.level);
        }
    }
}

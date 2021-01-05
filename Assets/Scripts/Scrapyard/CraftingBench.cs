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
            BlockData blockData = new BlockData
            {
                ClassType = nameof(Part),
                Type = (int)blueprint.partType
            };
            PlayerDataManager.AddPartToStorage(blockData);
        }
    }
}

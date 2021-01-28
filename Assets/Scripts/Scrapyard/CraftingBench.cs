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
        public void CraftBlueprint(Blueprint blueprint)
        {
            PartData blockData = new PartData
            {
                Type = (int)blueprint.partType
            };
            PlayerDataManager.AddPartToStorage(blockData);
        }
    }
}

﻿using Sirenix.OdinInspector;
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

            BlockData blockData = new BlockData
            {
                ClassType = nameof(Part),
                Type = (int)blueprint.remoteData.partType,
                Level = blueprint.level
            };

            PlayerPersistentData.PlayerData.SubtractPartCosts(blueprint.partType, blueprint.level, false);
            PlayerPersistentData.PlayerData.AddPartToStorage(blockData);
        }
    }
}

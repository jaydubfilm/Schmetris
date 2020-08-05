﻿using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.UI.Scrapyard;
using StarSalvager.Utilities;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarSalvager.Values
{
    public class PlayerData
    {
        //TODO: Add an add/subtract function for ResourceAmount
        
        public Dictionary<BIT_TYPE, int> resources = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 1250},
            {BIT_TYPE.BLUE, 1250},
            {BIT_TYPE.YELLOW, 1250},
            {BIT_TYPE.GREEN, 1250},
            {BIT_TYPE.GREY, 1250},
        };

        public Dictionary<COMPONENT_TYPE, int> components = new Dictionary<COMPONENT_TYPE, int>
        {
            {COMPONENT_TYPE.CALLIT, 3},
            {COMPONENT_TYPE.DOHICKEY, 3},
            {COMPONENT_TYPE.GADGET, 3},
            {COMPONENT_TYPE.GIZMO, 3},
            {COMPONENT_TYPE.THINGY, 3}
        };
        
        //FIXME This needs to use some sort of capacity value
        public Dictionary<BIT_TYPE, float> liquidResource = new Dictionary<BIT_TYPE, float>
        {
            {BIT_TYPE.RED, 250},
            {BIT_TYPE.BLUE, 0},
            {BIT_TYPE.YELLOW, 0},
            {BIT_TYPE.GREEN, 0},
            {BIT_TYPE.GREY, 0},
        };

        public List<BlockData> currentBlockData = new List<BlockData>();
        public List<BlockData> partsInStorageBlockData = new List<BlockData>();

        public List<TEST_Blueprint> unlockedBlueprints = new List<TEST_Blueprint>();

        public Dictionary<int, int> maxSectorProgression = new Dictionary<int, int>();

        //============================================================================================================//

        public Dictionary<BIT_TYPE, int> GetResources()
        {
            return resources;
        }

        public Dictionary<COMPONENT_TYPE, int> GetComponents()
        {
            return components;
        }

        public void AddResources(Dictionary<BIT_TYPE, int> toAdd)
        {
            CostCalculations.AddResources(ref resources, toAdd);
        }

        public void AddResources(PART_TYPE partType, int level, bool isRecursive)
        {
            CostCalculations.AddResources(ref resources, partType, level, isRecursive);
        }

        public void SubtractResources(Dictionary<BIT_TYPE, int> toSubtract)
        {
            CostCalculations.SubtractResources(ref resources, toSubtract);
        }

        public void SubtractResources(PART_TYPE partType, int level, bool isRecursive)
        {
            CostCalculations.SubtractResources(ref resources, partType, level, isRecursive);
        }

        public void SubtractResources(IEnumerable<CraftCost> cost)
        {
            CostCalculations.SubtractResources(ref resources, cost);
        }

        public void SubtractPartCosts(PART_TYPE partType, int level, bool isRecursive, float costModifier = 1.0f)
        {
            CostCalculations.SubtractPartCosts(ref resources, ref components, partsInStorageBlockData, partType, level, isRecursive, costModifier);
        }

        public bool CanAffordCost(IEnumerable<CraftCost> levelCost)
        {
            Dictionary<BIT_TYPE, int> tempDictionary = new Dictionary<BIT_TYPE, int>(resources);
            return CostCalculations.CanAffordResources(tempDictionary, levelCost);
        }

        public bool CanAffordPart(PART_TYPE partType, int level, bool isRecursive)
        {
            Dictionary<BIT_TYPE, int> tempResourceDictionary = new Dictionary<BIT_TYPE, int>(resources);
            Dictionary<COMPONENT_TYPE, int> tempComponentDictionary = new Dictionary<COMPONENT_TYPE, int>(components);
            List<BlockData> tempPartsInStorage = new List<BlockData>(partsInStorageBlockData);
            return CostCalculations.CanAffordPart(tempResourceDictionary, tempComponentDictionary, tempPartsInStorage, partType, level, isRecursive);
        }

        //============================================================================================================//

        public void AddSectorProgression(int sector, int waveAt)
        {
            if (maxSectorProgression.ContainsKey(sector))
                maxSectorProgression[sector] = Mathf.Max(maxSectorProgression[sector], waveAt);
            else
                maxSectorProgression.Add(sector, waveAt);
        }

        public bool CheckIfQualifies(int sector, int waveAt)
        {
            if (maxSectorProgression.ContainsKey(sector) && maxSectorProgression[sector] >= waveAt)
                return true;

            return false;
        }

        //============================================================================================================//

        public List<BlockData> GetCurrentBlockData()
        {
            return currentBlockData;
        }

        public void SetCurrentBlockData(List<BlockData> blockData)
        {
            currentBlockData.Clear();
            currentBlockData.AddRange(blockData);
        }

        public List<BlockData> GetCurrentPartsInStorage()
        {
            return partsInStorageBlockData;
        }

        public void SetCurrentPartsInStorage(List<BlockData> blockData)
        {
            partsInStorageBlockData.Clear();
            partsInStorageBlockData.AddRange(blockData);
        }

        public void AddPartToStorage(BlockData blockData)
        {
            partsInStorageBlockData.Add(blockData);
        }

        public void RemovePartFromStorage(BlockData blockData)
        {
            partsInStorageBlockData.Remove(partsInStorageBlockData.FirstOrDefault(b => b.Level == blockData.Level && b.Type == blockData.Type));
        }
    }
}
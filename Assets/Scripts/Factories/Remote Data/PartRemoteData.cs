using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public class PartRemoteData
    {
        [FoldoutGroup("$name")]
        public string name;
        
        [FoldoutGroup("$name")]
        public PART_TYPE partType;

        [FoldoutGroup("$name"), ListDrawerSettings(ShowIndexLabels = true)]
        public float[] health;

        [FoldoutGroup("$name")]
        public List<LevelCost> costs;

        [FoldoutGroup("$name"), ListDrawerSettings(ShowIndexLabels = true)]
        public int[] data;
    }

    [Serializable]
    public class LevelCost
    {
        public List<Cost> levelCosts;
    }

    [Serializable]
    public struct Cost
    {
        public BIT_TYPE type;
        public int amount;
    }
}



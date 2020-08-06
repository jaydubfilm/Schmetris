using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

//FIXME This should be under a more specific namespace
namespace StarSalvager
{
    [Serializable]
    public struct PartLevelData
    {
        #if UNITY_EDITOR
        public string Name =>
            $"Health: {health} - Data: {data} - Burn Rate: {(burnRate == 0f ? "None" : $"{burnRate}/s")} - lvl req: {unlockLevel}";
        #endif

        public int unlockLevel;
        
        public float health;

        public int data;

        [SuffixLabel("/sec", true)]
        public float burnRate;
        
        public List<CraftCost> cost;
    }
}

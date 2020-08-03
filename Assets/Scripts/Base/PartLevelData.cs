using System;
using System.Collections.Generic;
using StarSalvager.Factories.Data;

//FIXME This should be under a more specific namespace
namespace StarSalvager
{
    [Serializable]
    public struct PartLevelData
    {
        #if UNITY_EDITOR
        public string Name =>
            $"Health: {health} - Data: {data} - Burn Type: {(burnRate.type == BIT_TYPE.BLACK ? "None" : burnRate.type.ToString())}";
        #endif
        
        public float health;

        public int data;
        public ResourceAmount burnRate;
        
        public List<CraftCost> cost;
    }
}

using System;

namespace StarSalvager
{
    [Serializable]
    public struct BitLevelData
    {
        #if UNITY_EDITOR
        public string Name =>
            $"Health: {health} - Resources: {resources}";
        #endif

        public float health;

        public int resources;
    }
}
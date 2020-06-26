using System;
using Sirenix.OdinInspector;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class BitRemoteData
    {
        [FoldoutGroup("$name")]
        public string name;
        [FoldoutGroup("$name")]
        public BIT_TYPE bitType;
        [FoldoutGroup("$name")]
        public float[] health;
        [FoldoutGroup("$name")]
        public int[] resource;
    }
}


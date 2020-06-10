using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct BitProfile: IProfile
    {
        public int Type => (int) bitType;
        [FoldoutGroup("$Name")]
        public BIT_TYPE bitType;
        [ShowInInspector, FoldoutGroup("$Name")]
        public string Name { get; set; }
        [ShowInInspector, FoldoutGroup("$Name")]
        public Sprite[] Sprites { get; set; }
    }

}
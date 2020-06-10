using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct PartProfile: IProfile
    {
        public int Type => (int) partType;
        [FoldoutGroup("$Name")]
        public PART_TYPE partType;
        
        [ShowInInspector, FoldoutGroup("$Name")]
        public string Name { get; set; }
        [ShowInInspector, FoldoutGroup("$Name")]
        public Sprite[] Sprites { get; set; }
    }
}
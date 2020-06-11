using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [System.Serializable]
    public struct PartProfile : IProfile
    {
        public int Type => (int) partType;

        [SerializeField, FoldoutGroup("$Name")]
        public PART_TYPE partType;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        [SerializeField, FoldoutGroup("$Name")]
        private string _name;


        public Sprite[] Sprites
        {
            get => _sprites;
            set => _sprites = value;
        }

        [SerializeField, FoldoutGroup("$Name")]
        private Sprite[] _sprites;
    }
}
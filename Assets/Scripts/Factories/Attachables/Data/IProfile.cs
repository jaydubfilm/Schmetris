using StarSalvager.Utilities.Animations;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    public interface IProfile
    {
        string Name { get; set; }
        int Type { get; }
        Sprite[] Sprites { get; set; }

        AnimationScriptableObject animation { get; set; }
    }
}

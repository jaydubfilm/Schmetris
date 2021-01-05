using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class IProfileExtensions
    {
        public static Sprite GetSprite(this IProfile profile, int level)
        {
            return level >= profile.Sprites.Length ? profile.Sprites[0] : profile.Sprites[level];
        }
        
        public static Sprite GetRandomSprite(this IProfile profile)
        {
            return profile.Sprites[Random.Range(0, profile.Sprites.Length)];
        }

        public static Sprite GetSprite(this PartProfile profile)
        {
            return profile.Sprite;
        }
    }
}


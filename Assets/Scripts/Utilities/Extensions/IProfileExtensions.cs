using System.Collections;
using System.Collections.Generic;
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
    }
}


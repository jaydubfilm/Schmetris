using System;
using System.Collections;
using System.Collections.Generic;
using StarSalvager.PatchTrees.Data;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class PatchNodeJsonIEnumerableExtensions
    {
        public static bool HasUnlockedPatch(this IEnumerable<PatchNodeJson> patchTree, in PartData currentPart, in PatchData patchToCheck)
        {
            throw new NotImplementedException();
        }
    }
}

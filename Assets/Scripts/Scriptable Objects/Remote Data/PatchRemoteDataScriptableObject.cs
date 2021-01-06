using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Patch Remote", menuName = "Star Salvager/Scriptable Objects/Patch Remote Data")]
    public class PatchRemoteDataScriptableObject : ScriptableObject
    {
        public List<PatchRemoteData> patchRemoteData = new List<PatchRemoteData>();

        public PatchRemoteData GetRemoteData(PATCH_TYPE Type)
        {
            return patchRemoteData
                .FirstOrDefault(p => p.type == Type);
        }
    }
}

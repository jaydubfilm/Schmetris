using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Bit Remote", menuName = "Star Salvager/Scriptable Objects/Bit Remote Data")]
    public class BitRemoteDataScriptableObject : ScriptableObject
    {
        public List<BitRemoteData> BitRemoteData = new List<BitRemoteData>();

        public BitRemoteData GetRemoteData(BIT_TYPE Type)
        {
            return BitRemoteData
                .FirstOrDefault(p => p.bitType == Type);
        }
    }
}


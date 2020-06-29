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

        private Dictionary<BIT_TYPE, BitRemoteData> data;

        public BitRemoteData GetRemoteData(BIT_TYPE Type)
        {
            if (data == null)
            {
                data = new Dictionary<BIT_TYPE, BitRemoteData>();
            }

            if (!data.ContainsKey(Type))
            {
                data.Add(Type,BitRemoteData
                        .FirstOrDefault(p => p.bitType == Type));
            }

            return data[Type];
        }
    }
}


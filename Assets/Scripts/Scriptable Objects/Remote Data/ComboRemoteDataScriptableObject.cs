using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Puzzle.Data;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Combo Remote", menuName = "Star Salvager/Scriptable Objects/Combo Remote Data")]
    public class ComboRemoteDataScriptableObject : ScriptableObject
    {
        public List<ComboRemoteData> BitRemoteData = new List<ComboRemoteData>();

        private Dictionary<COMBO, ComboRemoteData> data;

        public ComboRemoteData GetRemoteData(COMBO Type)
        {
            if (data == null)
            {
                data = new Dictionary<COMBO, ComboRemoteData>();
            }

            if (!data.ContainsKey(Type))
            {
                data.Add(Type,BitRemoteData
                    .FirstOrDefault(p => p.type == Type));
            }

            return data[Type];
        }
    }
}

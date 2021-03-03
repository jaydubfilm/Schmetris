using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Puzzle.Data;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Combo Remote", menuName = "Star Salvager/Scriptable Objects/Combo Remote Data")]
    public class ComboRemoteDataScriptableObject : ScriptableObject
    {
        public List<ComboRemoteData> BitRemoteData = new List<ComboRemoteData>();
        
        [TableList]
        public List<SimultaneousComboData> SimultaneousComboData = new List<SimultaneousComboData>();
        [TableList]
        public List<ComboAmmo> ComboAmmos;

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

        public float GetGearMultiplier(int combos, int bits)
        {
            if (bits < 3)
                return 0f;
            
            if (combos < 2)
                return 1f;

            var maxCombo = SimultaneousComboData.Select(x => x.combos).Max();
            var maxBit = SimultaneousComboData.Select(x => x.bits).Max();

            combos = Mathf.Clamp(combos, 2, maxCombo + 1);
            bits =  Mathf.Clamp(bits, 3, maxBit + 1);

            return SimultaneousComboData.FirstOrDefault(x => x.combos == combos && x.bits == bits).multiplier;

        }
    }

    [Serializable]
    public struct SimultaneousComboData
    {
        [TableColumnWidth(45)]
        public int combos;
        [TableColumnWidth(45)]
        public int bits;
        public float multiplier;
    }

    [Serializable]
    public struct ComboAmmo
    {
        [TableColumnWidth(45)]
        public int level;
        [TableColumnWidth(45)]
        public int ammoEarned;
    }
}


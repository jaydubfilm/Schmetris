using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Parts.Data;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class PatchRemoteData
    {
        [Serializable]
        public struct Data
        {
            public string name => $"Level {level}";

            [FoldoutGroup("$name")]
            public int level;
            [FoldoutGroup("$name")]
            public int cost;
            [FoldoutGroup("$name")]
            public PartProperties[] Properties;
        }
        
        [FoldoutGroup("$name")] public string name;

        [FoldoutGroup("$name")] public PATCH_TYPE type;

        [TextArea, FoldoutGroup("$name")] public string description;

        [FoldoutGroup("$name")]
        public List<Data> Levels;

        [FoldoutGroup("$name")] public bool fitsAnyPart;
        [FoldoutGroup("$name"), DisableIf("fitsAnyPart")] 
        public PART_TYPE[] allowedParts;

        //====================================================================================================================//
        
        public T GetDataValue<T>(in int level, PartProperties.KEYS key)
        {
            var keyString = PartProperties.Names[(int)key];
            var dataValue = Levels[level].Properties.FirstOrDefault(d => d.key.Equals(keyString));

            if (dataValue.Equals(null))
                return default;

            if (!(dataValue.GetValue() is T i))
                return default;

            return i;
        }

        public bool TryGetValue<T>(in int level, PartProperties.KEYS key, out T value)
        {
            value = default;

            var keyString = PartProperties.Names[(int)key];
            var dataValue = Levels[level].Properties.FirstOrDefault(d => d.key.Equals(keyString));

            if (dataValue.Equals(null))
                return false;

            if (!(dataValue.GetValue() is T i))
                return false;

            value = i;

            return true;
        }

        //====================================================================================================================//
        
    }
}

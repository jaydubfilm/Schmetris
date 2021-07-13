using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Puzzle.Data;
using StarSalvager.Utilities.Puzzle.Structs;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Utilities.Analytics.SessionTracking.Data
{
    [Serializable]
    public struct BitSummaryData
    {
        //TODO Need to get the sprite image here
        [HideInInspector, HideInTables] public BitData bitData;

        [DisplayAsString] public int collected;
        [DisplayAsString] public int spawned;
        [DisplayAsString] public int disconnected;

#if UNITY_EDITOR

        [JsonIgnore, PropertyOrder(-100), ShowInInspector, PreviewField(ObjectFieldAlignment.Center, Height = 40),
         TableColumnWidth(50, Resizable = false)]
        public Sprite Type => GetSprite();

        private Sprite GetSprite()
        {
            return ((BIT_TYPE)bitData.Type).GetSprite(bitData.Level);
        }


#endif
    }
    
    [Serializable]
    public struct ComboSummaryData
    {
        //TODO Need to get the sprite image here
        [HideInInspector, HideInTables] public BitData bitData;
        [DisplayAsString] public COMBO comboType;
        [DisplayAsString] public int created;
        

        public ComboSummaryData(in ComboRecordData comboRecordData)
        {
            bitData = new BitData
            {
                //If we've created a white bit ensure that we change the recorded data to reflect that
                Type = (int) (comboRecordData.FromLevel + 1 >= 2 ? BIT_TYPE.WHITE : comboRecordData.BitType),
                Level = comboRecordData.FromLevel + 1
            };

            comboType = comboRecordData.ComboType;
            
            created = 1;
        }

#if UNITY_EDITOR

        [JsonIgnore, PropertyOrder(-100), ShowInInspector, PreviewField(ObjectFieldAlignment.Center, Height = 40),
         TableColumnWidth(50, Resizable = false)]
        public Sprite Type => GetSprite();

        private Sprite GetSprite()
        {
            return ((BIT_TYPE)bitData.Type).GetSprite(bitData.Level);
        }


#endif
    }
}
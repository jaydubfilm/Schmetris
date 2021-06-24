using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
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
}
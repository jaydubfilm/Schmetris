using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Utilities.Analytics.Data
{
    [Serializable]
    public struct BitSummaryData
    {
        //TODO Need to get the sprite image here
        [HideInInspector, HideInTables] public BIT_TYPE type;

        [DisplayAsString] public int liquidProcessed;
        [DisplayAsString] public int collected;
        [DisplayAsString] public int diconnected;

#if UNITY_EDITOR

        [JsonIgnore, PropertyOrder(-100), ShowInInspector, PreviewField(ObjectFieldAlignment.Center, Height = 40),
         TableColumnWidth(50, Resizable = false)]
        public Sprite Type => GetSprite();

        private Sprite GetSprite()
        {
            return Object.FindObjectOfType<FactoryManager>().BitProfileData.GetProfile(type).GetSprite(0);
        }


#endif
    }
}
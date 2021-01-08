using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

namespace StarSalvager.Utilities.Analytics.Data
{
    [Serializable]
    public struct ComponentSummaryData
    {
        [DisplayAsString] public int collected;
        [DisplayAsString] public int diconnected;

#if UNITY_EDITOR

        [JsonIgnore, PropertyOrder(-100), ShowInInspector, PreviewField(ObjectFieldAlignment.Center, Height = 40),
         TableColumnWidth(50, Resizable = false)]
        public Sprite Type => GetSprite();

        private Sprite GetSprite()
        {
            return UnityEngine.Object.FindObjectOfType<FactoryManager>().componentSprite;
        }


#endif
    }
}
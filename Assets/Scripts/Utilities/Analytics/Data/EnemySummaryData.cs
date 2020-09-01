using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Utilities.Analytics.Data
{
    [Serializable]
    public struct EnemySummaryData
    {
        [HideInTables] public string id;
        [DisplayAsString] public int killed;

#if UNITY_EDITOR

        [JsonIgnore, PropertyOrder(-100), ShowInInspector, PreviewField(ObjectFieldAlignment.Center, Height = 40),
         TableColumnWidth(50, Resizable = false)]
        public Sprite Type => GetSprite();

        private Sprite GetSprite()
        {
            return Object.FindObjectOfType<FactoryManager>().EnemyProfile.GetEnemyProfileData(id).Sprite;
        }


#endif
    }
}
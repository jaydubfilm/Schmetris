using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects.Procedural;
using UnityEngine;

namespace StarSalvager.ScriptableObjects.Procedural
{
    public abstract class WeightedChanceBase
    {
        [Range(1, 10), HideIf("ShouldHide")] public int weight;

#if UNITY_EDITOR

        [DisplayAsString, TableColumnWidth(75, Resizable = false), HideIf("ShouldHide")]
        public string chance;

        [HideInTables] public float chanceValue;

        protected virtual bool ShouldHide() => false;

#endif
    }

    public abstract class WeightedChanceAssetBase<T> : WeightedChanceBase where T : ScriptableObject
    {
        [OnValueChanged("UpdateName", true), PropertyOrder(-99)]
        public T asset;

#if UNITY_EDITOR
        [PropertyOrder(-100), DisplayAsString] public string name;

        [OnInspectorInit]
        private void UpdateName() => name = asset == null ? string.Empty : asset.name;

        protected override bool ShouldHide()
        {
            return asset == null;
        }

#endif
    }
}

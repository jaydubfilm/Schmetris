using Sirenix.OdinInspector;
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
        protected virtual void UpdateName() => name = asset is IHasName ihn ? ihn.Name : string.Empty;

        protected override bool ShouldHide()
        {
            return asset == null;
        }

#endif
    }
}

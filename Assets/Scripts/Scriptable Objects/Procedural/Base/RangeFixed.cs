using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.ScriptableObjects.Procedural
{
    [Serializable]
    public struct RangeFixed
    {
        private readonly int _min;
        private readonly int _max;

        [LabelWidth(65), HorizontalGroup("Col1", 65)]
        public bool useRange;

        [VerticalGroup("Col1/Row1")] [MinMaxSlider("_min", "_max", true), ShowIf("useRange"), LabelWidth(50), HideLabel]
        public Vector2Int range;

        [VerticalGroup("Col1/Row1")]
        [PropertyRange("_min", "_max"), HideIf("useRange"), OnValueChanged("FixedTimeUpdate"), HideLabel]
        public int @fixed;

        public RangeFixed(in int min, in int max)
        {
            _min = min;
            _max = max;
            useRange = false;
            range = new Vector2Int(min, min);
            @fixed = min;
        }

#if UNITY_EDITOR
        private void FixedTimeUpdate() => range = Vector2Int.one * @fixed;
#endif
    }
}

using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Tutorial.Data
{
    [Serializable]
    public struct TutorialStepData
    {
        [SerializeField, FoldoutGroup("$title", false)]
        public string title;

        [HorizontalGroup("$title/UseWait"), ToggleLeft, LabelWidth(50f)]
        public bool useWaitTime;

        [HorizontalGroup("$title/UseWait"), EnableIf("useWaitTime"), HideLabel, SuffixLabel("Seconds", true)]
        public float waitTime;

        [TextArea, FoldoutGroup("$title")] public string text;
    }
}

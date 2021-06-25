using System;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

namespace StarSalvager.Utilities.Analytics.SessionTracking.Data
{
    [Serializable]
    public readonly struct PartSelectionData
    {
        public static readonly PartSelectionData Empty = new PartSelectionData(PART_TYPE.EMPTY, new PART_TYPE[0]);

        //====================================================================================================================//
        
        [HideInInspector]
        public readonly PART_TYPE Selected;
        [HideInInspector]
        public readonly PART_TYPE[] Options;

        public PartSelectionData(PART_TYPE selected, PART_TYPE[] options)
        {
            Selected = selected;
            Options = options;
        }

        //====================================================================================================================//
        
#if UNITY_EDITOR

        [ShowInInspector, DisplayAsString, HideIf("@Selected == PART_TYPE.EMPTY"), LabelWidth(100), LabelText("Picked:")] 
        public string Picked => Selected.ToString();

        [ShowInInspector, DisplayAsString, HideIf("@Selected == PART_TYPE.EMPTY"), LabelWidth(100), LabelText("Options:")]
        public string SelectionOptions => Options.IsNullOrEmpty() ? string.Empty : string.Join(", ", Options);

#endif

        //====================================================================================================================//
        
    }
}

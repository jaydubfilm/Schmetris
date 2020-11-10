using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Facilities;
using StarSalvager.Utilities.Puzzle.Data;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class FacilityRemoteData
    {
        [FoldoutGroup("$ScriptableHeaderName")]
        public FACILITY_TYPE type;
        [FoldoutGroup("$ScriptableHeaderName")]
        public string displayName;
        [FoldoutGroup("$ScriptableHeaderName")]
        public string displayDescription;
        [FoldoutGroup("$ScriptableHeaderName")]
        public bool hideInFacilityMenu;
        [FoldoutGroup("$ScriptableHeaderName")]
        public List<FacilityLevelData> levels;

        public string ScriptableHeaderName()
        {
            string returnString = displayName;
            if (hideInFacilityMenu)
            {
                returnString += $" HIDDEN IN FACILITY MENU";
            }

            return returnString;
        }
    }
}


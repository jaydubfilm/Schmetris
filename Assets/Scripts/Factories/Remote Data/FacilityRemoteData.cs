using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Puzzle.Data;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class FacilityRemoteData
    {
        [FoldoutGroup("$displayName")]
        public FACILITY_TYPE type;
        [FoldoutGroup("$displayName")]
        public string displayName;
        [FoldoutGroup("$displayName")]
        public string displayDescription;
        [FoldoutGroup("$displayName")]
        public List<FacilityLevelData> levels;
    }
}


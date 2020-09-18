using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Puzzle.Data;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class FacilityRemoteData
    {
        [FoldoutGroup("$type")]
        public FACILITY_TYPE type;
        [FoldoutGroup("$type")]
        public string displayName;
        [FoldoutGroup("$type")]
        public string displayDescription;
        [FoldoutGroup("$type")]
        public List<FacilityLevelData> levels;
    }
}


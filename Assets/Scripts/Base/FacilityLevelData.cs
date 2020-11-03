using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Facilities
{
    [Serializable]
    public struct FacilityLevelData
    {
        public int level;
        public int increaseAmount;

        public int patchCost;

        public List<FacilityPrerequisiteData> facilityPrerequisites;
    }
}

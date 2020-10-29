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
    public struct FacilityPrerequisiteData
    {
        public FACILITY_TYPE facilityType;
        public int level;
    }
}

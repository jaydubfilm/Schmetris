using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

//FIXME This should be under a more specific namespace
namespace StarSalvager
{
    [Serializable]
    public struct FacilityLevelData
    {
        public int level;
        public List<CraftCost> craftCost;
    }
}

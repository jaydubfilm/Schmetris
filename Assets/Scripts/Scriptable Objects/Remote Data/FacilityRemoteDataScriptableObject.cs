using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Puzzle.Data;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Facility Remote", menuName = "Star Salvager/Scriptable Objects/Facility Remote Data")]
    public class FacilityRemoteDataScriptableObject : ScriptableObject
    {
        public List<FacilityRemoteData> FacilityRemoteData = new List<FacilityRemoteData>();

        public FacilityRemoteData GetRemoteData(FACILITY_TYPE Type)
        {
            return FacilityRemoteData.FirstOrDefault(f => f.type == Type);
        }

        public List<FacilityRemoteData> GetRemoteDatas()
        {
            return FacilityRemoteData;
        }
    }
}

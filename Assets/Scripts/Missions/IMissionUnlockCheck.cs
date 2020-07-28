using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Missions
{
    public interface IMissionUnlockCheck
    {
        bool IsComplete { get; }
        bool CheckUnlockParameters();
        MissionUnlockCheckData ToMissionUnlockParameterData();
    }
}
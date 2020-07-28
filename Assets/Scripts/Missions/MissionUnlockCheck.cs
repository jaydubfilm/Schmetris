using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Missions
{
    public abstract class MissionUnlockCheck
    {
        public bool IsComplete = false;
        public abstract bool CheckUnlockParameters();

        public abstract MissionUnlockCheckData ToMissionUnlockParameterData();
    }
}
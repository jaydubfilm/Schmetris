using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public abstract class Mission
    {
        public string MissionName;
        public MISSION_EVENT_TYPE missionType;

        public abstract bool MissionComplete();

        public Mission()
        {

        }

        public abstract void ProcessMissionData(MISSION_EVENT_TYPE type, Dictionary<string, object> data);
    }
}
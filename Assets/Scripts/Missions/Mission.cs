using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public abstract class Mission
    {
        public string MissionName;
        public MISSION_EVENT_TYPE MissionEventType { get; protected set; }

        public Mission()
        {

        }

        public abstract bool MissionComplete();
    }
}
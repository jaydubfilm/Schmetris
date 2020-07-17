using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class SingleEventMission : Mission
    {
        public override bool MissionComplete() => false;

        public SingleEventMission() : base()
        {

        }

        public override void ProcessMissionData(MISSION_EVENT_TYPE type, Dictionary<string, object> data)
        {
            if (missionType != type)
                return;


        }
    }
}
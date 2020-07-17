using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class MultiEventMission : Mission
    {
        public override bool MissionComplete() => false;

        private int m_numEventsNeeded;
        private int m_numEventsCurrent = 0;

        public MultiEventMission(int numEventsNeeded) : base()
        {
            m_numEventsNeeded = numEventsNeeded;
        }

        public override void ProcessMissionData(MISSION_EVENT_TYPE type, Dictionary<string, object> data)
        {
            if (missionType != type)
                return;


        }
    }
}

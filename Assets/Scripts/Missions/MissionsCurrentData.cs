using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class MissionsCurrentData
    {
        private List<Mission> m_missionsCurrentList;

        public MissionsCurrentData()
        {
            m_missionsCurrentList = new List<Mission>();
        }

        public void AddMission(Mission mission)
        {
            m_missionsCurrentList.Add(mission);
        }

        public void AddMission(Mission mission, int atIndex)
        {
            m_missionsCurrentList.Insert(atIndex, mission);
        }

        public void ProcessMissionData(MISSION_EVENT_TYPE type, Dictionary<string, object> data)
        {
            foreach (var mission in m_missionsCurrentList)
            {
                //mission.ProcessMissionData(type, data);
            }
        }
    }
}
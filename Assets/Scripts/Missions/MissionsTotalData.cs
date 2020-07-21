using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StarSalvager
{
    public class MissionsMasterData
    {
        public List<Mission> m_missionsTotalList;

        public MissionsMasterData()
        {
            m_missionsTotalList = new List<Mission>();
        }

        public void AddMission(Mission mission)
        {
            m_missionsTotalList.Add(mission);
        }

        public void AddMission(Mission mission, int atIndex)
        {
            m_missionsTotalList.Insert(atIndex, mission);
        }
    }
}
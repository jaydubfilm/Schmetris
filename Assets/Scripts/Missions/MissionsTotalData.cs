using Newtonsoft.Json;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StarSalvager.Missions
{
    public class MissionsMasterData
    {
        public List<MissionData> m_missionsMasterData;
        
        private List<Mission> m_missionsMaster;

        public MissionsMasterData()
        {
            m_missionsMasterData = new List<MissionData>();
            m_missionsMaster = new List<Mission>();
        }

        public List<Mission> GetMasterMissions()
        {
            return m_missionsMaster;
        }

        public void AddMission(Mission mission)
        {
            m_missionsMaster.Add(mission);
        }

        public void AddMission(Mission mission, int atIndex)
        {
            m_missionsMaster.Insert(atIndex, mission);
        }

        public void LoadMissionData()
        {
            m_missionsMaster = m_missionsMasterData.ImportMissionDatas();
        }

        public void SaveMissionData()
        {
            m_missionsMasterData.Clear();
            foreach (Mission mission in m_missionsMaster)
            {
                m_missionsMasterData.Add(mission.ToMissionData());
            }
        }
    }
}
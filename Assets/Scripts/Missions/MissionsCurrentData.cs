using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Missions
{
    public class MissionsCurrentData
    {
        //TODO: Switch this set of lists into a dictionary with key = type, value = list<mission>
        public List<MissionData> m_notStartedMissionData;
        public List<MissionData> m_currentMissionData;
        public List<MissionData> m_completedMissionData;

        private List<Mission> m_notStartedMissions;
        private List<Mission> m_currentMissions;
        private List<Mission> m_completedMissions;

        public MissionsCurrentData()
        {
            m_notStartedMissionData = new List<MissionData>();
            m_currentMissionData = new List<MissionData>();
            m_completedMissionData = new List<MissionData>();
            m_notStartedMissions = new List<Mission>();
            m_currentMissions = new List<Mission>();
            m_completedMissions = new List<Mission>();
        }

        public List<Mission> GetNotStartedMissions()
        {
            return m_notStartedMissions;
        }

        public List<Mission> GetCurrentMissions()
        {
            return m_currentMissions;
        }

        public List<Mission> GetCompletedMissions()
        {
            return m_completedMissions;
        }

        public void AddMission(Mission mission)
        {
            if (m_notStartedMissions.Find(m => m.m_missionName == mission.m_missionName) != null)
            {
                m_notStartedMissions.RemoveAll(m => m.m_missionName == mission.m_missionName);
                m_currentMissions.Add(mission);
            }
        }

        public void AddMission(Mission mission, int atIndex)
        {
            if (m_notStartedMissions.Find(m => m.m_missionName == mission.m_missionName) != null)
            {
                m_notStartedMissions.RemoveAll(m => m.m_missionName == mission.m_missionName);
                m_currentMissions.Insert(atIndex, mission);
            }
        }

        public void CompleteMission(Mission mission)
        {
            m_completedMissions.Add(mission);
        }

        public void LoadMissionData()
        {
            m_notStartedMissions = m_notStartedMissionData.ImportMissionDatas();
            m_currentMissions = m_currentMissionData.ImportMissionDatas();
            m_completedMissions = m_completedMissionData.ImportMissionDatas();
        }

        public void SaveMissionData()
        {
            m_notStartedMissionData.Clear();
            foreach(Mission mission in m_notStartedMissions)
            {
                m_notStartedMissionData.Add(mission.ToMissionData());
            }

            m_currentMissionData.Clear();
            foreach (Mission mission in m_currentMissions)
            {
                m_currentMissionData.Add(mission.ToMissionData());
            }

            m_completedMissionData.Clear();
            foreach (Mission mission in m_completedMissions)
            {
                m_completedMissionData.Add(mission.ToMissionData());
            }
        }
    }
}
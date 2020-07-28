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
        public List<MissionData> CurrentMissionData;
        public List<MissionData> CompletedMissionData;

        [Newtonsoft.Json.JsonIgnore]
        public List<Mission> NotStartedMissions;
        [Newtonsoft.Json.JsonIgnore]
        public List<Mission> CurrentMissions;
        [Newtonsoft.Json.JsonIgnore]
        public List<Mission> CompletedMissions;

        public MissionsCurrentData()
        {
            m_notStartedMissionData = new List<MissionData>();
            CurrentMissionData = new List<MissionData>();
            CompletedMissionData = new List<MissionData>();
            NotStartedMissions = new List<Mission>();
            CurrentMissions = new List<Mission>();
            CompletedMissions = new List<Mission>();
        }

        public void AddMission(Mission mission)
        {
            if (NotStartedMissions.Find(m => m.m_missionName == mission.m_missionName) != null)
            {
                NotStartedMissions.RemoveAll(m => m.m_missionName == mission.m_missionName);
                CurrentMissions.Add(mission);
            }
        }

        public void AddMission(Mission mission, int atIndex)
        {
            if (NotStartedMissions.Find(m => m.m_missionName == mission.m_missionName) != null)
            {
                NotStartedMissions.RemoveAll(m => m.m_missionName == mission.m_missionName);
                CurrentMissions.Insert(atIndex, mission);
            }
        }

        public void CompleteMission(Mission mission)
        {
            CompletedMissions.Add(mission);
        }

        public void LoadMissionData()
        {
            NotStartedMissions = m_notStartedMissionData.ImportMissionDatas();
            CurrentMissions = CurrentMissionData.ImportMissionDatas();
            CompletedMissions = CompletedMissionData.ImportMissionDatas();
        }

        public void SaveMissionData()
        {
            m_notStartedMissionData.Clear();
            foreach(Mission mission in NotStartedMissions)
            {
                m_notStartedMissionData.Add(mission.ToMissionData());
            }

            CurrentMissionData.Clear();
            foreach (Mission mission in CurrentMissions)
            {
                CurrentMissionData.Add(mission.ToMissionData());
            }

            CompletedMissionData.Clear();
            foreach (Mission mission in CompletedMissions)
            {
                CompletedMissionData.Add(mission.ToMissionData());
            }
        }
    }
}
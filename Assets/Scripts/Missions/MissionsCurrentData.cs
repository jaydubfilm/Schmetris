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
        public List<MissionData> NotStartedMissionData;
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
            NotStartedMissionData = new List<MissionData>();
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

                MissionData missionData = NotStartedMissionData.Find(m => m.MissionName == mission.m_missionName);
                NotStartedMissionData.RemoveAll(m => m.MissionName == mission.m_missionName);
                CurrentMissionData.Add(missionData);
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
            NotStartedMissions = NotStartedMissionData.ImportMissionDatas();
            CurrentMissions = CurrentMissionData.ImportMissionDatas();
            CompletedMissions = CompletedMissionData.ImportMissionDatas();
        }

        public void SaveMissionData()
        {
            NotStartedMissionData.Clear();
            foreach(Mission mission in NotStartedMissions)
            {
                NotStartedMissionData.Add(mission.ToMissionData());
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
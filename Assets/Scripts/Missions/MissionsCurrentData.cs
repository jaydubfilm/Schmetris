using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarSalvager.Missions
{
    public class MissionsCurrentData
    {
        //TODO: Switch this set of lists into a dictionary with key = type, value = list<mission>
        [Newtonsoft.Json.JsonProperty]
        private List<MissionData> NotStartedMissionData;
        [Newtonsoft.Json.JsonProperty]
        private List<MissionData> CurrentMissionData;
        [Newtonsoft.Json.JsonProperty]
        private List<MissionData> CompletedMissionData;
        [Newtonsoft.Json.JsonProperty]
        private List<MissionData> CurrentTrackedMissionData;

        [Newtonsoft.Json.JsonIgnore]
        public List<Mission> NotStartedMissions;
        [Newtonsoft.Json.JsonIgnore]
        public List<Mission> CurrentMissions;
        [Newtonsoft.Json.JsonIgnore]
        public List<Mission> CompletedMissions;
        [Newtonsoft.Json.JsonIgnore]
        public List<Mission> CurrentTrackedMissions;

        public MissionsCurrentData()
        {
            NotStartedMissionData = new List<MissionData>();
            CurrentMissionData = new List<MissionData>();
            CompletedMissionData = new List<MissionData>();
            CurrentTrackedMissionData = new List<MissionData>();
            NotStartedMissions = new List<Mission>();
            CurrentMissions = new List<Mission>();
            CompletedMissions = new List<Mission>();
            CurrentTrackedMissions = new List<Mission>();
        }

        public void AddMission(Mission mission)
        {
            if (NotStartedMissions.Find(m => m.missionName == mission.missionName) != null)
            {
                NotStartedMissions.RemoveAll(m => m.missionName == mission.missionName);
                CurrentMissions.Add(mission);

                MissionData missionData = NotStartedMissionData.Find(m => m.MissionName == mission.missionName);
                NotStartedMissionData.RemoveAll(m => m.MissionName == mission.missionName);
                CurrentMissionData.Add(missionData);

                if (CurrentTrackedMissions.Count < Globals.NumCurrentTrackedMissionMax)
                {
                    AddTrackedMissions(mission);
                }
            }
        }

        public void AddTrackedMissions(Mission mission)
        {
            if (!CurrentTrackedMissions.Any(m => m.missionName == mission.missionName))
            {
                if (CurrentTrackedMissions.Count < Globals.NumCurrentTrackedMissionMax)
                {
                    MissionData missionData = CurrentMissionData.Find(m => m.MissionName == mission.missionName);
                    CurrentTrackedMissions.Add(mission);
                    CurrentTrackedMissionData.Add(missionData);
                }
            }
        }

        public void RemoveTrackedMission(Mission mission)
        {
            if (CurrentTrackedMissions.Any(m => m.missionName == mission.missionName))
            {
                CurrentTrackedMissions.RemoveAll(m => m.missionName == mission.missionName);
                CurrentTrackedMissionData.RemoveAll(m => m.MissionName == mission.missionName);
            }
        }

        public void CompleteMission(Mission mission)
        {
            if (CurrentMissions.Find(m => m.missionName == mission.missionName) != null)
            {
                DropMissionLoot(mission);
                CurrentMissions.RemoveAll(m => m.missionName == mission.missionName);
                CompletedMissions.Add(mission);

                MissionData missionData = CurrentMissionData.Find(m => m.MissionName == mission.missionName);
                CurrentMissionData.RemoveAll(m => m.MissionName == mission.missionName);
                CompletedMissionData.Add(missionData);

                if (CurrentTrackedMissions.Find(m => m.missionName == mission.missionName) != null)
                {
                    CurrentTrackedMissions.RemoveAll(m => m.missionName == mission.missionName);
                    CurrentTrackedMissionData.RemoveAll(m => m.MissionName == mission.missionName);
                }
            }
        }

        private void DropMissionLoot(Mission mission)
        {
            MissionRemoteData missionRemoteData = FactoryManager.Instance.MissionRemoteData.GetRemoteData(mission.missionName);
            PlayerData player = PlayerPersistentData.PlayerData;

            missionRemoteData.ConfigureLootTable();
            List<IRDSObject> missionLoot = missionRemoteData.rdsTable.rdsResult.ToList();
            for (int i = missionLoot.Count - 1; i >= 0; i--)
            {
                if (missionLoot[i] is RDSValue<Blueprint> rdsValueBlueprint)
                {
                    player.UnlockBlueprint(rdsValueBlueprint.rdsValue);
                    Toast.AddToast("Unlocked Blueprint!");
                    missionLoot.RemoveAt(i);
                    continue;
                }
                if (missionLoot[i] is RDSValue<FacilityBlueprint> rdsValueFacilityBlueprint)
                {
                    player.UnlockFacilityBlueprintLevel(rdsValueFacilityBlueprint.rdsValue);
                    Toast.AddToast("Unlocked Facility Blueprint!");
                    missionLoot.RemoveAt(i);
                    continue;
                }
                else if (missionLoot[i] is RDSValue<Vector2Int> rdsValueGears)
                {
                    player.ChangeGears(UnityEngine.Random.Range(rdsValueGears.rdsValue.x, rdsValueGears.rdsValue.y));
                    missionLoot.RemoveAt(i);
                    continue;
                }
                else if (missionLoot[i] is RDSValue<Bit> rdsValueBit)
                {
                    player.AddResource(rdsValueBit.rdsValue.Type, FactoryManager.Instance.BitsRemoteData.GetRemoteData(rdsValueBit.rdsValue.Type).levels[0].resources);
                }
                else if (missionLoot[i] is RDSValue<Component> rdsValueComponent)
                {
                    player.AddComponent(rdsValueComponent.rdsValue.Type, 1);
                }
            }
        }

        public void ResetMissionData()
        {
            foreach (Mission mission in MissionManager.MissionsMasterData.GetMasterMissions())
            {
                NotStartedMissionData.Add(mission.ToMissionData());
            }
        }

        public void LoadMissionData()
        {
            NotStartedMissions = NotStartedMissionData.ImportMissionDatas();
            CurrentMissions = CurrentMissionData.ImportMissionDatas();
            CompletedMissions = CompletedMissionData.ImportMissionDatas();
            CurrentTrackedMissions = CurrentTrackedMissionData.ImportMissionDatas();
        }

        /*public void SaveMissionData()
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

            CurrentTrackedMissionData.Clear();
            foreach (Mission mission in CurrentTrackedMissions)
            {
                CurrentTrackedMissionData.Add(mission.ToMissionData());
            }
        }*/
    }
}
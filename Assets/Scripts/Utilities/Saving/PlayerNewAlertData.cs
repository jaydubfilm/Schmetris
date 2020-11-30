using Newtonsoft.Json;
using StarSalvager.Missions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace StarSalvager.Utilities.Saving
{
    [Serializable]
    public class PlayerNewAlertData
    {
        public List<string> NewMissionNames => _newMissionNames;
        public List<string> NewBlueprintNames => _newBlueprintNames;
        public List<string> NewFacilityBlueprintNames => _newFacilityBlueprintNames;

        [JsonProperty]
        private List<string> _newMissionNames = new List<string>();
        [JsonProperty]
        private List<string> _newBlueprintNames = new List<string>();
        [JsonProperty]
        private List<string> _newFacilityBlueprintNames = new List<string>();

        public bool CheckHasMissionAlert(Mission mission)
        {
            return _newMissionNames.Any(m => m == mission.missionName);
        }

        public bool CheckHasAnyMissionAlerts()
        {
            return _newMissionNames.Count > 0;
        }

        public void AddNewMissionAlert(Mission mission)
        {
            if (!_newMissionNames.Any(m => m == mission.missionName))
            {
                _newMissionNames.Add(mission.missionName);
            }
        }

        public void ClearNewMissionAlert(Mission mission)
        {
            if (_newMissionNames.Any(m => m == mission.missionName))
            {
                _newMissionNames.Remove(mission.missionName);
            }
        }

        public void ClearAllMissionAlerts()
        {
            _newMissionNames.Clear();
        }

        //============================================================================================================//

        public bool CheckHasBlueprintAlert(Blueprint blueprint)
        {
            return _newBlueprintNames.Any(m => m == blueprint.name);
        }

        public bool CheckHasAnyBlueprintAlerts()
        {
            return _newBlueprintNames.Count > 0;
        }

        public void AddNewBlueprintAlert(Blueprint blueprint)
        {
            if (!_newBlueprintNames.Any(m => m == blueprint.name))
            {
                _newBlueprintNames.Add(blueprint.name);
            }
        }

        public void ClearNewBlueprintAlert(Blueprint blueprint)
        {
            if (_newBlueprintNames.Any(m => m == blueprint.name))
            {
                _newBlueprintNames.Remove(blueprint.name);
            }
        }

        public void ClearAllBlueprintAlerts()
        {
            _newBlueprintNames.Clear();
        }

        //============================================================================================================//

        public bool CheckHasFacilityBlueprintAlert(FacilityBlueprint facilityBlueprint)
        {
            return _newFacilityBlueprintNames.Any(m => m == facilityBlueprint.name);
        }

        public bool CheckHasAnyFacilityBlueprintAlerts()
        {
            return _newFacilityBlueprintNames.Count > 0;
        }

        public void AddNewFacilityBlueprintAlert(FacilityBlueprint facilityBlueprint)
        {
            if (!_newFacilityBlueprintNames.Any(m => m == facilityBlueprint.name))
            {
                _newFacilityBlueprintNames.Add(facilityBlueprint.name);
            }
        }

        public void ClearNewFacilityBlueprintAlert(FacilityBlueprint facilityBlueprint)
        {
            if (_newFacilityBlueprintNames.Any(m => m == facilityBlueprint.name))
            {
                _newFacilityBlueprintNames.Remove(facilityBlueprint.name);
            }
        }

        public void ClearAllFacilityBlueprintAlerts()
        {
            _newFacilityBlueprintNames.Clear();
        }
    }
}
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
        
        [JsonProperty]
        private List<string> _newMissionNames = new List<string>();

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
    }
}
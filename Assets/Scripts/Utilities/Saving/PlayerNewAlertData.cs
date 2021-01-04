﻿using Newtonsoft.Json;
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
        public List<string> NewBlueprintNames => _newBlueprintNames;

        [JsonProperty]
        private List<string> _newBlueprintNames = new List<string>();

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
    }
}
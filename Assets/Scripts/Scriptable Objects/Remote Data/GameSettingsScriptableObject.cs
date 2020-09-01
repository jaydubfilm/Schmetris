using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Game Settings", menuName = "Star Salvager/Scriptable Objects/Game Settings")]
    public class GameSettingsScriptableObject : ScriptableObject
    {
        public bool allowAccessToUnlockedLaterWaves = true;
        public float timeForAsteroidToFallOneSquare = 0.25f;
        public float DASTime = 0.15f;
        public bool disableTestingFeatures = false;

        public void SetupGameSettings()
        {
            Globals.SetGameSettings(this);
        }
    }
}
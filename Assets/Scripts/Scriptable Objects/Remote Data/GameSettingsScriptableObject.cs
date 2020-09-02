using Sirenix.OdinInspector;
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
        public bool disableTestingFeatures = false;
        public bool bitsPushThroughParts = false;
        public float timeForAsteroidToFallOneSquare = 0.25f;
        public float DASTime = 0.15f;
        public float gridHeightRelativeToScreen = 1.25f;
        public float obstacleMass = 2.0f;
        public float botHorizontalSpeed = 30.0f;
        public float missionReminderFrequency = 25.0f;

        public bool cameraUseInputMotion = true;
        [ShowIf("cameraUseInputMotion", true)]
        public float cameraSmoothing = 8.0f;
        [ShowIf("cameraUseInputMotion", true)]
        public float cameraOffsetBounds = 0.5f;

        public void SetupGameSettings()
        {
            Globals.SetGameSettings(this);
        }
    }
}
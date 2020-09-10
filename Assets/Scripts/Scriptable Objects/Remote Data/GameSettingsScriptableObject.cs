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
        
        [DisableInPlayMode]
        public int gridWidth = 300;
        public float timeForAsteroidToFallOneSquare = 0.25f;
        public float DASTime = 0.15f;

        [DisableInPlayMode]
        public float gridHeightRelativeToScreen = 1.25f;
        public float obstacleMass = 2.0f;
        public float obstacleDensityReductionModifier = 5.0f;
        public float botHorizontalSpeed = 30.0f;
        public float missionReminderFrequency = 25.0f;
        public float bonusShapeSpeed = 12.0f;
        public float bonusShapeGearsReward = 100.0f;

        public bool cameraUseInputMotion = true;
        [ShowIf("cameraUseInputMotion", true)]
        public float cameraSmoothing = 1.0f;
        [ShowIf("cameraUseInputMotion", true)]
        public float cameraOffsetBounds = 0.5f;

        public List<BlueprintInitialData> blueprintInitialData = new List<BlueprintInitialData>();

        public void SetupGameSettings()
        {
            Globals.SetGameSettings(this);
        }
    }
}
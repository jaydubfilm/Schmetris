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
        public bool testingFeatures = false;
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
        public float botRotationSpeed = 500.0f;
        public float botContinuousRotationSpeed = 700.0f;
        public float missionReminderFrequency = 25.0f;
        public float bonusShapeSpeed = 12.0f;
        [Range(0,1f)]
        public float levelResourceDropReductionAmount = 0.5f;
        public float asteroidDamage = 10.0f;
        public List<BonusShapeGearsValue> bonusShapeGearsRewards;
        public int patchPointBaseCost = 500;
        public int patchPointIncrementCost = 50;

        public float botEnterScreenMaxSize = 1.5f;
        public float botExitScreenMaxSize = 2.0f;

        public bool cameraUseInputMotion = true;
        [ShowIf("cameraUseInputMotion", true)]
        public float cameraSmoothing = 1.0f;
        [ShowIf("cameraUseInputMotion", true)]
        public float cameraOffsetBounds = 0.5f;

        public int magnetRefineThreshold = 5;
        public int numCurrentTrackedMissionMax = 3;
        public bool onlyGetWaveLootOnce = true;
        public bool recoveryOfDroneLocksHorizontalMovement = true;
        public bool shortcutJumpToAfter = false;

        public List<BlueprintInitialData> blueprintInitialData = new List<BlueprintInitialData>();
        public List<FacilityInitialData> facilityInitialData = new List<FacilityInitialData>();
        public List<FacilityInitialData> facilityInitialBlueprintData = new List<FacilityInitialData>();

        public void SetupGameSettings()
        {
            Globals.SetGameSettings(this);
        }
    }
}
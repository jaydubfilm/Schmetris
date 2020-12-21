using Sirenix.OdinInspector;
using StarSalvager.Values;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Game Settings", menuName = "Star Salvager/Scriptable Objects/Game Settings")]
    public class GameSettingsScriptableObject : ScriptableObject
    {
        [BoxGroup("Debugging")]
        public bool testingFeatures = false;

        [BoxGroup("Asteroids")]
        public float asteroidDamage = 10.0f;
        
        [Range(0, 1f)][BoxGroup("Resource Processing")]
        //FIXME The naming for this sucks
        public float gameUIResourceThreshold = 0.33f;

        //Mission Properties
        //====================================================================================================================//

        [BoxGroup("Missions")]
        public float missionReminderFrequency = 25.0f;
        [BoxGroup("Missions")]
        public int numCurrentTrackedMissionMax = 3;

        //Facility Properties
        //====================================================================================================================//
        
        [BoxGroup("Facilities")]
        public int patchPointBaseCost = 500;
        [BoxGroup("Facilities")]
        public int patchPointIncrementCost = 50;
        
        [BoxGroup("Facilities"), Space(10f)]
        public List<BlueprintInitialData> blueprintInitialData = new List<BlueprintInitialData>();
        [BoxGroup("Facilities")]
        public List<FacilityInitialData> facilityInitialData = new List<FacilityInitialData>();
        [BoxGroup("Facilities")]
        public List<FacilityInitialData> facilityInitialBlueprintData = new List<FacilityInitialData>();

        //Wave Properties
        //====================================================================================================================//
        
        [DisableInPlayMode][BoxGroup("Waves")]
        public int gridWidth = 300;
        
        [DisableInPlayMode]
        public float gridHeightRelativeToScreen = 1.25f;
        
        [BoxGroup("Waves")]
        public float timeForAsteroidToFallOneSquare = 0.25f;
        
        [BoxGroup("Waves")]
        public bool allowAccessToUnlockedLaterWaves = true;
        
        [Range(0,1f)][BoxGroup("Waves")]
        public float levelResourceDropReductionAmount = 0.5f;
        
        [BoxGroup("Waves")]
        public float obstacleMass = 2.0f;
        [BoxGroup("Waves")]
        public float obstacleDensityReductionModifier = 5.0f;
        
        [BoxGroup("Waves")]
        public bool onlyGetWaveLootOnce = true;
        [BoxGroup("Waves")]
        public bool recoveryOfDroneLocksHorizontalMovement = true;
        [BoxGroup("Waves")]
        public bool shortcutJumpToAfter = false;
        
        [FoldoutGroup("Waves/Bonus Shapes")]
        public float bonusShapeSpeed = 12.0f;
        [FoldoutGroup("Waves/Bonus Shapes")]
        public List<BonusShapeGearsValue> bonusShapeGearsRewards;
        
        [FoldoutGroup("Waves/Post")]
        public float asteroidSpawnDisableTimeBeforeWaveEnd = 5.0f;
        [FoldoutGroup("Waves/Post")]
        public float timeAfterWaveEndFlyOut = 5.0f;
        

        //Bot Properties
        //====================================================================================================================//
        
        [BoxGroup("Bot")]
        public float botEnterScreenMaxSize = 1.5f;
        [BoxGroup("Bot")]
        public float botExitScreenMaxSize = 2.0f;
        
        [FoldoutGroup("Bot/Animations")]
        public bool bitsPushThroughParts;
        [FoldoutGroup("Bot/Animations"), SuffixLabel("s", true), Range(0.01f, 1f)]
        public float bitShiftTime;
        [FoldoutGroup("Bot/Animations"), SuffixLabel("s", true), Range(0.01f, 1f)]
        public float comboMergeTime;
        
        
        [FoldoutGroup("Bot/Movement")]
        public float DASTime = 0.15f;
        [FoldoutGroup("Bot/Movement")]
        public float botHorizontalSpeed = 30.0f;
        [FoldoutGroup("Bot/Movement")]
        public float botRotationSpeed = 500.0f;
        [FoldoutGroup("Bot/Movement")]
        public float botContinuousRotationSpeed = 700.0f;
        
        [FoldoutGroup("Bot/Vertical Movement")]
        public float verticalMoveSpeed = 0.15f;
        [FoldoutGroup("Bot/Vertical Movement")]
        public float maxHeight = 10;
        [FoldoutGroup("Bot/Vertical Movement")]
        public float minHeight = 0.15f;

        //Camera Properties
        //====================================================================================================================//
        
        [BoxGroup("Camera")]
        public bool cameraUseInputMotion = true;
        [ShowIf("cameraUseInputMotion", true)][BoxGroup("Camera")]
        public float cameraSmoothing = 1.0f;
        [ShowIf("cameraUseInputMotion", true)][BoxGroup("Camera")]
        public float cameraOffsetBounds = 0.5f;

        //====================================================================================================================//

        public void SetupGameSettings()
        {
            Globals.SetGameSettings(this);
        }
    }
}
using Sirenix.OdinInspector;
using StarSalvager.Values;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Game Settings", menuName = "Star Salvager/Scriptable Objects/Game Settings")]
    public class GameSettingsScriptableObject : ScriptableObject
    {
        
        [BoxGroup("Prototyping")]
        public bool useCenterFiring = true;
        [BoxGroup("Prototyping")]
        public bool useShuffleDance = true;
        [BoxGroup("Prototyping")]
        public bool shuffleCanDisconnect = true;
        [BoxGroup("Prototyping"), Range(0.01f, 1f)]
        public float shuffleTimeThreshold = 0.3f;
        
        
        [BoxGroup("Prototyping")]
        public int startingAmmo = 20;

        [BoxGroup("Debugging")]
        public bool testingFeatures = false;

        [BoxGroup("Asteroids")]
        public float asteroidDamage = 10.0f;

        [Range(0, 1f)][BoxGroup("Resource Processing")]
        //FIXME The naming for this sucks
        public float gameUIResourceThreshold = 0.33f;

        //Experience Properties
        //====================================================================================================================//

        [BoxGroup("Experience")]
        public int levelBaseExperience = 500;
        [BoxGroup("Experience")]
        public int levelExperienceIncrement = 50;

        //Wave Properties
        //====================================================================================================================//

        /*[DisableInPlayMode][BoxGroup("Waves")]
        public int gridWidth = 300;*/

        [DisableInPlayMode]
        public float gridHeightRelativeToScreen = 1.25f;

        [BoxGroup("Waves")]
        public float timeForAsteroidToFallOneSquare = 0.25f;

        [Range(0,1f)][BoxGroup("Waves")]
        public float levelResourceDropReductionAmount = 0.5f;

        [BoxGroup("Waves")]
        public float obstacleMass = 2.0f;
        [BoxGroup("Waves")]
        public float obstacleDensityReductionModifier = 5.0f;

        [BoxGroup("Waves")]
        public float waveMessageReminderFrequency = 10.0f;

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

        //[BoxGroup("Bot")]
        //public float botHealth = 100f;
        /*[BoxGroup("Bot")]
        public float greenHealAmount = 10f;*/
        
        [BoxGroup("Bot"), Range(0f,10f)]
        public float botHealWaitTime = 2f;
        
        [BoxGroup("Bot")]
        public float decoyDroneHealth = 20f;
        
        [BoxGroup("Bot"), Range(0f,1f)]
        public float bitDropCollectionMultiplier = 1f;
        
        //[BoxGroup("Bot")]
        //public int magnetAmount = 10;

        [BoxGroup("Bot")]
        public float botEnterScreenMaxSize = 1.5f;
        [BoxGroup("Bot")]
        public float botExitScreenMaxSize = 2.0f;
        [BoxGroup("Bot")]
        public bool unmergeLargeBitsOnRefine = true;
        [BoxGroup("Bot")]
        public bool sendExcessResourceToBase = false;
        
        [BoxGroup("Bot")]
        public int bounceDistance = 3;

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
        [FoldoutGroup("Bot/Movement")]
        public int dashDistance = 5;
        [FoldoutGroup("Bot/Movement")]
        public float dashSpeed = 60f;
        [FoldoutGroup("Bot/Movement")]
        public float dashCooldown = 1f;
        
        //Camera Properties
        //====================================================================================================================//

        [BoxGroup("Camera")]
        public bool cameraUseInputMotion = true;
        [HorizontalGroup("Camera/Row1")]
        public int columnsOnScreen = 51;
        [ShowIf("cameraUseInputMotion", true)][BoxGroup("Camera")]
        public float cameraSmoothing = 1.0f;
        [ShowIf("cameraUseInputMotion", true)][BoxGroup("Camera")]
        public float cameraOffsetBounds = 0.5f;

        //====================================================================================================================//

        [Button("Update"), HorizontalGroup("Camera/Row1"), DisableInEditorMode]
        private void UpdateCameraScale()
        {
            Globals.ScaleCamera(Globals.CameraScaleSize);
        }



        public void SetupGameSettings()
        {
            Globals.SetGameSettings(this);
        }
    }
}

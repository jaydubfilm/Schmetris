using System;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Cameras;
using StarSalvager.Cameras.Data;
using StarSalvager.ScriptableObjects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Values
{
    public static class Globals
    {
        //Values that don't change throughout gameplay
        public static string UserID = string.Empty;
        //public static string UserID = AnalyticsSessionInfo.userId;
        //public static string SessionID = System.Guid.NewGuid().ToString();
        public static string SessionID = string.Empty;


        public static bool UsingTutorial;
        
        //Values that change throughout gameplay - only set defaults here
        public static int ColumnsOnScreen = Constants.initialColumnsOnScreen;
        //FIXME I no longer like how this is implemented
        public static DIRECTION MovingDirection = DIRECTION.NULL;
        public static int CurrentSector = 0;
        public static int CurrentWave = 0;
        public static bool SectorComplete = false;
        public static Action<ORIENTATION> OrientationChange;
        public static int GridSizeY;
        public static bool IsBetweenWavesInUniverseMap = false;
        public static bool IsRecoveryBot = false;


        private static GameSettingsScriptableObject m_gameSettings = null;
        //Properties from Game Settings - do not give explicit values
        public static bool AllowAccessToUnlockedLaterWaves => m_gameSettings.allowAccessToUnlockedLaterWaves;
        public static bool BitsPushThroughParts => m_gameSettings.bitsPushThroughParts;
        public static float TimeForAsteroidToFallOneSquare => m_gameSettings.timeForAsteroidToFallOneSquare;
        public static float DASTime => m_gameSettings.DASTime;
        public static float GridHeightRelativeToScreen => m_gameSettings.gridHeightRelativeToScreen;
        public static float ObstacleMass => m_gameSettings.obstacleMass;
        public static float ObstacleDensityReductionModifier => m_gameSettings.obstacleDensityReductionModifier;
        public static float BotHorizontalSpeed => m_gameSettings.botHorizontalSpeed;
        public static float BotRotationSpeed => m_gameSettings.botRotationSpeed;
        public static float BotContinuousRotationSpeed => m_gameSettings.botContinuousRotationSpeed;
        public static float MissionReminderFrequency => m_gameSettings.missionReminderFrequency;
        public static bool CameraUseInputMotion => m_gameSettings.cameraUseInputMotion;
        public static float CameraSmoothing => m_gameSettings.cameraSmoothing;
        public static float CameraOffsetBounds => Constants.gridCellSize * Globals.ColumnsOnScreen * m_gameSettings.cameraOffsetBounds / 2;
        public static int GridSizeX => m_gameSettings.gridWidth;
        public static float AsteroidDamage => m_gameSettings.asteroidDamage;
        public static float BonusShapeDuration => m_gameSettings.bonusShapeSpeed;
        public static float LevelResourceDropReductionAmount => m_gameSettings.levelResourceDropReductionAmount;
        public static List<BlueprintInitialData> BlueprintInitialData => m_gameSettings.blueprintInitialData;
        public static List<FacilityInitialData> FacilityInitialData => m_gameSettings.facilityInitialData;
        public static List<FacilityInitialData> FacilityInitialBlueprintData => m_gameSettings.facilityInitialBlueprintData;

        public static int NumCurrentTrackedMissionMax => m_gameSettings.numCurrentTrackedMissionMax;
        public static bool OnlyGetWaveLootOnce => m_gameSettings.onlyGetWaveLootOnce;
        public static bool RecoveryOfDroneLocksHorizontalMovement => m_gameSettings.recoveryOfDroneLocksHorizontalMovement;

        //Values set by Game Settings - do not set values here
        public static bool DisableTestingFeatures;
        public static float AsteroidFallTimer;

        public static ORIENTATION Orientation
        {
            get => _orientation;
            set
            {
                _orientation = value;
                OrientationChange?.Invoke(_orientation);
            }
        }
        private static ORIENTATION _orientation;

        public static int GetBonusShapeGearRewards(int numCells, int numColours)
        {
            BonusShapeGearsValue bonusShapeValue = m_gameSettings.bonusShapeGearsRewards.FirstOrDefault(b => b.numCells == numCells && b.numColours == numColours);

            if (bonusShapeValue != null)
            {
                return bonusShapeValue.gearsValue;
            }

            Debug.Log("Missing value for bonus shape gears reward, calculating a filler value");
            return numCells * numColours * 25;
        }

        public static void SetGameSettings(GameSettingsScriptableObject gameSettings)
        {
            m_gameSettings = gameSettings;

            DisableTestingFeatures = m_gameSettings.disableTestingFeatures;
            AsteroidFallTimer = TimeForAsteroidToFallOneSquare / 2;
        }
        
        public static void ScaleCamera(float cameraZoomScalerValue)
        {
            ColumnsOnScreen = (int)cameraZoomScalerValue;
            if (ColumnsOnScreen % 2 == 0)
                ColumnsOnScreen += 1;

            var cameraController = Object.FindObjectOfType<CameraController>();
            
            if(cameraController)
                cameraController.SetOrthographicSize(Constants.gridCellSize * ColumnsOnScreen, Vector3.zero);

            if (Orientation == ORIENTATION.VERTICAL)
            {
                GridSizeY = (int)((CameraController.Camera.orthographicSize * GridHeightRelativeToScreen * 2) / Constants.gridCellSize);
            }
            else
            {
                GridSizeY = (int)((CameraController.Camera.orthographicSize * GridHeightRelativeToScreen * 2 * (Screen.width / (float)Screen.height)) / Constants.gridCellSize);
            }
        }
    }
}



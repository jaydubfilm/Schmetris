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
        public static Action<ORIENTATION> OrientationChange;
        public static int GridSizeY;
        public static float TimeForAsteroidToFallOneSquare;
        public static float TimeForAsteroidToFallOneSquareOriginal;

        //FIXME This is a mess, and must be organized
        //Game Settings Values
        //====================================================================================================================//

        private static GameSettingsScriptableObject m_gameSettings = null;

        public static int Magnetism => m_gameSettings.magnetAmount;
        
        public static float ShuffleTimeThreshold => m_gameSettings.shuffleTimeThreshold;

        public static int StartingAmmo => m_gameSettings.startingAmmo;
        
        public static bool UseShuffleDance => m_gameSettings.useShuffleDance;
        public static bool ShuffleCanDisconnect => m_gameSettings.shuffleCanDisconnect;

        public static int CameraScaleSize => m_gameSettings.columnsOnScreen;
        public static int DashDistance => m_gameSettings.dashDistance;
        public static int AsteroidBounceDistance => m_gameSettings.bounceDistance;
        public static float DashSpeed => m_gameSettings.dashSpeed;
        public static float DashCooldown => m_gameSettings.dashCooldown;
        
        public static float BotStartingHealth => m_gameSettings.botHealth;
        public static float GreenHealAmount => m_gameSettings.greenHealAmount;
        public static bool BitsPushThroughParts => m_gameSettings.bitsPushThroughParts;
        public static float DASTime => m_gameSettings.DASTime;
        public static float GridHeightRelativeToScreen => m_gameSettings.gridHeightRelativeToScreen;
        public static float ObstacleMass => m_gameSettings.obstacleMass;
        public static float ObstacleDensityReductionModifier => m_gameSettings.obstacleDensityReductionModifier;
        public static float BotHorizontalSpeed => m_gameSettings.botHorizontalSpeed;
        public static float BotRotationSpeed => m_gameSettings.botRotationSpeed;
        public static float BotContinuousRotationSpeed => m_gameSettings.botContinuousRotationSpeed;
        public static float WaveMessageReminderFrequency => m_gameSettings.waveMessageReminderFrequency;
        public static bool CameraUseInputMotion => m_gameSettings.cameraUseInputMotion;
        public static float CameraSmoothing => m_gameSettings.cameraSmoothing;
        public static float CameraOffsetBounds => Constants.gridCellSize * Globals.ColumnsOnScreen * m_gameSettings.cameraOffsetBounds / 2;
        public static int GridSizeX => m_gameSettings.gridWidth;
        public static float AsteroidDamage => m_gameSettings.asteroidDamage;
        public static float BonusShapeDuration => m_gameSettings.bonusShapeSpeed;
        public static float LevelResourceDropReductionAmount => m_gameSettings.levelResourceDropReductionAmount;
        public static int LevelBaseExperience => m_gameSettings.levelBaseExperience;
        public static int LevelExperienceIncrement => m_gameSettings.levelExperienceIncrement;

        public static float GameUIResourceThreshold => m_gameSettings.gameUIResourceThreshold;

        public static float BotEnterScreenMaxSize => m_gameSettings.botEnterScreenMaxSize;
        public static float BotExitScreenMaxSize => m_gameSettings.botExitScreenMaxSize;

        public static float AsteroidSpawnDisableTimeBeforeWaveEnd => m_gameSettings.asteroidSpawnDisableTimeBeforeWaveEnd;
        public static float TimeAfterWaveEndFlyOut => m_gameSettings.timeAfterWaveEndFlyOut;

        public static float BitShiftTime => m_gameSettings.bitShiftTime;
        public static float ComboMergeTime => m_gameSettings.comboMergeTime;
        public static bool UnmergeLargeBitsOnRefine => m_gameSettings.unmergeLargeBitsOnRefine;
        public static bool SendExcessResourceToBase => m_gameSettings.sendExcessResourceToBase;

        //Values set by Game Settings - do not set values here
        public static bool TestingFeatures;
        public static float AsteroidFallTimer;

        //====================================================================================================================//


        public static void Init()
        {
            TimeForAsteroidToFallOneSquare = m_gameSettings.timeForAsteroidToFallOneSquare;
            TimeForAsteroidToFallOneSquareOriginal = m_gameSettings.timeForAsteroidToFallOneSquare;
        }

        public static void ResetFallSpeed()
        {
            TimeForAsteroidToFallOneSquare = TimeForAsteroidToFallOneSquareOriginal;
        }

        public static void IncreaseFallSpeed()
        {
            if (TimeForAsteroidToFallOneSquare >= TimeForAsteroidToFallOneSquareOriginal / 2.0f)
            {
                TimeForAsteroidToFallOneSquare *= (3.0f / 4.0f);
            }
        }

        public static void DecreaseFallSpeed()
        {
            if (TimeForAsteroidToFallOneSquare < TimeForAsteroidToFallOneSquareOriginal * 2.0f)
            {
                TimeForAsteroidToFallOneSquare *= (4.0f / 3.0f);
            }
        }

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

            TestingFeatures = m_gameSettings.testingFeatures;
            AsteroidFallTimer = TimeForAsteroidToFallOneSquare / 2;
        }

        public static void ScaleCamera(float cameraZoomScalerValue)
        {
            if (!CameraController.Camera)
                return;

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

using System;
using StarSalvager.Cameras;
using StarSalvager.Cameras.Data;
using StarSalvager.ScriptableObjects;
using UnityEngine;
using UnityEngine.Analytics;
using Object = UnityEngine.Object;

namespace StarSalvager.Values
{
    public static class Globals
    {
        //Values that don't change throughout gameplay
        public static string UserID = AnalyticsSessionInfo.userId;
        public static string SessionID = System.Guid.NewGuid().ToString();


        //Values that change throughout gameplay - only set defaults here
        public static int ColumnsOnScreen = Constants.initialColumnsOnScreen;
        public static DIRECTION MovingDirection = DIRECTION.NULL;
        public static int CurrentSector = 0;
        public static int CurrentWave = 0;
        public static bool SectorComplete = false;
        public static Action<ORIENTATION> OrientationChange;
        public static int GridSizeX;
        public static int GridSizeY;


        private static GameSettingsScriptableObject m_gameSettings = null;
        //Properties from Game Settings - do not give explicit values
        public static bool AllowAccessToUnlockedLaterWaves => m_gameSettings.allowAccessToUnlockedLaterWaves;
        public static bool BitsPushThroughParts => m_gameSettings.bitsPushThroughParts;
        public static float TimeForAsteroidToFallOneSquare => m_gameSettings.timeForAsteroidToFallOneSquare;
        public static float DASTime => m_gameSettings.DASTime;
        public static float GridHeightRelativeToScreen => m_gameSettings.gridHeightRelativeToScreen;
        public static float ObstacleMass => m_gameSettings.obstacleMass;
        public static float BotHorizontalSpeed => m_gameSettings.botHorizontalSpeed;
        public static float MissionReminderFrequency => m_gameSettings.missionReminderFrequency;

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
                GridSizeY = (int)((Camera.main.orthographicSize * GridHeightRelativeToScreen * 2) / Constants.gridCellSize);
            }
            else
            {
                GridSizeY = (int)((Camera.main.orthographicSize * GridHeightRelativeToScreen * 2 * (Screen.width / (float)Screen.height)) / Constants.gridCellSize);
            }
        }
    }
}



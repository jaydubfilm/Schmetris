using System;
using System.Collections.Generic;
using StarSalvager.Cameras;
using StarSalvager.Cameras.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Values
{
    public static class Globals
    {
        public static DIRECTION MovingDirection = DIRECTION.NULL;
        
        public static int GridSizeX;
        public static int GridSizeY;
        public static int ColumnsOnScreen = Constants.initialColumnsOnScreen;
        public static int CurrentSector = 0;
        public static int CurrentWave = 0;
        public static bool SectorComplete = false;
        public static float AsteroidFallTimer = Constants.timeForAsteroidsToFall / 2;
        public static Action<ORIENTATION> OrientationChange;
        
        public static float DASTime = 0.15f;

        private static string[] myValues = { "one", "two", "three" };

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
                GridSizeX = (int)(ColumnsOnScreen * Constants.GridWidthRelativeToScreen);
                GridSizeY = (int)((Camera.main.orthographicSize * Constants.GridHeightRelativeToScreen * 2) / Constants.gridCellSize);
            }
            else
            {
                GridSizeX = (int)(ColumnsOnScreen * Constants.GridWidthRelativeToScreen * (Screen.height / (float)Screen.width));
                GridSizeY = (int)((Camera.main.orthographicSize * Constants.GridHeightRelativeToScreen * 2 * (Screen.width / (float)Screen.height)) / Constants.gridCellSize);
            }
        }
    }
}



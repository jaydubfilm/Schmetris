using System;
using System.Collections.Generic;
using StarSalvager.Cameras.Data;

namespace StarSalvager.Values
{
    public static class Globals
    {
        public static DIRECTION MovingDirection = DIRECTION.NULL;
        
        public static int GridSizeX;
        public static int GridSizeY;
        public static int ColumnsOnScreen = Values.Constants.initialColumnsOnScreen;
        public static int CurrentSector = 0;
        public static int MaxSector = 0;
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
    }
}



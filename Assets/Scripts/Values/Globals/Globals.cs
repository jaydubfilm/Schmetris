using System;
using StarSalvager.Cameras.Data;

namespace StarSalvager.Values
{
    public static class Globals
    {
        public static Action<ORIENTATION> OrientationChange;

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



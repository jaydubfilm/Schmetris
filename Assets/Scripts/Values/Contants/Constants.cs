using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Values
{
    public static class Constants
    {
        public const float gridCellSize = 1.28f;
        private const float gridWidthRelativeToScreen = 10.0f;
        private const float gridHeightRelativeToScreen = 1.25f;
        public const int enemyGridScanRadius = 4;
        public const float obstacleMass = 2.0f;
        public const float botHorizontalSpeed = 30.0f;
        public const int initialColumnsOnScreen = 51;

        
        public static float GridHeightRelativeToScreen => gridHeightRelativeToScreen;
    }
}
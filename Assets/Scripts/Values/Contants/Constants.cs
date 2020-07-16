﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Values
{
    public static class Constants
    {
        public const float gridCellSize = 1.28f;
        private const float gridWidthRelativeToScreen = 3.0f;
        private const float gridHeightRelativeToScreen = 2.0f;
        public const float timeForAsteroidsToFall = 0.25f;
        public const int enemyGridScanRadius = 3;
        public const float obstacleMass = 2.0f;
        public const float botHorizontalSpeed = 30.0f;
        public const int initialColumnsOnScreen = 31;

        

        public static float GridWidthRelativeToScreen => gridWidthRelativeToScreen;
        public static float GridHeightRelativeToScreen => gridHeightRelativeToScreen;
    }
}
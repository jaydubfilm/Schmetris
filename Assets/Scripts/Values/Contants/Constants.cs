using System;

namespace StarSalvager.Values
{
    public static class Constants
    {
        public static readonly Version VERSION = new Version(1, 4, 0, 0);

        public const float VISIBLE_GAME_AREA = 0.55f;
        
        public const float gridCellSize = 1f;
        public const int enemyGridScanRadius = 4;
        public const int gridPositionSpacing = 3;
        public const int initialColumnsOnScreen = 51;

        public const float waterDrainRate = 0.25f;

        public static readonly BIT_TYPE[] BIT_ORDER = 
        {
            BIT_TYPE.RED,
            BIT_TYPE.GREY,
            BIT_TYPE.GREEN,
            BIT_TYPE.YELLOW,
            BIT_TYPE.BLUE
        };
    }
}
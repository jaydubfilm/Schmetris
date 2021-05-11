using System;

namespace StarSalvager.Values
{
    public static class Constants
    {
        public static readonly Version VERSION = new Version(1, 30, 6, 0);

        public const float VISIBLE_GAME_AREA = 0.55f;
        
        public const float gridCellSize = 1f;
        public const int enemyGridScanRadius = 4;
        public const int gridPositionSpacing = 3;
        public const int initialColumnsOnScreen = 51;

        //public const float waterDrainRate = 0.25f;

        /*{
            UP,
            DOWN,
            Bottom-Right Window,
            LEFT,
            RIGHT,
            
        }*/
        public static readonly BIT_TYPE[] BIT_ORDER = 
        {
            BIT_TYPE.YELLOW,    /*Up*/        
            BIT_TYPE.GREY,      /*Down*/    
            BIT_TYPE.GREEN,     /*BR Window*/    
            BIT_TYPE.BLUE,      /*Left*/    
            BIT_TYPE.RED,       /*Right*/
        };
    }
}
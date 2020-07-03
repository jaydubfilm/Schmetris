using System;

namespace StarSalvager.Utilities.Puzzle.Data
{
    [Obsolete("Use ComboRemoteData")]
    public struct ComboData
    {
        public COMBO type;
        public int points;
        public int addLevels;

        public static ComboData zero => new ComboData
        {
            type = COMBO.NONE,
            points = 0,
            addLevels = 0
        };

    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities.Puzzle.Data
{
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


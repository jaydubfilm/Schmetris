using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities.Puzzle.Data
{
    public struct ComboData
    {
        public string name;
        public int points;
        public int addLevels;

        public static ComboData zero => new ComboData
        {
            name = string.Empty,
            points = 0,
            addLevels = 0
        };

    }
}


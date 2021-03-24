using System;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Puzzle.Data;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class ComboRemoteData
    {
        [FoldoutGroup("$type")]
        public COMBO type;
        [FoldoutGroup("$type")]
        public int points;
        [FoldoutGroup("$type")]
        public int addLevels;
        
        public static ComboRemoteData zero => new ComboRemoteData
        {
            type = COMBO.NONE,
            points = 0,
            addLevels = 0,
        };
    }
}


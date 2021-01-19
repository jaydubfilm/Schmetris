using System;
using System.Collections.Generic;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Puzzle.Data;

namespace StarSalvager.Utilities.Puzzle.Interfaces
{
    public interface IComboCheck
    {
        [Obsolete]
        bool TryGetCombo(ICanCombo origin, List<ICanCombo>[] directions,
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out (ComboRemoteData comboData, List<ICanCombo> toMove) outData);
        
        bool TryGetCombo(Bit origin, List<Bot.DataTest>[] directions,
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out PuzzleChecker.MoveData outData);
    }
}


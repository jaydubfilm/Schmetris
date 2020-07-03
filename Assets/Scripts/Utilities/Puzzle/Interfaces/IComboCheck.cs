using System.Collections.Generic;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Puzzle.Data;

namespace StarSalvager.Utilities.Puzzle.Interfaces
{
    public interface IComboCheck
    {
        bool TryGetCombo(Bit origin, List<Bit>[] directions,
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out (ComboRemoteData comboData, List<Bit> toMove) outData);
    }
}


using System.Collections.Generic;
using StarSalvager.Utilities.Puzzle.Data;

namespace StarSalvager.Utilities.Puzzle.Interfaces
{
    public interface IComboCheck
    {
        bool TryGetCombo(Bit origin, List<Bit>[] directions,
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out (ComboData comboData, List<Bit> toMove) outData);
    }
}


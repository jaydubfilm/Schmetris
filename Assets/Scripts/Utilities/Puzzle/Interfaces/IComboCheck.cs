using System.Collections.Generic;
using StarSalvager.Utilities.Puzzle.Data;

namespace StarSalvager.Utilities.Puzzle.Interfaces
{
    public interface IComboCheck
    {
        bool TryGetCombo(AttachableBase origin, List<AttachableBase>[] directions,
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out (ComboData comboData, List<AttachableBase> toMove) outData);
    }
}


using System.Collections.Generic;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Puzzle.Data;

namespace StarSalvager.Utilities.Puzzle.Interfaces
{
    public interface IComboCheck
    {
        bool TryGetCombo(ICanCombo origin, List<ICanCombo>[] directions,
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out (ComboRemoteData comboData, List<ICanCombo> toMove) outData);
    }
}


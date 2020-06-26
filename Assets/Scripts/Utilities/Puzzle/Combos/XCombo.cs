using System.Collections.Generic;
using StarSalvager.Factories;
using StarSalvager.Utilities.Puzzle.Data;

namespace StarSalvager.Utilities.Puzzle.Combos
{
    public class XCombo : LineCombo
    {
        public override bool TryGetCombo(Bit origin, List<Bit>[] directions,
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out (ComboData comboData, List<Bit> toMove) outData)
        {
            outData = (emptyCombo, null);

            //--------------------------------------------------------------------------------------------------------//

            if (lineData.horizontalCount != lineData.verticalCount)
                return false;


            if (directions[(int) DIRECTION.LEFT].Count == 1 
                && directions[(int) DIRECTION.RIGHT].Count == 1
                && directions[(int) DIRECTION.UP].Count == 1
                && directions[(int) DIRECTION.DOWN].Count == 1)
            {
                outData.toMove = new List<Bit>{ origin };
                outData.toMove.AddRange(directions[(int) DIRECTION.LEFT]);
                outData.toMove.AddRange(directions[(int) DIRECTION.RIGHT]);
                outData.toMove.AddRange(directions[(int) DIRECTION.UP]);
                outData.toMove.AddRange(directions[(int) DIRECTION.DOWN]);
            }
            else
                return false;



            outData.comboData = FactoryManager.Instance.GetFactory<ComboFactory>().GetComboData(COMBO.ANGLE);
            
            //Debug.LogError($"Found X Combo at {origin.gameObject.name}", origin);

            return true;
        }
    }
}

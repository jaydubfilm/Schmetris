using System.Collections.Generic;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Puzzle.Data;

namespace StarSalvager.Utilities.Puzzle.Combos
{
    public class XCombo : LineCombo
    {
        public override bool TryGetCombo(ICanCombo origin, List<ICanCombo>[] directions,
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out (ComboRemoteData comboData, List<ICanCombo> toMove) outData)
        {
            outData = (emptyCombo, null);

            //--------------------------------------------------------------------------------------------------------//

            if (lineData.horizontalCount != lineData.verticalCount)
                return false;

            var check = directions[(int) DIRECTION.LEFT].Count;


            //Want to check that all sides are of equal distance
            if (directions[(int) DIRECTION.LEFT].Count == check
                && directions[(int) DIRECTION.RIGHT].Count == check
                && directions[(int) DIRECTION.UP].Count == check
                && directions[(int) DIRECTION.DOWN].Count == check)
            {
                outData.toMove = new List<ICanCombo>{ origin };
                outData.toMove.AddRange(directions[(int) DIRECTION.LEFT]);
                outData.toMove.AddRange(directions[(int) DIRECTION.RIGHT]);
                outData.toMove.AddRange(directions[(int) DIRECTION.UP]);
                outData.toMove.AddRange(directions[(int) DIRECTION.DOWN]);
            }
            else
                return false;



            outData.comboData = FactoryManager.Instance.GetFactory<ComboFactory>().GetComboData(COMBO.CROSS);
            
            //Debug.LogError($"Found X Combo at {origin.gameObject.name}", origin);

            return true;
        }
    }
}

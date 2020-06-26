using System.Collections.Generic;
using StarSalvager.Factories;
using StarSalvager.Utilities.Puzzle.Data;
using UnityEngine;

namespace StarSalvager.Utilities.Puzzle.Combos
{
    public class TCombo : LineCombo
    {
        public override bool TryGetCombo(IAttachable origin, List<IAttachable>[] directions,
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out (ComboData comboData, List<IAttachable> toMove) outData)
        {
            outData = (emptyCombo, null);

            //--------------------------------------------------------------------------------------------------------//

            if (lineData.horizontalCount != lineData.verticalCount)
                return false;

            //Checking for these types of combos (0 = origin)
            //    #0#     #
            //     #      #
            //     #     #0#
            if (directions[(int) DIRECTION.LEFT].Count == 1 && directions[(int) DIRECTION.RIGHT].Count == 1)
            {
                switch (directions[(int) DIRECTION.UP].Count)
                {
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 2:
                        outData.toMove = new List<IAttachable> {origin};
                        outData.toMove.AddRange(directions[(int) DIRECTION.DOWN]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.LEFT]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.RIGHT]);
                        break;
                    case 2 when directions[(int) DIRECTION.DOWN].Count == 0:
                        outData.toMove = new List<IAttachable> {origin};
                        outData.toMove.AddRange(directions[(int) DIRECTION.UP]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.LEFT]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.RIGHT]);
                        break;
                    default:
                        return false;
                }
            }
            //Checking for these types of combos (0 = origin)
            //      #    #
            //    ##0    0##
            //      #    #
            else if (directions[(int) DIRECTION.UP].Count == 1 && directions[(int) DIRECTION.DOWN].Count == 1)
            {
                switch (directions[(int) DIRECTION.LEFT].Count)
                {
                    case 0 when directions[(int) DIRECTION.RIGHT].Count == 2:
                        outData.toMove = new List<IAttachable> {origin};
                        outData.toMove.AddRange(directions[(int) DIRECTION.UP]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.DOWN]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.RIGHT]);
                        break;
                    case 2 when directions[(int) DIRECTION.RIGHT].Count == 0:
                        outData.toMove = new List<IAttachable> {origin};
                        outData.toMove.AddRange(directions[(int) DIRECTION.UP]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.DOWN]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.LEFT]);
                        break;
                    default:
                        return false;
                }
            }
            else
                return false;

            outData.comboData = FactoryManager.Instance.GetFactory<ComboFactory>().GetComboData(COMBO.TEE);
            
            Debug.LogError($"Found T Combo at {origin.gameObject.name}", origin.gameObject);

            return true;
        }
    }
}

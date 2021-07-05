using System.Collections.Generic;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Puzzle.Data;
using UnityEngine;

namespace StarSalvager.Utilities.Puzzle.Combos
{
    public class TCombo : LineCombo
    {
        public bool TryGetCombo(ICanCombo origin, List<ICanCombo>[] directions,
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out (ComboRemoteData comboData, List<ICanCombo> toMove) outData)
        {
            outData = (emptyCombo, null);

            //--------------------------------------------------------------------------------------------------------//

            if (lineData.horizontalCount != lineData.verticalCount)
                return false;

            //Checking for these types of combos (0 = origin)
            //    #0#     #
            //     #      #
            //     #     #0#
            if (directions[(int) DIRECTION.LEFT].Count >= 1 && directions[(int) DIRECTION.RIGHT].Count >= 1 &&
                directions[(int) DIRECTION.LEFT].Count == directions[(int) DIRECTION.RIGHT].Count)
            {
                switch (directions[(int) DIRECTION.UP].Count)
                {
                    case 0 when directions[(int) DIRECTION.DOWN].Count >= 2:
                        outData.toMove = new List<ICanCombo> {origin};
                        outData.toMove.AddRange(directions[(int) DIRECTION.DOWN]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.LEFT]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.RIGHT]);
                        break;
                    case 2 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 3 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 4 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 5 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 6 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 7 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 8 when directions[(int) DIRECTION.DOWN].Count == 0:
                        outData.toMove = new List<ICanCombo> {origin};
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
            else if (directions[(int) DIRECTION.UP].Count >= 1 && directions[(int) DIRECTION.DOWN].Count >= 1 &&
                     directions[(int) DIRECTION.UP].Count == directions[(int) DIRECTION.DOWN].Count)
            {
                switch (directions[(int) DIRECTION.LEFT].Count)
                {
                    case 0 when directions[(int) DIRECTION.RIGHT].Count >= 2:
                        outData.toMove = new List<ICanCombo> {origin};
                        outData.toMove.AddRange(directions[(int) DIRECTION.UP]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.DOWN]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.RIGHT]);
                        break;
                    case 2 when directions[(int) DIRECTION.RIGHT].Count == 0:
                    case 3 when directions[(int) DIRECTION.RIGHT].Count == 0:
                    case 4 when directions[(int) DIRECTION.RIGHT].Count == 0:
                    case 5 when directions[(int) DIRECTION.RIGHT].Count == 0:
                    case 6 when directions[(int) DIRECTION.RIGHT].Count == 0:
                    case 7 when directions[(int) DIRECTION.RIGHT].Count == 0:
                    case 8 when directions[(int) DIRECTION.RIGHT].Count == 0:
                        outData.toMove = new List<ICanCombo> {origin};
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

            //Debug.LogError($"Found T Combo at {origin.gameObject.name}", origin.gameObject);

            return true;
        }
    }
}

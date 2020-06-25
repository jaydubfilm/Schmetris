using System.Collections.Generic;
using StarSalvager.Utilities.Puzzle.Data;
using UnityEngine;

namespace StarSalvager.Utilities.Puzzle.Combos
{
    public class LCombo : LineCombo
    {
        public override bool TryGetCombo(AttachableBase origin, List<AttachableBase>[] directions,
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out (ComboData comboData, List<AttachableBase> toMove) outData)
        {
            outData = (emptyCombo, null);

            //--------------------------------------------------------------------------------------------------------//

            if (lineData.horizontalCount != lineData.verticalCount)
                return false;


            //Checking for these types of combos (0 = origin)
            //  ##0      #
            //    #      #
            //    #    ##0
            if (directions[(int) DIRECTION.LEFT].Count == 2 && directions[(int) DIRECTION.RIGHT].Count == 0)
            {
                switch (directions[(int) DIRECTION.UP].Count)
                {
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 2:
                        outData.toMove = new List<AttachableBase> { origin };
                        outData.toMove.AddRange(directions[(int) DIRECTION.LEFT]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.DOWN]);

                        break;
                    case 2 when directions[(int) DIRECTION.DOWN].Count == 0:
                        outData.toMove = new List<AttachableBase> { origin };
                        outData.toMove.AddRange(directions[(int) DIRECTION.LEFT]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.UP]);
                        break;
                    default:
                        return false;
                }
            }
            //Checking for these types of combos (0 = origin)
            //      #     0##
            //      #     #
            //      0##   #
            else if (directions[(int) DIRECTION.RIGHT].Count == 2 && directions[(int) DIRECTION.LEFT].Count == 0)
            {
                switch (directions[(int) DIRECTION.UP].Count)
                {
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 2:
                        outData.toMove = new List<AttachableBase> { origin };
                        outData.toMove.AddRange(directions[(int) DIRECTION.RIGHT]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.DOWN]);

                        break;
                    case 2 when directions[(int) DIRECTION.DOWN].Count == 0:
                        outData.toMove = new List<AttachableBase> { origin };
                        outData.toMove.AddRange(directions[(int) DIRECTION.RIGHT]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.UP]);
                        break;
                    default:
                        return false;
                }
            }



            outData.comboData = new ComboData
            {
                name = "L Combo",
                addLevels = 2,
                points = 150
            };
            
            Debug.LogError($"Found L Combo at {origin.gameObject.name}", origin);

            return true;
        }
    }
}

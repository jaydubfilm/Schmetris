using System.Collections.Generic;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Puzzle.Data;
using UnityEngine;

namespace StarSalvager.Utilities.Puzzle.Combos
{
    public class LCombo : LineCombo
    {
        //FIXME I need to improve this functionality, as it feels very inefficient
        public override bool TryGetCombo(Bit origin, List<Bit>[] directions,
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out (ComboRemoteData comboData, List<Bit> toMove) outData)
        {
            outData = (emptyCombo, null);

            //--------------------------------------------------------------------------------------------------------//

            if (lineData.horizontalCount != lineData.verticalCount)
                return false;


            //Checking for these types of combos (0 = origin)
            //  ##0      #
            //    #      #
            //    #    ##0
            if (directions[(int) DIRECTION.LEFT].Count >= 2 && directions[(int) DIRECTION.RIGHT].Count == 0)
            {
                //Debug.Log("Left");
                switch (directions[(int) DIRECTION.UP].Count)
                {
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 2:
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 3:
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 4:
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 5:
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 6:
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 7:
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 8:
                        //Debug.Log("Down");
                        outData.toMove = new List<Bit> { origin };
                        outData.toMove.AddRange(directions[(int) DIRECTION.LEFT]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.DOWN]);

                        break;
                    case 2 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 3 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 4 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 5 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 6 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 7 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 8 when directions[(int) DIRECTION.DOWN].Count == 0:
                        //Debug.Log("Up");
                        outData.toMove = new List<Bit> { origin };
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
            else if (directions[(int) DIRECTION.RIGHT].Count >= 2 && directions[(int) DIRECTION.LEFT].Count == 0)
            {
                //Debug.Log("Right");
                switch (directions[(int) DIRECTION.UP].Count)
                {
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 2:
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 3:
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 4:
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 5:
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 6:
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 7:
                    case 0 when directions[(int) DIRECTION.DOWN].Count == 8:
                        //Debug.Log("Down");
                        outData.toMove = new List<Bit> { origin };
                        outData.toMove.AddRange(directions[(int) DIRECTION.RIGHT]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.DOWN]);

                        break;
                    case 2 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 3 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 4 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 5 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 6 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 7 when directions[(int) DIRECTION.DOWN].Count == 0:
                    case 8 when directions[(int) DIRECTION.DOWN].Count == 0:
                        //Debug.Log("Up");
                        outData.toMove = new List<Bit> { origin };
                        outData.toMove.AddRange(directions[(int) DIRECTION.RIGHT]);
                        outData.toMove.AddRange(directions[(int) DIRECTION.UP]);
                        break;
                    default:
                        return false;
                }
            }
            else
                return false;



            outData.comboData = FactoryManager.Instance.GetFactory<ComboFactory>().GetComboData(COMBO.ANGLE);
            
            //Debug.LogError($"Found L Combo at {origin.gameObject.name}", origin);

            return true;
        }
    }
}

using System.Collections.Generic;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Puzzle.Data;
using StarSalvager.Utilities.Puzzle.Interfaces;
using UnityEngine;

namespace StarSalvager.Utilities.Puzzle.Combos
{
    public class LineCombo : IComboCheck
    {
        protected readonly ComboRemoteData emptyCombo;

        public LineCombo()
        {
            emptyCombo = ComboRemoteData.zero;

        }

        public virtual bool TryGetCombo(IAttachable origin, List<IAttachable>[] directions,
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out (ComboRemoteData comboData, List<IAttachable> toMove) outData)
        {
            outData = (emptyCombo, null);

            //--------------------------------------------------------------------------------------------------------//

            int comboCount;
            //If Horizontal is greater than vertical
            var (_, horizontalCount, verticalCount) = lineData;
            
            if (horizontalCount > verticalCount)
            {
                outData.toMove = new List<IAttachable>{ origin };
                outData.toMove.AddRange(directions[(int)DIRECTION.LEFT]);
                outData.toMove.AddRange(directions[(int)DIRECTION.RIGHT]);

                //If the horizontal is the greater line, use that to decide point distribution
                comboCount = horizontalCount;
            }
            //else If Horizontal is less than vertical
            else if (horizontalCount < verticalCount)
            {
                outData.toMove = new List<IAttachable>{ origin };
                outData.toMove.AddRange(directions[(int)DIRECTION.UP]);
                outData.toMove.AddRange(directions[(int)DIRECTION.DOWN]);

                //If the vertical is the greater line, use that to decide point distribution
                comboCount = verticalCount;
            }
            //If the combo is more complex than a straight line, this object function will delegate solving to another
            else
            {
                return false;
            }

            //--------------------------------------------------------------------------------------------------------//

            COMBO comboType;
            //TODO These values need to be setup for Remote Data
            if (comboCount >= 5)
                comboType = COMBO.FIVE;
            else if (comboCount == 4)
                comboType = COMBO.FOUR;
            else
                comboType = COMBO.THREE;

            outData.comboData = FactoryManager.Instance.GetFactory<ComboFactory>().GetComboData(comboType);
            
            return true;

            //--------------------------------------------------------------------------------------------------------//
        }

    }
}
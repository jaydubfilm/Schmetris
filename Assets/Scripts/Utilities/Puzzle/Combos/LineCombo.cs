﻿using System.Collections.Generic;
using StarSalvager.Utilities.Puzzle.Data;
using StarSalvager.Utilities.Puzzle.Interfaces;
using UnityEngine;

namespace StarSalvager.Utilities.Puzzle.Combos
{
    public class LineCombo : IComboCheck
    {
        protected readonly ComboData emptyCombo;

        public LineCombo()
        {
            emptyCombo = ComboData.zero;

        }

        public virtual bool TryGetCombo(AttachableBase origin, List<AttachableBase>[] directions, 
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out (ComboData comboData, List<AttachableBase> toMove) outData)
        {
            outData = (emptyCombo, null);

            //--------------------------------------------------------------------------------------------------------//

            int comboCount;
            //If Horizontal is greater than vertical
            var (_, horizontalCount, verticalCount) = lineData;
            
            if (horizontalCount > verticalCount)
            {
                outData.toMove = new List<AttachableBase>{ origin };
                outData.toMove.AddRange(directions[(int)DIRECTION.LEFT]);
                outData.toMove.AddRange(directions[(int)DIRECTION.RIGHT]);

                //If the horizontal is the greater line, use that to decide point distribution
                comboCount = horizontalCount;
            }
            //else If Horizontal is less than vertical
            else if (horizontalCount < verticalCount)
            {
                outData.toMove = new List<AttachableBase>{ origin };
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

            //TODO These values need to be setup for Remote Data
            if (comboCount >= 5)
                outData.comboData = new ComboData
                {
                    type = COMBO.FIVE,
                    addLevels = 2,
                    points = 100
                };
            else if (comboCount == 4)
            {
                outData.comboData = new ComboData
                {
                    type = COMBO.FOUR,
                    addLevels = 1,
                    points = 50
                };
            }
            else
            {
                outData.comboData = new ComboData
                {
                    type = COMBO.THREE,
                    addLevels = 1,
                    points = 20
                };
            }
            
            return true;

            //--------------------------------------------------------------------------------------------------------//
        }

        
    }

}
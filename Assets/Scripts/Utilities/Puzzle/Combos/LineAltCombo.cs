﻿using System.Collections.Generic;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Puzzle.Data;
using StarSalvager.Utilities.Puzzle.Interfaces;
using UnityEngine;

namespace StarSalvager.Utilities.Puzzle.Combos
{
    public class LineAltCombo : IComboCheck
    {
        protected readonly ComboRemoteData emptyCombo;

        public LineAltCombo()
        {
            emptyCombo = ComboRemoteData.zero;

        }

        public virtual bool TryGetCombo(Bit origin, List<Bit>[] directions, 
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out (ComboRemoteData comboData, List<Bit> toMove) outData)
        {
            outData = (emptyCombo, null);

            //--------------------------------------------------------------------------------------------------------//

            //If Horizontal is greater than vertical
            var (_, horizontalCount, verticalCount) = lineData;

            if (horizontalCount < 3 && verticalCount < 3)
                return false;
            
            outData.toMove = new List<Bit>{ origin };
            outData.toMove.AddRange(directions[(int)DIRECTION.LEFT]);
            outData.toMove.AddRange(directions[(int)DIRECTION.RIGHT]);

            //If the horizontal is the greater line, use that to decide point distribution
            var comboCount = horizontalCount;

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
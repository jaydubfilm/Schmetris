using System;
using System.Collections.Generic;
using System.Linq;
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

        /*[Obsolete]
        public virtual bool TryGetCombo(ICanCombo origin, List<ICanCombo>[] directions,
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out (ComboRemoteData comboData, List<ICanCombo> toMove) outData)
        {
            outData = (emptyCombo, null);

            //--------------------------------------------------------------------------------------------------------//

            //If Horizontal is greater than vertical
            var (_, horizontalCount, verticalCount) = lineData;

            if (horizontalCount < 3 && verticalCount < 3)
                return false;
            
            outData.toMove = new List<ICanCombo>{ origin };
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
        }*/

        public bool TryGetCombo(Bit origin, 
            List<Bot.DataTest>[] directions, 
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out PuzzleChecker.MoveData outData)
        {
            outData = new PuzzleChecker.MoveData();

            //--------------------------------------------------------------------------------------------------------//

            //If Horizontal is greater than vertical
            var (_, horizontalCount, verticalCount) = lineData;

            if (horizontalCount < 3 && verticalCount < 3)
                return false;
            
            outData.ToMove = new List<Bit>{ origin };
            outData.ToMove.AddRange(directions[(int)DIRECTION.LEFT].Select(x => x.Attachable).OfType<Bit>());
            outData.ToMove.AddRange(directions[(int)DIRECTION.RIGHT].Select(x => x.Attachable).OfType<Bit>());

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

            outData.ComboData = FactoryManager.Instance.GetFactory<ComboFactory>().GetComboData(comboType);
            
            return true;

            //--------------------------------------------------------------------------------------------------------//
        }
    }

}